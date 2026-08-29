using System.Text.Json;
using LittyLogs.Xunit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace LittyLogs.Tests;

/// <summary>
/// testing that the JSON formatter absolutely slaps — valid JSON, emojis, rewrites, the whole nine 🔥
/// dogfooding our own litty-logs xUnit integration for test output 💅
/// </summary>
public class LittyLogsJsonFormatterTests
{
    private readonly ILogger<LittyLogsJsonFormatterTests> _logger;

    public LittyLogsJsonFormatterTests(ITestOutputHelper output)
    {
        _logger = output.CreateLittyLogger<LittyLogsJsonFormatterTests>();
    }

    private static LittyLogsJsonFormatter CreateFormatter(LittyLogsOptions? opts = null)
    {
        var monitor = new Mock<IOptionsMonitor<LittyLogsOptions>>();
        monitor.Setup(m => m.CurrentValue).Returns(opts ?? new LittyLogsOptions());
        return new LittyLogsJsonFormatter(monitor.Object);
    }

    private static string RenderJsonLog(
        LittyLogsJsonFormatter formatter,
        LogLevel level,
        string category,
        string message,
        Exception? exception = null,
        EventId? eventId = null)
    {
        var writer = new StringWriter();
        var entry = new LogEntry<string>(
            level,
            category,
            eventId ?? new EventId(0),
            message,
            exception,
            (state, _) => state);

        formatter.Write(in entry, null, writer);
        return writer.ToString();
    }

    private static JsonDocument ParseJson(string output) =>
        JsonDocument.Parse(output.Trim());

    // ==========================================
    // core JSON structure tests
    // ==========================================

    [Fact]
    public void Write_ProducesValidJson()
    {
        // valid JSON or we riot no cap
        var formatter = CreateFormatter();
        var output = RenderJsonLog(formatter, LogLevel.Information, "TestCategory", "test message");

        var doc = ParseJson(output);
        Assert.NotNull(doc);
    }

    [Theory]
    [InlineData(LogLevel.Trace, "trace", "👀")]
    [InlineData(LogLevel.Debug, "debug", "🔍")]
    [InlineData(LogLevel.Information, "info", "🔥")]
    [InlineData(LogLevel.Warning, "warning", "😤")]
    [InlineData(LogLevel.Error, "err", "💀")]
    [InlineData(LogLevel.Critical, "crit", "☠️")]
    public void Write_HasCorrectLevelAndEmoji(LogLevel level, string expectedLabel, string expectedEmoji)
    {
        // level + emoji gotta be on point in the JSON bestie
        // testing BOTH parsed values AND raw output — no false positives allowed 🔒
        var formatter = CreateFormatter();
        var output = RenderJsonLog(formatter, level, "TestCategory", "test message");

        using var doc = ParseJson(output);
        var root = doc.RootElement;

        Assert.Equal(expectedLabel, root.GetProperty("level").GetString());
        Assert.Equal(expectedEmoji, root.GetProperty("emoji").GetString());

        // raw output must have literal emoji too — Loki searches on raw text bestie
        Assert.Contains(expectedEmoji, output);
    }

    [Fact]
    public void Write_HasAllRequiredFields()
    {
        // every JSON line needs the full squad of fields
        var formatter = CreateFormatter();
        var output = RenderJsonLog(formatter, LogLevel.Information, "TestCategory", "hello bestie");

        using var doc = ParseJson(output);
        var root = doc.RootElement;

        Assert.True(root.TryGetProperty("timestamp", out _), "missing timestamp field 💀");
        Assert.True(root.TryGetProperty("level", out _), "missing level field 💀");
        Assert.True(root.TryGetProperty("emoji", out _), "missing emoji field 💀");
        Assert.True(root.TryGetProperty("category", out _), "missing category field 💀");
        Assert.True(root.TryGetProperty("message", out _), "missing message field 💀");
    }

    // ==========================================
    // category + rewrite tests
    // ==========================================

    [Fact]
    public void Write_ShortensCategoryByDefault()
    {
        // yeet namespace bloat in JSON too fr
        var formatter = CreateFormatter();
        var output = RenderJsonLog(formatter, LogLevel.Information,
            "Microsoft.Hosting.Lifetime", "test message");

        using var doc = ParseJson(output);
        Assert.Equal("Lifetime", doc.RootElement.GetProperty("category").GetString());
    }

