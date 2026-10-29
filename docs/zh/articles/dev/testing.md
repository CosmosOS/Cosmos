---
source_hash: edf985c0eddbd5cc55626a43d392ca4bf9ca80bd
---
<a id="testing"></a>

# 测试

NativeAOT-Patcher 有两个互补的测试层次：一是**单元测试**，在主机进程中运行，针对构建期工具链（patcher、扫描器、分析器）以及不需要硬件的内核库逻辑；二是**内核集成测试**，在 QEMU 中运行编译好的内核镜像，并通过二进制 UART 协议报告结果。

---

<a id="unit-tests"></a>

## 单元测试

单元测试位于 `tests/Cosmos.Tests.*` 项目（xunit）和 `tests/Cosmos.Kernel.Tests.System`（NUnit）中，使用标准的 .NET 测试运行器运行。它们不需要 QEMU，也不需要任何特殊的基础设施。CI 在每次推送和每个拉取请求上，将它们分别作为 `.NET Tests` 工作流（`.github/workflows/dotnet.yml`）中的独立作业运行。

<a id="running-unit-tests"></a>

### 运行单元测试

```bash
dotnet test tests/Cosmos.Tests.Patcher            # one toolchain project
dotnet test tests/Cosmos.Kernel.Tests.System      # the kernel library tests
```

<a id="test-projects"></a>

### 测试项目

- **Cosmos.Tests.Build.Asm**：验证汇编构建任务通过 clang 运行。
  - `Test1`
- **Cosmos.Tests.Build.Analyzer.Patcher**：验证代码中不存在 plug 架构错误。
  - `Test_AnalyzeAccessedMember`
  - `Test_MethodNotImplemented`
  - `Test_StaticConstructorTooManyParameters`
  - `Test_StaticConstructorNotImplemented`
- **Cosmos.Tests.Scanner**：验证所有必需的 plug 都被正确检测到。
  - `LoadPlugMethods_ShouldReturnPublicStaticMethods`
  - `LoadPlugMethods_ShouldReturnEmpty_WhenNoMethodsExist`
  - `LoadPlugMethods_ShouldContainAddMethod_WhenPlugged`
  - `LoadPlugs_ShouldFindPluggedClasses`
  - `LoadPlugs_ShouldIgnoreClassesWithoutPlugAttribute`
  - `LoadPlugs_ShouldHandleOptionalPlugs`
  - `FindPluggedAssemblies_ShouldReturnMatchingAssemblies`
- **Cosmos.Tests.SourceGenerators**：在内存中的编译上运行 `CosmosEntryPointGenerator` 和 `DriverManifestGenerator`，并精确检查生成的入口点、驱动程序清单和 `COSMOSGEN` 诊断；`FeatureParityTests` 使 `DriverFeature`、`KernelFeatures` 和测试桩保持一致，`ReferencedDriverTests` 覆盖来自被引用程序集的驱动程序，包括公共驱动程序和通过 `InternalsVisibleTo` 授权访问的驱动程序，`CachingTests` 检查添加驱动程序后入口点仍保持缓存。
- **Cosmos.Tests.Patcher**：确保 plug 被成功应用到目标方法和类型上。
  - `PatchAssembly_ShouldSkipWhenNoMatchingPlugs`
  - `PatchObjectWithAThis_ShouldPlugInstanceCorrectly`
  - `PatchConstructor_ShouldPlugCtorCorrectly`
  - `PatchProperty_ShouldPlugProperty`
  - `PatchType_ShouldReplaceAllMethodsCorrectly`
  - `PatchType_ShouldPlugAssembly`
  - `AddMethod_BehaviorBeforeAndAfterPlug`
- **Cosmos.Kernel.Tests.System**：在主机进程中测试 `Cosmos.Kernel.System` 里不需要硬件的逻辑（`TcpConnection` 的接收缓冲区、`Address` 的标识与格式化、`WaveHeader` 的 WAV 读取、`MemoryAudioStream` 和 `WaveAudioStream`（后者基于内存中的 `IVfsFileHandle`），以及 `ToneAudioStream` 为 `Console.Beep` 生成的方波）。每个被测成员对应一个嵌套夹具（fixture），并持有该库授予的 `InternalsVisibleTo` 授权。
  - `AppendToData.WhenBothData_AndOtherAreEmpty_DataIsEmpty`
  - `AppendToData.WhenDataIsNotEmpty_AndOtherIsEmpty_DataDoesNotChange`
  - `AppendToData.WhenDataIsNotEmpty_AndOtherIsNotEmpty_OtherIsAppendedToData`
  - `AppendToData.WhenPartOfDataWasRead_AndOtherIsAppended_TheUnreadDataIsKept`
  - `AppendToData.WhenPartOfDataWasRead_AndOtherOutgrowsTheBuffer_TheUnreadDataIsKept`
  - `AppendToData.WhenAllDataWasRead_AndOtherIsAppended_OnlyOtherIsKept`
  - `AdvanceDataOffset.WhenAdvancingByZero_NoChangesAreMade`
  - `AdvanceDataOffset.WhenAdvancingByOneAndLengthIsTwo_OnlyLastElementRemains`
  - `AdvanceDataOffset.WhenAdvancingByTwoAndLengthIsTwo_DataLengthIsZero`
  - `Constructors.GivenPackedValue_SplitsItMostSignificantOctetFirst`
  - `Constructors.GivenBufferAndOffset_ReadsFourBytesFromTheOffset`
  - `Constructors.GivenSpanOfWrongLength_Throws`
  - `Id.GivenOctets_PacksThemMostSignificantFirst`
  - `Equality.GivenTheSameOctets_TwoInstancesAreEqualAndHashAlike`
  - `Equality.GivenDifferentOctets_TwoInstancesAreNotEqual`
  - `Equality.GivenNull_IsNotEqual`
  - `CompareTo.GivenTwoAddresses_OrdersByPackedValue`
  - `CompareTo.GivenNull_OrdersAfterIt`
  - `Formatting.GivenAddress_WritesDottedDecimal`
  - `IsBroadcastAddress.GivenAllOnes_IsTrue`
  - `IsBroadcastAddress.GivenAnyOtherAddress_IsFalse`
- **Cosmos.Tests.NativeWrapper**：包含运行时资源；没有单元测试。
- **Cosmos.Tests.NativeLibrary**：提供测试所用的原生代码；没有单元测试。

---

<a id="kernel-integration-tests"></a>

## 内核集成测试

内核集成测试会编译一个真实的 NativeAOT 内核，在 QEMU 中启动它，并通过二进制 UART 协议把结果传回主机。这些测试覆盖完整的构建与运行流水线。

<a id="test-suites"></a>

### 测试套件

| 套件 | 测试数 | 说明 |
|-------|-------|-------------|
| **HelloWorld** | 3 | 基本算术、布尔逻辑、整数比较 |
| **Memory** | 74 | 装箱/拆箱、结构体相等性、内存分配、集合、内存复制、地址转换 |
| **Drivers** | 每个单元格（cell）63 个 | 合成总线上的驱动工具包：清单、仲裁、发布、中断、延迟工作、拆除、子节点、到达显示设备管理器的显示设备、到达存储管理器的块设备；在 x64 上，q35 默认网卡上的 PCI 主机和 E1000E 驱动程序；在 usb-kbd 单元格上，启动时插入的 usb-kbd 上的 xHCI 驱动程序和 USB 键盘驱动程序，该键盘先被套件自己的驱动程序拒绝，再通过 QMP 注入一个按键，然后被拔出并重新插入；在 usb-kbd-ehci、usb-kbd-ohci、usb-kbd-ehci-ohci 和 usb-kbd-ohci-hub 单元格上，同一键盘上的 EHCI 和 OHCI 驱动程序以及 USB 键盘驱动程序，键盘分别位于 usb-ehci 上、pci-ohci 上、把全速键盘交给其 pci-ohci 伴随控制器的 ICH9 EHCI 上，以及 pci-ohci 上的 usb-hub 之后；在 usb-mouse 单元格上，启动时插入的 usb-mouse 上的 USB 鼠标驱动程序，并通过 QMP 注入指针事件；在 x64 上，q35 内置控制器上的 8042 驱动程序以及 PS/2 键盘和鼠标驱动程序，并通过 QMP 注入一个按键和指针事件；在 virtio-blk-pci 和 virtio-blk-mmio 单元格上，单元格磁盘上的 virtio-blk 驱动程序；在 virtio-blk-pci 上，磁盘所在端口上的 PCI Express 根端口驱动程序，并将磁盘拔出后重新插入 |
| **Pci** | 8 | 驱动工具包的 PCI 主机节点、其配置机制及其发布的功能节点，通过 `DriverDiagnostics`、主机的 `PciHostAccess` 和节点的 `PciAccess` 读取，并与对主机各总线的原始扫描结果比对 |
| **Virtio** | 每个单元格 14 个 | 驱动工具包之上的 virtio 驱动程序，覆盖两种传输方式以及关闭 ACPI 的情况：已绑定的网络、输入和块节点，被消费的接口及其每个单元格的链路、MAC 地址和中断模式，已注册的磁盘，以及由其传输驱动程序绑定的 PCI 功能或 MMIO 插槽 |
| **Graphic** | 25 | 每个单元格上的 2D 画布、字体和图像；每个单元格的显示设备管理器和主显示设备；virtio-gpu 单元格上的 virtio-gpu 显示驱动程序；vmware-svga 上通过能力接口（facet）访问的 VMware SVGA II 适配器及其 SVGA3D FIFO 指令流测试 |
| **Audio** | 每个单元格 22 个 | `bare` 和 `hda` 两个单元格上的音频栈：绑定到 QEMU `intel-hda` 的 Intel HD Audio 驱动程序、它发布的输出以及列出该输出的音频管理器、格式契约、排空环形缓冲区的流引擎及其在中断线已连接时的完成中断，以及播放器和后台播放：将一个流播放到结束、提前停止一个流、从内存播放一个 WAV 文件，还有 `Console.Beep` 通过该输出播放其音调、在输出繁忙时立即返回；在 bare 上，没有输出的管理器拒绝播放，`Console.Beep` 立即返回；在两者上，`Console.Beep` 都拒绝 Windows 所拒绝的参数 |
| **Storage** | 每个单元格 83 个 | 存储管理器及其背后的块设备，涵盖七种配置方案（profile）`ahci`、`nvme`、`usb`、`usb-ehci`、`usb-ohci`、`virtio-blk-pci` 和 `virtio-blk-mmio`（arm64），每种在 x64 上与 `acpi-off` 修饰符组合（12 个单元格），在 arm64 上与 `gicv2`、`gicv3` 和 `acpi-off` 组合（42 个单元格）：由工具包的 `AhciDriver`、`NvmeDriver`、`UsbMassStorageDriver` 或 `VirtioBlkDriver` 发布的磁盘，接受相同的块 I/O、分区表、分区生命周期和重启断言；每个单元格上工具包在 `DriverDiagnostics` 中对该磁盘的视图；每个单元格的 NVMe 中断模式；usb 单元格上的 USB 热插拔和 virtio-blk-pci 单元格上的 PCI Express 热插拔 |
| **Random** | 16 | 内核 CSPRNG：BLAKE2s 和 ChaCha20 已知答案测试、快速密钥擦除、RDSEED/RDRAND 和 RNDRRS/RNDR 检测、播种，以及构建于其上的 `RandomNumberGenerator`、`GetInt32` 和 `Guid.NewGuid`，测试跨越一次重新播种并从两个线程调用 |
| **Iso9660** | 47 | ISO 9660 驱动程序，基于 xorriso 在编译时从套件的 fixture 目录树构建的两个镜像：在 CD 和磁盘几何结构上挂载，拒绝全零、损坏或过短的设备，Rock Ridge 的文件名、属性、时间戳、符号链接、continuation 区和重定位目录，不区分大小写匹配的 level 1 原始标识符，整体读取、分块读取、seek 后读取和逐行读取，经由 `System.IO`（`IOException`）和 `VfsManager`（`false`）的只读挂载，卸载和重新挂载 |

<a id="helloworld-tests"></a>

#### HelloWorld 测试

- `Test_BasicArithmetic`：加法（2+2=4）
- `Test_BooleanLogic`：真/假断言
- `Test_IntegerComparison`：相等和比较运算符

<a id="memory-tests"></a>

