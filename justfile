# litty-logs — the most bussin logging library no cap 🔥

# keep MSBuild worker state isolated so every recipe cooks clean and deterministic 🔥
export MSBUILDDISABLENODEREUSE := "1"
export DOTNET_CLI_DO_NOT_USE_MSBUILD_SERVER := "1"

# build the whole solution — litty-fied output no cap 🏗️🔥
build *args:
    dotnet run --project src/LittyLogs.Tool -- build {{ args }}

# run all the tests — litty-fied output with detailed test results 🧪🔥
test *args:
    dotnet run --project src/LittyLogs.Tool -- test {{ args }}

# publish the solution — litty-fied output bestie 📤🔥
publish *args:
    dotnet run --project src/LittyLogs.Tool -- publish {{ args }}

# pack all NuGet packages — litty-fied output so the besties can install em 📦🔥
pack *args:
    dotnet run --project src/LittyLogs.Tool -- pack -c Release {{ args }}

# run an example — usage: just example web|hosted|console|xunit|json|filesink|webhooks [extra args] 🔥
example name *args:
    #!/usr/bin/env bash
    set -euo pipefail
    case "{{ name }}" in
        web)      dotnet run --project examples/LittyLogs.Example.WebApi -- {{ args }} ;;
        hosted)   dotnet run --project examples/LittyLogs.Example.HostedService -- {{ args }} ;;
        console)  dotnet run --project examples/LittyLogs.Example.Console -- {{ args }} ;;
        xunit)    dotnet run --project src/LittyLogs.Tool -- test --project examples/LittyLogs.Example.Xunit/LittyLogs.Example.Xunit.csproj {{ args }} ;;
        json)     dotnet run --project examples/LittyLogs.Example.Json -- {{ args }} ;;
        filesink) dotnet run --project examples/LittyLogs.Example.FileSink -- {{ args }} ;;
        webhooks) dotnet run --project examples/LittyLogs.Example.Webhooks -- {{ args }} ;;
        *)        echo "bruh '{{ name }}' aint valid — try web, hosted, console, xunit, json, filesink, or webhooks 💀🔥"; exit 1 ;;
    esac

# install shell completions for `just example <tab>` — works with zsh and bash 🔥
setup-completions:
    #!/usr/bin/env bash
    set -euo pipefail
    shell=$(basename "$SHELL")
    script_dir="{{ justfile_directory() }}/completions"
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
            echo "bruh '$shell' aint supported yet — only zsh and bash rn 💀🔥"
            exit 1
            ;;
    esac
    source_line="source \"${comp_file}\""
    if grep -qF "$comp_file" "$rc_file" 2>/dev/null; then
        echo "completions already installed in ${rc_file} bestie, youre good 💅🔥"
    else
        echo "$source_line" >> "$rc_file"
        echo "completions installed in ${rc_file} 🔥"
        echo "restart your shell or run: source ${rc_file} 🔥"
    fi

# yeet all build artifacts — litty-fied so you see what gets yeeted 🗑️🔥
clean *args:
    dotnet run --project src/LittyLogs.Tool -- clean {{ args }}

# preview the next strict SemVer without touching files or installing anything 🔍🔥
release-next:
    #!/usr/bin/env bash
    set -euo pipefail
    if ! command -v git-cliff >/dev/null 2>&1; then
        echo "bruh git-cliff is not on PATH — bring your managed install, this recipe never installs deps 💀🔥"
        exit 1
    fi
    git-cliff --config cliff.toml --bumped-version --unreleased

# preview the exact next release notes without touching files or installing anything 📜🔥
release-notes:
    #!/usr/bin/env bash
    set -euo pipefail
    if ! command -v git-cliff >/dev/null 2>&1; then
        echo "bruh git-cliff is not on PATH — bring your managed install, this recipe never installs deps 💀🔥"
        exit 1
    fi
    next=$(git-cliff --config cliff.toml --bumped-version --unreleased)
    if [ -z "$next" ]; then
        echo "no releasable commits are cooking right now bestie 🔍🔥"
        exit 0
    fi
    git-cliff --config cliff.toml --unreleased --tag "$next" --strip all
