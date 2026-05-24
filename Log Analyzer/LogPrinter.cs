using Log_Analyzer;
using Log_Analyzer.Response;

namespace LogAnalyzer;

public static class LogPrinter
{
    public static void PrintEntry(LogEntryResponseDto entryResponseDto)
    {
        Console.WriteLine("─────────────────────────────────────────────────────");

        Console.ForegroundColor = entryResponseDto.IsJsonFormat ? ConsoleColor.Cyan : ConsoleColor.Green;
        Console.WriteLine(entryResponseDto.IsJsonFormat ? "  [JSON]" : "  [Standard] ");
        Console.ResetColor();

        Console.WriteLine($"  Timestamp    : {entryResponseDto.Timestamp:dddd, dd MMMM yyyy}  {entryResponseDto.Timestamp:HH:mm:ss} UTC");
        Console.WriteLine($"  IP           : {entryResponseDto.IpAddress}");
        Console.WriteLine($"  Method       : {entryResponseDto.Method}");
        Console.WriteLine($"  Path         : {entryResponseDto.Path}");
        Console.WriteLine($"  Status       : {Helper.StatusLabel(entryResponseDto.StatusCode)}");
        Console.WriteLine($"  Response Time: {Helper.ResponseLabel(entryResponseDto.ResponseTimeMs)}");

        if (entryResponseDto.ExtraFields != null)
            Console.WriteLine($"  Extra Fields : {entryResponseDto.ExtraFields}");

        Console.WriteLine("─────────────────────────────────────────────────────");
    }

    public static void PrintSummary(int total, int malformed, int valid, int json)
    {
        Console.WriteLine();
        Console.WriteLine("╔═════════════════════════════════════════════════════╗");
        Console.WriteLine("║                      SUMMARY                        ║");
        Console.WriteLine("╠═════════════════════════════════════════════════════╣");
        Console.WriteLine($"║  Total Lines     : {total,-33}║");
        Console.WriteLine($"║  Valid Lines     : {valid,-33}║");
        Console.WriteLine($"║  ├─ Standard     : {valid - json,-33}║");
        Console.WriteLine($"║  └─ JSON Format  : {json,-33}║");
        Console.WriteLine($"║  Malformed Lines : {malformed,-33}║");
        Console.WriteLine("╚═════════════════════════════════════════════════════╝");
    }

    
}