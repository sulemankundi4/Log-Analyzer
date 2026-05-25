using Log_Analyzer.LogPrinter;
using Log_Analyzer.Parsers;
using Log_Analyzer.Response;
using Log_Analyzer.Script;

namespace LogAnalyzer;

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

            if (string.IsNullOrWhiteSpace(rawLine))
            {
                malformedCount++;
                continue;
            }

            var line = rawLine.Trim();

            if (line.Length < 30)
            {
                malformedCount++;
                continue;
            }

            if (line.StartsWith("{"))
            {
                if (JsonLineParser.TryParse(line, out LogEntryResponseDto? jsonEntry) && jsonEntry != null)
                {
                    validCount++;
                    jsonCount++;
                    LogPrinter.PrintEntry(jsonEntry);
                }
                else
                {
                    malformedCount++;
                }

                continue;
            }

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
        if (args.Length == 0)
        {
            Console.WriteLine("[ERROR] No arguments provided.");
            Console.WriteLine("Usage : dotnet run -- <path-to-log-file>");
            Console.WriteLine("        dotnet run -- --generate [lineCount] [fileName]");
            return;
        }

        // ── Route to generator ───────────────────────────
        if (args[0] == "--generate")
        {
            int lines = args.Length > 1 && int.TryParse(args[1], out int n) ? n : 1000;
            string fileName = args.Length > 2 ? args[2] : "test.log";

            var generator = new LogGenerator(seed: 42);
            var stats = generator.Generate(lines, fileName);

            Console.WriteLine($"  Total lines   : {stats.Total}");
            Console.WriteLine($"  Standard      : {stats.Standard}");
            Console.WriteLine($"  JSON valid    : {stats.JsonValid}");
            Console.WriteLine($"  JSON invalid  : {stats.JsonInvalid}");
            Console.WriteLine($"  Blank         : {stats.Blank}");
            Console.WriteLine($"  Stack traces  : {stats.StackTrace}");
            Console.WriteLine($"  System msgs   : {stats.SystemMessage}");
            Console.WriteLine($"  Saved to      : {Path.Combine(AppContext.BaseDirectory, "Logs", fileName)}");
            return;
        }

        // ── Route to analyzer ────────────────────────────
        LogAnalyzer.Analyze(args[0]);
    }
}