---
source_hash: afeee6239d1abb61814efdd6cb90f73dd0e1a4fd
---
<a id="pci-peripheral-component-interconnect"></a>

# PCI（外设部件互连）

PCI，以及沿用同一软件模型的后继者 PCI Express，连接着 PC 和许多 ARM 机器中的大多数设备。软件按设备所在的位置来寻址它：一个段（一组总线）、一个总线号、该总线上的设备号（0 到 31）和一个功能号（0 到 7）。功能是驱动程序绑定的单位，因为一个物理设备最多可以包含八个相互独立的功能，例如一块显卡和它的音频控制器。

每个功能都有一个配置空间（传统 PCI 中为 256 字节，PCI Express 中为 4 KiB），以一个标准头开始。头中的厂商 ID 和设备 ID 指明具体是哪一款芯片，三个类别字节（类别、子类别和编程接口）则说明它是哪一类设备。MSI-X（[中断投递](interrupt-delivery.md)）等可选特性在能力列表中声明，能力列表是一条由若干条目组成的链，每个条目包含一个 ID 和下一个条目的偏移量。头中还有命令寄存器，它的各个位决定该功能是否响应内存访问、是否响应 I/O 端口访问，以及是否可以自行发起 DMA（[总线主控](dma.md)）。

功能的寄存器没有固定地址。它的六个基址寄存器（BAR）各自请求一个固定大小的内存窗口或 I/O 端口范围，一个 64 位内存 BAR 会占用六个槽位中的两个。必须由某个环节为每个 BAR 分配地址，固件在启动期间完成这件事（[固件交接](firmware-handoff.md)），因此之后才插入的设备就没有地址（[热插拔](hot-plug.md)）。

访问配置空间有两种方式。x86 的传统机制把功能的地址写入端口 0xCF8，通过端口 0xCFC 传送数据，并且只能访问前 256 字节。ECAM（增强型配置访问机制）把每个功能完整的 4 KiB 映射到一个内存窗口中，窗口的基址由 ACPI 或设备树给出。

陷阱在于只按类别匹配。类别说明设备做什么，而不说明它如何编程：所有网络控制器的类别都是 0x02，不论由谁制造，因此按这个类别匹配的驱动程序会收到一些芯片，而它从未见过这些芯片的寄存器。只有在某个标准定义了编程模型的地方，类别字节才能确定编程模型，例如 xHCI、EHCI、OHCI、AHCI、NVMe 或 HD Audio。

在驱动工具包中，`PciMatch` 检查一个 `PciIdentity` 中的头字段，该标识的节点路径为 `pci:<segment>:<bus>:<device>.<function>`（[PCI 标识与匹配](../drivers.md#pci-identity-and-match)）；`PciAccess` 提供 `FindCapability`，并以 `EnableMemorySpace`、`EnableIoSpace` 和 `EnableBusMastering` 的形式提供命令寄存器的各个位（[PCI 访问对象](../drivers.md#the-pci-access-object)）。`Node.Resources` 保存各个 BAR，其地址由固件分配，或者在热插拔插槽后面由工具包放置；`None` 表示未分配的槽位或 64 位 BAR 的高半部分（[BAR 资源](../drivers.md#bar-resources)）。x64 上的主机是 `pci-host-legacy`，在它上面访问偏移量 255 之后的配置空间会抛出异常；ARM64 上的主机是 `pci-host-ecam-generic`（[平台节点](../drivers.md#platform-nodes)）。
