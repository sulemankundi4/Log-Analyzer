namespace LogAnalyzer;

public static class RequestPathParser
{
    public static bool TryParsePath(string token, out string path)
    {
        path = string.Empty;

        if (token.StartsWith("/"))
        {
            path = token;
            return true;
        }

        return false;
    }
}