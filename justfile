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

# slap a git tag on the current version 🏷️
tag:
    #!/usr/bin/env bash
    set -euo pipefail
    version=$(grep -oP '(?<=<Version>)[^<]+' Directory.Build.props)
    git tag -a "v${version}" -m "v${version} dropped no cap 🔥"
    echo "tagged v${version} — push with: git push origin v${version} 🏷️"

# the full release combo — bump + commit + tag (does NOT push, thats on you bestie) 🚀
# usage: just release patch (or minor, or major)
release part:
    #!/usr/bin/env bash
    set -euo pipefail
    echo "starting the release ritual bestie 🕯️"
    just bump {{part}}
    version=$(grep -oP '(?<=<Version>)[^<]+' Directory.Build.props)
    git add Directory.Build.props
    git commit -m "bump: v${version} incoming no cap 🔥"
    git tag -a "v${version}" -m "v${version} dropped no cap 🔥"
    echo ""
    echo "version bumped to ${version} and tagged as v${version} 🏷️"
    echo "now push it to trigger the release pipeline:"
    echo "  git push && git push origin v${version}"

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
