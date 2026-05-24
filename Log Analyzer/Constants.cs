namespace LogAnalyzer;

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
}