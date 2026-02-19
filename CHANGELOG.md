# changelog 📜

all the glow ups and level ups for litty-logs no cap

## [Unreleased]

### the expansion pack era — JSON logging + file sink + RFC 5424 compliance dropped 🔥📁

the squad got BIGGER bestie. structured JSON output for log aggregators and a file sink with async I/O, rotation, and gzip compression. emojis everywhere because JSON is UTF-8 native no cap 🏆

#### added — JSON structured logging (in core `LittyLogs` package)
- `LittyLogsJsonFormatter` — console formatter that outputs fire JSON instead of plain text 🍽️
- `FormatJsonLine()` in the shared brain — `Utf8JsonWriter` based, zero-alloc king energy 👑
- `AddLittyJsonLogs()` extension method — one liner for structured JSON console output
- emojis serialize perfectly in JSON fields — `emoji`, `level`, `category`, `message`, `eventId`, `exception`
- framework message rewrites with emojis land in the JSON too no cap 🔥

#### added — file sink (`LittyLogs.File`)
- `LittyFileLogsProvider` + `LittyFileLogger` — ILoggerProvider that yeets litty logs to disk 📁
- `LittyFileWriter` — async I/O engine using `Channel<string>` (bounded 10k), non-blocking writes 👑
- `AddLittyFileLogs()` extension methods — one liner file logging setup
- text or JSON output format — your choice bestie
- size-based rotation — when the file gets too thicc, we rotate 🔄
- time-based rotation — daily or hourly intervals 📅
- gzip compression — old rotated files auto-compress to `.gz` using BCL `GZipStream` (zero deps) 🗜️
- startup safeguard — NEVER auto-rotates on startup, only before writing the next entry 🔒
- no ANSI codes in file output — terminal escape chars in files is cursed 💀

#### added — examples
- `LittyLogs.Example.Json` — JSON logging example showing structured output with emojis
- `LittyLogs.Example.FileSink` — file sink example with text + JSON output and rotation

#### changed — RFC 5424 level labels
- log level labels now use RFC 5424 syslog severity keywords for maximum log aggregator compatibility 🔥
- `TRACE` → `trace`, `DBG` → `debug`, `INFO` → `info`, `WARN` → `warning`, `ERR` → `err`, `CRIT` → `crit`
- Loki, Datadog, Splunk etc gonna recognize these instantly no cap 🔍

#### changed — JSON unicode encoding
- JSON output now uses `UnsafeRelaxedJsonEscaping` so emojis and special chars are literal UTF-8
- `\u2620\uFE0F` → `☠️`, `\u002B` → `+`, `\u2014` → `—` — your Loki emoji searches actually hit now 🔍
- supplementary plane emojis still get the surrogate pair post-processor treatment 💅

#### added — dev experience
- `just example-json` and `just example-filesink` recipes
- `just pack` now builds four packages (added LittyLogs.File)

## [0.1.0] - 2026-02-18

### the genesis era — the whole squad dropped at once 🌅🔥

first drop of litty-logs into the world bestie. core library, xunit integration, AND the CLI tool all shipping together. the vibes are immaculate no cap

#### added — core library (`LittyLogs`)
- `LittyLogsFormatter` — custom ConsoleFormatter with emojis, ANSI colors, and short category names 🔥
- `LittyMessageRewriter` — the secret sauce that rewrites boring framework messages into gen alpha slang 🧠
- `LittyLogsFormatHelper` — shared formatting brain that all providers eat from (console, xunit, future sinks) 🧠
- `AddLittyLogs()` extension method — one line setup no cap 💅
- `LittyLogsOptions` — configure the vibe (rewriting, colors, timestamps, categories)
- ISO 8601 timestamps with millisecond precision for that international rizz 🌍
- ~15 framework message rewrites covering hosting lifecycle, request logging, and endpoint routing

#### added — xunit integration (`LittyLogs.Xunit`)
- `LittyLogsXunitProvider` + `LittyLogsXunitLogger` — ILoggerProvider that writes litty output to test results
- `AddLittyLogs(ITestOutputHelper)` — one line xUnit setup
- `output.CreateLittyLogger<T>()` — convenience one-liner for when you just need a logger

#### added — CLI tool (`LittyLogs.Tool`)
- `LittyLogs.Tool` — `dotnet litty` CLI tool installable via `dotnet tool install` 🔧
  - `dotnet litty test` — wraps dotnet test, rewrites all runner output into gen alpha slang
  - `dotnet litty build` — wraps dotnet build, same treatment
  - auto-injects detailed logging so ITestOutputHelper output actually shows up
  - all args pass through to the underlying dotnet command no cap
- `BuildOutputRewriter` — rewrites MSBuild output (restore, compile, warnings, errors, build summary) 🏗️
- `TestOutputRewriter` — rewrites xUnit runner output (discovering, starting, passed/failed, summary) 🧪
- `DotnetProcessRunner` — shared subprocess helper for real-time line-by-line output rewriting

#### added — dev experience
- 104 tests because we test our code like responsible besties 🧪
- four example projects: web api, hosted service, console script, and xUnit tests
- `just litty-test` and `just litty-build` recipes for that dev workflow rizz
- full test coverage because accountability is bussin fr fr
