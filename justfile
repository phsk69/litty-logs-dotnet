# litty-logs — the most bussin logging library no cap 🔥

# build the whole solution
build:
    dotnet build

# run all the tests bestie (verbosity normal so litty output shows up)
test:
    dotnet test --verbosity normal

# run tests through the litty tool for maximum bussin output
litty-test:
    dotnet run --project src/LittyLogs.Tool -- test

# build through the litty tool for that gen alpha build output
litty-build:
    dotnet run --project src/LittyLogs.Tool -- build

# pack all NuGet packages so the besties can install em
pack:
    dotnet pack src/LittyLogs/LittyLogs.csproj -c Release
    dotnet pack src/LittyLogs.Xunit/LittyLogs.Xunit.csproj -c Release
    dotnet pack src/LittyLogs.Tool/LittyLogs.Tool.csproj -c Release

# run the web api example to flex
example-web:
    dotnet run --project examples/LittyLogs.Example.WebApi

# run the hosted service example (background vibe checker)
example-hosted:
    dotnet run --project examples/LittyLogs.Example.HostedService

# run the console example (the ten-liner speedrun)
example-console:
    dotnet run --project examples/LittyLogs.Example.Console

# run the xunit example tests to see litty-fied test output
example-xunit:
    dotnet test examples/LittyLogs.Example.Xunit --verbosity normal

# yeet all build artifacts
clean:
    dotnet clean
