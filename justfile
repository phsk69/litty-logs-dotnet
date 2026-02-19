# litty-logs — the most bussin logging library no cap 🔥

# build the whole solution
build:
    dotnet build

# run all the tests bestie (verbosity normal so litty output shows up)
test:
    dotnet test --verbosity normal

# run tests through the litty tool for maximum bussin output
litty-test:
    dotnet run --project src/LittyLogs.Tool -- test

# build through the litty tool for that gen alpha build output
litty-build:
    dotnet run --project src/LittyLogs.Tool -- build

# pack all NuGet packages so the besties can install em
pack:
    dotnet pack src/LittyLogs/LittyLogs.csproj -c Release
    dotnet pack src/LittyLogs.Xunit/LittyLogs.Xunit.csproj -c Release
    dotnet pack src/LittyLogs.Tool/LittyLogs.Tool.csproj -c Release

# run the web api example to flex
example-web:
    dotnet run --project examples/LittyLogs.Example.WebApi

# run the hosted service example (background vibe checker)
example-hosted:
    dotnet run --project examples/LittyLogs.Example.HostedService

# run the console example (the ten-liner speedrun)
example-console:
    dotnet run --project examples/LittyLogs.Example.Console

# run the xunit example tests to see litty-fied test output
example-xunit:
    dotnet test examples/LittyLogs.Example.Xunit --verbosity normal

# yeet all build artifacts
clean:
    dotnet clean

# bump the version bestie — usage: just bump major|minor|patch 🔥
bump part:
    #!/usr/bin/env bash
    set -euo pipefail
    props="Directory.Build.props"
    current=$(grep -oP '(?<=<Version>)[^<]+' "$props")
    if [ -z "$current" ]; then
        echo "bruh cant find <Version> in $props thats not bussin 💀"
        exit 1
    fi
    # strip any pre-release suffix before bumping — we only bump the core semver
    base="${current%%-*}"
    IFS='.' read -r major minor patch <<< "$base"
    case "{{part}}" in
        major) major=$((major + 1)); minor=0; patch=0 ;;
        minor) minor=$((minor + 1)); patch=0 ;;
        patch) patch=$((patch + 1)) ;;
        *) echo "fam thats not a valid bump part — use major, minor, or patch no cap 😤"; exit 1 ;;
    esac
    new_version="${major}.${minor}.${patch}"
    sed -i "s|<Version>${current}</Version>|<Version>${new_version}</Version>|" "$props"
    echo "version went from ${current} -> ${new_version} lets gooo 🔥"

# slap a pre-release label on the current version — usage: just bump-pre dev.1 🧪
bump-pre label:
    #!/usr/bin/env bash
    set -euo pipefail
    props="Directory.Build.props"
    current=$(grep -oP '(?<=<Version>)[^<]+' "$props")
    if [ -z "$current" ]; then
        echo "bruh cant find <Version> in $props thats not bussin 💀"
        exit 1
    fi
    # strip any existing pre-release suffix and add the new one
    base="${current%%-*}"
    new_version="${base}-{{label}}"
    sed -i "s|<Version>${current}</Version>|<Version>${new_version}</Version>|" "$props"
    echo "version went from ${current} -> ${new_version} (pre-release mode activated) 🧪"

# gitflow release — bump + git flow release start/finish 🚀
# usage: just release patch (or minor, or major)
release part:
    #!/usr/bin/env bash
    set -euo pipefail
    if [ -n "$(git status --porcelain)" ]; then
        echo "fam your working tree is dirty, commit or stash first no cap 😤"
        exit 1
    fi
    echo "starting the gitflow release ritual bestie 🕯️"
    echo ""
    just bump {{part}}
    version=$(grep -oP '(?<=<Version>)[^<]+' Directory.Build.props)
    git flow release start "${version}"
    git add Directory.Build.props
    git commit -m "bump: v${version} incoming no cap 🔥"
    GIT_MERGE_AUTOEDIT=no git flow release finish "${version}" -m "v${version} dropped no cap 🔥"
    echo ""
    echo "=========================================="
    echo "  gitflow release v${version} complete 🔥"
    echo "=========================================="
    echo ""
    echo "now push everything to trigger the release pipeline:"
    echo "  git push origin develop main v${version}"

