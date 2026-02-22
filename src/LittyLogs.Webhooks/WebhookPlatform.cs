namespace LittyLogs.Webhooks;

/// <summary>
/// which platform we yeeting logs to bestie 🪝
/// </summary>
public enum WebhookPlatform
{
    /// <summary>Matrix hookshot webhook — markdown formatting, simple POST, auth is in the URL 🟣</summary>
    Matrix,

    /// <summary>Teams Adaptive Cards — colored containers per severity (coming soon bestie) 🟦</summary>
    Teams
}
