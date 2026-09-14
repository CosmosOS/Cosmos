# Credits

Cosmos Gen3 stands on the shoulders of other open-source projects. This page lists the third-party components that ship as part of Cosmos, with their licenses.

## Vendored source code

Code copied into this repository (with local adaptations noted in a `README.md` next to the sources):

| Project | License | Used for | Location |
|---|---|---|---|
| [BigGustave](https://github.com/EliotJones/BigGustave) | Unlicense | PNG decoding (`Cosmos.Kernel.System.Graphics.Png`) | `src/Cosmos.Kernel.System/Graphics/Images/BigGustave/` |
| [SharpZipLib](https://github.com/icsharpcode/SharpZipLib) | MIT | Managed DEFLATE/zlib decompression for the PNG decoder | `src/Cosmos.Kernel.System/IO/Compression/SharpZipLib/` |
| [LunarFonts](https://github.com/Relfos/LunarFonts) | MIT | TrueType loading and rasterization (`TrueTypeFont`) | `src/Cosmos.Kernel.System/Graphics/Fonts/TrueType/LunarFonts/` |
| [Spleen](https://github.com/fcambus/spleen) | BSD-2-Clause | The default 16x32 console and canvas font | `src/Cosmos.Kernel.System/Graphics/Fonts/` |
| [Cosm3D](https://github.com/Samma2009/Cosm3D) | MIT | VMWareSVGAII Canvas driver (`Cosmos.Kernel.HAL.Devices.Graphic.SVGAII`) |

The PNG integration follows [CosmosPNG](https://github.com/Szymekk44/CosmosPNG) (Unlicense), which first brought BigGustave to Cosmos Gen2, and the TrueType integration follows [CosmosTTF](https://github.com/GoldenretriverYT/CosmosTTF) (MIT), which first brought LunarFonts there.

The SvgaII integration originates from a gen3 port of [Cosm3D](https://github.com/Samma2009/Cosm3D) (an implementation of the [3D SvgaII spec by VMware](https://sourceforge.net/projects/vmware-svga)), with the port itself based on the gen2 SvgaII driver.

## Shipped binaries and dependencies

| Project | License | Used for |
|---|---|---|
| [Limine](https://github.com/limine-bootloader/limine) | BSD-2-Clause | The bootloader included in every kernel ISO |
| [.NET Runtime](https://github.com/dotnet/runtime) | MIT | The base class libraries and NativeAOT compiler (ILC) the kernel is built on |
| [Mono.Cecil](https://github.com/jbevain/cecil) | MIT | IL reading and rewriting in `cosmos-patcher` |

## Build and test tools

Not distributed with Cosmos, but required to build or test it: [YASM](https://yasm.tortall.net/), GNU binutils/GCC, [xorriso](https://www.gnu.org/software/xorriso/) and [QEMU](https://www.qemu.org/).