    [Fact]
    public void Write_KeepsFullCategory_WhenShorteningDisabled()
    {
        var opts = new LittyLogsOptions { ShortenCategories = false };
        var formatter = CreateFormatter(opts);
        var output = RenderJsonLog(formatter, LogLevel.Information,
            "Microsoft.Hosting.Lifetime", "test message");

        using var doc = ParseJson(output);
        Assert.Equal("Microsoft.Hosting.Lifetime", doc.RootElement.GetProperty("category").GetString());
    }

    [Fact]
    public void Write_RewritesFrameworkMessages()
    {
        // the secret sauce works in JSON too bestie 🧠
        var formatter = CreateFormatter();
        var output = RenderJsonLog(formatter, LogLevel.Information,
            "Microsoft.Hosting.Lifetime",
            "Application started. Press Ctrl+C to shut down.");

        using var doc = ParseJson(output);
        var message = doc.RootElement.GetProperty("message").GetString()!;

        Assert.Contains("bussin", message);
        Assert.DoesNotContain("Press Ctrl+C to shut down", message);
    }

    [Fact]
    public void Write_DoesNotRewrite_WhenDisabled()
    {
        var opts = new LittyLogsOptions { RewriteMessages = false };
        var formatter = CreateFormatter(opts);
        var output = RenderJsonLog(formatter, LogLevel.Information,
            "Microsoft.Hosting.Lifetime",
            "Application started. Press Ctrl+C to shut down.");

        using var doc = ParseJson(output);
        var message = doc.RootElement.GetProperty("message").GetString()!;

        Assert.Contains("Press Ctrl+C to shut down", message);
    }

    [Fact]
    public void Write_PassesThroughCustomMessages()
    {
        // custom messages stay untouched bestie — check parsed AND raw
        var formatter = CreateFormatter();
        var customMsg = "my custom message with 🔥 emojis";
        var output = RenderJsonLog(formatter, LogLevel.Information, "MyApp", customMsg);

        using var doc = ParseJson(output);
        Assert.Equal(customMsg, doc.RootElement.GetProperty("message").GetString());

        // raw output must have the literal emoji too
        Assert.Contains("🔥", output);
    }

    [Fact]
    public void Write_RewrittenMessageEmojisLandInJson()
    {
        // when framework messages get rewritten, the emojis from the rewrite
        // should show up in BOTH parsed and raw output 🔥
        var formatter = CreateFormatter();
        var output = RenderJsonLog(formatter, LogLevel.Information,
            "Microsoft.Hosting.Lifetime",
            "Now listening on: http://localhost:5000");

        using var doc = ParseJson(output);
        var message = doc.RootElement.GetProperty("message").GetString()!;

        Assert.Contains("🎧", message);
        Assert.Contains("fr fr", message);

        // raw output — Loki gonna search on this bestie
        Assert.Contains("🎧", output);
    }

    // ==========================================
    // emoji serialization — raw output assertions (the anti-false-positive squad) 🔒
    // ==========================================

    [Fact]
    public void Write_EmojisSerializeCorrectlyInJson()
    {
        // check BOTH parsed values and raw string — no sneaky GetString() unescaping allowed
        var formatter = CreateFormatter();
        var output = RenderJsonLog(formatter, LogLevel.Information,
            "TestCategory", "vibes are immaculate 💅🔥✨");

        // parsed values (these always work even with escaping — thats the false positive trap 💀)
        using var doc = ParseJson(output);
        var message = doc.RootElement.GetProperty("message").GetString()!;
        Assert.Contains("💅", message);
        Assert.Contains("🔥", message);
        Assert.Contains("✨", message);

        // raw output — THIS is what Loki actually searches on
        Assert.Contains("💅", output);
        Assert.Contains("🔥", output);
        Assert.Contains("✨", output);
    }

