using Api.AspNetCore.Filters;
using Api.AspNetCore.Helpers;
using Api.AspNetCore.Models.Configuration;
using Data.Repository.Dapper;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Serilog;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Lockium.Client.Api.Services;
using Lockium.Client.Api.Helpers;
using Lockium.Data.LockiumDb.DapperContext;
using Lockium.Data.LockiumDb.DatabaseContext;
using Scalar.AspNetCore;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
var services = builder.Services;
services.Configure<KestrelServerOptions>(options =>
{
    options.AllowSynchronousIO = true;
});

builder.Host.UseSerilog((context, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .Enrich.FromLogContext()
);

services.AddScoped<IDapperDbContext, LockiumDbDapperDbContext>();

services.AddCors();
services.AddHealthChecks();
services.AddControllers(options => options.SuppressOutputFormatterBuffering = true).AddJsonOptions(opt =>
{
    opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
}).AddNewtonsoftJson(_ => _.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore).AddXmlDataContractSerializerFormatters();
services.AddEndpointsApiExplorer();
services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Lockium Client API",
        Version = "v1",
        Description = """
## Назначение
REST API для клиентских приложений Lockium: просмотр устройств и ячеек, бронирование, оформление и сопровождение заказов размещения в постамате.

## Аутентификация
Все операции (кроме проверки работоспособности) требуют JWT. Получите токен на основном сервисе Lockium (`POST /api/v1/authenticate`). В Scalar укажите заголовок **Authorization** по схеме **Bearer** ниже.

## Роли
Доступны роли: **Client**, **Administrator**, **SuperAdministrator**.

## Соглашения
- Префикс ресурсов: **`/api/v1`**.
- Формат по умолчанию: **JSON**; часть операций также поддерживает **XML**.
- Создание заказов и броней выполняется через **workflow** (смена состояния), а не через CRUD `PUT`/`PATCH`/`DELETE`.
- Коды состояний заказа: `0` — не определён, `1` — создан, `2` — занят, `3` — выполнен.
- Коды состояний брони: `0` — не определена, `1` — активна, `2` — снята.

""",
        Contact = new OpenApiContact { Name = "Lockium" }
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
            + "Укажите строку **`Bearer {token}`** или только значение токена.\n\n"
            + "Пример: `Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...`"
    });
    foreach (var xmlFile in GetXmlDocumentationPaths())
        c.IncludeXmlComments(xmlFile);

    c.OperationFilter<FormatXmlCommentProperties>();
    c.OperationFilter<GeneratePathParamsValidationFilter>();

    c.CustomSchemaIds(type => type.ToString());
    c.DocumentFilter<ScalarTagOrderDocumentFilter>();
});

var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING") ??
    builder.Configuration.GetConnectionString("PostgresConnection");
services.AddEntityFrameworkNpgsql().AddDbContext<LockiumDbContext>(options =>
{
    options.UseNpgsql(connectionString,
        builder =>
        {
            builder.MigrationsAssembly(Assembly.GetExecutingAssembly().FullName);
            builder.EnableRetryOnFailure();
        });
});

IdentityModelEventSource.ShowPII = true;

services.Configure<TokenManagement>(configuration.GetSection("tokenManagement"));
var token = configuration.GetSection("tokenManagement").Get<TokenManagement>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(_ =>
    {
        _.Authority = token.Authority;
        _.RequireHttpsMetadata = false;
        _.SaveToken = true;
        _.TokenValidationParameters = new()
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(token.Secret)),
            ValidateAudience = false,
            ValidAudience = token.Audience,
            ValidateIssuer = true,
            ValidIssuer = token.Issuer
        };
    });

services.AddAuthorize<MicroserviceAuthorizeService>();

builder.AddServices();
builder.AddMapping();
builder.AddProviders();

builder.AddSecurity();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger(options =>
    {
        options.RouteTemplate = "/openapi/{documentname}.json";
    });

    app.MapScalarApiReference();
}

app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

app.UseHealthChecks("/api/v1/health");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

