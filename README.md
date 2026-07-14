<p align="center">
  <img src="https://user-images.githubusercontent.com/63316499/89792973-43587480-daf3-11ea-99d6-82f89dd2ffc3.png" width="25%" />
  &nbsp;&nbsp;&nbsp;&nbsp;
  <img src="https://img.shields.io/badge/gen3-preview-yellow?style=for-the-badge" align="bottom" />
</p>

<p align="center">
  <a href="https://discord.com/invite/kwtBwv6jhD"><img src="https://img.shields.io/discord/833970409337913344?label=discord&logo=discord&color=5865F2" /></a>
  <a href="https://github.com/valentinbreiz/nativeaot-patcher/actions/workflows/dotnet.yml"><img src="https://img.shields.io/github/actions/workflow/status/valentinbreiz/nativeaot-patcher/dotnet.yml?branch=main&label=.NET%20tests&logo=github" /></a>
  <a href="https://github.com/valentinbreiz/nativeaot-patcher/actions/workflows/kernel-tests.yml"><img src="https://img.shields.io/github/actions/workflow/status/valentinbreiz/nativeaot-patcher/kernel-tests.yml?branch=main&label=kernel%20tests&logo=github" /></a>
  <a href="https://github.com/valentinbreiz/nativeaot-patcher/actions/workflows/release.yml"><img src="https://img.shields.io/github/actions/workflow/status/valentinbreiz/nativeaot-patcher/release.yml?label=release&logo=github" /></a>
  <a href="https://valentinbreiz.github.io/nativeaot-patcher/roadmap.html"><img src="https://img.shields.io/badge/gen3_release-90%25-yellow" /></a>
</p>

# Cosmos gen3

A bare-metal C# kernel framework built on **NativeAOT**. Cosmos gen3 is the next generation of the [Cosmos](https://github.com/CosmosOS/Cosmos) operating system project, replacing the IL2CPU transpiler with the official .NET ahead-of-time compiler. The result is an ordinary `dotnet build` that produces a bootable kernel ELF for **x64 or ARM64**, linked with an integrated runtime, plugged with the Cosmos plug system, and packaged into an ISO with the Limine bootloader.

