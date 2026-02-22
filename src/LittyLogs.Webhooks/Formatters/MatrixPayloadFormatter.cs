using System.Buffers;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace LittyLogs.Webhooks.Formatters;

/// <summary>
/// formats log batches into Matrix hookshot JSON payload.
/// hookshot eats markdown and auto-converts to HTML — we just feed it formatted lines 🟣🔥
/// payload shape: {"text": "markdown here", "username": "LittyLogs"}
/// </summary>
internal sealed class MatrixPayloadFormatter : IWebhookPayloadFormatter
{
    public string FormatPayload(IReadOnlyList<string> messages, LittyWebhookOptions options)
    {
        // join all messages with newlines — hookshot renders markdown so each line is its own paragraph
        var combined = string.Join("\n", messages);

        // build JSON using Utf8JsonWriter — same zero-alloc pattern as FormatJsonLine() in LittyLogsFormatHelper
        var buffer = new ArrayBufferWriter<byte>();
        using var writer = new Utf8JsonWriter(buffer, new JsonWriterOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            Indented = false
        });

        writer.WriteStartObject();
        writer.WriteString("text", combined);
        if (!string.IsNullOrEmpty(options.Username))
        {
            writer.WriteString("username", options.Username);
        }
        writer.WriteEndObject();
        writer.Flush();

        return Encoding.UTF8.GetString(buffer.WrittenSpan);
    }
}
