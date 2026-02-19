using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

/// <summary>
/// background service that runs vibe checks at different log levels
/// so you can see the full emoji spectrum in action bestie 🔥
/// </summary>
public sealed class LittyBackgroundService(ILogger<LittyBackgroundService> logger) : BackgroundService
{
    private static readonly (LogLevel Level, string Message)[] VibeChecks =
    [
        (LogLevel.Trace,       "👀 lowkey peeking at the system internals rn"),
        (LogLevel.Debug,       "🔍 investigating if the vibes are still immaculate"),
        (LogLevel.Information, "🔥 vibe check passed, everything is bussin fr fr"),
        (LogLevel.Warning,     "😤 the vibes are kinda mid rn not gonna lie"),
        (LogLevel.Error,       "💀 vibe check FAILED, something took a fat L"),
        (LogLevel.Information, "🔥 recovering from that L, we back on top no cap"),
    ];

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("vibe check service just clocked in, finna monitor the vibes 💅");

        var index = 0;
        while (!stoppingToken.IsCancellationRequested)
        {
            var (level, message) = VibeChecks[index % VibeChecks.Length];
            logger.Log(level, message);

            index++;
            await Task.Delay(3000, stoppingToken);
        }
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("vibe check service is booting up bestie, lets get this bread 🍞");
        return base.StartAsync(cancellationToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("vibe check service said peace out, we dipping 🫡");
        return base.StopAsync(cancellationToken);
    }
}
