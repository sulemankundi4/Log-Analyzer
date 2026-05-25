namespace Log_Analyzer.Helpers;

public static class Helper
{
    public static string StatusLabel(int? code)
    {
        if (code == null) return "unknown";

        string category = code switch
        {
            >= 200 and < 300 => "✓ Success",
            >= 300 and < 400 => "→ Redirect",
            >= 400 and < 500 => "✗ Client Error",
            >= 500 => "✗ Server Error",
            _ => ""
        };

        return $"{code}  ({category})";
    }

    public static string ResponseLabel(double? ms)
    {
        if (ms == null) return "unknown";

        string speed = ms switch
        {
            < 100 => "fast",
            < 500 => "ok",
            < 1000 => "slow",
            _ => "very slow"
        };

        return $"{ms:F1} ms  ({speed})";
    }
}
