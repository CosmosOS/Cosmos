<!-- source_hash: dd6f10fed2a906f43f3a23dfe53d4d886fd99ccb -->
<p align="center">
  <img src="https://user-images.githubusercontent.com/63316499/89792973-43587480-daf3-11ea-99d6-82f89dd2ffc3.png" width="25%" />
</p>

<p align="center">
  <a href="https://chat.gocosmos.org"><img src="https://img.shields.io/badge/%E5%AE%98%E6%96%B9-%E8%81%8A%E5%A4%A9-blue?logo=matrix" /></a>
  <a href="https://github.com/CosmosOS/Cosmos/actions/workflows/dotnet.yml"><img src="https://img.shields.io/github/actions/workflow/status/CosmosOS/Cosmos/dotnet.yml?branch=gen3&label=.NET%20%E6%B5%8B%E8%AF%95&logo=github" /></a>
  <a href="https://github.com/CosmosOS/Cosmos/actions/workflows/kernel-tests.yml"><img src="https://img.shields.io/github/actions/workflow/status/CosmosOS/Cosmos/kernel-tests.yml?branch=gen3&label=%E5%86%85%E6%A0%B8%E6%B5%8B%E8%AF%95&logo=github" /></a>
  <a href="https://github.com/CosmosOS/Cosmos/actions/workflows/release.yml"><img src="https://img.shields.io/github/actions/workflow/status/CosmosOS/Cosmos/release.yml?label=%E5%8F%91%E5%B8%83&logo=github" /></a>
</p>

[English](README.md) | [Français](README_FR.md) | 简体中文 | [Polski](README_PL.md)

# Cosmos gen3

一个基于 **NativeAOT** 构建的裸机 C# 内核框架。Cosmos gen3 是 [Cosmos](https://github.com/CosmosOS/Cosmos) 操作系统项目的下一代，它用 .NET 官方的 AOT（ahead-of-time，预先）编译器取代了 IL2CPU 转译器。其结果是：一次普通的 `dotnet build` 就能生成适用于 **x64 或 ARM64** 的可引导内核 ELF，它链接了集成的运行时，经 Cosmos plug 系统用 plug 替换了所需的实现，并与 Limine 引导加载程序一起打包成 ISO。

最初基于 [Zarlo 的 NativeAOT patcher](https://gitlab.com/liquip/nativeaot-patcher)。gen3 背后的设计讨论见 [CosmosOS/Cosmos#3088](https://github.com/CosmosOS/Cosmos/issues/3088)。

## 为什么是 gen3？

Cosmos gen2 通过自定义的转译器 **IL2CPU** 把 C# IL 编译为 x86 汇编。IL2CPU 功能强大，但它维护着一套独立于 .NET 生态系统、类似 JIT 的后端。Gen3 用 .NET 官方的 AOT 工具链 **NativeAOT** 取代了它，因此内核能受益于整个 .NET 生态系统所使用的同一个优化器，并随上游的演进保持同步。这也让支持现代 .NET 特性和更多架构（目前是 ARM64，将来是 RISC-V）成为可能，而无需在工具链中重新实现它们。

## 功能

- NativeAOT 编译
- x64 和 ARM64
- [Limine](https://github.com/Limine-Bootloader/Limine) 引导协议
- [Cosmos plug 系统](https://cosmosos.github.io/zh/articles/dev/plugs.html)
- .NET 运行时支持
- [标记-清除垃圾回收器](https://cosmosos.github.io/zh/articles/dev/garbage-collector.html)
- [基于优先级的 Stride 调度器](https://cosmosos.github.io/zh/articles/dev/scheduler.html)
- 异常处理
- 中断（x64 上为 APIC，ARM64 上为 GIC）
- ACPI（通过 [LAI](https://github.com/managarm/lai)）
- PCI 和 MMIO 驱动程序
- UART 串口
- [Cosmos 图形子系统](https://cosmosos.github.io/zh/articles/user/graphics.html)
- [键盘](https://cosmosos.github.io/zh/articles/user/keyboard.html)和[鼠标](https://cosmosos.github.io/zh/articles/user/mouse.html)输入
- [网络协议栈](https://cosmosos.github.io/zh/articles/user/network.html)
- 存储驱动程序（AHCI/SATA、NVMe、virtio-blk、USB 大容量存储），支持 MBR、GPT 和 EBR 分区
- 基于 Unix 风格 VFS（挂载、超级块、inode）的 [FAT12/16/32 文件系统](https://cosmosos.github.io/zh/articles/user/filesystem.html)
- 定时器 / 时钟

## 文档

[文档站点](https://cosmosos.github.io/zh/index.html)：分为**用户指南**（用 Cosmos 构建你自己的操作系统）和**贡献者文档**（为 Cosmos 本身做贡献，了解架构的内部实现）。

## 许可证

[BSD 3-Clause](LICENSE)，即 Cosmos 最初的许可证。Copyright (c) 2007-2026, CosmosOS, COSMOS Project.
