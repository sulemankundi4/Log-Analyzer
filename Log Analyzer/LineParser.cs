using Log_Analyzer.Response;

namespace LogAnalyzer;

public static class LineParser
{
    public static bool TryParseRemainder(string remainder, DateTime timestamp, out LogEntryResponseDto? entry)
    {
        entry = null;

        var parts = remainder.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length < 3) return false;

        if (!IpParser.TryParseIp(parts[0], out string ip)) return false;

        if (!HttpMethodParser.TryParseHttpMethod(parts[1], out string method)) return false;

        if (!RequestPathParser.TryParsePath(parts[2], out string path)) return false;

        int? statusCode = parts.Length > 3 ? StatusCodeParser.ParseStatusCode(parts[3]) : null;
        double? responseTimeMs = parts.Length > 4 ? ResponseTimeParser.ParseResponseTime(parts[4]) : null;
        string? extraFields = parts.Length > 5 ? string.Join(" ", parts[5..]) : null;

        entry = new LogEntryResponseDto
        {
            Timestamp = timestamp,
            IpAddress = ip,
            Method = method,
            Path = path,
            StatusCode = statusCode,
            ResponseTimeMs = responseTimeMs,
            ExtraFields = extraFields,
            IsJsonFormat = false,
        };

        return true;
    }
}