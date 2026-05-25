using Log_Analyzer.Response;
using System.Globalization;

namespace Log_Analyzer.Parsers;

public static class TimestampParser
{
    public static bool TryParse(string token, out DateTime result)
    {
        result = default;

        if (DateTime.TryParseExact(
                token,
                Constants.KnownTimestampFormats,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out result))
        {
            return true;
        }

        if (long.TryParse(token, out long epoch))
        {
            long epochInSeconds = token.Length switch
            {
                9 or 10 => epoch,
                13 => epoch / Constants.MillisecondsDivisor,
                16 => epoch / Constants.MicrosecondsDivisor,
                19 => epoch / Constants.NanosecondsDivisor,
                _ => -1
            };

            if (epochInSeconds != -1)
            {
                result = DateTimeOffset.FromUnixTimeSeconds(epochInSeconds).UtcDateTime;
                return true;
            }
        }

        return false;
    }

    public static TimestampResponseDto ExtractTimestamp(string line)
    {
        var result = new TimestampResponseDto { OriginalToken = line };

        if (string.IsNullOrWhiteSpace(line))
        {
            result.Success = false;
            result.FailureReason = "Line is blank or whitespace";
            return result;
        }

        var parts = line.Trim().Split(' ');

        if (TimestampParser.TryParse(parts[0], out DateTime ts))
        {
            result.Success = true;
            result.Timestamp = ts;
            result.CharsConsumed = parts[0].Length + 1;
            result.OriginalToken = parts[0];
            return result;
        }

        if (parts.Length < 2)
        {
            result.Success = false;
            result.FailureReason = $"Could not parse timestamp from: '{parts[0]}'";
            return result;
        }

        string twoTokens = parts[0] + " " + parts[1];

        if (TimestampParser.TryParse(twoTokens, out ts))
        {
            result.Success = true;
            result.Timestamp = ts;
            result.CharsConsumed = twoTokens.Length + 1;
            result.OriginalToken = twoTokens;
            return result;
        }

        result.Success = false;
        result.FailureReason = $"Could not parse timestamp from: '{twoTokens}'";
        return result;
    }

}