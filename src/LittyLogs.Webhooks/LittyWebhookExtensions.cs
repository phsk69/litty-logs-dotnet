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
            opts.Platform = WebhookPlatform.Matrix;
            configure(opts);
        });
    }

    /// <summary>
    /// yeets litty-fied logs to a Teams webhook as Adaptive Cards. one liner bestie 🟦🔥
    /// default MinimumLevel is Warning so your chat dont get spammed no cap
    /// </summary>
    public static ILoggingBuilder AddLittyTeamsLogs(
        this ILoggingBuilder builder,
        string webhookUrl)
    {
        return builder.AddLittyWebhookLogs(opts =>
        {
            opts.WebhookUrl = webhookUrl;
            opts.Platform = WebhookPlatform.Teams;
        });
    }

    /// <summary>
    /// yeets litty-fied logs to a Teams webhook as Adaptive Cards with full options control 🟦✨
    /// severity-colored containers make your Teams chat lowkey look like a dashboard bestie 💅
    /// </summary>
    public static ILoggingBuilder AddLittyTeamsLogs(
        this ILoggingBuilder builder,
        string webhookUrl,
        Action<LittyWebhookOptions> configure)
    {
        return builder.AddLittyWebhookLogs(opts =>
        {
            opts.WebhookUrl = webhookUrl;
            opts.Platform = WebhookPlatform.Teams;
            configure(opts);
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
                WebhookPlatform.Teams => new TeamsPayloadFormatter(),
                _ => throw new ArgumentException($"bruh {options.Platform} aint a supported platform yet 💀")
            };
            var writer = new LittyWebhookWriter(httpFactory, formatter, options);
            return new LittyWebhookProvider(writer, options);
        });

        return builder;
    }
}
