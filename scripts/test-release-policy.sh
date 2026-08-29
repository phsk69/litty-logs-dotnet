#!/usr/bin/env bash
set -euo pipefail

repo_root=$(git rev-parse --show-toplevel)
cliff_bin="${CLIFF:-git-cliff}"

if ! command -v "$cliff_bin" >/dev/null 2>&1 && [ ! -x "$cliff_bin" ]; then
    echo "bruh git-cliff is missing — this check never installs deps for you 💀🔥"
    exit 1
fi

fixture_root=$(mktemp -d)
trap 'rm -rf "$fixture_root"' EXIT

new_fixture() {
    local name="$1"
    local version="${2:-0.2.4}"
    local fixture="${fixture_root}/${name}"
    mkdir -p "$fixture"
    git -C "$fixture" init --quiet --initial-branch=main
    git -C "$fixture" config user.name "release policy bestie 🔥"
    git -C "$fixture" config user.email "release-policy@users.noreply.local"
    # fixtures stay hermetic even when the developer has global 1Password/GPG signing vibes 🔒🔥
    git -C "$fixture" config commit.gpgsign false
    git -C "$fixture" config tag.gpgSign false
    git -C "$fixture" commit --quiet --allow-empty -m "chore: establish the tagged baseline 🔥"
    git -C "$fixture" tag "v${version}"
    echo "$fixture"
}

next_version() {
    local fixture="$1"
    "$cliff_bin" --config "${repo_root}/cliff.toml" --repository "$fixture" --bumped-version --unreleased
}

assert_bump() {
    local name="$1"
    local message="$2"
    local expected="$3"
    local fixture
    fixture=$(new_fixture "$name")
    git -C "$fixture" commit --quiet --allow-empty -m "$message"
    local actual
    actual=$(next_version "$fixture")
    actual="${actual#v}"
    if [ "$actual" != "$expected" ]; then
        echo "bruh ${message} calculated ${actual}, expected ${expected} 💀🔥"
        exit 1
    fi
    echo "${message} -> ${actual} slays 🔥"
}

assert_no_bump() {
    local name="$1"
    local message="$2"
    local fixture
    fixture=$(new_fixture "$name")
    git -C "$fixture" commit --quiet --allow-empty -m "$message"
    local actual
    actual=$(next_version "$fixture")
    actual="${actual#v}"
    if [ -n "$actual" ] && [ "$actual" != "0.2.4" ]; then
        echo "bruh ${message} spawned release ${actual} when it should stay quiet 💀🔥"
        exit 1
    fi
    echo "${message} correctly spawned zero release noise 🔍🔥"
}

assert_bump fix "fix: eliminate the cooked edge case 🔥" 0.2.5
assert_bump perf "perf: make the hot path zoom 🔥" 0.2.5
assert_bump revert "revert: undo the cursed change 🔥" 0.2.5
assert_bump deps "chore(deps): update the nuggies 🔥" 0.2.5
assert_bump feature "feat: ship fresh rizz 🔥" 0.3.0
assert_bump breaking "feat(api)!: reset the public surface 🔥" 1.0.0
assert_no_bump chore "chore: rearrange the maintenance vibes 🔥"
assert_no_bump docs "docs: explain the vibes harder 🔥"

graph_fixture=$(new_fixture deleted-branch-graph)
git -C "$graph_fixture" switch --quiet -c feature/temporary-rizz
echo "branch content gets squashed then the ref gets yeeted 🔥" > "${graph_fixture}/feature.txt"
git -C "$graph_fixture" add feature.txt
git -C "$graph_fixture" commit --quiet -m "feat: ship from a tiny branch 🔥"
git -C "$graph_fixture" switch --quiet main
git -C "$graph_fixture" merge --quiet --squash feature/temporary-rizz
git -C "$graph_fixture" commit --quiet -m "feat: squash the tiny branch into main 🔥"
git -C "$graph_fixture" branch --delete --force feature/temporary-rizz >/dev/null
graph_actual=$(next_version "$graph_fixture")
graph_actual="${graph_actual#v}"
if [ "$graph_actual" != "0.3.0" ]; then
    echo "bruh deleted source branch graph calculated ${graph_actual}, expected 0.3.0 💀🔥"
    exit 1
fi

echo "deleted source branches have zero power over release math, all policy checks ate 🔒🔥"
