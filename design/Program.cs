using System.Text;
using System.Text.Json.Serialization;
using Api.AspNetCore.Filters;
using Api.AspNetCore.Helpers;
using Api.AspNetCore.Models.Configuration;
using Api.AspNetCore.Services;
using Data.Repository.Dapper;
using Ticketing.Operative.Data.TicketOperativeDb.DatabaseContext;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Serilog;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;
using Ticketing.Client.Api.Services;
using Ticketing.Data.TicketDb.DatabaseContext;
using Ticketing.Data.TicketDb.DapperContext;
using Ticketing.Mappings;
using Ticketing.Services;
using Ticketing.Services.Auth;
using Ticketing.Workflows;
using Ticketing.Workflows.Services;
using Ticketing.Workflows.Steps;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
// Newtonsoft.Json output + ResponseCompression: serializer пишет в поток синхронно; иначе Kestrel: "Synchronous operations are disallowed".
builder.WebHost.ConfigureKestrel(options => { options.AllowSynchronousIO = true; });
var configuration = builder.Configuration;
var services = builder.Services;

builder.Host.UseSerilog((context, cfg) => cfg
    .ReadFrom.Configuration(context.Configuration)
    .Enrich.FromLogContext());

services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<GzipCompressionProvider>();
});
services.Configure<GzipCompressionProviderOptions>(o => { o.Level = System.IO.Compression.CompressionLevel.Fastest; });

services.AddScoped<IDapperDbContext, TicketDbDapperDbContext>();
services.AddHealthChecks();
services.AddControllers(options => options.SuppressOutputFormatterBuffering = true).AddJsonOptions(opt =>
{
    opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
}).AddNewtonsoftJson(_ => _.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore)
.AddXmlDataContractSerializerFormatters();

services.AddEndpointsApiExplorer();
services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Ticketing Gateway API",
        Version = "v1",
        Description = """
## Назначение
REST API шлюза (Gateway) билетной системы: расписания и справки поездов, оформление и обслуживание электронных билетов, транспортные и банковские карты, дополнительное питание, дополнительные услуги, локальные справочники, выдача и обновление токенов доступа.

## Аутентификация
Большинство операций требуют JWT. Получите токен вызовом **`POST /api/v1/authenticate`** или обновите сессию через **`POST /api/v1/refreshToken`** (оба метода доступны без авторизации). В Swagger UI укажите значение заголовка `Authorization` в формате, описанном в схеме **Bearer** ниже. При необходимости включите сохранение учётных данных в интерфейсе (**Persist authorization**), чтобы не вводить токен повторно после перезагрузки страницы.

## Соглашения по API
- Корневой префикс ресурсов: **`/api/v1`**.
- Формат обмена по умолчанию: **JSON**; отдельные операции дополнительно объявляют поддержку **XML** (см. атрибуты `Produces` и `Consumes` у методов).
- Детали по полям и допустимым значениям — в XML-комментариях моделей (описания схем OpenAPI).

""",
        Contact = new OpenApiContact { Name = "Ticketing" }
    });

    c.AddServer(new OpenApiServer
    {
        Url = "/",
        Description = "Текущий экземпляр сервиса; пути в документе задаются относительно корня этого хоста."
    });

    c.OrderActionsBy(apiDesc =>
        $"{apiDesc.ActionDescriptor.RouteValues["controller"]}_{apiDesc.RelativePath}");

    c.OperationFilter<AuthorizeCheckOperationFilter>();
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description =
            "Заголовок **Authorization**, схема Bearer (JWT).\n\n"
            + "Укажите либо строку **`Bearer {token}`**, либо только значение токена — в зависимости от настройки middleware допускается оба варианта.\n\n"
            + "Пример: `Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...`"
    });
    var xmlFiles = Directory.GetFiles(AppContext.BaseDirectory, "*.xml", SearchOption.TopDirectoryOnly).ToList();
    xmlFiles.ForEach(xmlFile => c.IncludeXmlComments(xmlFile));
    c.OperationFilter<FormatXmlCommentProperties>();
    c.OperationFilter<GeneratePathParamsValidationFilter>();
    c.CustomSchemaIds(type => type.ToString());
    c.DocumentFilter<ScalarTagOrderDocumentFilter>();
});

var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING") ??
    builder.Configuration.GetConnectionString("PostgresConnection")
    ?? throw new InvalidOperationException("PostgresConnection / DB_CONNECTION_STRING is not configured.");
