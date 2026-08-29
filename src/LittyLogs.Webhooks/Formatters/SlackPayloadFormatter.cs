using System.Buffers;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace LittyLogs.Webhooks.Formatters;

/// <summary>
/// formats log batches as safe Slack Block Kit payloads using plain_text only.
/// markdown-shaped user input stays literal, emojis stay bussin, and every limit is respected 🔒🔥
/// </summary>
internal sealed class SlackPayloadFormatter : IWebhookPayloadFormatter
{
    private const int HeaderCharacterLimit = 150;
    private const int SectionCharacterLimit = 3_000;
    private const int FallbackCharacterLimit = 4_000;
    private const int MaximumMessages = 49;
    private const string DefaultHeader = "🔥 LittyLogs";

    public string FormatPayload(IReadOnlyList<string> messages, LittyWebhookOptions options)
    {
        ArgumentNullException.ThrowIfNull(messages);
        ArgumentNullException.ThrowIfNull(options);

        if (messages.Count > MaximumMessages)
        {
            throw new ArgumentOutOfRangeException(
                nameof(messages),
                "Slack only gives us 50 blocks, so one header plus 49 logs is the max bestie 💀🔥");
        }

        var safeMessages = messages.Select(message =>
            TruncateToRunes(RemoveExceptionFences(message), SectionCharacterLimit)).ToArray();

        var fallback = safeMessages.Length == 0
            ? ResolveHeader(options.Username)
            : TruncateToRunes(string.Join("\n\n", safeMessages), FallbackCharacterLimit);

        var buffer = new ArrayBufferWriter<byte>();
        using var writer = new Utf8JsonWriter(buffer, new JsonWriterOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            Indented = false
        });

        writer.WriteStartObject();
        writer.WriteString("text", fallback);
        writer.WriteBoolean("mrkdwn", false);
        writer.WriteStartArray("blocks");

        WriteTextBlock(writer, "header", ResolveHeader(options.Username));
        foreach (var message in safeMessages)
        {
            WriteTextBlock(writer, "section", message);
        }

        writer.WriteEndArray();
        writer.WriteEndObject();
        writer.Flush();

        return Encoding.UTF8.GetString(buffer.WrittenSpan);
    }

    private static void WriteTextBlock(Utf8JsonWriter writer, string blockType, string text)
    {
        writer.WriteStartObject();
        writer.WriteString("type", blockType);
        writer.WriteStartObject("text");
        writer.WriteString("type", "plain_text");
        writer.WriteString("text", text);
        writer.WriteBoolean("emoji", true);
        writer.WriteEndObject();
        writer.WriteEndObject();
    }

    private static string ResolveHeader(string username)
    {
        var header = string.IsNullOrWhiteSpace(username) ? DefaultHeader : $"🔥 {username}";
        return TruncateToRunes(header, HeaderCharacterLimit);
    }

    private static string RemoveExceptionFences(string message)
    {
        const string openingFence = "\n```\n";
        const string closingFence = "\n```";

        var fenceStart = message.IndexOf(openingFence, StringComparison.Ordinal);
        if (fenceStart < 0)
            return message;

        var withoutOpeningFence = message.Remove(fenceStart, openingFence.Length).Insert(fenceStart, "\n");
        var fenceEnd = withoutOpeningFence.LastIndexOf(closingFence, StringComparison.Ordinal);

        return fenceEnd >= fenceStart
            ? withoutOpeningFence.Remove(fenceEnd, closingFence.Length)
            : withoutOpeningFence;
    }

    private static string TruncateToRunes(string value, int maximumRunes)
    {
        if (value.EnumerateRunes().Count() <= maximumRunes)
            return value;

        var result = new StringBuilder();
        foreach (var rune in value.EnumerateRunes().Take(maximumRunes))
        {
            result.Append(rune.ToString());
        }

        return result.ToString();
    }
}
