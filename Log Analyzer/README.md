# Log Analyzer

A CLI tool that parses server log files and produces a structured analysis report.
Handles multiple log formats, malformed lines, and mixed JSON entries gracefully, never crashes on bad input.

---

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download)

Verify your install:

```powershell
dotnet --version
```

---

## Project Structure

```
Log Analyzer/
└── Log Analyzer/
    ├── Helpers/               # Shared utility methods
    ├── LogPrinter/            # Console output formatting
    ├── Parsers/               # Field parsers (timestamp, IP, method, path, status, response time)
    ├── Response/              # DTOs — LogEntryResponseDto, TimestampResponseDto
    ├── Script/                # Test log file generator
    ├── Constants.cs           # Shared constants
    └── Program.cs             # Entry point — routes to analyzer or generator
```

---

## How to Run

### 1. Clone and navigate to the project folder

```powershell
cd "Log Analyzer\Log Analyzer"
```

---

### 2. Generate a test log file

This creates a realistic log file with all deviations mixed in and saves it to `bin\Debug\net10.0\Logs\`.

```powershell
# Default — 1000 lines → test.log
dotnet run -- --generate

# Custom line count
dotnet run -- --generate 5000

# Custom line count + custom filename
dotnet run -- --generate 5000 my_test.log
```

Generated file location:

```
bin\Debug\net10.0\Logs\test.log
```

---

### 3. Analyze a log file

Pass the path to any log file as the first argument.

```powershell
# Analyze the generated test file
dotnet run -- "bin\Debug\net10.0\Logs\test.log"

# Analyze any other log file
dotnet run -- "C:\path\to\your\file.log"
```

> **Note:** The `--` separates dotnet's own arguments from your program's arguments.
> Everything after `--` is passed into `args[]` in `Program.cs`.

---

### 4. Generate and analyze in one go

```powershell
dotnet run -- --generate
dotnet run -- "bin\Debug\net10.0\Logs\test.log"
```

---

## Example Output

```
  Analyzing: bin\Debug\net10.0\Logs\test.log

─────────────────────────────────────────────────────
  [STD]
  Timestamp    : Friday, 15 March 2024  14:23:01 UTC
  IP           : 192.168.1.42
  Method       : GET
  Path         : /api/users
  Status       : 200  (✓ Success)
  Response Time: 142.0 ms  (fast)
─────────────────────────────────────────────────────
  [JSON]
  Timestamp    : Friday, 15 March 2024  14:23:05 UTC
  IP           : 10.0.0.5
  Method       : DELETE
  Path         : /api/users/7
  Status       : 204  (✓ Success)
  Response Time: 31.0 ms  (fast)
─────────────────────────────────────────────────────
  [MALFORMED] Could not parse timestamp from: 'NullPointerException:'
  Raw         : NullPointerException: at line 42
─────────────────────────────────────────────────────

╔═════════════════════════════════════════════════════╗
║                      SUMMARY                        ║
╠═════════════════════════════════════════════════════╣
║  Total Lines     : 1000                             ║
║  Valid Lines     : 942                              ║
║  ├─ Standard     : 830                              ║
║  └─ JSON Format  : 112                              ║
║  Malformed Lines : 58                               ║
╚═════════════════════════════════════════════════════╝
```

---

## What the Generator Produces

Every generated file contains a realistic mix of:

| Category       | Share  | Description                                              |
|----------------|--------|----------------------------------------------------------|
| Standard lines | ~81%   | Normal log entries with various deviations               |
| Valid JSON     | ~10%   | 9 different JSON variants with different key names       |
| Bad JSON       | ~2%    | Unclosed braces, empty objects, missing required fields  |
| Stack traces   | ~2%    | Java / Python exception traces leaking into the log      |
| System messages| ~2%    | FATAL / WARN / DEBUG / INFO noise lines                  |
| Blank lines    | ~3%    | Empty or whitespace-only lines                           |