services.AddEntityFrameworkNpgsql().AddDbContext<TicketDbContext>(options =>
{
    options.UseNpgsql(connectionString, b => b.EnableRetryOnFailure());
});

var operativeConnectionString = Environment.GetEnvironmentVariable("OPERATIVE_DB_CONNECTION_STRING") ??
    configuration.GetConnectionString("PostgresOperativeConnection")
    ?? connectionString;
services.AddEntityFrameworkNpgsql().AddDbContext<TicketOperativeDbContext>(options =>
{
    options.UseNpgsql(operativeConnectionString, b => b.EnableRetryOnFailure());
});

services.AddHttpContextAccessor();
services.AddScoped<IAuthorizeService, AuthorizeService>();
services.AddScoped<PassengerMap>();
services.AddScoped<FreeReservationsService>();
services.AddScoped<TicketsService>();
services.AddScoped<SeatReservationsService>();
services.AddScoped<SeatTariffService>();
services.AddScoped<IMicroserviceStateWorkflow, MicroserviceStateWorkflow>();
services.AddTransient<TicketAddFoodTransactionStep>();
services.AddTransient<FoodRejectTransactionStep>();
services.AddTransient<TicketFoodConfirmTransactionStep>();
services.AddTransient<TicketFoodReturnTransactionStep>();
services.AddTransient<AdditionalServiceBuyTransactionStep>();
services.AddTransient<AdditionalServiceConfirmTransactionStep>();
services.AddTransient<AdditionalServiceRejectTransactionStep>();
services.AddTransient<AdditionalServiceReturnTransactionStep>();

IdentityModelEventSource.ShowPII = true;

services.Configure<TokenManagement>(configuration.GetSection("tokenManagement"));
var token = configuration.GetSection("tokenManagement").Get<TokenManagement>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(_ =>
    {
        _.Authority = token?.Authority;
        _.RequireHttpsMetadata = false;
        _.SaveToken = true;
        _.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(token!.Secret)),
            ValidateAudience = false,
            ValidAudience = token.Audience,
            ValidateIssuer = true,
            ValidIssuer = token.Issuer
        };
    });

services.AddScoped<DbMapContext>();
services.AddScoped<BuyTicketRequestMap>();
services.AddScoped<BuyTicketWorkflowIdResolver>();
services.AddScoped<UserMap>();
services.AddScoped<IUserService, UserService>();
services.AddScoped<IRoleService, RoleService>();
services.AddScoped<IUserManagementService, MicroserviceUserManagementService>();
services.AddScoped<IAuthenticateService, MicroserviceAuthenticationService>();

builder.AddSecurity();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger(_ =>
    {
        _.RouteTemplate = "/openapi/{documentname}.json";
    });

    app.MapScalarApiReference();
    //app.UseSwaggerUI(options =>
    //{
    //    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Ticketing Gateway API v1");
    //    options.RoutePrefix = "swagger";
    //    options.DocumentTitle = "Ticketing Gateway API — Swagger UI";

    //    options.DisplayRequestDuration();
    //    options.DocExpansion(DocExpansion.List);
    //    options.DefaultModelsExpandDepth(1);
    //    options.DefaultModelExpandDepth(2);
    //    options.EnableDeepLinking();
    //    options.EnableFilter();
    //    options.ShowExtensions();
    //    options.EnableTryItOutByDefault();
    //    options.EnablePersistAuthorization();
    //    options.DisplayOperationId();
    //});
}

app.UseResponseCompression();
app.UseHealthChecks("/api/v1/health");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

app.Run();

internal sealed class ScalarTagOrderDocumentFilter : IDocumentFilter
{
    private const string DictionariesTagName = "Dictionaries";
    private const string DictionariesDisplayName = "Dictionaries - справочники TicketCore";
    private const string ReservationsTagName = "Reservations";
    private const string ReservationsDisplayName = "Reservations - бронирования мест.";
    private const string TrainSchedulesTagName = "TrainSchedules";
    private const string TrainSchedulesDisplayName = "TrainSchedules - расписания поездов.";
    private const string TicketsTagName = "Tickets";
    private const string TicketsDisplayName = "Tickets - проездные документы.";
    private const string OrdersTagName = "Orders";
    private const string OrdersDisplayName = "Orders - Заказы.";
    private const string TicketServicesTagName = "TicketServices";
    private const string TicketServicesDisplayName = "TicketServices - услуги по билетам.";

