using System.Text;
using System.Text.Json;

namespace Log_Analyzer.Script;


// ─────────────────────────────────────────────────────────────────────────────
// LogGenerator.cs
// Generates a realistic test log file with all deviations described in the task.
//
// Usage:
//   dotnet run --project LogGenerator              → 1000 lines → test.log
//   dotnet run --project LogGenerator 5000         → 5000 lines → test.log
//   dotnet run --project LogGenerator 5000 out.log → 5000 lines → out.log
// ─────────────────────────────────────────────────────────────────────────────



public class GeneratorStats
{
    public int Total { get; set; }
    public int Standard { get; set; }
    public int JsonValid { get; set; }
    public int JsonInvalid { get; set; }
    public int Blank { get; set; }
    public int StackTrace { get; set; }
    public int SystemMessage { get; set; }
}

// ─────────────────────────────────────────────────────────────────────────────
// Generator
// ─────────────────────────────────────────────────────────────────────────────

public class LogGenerator
{
    private readonly Random _rng;

    // ── Data pools ───────────────────────────────────────────────────────────

    private static readonly string[] IPs =
    {
        "192.168.1.42", "10.0.0.7",  "172.16.0.5",    "10.0.0.15",
        "192.168.0.100","203.0.113.55","198.51.100.23","10.10.10.10"
    };

    // GET weighted higher — reflects real traffic
    private static readonly string[] Methods =
    {
        "GET","GET","GET","POST","POST","PUT","DELETE","PATCH"
    };

    private static readonly string[] Paths =
    {
        "/api/users",     "/api/users/12",    "/api/users/99",
        "/api/orders",    "/api/orders/5",    "/api/orders/88",
        "/api/login",     "/api/logout",
        "/api/products",  "/api/products/7",
        "/api/search",    "/api/health",
        "/api/metrics",   "/api/auth/token",
        "/api/cart",      "/api/checkout"
    };

    // 200 weighted — most requests succeed
    private static readonly int[] StatusCodes =
    {
        200, 200, 200, 200,
        201, 204,
        301, 302,
        400, 401, 403, 404,
        500, 502, 503
    };

    private static readonly string[] UserAgents =
    {
        "\"Mozilla/5.0 Chrome/120\"",
        "\"Mozilla/5.0 Firefox/119\"",
        "\"curl/7.68.0\"",
        "\"PostmanRuntime/7.32\"",
        "\"python-requests/2.31\""
    };

    private static readonly string[] StackTraceLines =
    {
        "NullPointerException: at line 42",
        "    at com.example.Service.handle(Request.java:88)",
        "    at com.example.App.main(App.java:14)",
        "java.lang.RuntimeException: Connection refused",
        "    at org.springframework.web.servlet.DispatcherServlet.doDispatch(DispatcherServlet.java:1067)",
        "Traceback (most recent call last):",
        "  File \"server.py\", line 88, in handle_request",
        "KeyError: 'user_id'"
    };

    private static readonly string[] SystemMessages =
    {
        "[FATAL] disk full, aborting write...",
        "WARN  something went wrong",
        "DEBUG scheduler heartbeat ok",
        "INFO  worker thread started",
        "[ERROR] connection pool exhausted",
        "::1 - - [15/Mar/2024:14:23:01 +0000]",
        "---",
        "partial line without timestamp"
    };

    private static readonly string[] BadJsonLines =
    {
        "{bad json: not valid}}",
        "{\"timestamp\": \"2024-03-15T14:23:01Z\"",   // unclosed
        "{\"only_key\": \"no useful fields here\"}",   // missing method + path
        "{",                                            // totally broken
        "{}"                                            // empty object
    };

    public LogGenerator(int seed = 42)
    {
        _rng = new Random(seed);
    }

    // ── Public entry point ───────────────────────────────────────────────────



    public GeneratorStats Generate(int totalLines, string fileName)
    {
        // Build Logs/ folder path next to the executable
        string logsFolder = Path.Combine(AppContext.BaseDirectory, "Logs");
        Directory.CreateDirectory(logsFolder);

        // Full output path inside Logs/
        string outputPath = Path.Combine(logsFolder, fileName);

        var stats = new GeneratorStats { Total = totalLines };
        var currentDt = new DateTime(2024, 3, 15, 14, 0, 0, DateTimeKind.Utc);
        var sb = new StringBuilder();

        for (int i = 0; i < totalLines; i++)
        {
            currentDt = currentDt.AddSeconds(_rng.Next(1, 10));

            double roll = _rng.NextDouble();
            string line;

            if (roll < 0.03)                    // 3%  blank lines
            {
                line = _rng.NextDouble() < 0.5 ? "" : "   ";
                stats.Blank++;
            }
            else if (roll < 0.05)               // 2%  stack trace lines
            {
                line = Pick(StackTraceLines);
                stats.StackTrace++;
            }
            else if (roll < 0.07)               // 2%  system messages
            {
                line = Pick(SystemMessages);
                stats.SystemMessage++;
            }
            else if (roll < 0.09)               // 2%  bad JSON
            {
                line = Pick(BadJsonLines);
                stats.JsonInvalid++;
            }
            else if (roll < 0.19)               // 10% valid JSON
            {
                line = MakeJsonLine(currentDt);
                stats.JsonValid++;
            }
            else                                // 81% standard lines
            {
                line = MakeStandardLine(currentDt);
                stats.Standard++;
            }

            sb.AppendLine(line);
        }

        var directory = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(outputPath, sb.ToString(), Encoding.UTF8);
        return stats;
    }

