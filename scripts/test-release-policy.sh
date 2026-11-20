#!/usr/bin/env bash
set -euo pipefail

repo_root=$(git rev-parse --show-toplevel)
cliff_bin="${CLIFF:-git-cliff}"

if [ "$("$cliff_bin" --version)" != 'git-cliff 2.14.2' ]; then
    echo "bruh release fixtures require git-cliff 2.14.2 💀🔥"
    exit 1
fi
export CLIFF="$cliff_bin"

fixture_root=$(mktemp -d)
trap 'rm -rf "$fixture_root"' EXIT

fixture_signing_key="${fixture_root}/release-signing"
fixture_allowed_signers="${fixture_root}/allowed_signers"
ssh-keygen -q -t ed25519 -N '' -f "$fixture_signing_key"
fixture_public_key=$(awk '{ print $1 " " $2 }' "${fixture_signing_key}.pub")
printf 'release-policy@users.noreply.local namespaces="git" %s\n' "$fixture_public_key" > "$fixture_allowed_signers"

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

commit_change() {
    local fixture=$1 path=$2 message=$3
    mkdir -p "$(dirname "$fixture/$path")"
    printf 'fixture change\n' >> "$fixture/$path"
    git -C "$fixture" add -- "$path"
    git -C "$fixture" commit --quiet -m "$message"
}

