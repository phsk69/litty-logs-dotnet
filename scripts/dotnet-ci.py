"""Verify the supported SDKs and the NuGet packages shipped by a workflow run."""

from dataclasses import dataclass, replace
import hashlib
import json
import os
from pathlib import Path, PurePosixPath
import re
import subprocess
import sys
import tempfile
import xml.etree.ElementTree as ET
import zipfile

SOLUTION = Path("LittyLogs.slnx")
PROPS = Path("Directory.Build.props")
PACKAGE_DIR = Path("nupkgs")
MANIFEST = PACKAGE_DIR / "manifest.json"
LIBRARIES = {
    "LittyLogs": "LittyLogs.LittyLogsOptions",
    "LittyLogs.File": "LittyLogs.File.LittyFileLogsOptions",
    "LittyLogs.Webhooks": "LittyLogs.Webhooks.LittyWebhookOptions",
    "LittyLogs.Xunit": "LittyLogs.Xunit.LittyLogsXunitExtensions",
}
TOOL = "LittyLogs.Tool"
PACKAGE_PROJECTS = {
    name: Path("src") / name / f"{name}.csproj"
    for name in sorted([*LIBRARIES, TOOL])
}
SDK_REGISTRY = "mcr.microsoft.com/dotnet/sdk"
WORKSPACE = PurePosixPath("/workspace")
CI_STATE = PurePosixPath("/ci")


@dataclass(frozen=True)
class SdkContainer:
    image: str
    state: Path
    framework: str | None = None

    def command(self, *args, workdir=WORKSPACE, packages=CI_STATE / "nuget", online=False,
                read_only=False, forward_env=()):
        env = {
            "HOME": str(CI_STATE / "home"),
            "DOTNET_CLI_HOME": str(CI_STATE / "home"),
            "DOTNET_CLI_TELEMETRY_OPTOUT": "1",
            "TESTINGPLATFORM_TELEMETRY_OPTOUT": "1",
            "DOTNET_CLI_WORKLOAD_UPDATE_NOTIFY_DISABLE": "true",
            "DOTNET_NOLOGO": "1",
            "DOTNET_CLI_DO_NOT_USE_MSBUILD_SERVER": "1",
            "MSBUILDDISABLENODEREUSE": "1",
            "NUGET_PACKAGES": str(packages),
        }
        if self.framework:
            env["LittyLogsFramework"] = self.framework
        command = [
            "podman", "run", "--rm", "--pull=never", "--userns=keep-id",
            "--volume", f"{Path.cwd()}:{WORKSPACE}:{'ro' if read_only else 'rw'}",
            "--volume", f"{self.state}:{CI_STATE}",
            "--workdir", str(workdir),
        ]
        if not online:
            command.append("--network=none")
        for name, value in env.items():
            command.extend(["--env", f"{name}={value}"])
        for name in forward_env:
            command.extend(["--env", name])
        return [*command, self.image, *args]

    def dotnet(self, *args, **options):
        subprocess.run(self.command("dotnet", *args, **options), check=True)


def frameworks():
    values = ET.parse(PROPS).findtext("PropertyGroup/LittyLogsSupportedFrameworks").split(";")
    if not values or any(not re.fullmatch(r"net[1-9]\d*\.0", value) for value in values):
        raise ValueError(f"Invalid supported frameworks: {values}")
    if values != sorted(set(values), key=lambda framework: int(framework[3:-2])):
        raise ValueError(f"List supported frameworks once each, from oldest to newest: {values}")
    return values


def version():
    return ET.parse(PROPS).findtext("PropertyGroup/Version")


def sha(path):
    with path.open("rb") as stream:
        return hashlib.file_digest(stream, "sha256").hexdigest()


def pull_sdk(framework, temp):
    channel = framework.removeprefix("net")
    tag = f"{SDK_REGISTRY}:{channel}"
    image_id = subprocess.check_output(["podman", "pull", "--quiet", tag], text=True).strip()
    if not re.fullmatch(r"(?:sha256:)?[0-9a-f]{64}", image_id):
        raise ValueError(f"Podman did not return an image ID for {tag}")
    sdk = SdkContainer(image_id, temp)
    actual = subprocess.check_output(sdk.command("dotnet", "--version"), text=True).strip()
    if not re.fullmatch(re.escape(channel) + r"\.\d+", actual):
        raise ValueError(f"Expected a GA SDK for {channel}, got {actual}")
    print(f"{framework}: using SDK {actual} from {tag} ({image_id})", flush=True)
    return sdk


def project_locks():
    for project in ET.parse(SOLUTION).iter("Project"):
        directory = Path(project.attrib["Path"]).parent
        yield directory / "packages.lock.json"


def prepare_lane_locks(framework):
    # An older SDK cannot restore the newer targets. Use the matching slice of
    # each tracked lock file so both restores use the same versions and hashes.
    for lock_path in project_locks():
        lock = json.loads(lock_path.read_text())
        lock["dependencies"] = {framework: lock["dependencies"][framework]}
        destination = lock_path.parent / "obj" / f"packages.{framework}.lock.json"
        destination.parent.mkdir(exist_ok=True)
        destination.write_text(json.dumps(lock))


def litty(sdk, framework, run_option, *args):
    sdk.dotnet("run", "--project", PACKAGE_PROJECTS[TOOL], "-c", "Release",
               "--framework", framework, run_option, "--", *args)


def build_solution(sdk, framework):
    sdk.dotnet("restore", SOLUTION, "--locked-mode", online=True)
    litty(sdk, framework, "--no-restore", "build", SOLUTION, "-c", "Release", "--no-restore")


