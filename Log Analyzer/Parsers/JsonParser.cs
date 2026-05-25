using System.Net;
using System.Text.Json;
using Log_Analyzer.Response;

namespace Log_Analyzer.Parsers;

public static class JsonLineParser
{
    public static bool TryParse(string line, out LogEntryResponseDto? entry)
    {
        entry = null;

        try
        {
            using JsonDocument doc = JsonDocument.Parse(line);
            JsonElement root = doc.RootElement;

            if (root.ValueKind != JsonValueKind.Object) return false;

            DateTime? timestamp = null;
            string? ip = null;
            string? method = null;
            string? path = null;
            int? statusCode = null;
            double? responseMs = null;

            foreach (JsonProperty prop in root.EnumerateObject())
            {
                string raw = prop.Value.ValueKind switch
                {
                    JsonValueKind.String => prop.Value.GetString() ?? "",
                    JsonValueKind.Number => prop.Value.ToString(),
                    _ => "" 
                };

                if (string.IsNullOrWhiteSpace(raw)) continue;

                if (timestamp == null && TimestampParser.TryParse(raw, out DateTime ts))
                {
                    timestamp = ts;
                    continue;
                }

                if (ip == null && IPAddress.TryParse(raw, out _))
                {
                    ip = raw;
                    continue;
                }

                if (method == null && HttpMethodParser.TryParseHttpMethod(raw, out string m))
                {
                    method = m;
                    continue;
                }

                if (path == null && raw.StartsWith("/"))
                {
                    path = raw;
                    continue;
                }

                if ((method == null || path == null) && TryParseCombined(raw, out string? cm, out string? cp))
                {
                    method ??= cm;
                    path ??= cp;
                    continue;
                }

                TryFillNumeric(raw, ref statusCode, ref responseMs);
            }

            if (timestamp == null || method == null || path == null)
                return false;

            entry = new LogEntryResponseDto
            {
                Timestamp = timestamp.Value,
                IpAddress = ip ?? "unknown",
                Method = method.ToUpperInvariant(),
                Path = path,
                StatusCode = statusCode,
                ResponseTimeMs = responseMs,
                IsJsonFormat = true,
            };

            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static bool TryParseCombined(string raw, out string? method, out string? path)
    {
        method = null;
        path = null;

        var parts = raw.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2) return false;

        if (HttpMethodParser.TryParseHttpMethod(parts[0], out string m) &&
            RequestPathParser.TryParsePath(parts[1], out string p))
        {
            method = m;
            path = p;
            return true;
        }

        return false;
    }

    private static void TryFillNumeric(string raw, ref int? statusCode, ref double? responseMs)
    {
        if (raw.EndsWith("ms") || raw.EndsWith("s"))
        {
            var parsed = ResponseTimeParser.ParseResponseTime(raw);
            if (parsed != null)
            {
                responseMs ??= parsed;
                return;
            }
        }

        if (double.TryParse(raw, out double num) && num > 0)
        {
            if (raw.Contains('.'))
            {
                responseMs ??= num * 1000;
                return;
            }

            if (num >= 100 && num <= 599 && statusCode == null)
            {
                statusCode = (int)num;
                return;
            }

            responseMs ??= num;
        }
    }
}