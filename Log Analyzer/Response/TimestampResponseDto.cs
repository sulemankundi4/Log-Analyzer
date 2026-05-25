namespace Log_Analyzer.Response;

public class TimestampResponseDto
{
    public bool Success { get; set; }
    public DateTime Timestamp { get; set; }
    public int CharsConsumed { get; set; }
    public string OriginalToken { get; set; } = string.Empty;
    public string FailureReason { get; set; } = string.Empty;
}