def smoke_test(sdk, framework):
    name = f"smoke-{framework}"
    workdir = sdk.state / name
    workdir.mkdir()
    container_dir = CI_STATE / name
    configuration = ET.Element("configuration")
    sources = ET.SubElement(configuration, "packageSources")
    ET.SubElement(sources, "clear")
    ET.SubElement(sources, "add", key="local", value=str(WORKSPACE / PACKAGE_DIR))
    ET.SubElement(sources, "add", key="nuget.org", value="https://api.nuget.org/v3/index.json")
    mapping = ET.SubElement(configuration, "packageSourceMapping")
    ET.SubElement(ET.SubElement(mapping, "packageSource", key="local"), "package", pattern="LittyLogs*")
    ET.SubElement(ET.SubElement(mapping, "packageSource", key="nuget.org"), "package", pattern="*")
    config_path = workdir / "NuGet.Config"
    ET.ElementTree(configuration).write(config_path)

    project = ET.Element("Project", Sdk="Microsoft.NET.Sdk")
    props = ET.SubElement(project, "PropertyGroup")
    ET.SubElement(props, "TargetFramework").text = framework
    ET.SubElement(props, "OutputType").text = "Exe"
    references = ET.SubElement(project, "ItemGroup")
    project_version = version()
    for name in LIBRARIES:
        ET.SubElement(references, "PackageReference", Include=name, Version=f"[{project_version}]")
    ET.ElementTree(project).write(workdir / "Smoke.csproj")

    runtime_major = int(framework[3:-2])
    checks = [f"_ = typeof({name}).Assembly.FullName;" for name in LIBRARIES.values()]
    checks.append(f"return System.Environment.Version.Major == {runtime_major} ? 0 : 1;")
    (workdir / "Program.cs").write_text("\n".join(checks) + "\n")
    options = {"workdir": container_dir, "packages": container_dir / "packages"}
    sdk.dotnet("restore", "Smoke.csproj", "--configfile", config_path.name, online=True, **options)
    sdk.dotnet("run", "--project", "Smoke.csproj", "--no-restore", **options)
    sdk.dotnet("tool", "install", TOOL, "--version", project_version, "--framework", framework,
               "--tool-path", "tools", "--configfile", config_path.name, online=True, **options)
    result = subprocess.check_output(sdk.command("./tools/litty", **options), text=True)
    if "litty test" not in result:
        raise ValueError(f"Unexpected litty output on {framework}")


def package_paths():
    return [PACKAGE_DIR / f"{name}.{version()}.nupkg" for name in PACKAGE_PROJECTS]


def assert_package_targets(targets):
    for package in package_paths():
        name = package.name.removesuffix(f".{version()}.nupkg")
        with zipfile.ZipFile(package) as archive:
            entries = set(archive.namelist())
        for target in targets:
            payload = f"tools/{target}/any/DotnetToolSettings.xml" if name == TOOL else f"lib/{target}/{name}.dll"
            if payload not in entries:
                raise ValueError(f"Missing {payload} in {package}")


def package_manifest():
    packages = package_paths()
    expected = {package.name for package in packages}
    actual = {path.name for path in PACKAGE_DIR.iterdir() if path != MANIFEST}
    if actual != expected or any(not path.is_file() or path.is_symlink() for path in packages):
        raise ValueError(f"Unexpected package files: expected {sorted(expected)}, got {sorted(actual)}")
    revision = subprocess.check_output(["git", "rev-parse", "HEAD"], text=True).strip()
    return {"revision": revision, "sha256": {package.name: sha(package) for package in packages}}


def build_and_test():
    targets = frameworks()
    with tempfile.TemporaryDirectory(prefix="litty-dotnet-") as directory:
        temp = Path(directory)
        (temp / "home").mkdir()
        sdks = {}
        for target in targets:
            sdk = pull_sdk(target, temp)
            sdks[target] = sdk
            prepare_lane_locks(target)
            lane = replace(sdk, framework=target)
            build_solution(lane, target)
            litty(lane, target, "--no-build", "test", "--solution", SOLUTION, "-c", "Release", "--no-build")

        newest = targets[-1]
        latest = sdks[newest]
        build_solution(latest, newest)
        for project in PACKAGE_PROJECTS.values():
            litty(latest, newest, "--no-build", "pack", project,
                  "-c", "Release", "--no-build", "-o", WORKSPACE / PACKAGE_DIR)
        assert_package_targets(targets)
        for target in targets:
            smoke_test(sdks[target], target)

    MANIFEST.write_text(json.dumps(package_manifest(), indent=2) + "\n")


def verify_packages():
    if json.loads(MANIFEST.read_text()) != package_manifest():
        raise SystemExit("Package artifact does not match the checked-out revision and digests")


def push_packages():
    api_key = os.environ.pop("NUGET_API_KEY", "")
    if not api_key:
        raise SystemExit("NUGET_API_KEY is required to publish packages")
    verify_packages()
    with tempfile.TemporaryDirectory(prefix="litty-nuget-push-") as directory:
        temp = Path(directory)
        (temp / "home").mkdir()
        sdk = pull_sdk(frameworks()[-1], temp)
        command = sdk.command(
            "sh", "-eu", "-c",
            'for package do dotnet nuget push "$package" --api-key "$NUGET_API_KEY" '
            '--source https://api.nuget.org/v3/index.json --skip-duplicate; done',
            "nuget-push", *package_paths(),
            online=True, read_only=True, forward_env=("NUGET_API_KEY",),
        )
        subprocess.run(command, env={**os.environ, "NUGET_API_KEY": api_key}, check=True)


if __name__ == "__main__":
    match sys.argv[1:]:
        case ["build"]:
            build_and_test()
        case ["verify-packages"]:
            verify_packages()
        case ["push-packages"]:
            push_packages()
        case _:
            raise SystemExit("Usage: dotnet-ci.py build|verify-packages|push-packages")