#### Memory 测试

**装箱/拆箱（11 个测试）：**
- `Boxing_Char`, `Boxing_Int32`, `Boxing_Byte`, `Boxing_Long`
- `Boxing_Nullable`, `Boxing_Interface`, `Boxing_CustomStruct`
- `Boxing_ArrayCopy`, `Boxing_Enum`, `Boxing_ValueTuple`, `Boxing_NullInterface`

**结构体相等性（3 个测试）**：继承的 `ValueType.Equals`/`GetHashCode` 以及 `EqualityComparer<T>.Default`，分别作用于逐字节比较的结构体、带引用字段的 `record struct`（使用其自身生成的 `Equals`），以及带引用字段的普通结构体，CoreLib 通过 ILC 注入的 `__GetFieldHelper` 和 `RhBoxAny` 对后者逐字段比较
- `StructEquals_BitwiseFields`, `StructEquals_RecordStructReferenceField`, `StructEquals_ReferenceField`

**内存分配（8 个测试）：**
- `Memory_CharArray`, `Memory_StringAllocation`, `Memory_IntArray`
- `Memory_StringConcat`, `Memory_StringBuilder`
- `Memory_ZeroLengthArray`, `Memory_EmptyString`, `Memory_LargeAllocation`

**泛型集合 - List（14 个测试）：**
- `Collections_ListInt`, `Collections_ListString`, `Collections_ListByte`
- `Collections_ListLong`, `Collections_ListStruct`
- `Collections_ListContains`, `Collections_ListIndexOf`, `Collections_ListRemoveAt`
- `Collections_ListInsert`, `Collections_ListRemove`, `Collections_ListClear`
- `Collections_ListToArray`, `Collections_ListForeach`, `Collections_ListEmpty`

**泛型集合 - Dictionary（9 个测试）：**
- `Collections_DictCustomComparer`, `Collections_DictAddGet`, `Collections_DictIndexer`
- `Collections_DictContains`, `Collections_DictRemove`, `Collections_DictClear`
- `Collections_DictTryGetValue`, `Collections_DictKeysValues`, `Collections_DictEmpty`

**泛型集合 - IEnumerable（1 个测试）：**
- `Collections_IEnumerable`

**内存复制 / SIMD（15 个测试）：**
- `MemCopy_8Bytes`, `MemCopy_16Bytes`, `MemCopy_24Bytes`, `MemCopy_32Bytes`
- `MemCopy_48Bytes`, `MemCopy_64Bytes`, `MemCopy_80Bytes`, `MemCopy_128Bytes`
- `MemCopy_256Bytes`, `MemCopy_264Bytes`
- `MemSet_64Bytes`, `MemMove_Overlap`, `MemMove_Overlap_DestBeforeSrc`
- `MemCopy_0Bytes`, `MemCopy_1Byte`

**Array.Copy（5 个测试）：**
- `ArrayCopy_IntArray`, `ArrayCopy_ByteArray`, `ArrayCopy_LargeArray`
- `ArrayCopy_ZeroLength`, `ArrayCopy_Overlap`

**垃圾回收（22 个测试）：**
- `GC_IsEnabled`, `GC_GetStats`, `GC_CollectBasic`, `GC_StatsIncrement`
- `GC_ExactCollectionCount`, `GC_ObjectSurvival`, `GC_StringSurvival`
- `GC_ArraySurvival`, `GC_ListSurvival`, `GC_UnreachableExactCount`
- `GC_ObjectGraphSurvival`, `GC_MixedTypeSurvival`, `GC_AllocAfterCollect`
- `GC_WeakReference`, `GC_LargeAllocCollect`, `GC_StructArraySurvival`
- `GC_DictSurvival`, `GC_PageAccounting`, `GC_DependentHandle`
- `GC_DependentHandleCleanup`, `GC_HandleStoreIntegrity`, `GC_PinnedHeapReuse`

<a id="drivers-tests"></a>

#### Drivers 测试

该套件由两个项目组成。`tests/Kernels/Cosmos.Kernel.Tests.Drivers` 是内核：测试装置（harness）由 `Kernel.cs` 及其两个消费者 `TestKeyboardConsumer` 和 `TestPointerConsumer` 组成，持有 `Cosmos.Kernel.HAL` 授予的 `InternalsVisibleTo` 授权，它把这项授权用于其消费者，并在硬件相关的组中用于通过 `DriverEngine.Nodes` 访问随附驱动程序的状态：`TestKeyboardConsumer` 派生自内部的 `KeyboardConsumer`，`TestPointerConsumer` 派生自内部的 `PointerConsumer`，二者都在 `BeforeRun` 中通过内部的 `DeviceRegistry.SetConsumer` 安装，在本次运行期间替换受支持层自身的键盘和指针消费者，这正是“每个种类只有一个消费者”规则的体现。`tests/Kernels/Cosmos.Kernel.Tests.Drivers.Library` 包含该套件驱动的每个 `[Driver]` 类及其所需的状态和标识类型：它是一个驱动程序程序集（`<CosmosDriverAssembly>true</CosmosDriverAssembly>`，列在 `CosmosDriverAssemblyNames` 中），不持有任何项目的授权，只基于公共扩展点编写，因此它能通过编译，就证明第三方可以编写其中的每一个驱动程序。内核引用该库，在 x64 上运行于八个单元格，在 arm64 上运行于九个：`bare`、`usb-kbd`（一个 qemu-xhci 控制器，启动时插有一个 usb-kbd）、`usb-kbd-ehci`（同一键盘接在 usb-ehci 上）、`usb-kbd-ohci`（接在 pci-ohci 上）、`usb-kbd-ehci-ohci`（一个 ICH9 EHCI，其端口上有一个 pci-ohci 伴随控制器，并带有一个 `usb_version=1` 键盘，EHCI 会把它交给伴随控制器）、`usb-kbd-ohci-hub`（一个 pci-ohci，根端口 1 上接有 usb-hub，键盘位于其后）、`usb-mouse`（xHCI 上接有一个 usb-mouse）、`virtio-blk-pci`（PCI Express 根端口之后的一个 virtio-blk 磁盘），以及仅在 arm64 上的 `virtio-blk-mmio`（同一磁盘位于 virt 机器的窗口中）；它通过合成总线驱动库中的驱动程序，任何节点背后都没有硬件，同时在每个单元格所带的硬件上运行随附驱动程序（即下文的硬件、USB 主机、USB 键盘、USB 鼠标、PS/2、virtio-blk 和 PCI Express 根端口组），并通过 `SyntheticBus.WaitForQueuedJobs` 等待工具包。它在构建时关闭 `CosmosEnableFat`，并带有一个 `CosmosDriverExclude` 项和一个 `CosmosDriverInclude` 项，二者都指定库中的类型，因此清单策略同样在测试之列。每个断言都读取 `DriverDiagnostics` 或套件自己的驱动程序和消费者，从不读取串口日志。

四个 EHCI 和 OHCI 单元格体现了配置方案目录中 `usbController` 字段的作用：`xhci`（默认值，`qemu-xhci`）、`ehci`（`usb-ehci`）、`ohci`（带六个根端口的 `pci-ohci`）或 `ehci-ohci`（`ich9-usb-ehci1`，其六个端口上有一个 `pci-ohci` 伴随控制器）；在伴随控制器单元格上，`usbDeviceVersion` 设为 1，使 QEMU 的键盘只呈现全速描述符集；`usbHub` 则在根端口 1 上放置一个 usb-hub，并把键盘接在其后，启动器和目录只在两种 OHCI 型号上接受它，因为 QEMU 的集线器是全速的。在 xHCI 以外的三种型号上，启动器把每个设备固定到一个根端口（键盘为 1，鼠标为 2，U 盘从 3 开始），否则 QEMU 会在最后一个空闲端口上插入它自己的集线器；这四个配置方案都指定 `nic: none`，因为 q35 默认的 e1000e 与这些控制器共享传统中断线，它的驱动程序会先占用这条中断线，使控制器只能轮询。`cosmos run --usb-controller <xhci|ehci|ohci|ehci-ohci>` 为手动启动的内核提供同样的选择，仅在连接了 USB 设备时生效。

库中驱动程序的清单顺序遵循被引用程序集的规则：生成器先按程序集名称、再按完整类型名称对它们排序（均为序数比较），排在内核自身的驱动程序之后。`Manifest_Order_ReferencedByTypeName` 断言 `TieFirstDriver` 仅凭这条规则就排在 `TieSecondDriver` 之前（`F` 排在 `S` 之前）；两者在何处声明不起任何作用。`Interrupt_WorkItemDeferredToWorker` 无需询问引擎工作项在哪里运行即可证明延迟：中断处理程序在返回时、中断仍处于禁用状态下记录工作项的运行次数，因此次数不变就说明该工作项没有在 `RaiseInterrupt` 内运行，而在 `WaitForQueuedJobs` 之后看到的那次运行来自工作线程。

**清单（6 个测试）：**
- `Manifest_HighPriorityDriver_Present`, `Manifest_FatFeatureDriver_Absent`, `Manifest_ExcludedDriver_Absent`
- `Manifest_OptInDriver_Present`, `Manifest_OptOutDriver_Absent`, `Manifest_Order_ReferencedByTypeName`

**引擎（2 个测试）：**
- `Engine_Started_WithWorker`, `BootPath_NodeFromConstructor_Bound`

**仲裁（5 个测试）：**
- `Arbitration_ByPriority`, `Arbitration_BySpecificity`, `Arbitration_TieByManifestOrder`
- `Decline_UnwindsResources`, `ThrowingProbe_RecordedAsFailed`

**键盘设备（7 个测试）：**
- `Publish_ReachesConsumer`, `WindowAndDma_Contents`
- `Interrupt_HandlerReadsWindow_ReportsKey`, `Interrupt_WorkItemDeferredToWorker`, `Interrupt_MaskUnmask`
- `Periodic_FiresAtLeastThreeTimes`, `BlockingHandler_FaultRecorded`

**显示设备（1 个测试）：**
- `Publish_Display_ReachesDisplayManager`：库中的 `DisplayDriver` 发布一个 `DisplayState`，即一个处于固定 64x32 模式、没有帧缓冲区的显示设备；`DisplayManager.Count` 增加 1，可在 `DriverDiagnostics` 中通过节点路径找到该显示设备且 `IsConsumed` 成立，它报告驱动程序的模式，是主显示设备（驱动程序的显示设备优先于固件的显示设备），并以能力接口的形式提供该状态，但不提供 `IDisplayModes`；撤回之后，它已离开管理器和已发布列表，不再有能力接口，主显示设备重新变为固件显示设备，或者不存在主显示设备

**块设备（1 个测试）：**
- `Publish_Block_ReachesStorageManager`：库中的 `BlockDriver` 发布一个 `BlockState`，即一个基于字节数组、名为 `synthetic-block` 的 64 块空白磁盘；`StorageManager.DeviceCount` 增加 1，`Devices` 包含该状态，`GetPartitions` 在其上找不到任何分区，在没有注册其他设备时它就是主设备，`DriverDiagnostics` 在其节点下把它列为已被消费、未被撤销、由驱动程序命名的块设备；撤回之后，计数、列表和主设备都恢复原状，已发布的条目消失，驱动程序记录了一次探测和一次分离。UART 输出 `[StorageManager] synthetic-block registered by BlockDriver (primary)` 和 `[StorageManager] synthetic-block unregistered (no primary)`，这些内容从日志中读取，不做断言

**撤回（3 个测试）：**
- `Retract_DetachOrderAndAccounting`, `Retract_DriverThreadExited`, `Retract_SinkReportDiscarded`

**子节点（3 个测试）：**
- `Children_PublishedFromProbe`, `UnmatchedNode_UnboundWithNoOffers`, `Children_RetractedWithParent`

**诊断（1 个测试）：**
- `DriverDiagnostics_OutOfRange_ReturnsFalse`

**硬件（5 个测试）：**
- `Hardware_PciHost_Bound`, `Hardware_E1000E_NodeBound`, `Hardware_E1000E_DeviceConsumed`
- `Hardware_E1000E_LinkUp`, `Hardware_E1000E_Transmit`