    [Fact]
    public void Write_RawJsonHasLiteralBmpEmojis()
    {
        // BMP emojis (below U+FFFF) must be literal in raw JSON — not escaped as \uXXXX
        // these were the sneaky ones that default encoder was escaping 💀
        var formatter = CreateFormatter();

        // ☠️ = U+2620 + U+FE0F (skull and crossbones — BMP)
        var output = RenderJsonLog(formatter, LogLevel.Critical, "TestCategory", "its giving death ☠️");
        Assert.Contains("☠️", output);
        _logger.LogInformation("CRIT raw output: {Output}", output.Trim());

        // ✨ = U+2728 (sparkles — BMP)
        var output2 = RenderJsonLog(formatter, LogLevel.Information, "TestCategory", "vibes ✨");
        Assert.Contains("✨", output2);

        // 😤 = U+1F624 (actually supplementary, but good to verify the whole squad)
        var output3 = RenderJsonLog(formatter, LogLevel.Warning, "TestCategory", "not it 😤");
        Assert.Contains("😤", output3);
    }

    [Fact]
    public void Write_RawJsonHasLiteralSupplementaryEmojis()
    {
        // supplementary plane emojis (above U+FFFF) — UnescapeSurrogatePairs() handles these
        var formatter = CreateFormatter();
        var output = RenderJsonLog(formatter, LogLevel.Information,
            "TestCategory", "the full emoji squad: 🔥💅🎧👀🔍💀");

        Assert.Contains("🔥", output);
        Assert.Contains("💅", output);
        Assert.Contains("🎧", output);
        Assert.Contains("👀", output);
        Assert.Contains("🔍", output);
        Assert.Contains("💀", output);
        _logger.LogInformation("supplementary emoji raw output: {Output}", output.Trim());
    }

    [Fact]
    public void Write_RawJsonHasLiteralSpecialChars()
    {
        // em dashes and other special chars must be literal — not \u2014 etc
        var formatter = CreateFormatter();
        var output = RenderJsonLog(formatter, LogLevel.Information,
            "TestCategory", "this — is an em dash and here's a plus +");

        Assert.Contains("—", output);
        Assert.Contains("+", output);
        _logger.LogInformation("special chars raw output: {Output}", output.Trim());
    }

    [Fact]
    public void Write_TimestampHasLiteralPlusSign()
    {
        // timestamp offset must be +00:00 not \u002B00:00
        var formatter = CreateFormatter();
        var output = RenderJsonLog(formatter, LogLevel.Information, "TestCategory", "timestamp check");

        // the raw JSON must NOT have \u002B anywhere
        Assert.DoesNotContain("\\u002B", output);
        _logger.LogInformation("timestamp raw output: {Output}", output.Trim());
    }

    // ==========================================
    // unhappy paths — make sure escaped unicode is GONE 🚫
    // ==========================================

    [Fact]
    public void Write_RawJsonHasNoEscapedBmpUnicode()
    {
        // none of these escaped forms should exist in raw output — Loki cant search em 💀
        var formatter = CreateFormatter();
        var output = RenderJsonLog(formatter, LogLevel.Critical,
            "TestCategory", "full test — emojis ☠️ and ✨ and special chars +");

        Assert.DoesNotContain("\\u2620", output); // skull
        Assert.DoesNotContain("\\uFE0F", output); // variation selector
        Assert.DoesNotContain("\\u2728", output); // sparkles
        Assert.DoesNotContain("\\u2014", output); // em dash
        Assert.DoesNotContain("\\u002B", output); // plus sign
        _logger.LogInformation("no-escape verification: {Output}", output.Trim());
    }

    [Fact]
    public void Write_RawJsonHasNoSurrogatePairEscapes()
    {
        // surrogate pair escapes like \uD83D should NEVER appear in final output
        var formatter = CreateFormatter();
        var output = RenderJsonLog(formatter, LogLevel.Information,
            "TestCategory", "emoji party: 🔥💅🎧👀🔍💀😤");

        Assert.DoesNotContain("\\uD83D", output);
        Assert.DoesNotContain("\\uD83E", output);
        Assert.DoesNotContain("\\uDC", output); // low surrogate prefix
        _logger.LogInformation("no-surrogate verification: {Output}", output.Trim());
    }

