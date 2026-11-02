---
source_hash: 0d3aeff06b8d65a8186f8e3a74eabfe356e9ab23
---
<a id="public-api-tracking"></a>

# 公共 API 跟踪

内核各个包的公共 API 面（public API surface）声明在提交到仓库中的文本文件里，在构建时由 Roslyn 分析器强制执行，在发布时防范破坏性变更，并且每个版本都会发布一个冻结的文档站点。本页先说明决定哪些内容公开的策略，再介绍三种机制以及它们给贡献者带来的工作流程。

---

<a id="the-policy"></a>

## 策略

三条规则决定哪些内容是公开的：

1. **唯一的受支持层。** `Cosmos.Kernel.System` 是内核编程所面向的 API：被跟踪、有文档、在发布时冻结，弃用周期只在这里适用，也仅在这里适用。来自其他程序集的契约类型，只有当内核能够通过受支持层获得一个实例，或者有意义地提供一个实例时，才会随之成为公开类型。`Cosmos.Kernel.HAL` 中的 `IBlockDevice` 是唯一符合条件的设备契约，而且两方面都符合：`StorageManager.PrimaryDevice`、`Devices` 和 `GetDevice` 会把设备交给内核，DevKernel 直接通过它读写块，31 个公共成员以它作为参数，Fat 和 File 两个测试套件也各自实现了一个属于自己的块设备并驱动它，而且都没有对 `Cosmos.Kernel.HAL` 的授权（Fat 套件的授权针对的是受支持层，那是另一个问题）。`MacAddress` 随之公开，因为数据包扩展点和 `NetworkManager.MacAddress` 都会交出一个 MAC 地址。VFS 契约不在其列：受支持层之下没有任何代码实现或调用它们，因此它们直接声明在受支持层自身中，位于 `Cosmos.Kernel.System.FileSystem`，与 `VfsManager` 和各个句柄放在一起。这些其他程序集中的其余一切都是内部的，包括恰好被某个 `Register` 方法提到的契约：节拍源和网络设备只由本程序集自己的 `LibraryInitializer` 及其网络使用方注册，输入设备是驱动工具包中的 `IKeyboard` 和 `IPointer` 设备种类，而内核读取的是 `KeyboardManager.ReadKey`、`MouseManager.X` 和 `NetworkManager.LinkUp`，从不直接读取设备。平台契约（`IPortIO`、`ICpuOps`、`IPowerOps`、`IInterruptController`、`IRQContext`）和启动契约 `IPlatformInitializer` 同样不公开，因为 `PlatformHAL` 是内部的，没有内核能够注册或获得它们。

   要警惕这个检验的弱化版本，即“受支持层的某个签名提到了它”。它会放行任何曾有人为其写过 `Register` 重载的类型，却会拒绝一个没人这样做过的相同契约，`IKeyboardDevice` 和 `IGraphicDevice` 就是这样在同一个问题上落到了对立的两边。`IGraphicDevice` 后来在显示设备迁移到驱动工具包时被删除：显示设备如今是工具包中的一种设备种类，即实验性扩展点上的 `IDisplay`，内核读取 `DisplayManager` 并通过 `Canvas` 绘制。`IKeyboardDevice` 在 PS/2 键盘和鼠标迁移到工具包时也随之删除：键盘如今是工具包中的一种设备种类，即实验性扩展点上的 `IKeyboard`。

   `INetworkDevice` 说明了这个弱化检验能把一个类型带到多远。受支持层中曾有十三个成员接受它作为参数，这看起来很有说服力，直到你数一数内核拿这个句柄做了什么：它读取 `Name`、`MacAddress`、`LinkUp` 和 `Ready`，并检查它是否为 null。在 DevKernel 和网络测试套件中，从来没有任何代码对它调用过 `Enable`、`Disable`、`Initialize` 或 `Send`，没有任何代码请求过 `GetDevice(1)`，而 `RegisterDevice` 恰好只有一个调用者，即本程序集自己的 `LibraryInitializer`。`NetworkManager` 早已基于主设备对外提供 `LinkUp` 和 `Send`，所以门面（facade）已经建好了一半，而调用者却绕过了它。补上缺失的四项信息后，门面就完整了，这个契约也随之变为内部的，而 `KeyboardManager` 和 `MouseManager` 一直以来就是这么做的。

   隐藏一个契约，不能连带隐藏一项能力。`INetworkDevice` 曾是指称某个设备的唯一途径，因此受支持层新增了 `NetworkAdapter`，这是一个携带注册索引的句柄，同时新增了 `NetworkManager.GetAdapter`、可设置的 `NetworkManager.Primary`、接受适配器参数的 `IPConfig.Enable` 重载，以及用于获取某个适配器当前生效配置的 `NetworkAdapter.IPConfig`。句柄归受支持层所有，内核确实会获得它，而内核从未调用过的那些成员则留在它的背后。

   随后这个检验会逐个成员再做一遍，因为一个类型赢得了自己的位置，并不意味着它的整个类都随之公开。`SoftwareTimer` 是公开的：`TimerManager.Schedule` 返回它，`Cancel` 再把它收回。它背后的注册表条目则不是：`TimerManager` 负责构造它，而它的 `SetActive`、`Tick` 和 `Invoke` 属于拥有该注册表的节拍源；内核如果调用它们，就会推进另一个定时器的倒计时、在线程上下文中运行本应在中断上下文中运行的回调，或者在未注销的情况下停用一个条目。句柄所保留的公开成员 `TimeoutNs`、`Recurring` 和 `IsActive`，正是调用者可以读取而不会破坏其背后对象的内容。句柄和条目之所以是两个类型，是出于分层的原因而非策略的原因：`TickSource` 持有注册表并从中断路径推进它，而 HAL 不能引用受支持层，因此条目 `TimerEntry` 作为内部类型留在 `Cosmos.Kernel.HAL.Timers` 中，与 `TickSource` 放在一起，而 `SoftwareTimer` 是受支持层自己用来包装条目的句柄，位于 `Cosmos.Kernel.System.Timers`，与分发它的管理器放在一起。

   同一条规则还有一个推论：受支持层不能为自己并不提供的能力写文档。`TimerManager.Schedule` 和 `ScheduleRecurring` 都曾告诉内核，对需要线程上下文的回调要使用 `AlarmSystem`，而 `AlarmSystem` 是 `Cosmos.Kernel.Core` 的内部类型。于是受支持层交出的是中断上下文的那一半（在那里回调不能阻塞、分配内存或获取锁），却隔着一堵墙指向安全的那一半。`AlarmManager` 就是受支持层通往它的途径，它把调用转发给内部实现；它之所以是一个单独的管理器，而不是在 `TimerManager` 上再加三个方法，是因为调用错了就会死锁，而正是管理器的名字让上下文在调用点上一目了然。既然内核要根据执行上下文在两个管理器之间做选择，这两个管理器就不应该在形式上也有差异，所以两者的 `Schedule` 和 `ScheduleRecurring` 都接受 `(Action callback, TimeSpan delay)`，两者的 `Cancel` 也都只在回调仍处于待处理状态时才返回 `true`。形式统一之后，那唯一无法统一的一点就值得在受支持层上明确写出来：闹钟自身的调度调用会在互斥锁上等待，因此只能在线程上下文中发起，而定时器的调度调用会屏蔽中断，可以在任何地方发起。仍然不同的是两者各自返回的东西：`AlarmSystem` 拥有自己的闹钟，并且是 `Cosmos.Kernel.Core` 的内部类型，因此受支持层只能为闹钟返回一个 `ulong` ID，而另一方返回的是一个 `SoftwareTimer`，它包装的是节拍源注册表中所持有的条目。

   同一轮复查还删除了 `NetworkConfigManager`。它看上去是 `IPConfig` 的管理器，但只掌握了其中一半：配置一个设备时，会把同一份配置写入 `IPConfig` 的路由列表、管理器中按设备索引的列表以及它的当前指针，三处存储一起写入、却分开清除，而那两个本会让它们失去同步的 `Remove` 方法都是死代码。按设备查找的功能移到了 `NetworkAdapter.IPConfig` 上，受支持层已经在那里保存其他所有按设备区分的信息，路由列表则成为唯一的存储。`DnsConfig` 保留自己的列表，而不是再配一个对应的管理器，因为域名服务器列表对解析器而言是全局的，而 IPv4 配置只属于某一个设备。
   只能给出一个固定值的成员不算是成员。`Address.IsIpv4` 读取的是 `Parts.Length == 4`，而该类型的每个构造函数都恰好生成四个字节，所以它恒为 `true`，`IsIpv6` 恒为 `false`，`CompareTo` 中“不是同一地址族”的异常检查实际上是拿这个常量和它自己比较。`MacAddress.IsValid` 也因为同样的原因落得同样的下场。对这类成员要删除而不是重命名：重命名会保留它在受支持层中的位置，却什么也证明不了；同一轮复查删除了 `Address.IsLoopbackAddress`、`IsAPIPA` 和 `CIDRToAddress`，因为没有任何代码调用它们，而 `IsBroadcastAddress`、`ToUInt32` 和 `ToSpan` 则因为有代码调用而改为内部成员。受支持层也不能为一个已经有两种回答方式的问题再提供第三种写法：`NetworkManager.HasDevice` 不过是换了个名字的 `DeviceCount > 0` 和 `Primary.IsValid`。

   缩写词的大小写遵循 .NET 规则，这一点值得写明，因为它只朝一个方向：两个字母的缩写词保持全大写（`IPPacket`、`SourceIP`），三个字母及以上的缩写词使用 Pascal 大小写（`IcmpType`、`UdpDataLength`、`MacAddress`）。所以 `NetworkManager.MacAddress` 本来就是对的，而 `EthernetPacket.DestinationMAC` 和地址类型本身（当时叫 `MACAddress`）则不对。这次清理中有一个重命名无法编译：`TcpPacket.TCPFlags` 不能改成 `TcpFlags`，因为枚举 `TcpFlags` 位于同一个命名空间中，而类体中会计算 `(byte)TcpFlags.SYN`，一个同名的 `byte` 类型属性会遮蔽这个枚举（CS1061）。所以它叫 `FlagBits`。与协议头对应的线路格式字段名保留原有写法，这就是 `TcpFlags.ACK` 和 `DnsPacket.AnswerRRs` 保持不变的原因。命名空间和文件夹名同样遵循这条规则：`Cosmos.Kernel.System.Network.Protocols` 下的协议命名空间是 `Arp`、`Dns`、`Tcp`、`Udp` 和 `IPv4.Dhcp`，而 `IPv4` 和 `IPv6` 保留两个大写字母。

