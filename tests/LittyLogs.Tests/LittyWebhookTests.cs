using System.Collections.Concurrent;
using System.Net;
using System.Text.Json;
using LittyLogs.Webhooks;
using LittyLogs.Webhooks.Formatters;
using LittyLogs.Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace LittyLogs.Tests;

/// <summary>
/// making sure webhook logs are getting yeeted to chat properly fr fr 🪝🔥
/// </summary>
public class LittyWebhookTests
{
    private readonly ILogger<LittyWebhookTests> _logger;

    public LittyWebhookTests(ITestOutputHelper output)
    {
        _logger = output.CreateLittyLogger<LittyWebhookTests>();
    }

    // ===============================
    // FORMATTER TESTS — the JSON output 📝
    // ===============================

    [Fact]
    public void MatrixFormatter_SingleMessage_ProducesValidJson()
    {
        var formatter = new MatrixPayloadFormatter();
        var options = new LittyWebhookOptions { Username = "LittyLogs" };
        var messages = new List<string> { "[🔥 info] [2026-02-21T12:00:00.000Z] [MyApp] we vibing bestie" };

        var payload = formatter.FormatPayload(messages, options);

        var doc = JsonDocument.Parse(payload);
        Assert.Equal("LittyLogs", doc.RootElement.GetProperty("username").GetString());
        Assert.Contains("vibing", doc.RootElement.GetProperty("text").GetString());
        _logger.LogInformation("matrix formatter producing valid JSON no cap 🔥");
    }

    [Fact]
    public void MatrixFormatter_MultipleMessages_JoinsWithNewlines()
    {
        var formatter = new MatrixPayloadFormatter();
        var options = new LittyWebhookOptions();
        var messages = new List<string>
        {
            "[💀 err] first error happened bestie",
            "[💀 err] second error also not bussin"
        };

        var payload = formatter.FormatPayload(messages, options);

        var doc = JsonDocument.Parse(payload);
        var text = doc.RootElement.GetProperty("text").GetString();
        Assert.Contains("first error", text);
        Assert.Contains("second error", text);
        Assert.Contains("\n", text);
        _logger.LogInformation("batch messages joined with newlines 💅");
    }

    [Fact]
    public void MatrixFormatter_EmptyUsername_OmitsField()
    {
        var formatter = new MatrixPayloadFormatter();
        var options = new LittyWebhookOptions { Username = "" };
        var messages = new List<string> { "test message" };

        var payload = formatter.FormatPayload(messages, options);

        var doc = JsonDocument.Parse(payload);
        Assert.False(doc.RootElement.TryGetProperty("username", out _));
    }

    [Fact]
    public void MatrixFormatter_EmojisInMessages_SurviveSerialization()
    {
        var formatter = new MatrixPayloadFormatter();
        var options = new LittyWebhookOptions();
        var messages = new List<string> { "💀 big L — database is cooked fr fr 🔥" };

        var payload = formatter.FormatPayload(messages, options);

        var doc = JsonDocument.Parse(payload);
        var text = doc.RootElement.GetProperty("text").GetString();
        Assert.Contains("💀", text);
        Assert.Contains("🔥", text);
        _logger.LogInformation("emojis surviving JSON serialization like champs 🏆");
    }

    [Fact]
    public void TeamsFormatter_ThrowsNotImplemented()
    {
        var formatter = new TeamsPayloadFormatter();
        var options = new LittyWebhookOptions();

        Assert.Throws<NotImplementedException>(() =>
            formatter.FormatPayload(["test"], options));
        _logger.LogInformation("teams stub correctly throwing no cap 🟦");
    }

    // ===============================
    // OPTIONS TESTS — the defaults 🎛️
    // ===============================

    [Fact]
    public void WebhookOptions_DefaultMinimumLevel_IsWarning()
    {
        var options = new LittyWebhookOptions();
        Assert.Equal(LogLevel.Warning, options.MinimumLevel);
    }

