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
/// making sure every webhook payload stays safe, valid, and absolutely bussin fr fr 🪝🔥
/// </summary>
public class LittyWebhookTests
{
    private readonly ILogger<LittyWebhookTests> _logger;

    public LittyWebhookTests(ITestOutputHelper output)
    {
        _logger = output.CreateLittyLogger<LittyWebhookTests>();
    }

    [Fact]
    public void MatrixFormatter_SingleMessage_ProducesValidJson()
    {
        var payload = new MatrixPayloadFormatter().FormatPayload(
            ["[🔥 info] we vibing bestie"],
            new LittyWebhookOptions { Username = "LittyLogs" });

        using var doc = JsonDocument.Parse(payload);
        Assert.Equal("LittyLogs", doc.RootElement.GetProperty("username").GetString());
        Assert.Contains("vibing", doc.RootElement.GetProperty("text").GetString());
        Assert.True(doc.RootElement.TryGetProperty("html", out _));
    }

    [Fact]
    public void MatrixFormatter_MultipleMessages_UsesParagraphAndHtmlBreaks()
    {
        var payload = new MatrixPayloadFormatter().FormatPayload(
            ["[😤 warning] first", "[💀 err] second"],
            new LittyWebhookOptions());

        using var doc = JsonDocument.Parse(payload);
        Assert.Contains("\n\n", doc.RootElement.GetProperty("text").GetString());
        Assert.Contains("<br/>", doc.RootElement.GetProperty("html").GetString());
    }

    [Fact]
    public void MatrixFormatter_EmptyUsername_OmitsField()
    {
        var payload = new MatrixPayloadFormatter().FormatPayload(
            ["test message"],
            new LittyWebhookOptions { Username = "" });

        using var doc = JsonDocument.Parse(payload);
        Assert.False(doc.RootElement.TryGetProperty("username", out _));
    }

    [Fact]
    public void MatrixFormatter_EncodesHtmlButKeepsEmojis()
    {
        var payload = new MatrixPayloadFormatter().FormatPayload(
            ["<script>alert('cooked')</script> 💀🔥"],
            new LittyWebhookOptions());

        using var doc = JsonDocument.Parse(payload);
        var html = doc.RootElement.GetProperty("html").GetString()!;
        Assert.DoesNotContain("<script>", html);
        Assert.Contains("&lt;script&gt;", html);
        Assert.Contains("💀🔥", html);
    }

    [Fact]
    public void MatrixFormatter_ExceptionFence_BecomesPreCode()
    {
        const string message = "[💀 err] database cooked\n```\nSystem.InvalidOperationException: bruh\n```";

        var payload = new MatrixPayloadFormatter().FormatPayload([message], new LittyWebhookOptions());

        using var doc = JsonDocument.Parse(payload);
        var html = doc.RootElement.GetProperty("html").GetString()!;
        Assert.Contains("<pre><code>", html);
        Assert.Contains("bruh", html);
        Assert.Contains("```", doc.RootElement.GetProperty("text").GetString());
    }

    [Fact]
    public void SlackFormatter_ProducesSafeBlockKitAndFallback()
    {
        var payload = new SlackPayloadFormatter().FormatPayload(
            ["[😤 warning] something sus"],
            new LittyWebhookOptions { Username = "CriticalAlerts" });

        using var doc = JsonDocument.Parse(payload);
        var root = doc.RootElement;
        Assert.False(root.GetProperty("mrkdwn").GetBoolean());
        Assert.Contains("something sus", root.GetProperty("text").GetString());

        var blocks = root.GetProperty("blocks");
        Assert.Equal(2, blocks.GetArrayLength());
        Assert.Equal("header", blocks[0].GetProperty("type").GetString());
        Assert.Equal("🔥 CriticalAlerts", blocks[0].GetProperty("text").GetProperty("text").GetString());
        Assert.Equal("section", blocks[1].GetProperty("type").GetString());

        foreach (var block in blocks.EnumerateArray())
        {
            var text = block.GetProperty("text");
            Assert.Equal("plain_text", text.GetProperty("type").GetString());
            Assert.True(text.GetProperty("emoji").GetBoolean());
        }
    }

