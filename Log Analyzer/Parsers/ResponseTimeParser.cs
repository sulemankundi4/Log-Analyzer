namespace Log_Analyzer.Parsers;

public static class ResponseTimeParser
{
    public static double? ParseResponseTime(string token)
    {
        if (token.EndsWith("ms") && double.TryParse(token[..^2], out double ms))
            return ms;

        if (token.EndsWith("s") && double.TryParse(token[..^1], out double secs))
            return secs * 1000;

        if (double.TryParse(token, out double bare))
            return bare;

        return null;
    }
}