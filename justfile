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

# pack all NuGet packages so the besties can install em (four packages now 📦)
pack:
    dotnet pack src/LittyLogs/LittyLogs.csproj -c Release
    dotnet pack src/LittyLogs.Xunit/LittyLogs.Xunit.csproj -c Release
    dotnet pack src/LittyLogs.Tool/LittyLogs.Tool.csproj -c Release
    dotnet pack src/LittyLogs.File/LittyLogs.File.csproj -c Release

# run an example — usage: just example web|hosted|console|xunit|json|filesink [extra args] 🔥
# extra args pass through to the underlying command (e.g. just example web --json)
example name *args:
    #!/usr/bin/env bash
    set -euo pipefail
    case "{{name}}" in
        web)      dotnet run --project examples/LittyLogs.Example.WebApi -- {{args}} ;;
        hosted)   dotnet run --project examples/LittyLogs.Example.HostedService -- {{args}} ;;
        console)  dotnet run --project examples/LittyLogs.Example.Console -- {{args}} ;;
        xunit)    dotnet test examples/LittyLogs.Example.Xunit --verbosity normal {{args}} ;;
        json)     dotnet run --project examples/LittyLogs.Example.Json -- {{args}} ;;
        filesink) dotnet run --project examples/LittyLogs.Example.FileSink -- {{args}} ;;
        *)        echo "bruh '{{name}}' aint a valid example — try: web, hosted, console, xunit, json, filesink 💀"; exit 1 ;;
    esac

# install shell completions for `just example <tab>` — works with zsh and bash 🔥
setup-completions:
    #!/usr/bin/env bash
    set -euo pipefail
    shell=$(basename "$SHELL")
    script_dir="{{justfile_directory()}}/completions"
    case "$shell" in
        zsh)
            comp_file="${script_dir}/just.zsh"
            rc_file="$HOME/.zshrc"
            ;;
        bash)
            comp_file="${script_dir}/just.bash"
            rc_file="$HOME/.bashrc"
            ;;
        *)
            echo "bruh '$shell' aint supported yet — only zsh and bash rn 💀"
            exit 1
            ;;
    esac
    source_line="source \"${comp_file}\""
    if grep -qF "$comp_file" "$rc_file" 2>/dev/null; then
        echo "completions already installed in ${rc_file} bestie, youre good 💅"
    else
        echo "$source_line" >> "$rc_file"
        echo "completions installed in ${rc_file} 🔥"
        echo "restart your shell or run: source ${rc_file}"
    fi

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

# gitflow release — start branch clean, bump on the branch, finish 🚀
# usage: just release patch (or minor, or major)
release part:
    #!/usr/bin/env bash
    set -euo pipefail
    if [ -n "$(git status --porcelain)" ]; then
        echo "fam your working tree is dirty, commit or stash first no cap 😤"
        exit 1
    fi
    props="Directory.Build.props"
    current=$(grep -oP '(?<=<Version>)[^<]+' "$props")
    base="${current%%-*}"
    IFS='.' read -r major minor patch <<< "$base"
    case "{{part}}" in
        major) major=$((major + 1)); minor=0; patch=0 ;;
        minor) minor=$((minor + 1)); patch=0 ;;
        patch) patch=$((patch + 1)) ;;
        *) echo "fam thats not a valid bump part — use major, minor, or patch no cap 😤"; exit 1 ;;
    esac
    new_version="${major}.${minor}.${patch}"
    echo "starting the gitflow release ritual bestie 🕯️"
    echo "  ${current} -> ${new_version}"
    echo ""
    git flow release start "v${new_version}"
    sed -i "s|<Version>${current}</Version>|<Version>${new_version}</Version>|" "$props"
    git add "$props"
    git commit -m "bump: v${new_version} incoming no cap 🔥"
    GIT_MERGE_AUTOEDIT=no git flow release finish "v${new_version}" -m "v${new_version} dropped no cap 🔥"
    echo ""
    echo "=========================================="
    echo "  release v${new_version} complete 🔥"
    echo "=========================================="
    echo ""
    echo "pushing develop, main, and tag to origin 📤"
    git push origin develop main "v${new_version}"
    echo "everything is pushed — pipeline go brrr 🚀🔥"

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
    git flow release start "v${version}"
    GIT_MERGE_AUTOEDIT=no git flow release finish "v${version}" -m "v${version} dropped no cap 🔥"
    echo ""
    echo "=========================================="
    echo "  release v${version} complete 🔥"
    echo "=========================================="
    echo ""
    echo "pushing develop, main, and tag to origin 📤"
    git push origin develop main "v${version}"
    echo "everything is pushed — pipeline go brrr 🚀🔥"

