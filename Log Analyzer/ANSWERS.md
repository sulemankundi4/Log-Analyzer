# How to Run

Please refer to `README.md`, I have added details there.

# Stack Choice

I went with **C# / .NET 10** because it is the language I know best, and it gave me everything I needed for this task without friction:
*   **Nullable types** like `int? StatusCode` and `double? ResponseTimeMs` naturally express that a field may be absent. 
*   The **switch expression** kept timestamp and response time normalization clean. 
*   **`System.Text.Json`** is built into the standard library so no external dependencies were needed.
*   **`File.ReadLines`** streams line by line, so the tool handles files of any size without memory issues.

A worse choice would have been a loosely typed or scripting-oriented language. When your core problem is distinguishing between "field missing", "field present but invalid", and "field present and valid", weak typing makes those distinctions easy to accidentally collapse. A compile-time type system catches that class of bug before the code even runs, which matters a lot when the goal is a tool that never crashes on unexpected input.

# One Real Edge Case

**File:** `TimestampParser.cs` — Unix epoch unit detection via length check.

Unix epoch timestamps can arrive in four different units depending on the system that wrote the log:
```csharp
long epochInSeconds = token.Length switch
{
    9 or 10 => epoch,                         
    13      => epoch / Constants.MillisecondsDivisor,
    16      => epoch / Constants.MicrosecondsDivisor,
    19      => epoch / Constants.NanosecondsDivisor,
    _       => -1   
};
```
Without this handling, a millisecond epoch like `1710512581000` would either be passed raw to `DateTimeOffset.FromUnixTimeSeconds` — producing a date somewhere in the year 56,000 — or rejected entirely. The length of the digit string is a reliable signal for the unit, so normalizing to seconds first before calling `FromUnixTimeSeconds` means all four variants produce the correct UTC time.

# AI Usage

I used **Claude** as an assistant throughout the task. I drove the approach and we worked through each problem together — malformed line detection, timestamp parsing, JSON field extraction strategy, and project structure.

I corrected the AI output several times where it did not meet the standard I wanted. **One concrete example:**

Claude initially placed the `parts.Length < 2` guard before the single-token timestamp attempt. I caught that this would incorrectly reject valid ISO 8601 timestamps like `2024-03-15T14:23:01Z` since they are a single token and would never reach the parse attempt. I flagged it, we fixed the order, and the guard was moved to after the single-token attempt.

Throughout the task I reviewed every output, questioned anything that looked wrong, and shaped the final code rather than just accepting what was generated.

# Honest Gap

The output is purely a console log — every parsed line printed one by one. For someone actually on call, that is not useful at scale.

With another day I would add a summary report: top 10 slowest endpoints, error rate by path, busiest IPs, and request volume over time. A basic frontend or even an HTML file export of that report would make the tool genuinely useful rather than just technically correct.