    [Fact]
    public void Write_JsonStructuralCharsAreStillEscaped()
    {
        // quotes, backslashes, and newlines MUST still be escaped — thats JSON structure not unicode
        var formatter = CreateFormatter();
        var output = RenderJsonLog(formatter, LogLevel.Information,
            "TestCategory", "message with \"quotes\" and \\backslash and \nnewline");

        // the output should be valid JSON (proves structural chars are properly escaped)
        using var doc = ParseJson(output);
        var message = doc.RootElement.GetProperty("message").GetString()!;

        Assert.Contains("\"quotes\"", message);
        Assert.Contains("\\backslash", message);
        Assert.Contains("\n", message);
    }

    [Fact]
    public void Write_MalformedUnicodeDoesntBreakFormatter()
    {
        // edge case: weird unicode boundaries should still produce valid JSON
        var formatter = CreateFormatter();

        // string with mixed BMP and supplementary chars jammed together
        var crazyMsg = "a]b\t☠️🔥c\r\nd\"e✨💅f";
        var output = RenderJsonLog(formatter, LogLevel.Information, "TestCategory", crazyMsg);

        // must still be valid JSON no matter what
        using var doc = ParseJson(output);
        Assert.NotNull(doc);
        _logger.LogInformation("malformed unicode test passed — output: {Output}", output.Trim());
    }

    // ==========================================
    // other JSON features
    // ==========================================

    [Fact]
    public void Write_NoAnsiCodesInJson()
    {
        // ANSI codes in JSON is cursed af 💀 make sure they never sneak in
        var formatter = CreateFormatter(new LittyLogsOptions { UseColors = true });
        var output = RenderJsonLog(formatter, LogLevel.Error, "TestCategory", "error message");

        Assert.False(output.Contains('\x1b'),
            "JSON output should NEVER contain ANSI escape chars, thats cursed bestie 💀");
    }

    [Fact]
    public void Write_IncludesExceptionObject()
    {
        // exceptions gotta be structured in JSON for proper debugging
        var formatter = CreateFormatter();
        var ex = new InvalidOperationException("something flopped hard bestie");
        var output = RenderJsonLog(formatter, LogLevel.Error, "TestCategory", "it broke", ex);

        using var doc = ParseJson(output);
        var exObj = doc.RootElement.GetProperty("exception");

        Assert.Equal("System.InvalidOperationException", exObj.GetProperty("type").GetString());
        Assert.Equal("something flopped hard bestie", exObj.GetProperty("message").GetString());
    }

    [Fact]
    public void Write_IncludesEventId_WhenNonDefault()
    {
        // eventId shows up when its actually set
        var formatter = CreateFormatter();
        var output = RenderJsonLog(formatter, LogLevel.Information,
            "TestCategory", "test message", eventId: new EventId(42, "LittyEvent"));

        using var doc = ParseJson(output);
        var eventIdObj = doc.RootElement.GetProperty("eventId");

        Assert.Equal(42, eventIdObj.GetProperty("id").GetInt32());
        Assert.Equal("LittyEvent", eventIdObj.GetProperty("name").GetString());
    }

    [Fact]
    public void Write_OmitsEventId_WhenDefault()
    {
        // no eventId clutter when its just the default zero
        var formatter = CreateFormatter();
        var output = RenderJsonLog(formatter, LogLevel.Information,
            "TestCategory", "test message", eventId: new EventId(0));

        using var doc = ParseJson(output);
        Assert.False(doc.RootElement.TryGetProperty("eventId", out _),
            "default eventId (0) should not appear in JSON output bestie");
    }

    [Fact]
    public void Write_HasIso8601Timestamp()
    {
        // ISO 8601 for that international rizz 🌍
        var formatter = CreateFormatter();
        var output = RenderJsonLog(formatter, LogLevel.Information, "TestCategory", "test");

        using var doc = ParseJson(output);
        var timestamp = doc.RootElement.GetProperty("timestamp").GetString()!;

        // should match ISO 8601 with milliseconds pattern
        Assert.Matches(@"\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}\.\d{3}", timestamp);
    }

    [Fact]
    public void Write_SkipsNullMessage_WhenNoException()
    {
        // no message + no exception = no output, we dont do empty JSON
        var formatter = CreateFormatter();
        var writer = new StringWriter();
        var entry = new LogEntry<string>(
            LogLevel.Information,
            "TestCategory",
            new EventId(0),
            "ignored",
            null,
            (_, _) => null!);

        formatter.Write(in entry, null, writer);
        Assert.Empty(writer.ToString());
    }
}
