#!/usr/bin/env bash
set -euo pipefail

# Keep dotnet run and the wrapped solution commands on the SDK's own target.
sdk_version=$(dotnet --version)
sdk_major=${sdk_version%%.*}
export LittyLogsFramework="${LittyLogsFramework:-net${sdk_major}.0}"
exec dotnet "$@"
