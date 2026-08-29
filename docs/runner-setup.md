# 🔥 forgejo runner + trunk release setup

this repo has one trunk, one rolling Release PR, immutable tags, and five NuGet nuggies shipping to three destinations. source branches can vanish after squash merge because git-cliff reads the tagged `main` graph, not old branch refs no cap 🌳🔥

## runner software 🧰🔥

the `linux` runner image needs these already available:

| tool | why its invited 🔥 | vibe check |
|---|---|---|
| .NET SDK 10.0 | build, test, and pack all five projects | `dotnet --version` |
| git | full tag + commit graph and release branch pushes | `git --version` |
| bash | workflow scripts | `bash --version` |
| curl + tar + sha256sum | install-action fetches and verifies temporary git-cliff | `curl --version` |
| grep with `-P` + awk | version and changelog transforms | `grep -P --version` |
| jq | safe Forgejo API payloads | `jq --version` |
| gh | GitHub mirror releases | `gh --version` |

git-cliff is intentionally not preinstalled on the runner. `https://github.com/taiki-e/install-action@v2` resolves the newest stable `git-cliff@2` with its short dependency cooldown, verifies the upstream checksum, and exposes it only for that job. the fully qualified URL keeps Forgejo from rewriting the action to its default mirror, `fallback: none` forbids Cargo/binstall/source fallbacks, and an explicit version assertion rejects any accidental `3.x` jump. the action tag itself is visible to Renovate, so future action majors stay reviewable instead of fossilizing in shell strings 🔒🔥

workflow syntax follows Forgejo's own [Actions reference](https://forgejo.org/docs/latest/user/actions/reference/), [GitHub Actions differences](https://forgejo.org/docs/latest/user/actions/github-actions/), and [runner troubleshooting guide](https://forgejo.org/docs/latest/user/actions/troubleshooting/). GitHub syntax is never assumed compatible: remote actions use explicit URLs, and `bash scripts/test-forgejo-workflows.sh` rejects keys Forgejo documents as ignored 🔍🔒🔥

## required secrets 🔐🔥

Forgejo repo → Settings → Actions → Secrets needs:

| secret | exact job 🔥 |
|---|---|
| `RELEASE_TOKEN` | Forgejo PAT restricted to `public/litty-logs-dotnet` with only `write:repository`; checkout uses it so tag and `release-pr` pushes can trigger downstream workflows |
| `NUGET_API_KEY` | nuget.org push key scoped to the `LittyLogs*` package glob; set expiry + rotation |
| `GH_PAT` | fine-grained GitHub token scoped only to `phsk69/litty-logs-dotnet`, Contents read/write, used for mirror release API calls |
| `GITHUB_TOKEN` | auto-provided by Forgejo Actions for Forgejo release creation and asset uploads |

`RELEASE_TOKEN` cannot be swapped for the workflow’s automatic token: Forgejo explicitly prevents automatic-token pushes from waking another workflow, which would leave a valid tag chilling without its ship job. least privilege still wins — keep every token pinned to this repo and only its stated job. Forgejo ignores GitHub-style `permissions` blocks, so none belong in these workflows; use an Authorized Integration when a future job needs native short-lived capabilities beyond this recursion-preserving release PAT 🔒🔥

## Forgejo repo settings 🌳🔒🔥

protect `main` with these rules:

1. Require pull requests and green `commitlint`, `release-policy`, and `build-and-test` checks.
2. Allow squash merge only so the PR title becomes the single conventional commit that drives SemVer.
3. Enable automatic source-branch deletion after merge; the rolling `release-pr` branch is recreated whenever needed.
4. Block direct human pushes while allowing the scoped release identity to push the automation branch and create tags.

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
- `release-policy` proves fix/perf/revert/dependency chores are patch, features are minor, breaking changes are major even below 1.0, noise-only commits are silent, and deleted source branches change nothing.
- `build-and-test` builds, tests, and packs without publishing.

### rolling Release PR — `.forgejo/workflows/release-pr.yml`

- every push to `main` runs the newest checksum-verified stable git-cliff `2.x` against commits after the newest matching tag.
- `feat!` is major; `feat` is minor; `fix`, `perf`, `revert`, and `chore(deps)` are patch; generic maintenance commits are skipped.
- releasable work force-refreshes the single `release-pr` branch and opens or updates `chore(release): vX.Y.Z 🔥`.
- merging that PR changes `Directory.Build.props`; the next run creates one annotated tag on that exact `main` commit.
- if that first run fails before creating the tag, the next green `main` commit may recover the still-unused version when its changelog section exists and the last tagged baseline has a different version. the recovered tag lands on the fixed head, folding pre-tag repairs into the intended release instead of shipping known-broken automation 🔧🔥
- an existing tag at that same commit is a retry-safe no-op. an existing tag anywhere else is a hard failure and is never moved or deleted.
- manual dispatch can request `auto`, `patch`, `minor`, `major`, or `promote`; dispatch only makes a PR and never tags its starting commit.
- while the one-off RC is current, automatic bumping pauses instead of inventing `rc.2`; `promote` is the explicit gate to stable and also includes any emergency conventional commits landed during the freeze 🔒🔥

### ship job — `.forgejo/workflows/release.yml`

the immutable `v*` tag wakes one serialized job that:

1. proves the tagged commit is on `main` and the tag matches strict SemVer + `Directory.Build.props`.
2. builds, tests, and packs all five NuGet packages.
3. pushes to nuget.org with `--skip-duplicate`.
4. creates or refreshes the Forgejo release and package assets.
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
