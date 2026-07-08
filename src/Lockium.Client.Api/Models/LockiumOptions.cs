namespace Lockium.Client.Api.Models;

public sealed class LockiumOptions
{
    public const string SectionName = "Lockium";

    public string BaseUrl { get; set; } = "http://localhost:5000";
}
