# Installation Guide

## Prerequisites

- [.NET SDK 10.0](https://dotnet.microsoft.com/download) or later
- An IDE with its Cosmos extension: [Visual Studio Code](https://code.visualstudio.com/), [Visual Studio](https://visualstudio.microsoft.com/) or [JetBrains Rider](https://www.jetbrains.com/rider/) (see [IDE Extensions](#ide-extensions))
- [Homebrew](https://brew.sh/) on macOS

## Windows

Download and run the latest installer from the [Releases](https://github.com/CosmosOS/Cosmos/releases) page:

```
CosmosSetup-<version>-windows.exe
```

The installer looks for VS Code, Visual Studio and Rider and offers to install the Cosmos extension in each one it finds (see [IDE Extensions](#ide-extensions)). Visual Studio must be closed, and the Visual Studio extension asks for administrator rights because it installs for all users. Rider is found only once it has been started at least once.

After installation, open a new terminal and verify:

```powershell
cosmos check
```

To uninstall, use **Add or Remove Programs** in Windows Settings.

## Linux / macOS

Install the Cosmos CLI and all dependencies:

```bash
dotnet tool install -g Cosmos.Tools
cosmos install
```

This downloads the prebuilt toolchain bundles (Clang and LLD, xorriso, QEMU, GDB) from the `tools-latest` GitHub release into `~/.cosmos/tools/`, then installs the Cosmos patcher, the project templates, and the VS Code extension. A tool already on `PATH` is used instead of the bundle when its version matches the bundled one.

Verify installation:

```bash
cosmos check
```

To uninstall:

```bash
cosmos uninstall
dotnet tool uninstall -g Cosmos.Tools
```

To update:

```bash
cosmos update
```

Run it inside a kernel project directory to also move the project's Cosmos version pins (the `Sdk="Cosmos.Sdk/..."` attribute and `Cosmos.*` package references) to the latest release. Additional options:

| Option | Effect |
|--------|--------|
| `cosmos update --check` | Report available updates without installing anything |
| `cosmos update --no-project` | Update the tools but leave project files untouched |
| `cosmos update --version <VERSION>` | Move the CLI, patcher, templates, and project pins to a specific version (system tools always follow the `tools-latest` bundles) |

## macOS on Apple Silicon

The tool bundles are built for Apple Silicon only (`darwin-arm64`); there is no Intel bundle. An Apple Silicon Mac needs the following steps in addition to the Linux / macOS steps. `cosmos check` does not report a missing Rosetta 2 or SDL 3.

| Step | Why |
|------|-----|
| Install Rosetta 2 | The SDK runs the x64 ILCompiler (`runtime.osx-x64.Microsoft.DotNet.ILCompiler`) on every macOS host. Without Rosetta 2, `cosmos build` fails with `error MSB3073` and ILC exit code 126. |
| Install SDL 3 with Homebrew | The bundled QEMU loads SDL 3 from Homebrew for its display window and aborts at startup when the library is missing. `cosmos run --headless` does not need it. |
| Re-sign the bundled QEMU | macOS kills the bundled binary at launch when its code signature is missing or invalid. The same applies to `qemu-system-x86_64.real` when running x64 kernels. |
| Pass `-a arm64` to `cosmos run` | `cosmos build` targets arm64 by default on Apple Silicon, but `cosmos run` defaults to x64 and looks for the ISO in `output-x64/`. |

```bash
softwareupdate --install-rosetta
brew install sdl3
codesign --force --deep --sign - ~/.cosmos/tools/qemu/qemu-system-aarch64.real
codesign --force --deep --sign - ~/.cosmos/tools/qemu/qemu-system-x86_64.real
```

Then build and run:

```bash
cosmos build
cosmos run -a arm64
```

If a build failed before Rosetta 2 was installed, delete the project's `obj/` folder before building again.

## IDE Extensions

Each extension creates, builds, runs and debugs a kernel from the IDE, and can check and install the toolchain itself.

| IDE | Extension | Install |
|-----|-----------|---------|
| Visual Studio Code | [CosmosVsCodeExtension](https://github.com/CosmosOS/CosmosVsCodeExtension) | The Windows installer or `cosmos install` |
| Visual Studio 2026 (or 2022 17.14+) | [CosmosVsExtension](https://github.com/CosmosOS/CosmosVsExtension) | The Windows installer, or the `.vsix` from the [releases page](https://github.com/CosmosOS/CosmosVsExtension/releases) |
| JetBrains Rider 2025.3+ | [CosmosRiderExtension](https://github.com/CosmosOS/CosmosRiderExtension) | The Windows installer, or the plugin from the [releases page](https://github.com/CosmosOS/CosmosRiderExtension/releases) |

## Quick Start

Once installed, see [Kernel Startup](startup.md) to create your first kernel and learn the boot flow.
