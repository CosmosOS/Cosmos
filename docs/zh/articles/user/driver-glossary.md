---
source_hash: 1deddb819ca1cc5ef80b26e2c096f38584d59efc
---
<a id="driver-glossary"></a>

# 驱动程序术语表

这些说明为从未编写过驱动程序的读者提供硬件、操作系统和构建方面的背景知识，[编写驱动程序](drivers.md)以及贡献者文档中关于[驱动程序清单](../dev/build/driver-manifest.md)的页面都以这些知识为基础。每一页先单独解释概念本身，最后说明 Cosmos 如何应用它。下表把它们分为四个部分：驱动程序如何访问设备，设备与驱动程序如何相互通知，各种总线，以及内核如何带上驱动程序并为每个设备选出一个驱动程序。中断上下文还以贡献者文档中的两篇调度说明为基础：[抢占](../dev/sched-concepts/preemption.md)和[指令边界](../dev/sched-concepts/instruction-boundary.md)。

| 概念 | 摘要 |
|---------|---------|
| [MMIO 与端口 I/O](driver-concepts/registers.md) | 通过内存或端口访问设备寄存器，以及映射如何缓存这些寄存器 |
| [物理地址](driver-concepts/physical-addresses.md) | 虚拟地址与物理地址的对比、物理连续性，以及设备要求的对齐 |
| [DMA](driver-concepts/dma.md) | 设备自行读写内存：总线主控、一致性、32 位限制 |
| [内存屏障](driver-concepts/barriers.md) | 为 CPU 的加载和存储排序，使设备按顺序看到它们 |
| [描述符环](driver-concepts/descriptor-rings.md) | 与设备共享的环形描述符数组：所有权、头和尾、门铃 |
| [中断投递](driver-concepts/interrupt-delivery.md) | IRQ、确认设备、边沿触发与电平触发、INTx 与 MSI-X 的对比 |
| [中断上下文](driver-concepts/interrupt-context.md) | 为什么中断处理程序不能等待，为什么驱动程序的中断处理程序不能分配内存，以及它的工作如何拆分 |
| [总线与枚举](driver-concepts/buses.md) | 可枚举总线、由固件描述的平台设备，以及节点树 |
| [固件交接](driver-concepts/firmware-handoff.md) | UEFI 在内核运行之前配置了什么，以及 Cosmos 保留了什么 |
| [PCI](driver-concepts/pci.md) | PCI 功能、它的配置空间、类代码、能力和 BAR |
| [热插拔](driver-concepts/hot-plug.md) | 设备在运行时接入和离开，以及意外移除 |
| [Virtio](driver-concepts/virtio.md) | 半虚拟化设备：传输方式、状态握手、特性协商和 virtqueue |
| [USB](driver-concepts/usb.md) | 接口、端点、传输类型、轮询的中断端点、枚举、三种主机控制器接口及其伴随控制器和集线器、HID 启动协议 |
| [PS/2 与 8042 控制器](driver-concepts/ps2.md) | 8042 控制器、它的两个端口、命令、应答和扫描 |
| [源代码生成器](driver-concepts/source-generators.md) | 在构建时添加代码以代替反射的编译器插件，以及构建属性如何传递给它们 |
| [裁剪与功能开关](driver-concepts/trimming.md) | 把不可达代码排除在镜像之外，以及让某个分支变得不可达的构建时开关 |
| [驱动程序匹配](driver-concepts/driver-matching.md) | 匹配表、特异性和等级，以及多个驱动程序都适合同一个设备时确定性的最后一个键 |
