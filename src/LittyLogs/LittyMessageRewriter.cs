namespace LittyLogs;

/// <summary>
/// the brain of the whole operation — rewrites boring corporate framework messages
/// into pure gen alpha energy no cap 🧠
/// </summary>
public static class LittyMessageRewriter
{
    private record MessageTransform(
        string MatchPrefix,
        Func<string, string> Rewrite);

    private static readonly MessageTransform[] Transforms =
    [
        // Microsoft.Hosting.Lifetime messages — the ones you see every single dotnet run
        new("Application started. Press Ctrl+C to shut down.",
            _ => "app is bussin and ready to slay bestie 💅 yeet Ctrl+C to dip out no cap"),

        new("Application is shutting down...",
            _ => "app said aight imma head out 💀"),

        new("Now listening on: ",
            msg => $"we vibing on {msg["Now listening on: ".Length..]} fr fr 🎧"),

        new("Content root path: ",
            msg => $"content root living at {msg["Content root path: ".Length..]} bestie 📁"),

        new("Hosting environment: ",
            msg => $"we in our {msg["Hosting environment: ".Length..]} era rn ✨"),

        // Microsoft.Hosting.Lifetime — generic hosting lifecycle
        new("Hosting starting",
            _ => "booting up, lets get this bread 🍞"),

        new("Hosting started",
            _ => "fully locked in and ready to slay 💅"),

        new("Hosting stopping",
            _ => "yeeting ourselves out of existence 🫡"),

        new("Hosting stopped",
            _ => "we out, peace was never an option ✌️"),

        // Microsoft.AspNetCore.Hosting.Diagnostics — request logging
        new("Request starting ",
            msg => $"yo a request just slid in: {msg["Request starting ".Length..]} 👀"),

        new("Request finished ",
            msg => $"request finished cooking: {msg["Request finished ".Length..]} 🍳"),

        // Microsoft.AspNetCore.Routing
        new("Request matched endpoint '",
            msg =>
            {
                // original: "Request matched endpoint 'endpointName'"
                var inner = msg["Request matched endpoint ".Length..];
                return $"request found its bestie endpoint {inner} 🤝";
            }),

        new("Executing endpoint '",
            msg =>
            {
                var inner = msg["Executing endpoint ".Length..];
                return $"finna execute endpoint {inner} lets gooo 🚀";
            }),

        new("Executed endpoint '",
            msg =>
            {
                var inner = msg["Executed endpoint ".Length..];
                return $"endpoint {inner} just ate and left no crumbs 💅";
            }),
    ];

    /// <summary>
    /// attempts to rewrite a known framework message into gen alpha slang.
    /// returns null if the message aint recognized (passthrough, we dont mess with custom messages)
    /// </summary>
    public static string? TryRewrite(string message)
    {
        foreach (var transform in Transforms)
        {
            if (message.StartsWith(transform.MatchPrefix, StringComparison.Ordinal))
                return transform.Rewrite(message);
        }

        return null;
    }
}
