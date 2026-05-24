namespace LogAnalyzer;

public static class ResponseTimeParser
{
    public static double? ParseResponseTime(string token)
    {
        // "142ms" → 142.0
        if (token.EndsWith("ms") && double.TryParse(token[..^2], out double ms))
            return ms;

        // "0.142s" → 142.0
        if (token.EndsWith("s") && double.TryParse(token[..^1], out double secs))
            return secs * 1000;

        // "142" bare number → assume ms
        if (double.TryParse(token, out double bare))
            return bare;

        return null;
    }
}