# dev/pre-release — bump + slap a label on it and ship the whole thing 🧪
# usage: just release-dev patch [label] — label defaults to "dev"
# examples: just release-dev patch → 0.1.0 becomes 0.1.1-dev
#           just release-dev minor beta.1 → 0.1.0 becomes 0.2.0-beta.1
release-dev part label="dev":
    #!/usr/bin/env bash
    set -euo pipefail
    if [ -n "$(git status --porcelain)" ]; then
        echo "fam your working tree is dirty, commit or stash first no cap 😤"
        exit 1
    fi
    props="Directory.Build.props"
    current=$(grep -oP '(?<=<Version>)[^<]+' "$props")
    base="${current%%-*}"
    IFS='.' read -r major minor patch <<< "$base"
    case "{{part}}" in
        major) major=$((major + 1)); minor=0; patch=0 ;;
        minor) minor=$((minor + 1)); patch=0 ;;
        patch) patch=$((patch + 1)) ;;
        *) echo "fam thats not a valid bump part — use major, minor, or patch no cap 😤"; exit 1 ;;
    esac
    new_version="${major}.${minor}.${patch}-{{label}}"
    echo "starting dev release bestie 🧪"
    echo "  ${current} -> ${new_version}"
    echo ""
    git flow release start "v${new_version}"
    sed -i "s|<Version>${current}</Version>|<Version>${new_version}</Version>|" "$props"
    git add "$props"
    git commit -m "bump: v${new_version} dev release incoming 🧪"
    GIT_MERGE_AUTOEDIT=no git flow release finish "v${new_version}" -m "v${new_version} dropped no cap 🔥"
    echo ""
    echo "=========================================="
    echo "  dev release v${new_version} complete 🧪🔥"
    echo "=========================================="
    echo ""
    echo "pushing develop, main, and tag to origin 📤"
    git push origin develop main "v${new_version}"
    echo "everything is pushed — pipeline go brrr 🚀🔥"

# start a hotfix — for when something is bricked in prod 🚑
# usage: just hotfix patch (or minor, or major)
hotfix part:
    #!/usr/bin/env bash
    set -euo pipefail
    if [ -n "$(git status --porcelain)" ]; then
        echo "fam your working tree is dirty, commit or stash first no cap 😤"
        exit 1
    fi
    props="Directory.Build.props"
    current=$(grep -oP '(?<=<Version>)[^<]+' "$props")
    base="${current%%-*}"
    IFS='.' read -r major minor patch <<< "$base"
    case "{{part}}" in
        major) major=$((major + 1)); minor=0; patch=0 ;;
        minor) minor=$((minor + 1)); patch=0 ;;
        patch) patch=$((patch + 1)) ;;
        *) echo "fam thats not a valid bump part — use major, minor, or patch no cap 😤"; exit 1 ;;
    esac
    new_version="${major}.${minor}.${patch}"
    echo "starting hotfix — something in prod is not bussin 🚑"
    echo "  ${current} -> ${new_version}"
    git flow hotfix start "v${new_version}"
    sed -i "s|<Version>${current}</Version>|<Version>${new_version}</Version>|" "$props"
    git add "$props"
    git commit -m "bump: v${new_version} hotfix incoming 🚑"
    echo ""
    echo "hotfix/v${new_version} branch created and version bumped 🔥"
    echo "now make your fix, commit it, then run:"
    echo "  just finish"

# finish whatever gitflow branch youre on — hotfix, release, or support 🏁
# auto-detects the branch type, finishes it, and pushes everything no cap
finish:
    #!/usr/bin/env bash
    set -euo pipefail
    branch=$(git rev-parse --abbrev-ref HEAD)
    if [ -n "$(git status --porcelain)" ]; then
        echo "fam your working tree is dirty, commit or stash first no cap 😤"
        exit 1
    fi
    if [[ "$branch" == hotfix/* ]]; then
        version="${branch#hotfix/}"
        kind="hotfix"
        emoji="🚑"
    elif [[ "$branch" == release/* ]]; then
        version="${branch#release/}"
        kind="release"
        emoji="🚀"
    elif [[ "$branch" == support/* ]]; then
        version="${branch#support/}"
        kind="support"
        emoji="🛠️"
    else
        echo "bruh youre on '${branch}' — thats not a hotfix, release, or support branch 💀"
        echo "get on the right branch first bestie"
        exit 1
    fi
    # strip leading v if present so we dont double up
    version_clean="${version#v}"
    echo "finishing ${kind} v${version_clean} ${emoji}🏁"
    GIT_MERGE_AUTOEDIT=no git flow "${kind}" finish "${version}" -m "v${version_clean} ${kind} dropped no cap 🔥"
    echo ""
    echo "=========================================="
    echo "  v${version_clean} complete ${emoji}🔥"
    echo "=========================================="
    echo ""
    echo "pushing develop, main, and tag to origin 📤"
    git push origin develop main "${version}"
    echo ""
    echo "everything is pushed — pipeline go brrr 🚀🔥"

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
