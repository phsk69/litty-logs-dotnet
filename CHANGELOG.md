# changelog 📜

all the glow ups and level ups for litty-logs no cap

## [0.2.4] - 2026-06-27

### dep glow up — Resilience pinned + locked in 🔒🔥

small but mighty maintenance drop bestie. bumped the HTTP resilience dep off the floating `10.0.*` range onto a pinned `10.7.0` so builds are reproducible and nobody gets surprise-patched mid-flight 💀. all 247 tests still slay, zero behavior change — webhook delivery stays just as bussin 🪝🔥

#### changed — pin Microsoft.Extensions.Http.Resilience to 10.7.0 🔒
- `LittyLogs.Webhooks` was floating on `10.0.*` which meant the resolved version could drift on every restore — not reproducible no cap
- now pinned to `10.7.0` for deterministic builds across the squad
- the Polly-backed retry/resilience pipeline for Matrix + Teams webhook delivery is unchanged — same best-effort, never-crash-your-app energy
- 247 tests all passing, no regressions 🧪🔥

## [0.2.3] - 2026-02-23

### the teams era + severity slay + litty clean drop 🟦🔥🗑️

Teams Adaptive Cards are LIVE, severity colors actually work for all levels now (they were lowkey bricked before no cap 💀), `dotnet litty clean` just dropped, justfile got decluttered, and all docs got the glow up. 247 tests all passing. we cooked SO HARD this release bestie 🏆

#### added — Teams Adaptive Cards webhook sink 🟦
- `AddLittyTeamsLogs(url)` — one liner to yeet logs to Teams via Adaptive Cards
- `AddLittyTeamsLogs(url, opts => ...)` — full control over MinimumLevel, batch config, username
- `TeamsPayloadFormatter` — Adaptive Card v1.5 with severity-colored containers (green/yellow/red/neutral) 🎨
- monospace TextBlocks for log lines, subtle blocks for exception stack traces
- same Channel\<T\> batching + Polly resilience as Matrix — best-effort delivery, never crashes your app
- TextBlock plain text rendering = inherently safe against XSS, tracking pixels, phishing links no cap 🔒

#### added — `dotnet litty clean` CLI command 🗑️
- wraps `dotnet clean` and rewrites boring output into gen alpha vibes
- `"Deleting file"` → `"yeeted: filename 🗑️"` with just the filename, no full path noise
- `CoreClean:` MSBuild noise gets suppressed
- `"Build succeeded"` → `"all artifacts yeeted into the void bestie 🗑️🔥"`
- auto-injects `--verbosity normal` so you actually see what gets yeeted
- falls back to build rewriter for warnings, errors, timing etc

#### fixed — Teams severity colors only worked for warning level 💀
- `DetectSeverityStyle()` was checking for `[💀 error]` and `[☠️ critical]` but `GetLevelInfo()` returns labels `"err"` and `"crit"` — only warning matched because its the same in both places
- tests passed because they used hand-crafted strings with the wrong labels 🤡
- fixed detection to match `[💀 err]` and `[☠️ crit]` — all severity colors slay now
- added pipeline resilience tests that derive from `GetLevelInfo()` so label drift can never hide again no cap 🔒

#### changed — justfile declutter + litty tool everywhere 🧹
- all main recipes (`build`, `test`, `pack`, `clean`) now use `dotnet litty` for that gen alpha output
- yeeted redundant `litty-build`, `litty-test`, `litty-pack` prefixed recipes — the main ones ARE litty now
- renamed `litty-publish` → `publish` for consistency
- `example xunit` and `nuget-push` pack step also use litty tool

#### changed — examples yeet Console.WriteLine 🐕
- all example projects replaced `Console.WriteLine` with proper `ILogger` meta loggers
- examples now eat their own dogfood — structured logging output even for the demo scaffolding

#### changed — docs glow up 📜
- **README** — Teams Adaptive Cards section, updated CLI tool description for clean command, recipes table matches reality, security summary updated
- **security.md** — Teams Adaptive Card security model documented (TextBlock plain text rendering = inherently safe)
- **TODO** — yeeted shipped sections because a todo is a todo bestie, not a victory lap

#### changed — test count 🧪
- 247 tests all passing (up from 216 in v0.2.2)
- 4 new pipeline resilience tests that go through the REAL formatting pipeline — no more hand-crafted string copouts
- 11 new `CleanOutputRewriter` tests covering all transforms + fallback
- Teams test strings fixed from wrong labels to correct `[💀 err]` / `[☠️ crit]`

## [0.2.2] - 2026-02-22

