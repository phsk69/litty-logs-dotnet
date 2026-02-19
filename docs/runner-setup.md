# 🔥 forgejo runner setup — getting the CI/CD drip installed

this doc tells you how to set up the self-hosted forgejo runner so it can build, test, pack, and ship litty-logs to nuget.org and github releases no cap

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

go to your forgejo repo → settings → secrets/variables → actions and add these:

| secret name | what it is | where to get it |
|-------------|-----------|-----------------|
| `NUGET_API_KEY` | NuGet.org API key for publishing packages | nuget.org → API Keys → Create |
| `GH_PAT` | GitHub Personal Access Token with `repo` scope | github.com → Settings → Developer Settings → Personal Access Tokens |

the `GH_PAT` needs `repo` scope so it can create releases and upload assets on the github mirror. don't give it more perms than it needs bestie — principle of least privilege is bussin 🔒

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

check your forgejo repo → settings → actions → runners to verify it shows up as online 🟢

## how the pipelines work 🔄

### CI (`ci.yml`)
- triggers on push/PR to `develop` and `main`
- builds → tests → packs (verify only, doesnt publish)
- if this fails your code is bricked and you should not merge no cap

### Release (`release.yml`)
- triggers when you push a `v*` tag (e.g. `v0.1.0`)
- builds → tests → packs → pushes to nuget.org → creates github release
- the tag version MUST match `Directory.Build.props` or it fails (sanity check)

### typical release flow
```bash
# on your dev machine:
just release patch          # bumps version, commits, tags
git push && git push origin v0.1.1   # push code + tag to forgejo
# forgejo runner takes it from here 🚀
```

## troubleshooting 🔧

- **"bruh the tag says X but Directory.Build.props says Y"** — you tagged without bumping first. use `just release` which does both, or manually `just bump` then `just tag`
- **nuget push fails with 403** — your `NUGET_API_KEY` is expired or doesnt have push scope. regenerate it on nuget.org
- **gh release fails** — check that `GH_PAT` has `repo` scope and the github mirror repo exists at `phsk69/litty-logs-dotnet`
- **runner not picking up jobs** — check `forgejo-runner daemon` is running and the runner shows as online in forgejo settings
