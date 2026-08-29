using LittyLogs.Webhooks.Formatters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Logging;

namespace LittyLogs.Webhooks;

/// <summary>
/// extension methods for litty-fying your webhook notifications bestie.
/// one liner and your logs are hitting chat rooms with emojis and gen alpha energy 🪝🔥
/// </summary>
public static class LittyWebhookExtensions
{
    /// <summary>
    /// yeets litty-fied logs to a Matrix hookshot webhook. one liner bestie 🟣🔥
    /// default MinimumLevel is Warning so your chat dont get spammed no cap
    /// </summary>
    public static ILoggingBuilder AddLittyMatrixLogs(
        this ILoggingBuilder builder,
        string webhookUrl)
    {
        return builder.AddLittyWebhookLogs(opts =>
        {
            opts.WebhookUrl = webhookUrl;
            opts.Platform = WebhookPlatform.Matrix;
        });
    }

    /// <summary>
    /// yeets litty-fied logs to a Matrix hookshot webhook with full options control 🟣✨
    /// </summary>
    public static ILoggingBuilder AddLittyMatrixLogs(
        this ILoggingBuilder builder,
        string webhookUrl,
        Action<LittyWebhookOptions> configure)
    {
        return builder.AddLittyWebhookLogs(opts =>
        {
            opts.WebhookUrl = webhookUrl;
            configure(opts);
            opts.Platform = WebhookPlatform.Matrix;
        });
    }

    /// <summary>
    /// yeets litty-fied logs to a Slack incoming webhook as safe plain-text blocks. one liner bestie 🟢🔥
    /// default MinimumLevel is Warning so your chat dont get spammed no cap
    /// </summary>
    public static ILoggingBuilder AddLittySlackLogs(
        this ILoggingBuilder builder,
        string webhookUrl)
    {
        return builder.AddLittyWebhookLogs(opts =>
        {
            opts.WebhookUrl = webhookUrl;
            opts.Platform = WebhookPlatform.Slack;
        });
    }

    /// <summary>
    /// yeets litty-fied logs to a Slack incoming webhook with full options control 🟢✨
    /// Username becomes the message header; Slack still owns the app identity no cap 🔒🔥
    /// </summary>
    public static ILoggingBuilder AddLittySlackLogs(
        this ILoggingBuilder builder,
        string webhookUrl,
        Action<LittyWebhookOptions> configure)
    {
        return builder.AddLittyWebhookLogs(opts =>
        {
            opts.WebhookUrl = webhookUrl;
            configure(opts);
            opts.Platform = WebhookPlatform.Slack;
        });
    }

    /// <summary>
    /// adds litty webhook logging with full options control.
    /// for when you need to configure every vibe bestie ✨
    /// </summary>
    public static ILoggingBuilder AddLittyWebhookLogs(
        this ILoggingBuilder builder,
        Action<LittyWebhookOptions> configure)
    {
        var options = new LittyWebhookOptions();
        configure(options);

        // validate webhook URL at registration time — catch misconfig early, not at 3am bestie 🔒
        if (string.IsNullOrWhiteSpace(options.WebhookUrl))
            throw new ArgumentException(
                "bruh WebhookUrl cant be empty, where we supposed to yeet the logs? 💀",
                nameof(options));

        if (!Uri.TryCreate(options.WebhookUrl, UriKind.Absolute, out var uri))
            throw new ArgumentException(
                $"WebhookUrl '{options.WebhookUrl}' aint a valid URI bestie 💀",
                nameof(options));

        if (uri.Scheme is not ("https" or "http"))
            throw new ArgumentException(
                $"WebhookUrl scheme '{uri.Scheme}' is not it — only http/https allowed bestie 🔒",
                nameof(options));

        if (options.BatchSize <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(options),
                "bruh BatchSize has to be at least 1 or the logs got nowhere to go 💀🔥");

        if (options.Platform == WebhookPlatform.Slack && options.BatchSize > 49)
            throw new ArgumentOutOfRangeException(
                nameof(options),
                "Slack batches max out at 49 logs so the header still fits the 50-block cap bestie 💀🔥");

        // register named HttpClient with standard resilience handler (Polly) —
        // retry with exponential backoff, circuit breaker, per-request timeout
        // all handled by Microsoft.Extensions.Http.Resilience no cap 🔒
        builder.Services.AddHttpClient("LittyWebhooks")
            .AddStandardResilienceHandler();

        // build the writer + provider using DI service provider
        // we need IHttpClientFactory from DI for proper socket management
        builder.Services.AddSingleton<ILoggerProvider>(sp =>
        {
            var httpFactory = sp.GetRequiredService<IHttpClientFactory>();
            IWebhookPayloadFormatter formatter = options.Platform switch
            {
                WebhookPlatform.Matrix => new MatrixPayloadFormatter(),
                WebhookPlatform.Slack => new SlackPayloadFormatter(),
                _ => throw new ArgumentException($"bruh {options.Platform} aint a supported platform yet 💀")
            };
            var writer = new LittyWebhookWriter(httpFactory, formatter, options);
            return new LittyWebhookProvider(writer, options);
        });

        return builder;
    }
}