### pipeline glow up — fully retryable release pipeline 🔄🔥

#### fixed — github release step bricks on re-run 💀
- `gh release create` straight up fails if the release already exists on github (e.g. when you re-run the pipeline from the forgejo UI)
- now we check if the release exists first — if it does we just upload fresh `.nupkg` assets with `--clobber`
- if it doesnt exist we create it like normal
- the forgejo release step was already retryable, now github matches. whole pipeline is re-run safe no cap 🔄

#### removed — overengineered publish approval gate 🗑️
- yeeted the draft release + publish.yml dual-trigger approach that was mega cooked
- turns out the standard pattern is dead simple: tag push → CI builds + tests → auto-publish if tests pass
- the CI pipeline IS the gate. if tests fail nothing ships. if they pass everything ships. thats how every repo on earth does it fr fr
- one workflow file (`release.yml`) does the entire pipeline in one shot — build, test, pack, NuGet, forgejo release, github release 🔥

## [0.2.1] - 2026-02-22

### the webhook rendering hotfix — chat output went from bricked to bussin 🪝🔥

v0.2.0 shipped with webhook rendering that was MEGA COOKED in Matrix chat — all messages mashed into one line, brackets getting backslash-escaped, emojis rendering as literal boxes. three rounds of fixes later we absolutely cracked it. the vibes are immaculate now bestie 💅

#### fixed — messages mashed into one line in Matrix chat 💀
- hookshot treats the `text` field as CommonMark markdown where single `\n` = soft line break = COLLAPSED TO A SPACE
- our `string.Join("\n", messages)` was feeding hookshot one giant paragraph instead of separate lines
- now we send an `html` field alongside `text` — hookshot PREFERS html when present (per the docs)
- messages separated by `<br/>` in HTML, `\n\n` (paragraph breaks) in text fallback
- exception stack traces render in `<pre><code>` blocks for that proper monospace energy

#### fixed — emojis rendering as boxes in Matrix chat 💀
- `System.Net.WebUtility.HtmlEncode()` encodes ALL chars >= 160 to numeric HTML entities (`&#128548;`)
- Matrix hookshot cant render those as actual emojis — just shows boxes
- replaced with minimal `HtmlEscape()` that only encodes the 5 dangerous HTML chars (`<>&"'`)
- emojis stay as literal UTF-8 bytes — 😤💀☠️🔥 all rendering perfectly now

#### fixed — brackets getting backslash-escaped in chat output 💀
- the custom `MarkdownSanitizer` was escaping `[` `]` in our log format `[😤 warning] [timestamp] [category]`
- Matrix rendered `\[😤 warning\]` with literal backslashes — thats not it
- yeeted the entire `MarkdownSanitizer.cs` — HTML encoding in the formatter handles all injection now
- no more cursed custom regex markdown escaping, just the stdlib doing its job no cap 🗑️

#### fixed — webhook example not loading `.env` file 💀
- .NET dont load `.env` files natively — `Environment.GetEnvironmentVariable("HOOKSHOT_URL")` was always null even with `.env` in the repo root
- added a simple `.env` file parser at the top of the webhook example that walks up the directory tree
- mock mode works from any directory, live mode works when `.env` has `HOOKSHOT_URL` set

#### changed — security approach for webhook messages 🔒
- markdown injection prevention now uses HTML encoding via `HtmlEscape()` instead of custom `MarkdownSanitizer`
- hookshot renders the `html` field directly — no markdown parsing means no markdown injection possible
- XSS, tracking pixels, phishing links all neutralized by standard HTML char escaping
- `docs/security.md` updated to document the new approach

#### changed — test count 🧪
- 216 tests all passing (up from 213 in v0.2.0)
- new tests verify: html field presence, `<br/>` line breaks, `<pre><code>` exception blocks, HTML encoding of XSS/links/images, paragraph breaks in text fallback

## [0.2.0] - 2026-02-22

### the webhook era — yeet logs to Matrix chat AND a security glow up 🪝🔒🔥

the biggest drop yet no cap. whole new package for webhook logging, two new CLI commands, security hardening that would make a boomer guru cry tears of joy, and a live hookshot integration demo. 213 tests all passing. we cooked HARD this release bestie 🏆

