using System.Net;

namespace LogAnalyzer;

public static class IpParser
{
    public static bool TryParseIp(string token, out string ip)
    {
        ip = string.Empty;
        if (IPAddress.TryParse(token, out _))
        {
            ip = token;
            return true;
        }

        return false;
    }
}