    [Fact]
    public void SlackFormatter_EmptyUsername_UsesDefaultHeader()
    {
        var payload = new SlackPayloadFormatter().FormatPayload(
            ["vibes"],
            new LittyWebhookOptions { Username = "  " });

        using var doc = JsonDocument.Parse(payload);
        var header = doc.RootElement.GetProperty("blocks")[0].GetProperty("text").GetProperty("text").GetString();
        Assert.Equal("🔥 LittyLogs", header);
    }

    [Fact]
    public void SlackFormatter_EachMessageGetsOnePlainTextSection()
    {
        var payload = new SlackPayloadFormatter().FormatPayload(
            ["first", "second", "third"],
            new LittyWebhookOptions());

        using var doc = JsonDocument.Parse(payload);
        var blocks = doc.RootElement.GetProperty("blocks");
        Assert.Equal(4, blocks.GetArrayLength());
        Assert.All(blocks.EnumerateArray().Skip(1), block =>
        {
            Assert.Equal("section", block.GetProperty("type").GetString());
            Assert.Equal("plain_text", block.GetProperty("text").GetProperty("type").GetString());
        });
    }

    [Fact]
    public void SlackFormatter_EmojisSurviveSerialization()
    {
        var payload = new SlackPayloadFormatter().FormatPayload(
            ["💀 database cooked but deploy recovered 🔥"],
            new LittyWebhookOptions());

        using var doc = JsonDocument.Parse(payload);
        var text = SlackSectionText(doc, 1);
        Assert.Contains("💀", text);
        Assert.Contains("🔥", text);
    }

    [Fact]
    public void SlackFormatter_ExceptionFencesDisappearButContentSurvives()
    {
        const string message = "[💀 err] database cooked\n```\nSystem.InvalidOperationException: bruh moment\n   at App.Run()\n```";

        var payload = new SlackPayloadFormatter().FormatPayload([message], new LittyWebhookOptions());

        using var doc = JsonDocument.Parse(payload);
        var text = SlackSectionText(doc, 1);
        Assert.DoesNotContain("```", text);
        Assert.Contains("database cooked", text);
        Assert.Contains("InvalidOperationException", text);
        Assert.Contains("App.Run", text);
    }

    [Fact]
    public void SlackFormatter_MarkdownAndMentionsStayLiteral()
    {
        const string attack = "<!channel> *pwned* <https://evil.example|click me> `oops`";

        var payload = new SlackPayloadFormatter().FormatPayload([attack], new LittyWebhookOptions());

        using var doc = JsonDocument.Parse(payload);
        Assert.False(doc.RootElement.GetProperty("mrkdwn").GetBoolean());
        var section = doc.RootElement.GetProperty("blocks")[1].GetProperty("text");
        Assert.Equal("plain_text", section.GetProperty("type").GetString());
        Assert.Equal(attack, section.GetProperty("text").GetString());
    }

    [Fact]
    public void SlackFormatter_TruncatesHeaderAt150UnicodeScalarsWithoutSplittingSurrogates()
    {
        var options = new LittyWebhookOptions { Username = string.Concat(Enumerable.Repeat("🔥", 200)) };

        var payload = new SlackPayloadFormatter().FormatPayload(["vibes"], options);

        using var doc = JsonDocument.Parse(payload);
        var header = doc.RootElement.GetProperty("blocks")[0].GetProperty("text").GetProperty("text").GetString()!;
        Assert.Equal(150, header.EnumerateRunes().Count());
        Assert.False(char.IsHighSurrogate(header[^1]));
    }