    [Fact]
    public void WebhookOptions_DefaultBatchSize_IsTen()
    {
        var options = new LittyWebhookOptions();
        Assert.Equal(10, options.BatchSize);
    }

    [Fact]
    public void WebhookOptions_DefaultBatchInterval_IsTwoSeconds()
    {
        var options = new LittyWebhookOptions();
        Assert.Equal(TimeSpan.FromSeconds(2), options.BatchInterval);
    }

    [Fact]
    public void WebhookOptions_DefaultPlatform_IsMatrix()
    {
        var options = new LittyWebhookOptions();
        Assert.Equal(WebhookPlatform.Matrix, options.Platform);
    }

    [Fact]
    public void WebhookOptions_DefaultUsername_IsLittyLogs()
    {
        var options = new LittyWebhookOptions();
        Assert.Equal("LittyLogs", options.Username);
    }

    [Fact]
    public void WebhookOptions_ToLittyLogsOptions_DisablesColors()
    {
        var options = new LittyWebhookOptions();
        var littyOptions = options.ToLittyLogsOptions();

        Assert.False(littyOptions.UseColors);
        _logger.LogInformation("ANSI in webhooks is NOT it confirmed 💀");
    }

    [Fact]
    public void WebhookOptions_ToLittyLogsOptions_PreservesAllSettings()
    {
        var options = new LittyWebhookOptions
        {
            RewriteMessages = false,
            ShortenCategories = false,
            UseUtcTimestamp = false,
            TimestampFirst = true,
            TimestampFormat = "HH:mm:ss"
        };
        var littyOptions = options.ToLittyLogsOptions();

        Assert.False(littyOptions.RewriteMessages);
        Assert.False(littyOptions.ShortenCategories);
        Assert.False(littyOptions.UseUtcTimestamp);
        Assert.True(littyOptions.TimestampFirst);
        Assert.Equal("HH:mm:ss", littyOptions.TimestampFormat);
        Assert.False(littyOptions.UseColors); // always false no cap
    }

    // ===============================
    // LOGGER TESTS — the ILogger layer 🔥
    // ===============================

    [Fact]
    public void Logger_IsEnabled_RespectsMinimumLevel()
    {
        var (logger, _, _) = CreateTestLogger(LogLevel.Warning);

        Assert.True(logger.IsEnabled(LogLevel.Warning));
        Assert.True(logger.IsEnabled(LogLevel.Error));
        Assert.True(logger.IsEnabled(LogLevel.Critical));
        Assert.False(logger.IsEnabled(LogLevel.Information));
        Assert.False(logger.IsEnabled(LogLevel.Debug));
        Assert.False(logger.IsEnabled(LogLevel.Trace));
        Assert.False(logger.IsEnabled(LogLevel.None));
        _logger.LogInformation("min level filtering slaying as expected 🔒");
    }

    [Fact]
    public void Logger_IsEnabled_AllLevelsWhenMinIsTrace()
    {
        var (logger, _, _) = CreateTestLogger(LogLevel.Trace);

        Assert.True(logger.IsEnabled(LogLevel.Trace));
        Assert.True(logger.IsEnabled(LogLevel.Debug));
        Assert.True(logger.IsEnabled(LogLevel.Information));
        Assert.True(logger.IsEnabled(LogLevel.Warning));
        Assert.True(logger.IsEnabled(LogLevel.Error));
        Assert.True(logger.IsEnabled(LogLevel.Critical));
    }

    [Fact]
    public async Task Logger_WarningMessage_ReachesWriter()
    {
        var (logger, writer, capturedRequests) = CreateTestLogger(LogLevel.Warning);

        logger.LogWarning("something sus happened bestie");

        // wait for batch interval flush (100ms interval + buffer)
        await Task.Delay(300);
        await writer.DisposeAsync();

        Assert.True(capturedRequests.Count >= 1, "warning should have been flushed to HTTP");
        var payload = capturedRequests.First();
        Assert.Contains("sus", payload);
        _logger.LogInformation("warning message landed in the writer 🔥");
    }

