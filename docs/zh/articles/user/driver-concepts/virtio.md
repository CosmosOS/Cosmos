---
source_hash: d91a00f53f7a9b62286cced63c4f728c423ded4d
---
# Virtio

虚拟机监控程序（hypervisor）可以逐个寄存器地模拟一块真实网卡，但客户机的每一次寄存器访问都要从虚拟机退出到虚拟机监控程序，代价高昂，而真实网卡的驱动程序处理每个数据包都要进行很多次这样的访问。Virtio 是一系列半虚拟化设备：这些设备专为由虚拟机监控程序实现而设计，把寄存器访问降到最少，并在共享内存中完成工作。同一份规范定义了网卡、块设备、GPU、输入设备等多种设备，每种设备都有一个设备类型编号。

同一个设备可以通过不同的传输方式到达客户机。在 [PCI](pci.md) 上，它是一个厂商 ID 为 0x1AF4 的功能，由它的各项能力给出其 virtio 寄存器的位置；在 MMIO 上，它是位于平台所描述的固定地址处的一个寄存器块。在传输方式之上，协议是相同的。

让设备进入工作状态需要通过它的状态寄存器完成一次握手。驱动程序写入 0 来重置设备，设置 ACKNOWLEDGE 和 DRIVER，然后协商特性：设备提供一个可选特性的位图，驱动程序写回它能理解的子集并设置 FEATURES_OK，如果设备再次清除该位，就表示它拒绝这个子集。第 32 位 VIRTIO_F_VERSION_1 表示设备遵循 1.0 规范，而不是传统接口。随后驱动程序建立它的 virtqueue 并设置 DRIVER_OK，设备即开始工作。virtqueue 是一个由三部分组成的[描述符环](descriptor-rings.md)：一个描述符表，一个供驱动程序提供缓冲区的可用环（available ring），以及一个供设备归还缓冲区的已用环（used ring）。可用环增长时，驱动程序写入一个通知寄存器；已用环增长时，设备触发中断。

陷阱在于依据请求的特性而不是协商后的特性行事：不在协商结果中的特性处于关闭状态，而且有些特性会改变设备所交换数据的布局。

在驱动工具包中，`VirtioAccess` 负责执行这次握手。工具包在每次探测之前重置设备并设置 ACKNOWLEDGE 和 DRIVER；`NegotiateFeatures` 接受驱动程序能理解的低 32 位，在设备提供 VIRTIO_F_VERSION_1 时添加该位，并在 FEATURES_OK 未能保持置位时返回 `false`；`TryCreateQueue` 在绑定的 DMA 内存中构建一个 `Virtqueue`；`SetDriverOk` 要在第一次 `Notify` 之前调用。驱动程序在 `OnDetach` 中调用 `Reset()`，让设备停止向工具包即将释放的环中写入数据；参见 [virtio 访问对象](../drivers.md#the-virtio-access-object)和[队列](../drivers.md#queues)。
