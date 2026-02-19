using Microsoft.Extensions.Logging;

namespace LittyLogs.File;

/// <summary>
/// ILoggerProvider that creates litty loggers writing to the async file sink.
/// owns the LittyFileWriter and handles disposal bestie 📁🔥
/// </summary>
public sealed class LittyFileLogsProvider : ILoggerProvider
{
    private readonly LittyFileWriter _writer;
    private readonly LittyFileLogsOptions _fileOptions;
    private readonly LittyLogsOptions _littyOptions;

    public LittyFileLogsProvider(LittyFileLogsOptions? options = null)
    {
        _fileOptions = options ?? new LittyFileLogsOptions();
        _littyOptions = _fileOptions.ToLittyLogsOptions();
        _writer = new LittyFileWriter(_fileOptions);
    }

    public ILogger CreateLogger(string categoryName) =>
        new LittyFileLogger(categoryName, _writer, _fileOptions, _littyOptions);

    public void Dispose()
    {
        _writer.Dispose();
    }
}