硬件组在真实的（模拟的）硬件上运行随附驱动程序，没有专门的配置方案：每当单元格没有传入 `-netdev`，QEMU 的 q35 都会添加一个默认的 e1000e，因此套件的 x64 bare 单元格本身就带有该控制器；而 virt 的默认网卡是一个 virtio-net-pci 功能，由工具包的 `VirtioNetDriver` 绑定，因此四个 E1000E 测试在 arm64 上以 `no e1000e on this machine` 跳过；在四个 EHCI 和 OHCI 单元格上，配置方案指定 `nic: none` 以空出控制器的传统中断线，这些测试在 x64 上同样跳过。它们的门控条件是一个 `BusName` 为 `pci`、`Description` 以 `8086:10d3` 开头的 `DriverDiagnostics` 节点。`Hardware_PciHost_Bound` 无条件运行：一个描述中包含 `pci-host-` 的 `platform` 节点被 `PciHostDriver` 绑定（`Bound`）。`Hardware_E1000E_NodeBound` 检查该功能被 `E1000EDriver` 绑定（`Bound`），发布了一个设备，并至少持有七项资源（寄存器窗口、四个 DMA 缓冲区、排空工作项及其周期性注册，中断线已连接时还有中断句柄）；`Hardware_E1000E_DeviceConsumed` 在该节点路径上找到一个种类为 `Network` 的已发布设备，其 `IsConsumed` 为 true，且 `NetworkManager.MacAddress` 非零；`Hardware_E1000E_LinkUp` 轮询 `NetworkManager.LinkUp`，最长 2 秒；`Hardware_E1000E_Transmit` 利用内核的 HAL 授权，通过 `DriverEngine.Nodes` 访问节点的 `E1000EState`，经 `NetworkManager.Send` 发送一个 60 字节的广播帧，并断言 `FramesTransmitted` 有所增长。

**USB 主机（3 个测试）：**
- `Usb_XhciHost_Bound`, `Usb_EhciHost_Bound`, `Usb_OhciHost_Bound`

每个随附的主机驱动程序对应一个测试，各自在总线上带有相应功能的单元格上运行，在其他单元格上以 `no xHCI controller on this cell`、`no EHCI controller on this cell` 或 `no OHCI controller on this cell` 跳过：xHCI 测试运行于 usb-kbd 和 usb-mouse，EHCI 测试运行于 usb-kbd-ehci 和 usb-kbd-ehci-ohci，OHCI 测试运行于 usb-kbd-ohci、usb-kbd-ohci-hub 和 usb-kbd-ehci-ohci，后者是唯一带两个控制器的单元格。门控条件是一个 `BusName` 为 `pci`、`Description` 包含 `class 0c.03.30`、`class 0c.03.20` 或 `class 0c.03.10`（即 xHCI、EHCI 或 OHCI 功能）的 `DriverDiagnostics` 节点，它在 `BeforeRun` 中被查找，不论其状态如何，因此探测失败会表现为测试失败，而不是跳过。每个测试都检查该功能被其驱动程序绑定（`Bound`）并持有一项资源，并通过 HAL 授权检查其状态（`XhciState`、`EhciState` 或 `OhciState`）运行着热插拔线程，且通过中断或轮询获取事件，既不会两者兼有，也不会两者皆无；EHCI 和 OHCI 是仅支持 INTx 的功能，因此在为它们定制的 x64 单元格上（配置方案已空出中断线），使用的路径必须是中断，而在不路由任何传统中断线的 arm64 上，它们使用轮询。总线序号遵循绑定顺序：xHCI 以及单独的 EHCI 或 OHCI 是总线 1，至少有一个设备，即启动时插入的键盘或鼠标，其节点位于控制器节点之下。在 usb-kbd-ehci-ohci 上，先绑定的 EHCI 是总线 1，没有子节点也没有设备，且 `CompanionPortsReleased` 至少为 1，因为全速键盘已通过端口所有者位交给了伴随控制器；后绑定的 OHCI 是总线 2，带有该键盘；在 usb-kbd-ohci-hub 上，OHCI 的总线至少有两个设备，即集线器及其后的键盘，键盘节点位于集线器节点之下，集线器节点由 `UsbHubDriver` 绑定，位于控制器节点之下。

**USB 键盘（6 个测试）：**
- `Usb_Keyboard_BoundAfterDecline`, `Usb_Keyboard_SetLedsRoundTrip`, `Usb_Keyboard_KeyInjected`
- `Usb_Keyboard_Unplug_RetractsNode`, `Usb_Keyboard_Replug_PublishesAgain`, `Usb_Keyboard_Replug_SetLeds`

它们在五个 usb-kbd 单元格上运行，无论键盘挂在哪个主机上；在没有控制器的单元格上以 `no USB host controller on this cell` 跳过，在 usb-mouse 上以 `no USB keyboard on this cell` 跳过。门控条件是一个 `Description` 以 `class 03.01.01`（HID 启动键盘接口）结尾、上方有主机功能节点的 `usb` 节点，它在 `BeforeRun` 中被查找，由于主机探测在驱动程序阶段扫描根端口，这一阶段能看到启动时存在的每个设备（在集线器单元格上，键盘要深一层分派，由集线器的探测发布，需要等待）：xHCI 上为 `usb:1-5:0`（QEMU 的 xHCI 把 USB 3 端口编号为 1 到 4，把 USB 2 端口编号为 5 到 8），单独的 EHCI 或 OHCI 上为 `usb:1-1:0`（它们的端口从 1 开始编号），伴随控制器单元格上为 `usb:2-1:0`（键盘属于 OHCI），集线器之后为 `usb:1-1.1:0`。主机的状态通过套件的 `UsbHostView` 读取，即从三种状态类型中持有该节点的那一种上读取同样的五个成员（`HotPlugRunning`、`HasInterrupt`、`IsPolling`、`Bus`、`PipesClosed`）。`Usb_Keyboard_BoundAfterDecline` 是“打开后拒绝”的证明：库中的 `UsbDeclineDriver` 以优先级 1 匹配同一个接口三元组，在探测中打开键盘的中断管道，然后以 `declined after opening a pipe` 拒绝，因此该节点的第一次分派（offer）被拒绝，`ReleasedResourceCount` 为 1，即工具包关闭的那个管道；第二次分派给了随附的 `UsbKeyboardDriver`，它完成绑定并发布一个设备，即被受支持层的管理器消费的键盘 `usb-keyboard`。`Usb_Keyboard_SetLedsRoundTrip` 在 `UsbKeyboardState` 上调用 `SetLeds`，并读回 `LastLedReport` 为 0x03、`LastLedStatus` 为 `Success`。`Usb_Keyboard_KeyInjected` 向引擎请求 `key-press a`，并以 50 毫秒为间隔轮询、最长等待 8 秒，直到套件的消费者收到来自 USB 键盘的两份报告，先是按下码 0x1E，然后是它的释放，同时驱动程序的 `KeyEvents` 增加 2：这是唯一一个在每种主机上都执行中断管道完成的测试，EHCI 的周期调度和 OHCI 的完成队列无法从其他单元格获得这种覆盖。`Usb_Keyboard_Unplug_RetractsNode` 向引擎请求 `usb-kbd-unplug`，并以 50 毫秒为间隔轮询、最长等待 8 秒，直到节点离开节点树、总线释放其设备：节点为 `Retracted`，其绑定为 `IsDetaching`，已发布的设备消失，节点数、设备数以及父节点（主机的节点，在集线器单元格上则是集线器的节点）的 `ChildCount` 各减少 1，套件消费者的 `WithdrawnCount` 增加 1，主机的 `PipesClosed` 至少为 1。`Usb_Keyboard_Replug_PublishesAgain` 请求 `usb-kbd-plug`，并等待一个新的、已绑定的键盘节点，它再次被 `UsbDeclineDriver` 拒绝，这次发布给了套件的消费者：在 xHCI 上，QEMU 选择下一个空闲端口，因此节点为 `usb:1-6:0`；而在 EHCI 和 OHCI 上，引擎固定键盘离开的那个端口，因此路径与原来相同；两种情况都会记录到日志（`[DriversTests] replugged keyboard at usb:1-6:0`），不做断言；`Usb_Keyboard_Replug_SetLeds` 通过新的状态写入一份 LED 报告。引擎的 `[Monitor] usb-kbd-unplug: done` 和 `[Monitor] usb-kbd-plug: done`，以及工具包的 `withdrew keyboard`、`retracted` 和 `usb 1-5: disconnected`（单独的 EHCI 或 OHCI 上为 `usb 1-1: disconnected`，伴随控制器单元格上为 `usb 2-1: disconnected`，集线器之后为 `usb 1-1.1: disconnected`）这些行从 UART 读取，不做断言；每个单元格的计数见下文的 PCI Express 根端口组。

**USB 鼠标（4 个测试）：**
- `Usb_Mouse_Bound`, `Usb_Mouse_MovementInjected`, `Usb_Mouse_ButtonInjected`, `Usb_Mouse_WheelInjected`

它们在 usb-mouse 单元格上运行；在没有控制器的单元格上以 `no USB host controller on this cell` 跳过，在 usb-kbd 单元格上以 `no USB mouse on this cell` 跳过。门控条件是一个 `Description` 以 `class 03.01.02`（HID 启动鼠标接口）结尾的 `usb` 节点，即 `usb:1-5:0`，它与键盘的节点一样在 `BeforeRun` 中被查找。`Usb_Mouse_Bound` 检查它在第一次分派时即被随附的 `UsbMouseDriver` 绑定（`Bound`），位于控制器节点之下，没有资源、没有中断，并发布了一个设备，即被受支持层的鼠标管理器消费的指针 `usb-mouse`。其余三个测试向引擎请求 PS/2 组所用的指针事件，一旦驱动程序的管道轮询过 QEMU 的 usb-mouse，QEMU 就会把这些事件交给它，而不是交给 q35 的 PS/2 鼠标：`Usb_Mouse_MovementInjected` 请求 `mouse-move 10 0`，并以 50 毫秒为间隔轮询、最长等待 8 秒，直到套件的指针消费者从 `usb-mouse` 累计得到 10 和 0 的位移，没有按键也没有滚轮，并通过 HAL 授权从 `UsbMouseState` 读到一份已转发的报告、没有过短的报告，以及一份 4 字节的最后报告，即启动协议部分加上 QEMU 的滚轮字节；`Usb_Mouse_ButtonInjected` 请求 `mouse-button left down` 和 `up`，从 `usb-mouse` 读到左键，随后读到无按键；`Usb_Mouse_WheelInjected` 请求 `mouse-button wheel-down down` 和 `up`，读到滚轮总量 +1，即受支持层中的向下滚动，从而固定了驱动程序对 HID 滚轮值的取反。

**PS/2（7 个测试）：**
- `Ps2_Controller_Bound`, `Ps2_Keyboard_Bound`, `Ps2_Keyboard_KeyInjected`, `Ps2_Keyboard_SetLedsRoundTrip`
- `Ps2_Mouse_Bound`, `Ps2_Mouse_MovementInjected`, `Ps2_Mouse_ButtonInjected`

它们在每个 x64 单元格上运行，在 arm64 上以 `no 8042 on this cell` 跳过；`Ps2_Keyboard_KeyInjected` 还会在每个 usb-kbd 单元格上以 `a usb-kbd on this cell takes the host's keys` 跳过，因为存在 USB 键盘时 QEMU 会把 `send-key` 交给它；`Ps2_Mouse_MovementInjected` 和 `Ps2_Mouse_ButtonInjected` 在 usb-mouse 上以 `a usb-mouse on this cell takes the host's pointer events` 跳过，因为 USB 鼠标一旦被轮询，QEMU 就会把指针事件交给它。门控条件是一个 `BusName` 为 `platform`、`Description` 包含 `pnp0303` 的 `DriverDiagnostics` 节点。`Ps2_Controller_Bound` 检查 `platform:i8042@60` 被 `I8042Driver` 绑定（`Bound`），有两项资源、两个中断、两个子节点和四项持有的资源，并通过 HAL 授权检查其 `I8042State` 为双通道且由中断驱动。`Ps2_Keyboard_Bound` 和 `Ps2_Mouse_Bound` 读取其下的 `ps2:kbd` 和 `ps2:aux` 节点，它们分别被 `Ps2KeyboardDriver` 和 `Ps2MouseDriver` 绑定，`ps2-keyboard` 和 `ps2-mouse` 已发布并被消费，分别是一个 MF2 键盘和一个滚轮鼠标。`Ps2_Keyboard_KeyInjected` 向引擎请求 `key-press a`，并以 50 毫秒为间隔轮询、最长等待 8 秒，直到套件的消费者收到两份报告：来自 PS/2 键盘的按下码 0x1E，然后是它的释放。`Ps2_Keyboard_SetLedsRoundTrip` 在 `Ps2KeyboardState` 上调用 `SetLeds`，读回随 0xED 命令发送的字节 0x06，且已被确认。`Ps2_Mouse_MovementInjected` 请求 `mouse-move 10 0`，在套件的指针消费者中读到一份来自 `ps2-mouse` 的 10 和 0 的相对报告；`Ps2_Mouse_ButtonInjected` 请求 `mouse-button left down` 和 `up`，在按键状态中读到左键，随后读到无按键。引擎的 `[Monitor] key-press a: done`、`[Monitor] mouse-move 10 0: done`、`[Monitor] mouse-button left down: done` 和 `[Monitor] mouse-button left up: done` 从其输出中读取，不做断言。

