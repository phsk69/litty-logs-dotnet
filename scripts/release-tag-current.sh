#!/usr/bin/env bash
set -euo pipefail

event_name=${1:?event name is required}
current=${2:?current version is required}
expected_sha=${3:?expected main SHA is required}

case "$event_name" in
    push | workflow_dispatch) ;;
    *) echo "continue"; exit 0 ;;
esac

version_at() {
    git show "$1:Directory.Build.props" 2>/dev/null |
        sed -n 's/^[[:space:]]*<Version>\([^<]*\)<\/Version>[[:space:]]*$/\1/p'
}

actual_sha=$(git rev-parse HEAD)
if [ "$actual_sha" != "$expected_sha" ] || [ "$(version_at HEAD)" != "$current" ]; then
    echo "Release tagging requires the expected main checkout and version" >&2
    exit 1
fi

tag="v${current}"
existing=$(git rev-parse --verify "refs/tags/${tag}^{commit}" 2>/dev/null || true)
if [ -n "$existing" ]; then
    if [ "$existing" = "$actual_sha" ]; then
        git verify-tag "$tag"
        if [ "$event_name" = "push" ]; then
            echo "tagged"
        else
            echo "continue"
        fi
    elif [ "$(version_at HEAD^ || true)" != "$current" ]; then
        echo "${tag} is already pinned to ${existing}" >&2
        exit 1
    else
        echo "continue"
    fi
    exit 0
fi

# The current version must originate in a signed Release MR on main's first-parent line.
# A later repair may retry tagging, but the signed tag still points to that version commit.
version_commit=""
candidates=$(git log --first-parent --format=%H HEAD -- Directory.Build.props)
while IFS= read -r candidate; do
    if [ -n "$candidate" ] &&
        [ "$(version_at "$candidate")" = "$current" ] &&
        [ "$(version_at "${candidate}^" || true)" != "$current" ]; then
        version_commit=$candidate
        break
    fi
done <<< "$candidates"
if [ -z "$version_commit" ]; then
    echo "continue"
    exit 0
fi

subject=$(git show -s --format=%s "$version_commit")
title="chore(release): ${tag} 🔥"
merge_suffix='^ \(#[0-9]+\)$'
if [[ "$subject" != "$title" && ! "${subject#"$title"}" =~ $merge_suffix ]]; then
    echo "Version ${current} does not come from an approved Release MR" >&2
    exit 1
fi
git verify-commit "$version_commit"

git config user.name "litty logs release bot 🔥"
git config user.email "litty-logs-release@noreply.git.ssy.dk"
git tag --sign "$tag" "$version_commit" --message "${tag} dropped from main no cap 🔥"
git verify-tag "$tag"
git push origin "refs/tags/${tag}"
echo "${tag} now points at the signed version commit ${version_commit} 🔒🔥" >&2
echo "tagged"
