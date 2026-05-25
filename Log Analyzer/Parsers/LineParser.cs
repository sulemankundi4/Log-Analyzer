using Log_Analyzer.Response;

namespace Log_Analyzer.Parsers;

public static class LineParser
{
    public static bool TryParseRemainder(string remainder, DateTime timestamp, out LogEntryResponseDto? entry)
    {
        entry = null;
        var parts = remainder.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length < 3) return false;

        int index = 0;
        string ip = "unknown/missing";

        if (IpParser.TryParseIp(parts[index], out string parsedIp))
        {
            ip = parsedIp;
            index++;
        }

        if (parts.Length - index < 2) return false;

        if (!HttpMethodParser.TryParseHttpMethod(parts[index], out string method)) return false;
        index++;

        if (!RequestPathParser.TryParsePath(parts[index], out string path)) return false;
        index++;

        int? statusCode = parts.Length > index ? StatusCodeParser.ParseStatusCode(parts[index]) : null;

        double? responseTimeMs = parts.Length > index + 1 ? ResponseTimeParser.ParseResponseTime(parts[index + 1]) : null;

        string? extraFields = parts.Length > index + 2 ? string.Join(" ", parts[(index + 2)..]) : null;

        entry = new LogEntryResponseDto()
        {
            Timestamp = timestamp,
            IpAddress = ip,      
            Method = method,
            Path = path,
            StatusCode = statusCode,
            ResponseTimeMs = responseTimeMs,
            ExtraFields = extraFields,
        };

        return true;
    }
}