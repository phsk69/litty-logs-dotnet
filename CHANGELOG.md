# changelog 📜

all the glow ups and level ups for litty-logs no cap

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