2. **精心选定的实验性扩展点。** 扩展点是有意开放的，并标记为 `[Experimental]`：现在就公开可用，但不作兼容性承诺，经过验证后再通过移除该特性提升为稳定。引用这类扩展点会导致构建错误，直到使用方项目抑制其诊断 ID 为止，这就表示使用方认可了这一契约。Core 中的调度策略扩展点是第一个，System 中的数据包扩展点是第二个，HAL 中的驱动工具包是第三个。
3. **其余一切都是内部的。** 可见性从来不是扩展机制，扩展点才是。第一方程序集和白盒测试内核通过 `InternalsVisibleTo` 访问内部成员；其他任何人都要通过 `[UnsafeAccessor]`/`[UnsafeAccessorType]`（[访问内部成员](accessing-internals.md)），并接受内部成员会在不另行通知的情况下变更。

强制检验是机械式的：`examples/DevKernel` 必须在没有任何 `InternalsVisibleTo` 授权的情况下编译通过。如果 DevKernel 需要某个符号，这个符号就要变为公开，或者在 `Cosmos.Kernel.System` 中获得一个门面；如果不需要，这个符号就保持内部。

受支持层中有一个公开类型不是给内核用的。`Cosmos.Kernel.System.Internal.KernelEntry` 为构建生成的入口点注册并启动内核，而这段生成的代码会编译进内核自己的程序集，因此它只能调用公开成员，就像生成的驱动程序清单调用公开的 `DriverRegistry.Register` 一样。该类型和受支持层的其他部分一样被跟踪，并标记为 `[EditorBrowsable(EditorBrowsableState.Never)]`；在受支持层中，`Cosmos.Kernel.System.Internal` 命名空间只包含这一个类型：生成的 `CosmosEntryPoint` 和 `DriverManifest` 已经在每个内核的程序集中声明了这个命名空间。内核代码通过 `Kernel.Current` 访问正在运行的内核。这两者取代了一个位于根级别的 `Global` 类，它曾把底层衔接代码和正在运行的内核放在一起，位于受支持层的顶层、`Kernel` 旁边。

| 程序集 | API 面 |
|----------|---------|
| `Cosmos.Kernel.System` | 受支持层 |
| `Cosmos.Kernel.HAL` | 驱动工具包扩展点（`[Experimental]`）、`IBlockDevice` 和 `MacAddress`，被跟踪；启动契约、固件时钟、节拍源基类及其定时器条目、端口均为内部 |
| `Cosmos.Kernel.Drivers` | 二十五个随附的驱动程序类，包括它们的无参构造函数和对 `Driver` 的重写，此外别无其他：每个内核中生成的清单会构造它们，而一个包无法把内部成员授权给它并不知道的内核。其余一切（状态对象、传输层、SVGA 适配器的能力接口（facet））都是内部的，由测试套件通过 `InternalsVisibleTo` 读回；被跟踪，但不纳入 API 参考；是一个受 User 层约束的驱动程序集 |
| `Cosmos.Kernel.Core` | 调度器扩展点（`[Experimental]`），此外别无其他，被跟踪 |
| 架构程序集、Native、Plugs、Debug、Boot.Limine、`Cosmos.Kernel` | 内部，通过 `InternalsVisibleTo` 向第一方程序集开放 |