#### added — `LittyLogs.Webhooks` package 🪝
- whole new NuGet package for yeeting logs to Matrix chat via hookshot webhooks
- `AddLittyMatrixLogs()` one-liner extension method — same energy as the other litty-logs setups
- async batching engine using `Channel<T>` — groups messages by interval (2s default) or count (10 default), your app thread NEVER blocks 👑
- Polly resilience via `Microsoft.Extensions.Http.Resilience` — retry with exponential backoff, circuit breaker, per-request timeout. if the webhook is bricked we drop the batch and keep vibing, never crashes your app 🔒
- `IHttpClientFactory` with named client `"LittyWebhooks"` — proper socket management no socket exhaustion
- min level filtering — default `Warning` so your group chat dont get spammed with trace logs 💀
- custom username support — show up in chat as `LittyBot9000` or whatever you want
- exceptions wrapped in markdown code blocks so they render nice in chat
- Matrix hookshot format with full emoji support because JSON is UTF-8 native

#### added — `dotnet litty pack` CLI command 📦
- wraps `dotnet pack` and rewrites the boring nupkg output into gen alpha slang
- `Successfully created package` lines show the filename with "cooked and ready to yeet to NuGet" energy
- `Pack succeeded` / `Pack FAILED` get the full litty treatment
- falls back to build rewriter for restore, compile, warnings, errors, timing
- `just litty-pack` recipe added to justfile

#### added — `dotnet litty publish` CLI command 📤
- wraps `dotnet publish` with transforms for publish-specific output
- self-contained, trimming, ReadyToRun, native code gen, compressing — all litty-fied
- publish artifact paths get "packed and ready to ship" energy
- falls back to build rewriter for all the standard MSBuild lines

#### added — security hardening 🔒
- **SSRF prevention** — webhook URLs validated at registration time with scheme restriction to `http`/`https` only. `file://`, `ftp://`, `gopher://` and other sketchy schemes get yeeted with `ArgumentException`. fail at startup not at 3am bestie
- **log injection prevention** — newlines in text format messages (`\r\n`, `\n`, `\r`) sanitized to spaces. blocks fake log entry injection. JSON format already safe via `Utf8JsonWriter`
- **markdown injection prevention** — webhook messages backslash-escape markdown syntax (`[]()!*_` etc) so tracking pixels and phishing links render as literal text in chat, not as clickable/renderable markdown
- **HTTP category filtering** — `System.Net.Http`, `Microsoft.Extensions.Http`, and `Polly` categories routed to `NullLogger.Instance` in the webhook provider. prevents infinite recursion (webhook POST → HTTP log → webhook POST → ...) AND accidental webhook URL token exposure in logs
- **`docs/security.md`** — full security documentation covering trust model, whats protected, and security reporting

#### added — webhook example with live hookshot support 🪝🔥
- `examples/LittyLogs.Example.Webhooks` — seventh example project
- mock mode (default) — spins up local `HttpListener`, captures payloads, displays raw JSON and chat preview
- live mode — set `HOOKSHOT_URL` env var to hit a real Matrix hookshot. logs actually land in your room no cap
- two demos: default Warning+ config and custom Trace+ with LittyBot9000 username

#### added — `just re-release` recipe 🔄
- one command to un-brick a release when you forgot the changelog or need a do-over
- nukes old releases on forgejo + github, deletes tags everywhere, re-does the gitflow release
- non-fatal errors on cleanup steps so it keeps vibing even if some stuff is already gone
- requires `.env` with `FORGEJO_PAT` and `GH_PAT`

#### changed — test count 🧪
- 213 tests all passing (up from ~150 in the expansion pack era)
- webhook tests cover formatter, options, logger, provider, writer, integration, URL validation, markdown sanitization
- pack rewriter tests cover all transforms + build fallback
- publish rewriter tests cover all transforms + edge cases

## [0.1.4] - 2026-02-20

### the mirror arc — forgejo push mirroring does the heavy lifting now 🪞🔥

the manual github mirror push in the release pipeline was doing too much — forgejo push mirroring already syncs branches + tags to github automatically. yeeted the redundant step and replaced it with a smart wait-for-tag loop so the github release creation dont race the mirror sync. docs got the glow up to match fr fr

#### fixed — manual mirror push was redundant and sometimes caught Ls 💀
- yeeted the `push to github mirror so the tag exists over there` step from `release.yml` — forgejo push mirroring (Settings → Mirror) already handles syncing branches + tags to github automatically
- added a wait-for-tag polling loop (up to 30 attempts, 2s apart) that checks `gh api repos/.../git/refs/tags/{TAG}` before creating the github release — no more racing the mirror sync 🏎️

#### changed — `GH_PAT` scope clarified in runner docs 📜
- `docs/runner-setup.md` updated: `GH_PAT` is now only for `gh release create` API calls, not for pushing git refs
- release flow docs simplified — all `just release` / `just hotfix` / `just finish` commands auto-push, no manual `git push` instructions needed fr fr
- pipeline step descriptions updated to mention mirror sync + retryability

