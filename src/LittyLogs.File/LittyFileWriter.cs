using System.IO.Compression;
using System.Threading.Channels;

namespace LittyLogs.File;

/// <summary>
/// the async I/O engine that makes file logging bussin no cap 🔥
/// Channel-based: Enqueue() returns immediately (non-blocking),
/// background task writes to disk with fully async I/O.
/// handles rotation (size + time) and gzip compression of rotated files.
/// startup safeguard: NEVER auto-rotates on startup — only rotates before writing the NEXT entry 🔒
/// </summary>
internal sealed class LittyFileWriter : IAsyncDisposable, IDisposable
{
    private readonly LittyFileLogsOptions _options;
    private readonly Channel<string> _channel;
    private readonly Task _consumerTask;
    private readonly CancellationTokenSource _cts = new();

    private StreamWriter? _writer;
    private string _currentFilePath;
    private long _currentFileSize;
    private DateTimeOffset _currentFilePeriod;
    private bool _isFirstWrite = true;
    private int _rotationCounter; // prevents filename collisions when multiple rotations happen in same second

    public LittyFileWriter(LittyFileLogsOptions options)
    {
        _options = options;
        _currentFilePath = options.FilePath;
        _currentFilePeriod = GetCurrentPeriod();

        // bounded channel with 10k capacity — if the app logs faster than disk can write,
        // we drop the oldest entries. better than OOM-ing bestie 💀
        _channel = Channel.CreateBounded<string>(new BoundedChannelOptions(10_000)
        {
            FullMode = BoundedChannelFullMode.DropOldest,
            SingleReader = true,
            SingleWriter = false
        });

        // fire up the background consumer — fully async I/O all the way down 🔥
        _consumerTask = Task.Run(() => ConsumeAsync(_cts.Token));
    }

    /// <summary>
    /// enqueues a formatted log line for async writing. returns immediately — non-blocking king 👑
    /// </summary>
    public void Enqueue(string formattedLine)
    {
        _channel.Writer.TryWrite(formattedLine);
    }

    private async Task ConsumeAsync(CancellationToken ct)
    {
        try
        {
            await foreach (var line in _channel.Reader.ReadAllAsync(ct))
            {
                await WriteLineAsync(line);
            }
        }
        catch (OperationCanceledException)
        {
            // we out, no biggie 🫡
        }
        finally
        {
            // drain any remaining items in the channel before closing
            while (_channel.Reader.TryRead(out var remaining))
            {
                await WriteLineAsync(remaining);
            }

            if (_writer is not null)
            {
                await _writer.FlushAsync();
                await _writer.DisposeAsync();
                _writer = null;
            }
        }
    }

    private async Task WriteLineAsync(string line)
    {
        // on first write, just open/append — no rotation check (startup safeguard) 🔒
        if (_isFirstWrite)
        {
            _isFirstWrite = false;
            await EnsureWriterAsync();
        }
        else
        {
            // rotation only happens BEFORE writing the NEXT entry — never on startup
            await MaybeRotateAsync();
        }

        await _writer!.WriteLineAsync(line);
        _currentFileSize += System.Text.Encoding.UTF8.GetByteCount(line) + Environment.NewLine.Length;

        // flush after every write for safety — we dont wanna lose logs bestie
        await _writer.FlushAsync();
    }

