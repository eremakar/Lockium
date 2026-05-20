namespace Lockium.Options;

public sealed class LockBoardOptions
{
    public const string SectionName = "LockBoard";

    public int TcpPort { get; set; } = 8585;

    public TimeSpan CommandTimeout { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>Max time after connect to receive Registration (0x81) or Heartbeat (0x80) before disconnect.</summary>
    public TimeSpan InitialHandshakeTimeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>Max time without any received data before disconnecting an established connection.</summary>
    public TimeSpan ConnectionIdleTimeout { get; set; } = TimeSpan.FromSeconds(90);

    /// <summary>Write detailed WKLY protocol trace to a log file.</summary>
    public bool ProtocolLogEnabled { get; set; } = true;

    /// <summary>Path relative to app base directory, or absolute. Supports {date} placeholder.</summary>
    public string ProtocolLogPath { get; set; } = "logs/lockboard-protocol-{date}.log";
}