## [0.1.3] - 2026-02-19

### the infrastructure arc — release pipeline went from bricked to bussin 🏗️🔥

the whole release infra got a glow up. triple release pipeline (forgejo + github + nuget) actually works now, retryable without catching Ls, gitflow branches got the v-prefix drip, and the justfile recipes do everything for you. we tested the pipeline to DEATH with 0.1.0-dev through 0.1.2-dev and NOW it slays fr fr

#### fixed — release pipeline wasnt releasing to github 💀
- added a mirror push step that yeets main + tag to github BEFORE `gh release create` — cant create a release if the tag dont exist over there bro
- forgejo release step was bash-executing the changelog (backticks became command substitution lmaooo) — fixed by passing notes through `RELEASE_NOTES` env var instead of raw `${{ }}` inline expansion

#### fixed — release pipeline fully retryable 🔄
- forgejo release: checks `GET /releases/tags/{tag}` first, skips creation if already exists, deletes + re-uploads assets on retry
- github release: `gh release view` first, if exists uses `gh release upload --clobber` to overwrite assets
- nuget: already had `--skip-duplicate` so it was chillin
- git push mirror: naturally idempotent king 👑
- re-run the workflow from the forgejo UI all day long, zero errors

#### added — pre-release auto-detection 🧪
- versions with `-` in them (like `0.1.0-dev`, `1.0.0-beta.1`) auto-flag releases as pre-release on both forgejo and github
- no more shipping test releases as full releases thats not it

#### added — v-prefix on gitflow branches 🏷️
- `just release patch` now creates `release/v0.1.1` not `release/0.1.1`
- `just hotfix patch` now creates `hotfix/v0.1.1` not `hotfix/0.1.1`
- git flow creates `v0.1.1` tags automatically from the branch name — everything matches

#### added — `just finish` universal recipe 🏁
- auto-detects if youre on a hotfix, release, or support branch
- runs `git flow {type} finish` with no autoedit
- pushes develop + main + tag to origin automatically
- one command to rule them all, no more copy-pasting push commands

#### added — `just release-dev` recipe 🧪
- `just release-dev patch` → `0.1.0` becomes `0.1.1-dev`
- `just release-dev minor beta.1` → `0.1.0` becomes `0.2.0-beta.1`
- full gitflow cycle: bump + label → start → finish → push → pipeline triggered
- label defaults to `dev` if you dont specify

#### changed — `just release` and `just release-current` auto-push 📤
- no more "now push everything to trigger the pipeline" instructions
- recipes push develop + main + tag automatically after finishing

#### added — runner docs updated 📜
- `jq` and `curl` added to required software table in `docs/runner-setup.md`
- forgejo release step uses both for API calls and nobody told the runner docs 💀

## [0.1.0-dev] - 2026-02-19

### 🚨 TEST RELEASE — THIS AINT THE FINAL FORM BESTIE 🚨

yo this is a PRE-RELEASE so we can test the triple release pipeline (forgejo + github + nuget) before we go live with the real 0.1.0 drop. if you somehow installed this from nuget... bro why 💀 wait for the actual release fam. we just making sure the pipes aint clogged before we let the water flow fr fr

that said the FEATURES are all real and bussin. everything below is shipping in the actual release too no cap 🔥

### the expansion pack era — JSON + file sink + RFC 5424 + timestamp drip 🔥📁⏰

the squad got BIGGER and the vibes got CONFIGURABLE bestie. structured JSON for your log aggregators, a whole file sink with async I/O and gzip compression, RFC 5424 compliance so Loki and Datadog recognize the energy, AND a new timestamp ordering system that lets you choose your own adventure. emojis are UTF-8 native in JSON too because we dont play 🏆

#### added — JSON structured logging (in core `LittyLogs` package) 🍽️
- `LittyLogsJsonFormatter` — console formatter that outputs fire JSON instead of boring text
- `FormatJsonLine()` in the shared brain — `Utf8JsonWriter` based, zero-alloc king energy 👑
- `AddLittyJsonLogs()` extension method — one liner for structured JSON console output
- emojis serialize perfectly into JSON fields — `emoji`, `level`, `category`, `message`, `eventId`, `exception`
- framework message rewrites with emojis land in the JSON too because consistency is bussin 🔥