    private async Task EnsureWriterAsync()
    {
        if (_writer is not null)
            return;

        var resolvedPath = ResolveFilePath();
        _currentFilePath = resolvedPath;

        var dir = Path.GetDirectoryName(resolvedPath);
        if (dir is not null && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        var stream = new FileStream(resolvedPath, FileMode.Append, FileAccess.Write,
            FileShare.Read, bufferSize: 4096, useAsync: true);
        // UTF-8 without BOM — no invisible prefix bytes messing with parsers bestie
        _writer = new StreamWriter(stream, new System.Text.UTF8Encoding(false))
        {
            AutoFlush = false
        };

        // track current file size for size-based rotation
        _currentFileSize = new FileInfo(resolvedPath).Length;
        _currentFilePeriod = GetCurrentPeriod();
    }

    private async Task MaybeRotateAsync()
    {
        var shouldRotate = false;
        string? oldFilePath = null;

        // check size-based rotation
        if (_options.MaxFileSizeBytes > 0 && _currentFileSize >= _options.MaxFileSizeBytes)
            shouldRotate = true;

        // check time-based rotation
        if (_options.RollingInterval != LittyRollingInterval.None)
        {
            var currentPeriod = GetCurrentPeriod();
            if (currentPeriod != _currentFilePeriod)
                shouldRotate = true;
        }

        if (!shouldRotate)
            return;

        // close the current writer
        if (_writer is not null)
        {
            await _writer.FlushAsync();
            await _writer.DisposeAsync();
            _writer = null;
            oldFilePath = _currentFilePath;
        }

        // when no rolling interval, the new file gets the same path as the old one.
        // we gotta rename the old file first so the new one can take its place 🔄
        if (oldFilePath is not null && _options.RollingInterval == LittyRollingInterval.None)
        {
            var now = _options.UseUtcTimestamp ? DateTimeOffset.UtcNow : DateTimeOffset.Now;
            var dir = Path.GetDirectoryName(oldFilePath) ?? ".";
            var name = Path.GetFileNameWithoutExtension(oldFilePath);
            var ext = Path.GetExtension(oldFilePath);
            var counter = Interlocked.Increment(ref _rotationCounter);
            var rotatedPath = Path.Combine(dir, $"{name}-{now:yyyyMMdd-HHmmss}-{counter}{ext}");
            System.IO.File.Move(oldFilePath, rotatedPath);
            oldFilePath = rotatedPath;
        }

        // compress the old file if gzip is enabled 🗜️
        if (oldFilePath is not null && _options.CompressionMode == LittyCompressionMode.Gzip)
        {
            _ = Task.Run(() => CompressFileAsync(oldFilePath));
        }

        // open a new file
        await EnsureWriterAsync();
    }

    private string ResolveFilePath()
    {
        if (_options.RollingInterval == LittyRollingInterval.None)
            return _options.FilePath;

        var dir = Path.GetDirectoryName(_options.FilePath) ?? ".";
        var name = Path.GetFileNameWithoutExtension(_options.FilePath);
        var ext = Path.GetExtension(_options.FilePath);

        var now = _options.UseUtcTimestamp ? DateTimeOffset.UtcNow : DateTimeOffset.Now;
        var suffix = _options.RollingInterval switch
        {
            LittyRollingInterval.Daily => now.ToString("yyyyMMdd"),
            LittyRollingInterval.Hourly => now.ToString("yyyyMMdd-HH"),
            _ => ""
        };

        return Path.Combine(dir, $"{name}-{suffix}{ext}");
    }

    private DateTimeOffset GetCurrentPeriod()
    {
        var now = _options.UseUtcTimestamp ? DateTimeOffset.UtcNow : DateTimeOffset.Now;
        return _options.RollingInterval switch
        {
            LittyRollingInterval.Daily => new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, now.Offset),
            LittyRollingInterval.Hourly => new DateTimeOffset(now.Year, now.Month, now.Day, now.Hour, 0, 0, now.Offset),
            _ => DateTimeOffset.MinValue
        };
    }

    private static async Task CompressFileAsync(string filePath)
    {
        try
        {
            var gzPath = filePath + ".gz";
            await using var input = new FileStream(filePath, FileMode.Open, FileAccess.Read,
                FileShare.None, bufferSize: 4096, useAsync: true);
            await using var output = new FileStream(gzPath, FileMode.Create, FileAccess.Write,
                FileShare.None, bufferSize: 4096, useAsync: true);
            await using var gzip = new GZipStream(output, CompressionLevel.Optimal);
            await input.CopyToAsync(gzip);

            // yeet the uncompressed file now that we got the .gz 🗑️
            System.IO.File.Delete(filePath);
        }
        catch
        {
            // compression failed? no biggie, the uncompressed file is still there bestie 🤷
        }
    }

    public async ValueTask DisposeAsync()
    {
        // complete the channel — ReadAllAsync will finish once all items are read 🔒
        _channel.Writer.Complete();

        // wait for the consumer to drain everything and close the file
        await _consumerTask;

        _cts.Dispose();
    }

    public void Dispose()
    {
        // complete the channel — ReadAllAsync will finish once all items are read 🔒
        _channel.Writer.Complete();

        // wait for the consumer to drain everything and close the file
        _consumerTask.GetAwaiter().GetResult();

        _cts.Dispose();
    }
}
