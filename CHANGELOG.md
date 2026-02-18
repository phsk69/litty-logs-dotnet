# changelog 📜

all the glow ups and level ups for litty-logs no cap

## [0.2.0] - 2026-02-18

### the tool era 🧪

`dotnet litty` dropped and now your build AND test output is bussin too fr fr

#### added
- `LittyLogs.Tool` — `dotnet litty` CLI tool installable via `dotnet tool install` 🔧
  - `dotnet litty test` — wraps dotnet test, rewrites all runner output into gen alpha slang
  - `dotnet litty build` — wraps dotnet build, same treatment
  - auto-injects detailed logging so ITestOutputHelper output actually shows up
  - all args pass through to the underlying dotnet command no cap
- `BuildOutputRewriter` — rewrites MSBuild output (restore, compile, warnings, errors, build summary) 🏗️
- `TestOutputRewriter` — rewrites xUnit runner output (discovering, starting, passed/failed, summary) 🧪
- `DotnetProcessRunner` — shared subprocess helper for real-time line-by-line output rewriting
- 34 new tests for the rewriters (100 total now bestie)
- `just litty-test` and `just litty-build` recipes for that dev workflow rizz

## [0.1.0] - 2026-02-18

### the genesis era 🌅

first drop of litty-logs into the world bestie. the vibes are immaculate

#### added
- `LittyLogsFormatter` — custom ConsoleFormatter with emojis, ANSI colors, and short category names 🔥
- `LittyMessageRewriter` — the secret sauce that rewrites boring framework messages into gen alpha slang 🧠
- `LittyLogsFormatHelper` — shared formatting brain that all providers eat from (console, xunit, future sinks) 🧠
- `AddLittyLogs()` extension method — one line setup no cap 💅
- `LittyLogsOptions` — configure the vibe (rewriting, colors, timestamps, categories)
- `LittyLogs.Xunit` — separate package for xUnit test output integration through ITestOutputHelper
  - `LittyLogsXunitProvider` + `LittyLogsXunitLogger` — ILoggerProvider that writes litty output to test results
  - `AddLittyLogs(ITestOutputHelper)` — one line xUnit setup
  - `output.CreateLittyLogger<T>()` — convenience one-liner for when you just need a logger
- ISO 8601 timestamps with millisecond precision for that international rizz 🌍
- ~15 framework message rewrites covering hosting lifecycle, request logging, and endpoint routing
- four example projects: web api, hosted service, console script, and xUnit tests
- full test coverage because we test our code like responsible besties