**virtio-blk（6 个测试）：**
- `VirtioBlk_Bound`, `VirtioBlk_TransportMatchesCell`, `VirtioBlk_CapacityMatchesImage`
- `VirtioBlk_ReadWriteRoundTrip`, `VirtioBlk_FlushCompletes`, `VirtioBlk_InterruptOrPolled`

它们在 virtio-blk-pci 和 virtio-blk-mmio 单元格上运行。该组在 bare、各 usb-kbd 单元格和 usb-mouse 上以 `no virtio-blk device on this cell` 跳过；被测驱动程序是随附的 `VirtioBlkDriver`，它最初存放在本套件的库中。门控条件是一个 `BusName` 为 `virtio`、`Description` 以 `type 2 ` 开头的 `DriverDiagnostics` 节点，每个测试体都通过 HAL 授权读取该节点的 `VirtioBlkState`。`VirtioBlk_Bound` 检查节点被 `VirtioBlkDriver` 绑定（`Bound`），发布了一个设备，该设备已被消费、未被撤销，并以状态命名，即 `vblk0`，在 `StorageManager.Devices` 中的索引为 0。`VirtioBlk_TransportMatchesCell` 检查：当 PCI 总线上有 virtio-blk 功能（`1af4:1001` 或 `1af4:1042`）时，路径以 `virtio:pci:` 开头，否则以 `virtio:mmio:` 开头；并且父节点被 `VirtioPciTransportDriver` 或 `VirtioMmioTransportDriver` 绑定（`Bound`），带有一个子节点。`VirtioBlk_CapacityMatchesImage` 读到 524288 个 512 字节的块，即引擎的 256 MiB 镜像，可写，每个请求至少一个块。`VirtioBlk_ReadWriteRoundTrip` 保存 LBA 200000 处的一个块及其后的 130 个块，在每一处写入一种测试图样，把两者读回并确认一致，然后写回保存的内容，再次读回并确认一致；130 个块共 66560 字节，超过单个请求传输的 65536 字节，因此分块逻辑会被执行。`VirtioBlk_FlushCompletes` 执行刷新，在协商了 FLUSH 时读到多完成了一个请求，否则不多。`VirtioBlk_InterruptOrPolled` 检查磁盘通过队列中断或轮询获取完成通知，既不会两者兼有，也不会两者皆无，并且在连接了中断时，往返操作触发了该中断；模式不做固定，因为这些单元格不带 GIC 修饰符（x64 磁盘和 arm64 MMIO 磁盘使用中断，arm64 PCI 磁盘在默认的 GICv2 上使用轮询）。在 arm64 上，引擎通过 virtio-scsi 控制器而不是 `-cdrom` 挂接启动 ISO，因为 virt 机器会把后者变成一个独立的 virtio-blk 功能，这样单元格上唯一的 virtio-blk 设备就是其配置方案挂接的磁盘。

**PCI Express 根端口（3 个测试）：**
- `Pci_RootPort_Bound`, `Pci_HotPlug_Unplug_RetractsNode`, `Pci_HotPlug_Replug_PublishesAgain`

它们在 virtio-blk-pci 单元格上运行，引擎把该单元格的磁盘放在一个专用的 `pcie-root-port` 之后；在其他单元格上以 `no PCI Express root port on this cell` 跳过。门控条件是一个 `Description` 以 `1b36:000c`（QEMU 的通用根端口）开头的 `pci` 节点，每个测试体都通过 HAL 授权读取其 `PcieRootPortState`。`Pci_RootPort_Bound` 检查：端口被 `PcieRootPortDriver` 绑定（`Bound`），带有一个子节点；插槽中有设备且已上电，其线程正在运行，事件通过消息中断或轮询获取，既不会两者兼有，也不会两者皆无，位于一条次级总线之后；子节点被 `VirtioPciTransportDriver` 绑定（`Bound`），其下有 virtio 节点；子节点的内存资源位于端口的内存窗口或可预取窗口之内，这些窗口由端口自身的寄存器解码得出；端口的资源 0 是一个内存窗口，即存放其 MSI-X 表的 BAR0，其资源 1 到 5 为 `None`。`Pci_HotPlug_Unplug_RetractsNode` 向引擎请求 `pci-unplug 0`，并以 50 毫秒为间隔轮询、最长等待 8 秒，直到功能节点离开节点树、端口处理完注意按钮：没有子节点，移除次数加一，插槽为空且已断电。此时功能节点为 `Retracted`，节点数减少 2（功能节点及其下的 virtio 节点），已发布的设备和存储管理器的磁盘各减少 1，端口仍为 `Bound`，子节点少了一个。`Pci_HotPlug_Replug_PublishesAgain` 请求 `pci-plug 0`，并等待同一路径上出现一个功能节点：它是一个新对象，被 `VirtioPciTransportDriver` 绑定（`Bound`），其 virtio 节点被 `VirtioBlkDriver` 绑定（`Bound`），磁盘已注册，到达次数加一，插槽已上电；此时节点数重新增加 2，插槽中有设备，新功能的内存资源位于端口窗口之内（由于 QEMU 不为热添加的功能分配寄存器，这些资源由工具包放置），`vblk0` 已发布并被消费。端口的 `slot 1: attention button, 1 functions retracted, slot powered off` 和 `slot 1: device arrived, 1 functions published`，以及引擎的 `[Monitor] pci-unplug 0: done` 和 `[Monitor] pci-plug 0: done`，从 UART 和引擎输出中读取，不做断言。各单元格的结果：x64 上，bare 通过 41 个、跳过 22 个，usb-kbd 47 和 16，usb-kbd-ehci 43 和 20，usb-kbd-ohci 43 和 20，usb-kbd-ehci-ohci 44 和 19，usb-kbd-ohci-hub 43 和 20，usb-mouse 44 和 19，virtio-blk-pci 50 和 13；arm64 上，bare 30 和 33，usb-kbd 37 和 26，usb-kbd-ehci 37 和 26，usb-kbd-ohci 37 和 26，usb-kbd-ehci-ohci 38 和 25，usb-kbd-ohci-hub 37 和 26，usb-mouse 35 和 28，virtio-blk-pci 39 和 24，virtio-blk-mmio 36 和 27；x64 上共 504 个，通过 355 个、跳过 149 个、失败 0 个；arm64 上共 567 个，分别为 326、241 和 0。

<a id="pci-tests"></a>

#### Pci 测试

`tests/Kernels/Cosmos.Kernel.Tests.Pci` 用八个测试在两种架构上检查驱动工具包的 PCI 支持（arm64 的 EDK2 启动提供 ACPI，因此那里存在 MCFG，ECAM 主机节点也存在）。四个主机测试无条件运行。`Host_PlatformNode_BoundByPciHostDriver`：一个描述中包含 `pci-host-` 的 `platform` 节点为 `Bound`，其 `DriverName` 为 `PciHostDriver`。`Host_ConfigMechanism_MatchesCompatible`：携带 `PciHostAccess` 的平台节点（通过内核的 HAL 授权在 `DriverEngine.Nodes` 中找到）至少解码一条总线；若与 `pci-host-legacy` 兼容，其机制为 `PciConfigSpace.Ports`，每个功能 256 字节，唯一的资源是位于 0xCF8、共 8 个端口的端口范围；若与 `pci-host-ecam-generic` 兼容，其机制为一个 `PciEcamConfigSpace`，每个功能 4096 字节，唯一的资源是一个内存窗口，主机映射的每条总线占 1 MiB；主机段和各总线上的每个功能节点都通过同一机制访问。`Host_PublishesPciNodes`：至少有一个 `pci` 节点以主机的路径作为 `ParentPath`，并且每个这样的节点的 `ResourceCount` 都为 6，`InterruptCount` 等于其传统中断线加上每条已描述消息各一个中断源：读取节点的 `PciAccess`，当 `pci.IsMsiXCapable` 时把计数与 `1 + Math.Min(pci.MessageInterruptCount, PciHostAccess.MaxDescribedMessages)`（最多 32）比较，否则与 1 比较；第一个中断源的 `Describe()` 以 `line` 开头，在支持 MSI-X 的功能上，第二个中断源的结果等于 `message 0 of ` 后接表大小，按序数比较。`Host_NodeCount_MatchesConfigSpaceScan`：通过 `PciHostAccess.ReadConfig16` 和 `ReadConfig8` 对主机解码的每条总线进行原始扫描（扫描设备 0 到 31 的功能 0，当头部类型带有多功能位时再扫描功能 1 到 7；厂商 ID 读数既不是 0xFFFF 也不是 0x0000 时，即视为功能存在），至少找到一个功能，且数量恰好等于主机段和各总线上带有 `PciIdentity` 的节点数。四个配置空间测试在每个功能节点（带有 `PciIdentity` 和 `PciAccess` 的节点）上运行，若没有这样的节点，则以 `no PCI function node in the tree, the PCI host was not described or published nothing` 跳过。`ConfigSpace_VendorId_NotAllOnes`：`PciAccess.ReadConfig16(0x00)` 既不是 0xFFFF 也不是 0x0000，且等于标识中的 `VendorId`。`ConfigSpace_DeviceId_NotAllOnes`：`ReadConfig16(0x02)` 不是 0xFFFF，且等于 `DeviceId`。`ConfigSpace_ClassCode_InRange`：标识中的 `ClassCode` 不大于 0x13，且 `ReadConfig8(0x0B)` 与其相等。`ConfigSpace_VendorRead_StableAcrossCalls`：通过节点两次读取厂商 ID 的结果一致，并与主机对同一寄存器的原始读取结果一致。两个默认单元格都带有支持 MSI-X 的功能（q35 的 e1000e、virt 的 virtio-net-pci）以及不具备该能力的功能（主桥），因此按节点计算的公式覆盖了各种组合；该项目抑制了 `COSMOS0003`，因为 `DeviceNode`、`DeviceResource`、`PlatformIdentity`、`PciHostAccess`、`PciIdentity` 和 `PciAccess` 都是实验性的。

<a id="virtio-tests"></a>

#### Virtio 测试

`tests/Kernels/Cosmos.Kernel.Tests.Virtio` 用相同的断言在两种传输方式上验证 `Cosmos.Kernel.Drivers` 中的 virtio 驱动程序。它的两个配置方案为每个单元格挂接一个 virtio 网卡、键盘、鼠标和块磁盘，每个配置方案都分别在开启和关闭 ACPI 的情况下运行：`virtio-pci` 和 `virtio-pci+acpi-off` 用于 x64 和 arm64（arm64 单元格以 `gic-version=3` 启动，因为路由功能的 MSI-X 消息的是 ITS），`virtio-mmio` 和 `virtio-mmio+acpi-off` 仅用于 arm64，通过 virt 机器的 virtio-mmio 窗口实现（q35 没有这样的窗口）；arm64 的 acpi-off 单元格是引导加载程序会传递设备树的单元格，因此在那里 ECAM 主机和各插槽都来自设备树。单元格呈现哪种传输方式是配置方案的属性，而不是架构的属性，因此同一个内核服务于每个 arm64 单元格，并在运行时检测传输方式：`BeforeRun` 捕获 virtio-net 节点（一个 `BusName` 为 `virtio`、`Description` 以 `type 1 ` 开头的 `DriverDiagnostics` 节点）、virtio-input 节点（`type 18 `）、virtio-blk 节点（`type 2 `）以及 PCI 功能节点（`BusName` 为 `pci`，`Description` 以 `1af4:1041` 或 `1af4:1000` 开头），后者的存在使该单元格成为 PCI 单元格。内核保留 HAL 授权有两个用途：像 Drivers 套件那样通过 `DriverEngine.Nodes` 访问节点的绑定状态，以及为唯一一个中断模式取决于架构的单元格读取 `PlatformHAL.Architecture`；内核还抑制了 `COSMOS0003`。

