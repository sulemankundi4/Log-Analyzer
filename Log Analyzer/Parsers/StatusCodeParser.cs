namespace Log_Analyzer.Parsers;

public static class StatusCodeParser
{
    public static int? ParseStatusCode(string token)
    {
        if (token == "-") return null;

        if (int.TryParse(token, out int code) && code >= 100 && code <= 599)
            return code;

        return null;
    }
}