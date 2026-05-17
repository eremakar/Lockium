namespace Lockium.Options;

public sealed class LockBoardOptions
{
    public const string SectionName = "LockBoard";

    public int TcpPort { get; set; } = 8585;

    public TimeSpan CommandTimeout { get; set; } = TimeSpan.FromSeconds(5);
}
