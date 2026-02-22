using System.Buffers;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace LittyLogs.Webhooks.Formatters;

/// <summary>
/// formats log batches into Teams Adaptive Card v1.5 JSON payload.
/// each message gets its own severity-colored Container — your Teams chat
/// lowkey looks like a dashboard bestie 🟦🔥
///
/// severity detection sniffs the emoji+level pattern from the formatted string
/// (like [💀 error]) and maps it to Adaptive Card container styles.
/// exceptions get their own subtle monospace TextBlock no cap
/// </summary>
internal sealed class TeamsPayloadFormatter : IWebhookPayloadFormatter
{
    public string FormatPayload(IReadOnlyList<string> messages, LittyWebhookOptions options)
    {
        var buffer = new ArrayBufferWriter<byte>();
        using var writer = new Utf8JsonWriter(buffer, new JsonWriterOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            Indented = false
        });

        // teams webhook envelope — type: message with an adaptive card attachment
        writer.WriteStartObject();
        writer.WriteString("type", "message");

        writer.WriteStartArray("attachments");
        writer.WriteStartObject();
        writer.WriteString("contentType", "application/vnd.microsoft.card.adaptive");
        writer.WriteNull("contentUrl");

        // the actual adaptive card lives in "content" 🟦
        writer.WritePropertyName("content");
        writer.WriteStartObject();
        writer.WriteString("$schema", "http://adaptivecards.io/schemas/adaptive-card.json");
        writer.WriteString("type", "AdaptiveCard");
        writer.WriteString("version", "1.5");

        // card body — header + one container per message
        writer.WriteStartArray("body");

        // header with username 🔥
        writer.WriteStartObject();
        writer.WriteString("type", "TextBlock");
        writer.WriteString("text", string.IsNullOrEmpty(options.Username) ? "🔥 LittyLogs" : $"🔥 {options.Username}");
        writer.WriteString("weight", "bolder");
        writer.WriteString("size", "medium");
        writer.WriteEndObject();

        // each message gets its own severity-colored container 💅
        foreach (var message in messages)
        {
            WriteMessageContainer(writer, message);
        }

        writer.WriteEndArray(); // end body

        writer.WriteEndObject(); // end content (adaptive card)
        writer.WriteEndObject(); // end attachment
        writer.WriteEndArray();  // end attachments
        writer.WriteEndObject(); // end envelope

        writer.Flush();
        return Encoding.UTF8.GetString(buffer.WrittenSpan);
    }

    /// <summary>
    /// writes a single message as a Container with severity-colored styling.
    /// if the message has an exception code block, it gets a separate subtle monospace TextBlock 🧠
    /// </summary>
    private static void WriteMessageContainer(Utf8JsonWriter writer, string message)
    {
        writer.WriteStartObject();
        writer.WriteString("type", "Container");
        writer.WriteString("style", DetectSeverityStyle(message));

        writer.WriteStartArray("items");

        // check for exception code block (same fence pattern as Matrix)
        var fenceStart = message.IndexOf("\n```\n", StringComparison.Ordinal);
        if (fenceStart < 0)
        {
            // no exception — just the log line
            WriteMonospaceTextBlock(writer, message, isSubtle: false);
        }
        else
        {
            // split log line from exception
            var logLine = message[..fenceStart];
            var codeContent = message[(fenceStart + 5)..]; // skip past \n```\n (5 chars)
            var fenceEnd = codeContent.LastIndexOf("\n```", StringComparison.Ordinal);
            if (fenceEnd >= 0)
                codeContent = codeContent[..fenceEnd];

            // log line — normal monospace
            WriteMonospaceTextBlock(writer, logLine, isSubtle: false);

            // exception — subtle monospace so it dont overpower the log line
            WriteMonospaceTextBlock(writer, codeContent, isSubtle: true);
        }

        writer.WriteEndArray(); // end items
        writer.WriteEndObject(); // end container
    }

    /// <summary>
    /// writes a monospace TextBlock element — the building block of our card 📝
    /// </summary>
    private static void WriteMonospaceTextBlock(Utf8JsonWriter writer, string text, bool isSubtle)
    {
        writer.WriteStartObject();
        writer.WriteString("type", "TextBlock");
        writer.WriteString("text", text);
        writer.WriteBoolean("wrap", true);
        writer.WriteString("fontType", "monospace");
        writer.WriteString("size", "small");
        if (isSubtle)
            writer.WriteBoolean("isSubtle", true);
        writer.WriteEndObject();
    }

    /// <summary>
    /// sniffs the emoji+level pattern in the formatted string and returns the
    /// Adaptive Card container style. severity colors go hard in Teams 🎨
    ///
    /// handles both timestamp modes:
    /// - RFC 5424 (default): [emoji level] [timestamp] [category] message
    /// - observability:      [timestamp] [emoji level] [category] message
    /// </summary>
    private static string DetectSeverityStyle(string message)
    {
        // check for emoji+level patterns anywhere in the string
        // these come from LittyLogsFormatHelper and are always in brackets
        if (message.Contains("[☠️ critical]") || message.Contains("[💀 error]"))
            return "attention"; // red — big L 💀

        if (message.Contains("[😤 warning]"))
            return "warning"; // yellow — not it 😤

        if (message.Contains("[🔥 info]"))
            return "good"; // green — we vibing 🔥

        // trace, debug, or unknown — neutral container
        return "default";
    }
}
