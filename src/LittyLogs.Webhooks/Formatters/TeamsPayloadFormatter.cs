namespace LittyLogs.Webhooks.Formatters;

/// <summary>
/// formats log batches into Teams Adaptive Card JSON.
/// aint cooked yet — this is a stub for the future bestie 🟦
/// </summary>
internal sealed class TeamsPayloadFormatter : IWebhookPayloadFormatter
{
    public string FormatPayload(IReadOnlyList<string> messages, LittyWebhookOptions options)
    {
        throw new NotImplementedException(
            "teams adaptive cards aint cooked yet bestie, use Matrix for now 🟦💀 " +
            "check TODO.md for the vibes on whats coming");
    }
}