    private static readonly string[] PreferredOrder =
    [
        DictionariesTagName,
        TrainSchedulesTagName,
        ReservationsTagName,
        TicketsTagName,
        OrdersTagName,
        TicketServicesTagName
    ];

    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        // Scalar groups operations by operation tags, so rename there first.
        foreach (var pathItem in swaggerDoc.Paths.Values)
        {
            foreach (var operation in pathItem.Operations.Values)
            {
                if (operation.Tags is null || operation.Tags.Count == 0)
                {
                    continue;
                }

                foreach (var tag in operation.Tags)
                {
                    if (tag.Name == DictionariesTagName)
                    {
                        tag.Name = DictionariesDisplayName;
                    }
                    else if (tag.Name == TrainSchedulesTagName)
                    {
                        tag.Name = TrainSchedulesDisplayName;
                    }
                    else if (tag.Name == ReservationsTagName)
                    {
                        tag.Name = ReservationsDisplayName;
                    }
                    else if (tag.Name == TicketsTagName)
                    {
                        tag.Name = TicketsDisplayName;
                    }
                    else if (tag.Name == OrdersTagName)
                    {
                        tag.Name = OrdersDisplayName;
                    }
                    else if (tag.Name == TicketServicesTagName)
                    {
                        tag.Name = TicketServicesDisplayName;
                    }
                }
            }
        }

        if (swaggerDoc.Tags is null || swaggerDoc.Tags.Count == 0)
        {
            return;
        }

        foreach (var tag in swaggerDoc.Tags)
        {
            if (tag.Name == DictionariesTagName)
            {
                tag.Name = DictionariesDisplayName;
            }
            else if (tag.Name == TrainSchedulesTagName)
            {
                tag.Name = TrainSchedulesDisplayName;
            }
            else if (tag.Name == ReservationsTagName)
            {
                tag.Name = ReservationsDisplayName;
            }
            else if (tag.Name == TicketsTagName)
            {
                tag.Name = TicketsDisplayName;
            }
            else if (tag.Name == OrdersTagName)
            {
                tag.Name = OrdersDisplayName;
            }
            else if (tag.Name == TicketServicesTagName)
            {
                tag.Name = TicketServicesDisplayName;
            }
        }

        var priority = PreferredOrder
            .Select((name, index) => new { name, index })
            .ToDictionary(x => x.name, x => x.index, StringComparer.Ordinal);

        swaggerDoc.Tags = swaggerDoc.Tags
            .OrderBy(tag =>
            {
                var normalizedTagName = tag.Name switch
                {
                    DictionariesDisplayName => DictionariesTagName,
                    TrainSchedulesDisplayName => TrainSchedulesTagName,
                    ReservationsDisplayName => ReservationsTagName,
                    TicketsDisplayName => TicketsTagName,
                    OrdersDisplayName => OrdersTagName,
                    TicketServicesDisplayName => TicketServicesTagName,
                    _ => tag.Name
                };

                return priority.TryGetValue(normalizedTagName, out var idx) ? idx : int.MaxValue;
            })
            .ThenBy(tag => tag.Name, StringComparer.Ordinal)
            .ToList();

        // Scalar builds tag sections from operation stream, so path order also affects sidebar order.
        var orderedPaths = swaggerDoc.Paths
            .OrderBy(path =>
            {
                var minPriority = path.Value.Operations.Values
                    .SelectMany(op => op.Tags ?? Enumerable.Empty<OpenApiTag>())
                    .Select(tag => tag.Name switch
                    {
                        DictionariesDisplayName => DictionariesTagName,
                        TrainSchedulesDisplayName => TrainSchedulesTagName,
                        ReservationsDisplayName => ReservationsTagName,
                        TicketsDisplayName => TicketsTagName,
                        OrdersDisplayName => OrdersTagName,
                        TicketServicesDisplayName => TicketServicesTagName,
                        _ => tag.Name
                    })
                    .Select(name => priority.TryGetValue(name, out var idx) ? idx : int.MaxValue)
                    .DefaultIfEmpty(int.MaxValue)
                    .Min();

                return minPriority;
            })
            .ThenBy(path => path.Key, StringComparer.Ordinal)
            .ToList();

        var reorderedPaths = new OpenApiPaths();
        foreach (var (pathKey, pathValue) in orderedPaths)
        {
            reorderedPaths.Add(pathKey, pathValue);
        }

        swaggerDoc.Paths = reorderedPaths;
    }
}
