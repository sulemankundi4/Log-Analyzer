using Log_Analyzer.Response;

namespace LogAnalyzer;


//public static class JsonLineParser
//{
//    public static bool TryParse(string line, out LogEntryResponseDto? entry)
//    {
//        entry = null;

//        try
//        {
//            using JsonDocument doc = JsonDocument.Parse(line);
//            JsonElement root = doc.RootElement;

//            // Minimum required: timestamp + method + path
//            if (!TryGetString(root, TimestampKeys, out string? tsRaw) || tsRaw == null) return false;
//            if (!TimestampParser.TryParse(tsRaw, out DateTime timestamp)) return false;
//            if (!TryGetString(root, MethodKeys, out string? method) || method == null) return false;
//            if (!TryGetString(root, PathKeys, out string? path) || path == null) return false;

//            TryGetString(root, IpKeys, out string? ip);

//            entry = new LogEntryResponseDto
//            {
//                Timestamp = timestamp,
//                IpAddress = ip ?? "unknown",
//                Method = method.ToUpperInvariant(),
//                Path = path,
//                StatusCode = TryGetInt(root, StatusKeys),
//                ResponseTimeMs = TryGetResponseTime(root),
//                ExtraFields = null,
//                IsJsonFormat = true,
//            };

//            return true;
//        }
//        catch (JsonException)
//        {
//            // Malformed JSON — treat as malformed line
//            return false;
//        }
//    }

//    // ── Helpers ─────────────────────────────────────────────

//    private static bool TryGetString(JsonElement root, string[] keys, out string? value)
//    {
//        value = null;
//        foreach (var key in keys)
//        {
//            if (!root.TryGetProperty(key, out JsonElement el)) continue;

//            value = el.ValueKind == JsonValueKind.String
//                ? el.GetString()
//                : el.ToString();
//            return true;
//        }
//        return false;
//    }

//    private static int? TryGetInt(JsonElement root, string[] keys)
//    {
//        foreach (var key in keys)
//        {
//            if (!root.TryGetProperty(key, out JsonElement el)) continue;

//            if (el.ValueKind == JsonValueKind.Number && el.TryGetInt32(out int val))
//                return val;

//            // Sometimes status arrives as a string: "200"
//            if (el.ValueKind == JsonValueKind.String &&
//                int.TryParse(el.GetString(), out int parsed))
//                return parsed;
//        }
//        return null;
//    }

//    private static double? TryGetResponseTime(JsonElement root)
//    {
//        foreach (var key in ResponseKeys)
//        {
//            if (!root.TryGetProperty(key, out JsonElement el)) continue;

//            double raw = 0;

//            if (el.ValueKind == JsonValueKind.Number)
//            {
//                raw = el.GetDouble();
//            }
//            else if (el.ValueKind == JsonValueKind.String)
//            {
//                var s = el.GetString() ?? "";

//                if (s.EndsWith("ms") && double.TryParse(s[..^2], out double ms)) return ms;
//                if (s.EndsWith("s") && double.TryParse(s[..^1], out double sc)) return sc * 1000;
//                if (!double.TryParse(s, out raw)) continue;
//            }

//            // Key name containing "ms" → already milliseconds; otherwise assume seconds
//            return key.Contains("ms") ? raw : raw * 1000;
//        }
//        return null;
//    }
//}

// ─────────────────────────────────────────────────────────────
// PRINTER
// ─────────────────────────────────────────────────────────────

// ─────────────────────────────────────────────────────────────
// LOG ANALYZER  (main orchestrator)
// ─────────────────────────────────────────────────────────────

public static class LogAnalyzer
{
    public static void Analyze(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[ERROR] File not found: {filePath}");
            Console.ResetColor();
            return;
        }

        int totalLines = 0;
        int malformedCount = 0;
        int validCount = 0;
        int jsonCount = 0;

        Console.WriteLine($"\n  Analyzing: {filePath}\n");

        foreach (var rawLine in File.ReadLines(filePath))
        {
            totalLines++;

            // ── Gate 1: blank line ───────────────────────────
            if (string.IsNullOrWhiteSpace(rawLine))
            {
                malformedCount++;
                continue;
            }

            var line = rawLine.Trim();

            // ── Gate 2: too short to be valid ────────────────
            if (line.Length < 30)
            {
                malformedCount++;
                continue;
            }

            // ── Gate 3: JSON line ────────────────────────────
            //if (line.StartsWith("{"))
            //{
            //    if (JsonLineParser.TryParse(line, out LogEntryResponseDto? jsonEntry) && jsonEntry != null)
            //    {
            //        validCount++;
            //        jsonCount++;
            //        LogPrinter.PrintEntry(jsonEntry);
            //    }
            //    else
            //    {
            //        malformedCount++;
            //        LogPrinter.PrintMalformed(line, "Starts with '{' but failed JSON parse");
            //    }
            //    continue;
            //}

            TimestampResponseDto tsResponseDto = TimestampParser.ExtractTimestamp(line);

            if (!tsResponseDto.Success)
            {
                malformedCount++;
                continue;
            }

            string remainder = line[tsResponseDto.CharsConsumed..];

            if (!LineParser.TryParseRemainder(remainder, tsResponseDto.Timestamp, out LogEntryResponseDto? entry) || entry == null)
            {
                malformedCount++;
                continue;
            }

            validCount++;
            LogPrinter.PrintEntry(entry);
        }

        LogPrinter.PrintSummary(totalLines, malformedCount, validCount, jsonCount);
    }
}


class Program
{
    static void Main(string[] args)
    {
        if (args.Length < 1)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("[ERROR] No log file path provided.");
            Console.WriteLine("Usage : LogAnalyzer <path-to-log-file>");
            Console.ResetColor();
            return;
        }

        LogAnalyzer.Analyze(args[0]);
    }
}