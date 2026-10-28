---
source_hash: efa0e487ab8d5e0d0a48f4f88281e5ec483a36ad
---
<a id="installation-guide"></a>

# 安装指南

<a id="prerequisites"></a>

## 前提条件

- [.NET SDK 10.0](https://dotnet.microsoft.com/download) 或更高版本
- 一个装有 Cosmos 扩展的 IDE：[Visual Studio Code](https://code.visualstudio.com/)、[Visual Studio](https://visualstudio.microsoft.com/) 或 [JetBrains Rider](https://www.jetbrains.com/rider/)（见 [IDE 扩展](#ide-extensions)）
- 在 macOS 上还需要 [Homebrew](https://brew.sh/)

## Windows

从 [Releases](https://github.com/CosmosOS/Cosmos/releases) 页面下载并运行最新的安装程序：

```
CosmosSetup-<version>-windows.exe
```

安装程序会查找 VS Code、Visual Studio 和 Rider，并为找到的每一个提供安装 Cosmos 扩展的选项（见 [IDE 扩展](#ide-extensions)）。Visual Studio 必须处于关闭状态，而且 Visual Studio 扩展会为所有用户安装，因此会请求管理员权限。Rider 只有在至少启动过一次之后才能被找到。

安装完成后，打开一个新的终端并验证：

```powershell
cosmos check
```

如需卸载，请使用 Windows 设置中的**添加或删除程序**。

## Linux / macOS

安装 Cosmos CLI 及其所有依赖项：

```bash
dotnet tool install -g Cosmos.Tools
cosmos install
```

该命令会从 GitHub 上的 `tools-latest` 发布版本下载预构建的工具链捆绑包（Clang 和 LLD、xorriso、QEMU、GDB）到 `~/.cosmos/tools/`，然后安装 Cosmos patcher、项目模板和 VS Code 扩展。如果 `PATH` 上已有某个工具，且其版本与捆绑包中的版本一致，则使用该工具而不是捆绑包。

验证安装：

```bash
cosmos check
```

如需卸载：

```bash
cosmos uninstall
dotnet tool uninstall -g Cosmos.Tools
```

如需更新：

```bash
cosmos update
```

在内核项目目录中运行该命令，还会把项目中固定的 Cosmos 版本（`Sdk="Cosmos.Sdk/..."` 属性和 `Cosmos.*` 包引用）更新到最新版本。其他选项：

| 选项 | 作用 |
|--------|--------|
| `cosmos update --check` | 报告可用的更新，但不安装任何内容 |
| `cosmos update --no-project` | 更新工具，但不改动项目文件 |
| `cosmos update --version <VERSION>` | 将 CLI、patcher、模板和项目中固定的版本切换到指定版本（系统工具始终跟随 `tools-latest` 捆绑包） |

<a id="macos-on-apple-silicon"></a>

## Apple Silicon 上的 macOS

工具捆绑包仅针对 Apple Silicon（`darwin-arm64`）构建，没有 Intel 版捆绑包。在 Apple Silicon Mac 上，除了 Linux / macOS 的步骤之外，还需要执行以下步骤。`cosmos check` 不会报告缺少 Rosetta 2 或 SDL 3。

| 步骤 | 原因 |
|------|-----|
| 安装 Rosetta 2 | SDK 在所有 macOS 主机上都会运行 x64 版 ILCompiler（`runtime.osx-x64.Microsoft.DotNet.ILCompiler`）。没有 Rosetta 2 时，`cosmos build` 会失败并报告 `error MSB3073`，ILC 退出码为 126。 |
| 使用 Homebrew 安装 SDL 3 | 捆绑包中的 QEMU 会从 Homebrew 加载 SDL 3 来创建显示窗口，缺少该库时会在启动时中止。`cosmos run --headless` 不需要它。 |
| 重新签名捆绑包中的 QEMU | 如果捆绑包中的二进制文件缺少代码签名或签名无效，macOS 会在启动时将其终止。运行 x64 内核时，`qemu-system-x86_64.real` 也是如此。 |
| 向 `cosmos run` 传入 `-a arm64` | 在 Apple Silicon 上，`cosmos build` 默认以 arm64 为目标，但 `cosmos run` 默认使用 x64，并在 `output-x64/` 中查找 ISO。 |

```bash
softwareupdate --install-rosetta
brew install sdl3
codesign --force --deep --sign - ~/.cosmos/tools/qemu/qemu-system-aarch64.real
codesign --force --deep --sign - ~/.cosmos/tools/qemu/qemu-system-x86_64.real
```

然后构建并运行：

```bash
cosmos build
cosmos run -a arm64
```

如果在安装 Rosetta 2 之前构建失败过，请先删除项目的 `obj/` 文件夹，再重新构建。

<a id="ide-extensions"></a>

## IDE 扩展

每个扩展都可以在 IDE 中创建、构建、运行和调试内核，并能自行检查和安装工具链。

| IDE | 扩展 | 安装方式 |
|-----|-----------|---------|
| Visual Studio Code | [CosmosVsCodeExtension](https://github.com/CosmosOS/CosmosVsCodeExtension) | Windows 安装程序或 `cosmos install` |
| Visual Studio 2026（或 2022 17.14+） | [CosmosVsExtension](https://github.com/CosmosOS/CosmosVsExtension) | Windows 安装程序，或 [Releases 页面](https://github.com/CosmosOS/CosmosVsExtension/releases)中的 `.vsix` |
| JetBrains Rider 2025.3+ | [CosmosRiderExtension](https://github.com/CosmosOS/CosmosRiderExtension) | Windows 安装程序，或 [Releases 页面](https://github.com/CosmosOS/CosmosRiderExtension/releases)中的插件 |

<a id="quick-start"></a>

## 快速入门

安装完成后，请参阅[内核启动](startup.md)，创建你的第一个内核并了解启动流程。
