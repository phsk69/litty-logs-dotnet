using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace LittyLogs.Webhooks;

/// <summary>
/// ILoggerProvider that creates litty loggers writing to the async webhook writer.
/// owns the LittyWebhookWriter and handles disposal bestie 🪝🔥
/// </summary>
public sealed class LittyWebhookProvider : ILoggerProvider
{
    private readonly LittyWebhookWriter _writer;
    private readonly LittyWebhookOptions _webhookOptions;
    private readonly LittyLogsOptions _littyOptions;

    internal LittyWebhookProvider(LittyWebhookWriter writer, LittyWebhookOptions? options = null)
    {
        _webhookOptions = options ?? new LittyWebhookOptions();
        _littyOptions = _webhookOptions.ToLittyLogsOptions();
        _writer = writer;
    }

    public ILogger CreateLogger(string categoryName)
    {
        // prevent infinite recursion — the HTTP client used by the webhook writer
        // generates its own logs. if those go to the webhook, we get an infinite loop
        // of HTTP requests generating more logs generating more HTTP requests 💀
        if (IsHttpClientCategory(categoryName))
            return NullLogger.Instance;

        return new LittyWebhookLogger(categoryName, _writer, _webhookOptions, _littyOptions);
    }

    /// <summary>
    /// checks if this category belongs to the HTTP pipeline we use for webhook delivery.
    /// filtering these out prevents the infinite feedback loop bestie 🔒
    /// </summary>
    private static bool IsHttpClientCategory(string category) =>
        category.StartsWith("System.Net.Http", StringComparison.Ordinal) ||
        category.StartsWith("Microsoft.Extensions.Http", StringComparison.Ordinal) ||
        category.StartsWith("Polly", StringComparison.Ordinal);

    public void Dispose()
    {
        _writer.Dispose();
    }
}
