using Api.AspNetCore.Filters;
using Api.AspNetCore.Helpers;
using Api.AspNetCore.Models.Configuration;
using Api.AspNetCore.Services;
using Data.Repository.Dapper;
using Data.Repository.Helpers;
using Lockium;
using Lockium.Data.LockiumDb.DapperContext;
using Lockium.Data.LockiumDb.DatabaseContext;
using Lockium.Data.LockiumDb.Entities;
using Lockium.Data.LockiumDb.Ids;
using Lockium.Helpers;
using Lockium.Options;
using Lockium.Services;
using Lockium.Services.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<LockBoardOptions>(builder.Configuration.GetSection(LockBoardOptions.SectionName));
builder.Services.AddSingleton<LockiumProtocolFileLogger>();
builder.Services.AddSingleton<LockConnectionRegistry>();
builder.Services.AddScoped<IDeviceService, DeviceService>();
builder.Services.AddSingleton<ILockiumEventHandler, LockiumEventHandler>();
builder.Services.AddSingleton<DoorStatusStore>();
builder.Services.AddSingleton<LockiumTcpServer>();
builder.Services.AddHostedService<LockBoardTcpHostedService>();
builder.Services.AddControllers();

var services = builder.Services;

services.AddEndpointsApiExplorer();
services.AddSwaggerGen(c =>
{
    c.OperationFilter<AuthorizeCheckOperationFilter>();
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = @"JWT Authorization header using the Bearer scheme
Enter 'Bearer' [space] and then your token in the text input below.
Example: 'Bearer 12345abcdef'"
    });
    var xmlFiles = Directory.GetFiles(AppContext.BaseDirectory, "*.xml", SearchOption.TopDirectoryOnly).ToList();
    xmlFiles.ForEach(xmlFile => c.IncludeXmlComments(xmlFile));

    c.OperationFilter<FormatXmlCommentProperties>();
    // Include DataAnnotation attributes on Controller Action parameters as Swagger validation rules (e.g required, pattern, ..)
    // Use [ValidateModelState] on Actions to actually validate it in C# as well!
    c.OperationFilter<GeneratePathParamsValidationFilter>();

    c.CustomSchemaIds(type => type.ToString());
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
var configuration = builder.Configuration;

services.AddScoped<IDapperDbContext, LockiumDbDapperDbContext>();

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

services.AddScoped<IUserManagementService, MicroserviceUserManagementService>();
services.AddScoped<IAuthenticateService, MicroserviceAuthenticationService>();
services.AddAuthorize<MicroserviceAuthorizeService>();

builder.AddServices();
builder.AddMapping();
builder.AddProviders();
builder.AddSecurity();


var app = builder.Build();

app.MapControllers();

// global cors policy
app.UseCors(x =>
{
    x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
});
app.MigrateDb<LockiumDbContext>((context) =>
{
    context.EnsureSeeded();

    app.SeedSuperUser(context,
        existUser: (context, userName) => context.Users.Any(_ => _.UserName == userName),
        addUser: (context, superUser) =>
        {
            context.Users.Add(new User
            {
                UserName = superUser.UserName,
                PasswordHash = CryptHelper.EncryptString(superUser.Password),
                IsActive = true,
                Roles = new List<UserRole>
                {
                    new UserRole
                    {
                        RoleId = (int)RoleIds.SuperAdministrator
                    }
                }
            });
            context.SaveChanges();
        });
});


app.Run();