#### added — file sink (`LittyLogs.File`) 📁
- `LittyFileLogsProvider` + `LittyFileLogger` — ILoggerProvider that yeets litty logs to disk
- `LittyFileWriter` — async I/O engine using `Channel<string>` (bounded 10k), non-blocking writes 👑
- `AddLittyFileLogs()` extension methods — one liner file logging setup
- text or JSON output format — dealers choice bestie
- `TimestampFirst` support on `LittyFileLogsOptions` — file sink gets the timestamp drip too ⏰
- size-based rotation — when the file gets too thicc, we rotate 🔄
- time-based rotation — daily or hourly intervals 📅
- gzip compression — old rotated files auto-compress to `.gz` using BCL `GZipStream` (zero deps) 🗜️
- startup safeguard — NEVER auto-rotates on startup, only before writing the next entry 🔒
- no ANSI codes in file output — terminal escape chars in files is cursed and we dont do cursed 💀

#### added — `TimestampFirst` config option ⏰
- two timestamp orderings because one size does NOT fit all fr fr:
  - **level-first** (default) — `[🔥 info] [2026-02-19T...]` — RFC 5424 syslog vibes, level hits your eyes first
  - **timestamp-first** — `[2026-02-19T...] [🔥 info]` — observability mode, timestamps lead for when youre correlating across services
- `opts.TimestampFirst = true` on `LittyLogsOptions` and `LittyFileLogsOptions`
- works across console text, console JSON (key order stays same tho), file text, file JSON, and xUnit output
- ALL examples now auto-showcase BOTH modes when you run em — no hidden flags, no secret handshakes 💅

#### added — examples that actually FLEX 💪
- every single `just example <name>` now shows ALL the modes automatically
- `just example web` — startup demo with level-first → timestamp-first → JSON, then server boots with default config
- `just example hosted` — startup demo with both timestamp modes, then the background service runs
- `just example filesink` — writes text (level-first) → text (timestamp-first) → JSON, reads em all back
- `just example json` — shows JSON with both timestamp configs, explains key order stays consistent
- `just example xunit` — got a new `TimestampFirst_ShowsObservabilityOrdering` test that flexes the mode
- `just example console` — was already goated, still goated 👑
- no more "run with --timestamp-first to see the other mode" nonsense. you run the example, you see ALL the rizz. period 🔥

#### changed — RFC 5424 level labels 🔍
- log level labels now use RFC 5424 syslog severity keywords for maximum log aggregator compatibility
- `TRACE` → `trace`, `DBG` → `debug`, `INFO` → `info`, `WARN` → `warning`, `ERR` → `err`, `CRIT` → `crit`
- Loki, Datadog, Splunk etc gonna recognize these instantly no cap

#### changed — JSON unicode encoding 🔍
- JSON output now uses `UnsafeRelaxedJsonEscaping` so emojis and special chars are literal UTF-8
- `\u2620\uFE0F` → `☠️`, `\u002B` → `+`, `\u2014` → `—` — your Loki emoji searches actually hit now
- supplementary plane emojis still get the surrogate pair post-processor treatment 💅

#### fixed — CI and packaging 🏗️
- nuget packages for File, Xunit, and Tool now include README.md so nuget.org listing aint naked 📦
- CI test step no longer hardcodes the test count — it just checks all the vibes dynamically 🧪
- example web api no longer tries to pack itself (IsPackable stays false where it should)

#### fixed — gitflow release recipes 🚀
- `just release` and `just hotfix` now calculate the version FIRST then start the git flow branch while the tree is still clean — no more dirty tree errors when git flow tries to switch branches
- `just hotfix-finish` was already correct bestie it was born perfect 💅

#### added — triple release pipeline docs 📜
- `docs/runner-setup.md` now covers the full triple release drip:
  - **forgejo releases** — auto `GITHUB_TOKEN`, .nupkg assets attached via Gitea API 🏠
  - **github mirror releases** — `GH_PAT` fine-grained token, Contents read/write on the mirror repo 🐙
  - **nuget.org** — `NUGET_API_KEY` with push scope and `LittyLogs*` glob pattern 📦
- troubleshooting section covers all three destinations
- principle of least privilege is emphasized because security is bussin 🔒

#### added — dev experience 🧰
- 146 tests because we test our code like responsible besties (up from 104 in genesis era) 🧪
- six example projects: web api, hosted service, console, xUnit, JSON, and file sink
- `just pack` now builds four packages (LittyLogs, LittyLogs.Xunit, LittyLogs.File, LittyLogs.Tool)
- `just release` / `just hotfix` / `just hotfix-finish` recipes for gitflow release management
- `just nuget-push` for manual local publishes
- `just bump` / `just bump-pre` for version management
- shell completions via `just setup-completions` for zsh and bash

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
