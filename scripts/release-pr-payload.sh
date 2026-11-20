#!/usr/bin/env bash
set -euo pipefail

mode=${1:?create or update required}
version=${2:?release version required}
notes=${3:?release notes file required}
case "$mode" in
    create | update) ;;
    *) echo "Expected create or update" >&2; exit 2 ;;
esac

jq -n --arg mode "$mode" --arg title "chore(release): v${version} 🔥" \
    --arg summary "v${version} is ready for review. Squash this into main to tag the signed version commit and ship the NuGet nuggies 🔥" \
    --rawfile notes "$notes" '
    {title: $title, body: ($summary + "\n\n" + $notes)}
    + (if $mode == "create" then {head: "release-pr", base: "main"} else {} end)
'
