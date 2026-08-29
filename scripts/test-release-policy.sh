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

recovery_remote="${fixture_root}/recovery-remote.git"
recovery_fixture="${fixture_root}/pre-tag-recovery"
git init --quiet --bare "$recovery_remote"
git init --quiet --initial-branch=main "$recovery_fixture"
git -C "$recovery_fixture" config user.name "release recovery bestie 🔥"
git -C "$recovery_fixture" config user.email "release-recovery@users.noreply.local"
git -C "$recovery_fixture" config commit.gpgsign false
git -C "$recovery_fixture" config tag.gpgSign false
printf '<Project>\n  <PropertyGroup>\n    <Version>0.2.4</Version>\n  </PropertyGroup>\n</Project>\n' > "${recovery_fixture}/Directory.Build.props"
printf '# changelog 📜🔥\n\n## [Unreleased]\n\n## [0.2.4]\n' > "${recovery_fixture}/CHANGELOG.md"
git -C "$recovery_fixture" add Directory.Build.props CHANGELOG.md
git -C "$recovery_fixture" commit --quiet -m "chore: establish the recovery baseline 🔥"
git -C "$recovery_fixture" tag v0.2.4
git -C "$recovery_fixture" remote add origin "$recovery_remote"
git -C "$recovery_fixture" push --quiet origin main refs/tags/v0.2.4

sed -i 's/<Version>0.2.4<\//<Version>1.0.0<\//' "${recovery_fixture}/Directory.Build.props"
printf '\n## [1.0.0]\n\n- the breaking glow-up 🔥\n' >> "${recovery_fixture}/CHANGELOG.md"
git -C "$recovery_fixture" add Directory.Build.props CHANGELOG.md
git -C "$recovery_fixture" commit --quiet -m "feat(api)!: ship the stable era 🔥"
printf 'Forgejo-native workflow fix lands before the tag 🔥\n' > "${recovery_fixture}/workflow-fix.txt"
git -C "$recovery_fixture" add workflow-fix.txt
git -C "$recovery_fixture" commit --quiet -m "fix(ci): remove unsupported workflow permissions 🔥"
recovery_head=$(git -C "$recovery_fixture" rev-parse HEAD)
recovery_result=$(cd "$recovery_fixture" && bash "${repo_root}/scripts/release-tag-current.sh" push 1.0.0 "$recovery_head")
if [ "$recovery_result" != "tagged" ]; then
    echo "bruh pre-tag recovery returned ${recovery_result}, expected tagged 💀🔥"
    exit 1
fi
recovery_tag=$(git -C "$recovery_fixture" rev-list -n 1 refs/tags/v1.0.0)
remote_recovery_tag=$(git --git-dir="$recovery_remote" rev-list -n 1 refs/tags/v1.0.0)
if [ "$recovery_tag" != "$recovery_head" ] || [ "$remote_recovery_tag" != "$recovery_head" ]; then
    echo "bruh recovered v1.0.0 missed the fixed main head 💀🔥"
    exit 1
fi

git -C "$recovery_fixture" commit --quiet --allow-empty -m "chore: post-release maintenance stays untagged 🔥"
post_release_head=$(git -C "$recovery_fixture" rev-parse HEAD)
post_release_result=$(cd "$recovery_fixture" && bash "${repo_root}/scripts/release-tag-current.sh" push 1.0.0 "$post_release_head")
if [ "$post_release_result" != "continue" ]; then
    echo "bruh post-release maintenance tried to retarget v1.0.0 💀🔥"
    exit 1
fi
if [ "$(git -C "$recovery_fixture" rev-list -n 1 refs/tags/v1.0.0)" != "$recovery_head" ]; then
    echo "bruh immutable v1.0.0 moved after release 💀🔥"
    exit 1
fi

echo "pre-tag recovery folds the fix into v1.0.0 once, then immutable means immutable 🔒🔥"
