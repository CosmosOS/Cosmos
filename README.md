<p align="center">
  <img src="https://user-images.githubusercontent.com/63316499/89792973-43587480-daf3-11ea-99d6-82f89dd2ffc3.png" width="25%" />
  &nbsp;&nbsp;&nbsp;&nbsp;
  <img src="https://img.shields.io/badge/gen3-preview-yellow?style=for-the-badge" align="bottom" />
</p>

<p align="center">
  <a href="https://discord.com/invite/kwtBwv6jhD"><img src="https://img.shields.io/discord/833970409337913344?label=discord&logo=discord&color=5865F2" /></a>
  <a href="https://github.com/CosmosOS/Cosmos/actions/workflows/dotnet.yml"><img src="https://img.shields.io/github/actions/workflow/status/CosmosOS/Cosmos/dotnet.yml?branch=gen3&label=.NET%20tests&logo=github" /></a>
  <a href="https://github.com/CosmosOS/Cosmos/actions/workflows/kernel-tests.yml"><img src="https://img.shields.io/github/actions/workflow/status/CosmosOS/Cosmos/kernel-tests.yml?branch=gen3&label=kernel%20tests&logo=github" /></a>
  <a href="https://github.com/CosmosOS/Cosmos/actions/workflows/release.yml"><img src="https://img.shields.io/github/actions/workflow/status/CosmosOS/Cosmos/release.yml?label=release&logo=github" /></a>
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
- [Cosmos plug system](https://cosmosos.github.io/articles/dev/plugs.html)
- Native runtime stubs
- .NET runtime support (String, Collections, List, Dictionary, Math, Console, Date Time, Random, Bit Operations, Threading, Generics)
- [Mark-and-sweep Garbage Collector](https://cosmosos.github.io/articles/dev/garbage-collector.html)
- [Priority-based Stride Scheduler](https://cosmosos.github.io/articles/dev/scheduler.html)
- Exception handling
- Interrupts (APIC on x64, GIC on ARM64)
- ACPI (via [LAI](https://github.com/managarm/lai))
- PCI and MMIO drivers
- UART serial
- [Cosmos Graphics Subsystem](https://cosmosos.github.io/articles/user/graphics.html), double-buffered Canvas API (shapes, text fonts, images) on the UEFI GOP framebuffer
- [Keyboard](https://cosmosos.github.io/articles/user/keyboard.html) and [Mouse](https://cosmosos.github.io/articles/user/mouse.html) input
- [Network stack](https://cosmosos.github.io/articles/user/network.html), standard `System.Net.Sockets` TCP/UDP over ARP, IPv4, DHCP and DNS (no HTTPS *yet*)
- Storage drivers (AHCI/SATA, NVMe) with MBR, GPT and EBR partitioning
- [FAT12/16/32 filesystem](https://cosmosos.github.io/articles/user/filesystem.html) on a Unix-style VFS (mount, superblocks, inodes), exposed through the standard `System.IO` API
- Timer / Clock

## Documentation

[Documentation site](https://cosmosos.github.io/index.html): split into a **User Guide** (build your own OS with Cosmos) and **Contributor Docs** (contribute to Cosmos itself / architecture internals).

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

See the live list on the [Contributors page](https://github.com/CosmosOS/Cosmos/graphs/contributors).

## License

[BSD 3-Clause](LICENSE), the original Cosmos license. Copyright (c) 2007-2026, CosmosOS, COSMOS Project.

