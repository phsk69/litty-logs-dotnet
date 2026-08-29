# changelog 📜🔥

all the glow-ups and level-ups for litty-logs no cap 🔥

## [Unreleased]

### breaking glow-ups — xUnit v3 🧪🔥

- replaced deprecated xUnit v2 packages with xUnit v3 `4.0.0` across the library, tests, and example 🔥
- moved the public `ITestOutputHelper` integration from `Xunit.Abstractions` to `Xunit`; consumers must remove `using Xunit.Abstractions` when upgrading 💥🔥
- converted both test projects to the executable xUnit v3 project model and selected Microsoft Testing Platform v2 for `.NET 10` test runs 🧪🔥
- taught `dotnet litty test` to use native MTP output flags while keeping the legacy VSTest logger fallback for repos still mid-migration 🛟🔥

### security glow-ups — signed supply chain 🔏🔥

- enforced trusted SSH signatures for every post-rollout commit, release commit, and immutable release tag 🔒🔥
- added a restricted repository-scoped release bot with an independent 1Password-backed signing key and PAT 🪪🔥
- made the ship workflow verify its tag before any NuGet package can leave the runner 📦🔏🔥

## [1.0.2] - 2026-08-29


### dependencies — fresh nuggies 🤖🔥

- Update https://data.forgejo.org/actions/checkout action to v7 🔥 🔥
- Update dependency xunit.runner.visualstudio to v4 🔥 🔥
- Update dependency microsoft.net.test.sdk to v18 🔥 (#6) 🔥


## [1.0.1] - 2026-08-29


### dependencies — fresh nuggies 🤖🔥

- Update dotnet monorepo 🔥 🔥


## [1.0.0] - 2026-08-29

### breaking glow-ups 💥🔥

- reset the public webhook platform surface around Slack, Matrix, and the generic webhook API; consumers of the retired chat adapter must move to `AddLittySlackLogs()` 🔥
- moved versioning and publishing fully onto squash commits from the `main` trunk, so the old manual release command surface is gone 🌳🔥

### added — Slack Block Kit 🟢🔥

- added `AddLittySlackLogs(url)` and its full-options overload without adding a Slack SDK dependency 🔥
- added safe Block Kit payloads with a fallback `text`, `mrkdwn: false`, one `plain_text` header, and one `plain_text` section per log 🔒🔥
- kept emojis and multiline exceptions while removing logger-owned markdown fences before Slack delivery 🟢🔥
- enforced Unicode-safe limits: 150 scalars per header, 3000 per section, and 49 logs plus one header per payload 🔒🔥
- retained Matrix hookshot and the generic `AddLittyWebhookLogs()` path with the existing async batching + resilience vibes 🟣🔥

### added — automated trunk releases 🌳🤖🔥

- added strict git-cliff SemVer: breaking changes major, features minor, fixes/perf/reverts/dependency chores patch, maintenance-only commits silent 🧠🔥
- added one rolling `release-pr` branch that can be deleted after merge and recreated from `main` with zero history dependence 🔥
- CI resolves the newest stable git-cliff `2.x` through a checksum-verifying action with fallback installs disabled, so fixes land without an invisible hard-pin fossil 🔒🔥
- added immutable tag guards: same tag + same commit is retry-safe, while any attempt to move or reuse a version fails hard 🔒🔥
- added pre-tag recovery so a CI repair can be folded into an already-prepared but still-untagged release exactly once 🔧🔒🔥
- added manual `auto`, `patch`, `minor`, `major`, and prerelease promotion dispatch modes that only open a PR 🔥
- added PR-title linting and release-policy graph fixtures, including a squash + deleted-source-branch regression 🧪🔥
- added read-only `just release-next` and `just release-notes` previews that never install dependencies 🔍🔥

### changed — repo and docs glow-up 📜🔥

- Renovate keeps non-major updates auto-merged after green CI while major updates stay normal human-reviewed PRs 🤖🔥
- dependency version changes are intentionally outside this release; Renovate owns the follow-up PRs so every bump gets its own CI signal 🤖🔒🔥
- Forgejo docs now require squash-only protected `main`, auto-deleted source branches, protected `v*` tags, and a scoped `RELEASE_TOKEN` 🔒🔥
- removed unsupported GitHub `permissions` blocks; Forgejo workflow auth now stays explicit, documented, and guarded by CI 🔒🔥
- release shipping stays idempotent across NuGet, Forgejo, and the GitHub mirror, all rooted in the exact tagged `main` commit 🚀🔥

## [0.2.4] - 2026-06-27

### dependencies — resilience pinned 🔒🔥

- pinned `Microsoft.Extensions.Http.Resilience` to `10.7.0` so restores stay deterministic while webhook delivery keeps its best-effort retry energy 🪝🔥

## [0.2.3] - 2026-02-23

### added — cleaner tooling 🗑️🔥

- added `dotnet litty clean` with litty-fied artifact deletion output 🔥
- moved the main build, test, pack, publish, and clean recipes onto `LittyLogs.Tool` for one consistent CLI vibe 🧰🔥
- replaced example `Console.WriteLine` scaffolding with structured `ILogger` dogfooding 🐕🔥

## [0.2.2] - 2026-02-22

### fixed — retryable release destinations 🔄🔥

- Forgejo and GitHub releases now reuse existing releases and refresh package assets when a workflow reruns 🔥
- kept NuGet pushes idempotent with `--skip-duplicate` so a flaky destination never requires a new version 🔒🔥

## [0.2.1] - 2026-02-22

### fixed — Matrix rendering 🟣🔥

- added an escaped HTML field that hookshot prefers, with `<br/>` message breaks and `<pre><code>` exception blocks 🔒🔥
- kept the text fallback with CommonMark paragraph breaks and literal UTF-8 emojis 🔥
- replaced broad HTML encoding with the five-character `HtmlEscape()` path so emojis render correctly 🟣🔥
- loaded the nearest `.env` in the webhook example so live hookshot mode works from any working directory 🔥

## [0.2.0] - 2026-02-22

### added — webhook era 🪝🔥

- added `LittyLogs.Webhooks` with Matrix hookshot support, `Channel<T>` batching, minimum-level filtering, and `IHttpClientFactory` resilience 🔥
- validated webhook URLs at registration and allowed only absolute `http` or `https` schemes 🔒🔥
- filtered HTTP-client logging categories to prevent recursion and accidental webhook-token exposure 🔒🔥
- added the self-contained live-or-mock webhook example 🧪🔥

### added — CLI pack + publish 📦🔥

- added `dotnet litty pack` and `dotnet litty publish` output rewriting with shared build fallback behavior 🔥

## [0.1.4] - 2026-02-20

### changed — mirror owns git sync 🪞🔥

- removed redundant manual GitHub ref pushes and waited for the Forgejo push mirror before creating a GitHub release 🔥
- kept the GitHub token scoped to release API work only 🔒🔥

## [0.1.3] - 2026-02-19

### added — release infrastructure 🏗️🔥

- made Forgejo, GitHub, and NuGet publishing retryable and prerelease-aware 🔄🔥
- passed changelog notes through environment data so shell command substitution cannot eat release prose 🔒🔥
- documented the runner dependencies and least-privilege tokens needed by all three destinations 📜🔥

## [0.1.0-dev] - 2026-02-19

### added — expansion pack preview 🧪🔥

- added structured JSON logging with literal UTF-8 emojis and the shared rewrite brain 🔥
- added async file logging with text/JSON output, size + time rotation, and gzip compression 📁🔥
- added configurable level-first or timestamp-first ordering across console, file, and xUnit output ⏰🔥
- expanded examples and CI packaging coverage for the growing project squad 🧪🔥

## [0.1.0] - 2026-02-18

### added — genesis era 🌅🔥

- shipped the core emoji console formatter, framework-message rewrites, short categories, ANSI colors, and ISO 8601 timestamps 🔥
- shipped `LittyLogs.Xunit` with `ITestOutputHelper` integration and typed logger helpers 🧪🔥
- shipped `LittyLogs.Tool` with litty-fied build and test output rewriting 🧰🔥
- added examples, tests, just recipes, and shell completions so the first drop arrived fully dressed 💅🔥
