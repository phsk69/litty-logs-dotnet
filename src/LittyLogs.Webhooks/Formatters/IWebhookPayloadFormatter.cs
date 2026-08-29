namespace LittyLogs.Webhooks.Formatters;

/// <summary>
/// interface for platform-specific webhook payload formatting.
/// each platform implements this to build its own JSON bestie 🪝🔥
/// </summary>
public interface IWebhookPayloadFormatter
{
    /// <summary>
    /// takes a batch of formatted log lines and builds the platform-specific JSON payload.
    /// returns the serialized JSON string ready to POST no cap 🔥
    /// </summary>
    string FormatPayload(IReadOnlyList<string> messages, LittyWebhookOptions options);
}