    [Fact]
    public void SlackFormatter_TruncatesSectionsAt3000UnicodeScalarsWithoutSplittingSurrogates()
    {
        var message = string.Concat(Enumerable.Repeat("🔥", 3_001));

        var payload = new SlackPayloadFormatter().FormatPayload([message], new LittyWebhookOptions());

        using var doc = JsonDocument.Parse(payload);
        var section = SlackSectionText(doc, 1);
        Assert.Equal(3_000, section.EnumerateRunes().Count());
        Assert.False(char.IsHighSurrogate(section[^1]));
    }

    [Fact]
    public void SlackFormatter_Allows49MessagesFor50TotalBlocks()
    {
        var messages = Enumerable.Range(1, 49).Select(index => $"message {index}").ToArray();

        var payload = new SlackPayloadFormatter().FormatPayload(messages, new LittyWebhookOptions());

        using var doc = JsonDocument.Parse(payload);
        Assert.Equal(50, doc.RootElement.GetProperty("blocks").GetArrayLength());
    }

    [Fact]
    public void SlackFormatter_RejectsMoreThan49Messages()
    {
        var messages = Enumerable.Range(1, 50).Select(index => $"message {index}").ToArray();

        var error = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new SlackPayloadFormatter().FormatPayload(messages, new LittyWebhookOptions()));