static IEnumerable<string> GetXmlDocumentationPaths()
{
    var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    foreach (var path in Directory.GetFiles(AppContext.BaseDirectory, "*.xml", SearchOption.TopDirectoryOnly))
    {
        if (seen.Add(path))
            yield return path;
    }

    foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
    {
        if (assembly.IsDynamic || string.IsNullOrWhiteSpace(assembly.Location))
            continue;

        var path = Path.ChangeExtension(assembly.Location, ".xml");
        if (File.Exists(path) && seen.Add(path))
            yield return path;
    }
}

internal sealed class ScalarTagOrderDocumentFilter : IDocumentFilter
{
    private const string DevicesTagName = "Devices";
    private const string DevicesDisplayName = "Devices — устройства (постаматы)";
    private const string ChannelsTagName = "Channels";
    private const string ChannelsDisplayName = "Channels — ячейки";
    private const string ReservationsTagName = "Reservations";
    private const string ReservationsDisplayName = "Reservations — бронирование ячеек";
    private const string OrdersTagName = "Orders";
    private const string OrdersDisplayName = "Orders — заказы размещения";
    private const string ShipmentsTagName = "Shipments";
    private const string ShipmentsDisplayName = "Shipments — доставки";
    private const string PickupTagName = "Pickup";
    private const string PickupDisplayName = "Pickup — получение по PIN";

    private static readonly string[] PreferredOrder =
    [
        DevicesTagName,
        ChannelsTagName,
        ReservationsTagName,
        OrdersTagName,
        ShipmentsTagName,
        PickupTagName
    ];

    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        foreach (var pathItem in swaggerDoc.Paths.Values)
        {
            foreach (var operation in pathItem.Operations.Values)
            {
                if (operation.Tags is null || operation.Tags.Count == 0)
                    continue;

                foreach (var tag in operation.Tags)
                {
                    tag.Name = tag.Name switch
                    {
                        DevicesTagName => DevicesDisplayName,
                        ChannelsTagName => ChannelsDisplayName,
                        ReservationsTagName => ReservationsDisplayName,
                        OrdersTagName => OrdersDisplayName,
                        ShipmentsTagName => ShipmentsDisplayName,
                        PickupTagName => PickupDisplayName,
                        _ => tag.Name
                    };
                }
            }
        }

        if (swaggerDoc.Tags is null || swaggerDoc.Tags.Count == 0)
            return;

        foreach (var tag in swaggerDoc.Tags)
        {
            tag.Name = tag.Name switch
            {
                DevicesTagName => DevicesDisplayName,
                ChannelsTagName => ChannelsDisplayName,
                ReservationsTagName => ReservationsDisplayName,
                OrdersTagName => OrdersDisplayName,
                ShipmentsTagName => ShipmentsDisplayName,
                PickupTagName => PickupDisplayName,
                _ => tag.Name
            };
        }

        var priority = PreferredOrder
            .Select((name, index) => new { name, index })
            .ToDictionary(x => x.name, x => x.index, StringComparer.Ordinal);

        swaggerDoc.Tags = swaggerDoc.Tags
            .OrderBy(tag =>
            {
                var normalizedTagName = tag.Name switch
                {
                    DevicesDisplayName => DevicesTagName,
                    ChannelsDisplayName => ChannelsTagName,
                    ReservationsDisplayName => ReservationsTagName,
                    OrdersDisplayName => OrdersTagName,
                    ShipmentsDisplayName => ShipmentsTagName,
                    PickupDisplayName => PickupTagName,
                    _ => tag.Name
                };

                return priority.TryGetValue(normalizedTagName, out var idx) ? idx : int.MaxValue;
            })
            .ThenBy(tag => tag.Name, StringComparer.Ordinal)
            .ToList();

        var orderedPaths = swaggerDoc.Paths
            .OrderBy(path =>
            {
                var minPriority = path.Value.Operations.Values
                    .SelectMany(op => op.Tags ?? Enumerable.Empty<OpenApiTag>())
                    .Select(tag => tag.Name switch
                    {
                        DevicesDisplayName => DevicesTagName,
                        ChannelsDisplayName => ChannelsTagName,
                        ReservationsDisplayName => ReservationsTagName,
                        OrdersDisplayName => OrdersTagName,
                        ShipmentsDisplayName => ShipmentsTagName,
                        PickupDisplayName => PickupTagName,
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
            reorderedPaths.Add(pathKey, pathValue);

        swaggerDoc.Paths = reorderedPaths;
    }
}