    [Fact]
    public async Task Logger_InfoMessage_FilteredWhenMinIsWarning()
    {
        var (logger, writer, capturedRequests) = CreateTestLogger(LogLevel.Warning);

        logger.LogInformation("this should get filtered fr fr");

        // wait for batch interval flush
        await Task.Delay(300);
        await writer.DisposeAsync();

        Assert.Empty(capturedRequests);
        _logger.LogInformation("info correctly filtered when min=warning 💅");
    }

    [Fact]
    public async Task Logger_ErrorWithException_GetsMarkdownCodeBlock()
    {
        var (logger, writer, capturedRequests) = CreateTestLogger(LogLevel.Warning);

        try
        {
            throw new InvalidOperationException("database is cooked fr fr");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "something is mega bricked");
        }

        // wait for batch interval flush
        await Task.Delay(300);
        await writer.DisposeAsync();

        Assert.True(capturedRequests.Count >= 1, "error with exception should reach HTTP");
        var payload = capturedRequests.First();
        Assert.Contains("```", payload);
        Assert.Contains("database is cooked", payload);
        _logger.LogInformation("exceptions get markdown code blocks 📝");
    }

    [Fact]
    public async Task Logger_FrameworkMessage_GetsRewritten()
    {
        var (logger, writer, capturedRequests) = CreateTestLogger(LogLevel.Warning, rewrite: true);

        // use a message that LittyMessageRewriter knows about
        logger.LogWarning("Application is shutting down...");

        // wait for batch interval flush
        await Task.Delay(300);
        await writer.DisposeAsync();

        Assert.True(capturedRequests.Count >= 1, "framework message should reach HTTP");
        var payload = capturedRequests.First();
        // rewritten version should have "head out" instead of the boring original
        Assert.Contains("head out", payload);
        _logger.LogInformation("framework messages rewritten in webhook context 🔥");
    }

    [Fact]
    public void Logger_BeginScope_ReturnsNull()
    {
        var (logger, _, _) = CreateTestLogger(LogLevel.Warning);

        var scope = logger.BeginScope("test scope");

        Assert.Null(scope);
    }

    [Fact]
    public async Task Logger_NoColors_InFormattedOutput()
    {
        var (logger, writer, capturedRequests) = CreateTestLogger(LogLevel.Warning);

        logger.LogWarning("testing no ANSI codes");

        // wait for batch interval flush
        await Task.Delay(300);
        await writer.DisposeAsync();

        Assert.True(capturedRequests.Count >= 1, "warning should reach HTTP");
        var payload = capturedRequests.First();
        // parse the JSON and check the actual text field for ESC character (0x1B)
        // ANSI escape sequences start with ESC — should NOT be in webhook output
        var doc = JsonDocument.Parse(payload);
        var text = doc.RootElement.GetProperty("text").GetString()!;
        // use char overload for guaranteed ordinal comparison no cap
        Assert.False(text.Contains('\x1b'), $"webhook text should not contain ESC (0x1B) but found it: {text}");
        _logger.LogInformation("no ANSI in webhook output confirmed 💀");
    }

    // ===============================
    // PROVIDER TESTS — the ILoggerProvider layer 🏗️
    // ===============================

    [Fact]
    public void Provider_CreateLogger_ReturnsLoggerWithCategory()
    {
        var (mockFactory, _) = CreateMockHttpClientFactory();
        var options = new LittyWebhookOptions
        {
            WebhookUrl = "http://localhost/test",
            BatchInterval = TimeSpan.FromMilliseconds(100)
        };
        var writer = new LittyWebhookWriter(mockFactory, new MatrixPayloadFormatter(), options);
        var provider = new LittyWebhookProvider(writer, options);

        var logger = provider.CreateLogger("MyApp.Services.UserService");

        Assert.NotNull(logger);
        Assert.IsType<LittyWebhookLogger>(logger);
        provider.Dispose();
    }

    [Fact]
    public void Provider_MultipleCreateLogger_ReturnsIndependentLoggers()
    {
        var (mockFactory, _) = CreateMockHttpClientFactory();
        var options = new LittyWebhookOptions
        {
            WebhookUrl = "http://localhost/test",
            BatchInterval = TimeSpan.FromMilliseconds(100)
        };
        var writer = new LittyWebhookWriter(mockFactory, new MatrixPayloadFormatter(), options);
        var provider = new LittyWebhookProvider(writer, options);

        var logger1 = provider.CreateLogger("Category1");
        var logger2 = provider.CreateLogger("Category2");

        Assert.NotNull(logger1);
        Assert.NotNull(logger2);
        Assert.NotSame(logger1, logger2);
        provider.Dispose();
    }

    // ===============================
    // WRITER TESTS — the batching engine with mock HTTP 🪝
    // ===============================

    [Fact]
    public async Task Writer_BatchFlushesOnSize()
    {
        var (mockFactory, capturedRequests) = CreateMockHttpClientFactory();
        var options = new LittyWebhookOptions
        {
            WebhookUrl = "http://localhost/webhook",
            BatchSize = 3,
            BatchInterval = TimeSpan.FromSeconds(30), // long interval so size triggers first
        };
        var writer = new LittyWebhookWriter(mockFactory, new MatrixPayloadFormatter(), options);

        // enqueue exactly BatchSize messages
        for (var i = 0; i < 3; i++)
            writer.Enqueue(new WebhookMessage($"message {i}"));

        // give the consumer a moment to flush
        await Task.Delay(500);

        Assert.True(capturedRequests.Count >= 1, "should have flushed at least one batch");

        var payload = capturedRequests.First();
        var doc = JsonDocument.Parse(payload);
        var text = doc.RootElement.GetProperty("text").GetString()!;
        Assert.Contains("message 0", text);
        Assert.Contains("message 2", text);

        await writer.DisposeAsync();
        _logger.LogInformation("batch flushed on size threshold 📦");
    }

    [Fact]
    public async Task Writer_BatchFlushesOnInterval()
    {
        var (mockFactory, capturedRequests) = CreateMockHttpClientFactory();
        var options = new LittyWebhookOptions
        {
            WebhookUrl = "http://localhost/webhook",
            BatchSize = 100, // high size so interval triggers first
            BatchInterval = TimeSpan.FromMilliseconds(200),
        };
        var writer = new LittyWebhookWriter(mockFactory, new MatrixPayloadFormatter(), options);

        // enqueue fewer than BatchSize
        writer.Enqueue(new WebhookMessage("lonely message"));

        // wait longer than BatchInterval
        await Task.Delay(600);

        Assert.True(capturedRequests.Count >= 1, "should have flushed on interval timeout");

        var payload = capturedRequests.First();
        Assert.Contains("lonely message", payload);

        await writer.DisposeAsync();
        _logger.LogInformation("batch flushed on interval timeout ⏱️");
    }

    [Fact]
    public async Task Writer_DisposalDrainsRemainingMessages()
    {
        var (mockFactory, capturedRequests) = CreateMockHttpClientFactory();
        var options = new LittyWebhookOptions
        {
            WebhookUrl = "http://localhost/webhook",
            BatchSize = 100,
            BatchInterval = TimeSpan.FromSeconds(30), // long so it doesnt trigger
        };
        var writer = new LittyWebhookWriter(mockFactory, new MatrixPayloadFormatter(), options);

        // enqueue without waiting for flush
        writer.Enqueue(new WebhookMessage("drain me bestie"));

        // dispose should drain remaining
        await writer.DisposeAsync();

        // the final flush should have captured our message
        var allPayloads = string.Join(" ", capturedRequests);
        Assert.Contains("drain me bestie", allPayloads);
        _logger.LogInformation("disposal drains remaining messages 🔒");
    }

    [Fact]
    public async Task Writer_HttpFailure_DoesNotCrash()
    {
        var (mockFactory, _) = CreateMockHttpClientFactory(HttpStatusCode.InternalServerError);
        var options = new LittyWebhookOptions
        {
            WebhookUrl = "http://localhost/webhook",
            BatchSize = 1,
            BatchInterval = TimeSpan.FromMilliseconds(100),
        };
        var writer = new LittyWebhookWriter(mockFactory, new MatrixPayloadFormatter(), options);

        // enqueue a message — webhook will return 500
        writer.Enqueue(new WebhookMessage("this will fail but we keep vibing"));

        // should not throw
        await Task.Delay(300);
        await writer.DisposeAsync();

        _logger.LogInformation("HTTP 500 didnt crash the writer, best effort delivery 💪");
    }

    [Fact]
    public async Task Writer_HighVolume_DoesNotOOM()
    {
        var (mockFactory, capturedRequests) = CreateMockHttpClientFactory();
        var options = new LittyWebhookOptions
        {
            WebhookUrl = "http://localhost/webhook",
            BatchSize = 100,
            BatchInterval = TimeSpan.FromMilliseconds(50),
        };
        var writer = new LittyWebhookWriter(mockFactory, new MatrixPayloadFormatter(), options);

        // flood with messages — channel is bounded at 10k with DropOldest
        for (var i = 0; i < 15_000; i++)
            writer.Enqueue(new WebhookMessage($"flood message {i}"));

        // give it time to process
        await Task.Delay(1000);
        await writer.DisposeAsync();

        // should have processed some batches without crashing
        Assert.True(capturedRequests.Count > 0, "should have flushed at least some batches");
        _logger.LogInformation("15k messages flooded without OOM 🌊");
    }

    // ===============================
    // INTEGRATION TESTS — end to end 🔗
    // ===============================

    [Fact]
    public async Task Integration_LoggerFactory_WarningReachesMockHttp()
    {
        var (mockFactory, capturedRequests) = CreateMockHttpClientFactory();

        // register the webhook provider manually (simulating what AddLittyMatrixLogs does)
        var options = new LittyWebhookOptions
        {
            WebhookUrl = "http://localhost/webhook",
            MinimumLevel = LogLevel.Warning,
            BatchSize = 1,
            BatchInterval = TimeSpan.FromMilliseconds(100),
        };
        var formatter = new MatrixPayloadFormatter();
        var writer = new LittyWebhookWriter(mockFactory, formatter, options);
        var provider = new LittyWebhookProvider(writer, options);

        using var factory = LoggerFactory.Create(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Trace);
            builder.AddProvider(provider);
        });

        var logger = factory.CreateLogger("IntegrationTest");

        // this should be filtered (Info < Warning)
        logger.LogInformation("this should not reach the webhook");
        // this should go through
        logger.LogWarning("this warning is bussin and should hit the webhook fr fr");

        // wait for batch to flush
        await Task.Delay(500);

        Assert.True(capturedRequests.Count >= 1, "warning should have reached the webhook");
        var allPayloads = string.Join(" ", capturedRequests);
        Assert.Contains("bussin", allPayloads);
        Assert.DoesNotContain("should not reach", allPayloads);
        _logger.LogInformation("end-to-end integration test passed 🔥🔥🔥");
    }

    [Fact]
    public async Task Integration_CustomUsername_AppearsInPayload()
    {
        var (mockFactory, capturedRequests) = CreateMockHttpClientFactory();
        var options = new LittyWebhookOptions
        {
            WebhookUrl = "http://localhost/webhook",
            Username = "CriticalAlerts",
            MinimumLevel = LogLevel.Warning,
            BatchSize = 1,
            BatchInterval = TimeSpan.FromMilliseconds(100),
        };
        var writer = new LittyWebhookWriter(mockFactory, new MatrixPayloadFormatter(), options);
        var provider = new LittyWebhookProvider(writer, options);

        using var factory = LoggerFactory.Create(b =>
        {
            b.SetMinimumLevel(LogLevel.Trace);
            b.AddProvider(provider);
        });

        factory.CreateLogger("Test").LogError("something is cooked");
        await Task.Delay(500);

        Assert.True(capturedRequests.Count >= 1);
        var doc = JsonDocument.Parse(capturedRequests.First());
        Assert.Equal("CriticalAlerts", doc.RootElement.GetProperty("username").GetString());
        _logger.LogInformation("custom username in payload confirmed 🤖");
    }

    // ===============================
    // URL VALIDATION — SSRF prevention bestie 🔒
    // ===============================

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not-a-url-at-all")]
    [InlineData("file:///etc/passwd")]
    [InlineData("ftp://sketchy.server/exfil")]
    [InlineData("gopher://ancient.protocol")]
    public void WebhookRegistration_RejectsInvalidUrls(string badUrl)
    {
        var services = new ServiceCollection();
        services.AddLogging(logging =>
        {
            Assert.Throws<ArgumentException>(() =>
                logging.AddLittyWebhookLogs(opts => opts.WebhookUrl = badUrl));
        });
        _logger.LogInformation($"rejected '{badUrl}' — SSRF blocked no cap 🔒");
    }

    [Theory]
    [InlineData("https://hookshot.example.com/webhook/abc123")]
    [InlineData("http://localhost:9000/webhook")]
    [InlineData("https://matrix.example.org/hookshot/hook/secret-token-here")]
    public void WebhookRegistration_AcceptsValidUrls(string goodUrl)
    {
        // should not throw — these are legit webhook URLs bestie
        var services = new ServiceCollection();
        services.AddLogging(logging =>
            logging.AddLittyWebhookLogs(opts => opts.WebhookUrl = goodUrl));
        _logger.LogInformation($"accepted '{goodUrl}' — valid URL goes brrr 🔥");
    }

    // ===============================
    // HTML ENCODING — WebUtility.HtmlEncode() handles injection, hookshot uses html field 🔒
    // no more cursed custom markdown sanitizer — stdlib ate and left no crumbs 💅
    // ===============================

    [Fact]
    public async Task Payload_ContainsHtmlField_HookshotPrefersIt()
    {
        // hookshot docs say: when html field is present, it takes priority over text for rendering
        // we send both for maximum compatibility no cap 🔥
        var (logger, writer, capturedRequests) = CreateTestLogger(LogLevel.Warning);

        logger.LogWarning("yo something sus just happened bestie");

        await Task.Delay(300);
        await writer.DisposeAsync();

        Assert.True(capturedRequests.Count >= 1, "warning should reach HTTP");
        var doc = JsonDocument.Parse(capturedRequests.First());
        // both fields present
        Assert.True(doc.RootElement.TryGetProperty("text", out _), "text field should exist as fallback");
        Assert.True(doc.RootElement.TryGetProperty("html", out _), "html field should exist for rendering");
        _logger.LogInformation("payload has both text and html fields — hookshot prefers html 🔥");
    }

    [Fact]
    public async Task Payload_MultipleBatchedMessages_HtmlHasLineBreaks()
    {
        // THE fix for messages mashed into one line — html field uses <br/> between messages
        // this is the whole point of the v0.2.1 hotfix bestie 🔥
        var (logger, writer, capturedRequests) = CreateTestLogger(LogLevel.Warning);

        logger.LogWarning("first warning bestie");
        logger.LogError("second one is an L");
        logger.LogCritical("third one we are SO cooked");

        await Task.Delay(300);
        await writer.DisposeAsync();

        Assert.True(capturedRequests.Count >= 1, "messages should batch together");
        var doc = JsonDocument.Parse(capturedRequests.First());
        var html = doc.RootElement.GetProperty("html").GetString()!;
        var text = doc.RootElement.GetProperty("text").GetString()!;
        // html: messages separated by <br/> for proper line breaks in hookshot
        Assert.Contains("<br/>", html);
        // text: paragraph breaks (\n\n) for markdown fallback
        Assert.Contains("\n\n", text);
        _logger.LogInformation("batched messages have proper line breaks in both fields 🔥");
    }

    [Fact]
    public async Task HtmlField_HtmlEncodesInjectionAttempts()
    {
        // WebUtility.HtmlEncode() handles ALL injection — XSS, tracking pixels, phishing links
        // no custom sanitizer needed when the stdlib already solved this bestie 💅
        var (logger, writer, capturedRequests) = CreateTestLogger(LogLevel.Warning);

        logger.LogWarning("xss attempt <script>alert('pwned')</script> bestie");

        await Task.Delay(300);
        await writer.DisposeAsync();

        Assert.True(capturedRequests.Count >= 1, "warning should reach HTTP");
        var doc = JsonDocument.Parse(capturedRequests.First());
        var html = doc.RootElement.GetProperty("html").GetString()!;
        // script tag should be HTML-encoded, not executable
        Assert.DoesNotContain("<script>", html);
        Assert.Contains("&lt;script&gt;", html);
        _logger.LogInformation("XSS injection neutralized by HtmlEncode — stdlib ate 🔒");
    }

    [Fact]
    public async Task HtmlField_MarkdownLink_NeutralizedByHtmlEncoding()
    {
        // phishing link attempt — HtmlEncode turns [text](url) into literal text
        // no clickable links in hookshot when rendered from html field 🔒
        var (logger, writer, capturedRequests) = CreateTestLogger(LogLevel.Warning);

        logger.LogWarning("click [totally legit](https://phishing.example.com/steal) fr");

        await Task.Delay(300);
        await writer.DisposeAsync();

        Assert.True(capturedRequests.Count >= 1, "warning should reach HTTP");
        var doc = JsonDocument.Parse(capturedRequests.First());
        var html = doc.RootElement.GetProperty("html").GetString()!;
        // the whole thing is HTML-encoded — renders as literal text, not a clickable link
        Assert.DoesNotContain("href", html);
        Assert.DoesNotContain("<a ", html);
        _logger.LogInformation("markdown link injection neutralized via HtmlEncode 🔒");
    }

    [Fact]
    public async Task HtmlField_MarkdownImage_NeutralizedByHtmlEncoding()
    {
        // tracking pixel attempt — HtmlEncode prevents image rendering in hookshot 🔒
        var (logger, writer, capturedRequests) = CreateTestLogger(LogLevel.Warning);

        logger.LogWarning("check this out ![tracking](https://evil.com/pixel.png) bestie");

        await Task.Delay(300);
        await writer.DisposeAsync();

        Assert.True(capturedRequests.Count >= 1, "warning should reach HTTP");
        var doc = JsonDocument.Parse(capturedRequests.First());
        var html = doc.RootElement.GetProperty("html").GetString()!;
        // no <img> tags — everything is HTML-encoded literal text
        Assert.DoesNotContain("<img", html);
        Assert.Contains("![tracking]", html); // literal text, not rendered
        _logger.LogInformation("markdown image injection neutralized via HtmlEncode — no tracking pixels 🔒");
    }

    [Fact]
    public async Task HtmlField_ExceptionCodeBlock_RendersAsPreCode()
    {
        // exceptions get wrapped in <pre><code> for proper monospace rendering in hookshot
        // the code fences (```) from the logger get converted to HTML in the formatter 🔥
        var (logger, writer, capturedRequests) = CreateTestLogger(LogLevel.Warning);

        try
        {
            throw new InvalidOperationException("database absolutely cooked");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "something bricked");
        }

        await Task.Delay(300);
        await writer.DisposeAsync();

        Assert.True(capturedRequests.Count >= 1, "error with exception should reach HTTP");
        var doc = JsonDocument.Parse(capturedRequests.First());
        var html = doc.RootElement.GetProperty("html").GetString()!;
        var text = doc.RootElement.GetProperty("text").GetString()!;
        // html: exception in <pre><code> block
        Assert.Contains("<pre><code>", html);
        Assert.Contains("</code></pre>", html);
        Assert.Contains("database absolutely cooked", html);
        // text: markdown code fences still present for fallback
        Assert.Contains("```", text);
        Assert.Contains("database absolutely cooked", text);
        _logger.LogInformation("exception code blocks render as <pre><code> in html field 🔥");
    }

    [Fact]
    public async Task TextFallback_ParagraphBreaks_BetweenMessages()
    {
        // text field uses \n\n (paragraph breaks) so even the markdown fallback renders properly
        // CommonMark spec: blank line = paragraph break = guaranteed separate blocks 💅
        var (logger, writer, capturedRequests) = CreateTestLogger(LogLevel.Warning);

        logger.LogWarning("first message");
        logger.LogError("second message");

        await Task.Delay(300);
        await writer.DisposeAsync();

        Assert.True(capturedRequests.Count >= 1, "messages should batch together");
        var doc = JsonDocument.Parse(capturedRequests.First());
        var text = doc.RootElement.GetProperty("text").GetString()!;
        // paragraph breaks between messages — not single \n that gets collapsed
        Assert.Contains("\n\n", text);
        Assert.DoesNotContain("first message" + "second message", text); // NOT mashed together
        _logger.LogInformation("text fallback has paragraph breaks between messages 💅");
    }

    // ===============================
    // HELPERS — mock HttpClient setup 🔧
    // ===============================

    private static (ILogger, LittyWebhookWriter, ConcurrentBag<string>) CreateTestLogger(
        LogLevel minLevel = LogLevel.Warning,
        bool rewrite = false)
    {
        var options = new LittyWebhookOptions
        {
            MinimumLevel = minLevel,
            RewriteMessages = rewrite,
            WebhookUrl = "http://localhost/test",
            BatchInterval = TimeSpan.FromMilliseconds(100),
        };
        var littyOptions = options.ToLittyLogsOptions();
        var (mockFactory, capturedRequests) = CreateMockHttpClientFactory();
        var writer = new LittyWebhookWriter(mockFactory, new MatrixPayloadFormatter(), options);
        var logger = new LittyWebhookLogger("TestCategory", writer, options, littyOptions);
        return (logger, writer, capturedRequests);
    }

    private static (IHttpClientFactory, ConcurrentBag<string>) CreateMockHttpClientFactory(
        HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        var capturedRequests = new ConcurrentBag<string>();

        var handler = new MockHttpMessageHandler(statusCode, capturedRequests);
        var httpClient = new HttpClient(handler);

        var mockFactory = new Mock<IHttpClientFactory>();
        mockFactory.Setup(f => f.CreateClient("LittyWebhooks")).Returns(httpClient);

        return (mockFactory.Object, capturedRequests);
    }
}

/// <summary>
/// mock HTTP handler that captures request bodies and returns configurable status codes.
/// lets us test the webhook writer without hitting real endpoints bestie 🧪
/// </summary>
internal class MockHttpMessageHandler(
    HttpStatusCode statusCode,
    ConcurrentBag<string> capturedRequests) : HttpMessageHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (request.Content is not null)
        {
            var body = await request.Content.ReadAsStringAsync(cancellationToken);
            capturedRequests.Add(body);
        }

        return new HttpResponseMessage(statusCode);
    }
}
