namespace Log_Analyzer.Parsers;

public static class HttpMethodParser
{
    public static bool TryParseHttpMethod(string token, out string method)
    {
        method = string.Empty;
        var upper = token.ToUpperInvariant();

        if (Constants.ValidHttpMethods.Contains(upper))
        {
            method = upper;
            return true;
        }

        return false;
    }
}