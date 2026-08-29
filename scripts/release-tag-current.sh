#!/usr/bin/env bash
set -euo pipefail

event_name="${1:?event name is required no cap 💀🔥}"
current="${2:?current version is required no cap 💀🔥}"
expected_sha="${3:?expected main SHA is required no cap 💀🔥}"

if [ "$event_name" != "push" ]; then
    echo "continue"
    exit 0
fi

actual_sha=$(git rev-parse HEAD)
if [ "$actual_sha" != "$expected_sha" ]; then
    echo "bruh release tagging expected ${expected_sha}, but checkout is ${actual_sha} 💀🔥" >&2
    exit 1
fi

tag="v${current}"
existing=$(git rev-list -n 1 "refs/tags/${tag}" 2>/dev/null || true)
parent=""
if git rev-parse HEAD^ >/dev/null 2>&1; then
    parent=$(git show HEAD^:Directory.Build.props 2>/dev/null | grep -oP '(?<=<Version>)[^<]+' || true)
fi

if [ -n "$existing" ]; then
    if [ "$existing" = "$actual_sha" ]; then
        echo "${tag} already points at this exact commit — retry is a clean no-op 🔄🔥" >&2
        echo "tagged"
        exit 0
    fi
    if [ -n "$parent" ] && [ "$parent" != "$current" ]; then
        echo "bruh ${tag} already points at ${existing}; immutable tags never get deleted or moved 💀🔥" >&2
        exit 1
    fi
    echo "continue"
    exit 0
fi

tag_reason=""
if [ -n "$parent" ] && [ "$parent" != "$current" ]; then
    tag_reason="merged version change"
else
    baseline_tag=$(git describe --tags --abbrev=0 --match 'v[0-9]*' HEAD 2>/dev/null || true)
    baseline_version=""
    if [ -n "$baseline_tag" ]; then
        baseline_version=$(git show "${baseline_tag}:Directory.Build.props" 2>/dev/null | grep -oP '(?<=<Version>)[^<]+' || true)
    fi
    if [ -n "$baseline_version" ] && [ "$baseline_version" != "$current" ] && grep -qF "## [${current}]" CHANGELOG.md; then
        tag_reason="pre-tag recovery after ${baseline_tag}"
    fi
fi

if [ -z "$tag_reason" ]; then
    echo "continue"
    exit 0
fi

if ! grep -qF "## [${current}]" CHANGELOG.md; then
    echo "bruh CHANGELOG.md has no ${current} section, tag stays locked down 💀🔥" >&2
    exit 1
fi

git config user.name "litty logs release bot 🔥"
git config user.email "litty-logs-release@noreply.git.ssy.dk"
git tag --sign "$tag" --message "${tag} dropped from main no cap 🔥"
git verify-tag "$tag"
git push origin "refs/tags/${tag}"
echo "${tag} now points at ${actual_sha} via ${tag_reason}; the ship pipeline is awake 🚀🔥" >&2
echo "tagged"
