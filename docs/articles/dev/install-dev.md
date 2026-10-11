# Dev Container Setup

When developing or contributing to gen3, a [Dev Container](https://containers.dev/) is provided to set up a consistent environment with all required tools. After the container is created, the `postCreateCommand` script builds all Cosmos packages and installs the necessary global tools.

If you don't want to use Docker you can install the framework directly to your host machine by running the script manually.

## Linux / macOS (`postCreateCommand.sh`)

Run the script from the root of the repository:

```bash
./.devcontainer/postCreateCommand.sh
```

### What it does

1. **Clears the NuGet cache** for any existing `cosmos.*` packages to ensure a clean restore.
2. **Removes all build artifacts** (`artifacts/`) and intermediate `obj` folders under `src/`.
3. **Registers a local NuGet source** at `artifacts/package/release` so that locally built packages are preferred during restore.
4. **Builds and packs every Cosmos package** in dependency order:
   - Base libraries (`Cosmos.Build.API`, `Cosmos.Build.Common`)
   - Build tools (`Cosmos.Build.Asm`, `Cosmos.Build.CC`, `Cosmos.Build.Ilc`, `Cosmos.Build.Patcher`, `Cosmos.Patcher`, `Cosmos.Tools`)
   - Native runtime packages for x64 and ARM64 (`Cosmos.Kernel.Native.*`)
   - Architecture-independent kernel packages (`Cosmos.Kernel.Drivers`, `Cosmos.Kernel.Debug`, `Cosmos.Kernel.Boot.Limine`, `Cosmos.Kernel.SourceGenerators`)
   - Architecture-specific HAL packages (`Cosmos.Kernel.HAL.X64`, `Cosmos.Kernel.HAL.ARM64`)
   - Multi-arch kernel packages built for both `linux-x64` and `linux-arm64` (`Cosmos.Kernel.Core`, `Cosmos.Kernel.HAL`, `Cosmos.Kernel.System`, `Cosmos.Kernel.Plugs`, `Cosmos.Kernel`)
   - SDK and templates (`Cosmos.Sdk`, `Cosmos.Build.Templates`)
5. **Restores** the main solution (`nativeaot-patcher.slnx`).
6. **Installs global .NET tools**: `ilc`, `Cosmos.Patcher`, and `Cosmos.Tools`.

## Windows (`postCreateCommand.ps1`)

Run the PowerShell script from the root of the repository:

```powershell
.\.devcontainer\postCreateCommand.ps1
```

The Windows script performs the same steps as the Linux script using PowerShell equivalents:

1. **Clears the NuGet cache** under `%USERPROFILE%\.nuget\packages\cosmos.*`.
2. **Removes all build artifacts** and `obj` folders under `src/`.
3. **Registers the local NuGet source** at `artifacts/package/release`.
4. **Builds and packs every Cosmos package** in the same dependency order as on Linux.
5. **Restores** the main solution.
6. **Installs global .NET tools**: `ilc`, `Cosmos.Patcher`, and `Cosmos.Tools`.

## Verifying the Setup

After the script completes, confirm that the packages were created:

```bash
ls artifacts/package/release/*.nupkg
```

And verify the global tools are available:

```bash
ilc --version
cosmos.patcher --version
cosmos --version
```

If any step fails, re-run the script: it is designed to be idempotent and will clean up previous state before rebuilding.
