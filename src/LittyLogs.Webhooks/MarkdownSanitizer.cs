namespace LittyLogs.Webhooks;

/// <summary>
/// escapes markdown syntax so chat platforms dont render malicious content from log messages.
/// prevents tracking pixels, phishing links, and formatting injection no cap 🔒
/// reusable across all webhook formatters (Matrix hookshot, Teams, etc) 💅
/// </summary>
internal static class MarkdownSanitizer
{
    /// <summary>
    /// escapes markdown characters so they render as literal text in hookshot/chat.
    /// backslash-escaping is the standard markdown way to neutralize syntax bestie 🔥
    /// </summary>
    public static string EscapeMarkdown(string text)
    {
        // fast path — if theres no markdown chars, skip the allocations 🏎️
        if (!ContainsMarkdownChars(text))
            return text;

        // escape backslash first so we dont double-escape the escapes we add 💀
        return text
            .Replace("\\", "\\\\")
            .Replace("[", "\\[")
            .Replace("]", "\\]")
            .Replace("(", "\\(")
            .Replace(")", "\\)")
            .Replace("!", "\\!")
            .Replace("*", "\\*")
            .Replace("_", "\\_")
            .Replace("`", "\\`")
            .Replace("#", "\\#")
            .Replace(">", "\\>")
            .Replace("|", "\\|");
    }

    private static bool ContainsMarkdownChars(string text)
    {
        foreach (var c in text)
        {
            if (c is '\\' or '[' or ']' or '(' or ')' or '!' or '*' or '_' or '`' or '#' or '>' or '|')
                return true;
        }
        return false;
    }
}
