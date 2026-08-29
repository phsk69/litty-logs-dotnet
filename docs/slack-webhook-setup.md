# Slack incoming webhook setup for local testing 🟢🔥

use a dedicated test channel so local log spam does not jump-scare the whole workspace.

## create the webhook

1. create or choose a Slack channel for local LittyLogs testing. for a private channel, make sure you are already a member.
2. open [Slack's app creation page](https://api.slack.com/apps?new_app=1), choose **From scratch**, name the app, and select the workspace that owns the test channel.
3. in the app settings, open **Incoming Webhooks** and switch **Activate Incoming Webhooks** on.
4. select **Add New Webhook to Workspace**.
5. choose the test channel, then select **Allow**. a workspace admin may need to approve the app if app installation is restricted.
6. under **Webhook URLs for Your Workspace**, copy the new URL. it starts with `https://hooks.slack.com/services/`.

Slack's full walkthrough lives in the [official incoming-webhooks guide](https://docs.slack.dev/messaging/sending-messages-using-incoming-webhooks). the webhook is locked to the channel and app identity chosen during installation; LittyLogs' `Username` option changes the message header only.

## keep the URL local

create `.env` in the repository root:

```dotenv
SLACK_WEBHOOK_URL=https://hooks.slack.com/services/replace/with/your-secret
```

`.env` and `.env.*` are gitignored. treat this URL like a password: do not paste it into source, issues, logs, or test snapshots. Slack actively revokes leaked webhook URLs, so rotate it in the app settings if it escapes 🔒🔥

## test it locally

the normal test suite is hermetic: it uses an in-memory HTTP handler and never contacts Slack.

```bash
just test
```

for an opt-in live smoke test against the channel configured above, run:

```bash
just example webhooks
```

the example loads `.env`, sends the Slack demo to the live webhook, and uses a local mock for Matrix when `HOOKSHOT_URL` is absent. expect several Warning-or-higher messages in the Slack channel. no dependency installer runs as part of these recipes; if the .NET SDK reports missing packages, stop and get approval before restoring them.

when the smoke test is done, delete `.env` or revoke the webhook from the Slack app settings if the test credential is no longer needed.

## quick troubleshooting

- **`action_prohibited`** — workspace policy blocked the install; ask a Slack admin to approve the app.
- **no message appears** — confirm the app is still installed, the webhook belongs to the intended channel, and `SLACK_WEBHOOK_URL` has no quotes or trailing spaces.
- **HTTP 404 or 410** — the webhook was revoked or removed; create a fresh URL.
- **wrong sender name or icon** — Slack owns the installed app identity. change it in the Slack app configuration, not with `LittyWebhookOptions.Username`.
