#!/usr/bin/env bash
set -euo pipefail

mode=${1:?preview or tag required}
version=${2:?release version required}
root=$(cd "$(dirname "$0")/.." && pwd)
cliff=${CLIFF:-git-cliff}
tag="v${version}"
stable='^v(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)$'
release="${stable%\$}"'(-(0|[1-9][0-9]*|[0-9]*[A-Za-z-][0-9A-Za-z-]*)(\.(0|[1-9][0-9]*|[0-9]*[A-Za-z-][0-9A-Za-z-]*))*)?$'

# Only strict SemVer tags reachable from the requested revision are baselines.
previous_release() {
    local revision=$1 pattern=$2 candidate candidates
    local matches=()
    [ -n "$revision" ] || return 0
    candidates=$(git for-each-ref --merged "$revision" --format='%(refname:strip=2)' refs/tags)
    while IFS= read -r candidate; do
        if [[ "$candidate" =~ $pattern ]]; then
            matches+=("--match=$candidate")
        fi
    done <<< "$candidates"
    if [ "${#matches[@]}" -gt 0 ]; then
        git describe --tags --abbrev=0 "${matches[@]}" "$revision"
    fi
}

[[ "$tag" =~ $release ]] || { echo "Expected a strict SemVer release version" >&2; exit 2; }
target=HEAD
anchor=HEAD
args=()
case "$mode" in
    preview) args+=(--tag "$tag") ;;
    tag)
        [ "$(git rev-parse HEAD)" = "$(git rev-parse "refs/tags/${tag}^{commit}")" ] || {
            echo "Generate release notes from the exact tag checkout" >&2
            exit 1
        }
        target="refs/tags/$tag"
        anchor=$(git rev-parse --verify HEAD^ 2>/dev/null || true)
        ;;
    *) echo "Expected preview or tag" >&2; exit 2 ;;
esac

previous=$(previous_release "$anchor" "$release")
if [[ -n "$previous" && "$previous" == "$tag"-* && "$tag" =~ $stable ]]; then
    # A stable promotion collects the whole series since the last stable tag.
    previous=$(previous_release "$anchor" "$stable")
    args+=(--ignore-tags '^v[0-9]+\.[0-9]+\.[0-9]+-')
fi
if [ -n "$previous" ]; then
    args+=("${previous}..${target}")
fi

notes=$("$cliff" --config "$root/cliff.toml" --strip all "${args[@]}")
if grep -q '^- ' <<< "$notes"; then
    printf '%s\n' "$notes"
else
    printf '## [%s]\n\n### changed — manual release override 🎛️🔥\n\n- explicit maintenance release or prerelease promotion to %s 🔥\n' \
        "$version" "$version"
fi
