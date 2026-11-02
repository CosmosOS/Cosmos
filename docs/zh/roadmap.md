---
source_hash: 2bad57cfc5819f387da608190853fc35b52a1fd8
---
<a id="gen2gen3-feature-comparison"></a>

## Gen2/Gen3 功能对比

| 功能 | Gen2 | Gen3 | 说明 |
|---------|------|-------------|-------|
| 底层汇编访问 | ✅ | ✅ | 以前使用 X#，现在使用 x64 NASM + ARM64 GAS 汇编。 |
| ACPI | ✅ | ✅ | 通过 C 互操作使用 [uACPI](https://github.com/uACPI/uACPI)：两种架构都用它读取 ACPI 表，x64 上用它的 AML 解释器关机和重启。  |
| 中断处理 | ✅  | ✅  | x64：APIC（Local + I/O）。ARM64：GIC。 |
| 内存管理 | ✅ | ✅ ||
| 驱动程序支持 | ✅ | ✅ | 基于驱动工具包的 PCI 和 MMIO；平台总线由 ACPI、设备树（ARM64）或机器的表填充；根端口上的 PCI Express 热插拔 |
| 垃圾回收 | ✅ | ✅ | 标记-清除 GC |
| 文件系统 | ✅ | ✅ | 基于 Unix 风格 VFS（挂载、超级块、inode）的 FAT12/16/32 和 ext2，支持格式化，以及只读的带 Rock Ridge 的 ISO 9660；MBR/GPT/EBR 分区。通过 `System.IO.File`/`FileStream` 对外提供。 |
| .NET 核心库功能 | 🟡 | 🟡 部分 | 核心类型可用（String、Collections、List、Dictionary）。Math、Console、DateTime、Random、BitOperations、`System.IO.File` 已通过 plug 实现。 |
| Plug 系统 | ✅ | ✅  |  |
| 测试框架 | ✅ | ✅  |  |
| 调试器| ✅ | 🟡 部分 | vscode 中存在 Source link 和变量相关的 bug |
| CPU/FPU 加速数学运算 | ✅ | ✅ | 仅限 x64：x87 FPU（sin/cos/tan/exp/log/atan） |
| Cosmos 图形子系统 | ✅ | ✅ | 通过 Limine 获得的固件帧缓冲区，以及基于驱动工具包的 virtio-gpu、VMware SVGA II、AMD DCN 3.1.5 和 Intel 集成显卡显示驱动程序（VMware 驱动程序上还有 SVGA3D）。 |
| 网络接口 | ✅ | ✅ | 基于驱动工具包的 Intel E1000E（它同样会绑定 8254x e1000）和 virtio-net。通过标准的 `System.Net.Sockets` 和 `System.Net.Dns` 提供 ARP、IPv4、IPv6 链路本地地址、UDP、TCP、DHCP 和 DNS；Telnet、FTP 和 HTTP（客户端和服务器）以包的形式提供。 |
| 定时器 / 时钟 | ✅ | ✅ | |
| 键盘输入 | ✅ | ✅ | PS/2（x64）、virtio 和 USB 键盘，均以驱动工具包驱动程序的形式实现 |
| 鼠标输入 | ✅ | ✅ | PS/2（x64）、virtio 和 USB 鼠标，均以驱动工具包驱动程序的形式实现 |
| 音频接口 | 🟡 | ✅ | Intel HD Audio：WAV 播放、生成的音频流和 `Console.Beep`。仅支持播放，每个输出一个流。 |

<a id="additional-gen3-features"></a>

## Gen3 新增功能

除了与 Gen2 持平之外，Gen3 还带来了新的能力：

| 功能 | 状态 | 说明 |
|---------|--------|-------|
| **NativeAOT 运行时** | 🟡 部分 | 带运行时的完整 NativeAOT 编译，不再使用 IL2CPU。 |
| **ARM64 支持** | ✅  |  |
| **Limine 启动协议** | ✅ |  |
| **线程与调度器** | ✅ | 基于优先级的 Stride 调度器（x64 + ARM64）。支持 `lock` 关键字。 |
| **功能开关** | ✅ |  |
| **Cosmos VS Code 扩展** | ✅ |  |
| **USB 支持** | ✅ | xHCI、EHCI 和 OHCI 主机控制器，支持集线器和热插拔（x64 + ARM64），均以驱动工具包驱动程序的形式实现，并配有 HID 启动键盘、HID 启动鼠标和大容量存储（Bulk-Only Transport）类驱动程序。不支持 UHCI（Intel 通过端口 I/O 访问的 USB 1.1 配套控制器）。 |
| **HTTPS** | ✅ | 由 BouncyCastle 以托管代码实现 TLS 1.3 和 1.2，通过 Cosmos.Network.Http 包提供（[HTTP 和 HTTPS](articles/user/network.md#http-and-https)）：`HttpClient` 和 `HttpListener` 支持 `https://`，并根据 Mozilla 的根证书校验证书。 |

<a id="future-releases"></a>

## 未来版本

首个版本之后计划实现的功能：

| 功能 | 状态 | 说明 |
|---------|--------|-------|
| **SMP（对称多处理）** | ❌ 未开始 | 多核 AP 启动、每 CPU 调度、负载均衡器。 |
| **分代 GC** | ❌ 未开始 | 用分代回收器（Gen0/Gen1/Gen2）替换当前的标记-清除，以获得更好的性能。 |
| **代码执行** | ❌ 未开始 | 用户态 WASM 虚拟机 |
| **从设备树获取 GIC、ITS 和时钟** | ❌ 未开始 | 目前 ARM64 中断控制器来自 ACPI 或 virt 默认值，因此以 acpi=off 启动时没有 MSI；平台总线已经会读取设备树。x64 从来没有设备树。 |
| **原生 PCI Express 插槽之外的 PCI 热插拔** | ❌ 未开始 | ACPI（GPE）热插拔：QEMU 在未设置 ICH9-LPC global 选项的 q35 根端口上以及传统 PCI 桥上使用这种方式；下游交换端口能被匹配，但未经测试。 |
