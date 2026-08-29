# commit and tag signing 🔏🔥

every commit added to `main` after the signing rollout must carry a trusted SSH signature. old history stays grandfathered; rewriting published commits or tags for cosmetic signatures would be deeply cooked 💀🔥

release tags are signed too. the `v*` tag namespace is restricted to the dedicated release bot and the human break-glass administrator, and the ship workflow verifies the tag before any NuGet package leaves the runner 🔒🔥

## rollout cutoff 🧱🔥

the last grandfathered `main` commit is:

```text
89ce86f06910c4b8f6f7254afa11651f85385b62
```

CI verifies every commit reachable after that cutoff with `.forgejo/signing/allowed_signers`. the matching revocation list lives beside it.

## trusted identities 🪪🔥

| principal | fingerprint | private-key home | job |
|---|---|---|---|
| `phsk69@tuta.io` | `SHA256:vo5tLbpl7f2Lj+qzKY9T7IWInB3WDD3nJjsHly5ChX8` | human 1Password SSH agent | human commits and break-glass tags |
| `forgejo@noreply.git.ssy.dk` | `SHA256:d8XE9tW1hAilHMiXZLA4mdHNMedhrmahULnIxQpmFgY` | Forgejo instance signing key | squash merges and web/API commits |
| `renovate@monkeh.io` | `SHA256:jbzF42HtJS7irqE3Jd4JPubXStOIytzidxhdEgm02Pc` | `Automation/renovate-signing` | Renovate commits |
| `litty-logs-release@noreply.git.ssy.dk` | `SHA256:glcAr+2qy78C126hwWy7It4ND94VzwatrJkb+eeBByg` | `Automation/litty-logs-dotnet-release-signing` | release commits and tags |

the release bot is a restricted Forgejo collaborator on this repository only. its PAT is scoped to `public/litty-logs-dotnet` with only `write:repository`; its signing key is separate from that PAT and every other bot key 🔐🔥

## human and interactive signing with 1Password 🔏🔥

developer machines use the 1Password SSH agent. do not export a human private key, add an `IdentityFile`, or create a repository-local signing-key override.

the standard Git configuration is:

```bash
git config --global gpg.format ssh
git config --global commit.gpgsign true
git config --global user.signingkey 'ssh-ed25519 AAAA...'
```

on Linux, point clients at the 1Password socket when the desktop integration is not already exported:

```bash
export SSH_AUTH_SOCK="$HOME/.1password/agent.sock"
```

Forgejo SSH-key verification challenges are also signed by the 1Password agent. select the public key with `ssh-keygen -Y sign`; the private operation stays inside 1Password. automation-vault keys must be explicitly available through the existing 1Password agent allowlist without removing the default vault entries.

## unattended release signing 🤖🔏🔥

Forgejo Actions cannot use a developer desktop agent. the dedicated release key is therefore injected only into the release job through the repository-level `AUTOMATION_SSH_SIGNING_KEY` secret. the canonical recoverable copy stays in 1Password.

the workflow checks the public fingerprint before use and explicitly selects native `ssh-keygen` inside the isolated runner. that runner-only setting never replaces a developer's 1Password signing helper. CI signs the rolling release commit and immutable tag, verifies both locally against the repository allowlist, and only then pushes. `release.yml` independently runs `git verify-tag` before building or publishing packages.

## Forgejo enforcement 🏰🔥

`main` protection must keep `require_signed_commits=true` and `apply_to_admins=true` while preserving required CI checks. squash merge remains the only merge style so the conventional PR title becomes one Forgejo-signed release input.

the `v*` tag protection permits creation only by `litty-logs-release-bot` and `psk`. tags are immutable: never delete, move, or recreate a published version.

## revocation 🚨🔥

if a key is compromised:

1. add its public key to `.forgejo/signing/revoked_signers` and remove it from `allowed_signers` 🔥
2. revoke the key in Forgejo and remove its runtime secret 🔥
3. provision a fresh independent key and update the relevant account, allowlist, and fingerprint check 🔥
4. do not rewrite historical commits or tags; revocation is forward-looking 🔥