最后一行属于策略，而不是被跟踪的强制约束：这些程序集不被跟踪，其中剩余的公开类型会在它们被改动时逐步减少。

实验性扩展点带有诊断 ID：

| ID | 扩展点 |
|----|------|
| `COSMOS0001` | 调度策略扩展点：`IScheduler`、`SchedulerManager`、`SchedulerThread`、`PerCpuState`、`SchedulerExtensible`、`InterruptMaskScope`、`SchedulerThreadState`、`SchedulerThreadFlags`（[调度器 - 编写调度器](../user/scheduler-plugging.md)） |
| `COSMOS0002` | 数据包扩展点：`Cosmos.Kernel.System.Network.Protocols` 子树中的所有公开类型（其根部的 `EthernetPacket` 和 `InternetPacket`，以及 `Arp`、`Dns`、`IPv4`、`IPv4.Dhcp`、`Tcp` 和 `Udp` 数据包类型），外加 `Cosmos.Kernel.System.Network` 中三个稳定类型里发送、注入或返回数据包的成员：`NetworkStack.Send`/`HandlePacket`、`UdpClient.Send`/`ReceivePacket` 和 `IcmpClient.Send`/`ReceivePacket`（[网络 - 构造数据包](../user/network.md#crafting-packets)） |
| `COSMOS0003` | 驱动工具包扩展点：`Cosmos.Kernel.HAL.DriverKit` 中的所有类型（`Driver`、`DriverAttribute`、`DeviceBinding`、`DeviceNode`、`DriverRegistry`，以及探测和分离相关类型）、`Cosmos.Kernel.HAL.DriverKit.Resources`（`DeviceResource`、`RegisterWindow`、`DeviceRegion`、`DmaBuffer` 及其配套类型）、`Cosmos.Kernel.HAL.DriverKit.Interrupts`（`InterruptSource`、`InterruptHandle`、`InterruptHandler`、`InterruptContext`、`InterruptTrampoline`）、`Cosmos.Kernel.HAL.DriverKit.Threading`（`DeviceLock`、`DeviceLockScope`、`DeviceEvent`、`DriverThread`、`WorkItem`）、`Cosmos.Kernel.HAL.Devices.Display`（显示设备种类：`IDisplay`、`DisplayMode`、`DisplaySink`，以及显示设备的能力接口 `IDisplayModes` 和 `IHardwareCursor`）、`Cosmos.Kernel.HAL.Devices.Input`（键盘和指针设备种类：`IKeyboard`、`KeyboardLeds`、`KeyboardSink`、`IPointer`、`PointerButtons`、`PointerSink`）、`Cosmos.Kernel.HAL.Devices.Network`（网络设备种类：`INetworkInterface`、`NetworkSink`，但不包括稳定的 `MacAddress`）、`Cosmos.Kernel.HAL.Devices.Storage`（块设备侧面 `IRemovableMedia`，但不包括稳定的 `IBlockDevice`）、`Cosmos.Kernel.HAL.DriverKit.Buses.Synthetic`（测试总线）、`Cosmos.Kernel.HAL.DriverKit.Buses.Platform`（平台总线种类：`PlatformIdentity`、`PlatformMatch`）、`Cosmos.Kernel.HAL.DriverKit.Buses.Ps2`（PS/2 总线种类：`Ps2Port`、`Ps2Identity`、`Ps2Match`、`Ps2Access`、`Ps2Controller`）、`Cosmos.Kernel.HAL.DriverKit.Buses.Pci`（PCI 总线种类：`PciIdentity`、`PciMatch`、`PciAccess`、`PciBar`、`PciHostAccess`、`PciFunctionDescription`）、`Cosmos.Kernel.HAL.DriverKit.Buses.Virtio`（virtio 总线种类：`VirtioIdentity`、`VirtioMatch`、`VirtioDeviceType`、`VirtioAccess`、`VirtioTransport`、`Virtqueue`、`VirtqueueLayout`、`VirtqueueDescriptorFlags`、`VirtioInterruptStatus`）和 `Cosmos.Kernel.HAL.DriverKit.Buses.Usb`（USB 总线种类：`UsbIdentity`、`UsbMatch`、`UsbAccess`、`UsbBus`、`UsbHostController`、`UsbDevice`、`UsbPipe`、`UsbEndpoint`、`UsbSetupPacket`、`UsbReportHandler`、`UsbHubProtocol`、`UsbClassCode`，以及枚举 `UsbSpeed`、`UsbEndpointType`、`UsbRequestType`、`UsbStandardRequest`、`UsbDescriptorType` 和 `UsbTransferStatus`）；此外，在 HAL 之外以同一 ID 标记的还有 `Cosmos.Kernel.System.Graphics.Rendering3D` 中的 `ICanvas3DFactory`，这是由受支持层定义、显示设备在进行 3D 渲染时实现的能力接口（[编写驱动程序](../user/drivers.md)） |

驱动工具包扩展点有两道自己的防护，因为驱动程序是该策略预期会出现在代码树之外的唯一一类使用方。声明了 `<CosmosDriverAssembly>true</CosmosDriverAssembly>` 的项目只能使用第三方所能获得的 API 面：无论名字是什么，分层分析器都把它当作 User 程序集，并根据其代码中提到的类型和成员来判断（只能来自 `Cosmos.Kernel.System` 和 `Cosmos.Kernel.HAL`，不能更底层），`NAOT0008` 禁止在其中使用 `[UnsafeAccessor]` 和 `[UnsafeAccessorType]`，`NAOT0009` 禁止任何被引用的 `Cosmos.*` 程序集向它授权内部成员。最后这条规则在整个代码树范围内的那一半，是 `Directory.Build.targets` 中的 `CosmosForbidDriverAssemblyGrants` 目标：任何项目，只要其 `InternalsVisibleTo` 提到了 `CosmosDriverAssemblyNames`（`Directory.Build.props`）中列出的程序集，就会构建失败，而代码树中的每个驱动程序集都会加入这个列表。目前它列出的是 `Cosmos.Kernel.Drivers`，即随附驱动程序所在的包（PCI 主机和 PCI Express 根端口驱动，Intel E1000E 驱动，virtio PCI 和 MMIO 传输驱动，virtio-net 和 virtio-input 驱动，virtio-gpu、VMware SVGA II、AMD DCN 3.1.5 和 Intel 集成显卡显示驱动，AHCI、NVMe 和 virtio-blk 存储驱动，HD Audio 驱动，xHCI、EHCI 和 OHCI 主机控制器驱动、USB 集线器驱动、HID 启动协议键盘驱动、HID 启动协议鼠标驱动和 USB 大容量存储驱动，以及 8042、PS/2 键盘和 PS/2 鼠标驱动，共二十五个，每个内核都通过 `Cosmos.Kernel` 获得它们；virtio-blk 驱动从 Drivers 测试套件的库中迁移过来，除了命名空间之外什么都没有改变），以及 Drivers 测试套件的库 `Cosmos.Kernel.Tests.Drivers.Library`；两者都在该属性下构建，所以编译器本身就证明了它们各自只调用内核作者能够调用的内容。`Cosmos.Kernel.Drivers` 在 `PublicAPI.Shipped.txt` 和 `PublicAPI.Unshipped.txt` 中跟踪自己的 API 面，不带 `[COSMOS0003]` 前缀，因为它的驱动程序虽然派生自实验性工具包，自身却不是实验性的。这个 API 面只包含生成的清单所构造的驱动程序类；该程序集中没有任何内容是供内核调用的，因此其余部分都是内部的，只授权给需要读回驱动程序状态的测试套件，并且该程序集不纳入 API 参考。另一道防护是 HAL 项目中的 `CosmosGuardHalPromotion` 目标，它在 HAL 构建之后运行，读取 `PublicAPI.Shipped.txt` 和 `PublicAPI.Unshipped.txt`，只要某个符号行既没有分析器为 `[Experimental]` 符号写入的 `[COSMOS0003]` 前缀，也没有 `*REMOVED*`，就会失败，除非该行属于其 `_CosmosStableHalTypes` 列表中列出的稳定设备契约之一，即 `IBlockDevice` 和 `MacAddress`。稳定性是按类型而不是按文件夹标记的：一个扩展点类型如果丢失了它的特性（意外提升正是这样发生的），无论位于何处都会导致构建错误，而真正提升一个类型，则需要有意地修改那个列表。

---

<a id="behaviour-when-a-feature-is-compiled-out"></a>

## 功能被编译排除时的行为

每个功能开关（`CosmosEnableStorage`、`CosmosEnableMouse` 等）都可能让受支持层中的某个管理器背后没有任何子系统。这时它的成员如何表现，由一条规则决定：

| 成员种类 | 功能关闭时的行为 |
|----------------|--------------------------------|
| 能够表达“这里什么都没有”的读取 | 返回如实的答案：`0`、`null`、`false`、空列表、无效句柄 |
| `Try` 方法，或任何其 `bool` 返回值本就表示“没有发生”的成员 | 返回 `false` |
| 其他一切 | 抛出 `InvalidOperationException`，并指明需要设置的开关 |

读取类成员会给出答案，以便内核据此分支，而 `KernelFeatures` 直接回答编译时的问题。操作类成员之所以抛出异常，是因为另一种做法是静默地什么都不做，那看起来会像是内核里的 bug，而不是 `.csproj` 中有个开关没有打开。中间一行依据的是形式，而不是 `Try` 前缀：`TimerManager.Cancel` 和 `AlarmManager.Cancel` 对于并不处于待处理状态的回调本来就返回 `false`，子系统被编译排除只是“不处于待处理状态”的又一种情形，而清理路径不应该在运行之前还得先检查开关。

有两个成员无法遵循读取那一半的规则，并在各自的 XML 文档中说明了这一点：`KeyboardManager.Peek` 和 `ReadKey` 返回不可为 null 的 `KeyEvent`，因此没有表示“没有按键”的值，而且如果按这条规则处理，`ReadKey` 会阻塞在一个永远不会有键盘触发的中断上。

---

<a id="how-a-member-reports-failure"></a>

## 成员如何报告失败

关于编译排除的那张表只是一条更宽泛规则的一个特例。受支持层的成员只有一种渠道来表明自己没有成功，而具体是哪种渠道，取决于成员的形式。

| 成员是 | 它通过以下方式表明 |
|---------------|-----------------|
| 本身就有空值的读取 | 该值：`0`、`null`、`false`、空列表、无效句柄 |
| 因调用者可以处理的原因而失败的操作 | 一个 `bool`，如果同时带有 `out` 参数，则以 `Try` 命名 |
| 调用者必须区分其各种失败情况的操作 | 一个结果枚举，例如 `SchedulerDiagnostics.RequestKill` 返回 `ThreadKillResult` |
| 在正确的内核不可能产生的状态下被调用的任何成员 | 一个异常，用 `<exception>` 写入文档，并且绝不是裸的 `Exception` |

有四条规则让这张表真正可用。

**`Try` 成员不会为它自己的 bool 所表达的失败抛出异常。** `IVfsFileHandle` 正是按这一点划分的：对于已被释放的句柄，`TrySeek`、`TryFlush`、`TrySetAttr` 和 `TryStat` 返回 `false`，而 `Read` 和 `Write` 会抛出异常，因为字节数没有哪个值能表示“句柄已经不存在”，而不是“没有传输任何字节”。如果一个 `Try` 成员代替调用者执行了 `Dictionary` 查找、索引器访问或 `Dequeue`，那么原本会经由它抛出异常的参数检查和空检查就由它自己负责：`VfsManager.TryMount` 像 `RegisterFileSystem` 那样对驱动程序名称进行防护，`KeyboardManager.TryReadKey` 则用 `TryDequeue` 取出按键，这是一次调用，其自身的 bool 就是答案，而不是先测试 `Count`，再调用一个在测试出错时会抛出异常的 `Dequeue`。

**决定因素是形式，而不是前缀。** 编译排除那张表的中间一行已经说明了这一点，这里同样适用。`IInodeOperations` 用普通动词来命名 VFS 的驱动程序一侧：`Lookup`、`Create`、`Mkdir`、`Rename`；旁边的 `IVfsDirectoryHandle` 则把使用方一侧命名为 `TryLookup`、`TryCreateFile`、`TryCreateDirectory`、`TryRename`。就本页的每一条规则而言，两者都是 `Try` 成员。这种命名上的区分值得保留，因为它标明了读者所处的是 VFS 的哪一侧，但它换不来任何豁免。

**`Try` 成员中每个可为 null 的引用类型 `out` 参数都要带 `[NotNullWhen(true)]`，并且只在每条返回 true 的路径都会为其赋值时才这样做。** 没有这个特性，每个调用点都要付出一个 `!` 或一个多余的 `x != null`，而这种冗余是看不出来的：编译器无法区分一个子句是在防护真正的漏洞，还是什么都没有防护。该特性会在声明它的方法的各个返回点上检查，但流分析不是跨过程的，所以一个转发另一个方法的 `out` 参数的方法，需要那个方法也加上标注：在 `FatSuperblock.AllocateDirectoryEntry` 加上标注之前，`FatInodeOperations.Create` 无法证明自己的后置条件。绝不要把这个特性放在输入参数上。这样做是合法的，Roslyn 也会在那里遵循它，`string.IsNullOrEmpty` 就是这样用的，于是它不再表示“out 参数已被赋值”，而是开始断言调用者传入的参数不为 null。`IVfsFileSystemType.TryFormat` 当初恰恰就是这样交付的，而在它上方四行的文档里写着 `null` 表示选用驱动程序的默认值。

**与真实值冲突的哨兵值，需要第二个成员来把两者区分开。** `SchedulerDiagnostics.GetRunQueueCount` 对空队列和超出范围的 CPU ID 都返回 `0`，这是如实的，因为 `CpuCount` 说明了取值范围；在它旁边再加一个 `TryGetRunQueueCount`，就会成为受支持层已经回答过的问题的第三种写法。`MacAddress.None` 和为 null 的 `MacAddress` 是同一个问题的两个哨兵值，因此可能返回其中任一个的成员都会说明返回的是哪一个，并指向 `NetworkAdapter.Ready`。

---

<a id="property-or-method"></a>

## 属性还是方法

受支持层持有的值是属性，操作是方法。API 面已经以很大的优势按这种方式划分：268 个属性，其中 223 个只读，34 个带 setter，11 个带 `init`，相比之下只有三个 `Set` 方法，以及两个返回存储值的 `Get` 方法。之所以要把这条规则写下来，主要是为了那些看起来似乎必须采用方法形式、实际上却不必的情况。

工作量不会迫使它成为方法。`KernelConsole.Font` 的 setter 会屏蔽中断、获取控制台锁、重新分配字符单元网格、把光标移回原点并重绘画布，而它是一个属性。访问设备也不会：`Canvas3D.Camera` 的 setter 会调用一个虚钩子，VMware 后端重写了这个钩子，以使由它派生的设备状态失效。验证也不会：`NetworkManager.Primary` 对不指向任何设备的句柄抛出 `ArgumentException`，`TimerManager.Frequency` 对节拍源无法分频得到的频率抛出 `ArgumentOutOfRangeException`，而两者都是属性。功能开关防护也不会，这就是为什么 `MouseManager.Sensitivity` 出于与 `TimerManager.Wait` 抛出异常相同的原因从 setter 中抛出异常，却仍然是一个属性。

在写入一侧，有三个成员值得采用方法形式，每个都是因为某些东西无法用一次赋值来表达。

| 成员 | 为什么不是属性 |
|--------|--------------------------|
| `MouseManager.SetPosition(x, y)` | 两个坐标必须一起生效，然后相互钳制 |
| `MouseManager.SetScreenSize(width, height)` | 同上，并且会把指针重新钳制到新的边界内 |
| `KeyboardManager.SetLayout(layout)` | 两半无法共用同一个类型。读取的类型如实地是 `KeyboardLayout?`：在键盘注册之前为 null，功能被编译排除时则永远为 null；而为 null 的键盘布局会让中断路径无从解码。两种形式都会在运行时拒绝 null；只有不可为 null 的参数还能在编译时诊断出来 |

只读属性并不要求配一个 `Set` 孪生方法，真正需要留意的恰恰是两者成对出现的形式。`TimerManager` 曾持有 API 面上唯一的这样一对：用一个属性读取节拍，用一个方法修改它，这看起来像是疏忽而非决定，而且掩盖了写入从未被检查这一事实。设备会拒绝它无法分频得到的频率，而它的 `SetFrequency` 返回 `void`，所以 `SetFrequency(5)` 什么也没做却正常返回了。现在它是一个属性，并且由设备说明是否接受了这个值。`KeyboardManager` 是仅剩的一对，它在两半上都是方法，而不是一半属性一半方法：既然写入无法变成 setter，读取就保持为 `GetLayout()`，让两种写法相互对应。

读取一侧也采用同样的检验。属性返回的是类型已经持有的值。方法则用于以下几种读取：需要计算的（`TcpPacket.GetFlags` 会构建一个字符串，`TrueTypeFont.GetMaxAdvance` 会测量每个可打印字符），每次调用都会分配内存的（`IcmpPacket.GetIcmpData` 和 `UdpPacket.GetUdpData` 都会把负载从帧中复制出来），或者接受一个参数来选择取哪个值的（`NetworkManager.GetAdapter(index)`、`SchedulerDiagnostics.GetRunQueueCount(cpuId)`、`TrueTypeFont.GetAscent(sizePx)`）。

这两条检验横跨了所有返回数组的读取，这一点值得一提，因为“它返回一个数组”并不是检验标准。`Font.Data`、`Image.RawData`、`EthernetPacket.RawData`、`TcpOption.Data`、`DhcpOption.Data` 和 `DnsAnswer.Address` 交出的是类型所存储的数组，它们是属性。`IcmpPacket.GetIcmpData` 和 `UdpPacket.GetUdpData` 每次调用都会构建一份新副本，它们是方法；`UdpData` 在被重命名之前就是一个这样做的属性，所以在循环中写 `packet.UdpData[i]` 会在每次迭代时复制整个负载。`Canvas.GetBuffer` 是唯一一个针对类型所存储的值、却采用方法形式的数组读取，原因是对 `Canvas.Mode` 赋值会替换掉这个数组，而调用者手里可能还持有旧数组，因此调用形式表明在模式改变后需要重新获取。`.editorconfig` 把 CA1819 设为 `warning`，按它的要求，这些属性都得改成方法；本规则的范围更窄，这六个属性保持不变。

`out` 参数不是第三种选择。`GetGcStats` 位于如今名为 `MemoryDiagnostics` 的内存门面上，它曾以这种方式返回两个垃圾回收器计数器，与五个属性并列，而把它们拆开并没有任何代价：它们是垃圾回收器在不同时刻写入的两个字段，中间没有锁保护，所以这个带 out 参数的方法本来也没能给调用者一对一致的值。`TrueTypeFont.GetLineMetrics(sizePx, out ascent, out descent, out lineGap)` 保留了这种成组返回的形式，因为这三个值来自对字体垂直度量信息的一次缩放读取，否则想要其中两个值的调用者就要付出两次代价。API 面上其余所有的 `out` 都属于 `Try` 成员，而它们已经由上面的失败表格涵盖。

关于重载集有两条规则，都是为了避免受支持层让最显而易见的写法变成错误的写法。

**用来切换成员产出内容的 `bool` 参数是第二种写法，而不是一个选项。** `Log.WriteNumber(x, hex: true)` 打印的内容与 `Log.WriteHex(x)` 完全相同，没有任何调用点传过这个参数，于是这个参数被删除了。`WriteHex` 和 `WriteHexWithPrefix` 是受支持层对十六进制输出的唯一答案，两者只按唯一一个真正有意义的区别来划分。

**`params` 重载接受 `ReadOnlySpan`，而不是数组。** `Log` 承诺对字符串和数字的写入不分配内存，而 `Log.Write("text")` 曾绑定到 `Write(params object?[])`，后者会为它唯一的参数分配一个数组；plug 层在每次获取和释放 `Monitor` 时都要付出这个代价，也就是 BCL 代码中的每一个 `lock`。`params ReadOnlySpan<object?>` 在每个调用点上都是同样的调用，而编译器会在栈上构建参数列表，因此无论参数有多少个，分配都消失了，而不只是消除某个带类型的孪生重载所能覆盖的那一种参数个数。剩下的是值类型的装箱，解决办法是在调用点使用带类型的重载：用 `Log.WriteNumber(n)`，而不是 `Log.Write(n)`。

---

<a id="naming"></a>

## 命名

真正起作用的是两条规则；本节其余部分记录了它们做出了哪些决定，以及有意保持不变的内容。

**受支持层或扩展点中的类型必须无需 using 别名就能直接引用。** 整个仓库都启用了 `ImplicitUsings`，因此无论作者是否需要，`System`、`System.Collections.Generic`、`System.IO`、`System.Linq`、`System.Net.Http`、`System.Threading` 和 `System.Threading.Tasks` 在每个内核文件中都处于作用域内。一个公开的 Cosmos 类型，如果其简单名称与这些命名空间中的某个类型相同，那就不是命名偏好的问题，而是读者文件中的编译错误。调度器扩展点曾带有两个这样的类型：`Thread` 和 `ThreadState`。[附加状态](../user/scheduler-plugging.md#attaching-state)一节中的示例只有三行钩子签名，而针对受支持层编译时，它产生了三个 `CS0104: 'Thread' is an ambiguous reference between 'Cosmos.Kernel.Core.Scheduler.Thread' and 'System.Threading.Thread'`。代码树早已用行动投了票：十一个第一方文件里有十二行别名，其中八处写的是 `using SchedThread =`，两处是 `using SchedulerThread =`，一处是 `using SchedThreadState =`；而[调度器编写指南](../user/scheduler-plugging.md#round-robin)称之为“与用户策略的写法完全一样”的树内示例策略，也需要在第三行写上 `using Thread =` 才能编译。现在它们叫 `SchedulerThread` 和 `SchedulerThreadState`，`ThreadFlags` 也随之改名，使这一族类型保持一致，而这十二个别名全部被删除，而不是改写。

显而易见的修复方式所选的名字是错的。`KernelThread` 不会与编译器能看到的任何东西冲突，而问题恰恰就在这里：受支持层已经在 `Cosmos.Kernel.System.Diagnostics` 中提供了 `KernelThreadInfo` 和 `KernelThreadState`，作为该扩展点类型的投影，于是 `KernelThread.State` 会返回 `Core.Scheduler.ThreadState`，而 `KernelThreadInfo.State` 返回的是 `Diagnostics.KernelThreadState`，把两者对应起来的读者就会出错，却没有任何东西提醒他们。悄无声息的混淆比它所取代的那种会直接报错的混淆更糟。`SchedulerThread` 与其命名空间一致，与九个文件自发做出的选择一致，并且让 `KernelThread*` 这一对毫无歧义地属于受支持层那一半。

扩展点上另外三个发生冲突的简单名称是 `Monitor`、`Mutex` 和 `SpinLock`。三者都是内部的，因此代价只是第一方代码中的一个别名，对受支持层没有影响。

**成员不重复其声明类型的名称。** `Image.Depth` 是图形命名空间中同一概念的第四种写法，另外三种是 `Mode.ColorDepth`、`Canvas` 的 `colorDepth` 参数，以及 `Image` 自身的构造函数参数 `color`；在这个命名空间里，`color` 原本指的是 `System.Drawing.Color`，它却被用来表示深度。现在两者都叫 `ColorDepth`。有三个成员保留了重复，因为这种重复有其存在的价值：

| 成员 | 为什么保留重复 |
|--------|--------------------------|
| `Canvas3D.DrawLine3D` | `Canvas3D` 继承了 `Canvas.DrawLine(Color, int, int, int, int)`，因此两者都可以在同一个对象上调用。后缀说明了调用所处的坐标空间；去掉它，就会剩下两个同名的画线操作，只能靠参数类型来区分 |
| `ColorDepth.ColorDepth4` .. `ColorDepth32` | 这种重复只在限定名中出现，而在那里它不会产生歧义。`Bpp4`..`Bpp32` 会丢掉“depth”这个词，违反 .NET 准则使用缩写，还会在读者脑中与六个文件中的 `bytesPerPixel` 和三个文件中的 `BitsPerPixel` 混淆。Gen2 也是这样命名的 |
| `PCScreenFont.DefaultFont` | `PCScreenFont.Default` 已被嵌套的常量容器类型占用，而且这个词是一种契约，而不只是一种写法：`Sdk.props` 会把 `Cosmos.Kernel.System.Graphics.Fonts.DefaultFont` 作为运行时宿主配置选项输出，其值取自 `CosmosDefaultFont` 属性，而嵌入资源的名字是 `DefaultFont.psf` |

**一个值只用一个词。** 存储部分用的是 `Sector`：`Gpt`、`Mbr`、`Partition` 和 `PartitionManager` 中都是 `StartSector`、`SectorCount`、`newStartSector`。`Ebr` 用的是 `Lba`，而第 07 节更是把两者揉进了同一个签名 `TryAddLogical(device, extendedStartLba, extendedSectorCount, systemId, sectorCount, out startSector)`，其自身为该 out 参数写的 `<param>` 把它描述为“Absolute LBA”。现在这两个参数是 `extendedStartSector` 和 `newStartSector`。FAT 和 GPT 在磁盘格式本身就这样命名某个字段的地方保留 `Lba`（`FatStartLba`、`PrimaryHeaderLba`）：规则是一个值一个词，而不是一个程序集一个词。

VFS 的 stat 结构体按照同一规则把 C 语言的缩写写成完整单词。`VfsStatFs.MaxNameLength` 沿用了 `IVfsSuperblock.MaxNameLength` 已经为同一个值使用的词，`VfsStat.InodeNumber` 沿用了 `Ext2Inode` 使用的词，时间字段则是 `AccessTime`、`ModificationTime` 和 `ChangeTime`，与 `System.Formats.Tar` 的命名一致。两个看起来相似的值保留两个词：`VfsStat.PreferredBlockSize` 对应 `st_blksize`，即首选的 I/O 大小（在 FAT 上是一个扇区），而 `VfsStatFs.BlockSize` 对应 `f_bsize`，即 `Blocks` 和 `FreeBlocks` 计数所用的单位（在 FAT 上是一个簇）。`VfsStat` 没有 `BlockSize`，因为在那里 `Blocks * BlockSize` 看起来会是一个字节数，而在 ext2 上它并不是，ext2 的 `st_blocks` 以 512 字节为单位计数。每个字段的文档都保留了它的 POSIX 名称。

**成员不使用 BCL 中另一个类型的名字。** `IPConfig.IPAddress` 返回的是 `Cosmos.Kernel.System.Network.IPv4.Address`，而它所在的文件中 `System.Net.IPAddress` 处于作用域内并且被使用，[网络](../user/network.md)一文中这两种含义相隔仅十五行。现在它叫 `IPConfig.Address`，与 `EndPoint.Address` 一致，并与 `SubnetMask` 和 `DefaultGateway` 一起构成一个三元组。

**用 `Initialize`，不用 `Init`。** 四个程序集中有 63 个成员使用了完整拼写，而被跟踪的 API 面上只有一个成员用了缩写。键盘布局基类的 `InitKeys` 是一个受保护的抽象成员，有七个重写，任何编写自己键盘布局的内核都要实现它，因此它现在叫 `KeyboardLayout.InitializeKeys`；这个缩写还保留在 `Cosmos.Kernel.Core` 中六个不受受支持层约束的内部辅助方法上。抽象成员无法借助 `[Obsolete]` 过渡，这使得在 `Shipped` 仍为空时重命名没有任何代价，之后则再也无法补救。

**被跟踪的类型要声明其所在程序集自己拥有的命名空间。** `MacAddress`（当时叫 `MACAddress`）曾在 `Cosmos.Kernel.HAL.Interfaces` 中声明 `Cosmos.Kernel.HAL.Devices.Network` 命名空间，而该命名空间属于 `Cosmos.Kernel.HAL`。编译没有出任何问题，因为命名空间不是引用边界，但发布出来的 API 树中，却为随另一个包分发的类型显示了一个 `Cosmos.Kernel.HAL` 分支。此后这些设备契约已迁入 `Cosmos.Kernel.HAL` 本身，每个契约都在那里声明其所在文件夹对应的命名空间，与同一设备类别的其他类型放在一起：`IBlockDevice` 位于 `Cosmos.Kernel.HAL.Devices.Storage`，`MacAddress` 位于 `Cosmos.Kernel.HAL.Devices.Network`。`SoftwareTimer` 曾位于 `Cosmos.Kernel.HAL.Platform`，与驱动它的节拍源放在一起，是一个原本全为内部类型的命名空间中唯一的公开类型；现在它是受支持层自己的句柄，位于 `Cosmos.Kernel.System.Timers`，包装着一个仍作为内部类型留在节拍源旁边的注册表条目。内部的 `INetworkDevice` 迁到了 `Cosmos.Kernel.System.Network`，与唯一驱动它的协议栈放在一起。命名空间属于版本发布时冻结的内容，因此在 `PublicAPI.Shipped.txt` 仍为空时这样的迁移没有代价，之后就会成为破坏性变更。

**`Info` 是快照，`Diagnostics` 是内核读取快照所通过的视图。** 在 `Cosmos.Kernel.System.Diagnostics` 中，`Info` 类型是一个只读结构体，通过一次不加锁的读取从活动对象中复制出来：`DeviceNodeInfo`、`DeviceOfferInfo`、`PublishedDeviceInfo`、`KernelThreadInfo`，以及表示驱动程序清单中一个条目的 `DriverInfo`。`Diagnostics` 类型是某个子系统的静态诊断视图，在有快照的地方负责提供这些快照：`DriverDiagnostics`、`SchedulerDiagnostics` 和 `MemoryDiagnostics`。视图不一定是只读的：`SchedulerDiagnostics.RequestKill` 和 `MemoryDiagnostics.Collect` 会执行操作。这三个视图过去也带有 `Info` 后缀，直到这个后缀被赋予了单一含义；而驱动程序快照的名字之所以需要多一个词，只是因为视图占用了那个简单的名字。`Log` 保留原名：它负责写入，所以既不是视图也不是快照。

本节审视过但没有改动的有两点。`Manager` 表示受支持层中某个子系统的入口，而不是说该类型持有子系统的状态：`AlarmManager` 把调用转发给 `Cosmos.Kernel.Core` 内部的实现，上文正是基于这一点为它辩护的，因此 `PartitionManager` 与 `Gpt`、`Mbr` 和 `Ebr` 并列却没有状态，说明这个后缀在发挥作用，而不是失效。另外，受支持层中有两个类型嵌套在产生它们的类型中，即 `KeyEvent.KeyEventType` 和 `PartitionManager.PartitionLocation`；规则 1 决定的是类型是否公开，而不是它在哪里声明，一个脱离其产生者就没有意义的名字，正适合嵌套。`VfsMount` 曾是第三个，嵌套在 `VfsManager` 中。现在它是顶层类型，与 `VfsManager` 一起位于 `Cosmos.Kernel.System.FileSystem`，因为该命名空间容纳了整个 VFS，而管理器交出的其他所有类型（`IVfsFileHandle`、`IVfsDirectoryHandle`、`VfsStat`、`VfsStatFs`）在那里本来就是顶层类型。

---

<a id="the-declared-surface"></a>

## 声明的 API 面

项目通过在 `.csproj` 中设置 `<CosmosTrackPublicApi>true</CosmosTrackPublicApi>` 来启用跟踪（在 `Directory.Build.props` 中接入）。启用跟踪的有 `Cosmos.Kernel.System`、`Cosmos.Kernel.Core`、`Cosmos.Kernel.HAL` 和 `Cosmos.Kernel.Drivers`。

API 站点发布的正是这四个项目，没有其他项目：`docs/docfx.json` 列出了它们，未被跟踪的程序集不会被发布。这两份列表曾经出现过一次偏差，结果让 `Cosmos.Kernel.Boot.Limine` 的 28 个公开类型和 `Cosmos.Kernel.Debug` 的 1 个公开类型出现在了站点上，而按照策略两者都是内部的；同时还有两个条目，只是因为它们恰好根本没有声明任何公开类型，才没有生成任何内容。

这条规则只有一个方向。一个未被跟踪、却仍残留公开类型的程序集，并不是发布它的理由；它正是策略表最后一行所描述的待办事项。

这些待办事项并不只是 `Cosmos.Kernel` 带进来的那些。`Cosmos.Sdk` 会根据架构为每个内核直接添加指向 `Cosmos.Kernel.Core.X64` 或 `Cosmos.Kernel.Core.ARM64` 的 `PackageReference`，因此内核的补全列表在 x64 上包含 13 个公开类型（`ApicManager`、`Idt`、`IoApic`、`LocalApic`、`DeviceMapper` 以及 ACPI MADT 记录），在 arm64 上包含 7 个（`GIC` 系列，加上 `AcpiGic` 和 `DeviceMapper`），它们完全不经过聚合包就到达了用户手中。`Cosmos.Kernel.HAL.X64` 和 `Cosmos.Kernel.HAL.ARM64` 没有公开类型，所以架构这一层已经清理了一半。

启用跟踪的项目会引用 [Microsoft.CodeAnalysis.PublicApiAnalyzers](https://github.com/dotnet/roslyn-analyzers/blob/main/src/PublicApiAnalyzers/PublicApiAnalyzers.Help.md)，它要求每个 `public` 符号都出现在 `.csproj` 旁边的两个文件之一中：

| 文件 | 内容 |
|------|----------|
| `PublicAPI.Shipped.txt` | 上一个版本的 API 面。已冻结：只有在发布版本时才会改变。 |
| `PublicAPI.Unshipped.txt` | 此后新增、修改或删除的 API 面。发布时会被清空并移入 `Shipped`。 |

有两个诊断负责强制执行这一约定，二者都以构建错误的形式报告：

| 规则 | 含义 |
|------|---------|
| `RS0016` | 代码中存在某个公共符号，但两个文件中都没有声明它。 |
| `RS0017` | 某个已声明的符号在代码中已不存在。 |

其效果是，对公共 API 面的任何更改，无论有意还是无意，都必须与 `PublicAPI.Unshipped.txt` 的差异放在同一个提交中，让代码评审能够看到。

有三类内容不写入这两个文件，前两类是通过 `.editorconfig` 中的覆盖设置实现的，这些设置在对应路径下把 `RS0016`/`RS0017` 设为 `none`：

- `Cosmos.Kernel.System/ThirdParty/` 下内置的第三方代码目录（`SharpZipLib`、BigGustave PNG 解码器、LunarFonts TrueType 光栅化器）。正是这项豁免让那次清理得以进行，而不会搅动已声明的 API 面；如今这三个目录树已全部为 `internal`，因此它目前没有豁免任何内容，而是作为常设规则留给下一个内置的第三方代码树。
- 生成的 `KernelVersion.g.cs`，其中包含 `Kernel.VersionString`：声明 API 的格式会记录常量的值，而这个值每次标记版本号时都会变化。
- `internal` 符号，包括通过 `InternalsVisibleTo` 暴露给测试内核的所有内容。

<a id="changing-the-public-api"></a>

### 更改公共 API

1. 修改代码。此时构建会因 `RS0016` 或 `RS0017` 而失败。
2. 在仓库根目录运行 `make api`。它会重新构建被跟踪的项目，并把分析器的代码修复应用到 `PublicAPI.Unshipped.txt`。在每个诊断上使用 IDE 的快速修复（“Add to public API”）效果相同。
3. 把 txt 文件的差异与代码一起提交。

删除或更改已经交付的符号，会在 `Unshipped` 中记录为一行 `*REMOVED*`，这恰如其分：它就是一个破坏性变更，并在 PR 中以这种形式显现出来。在第一个版本发布之前，`Shipped` 还是空的，删除符号只需去掉对应的行。

代码修复会把新行插入到排序后的位置，而从不改动已有的行，所以文件的顺序取决于最后一个编辑它的人留下的样子。它的比较器是 `OrdinalIgnoreCase`，相同时再用 `Ordinal` 决定先后，这与任何 shell 排序都不同：曾经有人对 `Cosmos.Kernel.System/PublicAPI.Unshipped.txt` 运行过一次 `LC_ALL=C sort`，结果有 19 行被放到了代码修复永远不会放的位置，其中包括本应位于顶部的六个 `abstract` 条目和四个 `const` 条目。没有任何分析器会读取行的位置，因此没有任何东西报告过这个问题，将来也不会。请使用分析器自己的比较器排序，或者把文件交给代码修复处理：

```
python3 -c "p='src/Cosmos.Kernel.System/PublicAPI.Unshipped.txt'; L=open(p).read().split(chr(10)); L=[l for l in L if l]; open(p,'w').write(chr(10).join([L[0]]+sorted(L[1:],key=lambda s:(s.upper(),s)))+chr(10))"
```

---

<a id="package-validation"></a>

## 包验证

同一个 `CosmosTrackPublicApi` 标志还会在项目上启用 [NuGet 包验证](https://learn.microsoft.com/en-us/dotnet/fundamentals/package-validation/overview)，而 `Directory.Build.props` 中有一个设置基线的开关：

```xml
<CosmosApiBaselineVersion></CosmosApiBaselineVersion>
```

在第一个 Gen3 版本发布到 NuGet.org 之前，该属性保持为空。一旦把它固定为那个版本，两道防护就会自动启用：

- `PackageValidationBaselineVersion` 让每次打包都把包与基线版本进行比较，并在出现二进制破坏性变更时失败。
- `release.yml` 中的 `API compatibility guard` 步骤会从 NuGet.org 下载基线包，并针对刚刚构建出的包运行 [Microsoft.DotNet.ApiCompat.Tool](https://learn.microsoft.com/en-us/dotnet/fundamentals/apicompat/global-tool)，作为发布前的最后一道关卡。

有意引入的破坏性变更会记录在项目旁边的 `CompatibilitySuppressions.xml` 中（由 `dotnet pack /p:ApiCompatGenerateSuppressionFile=true` 生成），因此它同样会成为一份可供评审的差异。

---

<a id="versioned-docs"></a>

## 版本化文档

位于 `gh-pages` 根目录的文档站点跟随 main 分支：它是开发版文档，每次推送文档时都会由 `build-docs.yml` 重新构建。各个发布版本在它旁边拥有冻结的副本，在推送 `v*` 标签时由 `release.yml` 的 `publish-docs` 作业发布：

| 路径 | 内容 |
|------|----------|
| `/` | 开发版文档，跟随 main。 |
| `/vX.Y.Z/` | 从标签 `vX.Y.Z` 构建的文档，永不重新构建。 |
| `/latest/` | 最新发布版本的别名，为外部链接提供稳定的 URL。 |
| `/versions.json` | 版本列表，按从新到旧排列，外加 `latest` 标识。 |

站点导航栏中的版本下拉菜单来自 `docs/templates/custom/public/main.js`。它读取 `versions.json`，列出 `dev` 和每个发布版本，并在切换版本时，如果目标版本中存在同一页面，就让读者停留在该页面。只有在 `versions.json` 存在之后，也就是第一个打了标签的版本发布之后，它才会出现；在此之前，站点的行为与以前完全相同。

开发版部署使用 `keep_files: true`，因此永远不会清除 `v*/` 文件夹，代价是已删除的开发版页面会一直留在该分支上，直到被覆盖为止。

---

<a id="release-checklist"></a>

## 发布检查清单

发布一个版本会在这套系统中带来以下变化：

1. 打上标签 `vX.Y.Z`。CI 会构建各个包，运行 API 兼容性防护，发布到 NuGet.org，并冻结 `/vX.Y.Z/` 文档。
2. 把 `PublicAPI.Unshipped.txt` 的内容移到 `PublicAPI.Shipped.txt` 中（去掉成对的 `*REMOVED*` 行），让 `Unshipped` 保持为空。
3. 在 `Directory.Build.props` 中把 `CosmosApiBaselineVersion` 设为 `X.Y.Z`，让下一个周期以这个版本为基准进行验证。
