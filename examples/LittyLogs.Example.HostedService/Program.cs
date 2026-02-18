using LittyLogs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureLogging(logging =>
    {
        // one line to litty-fy ALL logs in a hosted service no cap 🔥
        logging.AddLittyLogs();
    })
    .ConfigureServices(services =>
    {
        services.AddHostedService<LittyBackgroundService>();
    })
    .Build();

await host.RunAsync();