十四个测试如下：`Net_DriverBound`（网络节点由 `VirtioNetDriver` 绑定，状态为 `Bound`）、`Net_TransportMatchesCell`（其 `Path` 在 PCI 单元格上以 `virtio:pci:` 开头，否则以 `virtio:mmio:` 开头）、`Net_DeviceReady`（节点路径上有一个已发布的 `Network` 设备，其 `IsConsumed` 为 true，`NetworkManager.DeviceCount` 至少为 1，且 `NetworkManager.Ready` 成立）、`Net_LinkUp`（`NetworkManager.LinkUp`：QEMU 的 user 后端会立即报告链路已连通）、`Net_MacAddressProgrammed`（`NetworkManager.MacAddress` 不为 null 且不全为零）、`Net_InterruptModeMatchesCell`（节点的 `VirtioNetState.HasInterrupt` 为 true、`IsPolling` 为 false，但 acpi-off 下的 arm64 PCI 单元格除外：那里没有发现 ITS，驱动程序使用轮询，因此两个标志的值正好相反；其他情况下，MSI-X 经 PCI 路由，GIC 中断线经 MMIO 路由）、`Input_KeyboardBound` 和 `Input_MouseBound`（一个 `type 18` 节点由 `VirtioInputDriver` 绑定，状态为 `Bound`，其发布的 `Keyboard` 或 `Pointer` 设备已被消费）、`Blk_DriverBound`（一个 `type 2` 节点由 `VirtioBlkDriver` 绑定，状态为 `Bound`，其发布的 `Block` 设备为 `vblk0`，已被消费、未被撤销，是存储管理器唯一的磁盘）和 `Blk_TransportMatchesCell`（其路径指明单元格的传输方式，并且在 PCI 单元格上，virtio-blk 功能由 `VirtioPciTransportDriver` 绑定，状态为 `Bound`，带有一个子节点），然后是每种传输方式各两个。在 PCI 单元格上，`Pci_FunctionBoundByTransport`（功能节点由 `VirtioPciTransportDriver` 绑定，状态为 `Bound`，`ChildCount` 为 1）和 `Pci_Version1Negotiated`（`VirtioNetState.Version1Negotiated`；QEMU 的 virtio-mmio 默认为传统模式，因此 MMIO 单元格不断言这一点），在其他单元格上以 `this cell presents virtio over MMIO` 跳过。在 MMIO 单元格上，`Mmio_SlotBoundByTransport`（一个 `Description` 包含 `virtio,mmio` 的 `platform` 节点由 `VirtioMmioTransportDriver` 绑定，状态为 `Bound`，`ChildCount` 为 1，这一断言能发现丢失的 MMIO 窗口）和 `Mmio_AnyLayoutNegotiated`（传统设备上的 `VirtioNetState.AnyLayoutNegotiated`，QEMU 默认开启该设备的 `any_layout` 属性），在其他单元格上以 `this cell presents virtio over PCI` 跳过。Network 套件的 `virtio-net-pci` 和 `virtio-net-mmio` 单元格无需修改测试即可覆盖同一驱动程序的数据路径。各单元格的结果：x64 的两个单元格（`24/28`）和 arm64 的四个单元格（`48/56`）上，每个单元格都是通过 12 个、跳过 2 个。

<a id="graphic-tests"></a>

#### Graphic 测试

`tests/Kernels/Cosmos.Kernel.Tests.Graphic` 在三个配置方案上运行：`bare`、`vmware-svga`（仅限 x64，适配器通过端口 I/O 编程）和 `virtio-gpu`（在机器默认适配器旁添加一个 virtio-gpu-pci，两种架构均可），并在每个单元格上注册同样的 25 个测试：特定于单元格的测试在其设备所在之处运行，在其他地方以 `no virtio-gpu device on this cell, needs the virtio-gpu profile` 或 `VMware SVGA II adapter not present, needs the vmware-svga profile` 跳过。单元格在 `BeforeRun` 中根据设备树判定，从不根据配置方案名称或绑定了哪个驱动程序来判定，因此显示驱动程序绑定失败会使套件失败，而不是让它跳过：`Description` 以 `15ad:0405` 开头的 `pci` 节点标志着 vmware-svga 单元格，以 `1af4:1050` 开头的 `pci` 节点或以 `type 16 ` 开头的 `virtio` 节点标志着 virtio-gpu 单元格，两者都没有则为 bare。套件通过 `DisplayManager.Primary` 的能力接口访问 `Cosmos.Kernel.Drivers` 中的驱动程序状态，不持有任何授权；项目为这些能力接口抑制了 `COSMOS0003`。

2D 测试（9 个），每个单元格：`PCScreenFont_ChangeFont`、`Bitmap_Basic`、`Png_Decode`、`Ttf_Render`、`ColorClass_Basic`、`Canvas_Basic`、`VirtualCanvas_Basic`、`Canvas_CopyPixels_Overlap`、`Canvas_Compositing`（`DrawImage` 以及半透明 `DrawPoint` 在不透明像素上的精确混合值，`DrawArray` 原样复制，alpha 在透明图层上保留、在半透明图层上合成，`DrawCanvas` 和 `DrawImage` 带与不带不透明度，拉伸至 200% 和 150% 的 `DrawCanvas`，在左上角裁剪，并带有一个把每一重复行混合到其自身背景上的不透明度，以及 `DrawImage` 的区域和缩放重载）；`Bitmap_Basic` 还会把不带 alpha 的 32 位 BMP 作为不透明图像加载。显示测试（4 个），每个单元格：`Display_PrimaryPresent`（存在主显示设备，控制台的画布宽度与之相同，且 `Name` 以其 `DriverName` 开头）、`Display_PrimaryMatchesCell`（bare：主显示设备是固件显示设备，且是唯一的显示设备；virtio-gpu：`VirtioGpuDriver` 的显示设备为主显示设备，与固件显示设备并存；vmware-svga：`VmwareSvgaDriver` 的显示设备是唯一的显示设备，`DriverDiagnostics` 中没有不带节点路径的显示设备，因为固件显示设备已被停用）、`Display_ListedInDriverDiagnostics`（管理器的每个显示设备都是 `DriverDiagnostics` 中一个已被消费的 `Display` 设备，恰好只有固件显示设备的 `NodePath` 为 null）和 `Display_ModeRequest_FollowsFacet`（当主显示设备提供 `IDisplayModes` 时，`Canvas.GetFullScreen(new Mode(640, 480, ColorDepth.ColorDepth32))` 报告 640x480，否则报告实际尺寸；它在能力接口测试之后运行，其后只有 `Canvas3D_Discovery`，因为它会切换这些测试所测量的模式）。`VirtioGpu_DriverState`（1 个），在 virtio-gpu 单元格上：主显示设备提供一个 `VirtioGpuState`，它没有故障，有中断或使用轮询，至少有一个扫描输出（scanout），与主显示设备的尺寸一致，并已发送探测的四条命令；对控制台画布执行一次 `Display()` 会使 `FlushCount` 增加 1、`CommandsSent` 增加 2。SVGA 测试（9 个），在 vmware-svga 上：`Svga_AdapterFacet`（带有能力位的 `ISvgaAdapter`，`FifoMin` 小于 `FifoMax`，在 QEMU 上没有协商 3D，没有 `ICanvas3DFactory`，且扫描输出已启用，因为控制台已经设置了一种模式）、`Svga_DisplayModesFacet`（`IDisplayModes` 列出 1024x768x32，即控制台选择的默认模式）、`Svga_HardwareCursorFacet`（`IHardwareCursor`；`TryDefine` 在 QEMU 上返回 false，因为 QEMU 没有 alpha 光标；`Set(1, 1, false)` 不抛出异常），然后是 FIFO 指令流测试，每个测试都捕获 `NextCommand`，逐个双字读回其调用写入的命令，然后回退：`Canvas3D_SceneSetup_Fifo` 开启这一组测试，它重新设置显示设备自身的模式（使 FIFO 回到起点），通过能力接口关闭扫描输出（使主机不消费任何内容），并通过 `CreateCanvas3D` 创建后续测试共用的 SVGA3D 画布，然后固定场景设置（上下文、按画布尺寸设定的颜色和深度目标、两次渲染目标绑定、视口、深度范围、七个渲染状态、无纹理阶段）；随后是 `Canvas3D_MeshUpload_Fifo`（每个流一个缓冲区的 `SURFACE_DEFINE` 和一个来自帧缓冲区区域的 `SURFACE_DMA`，立方体使用表面 3、4 和 5）、`Canvas3D_MeshValidation`、`Canvas3D_DrawCube_Fifo`、`Canvas3D_CameraCaching_Fifo` 和 `Canvas3D_DisposedTexture_Rejected`（`CreateTexture` 定义并上传一个 A8R8G8B8 表面，`Dispose` 销毁它，映射到它的网格在未写入任何内容的情况下被拒绝），最后一个测试恢复扫描输出。`Camera3D_Defaults` 和 `Canvas3D_Discovery`（2 个），每个单元格：在任何 CI 单元格上，`Canvas.GetFullScreen()` 都不是 `Canvas3D`。x64 上各单元格的结果：bare 通过 15 个、跳过 10 个，vmware-svga 通过 24 个、跳过 1 个，virtio-gpu 通过 16 个、跳过 9 个；arm64 运行 bare 和 virtio-gpu。UART 日志按顺序包含每个单元格的启动过程，以及各自的 `[Display] primary` 行。

<a id="audio-tests"></a>

#### Audio 测试

`tests/Kernels/Cosmos.Kernel.Tests.Audio` 在两种架构上的两个配置方案 `bare` 和 `hda` 上运行，其中后者添加 QEMU 的 `intel-hda` 以及启动器挂接在其旁的 `hda-duplex` 编解码器，并关闭默认网卡；套件在每个单元格上注册同样的 22 个测试。关闭网卡是因为工具包只把一条传统中断线分配给一个中断处理程序，并拒绝第二个，而在 q35 上，默认的 e1000e 会占用控制器与之共享的那条中断线。引擎把控制器的声音输出到 QEMU 的 `none` 后端（`-audiodev none`）：CI 运行器没有音频服务器，此时 QEMU 的默认后端无法打开编解码器的输出声道，而 `none` 仍会按流的速率接收帧，因此客户机的流引擎会实时推进。单元格在 `BeforeRun` 中根据设备树判定，即一个 `Description` 以 `class 04.03.00` 结尾的 `pci` 节点，从不根据配置方案名称判定，因此驱动程序绑定失败会使套件失败，而不是让它跳过；设备测试在 bare 上以 `no HD Audio controller on this cell, needs the hda profile` 跳过。套件持有 `Cosmos.Kernel.System` 授予的 `InternalsVisibleTo` 授权，用于统计主输出报告的完成次数（`AudioManager.Completions`）以及读取其契约列出的格式；还持有 `Cosmos.Kernel.Drivers` 授予的一项授权，用于读取驱动程序的 `HdAudioState` 是否连接了功能的中断线；并为 `AudioFormat` 和 `AudioBitDepth` 抑制 `COSMOS0003`，为 `Console.Beep(int, int)` 抑制 `CA1416`，因为 .NET 只在 Windows 上实现了后者。