# release the current version as-is without bumping 🚀
# for when Directory.Build.props already has the version you want (e.g. first release)
release-current:
    #!/usr/bin/env bash
    set -euo pipefail
    if [ -n "$(git status --porcelain)" ]; then
        echo "fam your working tree is dirty, commit or stash first no cap 😤"
        exit 1
    fi
    version=$(grep -oP '(?<=<Version>)[^<]+' Directory.Build.props)
    echo "releasing v${version} as-is bestie 🕯️"
    echo ""
    git flow release start "${version}"
    GIT_MERGE_AUTOEDIT=no git flow release finish "${version}" -m "v${version} dropped no cap 🔥"
    echo ""
    echo "=========================================="
    echo "  gitflow release v${version} complete 🔥"
    echo "=========================================="
    echo ""
    echo "now push everything to trigger the release pipeline:"
    echo "  git push origin develop main v${version}"

# start a hotfix — for when something is bricked in prod 🚑
# usage: just hotfix patch (or minor, or major)
hotfix part:
    #!/usr/bin/env bash
    set -euo pipefail
    if [ -n "$(git status --porcelain)" ]; then
        echo "fam your working tree is dirty, commit or stash first no cap 😤"
        exit 1
    fi
    echo "starting hotfix — something in prod is not bussin 🚑"
    just bump {{part}}
    version=$(grep -oP '(?<=<Version>)[^<]+' Directory.Build.props)
    git flow hotfix start "${version}"
    git add Directory.Build.props
    git commit -m "bump: v${version} hotfix incoming 🚑"
    echo ""
    echo "hotfix/${version} branch created and version bumped 🔥"
    echo "now make your fix, commit it, then run:"
    echo "  just hotfix-finish"

# finish a hotfix — git flow handles merge to main, tag, back-merge to develop 🏁
hotfix-finish:
    #!/usr/bin/env bash
    set -euo pipefail
    branch=$(git rev-parse --abbrev-ref HEAD)
    if [[ "$branch" != hotfix/* ]]; then
        echo "bruh youre not on a hotfix branch — youre on '${branch}' rn 💀"
        exit 1
    fi
    if [ -n "$(git status --porcelain)" ]; then
        echo "fam your working tree is dirty, commit or stash first no cap 😤"
        exit 1
    fi
    version="${branch#hotfix/}"
    echo "finishing hotfix v${version} 🏁"
    GIT_MERGE_AUTOEDIT=no git flow hotfix finish "${version}" -m "v${version} hotfix dropped no cap 🔥"
    echo ""
    echo "=========================================="
    echo "  hotfix v${version} complete 🚑🔥"
    echo "=========================================="
    echo ""
    echo "now push everything:"
    echo "  git push origin develop main v${version}"

# manually yeet packages to nuget.org — for local dev releases / testing 📤
nuget-push:
    #!/usr/bin/env bash
    set -euo pipefail
    if [ -z "${NUGET_API_KEY:-}" ]; then
        echo "bruh set NUGET_API_KEY env var first thats kinda important 💀"
        exit 1
    fi
    echo "packing the goods 📦"
    dotnet pack --configuration Release --output ./nupkgs
    for pkg in ./nupkgs/*.nupkg; do
        echo "pushing ${pkg} to nuget.org no cap 📤"
        dotnet nuget push "$pkg" \
            --api-key "$NUGET_API_KEY" \
            --source https://api.nuget.org/v3/index.json \
            --skip-duplicate
    done
    echo "all packages are on nuget.org now bestie 🔥"
