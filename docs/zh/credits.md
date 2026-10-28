---
source_hash: 86a75862e5053e5fdca2ca093d3a1a3f11e645a6
---
<a id="credits"></a>

# 致谢

Cosmos Gen3 站在其他开源项目的肩膀上。本页列出随 Cosmos 一起分发的第三方组件及其许可证。

<a id="vendored-source-code"></a>

## 内嵌源代码

复制到本仓库中的代码（本地改动记录在源代码旁的 `README.md` 中）：

| 项目 | 许可证 | 用途 | 位置 |
|---|---|---|---|
| [BigGustave](https://github.com/EliotJones/BigGustave) | Unlicense | PNG 解码（`Cosmos.Kernel.System.Graphics.Png`） | `src/Cosmos.Kernel.System/ThirdParty/BigGustave/` |
| [SharpZipLib](https://github.com/icsharpcode/SharpZipLib) | MIT | 为 PNG 解码器提供托管的 DEFLATE/zlib 解压缩 | `src/Cosmos.Kernel.System/ThirdParty/SharpZipLib/` |
| [LunarFonts](https://github.com/Relfos/LunarFonts) | MIT | TrueType 字体加载与光栅化（`TrueTypeFont`） | `src/Cosmos.Kernel.System/ThirdParty/LunarFonts/` |
| [Spleen](https://github.com/fcambus/spleen) | BSD-2-Clause | 控制台和画布默认使用的 16x32 字体 | `src/Cosmos.Kernel.System/Graphics/Fonts/` |
| [Cosm3D](https://github.com/Samma2009/Cosm3D) | MIT | VMware SVGA II 显示驱动程序的 SVGA3D 命令层与画布（`Cosmos.Kernel.Drivers.Pci.Display.VmwareSvga.VmwareSvga3D`、`VmwareSvgaCanvas3D`） | `src/Cosmos.Kernel.Drivers/Pci/Display/VmwareSvga/`：`VmwareSvga3D.cs`、`VmwareSvgaCanvas3D.cs` 以及 `Enums/` 和 `Structs/` 中的词汇类型 |

PNG 集成借鉴了 [CosmosPNG](https://github.com/Szymekk44/CosmosPNG)（Unlicense），它最早将 BigGustave 引入 Cosmos Gen2；TrueType 集成借鉴了 [CosmosTTF](https://github.com/GoldenretriverYT/CosmosTTF)（MIT），它最早将 LunarFonts 引入 Cosmos Gen2。

VMware SVGA II 集成源自 [Cosm3D](https://github.com/Samma2009/Cosm3D) 的 gen3 移植版（Cosm3D 是 [VMware 的 3D SVGA II 规范](https://sourceforge.net/projects/vmware-svga)的一个实现），而该移植本身基于 gen2 的 SVGA II 驱动程序；它以 `VmwareSvgaDriver` 的形式位于 `Cosmos.Kernel.Drivers` 中，构建在驱动工具包之上。

<a id="shipped-binaries-and-dependencies"></a>

## 随附的二进制文件与依赖项

| 项目 | 许可证 | 用途 |
|---|---|---|
| [Limine](https://github.com/limine-bootloader/limine) | BSD-2-Clause | 每个内核 ISO 中包含的引导加载程序 |
| [.NET Runtime](https://github.com/dotnet/runtime) | MIT | 内核构建所基于的基础类库和 NativeAOT 编译器（ILC） |
| [Mono.Cecil](https://github.com/jbevain/cecil) | MIT | `cosmos-patcher` 中的 IL 读取与重写 |

<a id="build-and-test-tools"></a>

## 构建与测试工具

它们不随 Cosmos 分发，但构建或测试 Cosmos 时需要：[YASM](https://yasm.tortall.net/)、GNU binutils/GCC、[xorriso](https://www.gnu.org/software/xorriso/) 和 [QEMU](https://www.qemu.org/)。
