namespace Log_Analyzer;

public static class Constants
{
    public static readonly string[] KnownTimestampFormats =
    {
        "yyyy-MM-ddTHH:mm:ssZ",
        "yyyy-MM-ddTHH:mm:ss",
        "yyyy/MM/dd HH:mm:ss",
        "dd-MMM-yyyy HH:mm:ss",
    };

    public static readonly HashSet<string> ValidHttpMethods = new()
    {
        "GET", "POST", "PUT", "DELETE", "PATCH", "HEAD", "OPTIONS"
    };

    public const long MillisecondsDivisor = 1_000L;
    public const long MicrosecondsDivisor = 1_000_000L;
    public const long NanosecondsDivisor = 1_000_000_000L;
}
