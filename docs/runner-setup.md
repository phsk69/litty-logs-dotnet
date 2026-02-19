# 🔥 forgejo runner setup — getting the CI/CD drip installed

this doc tells you how to set up the self-hosted forgejo runner so it can build, test, pack, and ship litty-logs to all three destinations: nuget.org, forgejo releases, and github releases no cap

## required software on the runner 🧰

install all of these or the pipeline will be bricked fr fr:

| tool | why | check command |
|------|-----|---------------|
| **.NET SDK 10.0** | builds/tests/packs the whole solution | `dotnet --version` → 10.0.x |
| **gh** (GitHub CLI) | creates releases on the github mirror | `gh --version` |
| **git** | obviously bestie | `git --version` |
| **bash** | workflow scripts use bash shebangs | `bash --version` |
| **grep** (with `-P`) | version extraction from XML | `grep -P --version` |
| **awk** | changelog section extraction | `awk --version` |
| **jq** | JSON-escapes changelog notes + parses forgejo API responses | `jq --version` |
| **curl** | forgejo release API calls | `curl --version` |

### install commands (debian/ubuntu)

```bash
# .NET SDK 10.0 — check https://dotnet.microsoft.com/download for latest
wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --channel 10.0

# gh CLI
sudo apt install gh

# the rest should already be there on any linux box no cap
```

## required forgejo secrets 🔐

go to your forgejo repo → Settings → Actions → Secrets and add these:

| secret name | what it does | where to get it |
|-------------|-------------|-----------------|
| `GITHUB_TOKEN` | forgejo release creation + .nupkg asset upload via Gitea API | **auto-provided** by forgejo actions — youre already good bestie 💅 |
| `GH_PAT` | github mirror release via `gh release create --repo phsk69/litty-logs-dotnet` | github.com → Settings → Developer Settings → Fine-grained Personal Access Tokens → generate for `phsk69/litty-logs-dotnet` with Contents read/write permission |
| `NUGET_API_KEY` | push .nupkg files to nuget.org | nuget.org → API Keys → Create → scope: Push, glob pattern: `LittyLogs*` |

### notes on the secrets

- **`GITHUB_TOKEN`** is auto-injected by forgejo actions into every workflow run. you dont need to create this manually, its just there. it handles creating the forgejo release and uploading .nupkg files as release assets 🏠
- **`GH_PAT`** should be a fine-grained token scoped to ONLY `phsk69/litty-logs-dotnet` with Contents read/write. dont give it more perms than it needs — principle of least privilege is bussin 🔒
- **`NUGET_API_KEY`** glob pattern `LittyLogs*` covers all four packages (LittyLogs, LittyLogs.Xunit, LittyLogs.File, LittyLogs.Tool). set an expiry and rotate it periodically bestie

## runner registration 🏃

the runner needs to be registered with your forgejo instance. the workflows use `runs-on: self-hosted`.

```bash
# download forgejo-runner (check forgejo docs for latest version)
# register it with your instance
forgejo-runner register \
  --instance https://git.dom.tld \
  --token YOUR_RUNNER_TOKEN \
  --name litty-runner \
  --labels self-hosted

# start it up
forgejo-runner daemon
```

check your forgejo repo → Settings → Actions → Runners to verify it shows up as online 🟢

## how the pipelines work 🔄

### CI (`ci.yml`)
- triggers on push/PR to `develop` and `main`
- builds → tests → packs (verify only, doesnt publish)
- if this fails your code is bricked and you should not merge no cap

### Release (`release.yml`)
- triggers when you push a `v*` tag (e.g. `v0.1.0`)
- the full pipeline hits three destinations:
  1. **build + test + pack** — sanity check, tag version must match Directory.Build.props
  2. **nuget.org** — pushes all four .nupkg files with `--skip-duplicate`
  3. **forgejo release** — creates a release on forgejo via Gitea API, uploads .nupkg assets
  4. **github release** — creates a release on the github mirror via `gh release create`, uploads .nupkg assets
- changelog section gets auto-extracted from `CHANGELOG.md` for release notes

### typical release flow
```bash
# on your dev machine (must have git flow CLI installed):
just release patch          # gitflow: branch, bump, commit, merge, tag, cleanup
git push origin develop main v0.1.1   # push everything to forgejo
# forgejo runner takes it from here — nuget + forgejo release + github release 🚀
```

### first release (version already set)
```bash
just release-current
git push origin develop main v0.1.0
```

### hotfix flow
```bash
just hotfix patch           # start hotfix branch, bump version on branch
# make your fix, commit it
just hotfix-finish          # git flow hotfix finish, merge, tag, cleanup
git push origin develop main v0.1.1
```

## troubleshooting 🔧

- **"fam your working tree is dirty"** — commit or stash your changes before running release/hotfix commands
- **"bruh the tag says X but Directory.Build.props says Y"** — version mismatch. make sure `just release` finished cleanly
- **nuget push fails with 403** — your `NUGET_API_KEY` is expired or doesnt have push scope. regenerate it on nuget.org
- **gh release fails** — check that `GH_PAT` has Contents read/write on `phsk69/litty-logs-dotnet` and the repo exists
- **forgejo release fails** — check the Gitea API response in the workflow logs. usually a token permissions issue
- **runner not picking up jobs** — check `forgejo-runner daemon` is running and the runner shows as online in forgejo settings
