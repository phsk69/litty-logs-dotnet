# 🔥 forgejo runner + trunk release setup

this repo has one trunk, one rolling Release PR, immutable tags, and five NuGet nuggies shipping to three destinations. source branches can vanish after squash merge because git-cliff reads the tagged `main` graph, not old branch refs no cap 🌳🔥

## runner software 🧰🔥

the `linux` runner image needs these already available:

| tool | why its invited 🔥 | vibe check |
|---|---|---|
| Podman with rootless user namespaces | run the upstream Microsoft .NET SDK images for builds, tests, packaging and NuGet upload | `podman info` |
| Python 3.11+ | run the SDK and package checks and verify NuGet artifacts | `python3 --version` |
| git | full tag + commit graph and release branch pushes | `git --version` |
| OpenSSH `ssh-keygen` | SSH commit/tag signing plus trust verification | `ssh -V` |
| bash | workflow scripts | `bash --version` |
| curl + tar + sha256sum | fetch and verify the pinned git-cliff binary | `curl --version` |
| grep with `-P` + awk | version and signer checks | `grep -P --version` |
| jq | safe Forgejo API payloads | `jq --version` |
| gh | GitHub mirror releases | `gh --version` |

each release-policy, release-PR, and publication job installs the same git-cliff `2.14.2` build with `scripts/install-git-cliff.sh`. the script verifies the pinned archive checksum before exposing the binary on that job's PATH 🔒🔥

workflow syntax follows Forgejo's own [Actions reference](https://forgejo.org/docs/latest/user/actions/reference/), [GitHub Actions differences](https://forgejo.org/docs/latest/user/actions/github-actions/), and [runner troubleshooting guide](https://forgejo.org/docs/latest/user/actions/troubleshooting/). GitHub syntax is never assumed compatible: remote actions use explicit URLs, and `bash scripts/test-forgejo-workflows.sh` rejects keys Forgejo documents as ignored 🔍🔒🔥

## required secrets 🔐🔥

Forgejo repo → Settings → Actions → Secrets needs:

| secret | exact job 🔥 |
|---|---|
| `RELEASE_TOKEN` | restricted `litty-logs-release-bot` PAT scoped to `public/litty-logs-dotnet` with only `write:repository`; checkout uses it so tag and `release-pr` pushes can trigger downstream workflows |
| `AUTOMATION_SSH_SIGNING_KEY` | dedicated Ed25519 release key from `Automation/litty-logs-dotnet-release-signing`; signs release commits and immutable tags |
| `NUGET_API_KEY` | nuget.org push key scoped to the `LittyLogs*` package glob; set expiry + rotation |
| `GH_PAT` | fine-grained GitHub token scoped only to `phsk69/litty-logs-dotnet`, Contents read/write, used for mirror release API calls |
| `GITHUB_TOKEN` | auto-provided by Forgejo Actions for Forgejo release creation and asset uploads |

`RELEASE_TOKEN` cannot be swapped for the workflow’s automatic token: Forgejo explicitly prevents automatic-token pushes from waking another workflow, which would leave a valid tag chilling without its ship job. least privilege still wins — keep every token pinned to this repo and only its stated job. Forgejo ignores GitHub-style `permissions` blocks, so none belong in these workflows; use an Authorized Integration when a future job needs native short-lived capabilities beyond this recursion-preserving release PAT 🔒🔥

## Forgejo repo settings 🌳🔒🔥

protect `main` with these rules:

1. Require pull requests and these exact green status contexts: `ci / commitlint (pull_request)`, `ci / signing-policy (pull_request)`, `ci / release-policy (pull_request)`, and `ci / build-and-test (pull_request)`. Forgejo matches the full reported context literally, so the shorter job IDs do not enforce the gate no cap 🔒🔥
2. Allow squash merge only so the PR title becomes the single conventional commit that drives SemVer.
3. Enable automatic source-branch deletion after merge; the rolling `release-pr` branch is recreated whenever needed.
4. Block direct human pushes while allowing the scoped release identity to push the automation branch and create tags.
5. Require signed commits and apply the protection to administrators too; the exact trust model lives in [commit-signing.md](commit-signing.md) 🔏🔥

protect the `v*` tag pattern too: allow the release identity to create a new tag, but deny deletion, force updates, or retargeting for everyone. a published version is immutable forever no cap 🔒🔥

keep the existing Forgejo → GitHub push mirror enabled for branches and tags. the release workflow waits for the mirrored tag before creating or refreshing the GitHub release 🪞🔥

## runner registration 🏃🔥

```bash
forgejo-runner register \
  --instance https://git.dom.tld \
  --token YOUR_RUNNER_TOKEN \
  --name litty-runner \
  --labels linux

forgejo-runner daemon
```

verify the runner is green under Forgejo repo → Settings → Actions → Runners before merging release-shaped work 🟢🔥

