using System.Buffers;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace LittyLogs.Webhooks.Formatters;

/// <summary>
/// formats log batches into Matrix hookshot JSON payload.
/// sends both `text` (markdown fallback) and `html` (primary rendering) fields
/// because hookshot prefers html when present — per the docs no cap 🟣🔥
///
/// security: HtmlEscape() encodes the 5 dangerous HTML chars (&lt;&gt;&amp;&quot;&#39;)
/// while preserving emojis as literal UTF-8. WebUtility.HtmlEncode() was encoding emojis
/// to numeric entities (&#128548;) which Matrix renders as boxes — thats not it 💀
/// </summary>
internal sealed class MatrixPayloadFormatter : IWebhookPayloadFormatter
{
    public string FormatPayload(IReadOnlyList<string> messages, LittyWebhookOptions options)
    {
        // text field: markdown fallback with paragraph breaks (\n\n)
        // CommonMark spec: blank line = paragraph break, guaranteed to render as separate blocks
        // single \n gets collapsed to a space by markdown renderers — thats why we use \n\n 💅
        var text = string.Join("\n\n", messages);

        // html field: hookshot PREFERS this when present — per the docs
        // HtmlEscape() encodes dangerous chars, preserves emojis as literal UTF-8
        // <br/> between messages for proper line breaks, <pre><code> for exception blocks 🔥
        var html = string.Join("<br/>", messages.Select(MessageToHtml));

        // build JSON using Utf8JsonWriter — same zero-alloc pattern as FormatJsonLine() in LittyLogsFormatHelper
        var buffer = new ArrayBufferWriter<byte>();
        using var writer = new Utf8JsonWriter(buffer, new JsonWriterOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            Indented = false
        });

        writer.WriteStartObject();
        writer.WriteString("text", text);
        writer.WriteString("html", html);
        if (!string.IsNullOrEmpty(options.Username))
        {
            writer.WriteString("username", options.Username);
        }
        writer.WriteEndObject();
        writer.Flush();

        return Encoding.UTF8.GetString(buffer.WrittenSpan);
    }

    /// <summary>
    /// converts a single message to HTML — handles both regular log lines and exception code blocks.
    /// exception messages have code fences we add in the logger: "log line\n```\nexception\n```"
    /// HtmlEscape() keeps emojis as literal UTF-8 while encoding dangerous HTML chars 💅
    /// </summary>
    private static string MessageToHtml(string message)
    {
        // check if message contains a code block (exception attached by the logger)
        var fenceStart = message.IndexOf("\n```\n", StringComparison.Ordinal);
        if (fenceStart < 0)
            return HtmlEscape(message);

        // split into log line and code block content
        var logLine = message[..fenceStart];
        var codeContent = message[(fenceStart + 5)..]; // skip past \n```\n (5 chars)
        var fenceEnd = codeContent.LastIndexOf("\n```", StringComparison.Ordinal);
        if (fenceEnd >= 0)
            codeContent = codeContent[..fenceEnd];

        return HtmlEscape(logLine) +
               "<br/><pre><code>" + HtmlEscape(codeContent) + "</code></pre>";
    }

    /// <summary>
    /// encodes the 5 dangerous HTML characters while preserving everything else as literal UTF-8.
    /// WebUtility.HtmlEncode() encodes emojis to numeric entities (&#128548;) which Matrix
    /// renders as boxes — so we roll our own minimal escape that keeps emojis bussin 🔥
    /// this is the same pattern every web framework uses for HTML escaping no cap
    /// </summary>
    private static string HtmlEscape(string text) => text
        .Replace("&", "&amp;")   // must be first to avoid double-encoding
        .Replace("<", "&lt;")
        .Replace(">", "&gt;")
        .Replace("\"", "&quot;")
        .Replace("'", "&#39;");
}
