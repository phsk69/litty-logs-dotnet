namespace LittyLogs.Webhooks;

/// <summary>
/// which platform we yeeting logs to bestie 🪝
/// </summary>
public enum WebhookPlatform
{
    /// <summary>Matrix hookshot webhook — markdown formatting, simple POST, auth is in the URL 🟣</summary>
    Matrix,

    /// <summary>Slack incoming webhook — safe plain-text Block Kit with zero markdown jump scares 🟢🔥</summary>
    Slack
}
