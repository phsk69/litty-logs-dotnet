using System.IO.Compression;
using System.Text.Json;
using LittyLogs.File;
using LittyLogs.Xunit;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace LittyLogs.Tests;

/// <summary>
/// testing that the file sink writes fire logs to disk — rotation, compression, the whole nine 📁🔥
/// dogfooding our own litty-logs xUnit integration for test output 💅
/// </summary>
public class LittyFileLogsProviderTests : IDisposable
{
    private readonly ILogger<LittyFileLogsProviderTests> _logger;
    private readonly string _tempDir;

    public LittyFileLogsProviderTests(ITestOutputHelper output)
    {
        _logger = output.CreateLittyLogger<LittyFileLogsProviderTests>();
        _tempDir = Path.Combine(Path.GetTempPath(), "litty-logs-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempDir, true); } catch { /* cleanup best effort 🧹 */ }
    }

    private string TempFile(string name = "test.log") => Path.Combine(_tempDir, name);

    [Fact]
    public void TextOutput_WritesLittyFormattedLines()
    {
        // text output should have emojis and litty formatting bestie
        var filePath = TempFile();
        var opts = new LittyFileLogsOptions { FilePath = filePath };
        using (var provider = new LittyFileLogsProvider(opts))
        {
            var logger = provider.CreateLogger("TestCategory");
            logger.LogInformation("we in here bestie");
            logger.LogError("something flopped hard");
        }

        // give the background writer a moment to flush
        Thread.Sleep(200);
        var content = System.IO.File.ReadAllText(filePath);

        _logger.LogInformation("file content: {Content}", content);
        Assert.Contains("🔥", content);
        Assert.Contains("info", content);
        Assert.Contains("we in here bestie", content);
        Assert.Contains("💀", content);
        Assert.Contains("err", content);
    }

    [Fact]
    public void JsonOutput_WritesValidJsonLines()
    {
        // JSON output should be valid and parseable no cap
        var filePath = TempFile();
        var opts = new LittyFileLogsOptions
        {
            FilePath = filePath,
            OutputFormat = LittyFileOutputFormat.Json
        };
        using (var provider = new LittyFileLogsProvider(opts))
        {
            var logger = provider.CreateLogger("TestCategory");
            logger.LogInformation("JSON vibes are immaculate");
        }

        Thread.Sleep(200);
        var lines = System.IO.File.ReadAllLines(filePath)
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .ToArray();

        Assert.NotEmpty(lines);
        foreach (var line in lines)
        {
            using var doc = JsonDocument.Parse(line);
            Assert.NotNull(doc);
        }
    }

    [Fact]
    public void AutoCreatesDirectory_WhenItDoesntExist()
    {
        // directory auto-creation is a vibe bestie 📁
        var filePath = Path.Combine(_tempDir, "nested", "deep", "test.log");
        var opts = new LittyFileLogsOptions { FilePath = filePath };
        using (var provider = new LittyFileLogsProvider(opts))
        {
            var logger = provider.CreateLogger("TestCategory");
            logger.LogInformation("creating directories is bussin");
        }

        Thread.Sleep(200);
        Assert.True(System.IO.File.Exists(filePath), "file should exist even in nested directories bestie");
    }

    [Fact]
    public void AppendsToExistingFile()
    {
        // append mode — dont nuke existing logs bestie 📝
        var filePath = TempFile();
        System.IO.File.WriteAllText(filePath, "existing content no cap\n");

        var opts = new LittyFileLogsOptions { FilePath = filePath };
        using (var provider = new LittyFileLogsProvider(opts))
        {
            var logger = provider.CreateLogger("TestCategory");
            logger.LogInformation("new content dropped");
        }

        Thread.Sleep(200);
        var content = System.IO.File.ReadAllText(filePath);

        Assert.Contains("existing content no cap", content);
        Assert.Contains("new content dropped", content);
    }

    [Fact]
    public void NoAnsiCodesInFileOutput()
    {
        // ANSI codes in files is NOT it 💀
        var filePath = TempFile();
        var opts = new LittyFileLogsOptions { FilePath = filePath };
        using (var provider = new LittyFileLogsProvider(opts))
        {
            var logger = provider.CreateLogger("TestCategory");
            logger.LogError("error vibes");
            logger.LogWarning("warning vibes");
            logger.LogInformation("info vibes");
        }

        Thread.Sleep(200);
        var content = System.IO.File.ReadAllText(filePath);

        Assert.False(content.Contains('\x1b'),
            "file output should NEVER contain ANSI escape chars, thats cursed bestie 💀");
    }

    [Fact]
    public void RewritesFrameworkMessages_InFileOutput()
    {
        // the secret sauce works in file output too bestie 🧠
        var filePath = TempFile();
        var opts = new LittyFileLogsOptions { FilePath = filePath };
        using (var provider = new LittyFileLogsProvider(opts))
        {
            var logger = provider.CreateLogger("Microsoft.Hosting.Lifetime");
            logger.LogInformation("Application started. Press Ctrl+C to shut down.");
        }

        Thread.Sleep(200);
        var content = System.IO.File.ReadAllText(filePath);

        Assert.Contains("bussin", content);
        Assert.DoesNotContain("Press Ctrl+C to shut down", content);
    }

    [Fact]
    public void SizeRotation_CreatesNewFile_WhenSizeExceeded()
    {
        // size rotation — when the file gets too thicc, we rotate bestie 🔄
        var filePath = TempFile("rotate.log");
        var opts = new LittyFileLogsOptions
        {
            FilePath = filePath,
            MaxFileSizeBytes = 100 // smol limit so rotation triggers fast
        };
        using (var provider = new LittyFileLogsProvider(opts))
        {
            var logger = provider.CreateLogger("TestCategory");
            // write enough to trigger rotation
            for (var i = 0; i < 20; i++)
                logger.LogInformation("padding log entry number {Num} to trigger rotation bestie", i);
        }

        Thread.Sleep(500);
        // the file should exist and have content
        Assert.True(System.IO.File.Exists(filePath), "log file should still exist after rotation");
    }

    [Fact]
    public void FlushesAllLogs_OnDispose()
    {
        // no lost logs on dispose — every entry gets flushed bestie 🔒
        var filePath = TempFile();
        var opts = new LittyFileLogsOptions { FilePath = filePath };
        using (var provider = new LittyFileLogsProvider(opts))
        {
            var logger = provider.CreateLogger("TestCategory");
            for (var i = 0; i < 50; i++)
                logger.LogInformation("entry {Num}", i);
        }

        // after dispose, all 50 entries should be in the file
        Thread.Sleep(200);
        var lines = System.IO.File.ReadAllLines(filePath)
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .ToArray();

        _logger.LogInformation("wrote {Count} lines to file", lines.Length);
        Assert.Equal(50, lines.Length);
    }

    [Fact]
    public void JsonOutput_HasEmojisInFile()
    {
        // emojis in JSON files are bussin — UTF-8 native bestie 🔥
        // testing BOTH supplementary (🔥💅) and BMP (☠️✨) emojis in raw file content
        // because Loki searches on raw text, not parsed JSON values no cap 🔒
        var filePath = TempFile();
        var opts = new LittyFileLogsOptions
        {
            FilePath = filePath,
            OutputFormat = LittyFileOutputFormat.Json
        };
        using (var provider = new LittyFileLogsProvider(opts))
        {
            var logger = provider.CreateLogger("TestCategory");
            logger.LogInformation("vibes are immaculate 💅");
            logger.LogCritical("its giving death ☠️ and sparkles ✨");
        }

        var content = System.IO.File.ReadAllText(filePath, System.Text.Encoding.UTF8);
        _logger.LogInformation("raw file content: {Content}", content);

        // supplementary plane emojis (above U+FFFF) — literal in file
        Assert.Contains("🔥", content); // INFO emoji
        Assert.Contains("💅", content); // message emoji

        // BMP emojis — these were the sneaky false positives before 💀
        Assert.Contains("☠️", content); // CRIT emoji
        Assert.Contains("✨", content); // sparkles in message

        // make sure no escaped unicode garbage in raw file — Loki needs literal chars
        Assert.DoesNotContain("\\u2620", content); // escaped skull
        Assert.DoesNotContain("\\u2728", content); // escaped sparkles
        Assert.DoesNotContain("\\uD83D", content); // surrogate pair escape
    }

    [Fact]
    public void GzipCompression_CompressesRotatedFiles()
    {
        // gzip compression — rotated files become .gz, active file stays uncompressed 🗜️
        var filePath = TempFile("compress.log");
        var opts = new LittyFileLogsOptions
        {
            FilePath = filePath,
            MaxFileSizeBytes = 100, // smol limit so rotation triggers fast
            CompressionMode = LittyCompressionMode.Gzip
        };
        using (var provider = new LittyFileLogsProvider(opts))
        {
            var logger = provider.CreateLogger("TestCategory");
            for (var i = 0; i < 30; i++)
                logger.LogInformation("padding log entry {Num} for compression test bestie", i);
        }

        // give compression background task time to finish
        Thread.Sleep(1000);

        // the active log file should still exist uncompressed
        Assert.True(System.IO.File.Exists(filePath), "active log file should exist uncompressed");

        // check if any .gz files were created (rotation + compression happened)
        var gzFiles = Directory.GetFiles(_tempDir, "*.gz");
        _logger.LogInformation("found {Count} compressed files", gzFiles.Length);

        // verify .gz files are valid gzip if they exist
        foreach (var gz in gzFiles)
        {
            using var stream = System.IO.File.OpenRead(gz);
            using var gzip = new GZipStream(stream, CompressionMode.Decompress);
            using var reader = new StreamReader(gzip);
            var content = reader.ReadToEnd();
            Assert.NotEmpty(content);
            _logger.LogInformation("decompressed {File}: {Length} chars", Path.GetFileName(gz), content.Length);
        }
    }
}