    // ── Standard line builder ────────────────────────────────────────────────

    private string MakeStandardLine(DateTime dt)
    {
        string ts = RandomTimestamp(dt);
        string ip = Pick(IPs);
        string method = Pick(Methods);
        string path = Pick(Paths);
        string status = RandomStatus();
        string rt = RandomResponseTime();

        string line = $"{ts} {ip} {method} {path} {status} {rt}";

        // ~5% chance of extra user agent field appended
        if (_rng.NextDouble() < 0.05)
            line += $" {Pick(UserAgents)}";

        return line;
    }

    // ── JSON line builder — 9 variants ──────────────────────────────────────

    private string MakeJsonLine(DateTime dt)
    {
        string ip = Pick(IPs);
        string method = Pick(Methods);
        string path = Pick(Paths);
        int status = Pick(StatusCodes);
        int rtMs = _rng.Next(5, 5000);

        return _rng.Next(1, 10) switch
        {
            // Variant 1 — standard keys
            1 => Json(new
            {
                timestamp = Iso(dt),
                ip,
                method,
                path,
                status,
                response_time_ms = rtMs
            }),

            // Variant 2 — alt keys: time/src/verb/uri/code/ms
            2 => Json(new
            {
                time = Iso(dt),
                src = ip,
                verb = method,
                uri = path,
                code = status,
                ms = rtMs
            }),

            // Variant 3 — epoch seconds timestamp
            3 => Json(new
            {
                ts = EpochSeconds(dt),
                client_ip = ip,
                http_method = method,
                endpoint = path,
                http_status = status,
                duration_ms = rtMs
            }),

            // Variant 4 — epoch milliseconds timestamp
            4 => Json(new
            {
                @timestamp = EpochMs(dt),
                remote_addr = ip,
                method,
                url = path,
                status_code = status,
                duration = rtMs
            }),

            // Variant 5 — combined request field "GET /api/users HTTP/1.1"
            5 => Json(new
            {
                timestamp = Iso(dt),
                ip_address = ip,
                request = $"{method} {path} HTTP/1.1",
                status,
                response_time_ms = rtMs
            }),

            // Variant 6 — response time as string "142ms" or "0.142s"
            6 => Json(new
            {
                timestamp = Iso(dt),
                ip,
                method,
                path,
                status,
                latency = _rng.NextDouble() < 0.5
                              ? $"{rtMs}ms"
                              : $"{rtMs / 1000.0:F3}s"
            }),

            // Variant 7 — extra noise fields (level, logger, msg)
            7 => Json(new
            {
                timestamp = Iso(dt),
                level = "info",
                logger = "http.server",
                ip,
                method,
                path,
                status,
                response_time_ms = rtMs,
                msg = "request handled"
            }),

            // Variant 8 — status as string "200"
            8 => Json(new
            {
                timestamp = Iso(dt),
                ip,
                method,
                path,
                status = status.ToString(),   // string not int
                response_time_ms = rtMs
            }),

            // Variant 9 — response time in seconds as float
            _ => Json(new
            {
                datetime = Iso(dt),
                host = ip,
                verb = method,
                route = path,
                response_code = status,
                elapsed = Math.Round(rtMs / 1000.0, 3)   // seconds
            }),
        };
    }

    // ── Timestamp generators — all 4 formats ─────────────────────────────────

    private string RandomTimestamp(DateTime dt)
    {
        return _rng.Next(0, 9) switch
        {
            0 or 1 or 2 => Iso(dt),                              // ISO 8601      (most common)
            3 or 4 => dt.ToString("yyyy/MM/dd HH:mm:ss"),   // Slash format
            5 or 6 => dt.ToString("dd-MMM-yyyy HH:mm:ss"),  // Human readable
            7 => EpochSeconds(dt).ToString(),          // Unix epoch seconds
            _ => EpochMs(dt).ToString(),               // Unix epoch milliseconds
        };
    }

    // ── Response time — ms / s / bare ────────────────────────────────────────

    private string RandomResponseTime()
    {
        int ms = _rng.Next(5, 5000);
        return _rng.Next(0, 4) switch
        {
            0 or 1 => $"{ms}ms",                        // "142ms"
            2 => $"{ms / 1000.0:F3}s",             // "0.142s"
            _ => ms.ToString()                     // "142"  bare number
        };
    }

    // ── Status code — occasionally missing ───────────────────────────────────

    private string RandomStatus()
        => _rng.NextDouble() < 0.04 ? "-" : Pick(StatusCodes).ToString();

    // ── Helpers ───────────────────────────────────────────────────────────────

    private T Pick<T>(T[] array)
        => array[_rng.Next(array.Length)];

    private static string Iso(DateTime dt)
        => dt.ToString("yyyy-MM-ddTHH:mm:ssZ");

    private static long EpochSeconds(DateTime dt)
        => new DateTimeOffset(dt).ToUnixTimeSeconds();

    private static long EpochMs(DateTime dt)
        => new DateTimeOffset(dt).ToUnixTimeMilliseconds();

    private static string Json(object obj)
        => JsonSerializer.Serialize(obj);
}
