---
source_hash: e13b28e10d02259f3eddc1aeab6dbb7dd2d1a471
---
<a id="dev-container-setup"></a>

# 开发容器配置

在开发 gen3 或为其做贡献时，我们提供了一个[开发容器](https://containers.dev/)（Dev Container），用来搭建一个包含所有必需工具的一致环境。容器创建后，`postCreateCommand` 脚本会构建所有 Cosmos 包，并安装所需的全局工具。

如果你不想使用 Docker，也可以手动运行该脚本，把框架直接安装到你的主机上。

## Linux / macOS (`postCreateCommand.sh`)

在仓库根目录下运行该脚本：

```bash
./.devcontainer/postCreateCommand.sh
```

<a id="what-it-does"></a>

### 脚本做了什么

1. **清除 NuGet 缓存**中所有已有的 `cosmos.*` 包，以确保还原过程是干净的。
2. **删除所有构建产物**（`artifacts/`）以及 `src/` 下的中间 `obj` 文件夹。
3. **注册一个本地 NuGet 源**，位于 `artifacts/package/release`，使还原时优先使用本地构建的包。
4. **按依赖顺序构建并打包每个 Cosmos 包**：
   - 基础库（`Cosmos.Build.API`、`Cosmos.Build.Common`）
   - 构建工具（`Cosmos.Build.Asm`、`Cosmos.Build.CC`、`Cosmos.Build.Ilc`、`Cosmos.Build.Patcher`、`Cosmos.Patcher`、`Cosmos.Tools`）
   - 面向 x64 和 ARM64 的原生运行时包（`Cosmos.Kernel.Native.*`）
   - 与架构无关的内核包（`Cosmos.Kernel.Drivers`、`Cosmos.Kernel.Debug`、`Cosmos.Kernel.Boot.Limine`、`Cosmos.Kernel.SourceGenerators`）
   - 特定于架构的 HAL 包（`Cosmos.Kernel.HAL.X64`、`Cosmos.Kernel.HAL.ARM64`）
   - 同时为 `linux-x64` 和 `linux-arm64` 构建的多架构内核包（`Cosmos.Kernel.Core`、`Cosmos.Kernel.HAL`、`Cosmos.Kernel.System`、`Cosmos.Kernel.Plugs`、`Cosmos.Kernel`）
   - SDK 和模板（`Cosmos.Sdk`、`Cosmos.Build.Templates`）
5. **还原**主解决方案（`nativeaot-patcher.slnx`）。
6. **安装全局 .NET 工具**：`ilc`、`Cosmos.Patcher` 和 `Cosmos.Tools`。

## Windows (`postCreateCommand.ps1`)

在仓库根目录下运行该 PowerShell 脚本：

```powershell
.\.devcontainer\postCreateCommand.ps1
```

Windows 脚本使用 PowerShell 中的等效操作，执行与 Linux 脚本相同的步骤：

1. **清除 NuGet 缓存**，即 `%USERPROFILE%\.nuget\packages\cosmos.*` 下的内容。
2. **删除所有构建产物**以及 `src/` 下的 `obj` 文件夹。
3. **注册本地 NuGet 源**，位于 `artifacts/package/release`。
4. **构建并打包每个 Cosmos 包**，依赖顺序与 Linux 上相同。
5. **还原**主解决方案。
6. **安装全局 .NET 工具**：`ilc`、`Cosmos.Patcher` 和 `Cosmos.Tools`。

<a id="verifying-the-setup"></a>

## 验证配置

脚本完成后，确认包已经生成：

```bash
ls artifacts/package/release/*.nupkg
```

然后验证全局工具是否可用：

```bash
ilc --version
cosmos.patcher --version
cosmos --version
```

如果任何一步失败，重新运行脚本即可：它被设计为幂等的，会在重新构建之前清理之前的状态。