## what each pipeline cooks 🧠🔥

### CI — `.forgejo/workflows/ci.yml`

- `commitlint` requires a conventional PR title ending in 🔥; squash merge turns that title into release input.
- `signing-policy` verifies every commit after the grandfathered rollout cutoff against the repository SSH allowlist.
- `release-policy` proves fix/perf/revert/dependency chores are patch, features are minor, breaking changes are major even below 1.0, noise-only commits are silent, and deleted source branches change nothing.
- `build-and-test` pulls the stable Microsoft SDK image for each supported .NET channel and uses that image's ID throughout the job. The .NET CLI and Microsoft.Testing.Platform opt out of telemetry. Builds, tests, packing, and smoke-test execution run with container networking disabled. Image pulls and NuGet restores and installs use the network. Tag publication verifies the artifacts from that run and uses the newest supported SDK image to upload them to NuGet.

### rolling Release PR — `.forgejo/workflows/release-pr.yml`

- the signing job verifies the source graph before the release job receives credentials. pinned git-cliff reads commits after the latest reachable release tag.
- shipped breaking changes are major, features are minor, and fixes and shipped dependency updates are patch. CI, docs, standalone tests, and tooling stay release-silent 🔍🔥
- releasable work refreshes the single `release-pr` branch and puts the generated Markdown in the Release PR description.
- merging that PR changes `Directory.Build.props`; the next run creates one annotated tag on that exact `main` commit.
- the release bot signs and locally verifies both its rolling release commit and immutable tag before either is pushed.
- if tagging fails, rerun from current `main` to recover the signed first-parent commit that introduced the version. the retry keeps that exact release commit 🔧🔥
- an existing signed tag on the same commit is a retry-safe no-op. a version collision fails the workflow.
- manual dispatch can request `auto`, `patch`, `minor`, `major`, or `promote` from current `main`.
- while the one-off RC is current, automatic bumping pauses instead of inventing `rc.2`; `promote` is the explicit gate to stable and also includes any emergency conventional commits landed during the freeze 🔒🔥

### ship job — `.forgejo/workflows/release.yml`

the immutable `v*` tag wakes one serialized job that:

1. verifies the trusted SSH tag signature, proves the tagged commit is on `main`, and checks strict SemVer + `Directory.Build.props`.
2. builds, tests, and packs all five NuGet packages.
3. pushes to nuget.org with `--skip-duplicate`.
4. creates or refreshes the Forgejo release and package assets using notes rendered from the exact tag.
5. waits for the GitHub mirror tag, then creates or refreshes the GitHub release and assets.

retry the same workflow/tag whenever a destination flakes. do not create the same tag again and never delete it; the three publishers are idempotent on the original version 🔄🔒🔥

## first 1.0 rollout 🧪🔥

the adoption PR carries stable `1.0.0` because the Slack and Matrix sinks have both passed live channel smoke tests before commit. dependency versions stay unchanged in this release; Renovate gets clean follow-up PRs with independent CI instead of hiding upgrades inside the breaking API release 🤖🔒🔥

squash the adoption PR with the breaking title `feat(webhooks)!: ship Slack blocks and trunk releases 🔥`. the resulting `main` commit changes `Directory.Build.props` from `0.2.4` to `1.0.0`. if pre-tag automation needs a repair, merge that repair first; recovery then creates immutable `v1.0.0` on the fixed `main` head and ships the stable NuGet packages directly 🚀🔥

## troubleshooting without cursed tag surgery 🔧🔥

- **no Release PR appears** — the squash type may be non-releasable; run `just release-next` with your managed git-cliff install to preview read-only.
- **`RELEASE_TOKEN` missing or denied** — create a PAT restricted to `public/litty-logs-dotnet` with only `write:repository`, then update the Actions secret.
- **Forgejo warns about `permissions`** — remove the unsupported GitHub key; this repo's CI guard rejects it, and Forgejo Authorized Integrations are the native choice when a job needs extra short-lived capabilities.
- **checksum verification fails** — stop. do not disable verification or enable a fallback; let install-action's manifest catch up to the upstream release.
- **tag already points elsewhere** — do not delete or move it. choose the next unused version through a forced dispatch.
- **tag matches but a publisher failed** — rerun `release.yml` for the same tag; NuGet and both release destinations reuse the version safely.
- **NuGet 403** — rotate `NUGET_API_KEY` and keep the `LittyLogs*` push scope.
- **GitHub release cannot see the tag** — confirm the Forgejo push mirror is healthy and `GH_PAT` is scoped to the mirror repo.
- **runner stays asleep** — confirm `forgejo-runner daemon` and the `linux` label are online.
- **signature verification fails** — compare the signer fingerprint with `docs/commit-signing.md`; never weaken the allowlist or bypass `git verify-tag` to force a release.
