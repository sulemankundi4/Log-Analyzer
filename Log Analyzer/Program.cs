using Log_Analyzer.LogPrinter;
using Log_Analyzer.Parsers;
using Log_Analyzer.Response;

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
        args = new[] { "test.log" };

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