管理器测试（3 个）：`Feature_Enabled`（`KernelFeatures.Audio` 和 `AudioManager.IsEnabled`）、`Manager_CountMatchesCell`（在 hda 上，索引 0 处有一个输出，即主输出；在 bare 上没有输出，每个索引都为空），以及仅在 bare 上运行的 `Manager_PlayWithoutOutputRefused`（`AudioManager.Play` 和 `TryStartPlayback` 都拒绝，且不返回任何播放）。驱动程序测试（2 个），在 hda 上：`Driver_BoundByHdAudioDriver`（控制器节点由 `HdAudioDriver` 绑定，状态为 `Bound`，该驱动程序发布了一个设备且未记录任何故障）和 `Driver_OutputListedInDiagnostics`（`DriverDiagnostics` 在控制器节点上列出一个 `hda` 设备，由 `HdAudioDriver` 发布，已被消费、未被撤销）。输出测试（7 个），在 hda 上：`Output_IdentityMatchesNode`、`Output_DefaultFormat`（有符号 16 位立体声，48 kHz，空闲，未被占用，且是契约列出的唯一布局）、`Output_TrySetFormat_AcceptsDerivableRates`（44.1、96、22.05、8 和 192 kHz，然后回到 48）、`Output_TrySetFormat_RefusesOtherLayouts`（单声道、无符号 16 位、8 位、24 位和 32 位、链路无法派生的采样率以及零采样率，格式保持不变）、`Output_WritableWhileStopped`、`Output_EngineDrainsRing`（已启动的输出拒绝更改格式，接收完整的帧，直到某次写入返回的数量不足，并随着其引擎读取环形缓冲区而腾出空间）和 `Output_CompletionsFollowLine`（中断线已连接时，运行中的流会触发缓冲区完成中断，驱动程序通过其接收器报告该中断；没有中断线时，流照常运行，但不报告任何内容；UART 日志会说明属于哪种情况：x64 上为 `line connected`，arm64 上为 `no line`，因为工具包在 arm64 上不路由任何 PCI 传统中断线）。播放器测试（5 个），在 hda 上：在调用线程上运行的 `Player_PlaysStreamToEnd` 和 `Player_RefusesUnsupportedFormat`，`Playback_CompletesOnThread`（`AudioManager.TryStartPlayback` 立即占用输出，因此第二个播放会被拒绝，播放以 `Completed` 结束，流被读到末尾，其拥有的对象已被释放）、`Playback_StopEndsEarly`（一个三秒的流在输出开始运行后被停止，以 `Stopped` 结束，流没有被读到末尾）和 `Wave_FromMemoryPlays`（一个在内存中构建的 WAV 文件，在 `data` 之前带有一个长度为奇数的 `LIST` 块，按其声明的布局和长度读取并播放）。`Console.Beep` 测试（5 个）：在两个单元格上运行的 `Beep_RejectsOutOfRange`（36 Hz、32768 Hz、零时长和负时长都会抛出 `ArgumentOutOfRangeException`，与 Windows 上一致），然后是 hda 上的 `Beep_PlaysForItsDuration`（`Console.Beep(440, 300)` 使调用者至少等待其时长的一半，以 `Stopwatch` 计时，在 2 秒上限内返回，并使输出处于已释放且已停止的状态）、`Beep_DefaultPlaysWindowsTone`（`Console.Beep()` 以同样方式播放其 200 毫秒的音调，而不是向终端写入响铃字符）和 `Beep_WhileOutputBusyReturnsAtOnce`（当后台播放占用输出时，一秒的蜂鸣在 500 毫秒内返回，播放继续进行），以及仅在 bare 上运行的 `Beep_WithoutOutputReturnsAtOnce`（两个重载都在 500 毫秒内返回，且不抛出异常）。每次等待都使用 `TimerManager.Wait` 轮询，让调度器在等待期间运行播放线程，并以 `Stopwatch` 上的 2 秒为上限，而不是计算睡眠次数，因此一个测试的所有等待加起来都在引擎的 10 秒停滞窗口之内。每种架构上，bare 报告通过 5 个、跳过 17 个，hda 报告通过 20 个、跳过 2 个，即 44 个中通过 25 个、跳过 19 个。

<a id="storage-tests"></a>

#### Storage 测试

`tests/Kernels/Cosmos.Kernel.Tests.Storage` 在七个配置方案上运行：`ahci`、`nvme`、`usb`、`usb-ehci`、`usb-ohci`、`virtio-blk-pci` 和 `virtio-blk-mmio`（仅限 arm64），每个配置方案挂接一个磁盘（三个 usb 配置方案把同一个 U 盘分别挂在 qemu-xhci、usb-ehci 和 pci-ohci 上，分别以 SuperSpeed、高速和全速连接），在 x64 上与 `acpi-off` 修饰符组合（12 个单元格），在 arm64 上与 `gicv2`、`gicv3` 和 `acpi-off` 组合（42 个单元格），并在每个单元格上注册同样的 83 个测试：3 个管理器测试、1 个启动扫描测试、2 个配置方案测试、1 个驱动程序诊断测试、12 个设备测试、7 个分区测试、42 个分区生命周期测试（MBR 修改、EBR 链、`PartitionManager`、superfloppy）、2 个边界探测、2 个 MMIO/PCI 测试、5 个 USB 热插拔测试、5 个 PCI 热插拔测试和 1 个重启测试。`BeforeRun` 把 `StorageManager.GetDevice(0)` 作为单元格的磁盘，每个需要磁盘的测试在没有磁盘绑定时以 `no block device bound for this profile` 跳过（在两个分区组中为 `no block device bound for partition-table tests`），而这在每个单元格上都意味着 `Manager_ExactlyOneDevice` 失败：在 x64 上，无论有无 ACPI，PCI 都会完成枚举；在 arm64 上，acpi-off 单元格从设备树获取 ECAM 主机，而固件只在这种情况下才提供设备树。ahci 和 nvme 单元格的磁盘由工具包的 `AhciDriver` 和 `NvmeDriver` 发布，usb、usb-ehci 和 usb-ohci 单元格的磁盘由工具包的 `UsbMassStorageDriver` 发布，virtio-blk 单元格的磁盘由工具包的 `VirtioBlkDriver` 发布，存储管理器的消费者在驱动程序阶段注册每一个磁盘。`Profile_DeviceKindMatches` 将磁盘名称与配置方案比对（`sata0`、`nvme0n1`、`usb0`、`vblk0`）；`Manager_DeviceListedInDriverDiagnostics` 在每个配置方案上都能在 `DriverDiagnostics` 中找到该磁盘，它是一个已被消费、未被撤销的块设备，带有单元格预期的 `DriverName`；`Profile_NvmeInterruptModeMatches` 从 `NvmeNamespace` 的 `Controller` 读取 `HasInterrupt`，并在单元格能确定其值的地方加以固定（两个 x64 nvme 单元格上为中断，因为本地 APIC 从 `IA32_APIC_BASE` 启动，即使 acpi-off 不留下 MADT 也能路由 MSI-X；arm64 gicv3 上同样为中断；gicv2 和 arm64 acpi-off 单元格上为轮询，因为那里没有发现 ITS），在其他地方跳过（`not an NVMe profile`、`interrupt mode not pinned by this cell`）。设备组和分区组通过 `IBlockDevice` 和受支持层的分区表操作磁盘；`Boot_PartitionScanMatchesBootState` 验证管理器在启动时、`BeforeRun` 之前执行的扫描（在每个单元格上都发生在发布过程中）；两个 MMIO/PCI 探测在 x64 上、没有命令在途时，通过控制器 `pci` 节点的 `PciAccess`（借助 HAL 授权，按类 01、子类 08 在 `DriverEngine.Nodes` 中找到）把 NVMe 控制器的 BAR 重定位到 4 GiB 以上：`Mmio_HighBar_RemappedOnDemand` 在 `EnsureMmioMapped` 之后通过重定位后的 BAR 读回 VS 寄存器，`Pci_DescribeBar64_ReadsLiveConfig` 将已发布节点的 BAR0 与原始寄存器比对，然后通过 PCI 主机重新描述该功能，并从新的描述中完整读出重定位后的地址；热插拔测试通过引擎的主机请求拔出并重新插入 U 盘，检查磁盘离开管理器、其 `usb:` 节点离开节点树，检查在其上打开的文件以 `IOException` 失败，并检查 U 盘以 `usb0` 的身份带着数据回来：在 xHCI 上位于下一个空闲根端口，在 EHCI 和 OHCI 上位于它离开的那个端口（引擎会固定该端口）；在 virtio-blk-pci 单元格上，`PciHotPlug_*` 测试通过 `pci-unplug` 和 `pci-plug` 把磁盘从其根端口拔出并重新插入，检查磁盘离开管理器、其功能节点和 virtio 节点离开节点树，检查对已移除磁盘的读取以 `IOException` 失败，并检查它以 `vblk0` 的身份、在相同的节点路径上带着数据回来。每个热插拔组的第一个测试断言跟踪该变化的线程正在运行，即主机控制器状态（`XhciState`、`EhciState` 或 `OhciState`，取决于单元格带有哪一种）的热插拔线程，或 `PcieRootPortState` 的插槽线程，通过 HAL 授权读取；一次主机请求之后的所有等待共享 `Stopwatch` 上的同一个 8 秒截止时间：该请求是它们之前的最后一条协议消息，因此一个始终没有出现的变化会在引擎的 10 秒停滞窗口内使其断言失败，而不会拖垮整个单元格；最后的 `Boot_RebootAfterGptWrite` 写入一个 GPT 并重启，因此每个带磁盘的单元格都会启动两次，第二次启动的扫描会找到该分区。在 x64 上，十二个单元格共 996 个测试，通过 886 个、跳过 110 个、失败 0 个；在 arm64 上，四十二个单元格各运行同样的 83 个测试，共 3486 个，每个单元格的结果相同。Fat 和 File 套件操作的是内存中的块设备，从不涉及管理器，因此把存储驱动程序迁入工具包并未改动它们。

<a id="random-tests"></a>

#### Random 测试

`tests/Kernels/Cosmos.Kernel.Tests.Random` 通过 Core 授予该套件的授权，检查 `Cosmos.Kernel.Core/Security` 中的内核 CSPRNG（参见[随机数](../user/random.md)）。已知答案测试（6 个）：`ChaCha20_Block_Rfc8439`（RFC 8439 第 2.3.2 节）、`ChaCha20_Keystream_Rfc8439`（第 2.4.2 节，114 字节的“sunscreen”明文，因此最后一个块不完整）、`Blake2s_Abc_Rfc7693`（附录 B）、`Blake2s_Empty`（`69217a30...1ed0eef9`）、`Blake2s_SelfTest_Rfc7693`（附录 E 的总哈希，覆盖摘要长度 16 到 32、0 到 1024 字节输入的带密钥和不带密钥哈希）和 `FastKeyErasure_NextKeyNeverOutput`（通过 `FastKeyErasure` 发起一次请求：下一个密钥是被弃用密钥的密钥流的前 32 字节，输出是其余部分且从不包含下一个密钥，临时缓冲区被擦除）。熵测试（2 个）：`Hardware_Detection`（CPUID 或 `ID_AA64ISAR0_EL1` 检测会运行，声明支持的指令产生的字既不为零、也不全为一，且不重复）和 `KernelRandom_Seeding`（播种完成，并在声明支持该指令时从中取数）；两者都会记录单元格具备的能力，抖动健康判定会被记录，但不做断言。BCL 测试（8 个）：`Fill_TwoDrawsDiffer`、`Fill_ZeroLength`（BCL 的空请求，以及直接以 0 计数、负计数和 null 缓冲区调用 `KernelRandom.Fill`，这些都是 BCL 从不向下传递的）、`Fill_64KiB_Statistics`（比特平衡在一半的 1% 以内，每个字节值出现 144 到 368 次，没有重复的 8 字节字）、`Create_GetBytes_NonZero`、`GetInt32_Range`、`Guid_NewGuid_Differ`（版本 4）、`Fill_AcrossReseed`（越过按请求数和按字节数的重新播种间隔，每次都必须使 `KernelRandom.ReseedCount` 增加）和 `Fill_ConcurrentThreads`（两个线程从起跑闸门同时释放，抽取 300 毫秒，即多个调度器时间片，同时主线程在定时器上等待：每次抽取前获取的共享序列号必须显示它们的抽取相互交错，生成器必须在此期间重新播种，且它们的任何两次抽取的前 8 字节都不得相同）。x64 单元格在 KVM 的 `host` CPU 或 TCG 的 `max` CPU 下运行，它们具备 RDSEED 和 RDRAND；arm64 的 `cortex-a72` 没有 FEAT_RNG，因此该单元格走的是仅依赖抖动的路径，其日志中包含仅用抖动的警告。

<a id="iso9660-tests"></a>

#### Iso9660 测试