Originally based on [Zarlo's NativeAOT patcher](https://gitlab.com/liquip/nativeaot-patcher). See [CosmosOS/Cosmos#3088](https://github.com/CosmosOS/Cosmos/issues/3088) for the design discussion behind the gen3 effort.

## Why gen3?

Cosmos gen2 (the current public Cosmos OS) compiles C# IL to x86 assembly through **IL2CPU**, a custom transpiler. IL2CPU is powerful but maintains its own JIT-like backend separate from the .NET ecosystem. Gen3 replaces it with **NativeAOT**, the official .NET ahead-of-time toolchain, so kernels benefit from the same optimizer used in the wider .NET ecosystem and stay aligned with upstream as it evolves. This also makes it possible to support modern .NET features and additional architectures (currently ARM64 and RISC-V in the future) without re-implementing them in the toolchain.

## Features

- NativeAOT compilation
- x64 and ARM64
- [Limine](https://github.com/Limine-Bootloader/Limine) boot protocol
- [Cosmos plug system](https://valentinbreiz.github.io/nativeaot-patcher/articles/dev/plugs.html)
- Native runtime stubs
- .NET runtime support (String, Collections, List, Dictionary, Math, Console, Date Time, Random, Bit Operations, Threading, Generics)
- [Mark-and-sweep Garbage Collector](https://valentinbreiz.github.io/nativeaot-patcher/articles/dev/garbage-collector.html)
- [Priority-based Stride Scheduler](https://valentinbreiz.github.io/nativeaot-patcher/articles/dev/scheduler.html)
- Exception handling
- Interrupts (APIC on x64, GIC on ARM64)
- ACPI (via [LAI](https://github.com/managarm/lai))
- PCI and MMIO drivers
- UART serial
- [Cosmos Graphics Subsystem](https://valentinbreiz.github.io/nativeaot-patcher/articles/user/graphics.html), double-buffered Canvas API (shapes, text fonts, images) on the UEFI GOP framebuffer
- Keyboard and Mouse input
- [Network stack](https://valentinbreiz.github.io/nativeaot-patcher/articles/user/network.html), standard `System.Net.Sockets` TCP/UDP over ARP, IPv4, DHCP and DNS (no HTTPS *yet*)
- Storage drivers (AHCI/SATA, NVMe) with MBR, GPT and EBR partitioning
- [FAT12/16/32 filesystem](https://valentinbreiz.github.io/nativeaot-patcher/articles/user/filesystem.html) on a Unix-style VFS (mount, superblocks, inodes), exposed through the standard `System.IO` API
- Timer / Clock

## Documentation

[Documentation site](https://valentinbreiz.github.io/nativeaot-patcher/index.html) — split into a **User Guide** (build your own OS with Cosmos) and **Developer Docs** (contribute to Cosmos itself / architecture internals).

**User Guide**

- [Installation Guide](docs/articles/user/install.md)
- [Kernel Startup](docs/articles/user/startup.md)
- [File System](docs/articles/user/filesystem.md)
- [Network](docs/articles/user/network.md)
- [Graphics](docs/articles/user/graphics.md)
- [Debugging with VS Code and QEMU](docs/articles/user/debugging.md)

**Developer Docs**

- [Dev Container Setup](docs/articles/dev/install-dev.md)
- [Kernel Project Layout](docs/articles/dev/kernel-project-layout.md)
- [Coding Guidelines](docs/articles/dev/coding-guidelines.md)
- [Plugs](docs/articles/dev/plugs.md)
- [Testing](docs/articles/dev/testing.md)
- [Garbage Collector](docs/articles/dev/garbage-collector.md), [Precise Stack Scan (GCInfo)](docs/articles/dev/garbage-collector-gcinfo.md)
- [Scheduler](docs/articles/dev/scheduler.md)
- [Kernel Compilation Steps](docs/articles/dev/build/kernel-compilation-steps.md)
- [Cosmos.Build.Asm](docs/articles/dev/build/asm-build.md), [.GCC](docs/articles/dev/build/gcc-build.md), [.Patcher](docs/articles/dev/build/patcher-build.md), [.Ilc](docs/articles/dev/build/ilc-build.md)

## Related resources

- [Cosmos Gen3: The NativeAOT Era and the End of IL2CPU?](https://valentin.bzh/posts/3)
- [NativeAOT Developer Workflow](https://github.com/dotnet/runtime/blob/main/docs/workflow/building/coreclr/nativeaot.md)
- [NativeAOT Limitations](https://github.com/dotnet/runtime/blob/main/src/coreclr/nativeaot/docs/limitations.md)
- [Limine Boot Protocol](https://github.com/limine-bootloader/limine)

## Contributors

Thanks to everyone who has contributed code, reviews, plugs, and bug reports:

- [@zarlo](https://github.com/zarlo)
- [@valentinbreiz](https://github.com/valentinbreiz)
- [@Guillermo-Santos](https://github.com/Guillermo-Santos)
- [@kumja1](https://github.com/kumja1)
- [@AzureianGH](https://github.com/AzureianGH)
- [@warquys](https://github.com/warquys)
- [@ascpixi](https://github.com/ascpixi)
- [@Demiomad](https://github.com/Demiomad)
- [@ilobilo](https://github.com/ilobilo)
- [@spectradevv](https://github.com/spectradevv)
- All [Cosmos gen2 contributors](https://github.com/CosmosOS/Cosmos/graphs/contributors)

See the live list on the [Contributors page](https://github.com/valentinbreiz/nativeaot-patcher/graphs/contributors).

## License

[MIT](LICENSE) — Copyright (c) 2024 Kaleb McGhie (zarlo) and contributors.
