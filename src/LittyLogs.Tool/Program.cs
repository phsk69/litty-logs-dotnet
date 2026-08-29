using LittyLogs;
using LittyLogs.Tool;
using static LittyLogs.LittyLogsFormatHelper;

if (args.Length == 0)
{
    PrintUsage();
    return 0;
}

var subcommand = args[0].ToLowerInvariant();
var remainingArgs = args[1..];

return subcommand switch
{
    "test" => await RunTest(remainingArgs),
    "build" => await RunBuild(remainingArgs),
    "publish" => await RunPublish(remainingArgs),
    "pack" => await RunPack(remainingArgs),
    "clean" => await RunClean(remainingArgs),
    _ => HandleUnknown(subcommand),
};

static async Task<int> RunTest(string[] extraArgs)
{
    PrintBanner("test");

    var arguments = new List<string> { "test" };

    if (UsesMicrosoftTestingPlatform())
    {
        // MTP output stays detailed and ITestOutputHelper logs keep serving main-character energy 🔥
        if (!extraArgs.Any(a => a.Equals("--output", StringComparison.OrdinalIgnoreCase)))
            arguments.AddRange(["--output", "Detailed"]);

        if (!extraArgs.Any(a => a.Equals("--show-live-output", StringComparison.OrdinalIgnoreCase)))
            arguments.AddRange(["--show-live-output", "on"]);
    }
    else if (!extraArgs.Any(a => a.StartsWith("--logger", StringComparison.OrdinalIgnoreCase)))
    {
        // VSTest besties still get the detailed legacy logger while they migrate 🔥
        arguments.AddRange(["--logger", "console;verbosity=detailed"]);
    }

    arguments.AddRange(extraArgs);

    return await DotnetProcessRunner.RunAsync(arguments,
        line => TestOutputRewriter.TryRewrite(line) ?? line);
}

static bool UsesMicrosoftTestingPlatform()
{
    for (var directory = new DirectoryInfo(Directory.GetCurrentDirectory()); directory is not null; directory = directory.Parent)
    {
        var globalJsonPath = Path.Combine(directory.FullName, "global.json");
        if (!File.Exists(globalJsonPath))
            continue;

        try
        {
            using var document = System.Text.Json.JsonDocument.Parse(
                File.ReadAllText(globalJsonPath),
                new System.Text.Json.JsonDocumentOptions
                {
                    AllowTrailingCommas = true,
                    CommentHandling = System.Text.Json.JsonCommentHandling.Skip,
                });

            return document.RootElement.TryGetProperty("test", out var test)
                && test.TryGetProperty("runner", out var runner)
                && string.Equals(
                    runner.GetString(),
                    "Microsoft.Testing.Platform",
                    StringComparison.OrdinalIgnoreCase);
        }
        catch (System.Text.Json.JsonException)
        {
            // dotnet owns the real validation; a cooked file falls back to legacy behavior here 🔥
            return false;
        }
    }

    return false;
}

static async Task<int> RunBuild(string[] extraArgs)
{
    PrintBanner("build");

    var arguments = new List<string> { "build" };
    arguments.AddRange(extraArgs);

    return await DotnetProcessRunner.RunAsync(arguments,
        line => BuildOutputRewriter.TryRewrite(line) ?? line);
}

static async Task<int> RunPublish(string[] extraArgs)
{
    PrintBanner("publish");

    var arguments = new List<string> { "publish" };
    arguments.AddRange(extraArgs);

    return await DotnetProcessRunner.RunAsync(arguments,
        line => PublishOutputRewriter.TryRewrite(line) ?? line);
}

static async Task<int> RunPack(string[] extraArgs)
{
    PrintBanner("pack");

    var arguments = new List<string> { "pack" };
    arguments.AddRange(extraArgs);

    return await DotnetProcessRunner.RunAsync(arguments,
        line => PackOutputRewriter.TryRewrite(line) ?? line);
}

static async Task<int> RunClean(string[] extraArgs)
{
    PrintBanner("clean");

    var arguments = new List<string> { "clean" };

    // auto-inject normal verbosity so we can see the "Deleting file" lines
    // more fun to watch the yeeting in real time bestie 🗑️
    if (!extraArgs.Any(a => a.StartsWith("--verbosity", StringComparison.OrdinalIgnoreCase)
                            || a.StartsWith("-v", StringComparison.OrdinalIgnoreCase)))
    {
        arguments.AddRange(["--verbosity", "normal"]);
    }

    arguments.AddRange(extraArgs);

    return await DotnetProcessRunner.RunAsync(arguments,
        line => CleanOutputRewriter.TryRewrite(line) ?? line);
}

static int HandleUnknown(string subcommand)
{
    Console.WriteLine($"{Red}yo \"{subcommand}\" aint a valid subcommand bestie{Reset} 💀");
    Console.WriteLine();
    PrintUsage();
    return 1;
}

static void PrintBanner(string subcommand)
{
    Console.WriteLine($"{Cyan}litty {subcommand}{Reset} — making your output bussin no cap 🔥");
    Console.WriteLine();
}

static void PrintUsage()
{
    Console.WriteLine($"{Cyan}litty{Reset} — the CLI tool that makes all dotnet output bussin 🔥");
    Console.WriteLine();
    Console.WriteLine($"  {Green}litty test{Reset}    [args...]  wraps dotnet test with litty-fied output 🧪");
    Console.WriteLine($"  {Green}litty build{Reset}   [args...]  wraps dotnet build with litty-fied output 🏗️");
    Console.WriteLine($"  {Green}litty publish{Reset} [args...]  wraps dotnet publish with litty-fied output 📤");
    Console.WriteLine($"  {Green}litty pack{Reset}    [args...]  wraps dotnet pack with litty-fied output 📦");
    Console.WriteLine($"  {Green}litty clean{Reset}   [args...]  wraps dotnet clean with litty-fied output 🗑️");
    Console.WriteLine();
    Console.WriteLine($"  all args pass through to the underlying dotnet command no cap");
    Console.WriteLine($"  {Dim}litty test auto-injects detailed logging so your test output shows up{Reset}");
    Console.WriteLine($"  {Dim}litty clean auto-injects normal verbosity so you see what gets yeeted{Reset}");
}
