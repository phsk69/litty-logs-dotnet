#!/usr/bin/env bash
set -euo pipefail

version=2.14.2
sha256=24f397c733add5390fdceee3a2088588ab0d5f944ce00d34cb7029b888cf2db4
directory="${RUNNER_TEMP:?runner temp directory required}/litty-tools"
mkdir -p "$directory"
archive=$(mktemp "$directory/git-cliff.XXXXXXXX")
trap 'rm -f "$archive"' EXIT
[[ "$(uname -m)" == x86_64 ]]
curl --fail --silent --show-error --location --proto '=https' --proto-redir '=https' \
    --retry 4 --connect-timeout 15 --max-time 180 \
    --output "$archive" \
    "https://github.com/orhun/git-cliff/releases/download/v${version}/git-cliff-${version}-x86_64-unknown-linux-gnu.tar.gz"
printf '%s  %s\n' "$sha256" "$archive" | sha256sum --check --status
tar -xOzf "$archive" "git-cliff-${version}/git-cliff" > "$directory/git-cliff"
chmod 755 "$directory/git-cliff"
[[ "$("$directory/git-cliff" --version)" == "git-cliff ${version}" ]]
printf '%s\n' "$directory" >> "${GITHUB_PATH:?Actions path file required}"
