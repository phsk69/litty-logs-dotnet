# litty-logs bash completions for `just example <tab>` 🔥
# source this file in your .bashrc:
#   source /path/to/litty-logs-dotnet/completions/just.bash

_just_litty() {
    local cur prev
    cur="${COMP_WORDS[COMP_CWORD]}"
    prev="${COMP_WORDS[COMP_CWORD-1]}"

    # complete example names when typing `just example <tab>`
    if [[ "$prev" == "example" ]]; then
        COMPREPLY=($(compgen -W "web hosted console xunit json filesink" -- "$cur"))
        return
    fi

    # fall through to default just completions if available
    if type -t _just &>/dev/null; then
        _just
    else
        # basic recipe completion from justfile
        local recipes
        recipes=$(just --summary 2>/dev/null)
        COMPREPLY=($(compgen -W "$recipes" -- "$cur"))
    fi
}

complete -F _just_litty just
