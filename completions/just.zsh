# litty-logs zsh completions for `just example <tab>` 🔥
# source this file in your .zshrc:
#   source /path/to/litty-logs-dotnet/completions/just.zsh

_just_litty() {
    local examples=(web hosted console xunit json filesink)

    # only complete if we're typing `just example <arg>`
    if [[ "${words[2]}" == "example" && $CURRENT -eq 3 ]]; then
        _describe 'example' examples
        return
    fi

    # fall through to default just completions if available
    if (( $+functions[_just] )); then
        _just "$@"
    else
        _files
    fi
}

compdef _just_litty just