`tests/Kernels/Cosmos.Kernel.Tests.Iso9660` 在两种架构上检查 `Cosmos.Kernel.System/FileSystem/Iso9660` 中的 ISO 9660 驱动程序，不使用配置文件。它的镜像不提交到仓库：项目文件在编译时对 `Fixtures/` 下的目录树运行 xorriso（每个 Cosmos ISO 都用它构建，由 `Cosmos.Build.Common` 的 `GetXorriso` 目标解析），并固定所有者、权限模式和全部时间戳（2026-10-28 12:00:00 UTC），使生成的字节不依赖于检出、时钟或时区，然后把两个镜像嵌入内核，内核通过 Core 的 `ResourceManager` 读取它们（授权在 `Cosmos.Kernel.Core.csproj` 中）。`Fixtures/rockridge/` 以 Rock Ridge 映射：一个 140 字符的文件名，其 `NM` 条目溢出到 `CE` continuation 中；一个超过深度 8、被 xorriso 重定位到 `rr_moved` 的目录（`CL`、`RE`、`PL`）；四十个条目，把一个目录铺到四个逻辑块上；一个以部分块结尾的 10 000 字节文件；一个由构建生成的 200 KiB 模式文件；一个空文件；指向文件、目录和不存在目标的符号链接；以及一个空目录。`Fixtures/level1/` 只以 level 1 标识符映射，并带有驱动程序必须跳过的 Joliet 描述符。Rock Ridge 镜像挂载两次：一次来自 2048 字节扇区的设备（CD），一次来自 512 字节扇区的设备（写入了镜像的 U 盘）。

测试单元：挂载（`Test_Mount_*`、`Test_StatFs`、`Test_FormatAndDestroy_Refused`）、列目录（`Test_RootListing_*`、`Test_Listing_*`）、查找（`Test_Lookup_*`：Rock Ridge 上精确匹配，level 1 上不区分大小写，长文件名，重定位目录，`.` 和 `..`）、读取（`Test_Read_*`：整体读取、按 4 097 字节分块读取以便一次读取的头、体、尾都得到执行、seek 后读取、逐行读取，并对 200 KiB 文件的设备读取次数设上限）、属性（`Test_Attributes_*`：只读、固定的时间戳、共享同一 extent 的条目拥有不同的 inode 号）、符号链接（`Test_Symlink_*`）、拒绝（`Test_ReadOnly_*`：每次修改经由 `System.IO` 都是普通的 `IOException`，经由 `VfsManager` 都是 `false`），以及 `Test_Unmount_And_Remount` 和 `Test_TwoMounts_SameDevice`。

<a id="running-kernel-tests"></a>

### 运行内核测试

<a id="from-vs-code"></a>

#### 从 VS Code 运行

**使用任务（推荐）：**
1. 按 `Ctrl+Shift+P` → “Tasks: Run Task”
2. 选择以下任务之一：
   - **Run Test: HelloWorld (x64)**：控制台 + XML 输出
   - **Run Test: HelloWorld (x64, Console Only)**：仅控制台输出
   - **Run Test: HelloWorld (ARM64)**：带 XML 输出的 ARM64 测试
   - **Dev Test: HelloWorld (x64)**：带详细输出的开发者模式

**调试测试运行器：**
1. 打开“运行和调试”面板（`Ctrl+Shift+D`）
2. 选择一个配置：
   - **Debug Test Runner (HelloWorld x64)**
   - **Debug Test Runner (HelloWorld ARM64)**
3. 按 `F5`

<a id="from-the-command-line"></a>

#### 从命令行运行

```bash
# Run test with XML output
dotnet run --project tests/Cosmos.TestRunner.Engine/Cosmos.TestRunner.Engine.csproj -- \
  tests/Kernels/Cosmos.Kernel.Tests.HelloWorld \
  x64 \
  60 \
  test-results.xml

# Run test with console output only
dotnet run --project tests/Cosmos.TestRunner.Engine/Cosmos.TestRunner.Engine.csproj -- \
  tests/Kernels/Cosmos.Kernel.Tests.HelloWorld \
  x64 \
  60
```

**参数：**
1. 内核项目路径（绝对路径或相对路径）
2. 架构：`x64` 或 `arm64`
3. 超时时间（秒）
4. *（可选）* XML 输出路径（JUnit 格式）
5. *（可选）* 模式：`ci` 或 `dev`

**推荐的超时时间：**

| 套件 | x64 | ARM64 |
|-------|-----|-------|
| HelloWorld | 60 s | 90 s |
| Memory | 180 s | 300 s |
| Drivers | 60 s | 120 s |
| Pci | 60 s | 90 s |
| Graphic | 60 s | 90 s |
| Storage | 90 s | 180 s |

<a id="output-formats"></a>

### 输出格式

<a id="console-colored"></a>

#### 控制台（彩色）

```
================================================================================
Starting test suite: HelloWorld Basic Tests
Architecture: x64
Time: 2025-11-05 04:06:48
================================================================================
[1] Test_BasicArithmetic: PASSED (15ms)
[2] Test_BooleanLogic: PASSED (12ms)
[3] Test_IntegerComparison: PASSED (10ms)
================================================================================
Suite: HelloWorld Basic Tests
Total tests: 3 | Passed: 3 | Failed: 0 | Skipped: 0 | Duration: 0.04s
================================================================================
ALL TESTS PASSED
================================================================================
```

<a id="xml-junit-format"></a>

#### XML（JUnit 格式）

```xml
<?xml version="1.0" encoding="utf-16"?>
<testsuites name="HelloWorld Basic Tests" tests="3" failures="0" skipped="0" time="0.037">
  <testsuite name="HelloWorld Basic Tests" tests="3" failures="0" skipped="0" time="0.037">
    <properties>
      <property name="architecture" value="x64" />
    </properties>
    <testcase name="Test_BasicArithmetic" classname="HelloWorld Basic Tests" time="0.015" />
    <testcase name="Test_BooleanLogic" classname="HelloWorld Basic Tests" time="0.012" />
    <testcase name="Test_IntegerComparison" classname="HelloWorld Basic Tests" time="0.010" />
  </testsuite>
</testsuites>
```

<a id="exit-codes"></a>

### 退出码

| 代码 | 含义 |
|------|---------|
| 0 | 所有测试均通过（跳过的测试可以接受） |
| 1 | 测试失败或执行出错 |
| 137 | 超时（SIGKILL） |

---

<a id="uart-debug-protocol"></a>

## UART 调试协议

测试内核通过嵌入在 QEMU 串口（UART）数据流中的二进制协议，把结果传回主机端引擎。该协议定义在 `tests/Cosmos.TestRunner.Protocol/` 中，移植自 CosmosOS 的调试连接器。

<a id="framing"></a>

### 帧格式

每条消息都以 4 字节的魔数签名开头，后跟命令字节和 2 字节小端序的负载长度：

```
[Magic: 4 bytes][Command: 1 byte][Length: 2 bytes LE][Payload: N bytes]
```

- **Magic**：`0x07 0x08 0x74 0x19`（即小端序下的 `0x19740807`，`Consts.cs` 中的 `SerialSignature`）
- **Command**：`Ds2Vs` 常量之一（见下表）
- **Length**：其后负载的字节数

在最后一条 `TestSuiteEnd` 消息之后，内核还会发送一个 8 字节的终止标识（`0xDE 0xAD 0xBE 0xEF 0xCA 0xFE 0xBA 0xBE`），使引擎能够立即终止 QEMU，而无需等到整个超时时间结束。

<a id="commands-kernel--host-ds2vs"></a>

### 命令（内核 → 主机，`Ds2Vs`）

测试运行器专用的命令占用 **100-109** 范围。原有的 CosmosOS 调试命令（0-25）也有定义，但测试运行器不使用它们。

| 命令 | 值 | 负载格式 | 说明 |
|---------|-------|----------------|-------------|
| `TestSuiteStart` | 100 | `[ExpectedTests: 2 LE][SuiteName: UTF-8]` | 测试套件开始时发送一次 |
| `TestStart` | 101 | `[TestNumber: 2 LE][TestName: UTF-8]` | 每个测试执行前发送 |
| `TestPass` | 102 | `[TestNumber: 2 LE][DurationMs: 4 LE]` | 测试通过时发送 |
| `TestFail` | 103 | `[TestNumber: 2 LE][ErrorMessage: UTF-8]` | 断言失败时发送 |
| `TestSkip` | 104 | `[TestNumber: 2 LE][SkipReason: UTF-8]` | 测试被显式跳过时发送 |
| `TestSuiteEnd` | 105 | `[Total: 2 LE][Passed: 2 LE][Failed: 2 LE]` | 测试套件结束时发送一次 |
| `ArchitectureInfo` | 106 | `[ArchId: 1][CpuCount: 1]` | 内核启动时发送（架构 ID：1=x86，2=x64，3=ARM32，4=ARM64） |
| `CoverageData` | 107 | `[HitCount: 2 LE][MethodId: 2 LE]...` | 启用覆盖率构建的内核在套件结束后发送 |
| `TestDestructiveReached` | 108 | `[TestNumber: 2 LE]` | 由 `TR.RunDestructive` 在执行一个不会返回的操作（重启、关机）之前发送 |
| `HostRequest` | 109 | `[Request: ASCII]` | 请求引擎在运行中的客户机之下更改机器（见下文） |

<a id="message-flow"></a>

### 消息流程

典型的会话如下：

```
→ TestSuiteStart  (suiteName="HelloWorld Basic Tests", expectedTests=3)
→ TestStart       (testNumber=1, testName="Test_BasicArithmetic")
→ TestPass        (testNumber=1, durationMs=15)
→ TestStart       (testNumber=2, testName="Test_BooleanLogic")
→ TestPass        (testNumber=2, durationMs=12)
→ TestStart       (testNumber=3, testName="Test_IntegerComparison")
→ TestPass        (testNumber=3, durationMs=10)
→ TestSuiteEnd    (total=3, passed=3, failed=0)
→ [0xDE 0xAD 0xBE 0xEF 0xCA 0xFE 0xBA 0xBE]  ← termination marker
```

<a id="parser"></a>

### 解析器

`tests/Cosmos.TestRunner.Engine/Protocol/UartMessageParser.cs` 读取 QEMU 捕获的原始 UART 日志（`uart-output.log`），逐字节扫描魔数签名，验证命令字节和长度，然后分派给相应的解析辅助函数。

损坏检测：通过检查 `total == passed + failed` 来验证 `TestSuiteEnd` 的负载。如果该不变式不成立（例如定时器中断穿插导致 UART 字节损坏），则忽略结束消息，结果回退为逐个跟踪的计数器。

<a id="host-requests"></a>

### 主机请求

当测试需要在其运行期间更改机器，或需要主机执行某个操作时，会调用 `TR.RequestHost(request)`；帧一出现在 UART 上，引擎就会执行该请求：针对机器的请求通过 QEMU 的 QMP 监视器执行，`tcp-connect` 则通过引擎自己的 TCP 测试服务器执行：

| 请求 | 效果 |
|---------|--------|
| `usb-unplug [n]` | 从配置方案指定的 USB 主机控制器上拔下 U 盘 `n`（默认为 0） |
| `usb-plug [n]` | 将其重新插入，使用相同的磁盘镜像和相同的总线，并在控制器固定端口的情况下（EHCI 和 OHCI 型号）插回它离开的那个根端口 |
| `pci-unplug [n]` | 从 QEMU 中删除可热插拔的 PCI 磁盘 `n`（默认为 0），这会按下插槽的注意按钮，并在客户机为插槽断电时完成移除 |
| `pci-plug [n]` | 将其重新添加到同一根端口之后，使用相同的磁盘镜像；客户机为插槽上电并放置该功能的寄存器 |
| `usb-kbd-unplug` | 从配置方案指定的 USB 主机控制器上拔下配置方案的 USB 键盘 |
| `usb-kbd-plug` | 将其重新插入，使用相同的总线，在控制器固定端口的情况下插回它离开的端口（配置方案要求集线器时则位于集线器之后），并使用相同的 `usb_version` |
| `key-press <qcode>` | 按下并释放一个由 QEMU QKeyCode 名称（`a`、`ret`、`spc`……）指定的键；QEMU 在默认的 100 毫秒按住时间后释放它 |
| `mouse-move <dx> <dy>` | 以设备单位将指针移动 `dx`、`dy`，作为一个带两个相对事件的 `input-send-event` 发送 |
| `mouse-button <left or middle or right> <down or up>` | 按下或释放一个指针按键 |
| `tcp-connect` | 将引擎的 TCP 测试服务器连接到内核在端口 5558 上的 TCP 服务器，并执行 `TEST_FROM_HOST` 回显交换；Network 套件的 `TCP_ServerAccept` 在 `listener.Start()` 之后立即发送该请求，使 SYN 在监听器已启动时发出（端口 5558 是一个 `hostfwd`，slirp 在主机端立即接受连接，并按照自己的重传节奏交付给客户机，先隔 6 秒，再隔 12 秒：按时钟发起的连接会错过启动较晚的监听器，#3361） |

