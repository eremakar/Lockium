// Aspire AppHost only — do not copy this file into Lockium/Program.cs
var builder = DistributedApplication.CreateBuilder(args);

// Fixed ports 5000–5007 (nginx). ExcludeLaunchProfile avoids duplicate "http" endpoint;
// isProxied: false binds Kestrel directly (no random DCP proxy port).
AddApi<Projects.Lockium>(builder, "lockium", 5000, excludeKestrel: true);
AddApi<Projects.Lockium_Devices>(builder, "devices", 5001);
AddApi<Projects.Lockium_Reservations>(builder, "reservations", 5002);
AddApi<Projects.Lockium_Orders>(builder, "orders", 5003);
AddApi<Projects.Lockium_Transactions>(builder, "transactions", 5004);
AddApi<Projects.Lockium_Billings>(builder, "billings", 5005);
AddApi<Projects.Lockium_Lockers>(builder, "lockers", 5006);
AddApi<Projects.Lockium_Client_Api>(builder, "client-api", 5007);

builder.Build().Run();

static IResourceBuilder<ProjectResource> AddApi<TProject>(
    IDistributedApplicationBuilder builder,
    string name,
    int port,
    bool excludeKestrel = false)
    where TProject : class, IProjectMetadata, new()
{
    var urls = $"http://0.0.0.0:{port}";

    return builder.AddProject<TProject>(name, options =>
    {
        options.ExcludeLaunchProfile = true;
        options.ExcludeKestrelEndpoints = excludeKestrel;
    })
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
    .WithEnvironment("DOTNET_ENVIRONMENT", "Development")
    .WithEnvironment("ASPNETCORE_URLS", urls)
    // Do not pass env: ASPNETCORE_URLS to WithHttpEndpoint — Aspire injects only the port number
    .WithHttpEndpoint(targetPort: port, port: port, isProxied: false);
}