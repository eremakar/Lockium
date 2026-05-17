using Lockium;
using Lockium.Options;
using Lockium.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<LockBoardOptions>(builder.Configuration.GetSection(LockBoardOptions.SectionName));
builder.Services.AddSingleton<LockConnectionRegistry>();
builder.Services.AddSingleton<DoorStatusStore>();
builder.Services.AddSingleton<LockiumTcpServer>();
builder.Services.AddHostedService<LockBoardTcpHostedService>();
builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();

app.Run();