没有任何回复。测试自行等待变化发生，并且必须在 10 秒内看到它：超过这个时间没有协议消息，引擎就会认为内核已卡死。键盘请求不带索引：一个配置方案最多有一个 `usb-kbd` 键盘；PCI 请求按索引指向配置方案中的可热插拔磁盘（`virtio-blk` 磁盘上的 `"hotplug": true`）。每个单元格都带监视器启动 QEMU；运行中无法执行的请求会被报告并丢弃（`[Monitor] usb-kbd-unplug failed: the profile attaches no USB keyboard`），客户机中的测试会因超时而失败。Storage 套件的 `UsbHotPlug_*` 和 `PciHotPlug_*` 测试，以及 Drivers 套件的 `Usb_Keyboard_*`、`Usb_Mouse_*`、`Ps2_*` 和 `Pci_HotPlug_*` 测试就是示例。

<a id="host--kernel-commands-vs2ds"></a>

### 主机 → 内核命令（`Vs2Ds`）

测试运行器目前不向内核发送命令。`Vs2Ds` 类（`Noop=0`、`Continue=4`、`Ping=17`）继承自 CosmosOS 调试连接器，保留供将来使用。

---

<a id="project-structure"></a>

## 项目结构

```
tests/
├── Cosmos.TestRunner.Engine/        # Host-side test runner
│   ├── Engine.cs                    # Main orchestration
│   ├── Engine.Build.cs              # NativeAOT build pipeline
│   ├── Program.cs                   # CLI entry point
│   ├── TestConfiguration.cs         # Configuration
│   ├── TestResults.cs               # Result model
│   ├── Hosts/                       # QEMU host implementations
│   │   ├── IQemuHost.cs
│   │   ├── QemuMonitor.cs           # The QMP monitor: USB and PCI hot-plug, key and pointer injection
│   │   ├── QemuX64Host.cs
│   │   └── QemuARM64Host.cs
│   ├── OutputHandlers/              # Result output formats
│   │   ├── OutputHandlerBase.cs
│   │   ├── OutputHandlerConsole.cs  # Colored terminal output
│   │   ├── OutputHandlerXml.cs      # JUnit XML output
│   │   └── MultiplexingOutputHandler.cs
│   └── Protocol/
│       └── UartMessageParser.cs     # Binary message parser
├── Cosmos.TestRunner.Framework/     # In-kernel test framework
│   ├── TestRunner.cs                # Start / Run / Skip / Finish
│   └── Assert.cs                    # Assertion helpers
├── Cosmos.TestRunner.Protocol/      # Shared protocol definitions
│   ├── Consts.cs                    # Magic signature and constants
│   └── Messages.cs                  # Typed message classes
├── Cosmos.Kernel.Tests.System/      # Unit tests: Cosmos.Kernel.System (NUnit)
├── Cosmos.Tests.Build.Asm/          # Unit tests: Clang assembly build task
├── Cosmos.Tests.Build.Analyzer.Patcher/ # Unit tests: plug analyzer
├── Cosmos.Tests.Scanner/            # Unit tests: plug scanner
├── Cosmos.Tests.Patcher/            # Unit tests: IL patcher
├── Cosmos.Tests.SourceGenerators/   # Unit tests: entry point and driver manifest generator
├── Cosmos.Tests.NativeWrapper/      # Runtime assets (no tests)
├── Cosmos.Tests.NativeLibrary/      # Native code for tests (no tests)
└── Kernels/                         # Kernel test projects
    ├── Cosmos.Kernel.Tests.HelloWorld/
    │   ├── Kernel.cs
    │   └── Bootloader/limine.conf
    └── Cosmos.Kernel.Tests.Memory/
        ├── Kernel.cs
        └── Bootloader/limine.conf
```

`tests/Cosmos.Kernel.Tests.System/` 存放 `Cosmos.Kernel.System` 的主机端测试。它以项目引用的方式引用该库，并持有该库授予的 `InternalsVisibleTo` 授权，其文件夹结构与库的结构对应（`Network/`、`Network/Protocols/`、`Input/`、`FileSystem/Ext2/`）。

---

<a id="writing-a-test-kernel"></a>

## 编写测试内核

<a id="minimal-example"></a>

### 最小示例

测试内核继承自 `Cosmos.Kernel.System.Kernel`，并在 `BeforeRun()` 中运行所有测试。使用 `TR` 作为 `TestRunner` 的别名，使用 `Assert` 进行断言。

```csharp
using Cosmos.Kernel.Core.IO;
using Cosmos.TestRunner.Framework;
using Sys = Cosmos.Kernel.System;
using TR = Cosmos.TestRunner.Framework.TestRunner;

namespace Cosmos.Kernel.Tests.MyTests;

public class Kernel : Sys.Kernel
{
    protected override void BeforeRun()
    {
        // Initialize test suite (expectedTests must equal the total number of TR.Run + TR.Skip calls)
        TR.Start("My Test Suite", expectedTests: 3);

        TR.Run("Test_Addition", () =>
        {
            int result = 2 + 2;
            Assert.Equal(4, result);
        });

        TR.Run("Test_StringOps", () =>
        {
            string str = "Hello";
            Assert.Equal("Hello", str);
            Assert.NotNull(str);
        });

        // Mark unsupported operations as skipped rather than letting them crash
        // TR.Skip also counts toward expectedTests
        TR.Skip("Test_Unsupported", "Feature not implemented");

        TR.Finish();

        Serial.WriteString("[Tests Complete - System Halting]\n");
        Stop();
    }

    protected override void Run()
    {
        // Tests completed in BeforeRun, nothing to do here
    }

    protected override void AfterRun()
    {
        Cosmos.Kernel.Kernel.Halt();
    }
}
```

<a id="kernel-lifecycle"></a>

### 内核生命周期

`Sys.Kernel` 基类驱动一个固定的生命周期：

1. `OnBoot()`：系统初始化（自动调用，很少重写）
2. `BeforeRun()`：**在此运行所有测试**，然后调用 `Stop()`
3. `Run()`：循环调用，直到调用 `Stop()`；测试内核中保持为空
4. `AfterRun()`：循环退出后调用一次；在此调用 `Cosmos.Kernel.Kernel.Halt()`

<a id="available-assertions"></a>

### 可用断言

```csharp
// Equality: typed overloads (int, uint, long, byte, bool, string, byte[], int[])
Assert.Equal(expected, actual);
Assert.Equal<T>(expected, actual);   // Generic overload (requires IEquatable<T>)

// Null checks
Assert.Null(obj);
Assert.NotNull(obj);

// Boolean
Assert.True(condition);
Assert.True(condition, "message");
Assert.False(condition);

// Manual failure
Assert.Fail("Custom error message");
```

> `Assert` 使用静态失败状态（不使用异常），以兼容 NativeAOT。
> 每个测试只记录第一次失败；同一 `TR.Run` 块中的后续断言仍会被求值。

<a id="test-status"></a>

### 测试状态

| 状态 | 时机 |
|--------|------|
| **通过** | 测试完成，没有断言失败 |
| **失败** | 断言在 `TR.Run` 内设置了失败状态 |
| **跳过** | 测试通过 `TR.Skip(name, reason)` 被显式标记 |

<a id="adding-a-new-test-suite"></a>

### 添加新的测试套件

1. 在 `tests/Kernels/Cosmos.Kernel.Tests.{Name}/` 下创建一个内核项目
2. 从现有套件复制 `.csproj` 和 `Bootloader/limine.conf`；更新 `limine.conf` 中的 ELF 路径
3. 使用 `TestRunner.Framework` 实现测试
4. 在 `.github/workflows/kernel-tests.yml` 中添加一个 CI 作业：
   - 复制一个现有的 `*-tests` 作业，重命名它，并更新内核路径
   - 添加一个对应的 `{name}-results` 作业，由它生成 PR 评论
   - 将新作业加入 `test-summary` 的依赖项
5. 在 `.vscode/tasks.json` 中添加 VS Code 任务

---

<a id="ci-integration"></a>

## CI 集成

CI 工作流（`.github/workflows/kernel-tests.yml`）在 x64 和 ARM64 上运行内核集成测试。

**作业：**
- `helloworld-tests`：x64/arm64 的矩阵构建
- `helloworld-results`：将合并后的 PR 评论生成到 `pr-comment-helloworld` 产物中
- `memory-tests`：x64/arm64 的矩阵构建
- `memory-results`：将合并后的 PR 评论生成到 `pr-comment-memory` 产物中
- 每个套件都有这样一对作业，直到 `fat-tests`/`fat-results`、`file-tests`/`file-results` 和 `iso9660-tests`/`iso9660-results`
- `test-summary`：最终状态汇总

**触发条件：**
- 推送到 `main`
- 拉取请求（任意分支）
- 手动触发，可选择架构

**PR 评论**：每个测试套件对应一条评论，x64 和 arm64 各占一行，显示测试数量、耗时和产物链接。`*-results` 作业只负责生成评论：来自派生仓库（fork）的拉取请求所触发的 `pull_request` 运行只持有只读令牌，无法发布评论。`.github/workflows/kernel-test-comment.yml` 在 Kernel Tests 或 Kernel Coverage 完成后通过 `workflow_run` 运行，它在基础仓库中运行，无论拉取请求来自何处都具有写权限，会下载 `pr-comment-*` 产物，并发布或更新评论。只有当某个拉取请求的头部就是该次运行的头部提交时，它才信任产物中的 PR 编号；而且 GitHub 从默认分支运行它，因此对它的修改要在合并后才会生效。

**产物（保留 30 天）：**
- `test-results-{suite}-{arch}.xml`：JUnit XML 结果
- `uart-log-{suite}-{arch}`：完整的 UART 输出
- `{Suite}-Test-ISO-{arch}`：可启动的内核 ISO + ELF

<a id="example-ci-step"></a>

### CI 步骤示例

```yaml
- name: Run Cosmos Tests
  run: |
    dotnet run --project tests/Cosmos.TestRunner.Engine/Cosmos.TestRunner.Engine.csproj -- \
      tests/Kernels/Cosmos.Kernel.Tests.HelloWorld \
      x64 \
      120 \
      test-results.xml \
      ci

- name: Publish Test Results
  uses: dorny/test-reporter@v1
  if: always()
  with:
    name: Cosmos Tests
    path: test-results.xml
    reporter: java-junit
```

---

<a id="performance-reference"></a>

## 性能参考

| 阶段 | x64 | ARM64 |
|-------|-----|-------|
| 内核构建 | ~60 s | ~70 s |
| HelloWorld 执行 | 2-5 s | 5-10 s |
| Memory 执行 | 60-120 s | 120-240 s |

---

<a id="troubleshooting"></a>

## 故障排除

<a id="timeout"></a>

### 超时
- 增大超时参数
- 检查 `uart-output.log` 中的启动问题
- 确认已安装 QEMU：`qemu-system-x86_64 --version`

<a id="build-failures"></a>

### 构建失败
- 运行 `.devcontainer/postCreateCommand.sh` 重新构建框架
- 还原 NuGet 包：`dotnet restore`
- 确认已安装 .NET 9 SDK

<a id="arm64-issues"></a>

### ARM64 问题
- 确保 UEFI 固件存在：`~/.cosmos/tools/qemu/share/qemu/edk2-aarch64-code.fd`
- 使用更长的超时时间（HelloWorld 90 秒以上，Memory 180 秒以上）

<a id="ud-exceptions-invalid-opcode"></a>

### #UD 异常（无效操作码）
某些操作可能会根据运行时状态触发无效操作码故障。请使用 `TR.Skip()` 标记它们，而不是任由内核崩溃。
