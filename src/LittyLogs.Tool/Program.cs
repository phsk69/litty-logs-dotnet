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
    _ => HandleUnknown(subcommand),
};

static async Task<int> RunTest(string[] extraArgs)
{
    PrintBanner("test");

    var arguments = new List<string> { "test" };

    // auto-inject logger so ITestOutputHelper litty logs show up in output
    // console;verbosity=detailed gives us test output without MSBuild spam
    if (!extraArgs.Any(a => a.StartsWith("--logger", StringComparison.OrdinalIgnoreCase)))
    {
        arguments.AddRange(["--logger", "console;verbosity=detailed"]);
    }

    arguments.AddRange(extraArgs);

    return await DotnetProcessRunner.RunAsync(arguments,
        line => TestOutputRewriter.TryRewrite(line) ?? line);
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
    Console.WriteLine();
    Console.WriteLine($"  all args pass through to the underlying dotnet command no cap");
    Console.WriteLine($"  {Dim}litty test auto-injects detailed logging so your test output shows up{Reset}");
}