assert_bump() {
    local name="$1"
    local message="$2"
    local expected="$3"
    local baseline="${4:-0.2.4}"
    local fixture
    fixture=$(new_fixture "$name" "$baseline")
    commit_change "$fixture" "src/LittyLogs/Product.cs" "$message"
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
    commit_change "$fixture" "${3:-src/LittyLogs/Product.cs}" "$message"
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
assert_bump dotnet-support "feat(dotnet)!: support the current LTS and STS SDKs 🔥" 3.0.0 2.0.10
assert_no_bump chore "chore: rearrange the maintenance vibes 🔥"
assert_no_bump docs "docs: explain the vibes harder 🔥"

assert_no_bump ci "ci: harden the pipeline 🔥"
assert_no_bump ci-fix "fix(ci): repair the pipeline 🔥" .forgejo/workflows/ci.yml
assert_no_bump ci-breaking "feat(ci)!: replace the pipeline 🔥" .forgejo/workflows/ci.yml
assert_no_bump action-deps "chore(deps): update an action 🔥" .forgejo/workflows/ci.yml
assert_no_bump docs-fix "fix: correct the documentation 🔥" README.md
assert_no_bump source-doc "fix: correct package docs 🔥" src/LittyLogs/README.md
assert_no_bump agent-docs "docs: align the agent guidance 🔥" AGENTS.md
assert_no_bump test-deps "chore(deps): update the test runner 🔥" tests/LittyLogs.Tests/packages.lock.json
assert_no_bump tooling "fix: repair release automation 🔥" scripts/release-notes.sh

notes_fixture=$(new_fixture mixed-release)
git -C "$notes_fixture" switch --quiet -c unrelated
commit_change "$notes_fixture" src/LittyLogs/Product.cs "feat: an unrelated feature 🔥"
git -C "$notes_fixture" tag v99.9.9
git -C "$notes_fixture" switch --quiet main
commit_change "$notes_fixture" .forgejo/workflows/ci.yml "feat(ci)!: replace the pipeline 🔥"
commit_change "$notes_fixture" src/LittyLogs/packages.lock.json "chore(deps): refresh shipped nuggies 🔥"
commit_change "$notes_fixture" src/LittyLogs/Product.cs "fix: repair the formatter 🔥"
[[ $(next_version "$notes_fixture") == v0.2.5 ]]
preview="$fixture_root/preview.md"
published="$fixture_root/published.md"
(cd "$notes_fixture" && bash "$repo_root/scripts/release-notes.sh" preview 0.2.5) > "$preview"
grep -qF 'Refresh shipped nuggies' "$preview"
grep -qF 'Repair the formatter' "$preview"
if grep -Eqi 'replace the pipeline|unrelated feature' "$preview"; then exit 1; fi
for mode in create update; do
    payload=$(bash "$repo_root/scripts/release-pr-payload.sh" "$mode" 0.2.5 "$preview")
    jq -e --arg mode "$mode" --rawfile notes "$preview" '
        (.body | endswith("\n\n" + $notes)) and
        (if $mode == "create" then .head == "release-pr" and .base == "main" else (has("head") or has("base") | not) end)
    ' <<< "$payload" > /dev/null
done
commit_change "$notes_fixture" Directory.Build.props "chore(release): v0.2.5 🔥"
git -C "$notes_fixture" tag v0.2.5
(cd "$notes_fixture" && bash "$repo_root/scripts/release-notes.sh" tag 0.2.5) > "$published"
diff -u <(tail -n +2 "$preview") <(tail -n +2 "$published")

manual_fixture=$(new_fixture manual-notes)
(cd "$manual_fixture" && bash "$repo_root/scripts/release-notes.sh" preview 0.2.5) > "$preview"
commit_change "$manual_fixture" Directory.Build.props "chore(release): v0.2.5 🔥"
git -C "$manual_fixture" tag v0.2.5
(cd "$manual_fixture" && bash "$repo_root/scripts/release-notes.sh" tag 0.2.5) > "$published"
diff -u "$preview" "$published"
grep -qF 'explicit maintenance release' "$published"

promotion_fixture=$(new_fixture promotion)
commit_change "$promotion_fixture" src/LittyLogs/Product.cs "feat: ship the RC feature 🔥"
git -C "$promotion_fixture" tag v0.3.0-rc.1
commit_change "$promotion_fixture" src/LittyLogs/Product.cs "fix: repair the second RC 🔥"
git -C "$promotion_fixture" tag v0.3.0-rc.2
(cd "$promotion_fixture" && bash "$repo_root/scripts/release-notes.sh" preview 0.3.0) > "$preview"
commit_change "$promotion_fixture" Directory.Build.props "chore(release): v0.3.0 🔥"
git -C "$promotion_fixture" tag v0.3.0
(cd "$promotion_fixture" && bash "$repo_root/scripts/release-notes.sh" tag 0.3.0) > "$published"
[[ $(grep -c '^## ' "$published") == 1 ]]
grep -qF 'Ship the RC feature' "$published"
grep -qF 'Repair the second RC' "$published"
diff -u <(tail -n +2 "$preview") <(tail -n +2 "$published")


graph_fixture=$(new_fixture deleted-branch-graph)
git -C "$graph_fixture" switch --quiet -c feature/temporary-rizz
commit_change "$graph_fixture" src/LittyLogs/Product.cs "feat: ship from a tiny branch 🔥"
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
git -C "$recovery_fixture" config gpg.format ssh
git -C "$recovery_fixture" config gpg.ssh.program ssh-keygen
git -C "$recovery_fixture" config user.signingkey "$fixture_signing_key"
git -C "$recovery_fixture" config gpg.ssh.allowedSignersFile "$fixture_allowed_signers"
git -C "$recovery_fixture" config tag.gpgSign true
printf '<Project>\n  <PropertyGroup>\n    <Version>0.2.4</Version>\n  </PropertyGroup>\n</Project>\n' > "${recovery_fixture}/Directory.Build.props"
git -C "$recovery_fixture" add Directory.Build.props
git -C "$recovery_fixture" commit --quiet -m "chore: establish the recovery baseline 🔥"
git -C "$recovery_fixture" tag --message "v0.2.4 recovery baseline dropped 🔥" v0.2.4
git -C "$recovery_fixture" remote add origin "$recovery_remote"
git -C "$recovery_fixture" push --quiet origin main refs/tags/v0.2.4

baseline_commit=$(git -C "$recovery_fixture" rev-parse HEAD)
[[ $(cd "$recovery_fixture" && bash "$repo_root/scripts/release-tag-current.sh" workflow_dispatch 0.2.4 "$baseline_commit") == continue ]]
[[ $(cd "$recovery_fixture" && bash "$repo_root/scripts/release-tag-current.sh" push 0.2.4 "$baseline_commit") == tagged ]]

git -C "$recovery_fixture" switch --quiet -c unsigned-release
sed -i 's/<Version>0.2.4<\//<Version>0.2.5<\//' "${recovery_fixture}/Directory.Build.props"
git -C "$recovery_fixture" add Directory.Build.props
git -C "$recovery_fixture" commit --quiet -m "chore(release): v0.2.5 🔥"
unsigned_commit=$(git -C "$recovery_fixture" rev-parse HEAD)
if (cd "$recovery_fixture" && bash "$repo_root/scripts/release-tag-current.sh" push 0.2.5 "$unsigned_commit"); then
    echo "Unsigned version commit was allowed to tag" >&2
    exit 1
fi
test -z "$(git -C "$recovery_fixture" tag --list v0.2.5)"
git -C "$recovery_fixture" switch --quiet main

sed -i 's/<Version>0.2.4<\//<Version>1.0.0<\//' "${recovery_fixture}/Directory.Build.props"
git -C "$recovery_fixture" add Directory.Build.props
git -C "$recovery_fixture" commit --quiet --gpg-sign -m "chore(release): v1.0.0 🔥 (#32)"
version_commit=$(git -C "$recovery_fixture" rev-parse HEAD)
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
if [ "$recovery_tag" != "$version_commit" ] || [ "$remote_recovery_tag" != "$version_commit" ]; then
    echo "bruh recovered v1.0.0 missed the exact signed version commit 💀🔥"
    exit 1
fi

git -C "$recovery_fixture" commit --quiet --allow-empty -m "chore: post-release maintenance stays untagged 🔥"
post_release_head=$(git -C "$recovery_fixture" rev-parse HEAD)
post_release_result=$(cd "$recovery_fixture" && bash "${repo_root}/scripts/release-tag-current.sh" push 1.0.0 "$post_release_head")
if [ "$post_release_result" != "continue" ]; then
    echo "bruh post-release maintenance tried to retarget v1.0.0 💀🔥"
    exit 1
fi
if [ "$(git -C "$recovery_fixture" rev-list -n 1 refs/tags/v1.0.0)" != "$version_commit" ]; then
    echo "bruh immutable v1.0.0 moved after release 💀🔥"
    exit 1
fi

echo "pre-tag recovery preserves the reviewed version commit 🔒🔥"