        Assert.Contains("49 logs", error.Message);
    }

    [Fact]
    public void WebhookOptions_DefaultsStayBussin()
    {
        var options = new LittyWebhookOptions();

        Assert.Equal(LogLevel.Warning, options.MinimumLevel);
        Assert.Equal(10, options.BatchSize);
        Assert.Equal(TimeSpan.FromSeconds(2), options.BatchInterval);
        Assert.Equal(WebhookPlatform.Matrix, options.Platform);
        Assert.Equal("LittyLogs", options.Username);
        Assert.False(options.ToLittyLogsOptions().UseColors);
    }

    [Fact]
    public void WebhookOptions_ToLittyLogsOptions_PreservesSettings()
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
        Assert.False(littyOptions.UseColors);
    }

    [Fact]
    public void Logger_IsEnabled_RespectsMinimumLevel()
    {
        var (logger, writer, _) = CreateTestLogger(LogLevel.Warning);

        Assert.False(logger.IsEnabled(LogLevel.Information));
        Assert.True(logger.IsEnabled(LogLevel.Warning));
        Assert.True(logger.IsEnabled(LogLevel.Critical));
        Assert.False(logger.IsEnabled(LogLevel.None));

        writer.Dispose();
    }

    [Fact]
    public async Task Logger_FiltersLowLevelsAndFlushesWarnings()
    {
        var (logger, writer, capturedRequests) = CreateTestLogger(LogLevel.Warning);

        logger.LogInformation("this quiet vibe stays local");
        logger.LogWarning("this warning gets yeeted 🔥");
        await writer.DisposeAsync();

        Assert.Single(capturedRequests);
        Assert.Contains("warning gets yeeted", capturedRequests.Single());
        Assert.DoesNotContain("quiet vibe", capturedRequests.Single());
    }

    [Fact]
    public async Task MatrixLogger_ExceptionAppearsOnceInsideFence()
    {
        var (logger, writer, capturedRequests) = CreateTestLogger(LogLevel.Warning);

        logger.LogError(new InvalidOperationException("database singularly cooked"), "query bricked");
        await writer.DisposeAsync();

        using var doc = JsonDocument.Parse(capturedRequests.Single());
        var text = doc.RootElement.GetProperty("text").GetString()!;
        Assert.Contains("```", text);
        Assert.Equal(1, text.Split("database singularly cooked").Length - 1);
    }

    [Fact]
    public async Task Logger_FrameworkMessageStillGetsRewritten()
    {
        var (logger, writer, capturedRequests) = CreateTestLogger(LogLevel.Warning, rewrite: true);

        logger.LogWarning("Application is shutting down...");
        await writer.DisposeAsync();

        Assert.Contains("head out", capturedRequests.Single());
    }

    [Fact]
    public async Task SlackLogger_ExceptionPipeline_RemovesFences()
    {
        var (logger, writer, capturedRequests) = CreateSlackTestLogger();

        logger.LogError(new InvalidOperationException("database mega cooked"), "query bricked");
        await writer.DisposeAsync();

        using var doc = JsonDocument.Parse(capturedRequests.Single());
        var text = SlackSectionText(doc, 1);
        Assert.Contains("query bricked", text);
        Assert.Contains("database mega cooked", text);
        Assert.DoesNotContain("```", text);
    }

    [Fact]
    public async Task SlackWriter_ClampsInternalBatchesTo49Messages()
    {
        var options = new LittyWebhookOptions
        {
            WebhookUrl = "http://localhost/test",
            Platform = WebhookPlatform.Slack,
            BatchSize = 99,
            BatchInterval = TimeSpan.FromMilliseconds(20)
        };
        var (mockFactory, capturedRequests) = CreateMockHttpClientFactory();
        var writer = new LittyWebhookWriter(mockFactory, new SlackPayloadFormatter(), options);

        foreach (var index in Enumerable.Range(1, 50))
            writer.Enqueue(new WebhookMessage($"message {index}"));

        await writer.DisposeAsync();

        Assert.Equal(2, capturedRequests.Count);
        var blockCounts = capturedRequests.Select(payload =>
        {
            using var doc = JsonDocument.Parse(payload);
            return doc.RootElement.GetProperty("blocks").GetArrayLength();
        }).Order().ToArray();
        Assert.Equal([2, 50], blockCounts);
    }

    [Theory]
    [InlineData("System.Net.Http.HttpClient.LittyWebhooks")]
    [InlineData("Microsoft.Extensions.Http.DefaultHttpClientFactory")]
    [InlineData("Polly.ResiliencePipeline")]
    public void Provider_FiltersItsOwnHttpPipeline(string category)
    {
        var options = new LittyWebhookOptions { WebhookUrl = "http://localhost/test" };
        var (mockFactory, _) = CreateMockHttpClientFactory();
        var writer = new LittyWebhookWriter(mockFactory, new MatrixPayloadFormatter(), options);
        using var provider = new LittyWebhookProvider(writer, options);

        Assert.IsType<Microsoft.Extensions.Logging.Abstractions.NullLogger>(provider.CreateLogger(category));
        Assert.IsType<LittyWebhookLogger>(provider.CreateLogger("MyApp.Worker"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not-a-url-at-all")]
    [InlineData("file:///etc/passwd")]
    [InlineData("ftp://sketchy.server/exfil")]
    public void WebhookRegistration_RejectsInvalidUrls(string badUrl)
    {
        var services = new ServiceCollection();

        Assert.Throws<ArgumentException>(() => services.AddLogging(logging =>
            logging.AddLittyWebhookLogs(options => options.WebhookUrl = badUrl)));
    }

    [Theory]
    [InlineData("https://hooks.example.test/services/secret")]
    [InlineData("http://localhost:9000/webhook")]
    public void SlackRegistration_AcceptsValidUrls(string goodUrl)
    {
        var services = new ServiceCollection();

        services.AddLogging(logging => logging.AddLittySlackLogs(goodUrl));

        Assert.NotEmpty(services);
    }

    [Fact]
    public void SlackRegistration_ConfigureOverloadAppliesOptions()
    {
        var services = new ServiceCollection();

        services.AddLogging(logging => logging.AddLittySlackLogs(
            "https://hooks.example.test/services/secret",
            options =>
            {
                options.MinimumLevel = LogLevel.Error;
                options.BatchSize = 49;
                options.Username = "DeployAlerts";
            }));

        Assert.NotEmpty(services);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(50)]
    public void SlackRegistration_RejectsUnsafeBatchSizes(int batchSize)
    {
        var services = new ServiceCollection();

        Assert.Throws<ArgumentOutOfRangeException>(() => services.AddLogging(logging =>
            logging.AddLittySlackLogs(
                "https://hooks.example.test/services/secret",
                options => options.BatchSize = batchSize)));
    }

    [Fact]
    public void GenericRegistration_SelectsSlackWithoutThrowing()
    {
        var services = new ServiceCollection();
        services.AddLogging(logging => logging.AddLittyWebhookLogs(options =>
        {
            options.WebhookUrl = "https://hooks.example.test/services/secret";
            options.Platform = WebhookPlatform.Slack;
        }));

        using var provider = services.BuildServiceProvider();
        Assert.NotNull(provider.GetService<ILoggerFactory>());
    }

    [Fact]
    public async Task Writer_HttpFailureNeverCrashesTheApp()
    {
        var options = new LittyWebhookOptions
        {
            WebhookUrl = "http://localhost/test",
            BatchSize = 1,
            BatchInterval = TimeSpan.FromMilliseconds(20)
        };
        var (mockFactory, capturedRequests) = CreateMockHttpClientFactory(HttpStatusCode.InternalServerError);
        var writer = new LittyWebhookWriter(mockFactory, new MatrixPayloadFormatter(), options);

        writer.Enqueue(new WebhookMessage("something cooked"));
        await writer.DisposeAsync();

        Assert.Single(capturedRequests);
        _logger.LogInformation("webhook HTTP failure stayed best-effort and app-safe 🔒🔥");
    }

    private static string SlackSectionText(JsonDocument doc, int index) => doc.RootElement
        .GetProperty("blocks")[index]
        .GetProperty("text")
        .GetProperty("text")
        .GetString()!;

    private static (ILogger, LittyWebhookWriter, ConcurrentBag<string>) CreateTestLogger(
        LogLevel minimumLevel = LogLevel.Warning,
        bool rewrite = false)
    {
        var options = new LittyWebhookOptions
        {
            MinimumLevel = minimumLevel,
            RewriteMessages = rewrite,
            WebhookUrl = "http://localhost/test",
            BatchInterval = TimeSpan.FromMilliseconds(20)
        };
        var (mockFactory, capturedRequests) = CreateMockHttpClientFactory();
        var writer = new LittyWebhookWriter(mockFactory, new MatrixPayloadFormatter(), options);
        var logger = new LittyWebhookLogger("TestCategory", writer, options, options.ToLittyLogsOptions());
        return (logger, writer, capturedRequests);
    }

    private static (ILogger, LittyWebhookWriter, ConcurrentBag<string>) CreateSlackTestLogger()
    {
        var options = new LittyWebhookOptions
        {
            MinimumLevel = LogLevel.Warning,
            RewriteMessages = false,
            WebhookUrl = "http://localhost/test",
            Platform = WebhookPlatform.Slack,
            BatchInterval = TimeSpan.FromMilliseconds(20)
        };
        var (mockFactory, capturedRequests) = CreateMockHttpClientFactory();
        var writer = new LittyWebhookWriter(mockFactory, new SlackPayloadFormatter(), options);
        var logger = new LittyWebhookLogger("TestCategory", writer, options, options.ToLittyLogsOptions());
        return (logger, writer, capturedRequests);
    }

    private static (IHttpClientFactory, ConcurrentBag<string>) CreateMockHttpClientFactory(
        HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        var capturedRequests = new ConcurrentBag<string>();
        var handler = new MockHttpMessageHandler(statusCode, capturedRequests);
        var httpClient = new HttpClient(handler);
        var mockFactory = new Mock<IHttpClientFactory>();
        mockFactory.Setup(factory => factory.CreateClient("LittyWebhooks")).Returns(httpClient);
        return (mockFactory.Object, capturedRequests);
    }
}

/// <summary>
/// mock HTTP handler catches payloads without touching the network bestie 🧪🔥
/// </summary>
internal sealed class MockHttpMessageHandler(
    HttpStatusCode statusCode,
    ConcurrentBag<string> capturedRequests) : HttpMessageHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (request.Content is not null)
        {
            capturedRequests.Add(await request.Content.ReadAsStringAsync(cancellationToken));
        }

        return new HttpResponseMessage(statusCode);
    }
}
