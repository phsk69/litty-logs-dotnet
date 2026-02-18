using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace LittyLogs.Xunit;

/// <summary>
/// ILoggerProvider that creates litty loggers writing to xUnit's ITestOutputHelper.
/// this is how we get litty formatting in test output bestie 🔥
/// </summary>
public sealed class LittyLogsXunitProvider : ILoggerProvider
{
    private readonly ITestOutputHelper _output;
    private readonly LittyLogsOptions _options;

    public LittyLogsXunitProvider(ITestOutputHelper output, LittyLogsOptions? options = null)
    {
        _output = output;
        _options = options ?? new LittyLogsOptions { UseColors = false };
    }

    public ILogger CreateLogger(string categoryName) =>
        new LittyLogsXunitLogger(_output, categoryName, _options);

    public void Dispose() { }
}
