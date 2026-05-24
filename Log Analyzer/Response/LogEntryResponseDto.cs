namespace Log_Analyzer.Response;

public class LogEntryResponseDto
{
    public DateTime Timestamp { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public int? StatusCode { get; set; }
    public double? ResponseTimeMs { get; set; }
    public string? ExtraFields { get; set; }
    public bool IsJsonFormat { get; set; }
}