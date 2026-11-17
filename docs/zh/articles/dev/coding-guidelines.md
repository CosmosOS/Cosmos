---
source_hash: 175a9522f3bda585b51358557f4c94979cf5e5cf
---
<a id="coding-guidelines"></a>

# 编码规范

本文档确立了 Cosmos Gen3 的编码风格与架构模式。这些规范确保所有贡献者共同维护一个整洁、现代且一致的代码库。

<a id="table-of-contents"></a>

## 目录

- [1. 项目架构](#1-project-architecture)
- [2. 命名约定](#2-naming-conventions)
- [3. 文件组织](#3-file-organization)
- [4. 类与类型设计](#4-class--type-design)
- [5. 内核生命周期](#5-kernel-lifecycle)
- [6. HAL 架构](#6-hal-architecture)
- [7. Plug 系统](#7-plug-system)
- [8. 原生互操作](#8-native-interop)
- [9. 架构相关代码](#9-architecture-specific-code)
- [10. 内存与安全](#10-memory--safety)
- [11. 错误处理](#11-error-handling)
- [12. 可空引用类型](#12-nullable-reference-types)
- [13. 现代 C# / .NET 10 惯用法](#13-modern-c--net-10-idioms)
- [14. AOT 约束](#14-aot-constraints)
- [15. 文档](#15-documentation)
- [16. 测试](#16-testing)
- [17. 公共 API 面](#17-public-api-surface)

---

<a id="1-project-architecture"></a>

## 1. 项目架构

<a id="layer-dependency-rules"></a>

### 层依赖规则

项目划分为严格的层次。依赖关系**只能向下**。这些规则由 `Cosmos.Build.Analyzer.Patcher` 中的 Roslyn 分析器 `LayerAnalyzer` **在编译时强制执行**：它依据项目代码所引用的类型和成员来判定，而不是依据还原（restore）生成的引用列表，并且对每个跨越边界使用的程序集只在其首次使用处报告一次。用户内核以及驱动程序程序集（`<CosmosDriverAssembly>true</CosmosDriverAssembly>`，参见[公共 API 跟踪](public-api.md)）还可以引用 `Cosmos.Kernel.HAL` 提供的内容：驱动工具包扩展点和设备契约（`IBlockDevice`、`MacAddress`）。

```
User Kernel (DevKernel, test kernels)
Cosmos.Kernel.Drivers (the shipped drivers, a driver assembly held to the User layer)
    └── Cosmos.Kernel.System        ← high-level OS APIs (Console, Graphics, Network)
         └── Cosmos.Kernel.HAL      ← hardware abstraction (shared logic, boot and device contracts)
              ├── Cosmos.Kernel.HAL.X64        ← x64 platform code (machine description, line routing, PIT, RTC)
              ├── Cosmos.Kernel.HAL.ARM64      ← ARM64 platform code (machine description, line routing, generic timer, RTC)
              └── Cosmos.Kernel.Core   ← low-level runtime (memory, scheduler, serial)
                   ├── Cosmos.Kernel.Native.X64       ← x64 assembly (.s)
                   ├── Cosmos.Kernel.Native.ARM64     ← ARM64 assembly (.s)
                   └── Cosmos.Kernel.Native.MultiArch ← cross-platform native C code (ACPI, libc stubs)
```

完整的依赖关系图、项目说明和规则，请参见[内核项目结构](kernel-project-layout.md)。

<a id="when-to-create-a-new-project"></a>

### 何时创建新项目

- 工具包的总线能够到达的新硬件（一个 PCI 功能、一个 virtio 设备、一个 USB 接口、一个 PS/2 端口、一个平台节点）→ 在 `Cosmos.Kernel.Drivers` 中编写一个 `[Driver]` 类，基于公共扩展点实现（[编写驱动程序](../user/drivers.md)）；聚合项目会携带这个包，因此每个内核都会获得它。新的总线种类 → 在工具包中添加一个标识、一个匹配、一个访问对象和一个路径。跨平台 HAL 代码放在 `Cosmos.Kernel.HAL` 中，平台代码放在 `Cosmos.Kernel.HAL.X64`/`Cosmos.Kernel.HAL.ARM64` 中。
- 新的操作系统级功能、对外暴露的用户 API → 放在 `Cosmos.Kernel.System` 中。
- 新的底层运行时关注点 → 放在 `Cosmos.Kernel.Core` 中。

---

<a id="2-naming-conventions"></a>

## 2. 命名约定

部分核心命名规则由 `.editorconfig` 强制执行；下表记录了完整的命名规范：

| 元素 | 约定 | 示例 |
|---------|-----------|---------|
| 命名空间 | `Cosmos.Kernel.{Layer}.{Category}`（项目名，然后是其文件夹） | `Cosmos.Kernel.Core.Scheduler` |
| 公共类/结构体 | PascalCase | `GarbageCollector`, `Thread` |
| 接口 | `I` + PascalCase | `IScheduler`, `IPlatformInitializer` |
| 公共方法/属性 | PascalCase | `InitializeStack()`, `StackPointer` |
| 私有/内部字段 | `_camelCase` | `_currentScheduler`, `_kernel` |
| 私有/内部静态字段 | `s_camelCase` | `s_cursorPattern`, `s_nextThreadId` |
| 常量 | PascalCase | `DefaultStackSize`, `MaxThreadCount` |
| 私有常量 | PascalCase | `COM1_BASE`（硬件寄存器使用 UPPER_SNAKE 以提高可读性） |
| 局部变量 | camelCase | `stackTop`, `entryPoint` |
| 参数 | camelCase | `cpuState`, `threadId` |
| 类型参数 | `T` + PascalCase | `TValue`, `TKey` |
| 枚举成员 | PascalCase | `ThreadState.Running` |

在 `Cosmos.Kernel.System.Diagnostics` 中，后缀表明类型的性质：`*Diagnostics` 类型是某个子系统的静态诊断视图（`DriverDiagnostics`、`SchedulerDiagnostics`、`MemoryDiagnostics`），它主要用于读取，但也可以执行操作（`SchedulerDiagnostics.RequestKill`、`MemoryDiagnostics.Collect`）；`*Info` 类型则是这种视图交出的只读快照结构体（`DriverInfo`、`DeviceNodeInfo`、`KernelThreadInfo`）。参见[公共 API 跟踪](public-api.md#naming)。

<a id="avoid"></a>

### 避免

- 匈牙利命名法（`m_`、`p_`、`g_`），`_` 前缀只用于私有字段。
- 旧式的 `mStopped` 写法，请使用 `_stopped`。
- 缩写，除非是公认的缩写（`GC`、`CPU`、`HAL`、`IO`、`IP`、`MAC`）。

---

<a id="3-file-organization"></a>

## 3. 文件组织

<a id="one-type-per-file"></a>

### 每个文件一个类型

每个公共类型都有自己的文件，以该类型命名：

```
Cosmos.Kernel.Core/
  Scheduler/
    SchedulerThread.cs
    SchedulerThreadState.cs
    IScheduler.cs
    SchedulerManager.cs
    PerCpuState.cs
```

有两类伴随类型与主类型共用一个文件，文件仍以主类型命名：一是某个类型自身紧密耦合的伴随类型，例如 `Gpt` 的 `GptPartitionEntry`，`TcpPacket` 的 `TcpFlags` 和 `TcpOption`；二是子类家族，例如 `IcmpPacket` 的 `IcmpEchoRequest` 和 `IcmpEchoReply`。实现类不是其接口的伴随类型：有 24 个文件声明了顶层接口，每个文件都只声明该接口，别无其他。

<a id="partial-classes-for-large-types"></a>

### 大型类型使用分部类

使用 `partial` 将大型类拆分到多个文件中。文件命名为 `{ClassName}.{Aspect}.cs`：

```
Memory/
  GarbageCollector.cs              ← core structure, fields, initialization
  GarbageCollector.Alloc.cs        ← allocation methods
  GarbageCollector.Mark.cs         ← mark phase
  GarbageCollector.Sweep.cs        ← sweep phase
  GarbageCollector.GCHandler.cs    ← GC handle table
  GarbageCollector.Frozen.cs       ← frozen segment support
```

<a id="architecture-specific-files"></a>

### 架构相关文件

对于在同一项目内需要按架构分别实现的类型，使用后缀：

```
Scheduler/
  ThreadContext.X64.cs             ← x64-specific context layout
  ThreadContext.ARM64.cs           ← ARM64-specific context layout
```

这些文件通过 `.csproj` 进行条件编译：

```xml
<ItemGroup Condition="'$(CosmosArch)' != 'arm64'">
    <Compile Remove="Scheduler\ThreadContext.ARM64.cs" />
</ItemGroup>
```

目前只允许在 `Cosmos.Kernel.Core` 中这样做，但将来可能会改变。

<a id="namespaces-follow-folders"></a>

### 命名空间与文件夹一致

文件的命名空间是其项目名称加上项目与文件之间的各级文件夹：`Cosmos.Kernel.Core/Scheduler/SchedulerThread.cs` 声明的是 `Cosmos.Kernel.Core.Scheduler`。构建通过错误级别的 IDE0130 对所有 `src/Cosmos.Kernel*` 项目、测试内核、`tests/Cosmos.Kernel.Tests.System` 中的主机端单元测试以及 `examples/DevKernel` 强制执行这一点，因此偏离规则的文件会让 `dotnet build` 失败（格式检查 CI 检查的是 `nativeaot-patcher.slnx` 中的项目，其中不包含测试内核）。请移动文件或修正命名空间，而不是添加豁免。各项豁免及其理由列在 `.editorconfig` 的末尾：ILC 按完整名称匹配的名称（每个 `LibraryInitializer`、`EagerStaticClassConstructionAttribute`、`Core/Runtime/Stdllib.cs`），源代码生成器的 `IsExternalInit`（C# 编译器的 init 访问器标识，必须位于 `System.Runtime.CompilerServices` 中），`Bridge/Import` 和 `Bridge/Export` 文件夹（其中的文件共用项目的 `Bridge` 命名空间），`Cosmos.Kernel.System/ThirdParty/` 下引入的第三方代码，以及 `Cosmos.Kernel.Core/Runtime/Internal/` 下的 dotnet/runtime 镜像代码。IDE0130 不检查包含跨文件分部类型的命名空间（每个 `[LibraryImport]` 类都属于这种情况），也不检查嵌套命名空间；这些命名空间同样遵循该规则，只是不受检查。

<a id="driver-folders"></a>

### 驱动程序文件夹

`Cosmos.Kernel.Drivers` 将每个驱动程序放在三层深的位置，依次是总线种类、类别和驱动程序本身，每个文件的命名空间都与其文件夹一致：

```
Cosmos.Kernel.Drivers/
  Pci/                       ← the bus kind the driver matches on (PciMatch)
    Bus/                     ← a bus driver: its binding publishes child nodes
      Xhci/
        XhciDriver.cs        ← Cosmos.Kernel.Drivers.Pci.Bus.Xhci
    Storage/                 ← otherwise, what it publishes (a block device)
      Nvme/
        NvmeDriver.cs
  Virtio/
    Display/
      VirtioGpu/
        VirtioGpuDriver.cs   ← Cosmos.Kernel.Drivers.Virtio.Display.VirtioGpu
```

总线种类是驱动程序进行匹配所依据的总线，以其匹配类型命名：`Pci`、`Platform`、`Ps2`、`Usb` 或 `Virtio`。总线驱动程序的类别为 `Bus`，其他驱动程序的类别则表示它发布的内容：`Audio`、`Display`、`Input`（键盘或指针）、`Network` 或 `Storage`（块设备）。驱动程序文件夹的名称是去掉 `Driver` 的类名，除非文件夹内已有类型使用了这个名称，因为命名空间不得与其中的某个类型同名：传输驱动程序位于 `VirtioPci` 和 `VirtioMmio` 中，旁边是它们的 `VirtioPciTransport` 和 `VirtioMmioTransport` 类。驱动程序的状态、协议和词汇类型都留在它的文件夹中，数量多时放在子文件夹里（`VmwareSvga/Enums`、`VmwareSvga/Structs`）。命名空间是完整类型名的一部分，而 `CosmosDriverExclude` 引用的正是完整类型名，清单也按它排序（[驱动程序清单](build/driver-manifest.md)），因此移动驱动程序会同时影响这两者。

<a id="hal-folders"></a>

### HAL 文件夹

`Cosmos.Kernel.HAL` 为每个概念提供一个顶层文件夹，整个目录树中没有重复的文件夹名：

```
Cosmos.Kernel.HAL/
  Experimentals.cs           ← the seam's diagnostic ID (COSMOS0003)
  Boot/                      ← internal: IPlatformInitializer, PlatformHAL
  Timers/                    ← internal: TickSource, TimerEntry
  Internal/                  ← the LibraryInitializer ILC finds by full name
  Devices/                   ← what a device is, a folder per category
    Audio/                   ← IAudioOutput, AudioFormat, AudioSink, AudioConsumer
    Display/                 ← IDisplay, its facets, DisplayMode, DisplaySink, DisplayConsumer, FirmwareDisplay
    Input/
    Network/                 ← INetworkInterface, NetworkSink, NetworkConsumer, MacAddress
    Storage/                 ← IBlockDevice, its facet IRemovableMedia, BlockConsumer
  DriverKit/                 ← how a driver finds and binds a device
    Driver.cs                ← the driver and its binding: matching, probing, the node tree
    DeviceBinding.cs
    Resources/               ← what a binding hands out: RegisterWindow, DeviceRegion, DmaBuffer
    Interrupts/              ← InterruptSource, InterruptHandle, InterruptHandler, InterruptContext
    Threading/               ← DeviceLock, DeviceEvent, DriverThread, WorkItem
    Buses/
      Pci/                   ← PciIdentity, PciMatch, PciAccess, PciLineInterruptSource, …
      Usb/
    Engine/                  ← internal: DeviceRegistry, DriverEngine, Arbitration, DriverLog
```

`Devices/` 下的每个设备类别都包含与该类设备相关的一切：驱动程序实现的契约、它的能力接口（facet）和词汇、驱动程序用来报告的接收器（块设备没有接收器），以及受支持层的内部消费者，后者获知每个设备的到达与离开，并接收设备的报告。这些类别就是 `Cosmos.Kernel.Drivers` 用来归档驱动程序的类别（[驱动程序文件夹](#driver-folders)），只是少了 `Bus`。`Display/` 还包含 `FirmwareDisplay`，即引擎基于 Core 的 `BootFirmware` 所读取的启动帧缓冲区而发布的显示设备。HAL 没有固件文件夹：读取引导加载程序和固件交出的内容需要使用指针，因此这属于 Core 的职责（[不安全代码](#unsafe-code)）。

`DriverKit/` 根目录包含驱动程序及其绑定：`Driver` 及其特性、注册表、`DeviceBinding`、带有标识与匹配基类的节点树，以及探测与分离相关的词汇。绑定交给驱动程序的东西位于下一级文件夹，按关注点划分：`Resources/` 存放访问硬件的方式（资源、寄存器窗口、映射区域、DMA 缓冲区），`Interrupts/` 存放中断源和处理程序契约，`Threading/` 存放驱动程序用于同步的锁、事件、线程和工作项。驱动程序先引用根目录，再引用它用到的文件夹。

`DriverKit/Buses/` 下的每个总线种类在每个文件夹中为同一角色使用相同的名称：`<Bus>Identity`、`<Bus>Match`；当驱动程序通过总线访问设备时有 `<Bus>Access`（平台总线没有：平台驱动程序映射其节点的资源，只有 PCI 主机节点带有访问对象，即 PCI 文件夹中的 `PciHostAccess`）；当总线自行投递中断时有 `<Bus>InterruptSource`（在 PCI 和平台总线上，名称还说明了投递方式：`PciLineInterruptSource`、`PciMessageInterruptSource`、`PlatformLineInterruptSource`）；旁边则是该总线自身的词汇。总线主机为工具包实现的基类沿用其规范所用的名称（`Ps2Controller`、`UsbHostController`、`VirtioTransport`），而不使用各总线共用的后缀；平台总线的基类是 `PlatformLineRouting`，由机器描述实现。

稳定性标记在每个类型上，而不是按文件夹划分。除了两个稳定契约 `IBlockDevice` 和 `MacAddress` 之外，每个公共 HAL 类型都带有 `[Experimental(Experimentals.DriverKitSeamDiagId)]`；只要有其他公共类型丢失该特性，HAL 项目中的 `CosmosGuardHalPromotion` 目标就会使构建失败（[公共 API 跟踪](public-api.md#the-policy)）。

<a id="member-order"></a>

### 成员顺序

没有需要遵循的分隔符约定。垃圾回收器的四个文件带有 `// --- Section Name ---` 标题，这些标题比本页早三周写成，之后 `Cosmos.Kernel.Drivers` 中的显示驱动程序文件也照此编写。四个受跟踪程序集中其余 353 个非第三方文件都没有这样做，从来没有文件被转换过，也没有分析器检查这一点。同样也不存在可以描述的成员顺序：在拥有三种或更多种成员的 131 个类型中，共有 93 种不同的顺序，最吻合的单一顺序也只覆盖其中 43 个。

有两种顺序仍然值得遵循，因为读者在一屏之内就能检查它们：

- 字段和常量放在读取它们的成员之前，而不是放在第一个方法之后。
- 构造函数放在字段和属性之后、方法之前；静态工厂方法与它所替代的构造函数放在一起。

如果你想在新文件中参照完整的分隔符形式，`Cosmos.Kernel.Drivers/Virtio/Display/VirtioGpu/VirtioGpuDriver.cs` 就是演示它的文件。不要把现有文件转换成这种形式。

<a id="using-directives"></a>

### using 指令

- 将 `using` 指令放在命名空间**之外**。
- `System` 命名空间排在最前面。
- 使用文件范围的命名空间（例如 `namespace Cosmos.Kernel.Core.Scheduler;`）。`ThirdParty/` 下引入的第三方代码树（BigGustave、SharpZipLib、LunarFonts）和 dotnet/runtime 镜像代码保留块形式，以便与上游进行差异比对；而 `Core/Runtime/Stdllib.cs` 根本无法采用文件范围形式：它声明了四个命名空间，其中一个是嵌套的，这会触发 CS8955。

---

<a id="4-class--type-design"></a>

## 4. 类与类型设计

<a id="static-utility-classes"></a>

### 静态工具类

对于没有任何实例状态的无状态内核工具，使用 `static class`。

<a id="managers"></a>

### 管理器

使用 `static class`。管理器协调子系统的状态，无需实例。启动路径上的初始化是一个 `internal static void Initialize()` 方法，由 `LibraryInitializer` 调用，绝不由内核调用：

```csharp
// Simple manager: no underlying instance to expose
public static class TimerManager
{
    private static TickSource? s_tickSource;

    public static bool IsInitialized => s_tickSource is not null;

    internal static void RegisterTickSource(TickSource tickSource) { ... }
    public static void Wait(uint ms) { ... }
}

// Manager wrapping a pluggable implementation: expose via Current
public static class SchedulerManager
{
    private static IScheduler? s_currentScheduler;

    public static IScheduler? Current => s_currentScheduler;

    internal static void Initialize(uint cpuCount) { ... }
}
```

只有当管理器包装了一个可插拔的实现时（例如支持多种调度算法的 `IScheduler`），才添加 `Current` 属性。大多数管理器都不需要它。

不要在状态旁边另外维护一个代表该状态的 `_initialized` 标志。应从该状态推导出 `IsInitialized`，并让 `Initialize` 基于同一个字段防止重入，且最后才给该字段赋值，这样在其背后的状态存在之前，任何读取者都不会看到 `IsInitialized` 变为 true。

当管理器位于功能开关之后时，其成员遵循[公共 API 跟踪](public-api.md#behaviour-when-a-feature-is-compiled-out)中的规则：功能关闭时，读取类成员如实回答，操作类成员则抛出异常并指明开关。

<a id="structs-for-low-level-data"></a>

### 用于底层数据的结构体

对于映射到硬件或内存布局的数据，使用带 `[StructLayout(LayoutKind.Sequential)]` 的 `struct`：

```csharp
[StructLayout(LayoutKind.Sequential)]
internal struct FreeBlock
{
    public unsafe MethodTable* MethodTable;
    public nuint Size;
    public unsafe FreeBlock* Next;
}
```

<a id="enums"></a>

### 枚举

使用 `enum` 表示状态机和标志。在合适的地方应用 `[Flags]`：

```csharp
public enum ThreadState
{
    Ready,
    Running,
    Blocked,
    Sleeping,
    Dead
}

[Flags]
public enum ThreadFlags
{
    None = 0,
    IsIdle = 1 << 0,
    IsKernel = 1 << 1
}
```

<a id="types-compared-by-value"></a>

### 按值比较的类型

身份即内容的类型（`Address`、`MacAddress`、`Mode`）是 `sealed` 的，其状态保存在由构造函数设置的只读成员中，并在 `Equals(object?)` 和 `GetHashCode()` 之外实现 `IEquatable<T>`。三者保持一致：`Equals` 相等的两个实例哈希值相同，并且 `GetHashCode` 从不抛出异常。如果存在排序，则通过 `IComparable<T>` 实现，绝不使用非泛型的 `IComparable`。需要字节的调用者得到的是 `ReadOnlySpan<byte>`；底层数组绝不交出，因为能写入它的调用者会改变所有以该值为键的字典背后的身份。

```csharp
public sealed class Address : IComparable<Address>, IEquatable<Address>
{
    public ImmutableArray<byte> Parts { get; }

    public Address(ReadOnlySpan<byte> buffer) { ... }

    public ReadOnlySpan<byte> ToSpan()
    {
        return Parts.AsSpan();
    }

    public bool Equals(Address? other)
    {
        return other is not null && Parts.SequenceEqual(other.Parts);
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as Address);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id);
    }

    public int CompareTo(Address? other)
    {
        return other is null ? 1 : Id.CompareTo(other.Id);
    }
}
```

<a id="registered-instances"></a>

### 注册的实例

当子系统必须看到每个存活实例时（`TcpConnection.Connections`），构造函数是私有的，由一个静态工厂方法创建并注册实例（`TcpConnection.CreateConnection`）。注册表保持私有，移除实例时会释放它，使它租用的资源归还到池中。

---

<a id="5-kernel-lifecycle"></a>

## 5. 内核生命周期

<a id="user-kernel-pattern"></a>

### 用户内核模式

用户内核继承自 `Cosmos.Kernel.System.Kernel` 并重写生命周期方法：

```csharp
using Cosmos.Kernel.System;

namespace MyKernel;

public class Kernel : Cosmos.Kernel.System.Kernel
{
    protected override void BeforeRun()
    {
        // One-time setup (after boot, before main loop)
        Console.WriteLine("Booted!");
    }

    protected override void Run()
    {
        // Called in a loop until Stop() is called
        string? input = Console.ReadLine();
        ProcessCommand(input);
    }

    protected override void AfterRun()
    {
        // Cleanup (after main loop exits)
    }
}
```

**规则：**
- 只有在需要自定义启动过程时才重写 `OnBoot()`（默认实现会启动 `KernelConsole`）。
- 重写 `BeforeRun()`，在系统就绪后执行一次性初始化。
- `Run()` 是主循环体，让它保持专注。
- 调用 `Stop()` 以干净地退出主循环。
- 应避免从内核返回：基类会在 `AfterRun()` 之后让 CPU 停机。

---

<a id="6-hal-architecture"></a>

## 6. HAL 架构


<a id="interface-driven-hal"></a>

### 接口驱动的 HAL

平台服务通过接口提供。实现在启动时注册：

```csharp
// Interface (Cosmos.Kernel.Core, internal: no kernel registers or obtains one)
internal interface ICpuOps
{
    void Halt();
    void DisableInterrupts();
    void EnableInterrupts();
}

// Implementation (Cosmos.Kernel.HAL.X64)
public class X64CpuOps : ICpuOps
{
    public void Halt()
    {
        Native.Cpu.Halt();
    }

    // ...
}
```

<a id="platform-initializer-pattern"></a>

### 平台初始化器模式

每种架构都提供一个工厂，用于创建所有平台相关组件。工厂、契约及其返回的一切都是 HAL 内部的：内核从不安装它们。初始化器也是机器描述：它根据平台已知的来源发布总线驱动程序所绑定的根平台节点，这些来源包括 ACPI 表、引导加载程序交出的设备树，或者针对每台机器的表；它不包含任何设备驱动程序。

```csharp
internal class X64PlatformInitializer : IPlatformInitializer
{
    public string PlatformName => "x86-64";
    public PlatformArchitecture Architecture => PlatformArchitecture.X64;

    public IPortIO CreatePortIO()
    {
        return new X64PortIO();
    }

    public ICpuOps CreateCpuOps()
    {
        return new X64CpuOps();
    }

    public IInterruptController CreateInterruptController()
    {
        return new X64InterruptController();
    }

    public TickSource CreateTickSource()
    {
        return new PIT();
    }

    public void PublishPlatformNodes()
    {
        // the 8042 and the PCI host, with their port ranges and lines
    }

    public uint GetCpuCount()
    {
        return /* ACPI/MADT */ 1;
    }

    public void InitializeHardware()
    {
        // ACPI, APIC, timers
    }

    public void StartSchedulerTimer(uint quantumMs)
    {
        // Configure PIT/APIC timer for preemptive scheduling
    }
}
```

<a id="adding-a-new-device"></a>

### 添加新设备

在 `Cosmos.Kernel.Drivers` 中基于工具包编写一个 `[Driver]` 类，并通过其绑定发布设备（[编写驱动程序](../user/drivers.md)）。当设备位于工具包不认识的总线上时：

1. 将该总线种类添加到工具包中，放在 `DriverKit/Buses/` 下它自己的文件夹里：一个标识、一个匹配、驱动程序通过总线访问设备时所需的访问对象、总线自行投递中断时所需的中断源，以及一种路径格式（[HAL 文件夹](#hal-folders)）。
2. 从机器描述（`PublishPlatformNodes`）或总线驱动程序（`PublishChild`）发布其节点。
3. 基于访问对象编写叶驱动程序。

除平台代码外，任何东西都不放入 `Cosmos.Kernel.HAL.X64` 或 `Cosmos.Kernel.HAL.ARM64`。设备树解析器是 Core 代码，位于 `Cosmos.Kernel.Core/Firmware` 下，与读取固件交出内容的其他读取器放在一起；ARM64 描述只读取它。

<a id="hal-registration"></a>

### HAL 注册

```csharp
// At boot (from the library initializer, or from OnBoot):
PlatformHAL.Initialize(new X64PlatformInitializer());
```

---

<a id="7-plug-system"></a>

## 7. Plug 系统

Plug 在 IL 层面替换 BCL 方法。patcher 在构建时重新连接调用。有关 plug 特性（`[Plug]`、`[PlugMember]`、`[PlatformSpecific]`）和 plug 模板的完整文档，请参见 [Plug](plugs.md)。

<a id="when-to-use-plugs-vs-other-approaches"></a>

### 何时使用 Plug 而非其他方式

| 场景 | 方式 |
|----------|----------|
| 替换 BCL 方法 | Plug |
| 添加内核级 API | 在 `Cosmos.Kernel.System` 中新建类 |
| 提供运行时存根 | 在 Core 中使用 `[RuntimeExport]` |
| 调用原生代码 | 在 Core 中使用 `[LibraryImport]` |

---

<a id="8-native-interop"></a>

## 8. 原生互操作

<a id="c-callable-from-native"></a>

### 可从原生代码调用的 C#

根据调用者的不同，有两种模式：

**`[RuntimeExport]`**：用于 NativeAOT 运行时存根（由运行时本身调用）。它们与 [`System.Private.CoreLib/RuntimeImports.cs`](https://github.com/dotnet/runtime/blob/main/src/coreclr/nativeaot/System.Private.CoreLib/src/System/Runtime/RuntimeImports.cs) 中的 `[RuntimeImport]` 声明相对应：

```csharp
[RuntimeExport("RhNewArray")]
internal static unsafe void* RhNewArray(MethodTable* pEEType, int length)
{
    return GarbageCollector.AllocArray(pEEType, length);
}
```

**`[UnmanagedCallersOnly]`**：用于可从原生 C 代码调用的 C# 方法（桥接）：

```csharp
[UnmanagedCallersOnly(EntryPoint = "__cosmos_serial_write")]
public static void CosmosSerialWrite(byte* str)
{
    // C code calls this via the symbol __cosmos_serial_write
}
```

对于运行时或 ABI 要求的导出（例如 `Rh*` 存根，以及 `ceil`、`sqrt`、`memmove`、`memset` 等 libc/math 或内存符号），使用 `[RuntimeExport]`。对于其他从原生代码到托管代码的回调（中断处理程序、C 库桥接等），优先使用 `[UnmanagedCallersOnly]`。

<a id="native-callable-from-c"></a>

### 可从 C# 调用的原生代码

调用汇编例程的现代 P/Invoke 模式：

```csharp
[LibraryImport("*", EntryPoint = "_native_cpu_rdtsc")]
[SuppressGCTransition]
private static partial ulong NativeReadTSC();
```

**规则：**
- 使用 `LibraryImport`（而不是 `DllImport`），它由源代码生成，且与 AOT 兼容。
- 使用 `"*"` 作为库名（链接到内核二进制文件本身）。
- 对不会触发 GC 的热路径调用添加 `[SuppressGCTransition]`。
- 将原生互操作方法保持为 `private`，并用整洁的公共 API 包装它们。
- 汇编入口点名称使用带类别前缀的 `_snake_case`：`_native_cpu_rdtsc`、`_native_io_write_byte`。

---

<a id="9-architecture-specific-code"></a>

## 9. 架构相关代码

<a id="conditional-compilation"></a>

### 条件编译

对因架构而异的代码使用 `#if ARCH_X64` / `#if ARCH_ARM64`。目前这只在 `Cosmos.Kernel.Core` 中被容许，而且目标是将其消除。

```csharp
public static void ComWrite(byte value)
{
#if ARCH_ARM64
    while ((Native.MMIO.Read32(PL011_BASE + PL011_FR) & FR_TXFF) != 0) ;
    Native.MMIO.Write8(PL011_BASE + PL011_DR, value);
#else
    while ((Native.IO.Read8(COM1_BASE + REG_LSR) & LSR_TX_EMPTY) == 0) ;
    Native.IO.Write8(COM1_BASE + REG_DATA, value);
#endif
}
```

<a id="when-to-use-what"></a>

### 何时使用哪种方式

| 模式 | 适用场景 |
|---------|------|
| `#if ARCH_X64` | 同一个类，代码差异小（例如串口驱动程序） |
| 独立文件（`*.X64.cs`） | 同一个类，每种架构的代码块较大（例如 ThreadContext） |
| 独立的 HAL 项目 | 共享接口背后的不同实现 |
| 独立的 Native 项目 | 汇编代码（`.s`） |

<a id="guard-rules"></a>

### 守卫规则

- 默认的 `#else` 分支应为 x64（最常见的开发目标）。
- 始终提供两条分支，不要出现缺少另一种架构的孤立 `#if`。
- 考虑这段代码是否应该放在 HAL 实现中，而不是使用 `#if`。

---

<a id="10-memory--safety"></a>

## 10. 内存与安全

<a id="unsafe-code"></a>

### 不安全代码

不安全代码位于 `Cosmos.Kernel.Core` 及其架构项目（`Cosmos.Kernel.Core.X64`、`Cosmos.Kernel.Core.ARM64`）中，以及层次之外的组件中（`Cosmos.Kernel.Boot.Limine`、`Cosmos.Kernel.Plugs`、`Cosmos.Kernel.Debug` 和 `Cosmos.Kernel` 聚合项目）。`Cosmos.Kernel.HAL`、其平台项目、`Cosmos.Kernel.System` 和 `Cosmos.Kernel.Drivers` 使用 `<AllowUnsafeBlocks>false</AllowUnsafeBlocks>` 编译，因此编译器在这些地方拒绝任何指针。这些层需要的内存，由 Core 按地址提供：

- `MemoryBlock`（`Core/Memory/`，gen2 的类型）是某个地址处内存的视图：通过 `Span` 和 `AsSpan<T>` 读写。`PageAllocator.AllocBlock` 以块的形式交出页，`PageAllocator.Free(MemoryBlock)` 将其收回。工具包资源（`DeviceRegion`、`DmaBuffer`）持有一个块，并在其绑定被拆除后拒绝提供 span。
- `AddressSpace.HhdmOffset` 是 Limine 的高半区偏移量，只在一个地方读取。
- `Core/Firmware/` 读取引导加载程序和固件交出的内容：帧缓冲区、设备树、启动时间、ACPI MCFG 条目和 EFI 时钟（`BootFirmware`、`DeviceTree`、`AcpiMcfg`、`EfiRtc`）。

设备就地读取的结构（例如 virtqueue 环）通过在这种 span 上使用 `MemoryMarshal.Cast` 和 `MemoryMarshal.AsRef` 来访问，对设备写入的字段则使用 `Volatile.Read`。当 Core 之上的层需要只有指针才能提供的东西时，解决办法是在 Core 中添加入口点，而不是使用 `unsafe` 块。在 Core 内部，把 `unsafe` 限制在需要它的地方：

```csharp
// Good: unsafe scoped to where needed
public static unsafe void WriteString(string str)
{
    fixed (char* ptr = str)
    {
        for (int i = 0; i < str.Length; i++)
        {
            ComWrite((byte)ptr[i]);
        }
    }
}

// Good: unsafe class for types that inherently work with pointers
public unsafe class Thread : SchedulerExtensible
{
    public ThreadContext* GetContext()
    {
        return (ThreadContext*)StackPointer;
    }
}
```

<a id="memory-allocation"></a>

### 内存分配

```csharp
// Kernel heap allocation (manual, non-GC)
void* ptr = MemoryOp.Alloc(size);
MemoryOp.Free(ptr);

// GC-managed allocation (via RuntimeExport stubs)
// Happens automatically through normal C# object creation
List<int> list = new();  // uses RhAllocateNewArray under the hood
```

<a id="pointer-safety"></a>

### 指针安全

- 在 GC 或扫描代码中，解引用前始终检查指针。
- 地址使用 `nuint`（而不是 `uint`，以避免 64 位截断）。
- 指针运算使用 `nint`/`nuint`，而不是 `int`/`uint`。
- 小而短命的缓冲区使用 `stackalloc`，而不是堆分配。

<a id="buffers"></a>

### 缓冲区

| 缓冲区 | 用法 |
|--------|-----|
| 小、固定大小、仅限一次调用 | `stackalloc` |
| 大小可变、跨调用持有（例如接收缓冲区） | `ArrayPool<T>.Shared.Rent`，在 `Dispose()` 中归还 |
| 只读输入 | `ReadOnlySpan<T>` 参数，而不是 `byte[]` |
| 复制 | 在 span 上使用 `source.CopyTo(destination)`，而不是 `Buffer.BlockCopy` |

租用的数组至少与请求的长度一样长，而不是恰好一样长，因此所有者要跟踪自己使用的长度和偏移量，并以 `ReadOnlySpan<T>` 的形式暴露有效部分，绝不暴露数组本身。`TcpConnection` 就是范例：`_data`、`_dataOffset` 和 `_dataLength` 位于 `Data => _data.AsSpan().Slice(_dataOffset, _dataLength)` 背后，`AppendToData` 在租用数组还有空间时就地写入，`Dispose` 则归还数组。

<a id="critical-sections"></a>

### 临界区

```csharp
// Preferred: scoped interrupt disable
using (InternalCpu.DisableInterruptsScope())
{
    // Interrupts disabled here
    // Re-enabled automatically at scope exit
}
```

---

<a id="11-error-handling"></a>

## 11. 错误处理

<a id="kernel-panic"></a>

### 内核恐慌

对于不可恢复的错误，使用 `Panic.Halt()`。它会禁用中断，打印消息以及调用者的方法、文件和行号，然后让 CPU 停机：

```csharp
if (ptr == null)
    Panic.Halt("Memory allocation failed");
```

调用者信息由编译器填入，因此只需传入消息，不要传入其他参数。`Panic.Halt` 标有 `[DoesNotReturn]`，正因如此，null 检查之后的代码才可以解引用它刚刚拒绝的值。

<a id="exceptions"></a>

### 异常

异常可以正常工作，但要谨慎使用：

```csharp
// Good: validate at API boundaries
public static void RescanPartitions(IBlockDevice device)
{
    ArgumentNullException.ThrowIfNull(device);
    // ...
}

// Good: feature guards
private static void ThrowIfKeyboardDisabled()
{
    if (!CosmosFeatures.KeyboardEnabled)
        throw new InvalidOperationException("Console input requires keyboard feature.");
}

// Bad: exceptions in interrupt handlers, GC, or scheduler hot paths
// These paths cannot allocate (exception objects are heap-allocated)
```

<a id="argument-checks"></a>

### 参数检查

当检查只是一次比较时，用抛出辅助方法替代手写的 `if` 和 `throw`。指明的参数是值所来源的那个参数，这样阅读异常的调用者就能找到需要修正的参数：

```csharp
ArgumentNullException.ThrowIfNull(device);
ArgumentOutOfRangeException.ThrowIfNegative(width);
ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, MaxPartitions);
ArgumentOutOfRangeException.ThrowIfNotEqual(buffer.Length, 4, nameof(buffer));
ArgumentOutOfRangeException.ThrowIfGreaterThan(blockCount, (ulong)data.Length / sector, nameof(blockCount));
ObjectDisposedException.ThrowIf(_disposed, this);
```

一个范围使用两个辅助方法，每个边界一个（先 `ThrowIfLessThan` 后 `ThrowIfGreaterThan`），而不是一个手写的 `||`。没有辅助方法能表达的条件（`sectorCount > host.BlockCount - startSector`）保留显式的 `throw`，必须在抛出前记录日志的检查也是如此。参数名始终使用 `nameof(x)`，绝不使用字符串字面量，也绝不使用消息：`new ArgumentOutOfRangeException("Invalid offset or size")` 指明的是一个名为 `Invalid offset or size` 的参数。

<a id="when-to-panic-vs-throw"></a>

### 何时触发恐慌，何时抛出异常

| 情形 | 操作 |
|-----------|--------|
| 硬件故障、状态损坏 | `Panic.Halt()` |
| GC 或分配器内部错误 | `Panic.Halt()` |
| API 使用不当 | `throw` 相应的异常 |
| 运行时缺少某项功能，且成员要对其执行操作 | `throw InvalidOperationException` 并指明开关 |
| 运行时缺少某项功能，且成员只回答一个问题 | 返回 `0`/`null`/`false`/空 |
| 内核 shell 中面向用户的错误 | `try/catch` + 打印错误消息 |

抛出的类型是调用者可以按名称捕获的类型：`InvalidOperationException`、`ArgumentException` 及其子类、`NotSupportedException`。不写裸的 `throw new Exception(...)`（CA2201）。受支持层的成员用哪种渠道报告失败，以及 `Try` 何时改为返回 `false`，参见[公共 API 跟踪](public-api.md#how-a-member-reports-failure)。

---

<a id="12-nullable-reference-types"></a>

## 12. 可空引用类型

可空引用类型在整个解决方案中启用（`Directory.Build.props` 中的 `<Nullable>enable</Nullable>`）。注解是 API 契约的一部分：不带 `?` 的引用类型承诺它永远不为 null。新代码和修改过的代码不得引入可空性警告：应如实注解，而不是压制警告。

<a id="rules"></a>

### 规则

| 模式 | 用途 |
|---------|---------|
| 字段、参数、返回值上的 `T?` | 任何可以合理为 null 的值 |
| `?? throw` | 将可空值转换为必需值 |
| `ArgumentNullException.ThrowIfNull(arg)` | 在 API 边界验证参数 |
| `[MemberNotNull(...)]` 守卫辅助方法 | 编译器能够跟踪的初始化检查 |
| `is null` / `is not null` | 所有 null 测试（不用 `== null` / `!= null`）。指针保留 `== null`：指针类型上不允许使用模式 |
| 哨兵初始化（`= []`） | null 与空含义相同的字段 |
| `Try` 方法 `out` 参数上的 `[NotNullWhen(true)]` | 返回 `true` 时填充的值；参见[成员如何报告失败](public-api.md#how-a-member-reports-failure) |
| `bool Initialize()` 上的 `[MemberNotNullWhen(true, ...)]` | 能为调用者证明某个静态成员的 `true` 返回值（`KernelConsole.Initialize` 与 `Default`） |
| `T?[]` | 槽位可以为空的数组（`IrqDelegate?[]`） |

<a id="honest-annotations"></a>

### 如实注解

可能返回 `null` 的成员要用 `?` 声明：绝不要为了保留旧签名而用 `!` 掩盖它。反过来，不要为注解已经保证的值添加运行时 null 检查：不要在不可空参数上写 `if (buffer == null) throw`，也不要在不可空返回值上写 `?? "fallback"`。

```csharp
// Declare what can actually be null: callers are forced to handle it
public static IScheduler? Current => _currentScheduler;

public static PerCpuState? GetCpuState(uint cpuId)
{
    return _cpuStates?[cpuId];
}

// Prefer a sentinel over a nullable field when null and empty mean the same thing
private byte[] _window = [];                 // not: private byte[]? _window;
```

<a id="guard-helpers"></a>

### 守卫辅助方法

延迟初始化的管理器公开带有 `[MemberNotNull(...)]` 注解的 `ThrowIf*NotInitialized()` 守卫方法，并在公共入口点的开头调用它们（参见[管理器](#4-class--type-design)）。一次调用即可证明该字段在方法的其余部分中非 null，既不需要 `!`，也不需要逐行检查：

```csharp
private static PerCpuState[]? s_cpuStates;

[MemberNotNull(nameof(s_cpuStates))]
private static void ThrowIfCpuStateNotInitialized()
{
    if (s_cpuStates is null)
    {
        throw new InvalidOperationException($"{nameof(SchedulerManager)} not initialized");
    }
}

public static void CreateThread(uint cpuId, Thread thread)
{
    ThrowIfCpuStateNotInitialized();

    PerCpuState state = s_cpuStates[cpuId];   // no warning, no !
    // ...
}
```

同一系列特性也适用于 `bool` 初始化方法：`KernelConsole.Initialize()` 上的 `[MemberNotNullWhen(true, nameof(Default))]` 让调用者可以在 `if (KernelConsole.Initialize())` 内读取 `KernelConsole.Default`，而无需自己再加守卫。

<a id="callers-of-a-try"></a>

### `Try` 的调用者

带注解的 `Try` 返回 `true` 之后，其 `out` 非 null，任何调用点都不再重复测试它：`if (!TryOpen(path, out handle) || handle == null)` 应写作 `if (!TryOpen(path, out handle))`。BCL 以同样的方式注解了 `Dictionary<TKey, TValue>.TryGetValue`，因此当 `TValue` 不可空时，跟在它后面的 `&& value != null` 什么也没有守卫。多余的测试并非无害：它会掩盖真正需要守卫的那一处。

<a id="required-values"></a>

### 必需值

在 null 结果属于编程错误的地方使用 `?? throw`：

```csharp
Address source = IPConfig.FindNetwork(destination)
    ?? throw new InvalidOperationException("No network route to destination");

public MacAddress MacAddress
{
    get
    {
        return _macAddress ?? throw new InvalidOperationException($"{nameof(_macAddress)} is null");
    }
}
```

<a id="constructors"></a>

### 构造函数

每个构造函数都必须让所有不可空成员完成初始化。删除未使用的无参构造函数，而不是任由它们污染类型：

```csharp
// Bad: leaves RawData null and forces nullable noise on every user of the class
internal IcmpPacket()
{
}
```

<a id="the--operator"></a>

### `!` 运算符

null 包容运算符 `!` 是最后的手段，用于流分析无法看到的不变量（例如守卫标志为 `true` 时保证非 null 的字段）。如果不变量可以用 `[MemberNotNull]` 或 `[NotNullWhen]` 表达，请改用特性。

<a id="enforcement"></a>

### 强制执行

格式检查 CI（`dotnet format --severity error`）强制执行 `.editorconfig` 中的 null 处理风格规则：`csharp_style_throw_expression`、`dotnet_style_coalesce_expression`、`dotnet_style_null_propagation`、`csharp_style_prefer_null_check_over_type_check`、`csharp_style_pattern_matching_over_as_with_null_check`、`dotnet_style_prefer_is_null_check_over_reference_equality_method` 和 `csharp_style_conditional_delegate_call`。

可空性警告（`CS86xx`）目前还不是错误：不要引入新的此类警告。

---

<a id="13-modern-c--net-10-idioms"></a>

## 13. 现代 C# / .NET 10 惯用法

项目以 `<LangVersion>latest</LangVersion>` 和 .NET 10 为目标。请全面使用现代语言特性。

<a id="modern-patterns"></a>

### 现代模式

```csharp
// File-scoped namespaces
namespace Cosmos.Kernel.Core.Scheduler;

// Expression bodies for a property, indexer or accessor that fits on one line
public string PlatformName => "x86-64";

// Pattern matching
switch (args[i])
{
    case null:
        WriteString("null");
        break;
    case string s:
        WriteString(s);
        break;
    case int n:
        WriteNumber(n);
        break;
}

// Null-conditional and coalescing
IBlockDevice? device = StorageManager.PrimaryDevice;
if (device?.BlockSize is not > 0)
{
    return;
}

Address ip = config?.Address ?? defaultAddress;

// Collection expressions
DeviceResource[] resources = [DeviceResource.PortRange(0x60, 1), DeviceResource.PortRange(0x64, 1)];

// Target-typed new
Thread thread = new();
List<int> items = new();

// Numeric separators for readability
public const nuint DefaultStackSize = 64 * 1024;
public static long TscFrequency { get; set; } = 1_000_000_000;

// stackalloc for small buffers (no heap allocation)
Span<byte> buffer = stackalloc byte[64];

// field keyword: auto-property with validation without a manual backing field
public int Priority
{
    get => field;
    set => field = value >= 0 ? value : throw new ArgumentOutOfRangeException();
}

// Interpolation, not concatenation
Serial.WriteString($"[{Table[(int)Status]}] {packet}\n");

// required members for state the constructor cannot take; derive what follows from it
public sealed class DhcpOption
{
    public required byte Type { get; init; }
    public required byte[] Data { get; init; }
    public byte Length => (byte)Data.Length;
}

// One dictionary call carries the answer
if (!s_clients.TryGetValue(port, out UdpClient? client))
{
    return;
}
s_registeredTypes.TryAdd(name, fileSystemType);
cache[id] = mac;                        // upsert: no ContainsKey first

// readonly on every field assigned only at its declaration or in a constructor
private readonly Canvas _canvas;
private static readonly ArrayPool<byte> s_arrayPool = ArrayPool<byte>.Shared;

// Auto-property over a field behind a pass-through accessor
public static KernelConsole? Default { get; private set; }
```

最后两项有三条限制。在可变结构体类型（`SpinLock`）的字段上使用 `readonly` 是错误的：每次方法调用都会作用于一个防御性副本，锁永远不会被获取。`dotnet_style_readonly_field` 背后的分析器不知道哪些结构体方法会修改状态，因此只对引用类型和没有修改性成员的结构体采纳它的建议。plug 按名称访问的字段（`[FieldAccess]`）保持为字段：自动属性的后备字段名称由编译器生成。此外，执行分配的静态初始化器就是类构造函数，它在首次访问时通过一个需要当前线程的锁运行：设备启动过程中可能访问到的类型（`MacAddress`，任何驱动程序都可能读取它的 `None` 和 `Broadcast`）保留延迟填充的静态成员，并附上注释说明原因。

<a id="avoid-1"></a>

### 避免

```csharp
// Don't use var when type isn't obvious
var x = GetValue();           // Bad: what type is x?
int count = GetValue();       // Good: explicit type

// Don't use var for built-in types
var i = 0;                    // Bad
int i = 0;                    // Good

// Don't use this. qualification
this._field = value;          // Bad
_field = value;               // Good

// Don't allocate arrays in hot paths when Span works
byte[] temp = new byte[16];       // Bad in hot path: heap allocation
Span<byte> temp = stackalloc byte[16]; // Good: stack allocation

// Always use braces, no single-line if/for/while without braces
if (ptr == null) return;          // Bad
if (ptr == null)                  // Bad
    return;

if (ptr == null)                  // Good
{
    return;
}

// Don't give a method, constructor, operator or local function an expression body
public void Halt() => Native.Cpu.Halt();     // Bad

public void Halt()                           // Good
{
    Native.Cpu.Halt();
}

// Don't wrap an expression body: past one line, a property takes a get block
public bool IsFixedRoot =>                   // Bad
    Parent is null && Superblock.Boot.Type != FatType.Fat32;

public bool IsFixedRoot                      // Good
{
    get
    {
        return Parent is null && Superblock.Boot.Type != FatType.Fat32;
    }
}

// Don't compare a bool to a literal
if (applied == false)             // Bad
if (!applied)                     // Good

// Don't test a key, then index; the lookup answers once
if (map.ContainsKey(key))         // Bad: two lookups
{
    device = map[key];
}

// Don't loop on a literal; the condition says what ends the loop
while (true)                      // Bad: nothing in the header bounds it
{
    if (pos >= end)
    {
        break;
    }
    pos += Step(pos);
}

while (pos < end)                 // Good: the bound is the condition
{
    pos += Step(pos);
}
```

> 大括号由 `.editorconfig`（`csharp_prefer_braces = true:error`）和 CI（`dotnet format style --severity error`）强制执行。

方法、构造函数、运算符和局部函数使用块体，即使只有一条语句也是如此：换行的 `=>` 会掩盖方法体的起点。属性、索引器或访问器只有在整个成员能放在一行内时才使用 `=>`，而 lambda 保留其 `=>`。`ThirdParty/` 下引入的第三方代码树同样遵循这条规则，每个代码树的 README 都列出了它修改过的成员。`.editorconfig` 在 `csharp_style_expression_bodied_*` 选项中以 `silent` 级别记录了这条规则，因此 CI 不会强制执行它。

<a id="bounded-loops"></a>

### 有界循环

不写 `while (true)`。循环的头部要说明是什么让它结束：一个计数、一个索引的上界、一个截止时间，或者循环体在找到目标时设置的标志。边界写在头部时，读者只需在一处检查每条路径是否都朝它推进；使用 `while (true)` 时，出口就在 `break` 或 `return` 碰巧所在的地方，一个因修改而丢失出口的循环，读起来与一个从未有过出口的循环一模一样。三类看似无界的循环各有其要说明的边界：

| 循环 | 头部 |
|------|--------|
| 遍历缓冲区的解析 | `while (pos < end && !closed)`，以解析的终点为边界，并用一个标志表示结束解析的那条记录 |
| 等待硬件或其他线程 | `while (!ready && Stopwatch.GetTimestamp() < deadline)` 或 `while (!ready && attempts++ < MaxAttempts)`；没有预算的轮询正是 [#365](https://github.com/valentinbreiz/nativeaot-patcher/issues/365) 所跟踪的卡死问题 |
| 内核主循环，或永不返回的停机 | 前者使用 `while (!_stopped)`；后者在循环体中让 CPU 停机，所以自旋正是其目的，`while (true) { Halt(); }` 予以保留 |

最后一行是唯一的豁免：一个永不返回、且循环体会让 CPU 停机的方法，例如 `Power.Reboot` 和 `Power.Shutdown` 在平台两者都无法完成时所回退到的方法，以及 `Kernel.Start` 在 `AfterRun` 之后的做法。循环体为空的循环不属于这一豁免；它白白消耗 CPU 而毫无进展，应改为停机。其他所有循环都要说明其边界。`src/` 中现有的 `while (true)` 循环早于这条规则，会在修改其周边代码时一并转换，而不是集中清理；没有分析器检查这一点。

<a id="feature-switches"></a>

### 功能开关

使用 `[FeatureSwitchDefinition]` 进行编译时的功能切换（由 NativeAOT 链接器裁剪）。所有功能标志都位于 `Cosmos.Kernel.Core.CosmosFeatures` 中：

```csharp
// In CosmosFeatures.cs: one property per feature
[FeatureSwitchDefinition("Cosmos.Kernel.HAL.Interrupts.Enabled")]
public static bool InterruptsEnabled
{
    get
    {
        return AppContext.TryGetSwitch("Cosmos.Kernel.HAL.Interrupts.Enabled", out bool enabled) ? enabled : true;
    }
}

// The ILC linker trims the dead branch entirely
if (CosmosFeatures.KeyboardEnabled)
{
    // This code is removed from the binary when keyboard is disabled
}
```

---

<a id="14-aot-constraints"></a>

## 14. AOT 约束

NativeAOT 施加了严格的限制。**所有内核代码都必须与 AOT 兼容。**

.NET 10 改进了 NativeAOT，生成的二进制文件更小，支持的平台更广，但根本性的约束依然存在。

<a id="forbidden"></a>

### 禁止

- `System.Reflection.Emit`（不能在运行时生成代码）。
- `dynamic` 关键字。
- 在运行时使用 `Assembly.Load`。
- 在运行时按字符串调用 `Type.GetType("...")`（类型必须是静态可达的）。
- 在运行时使用 `MakeGenericType` / `MakeGenericMethod`（泛型实例化必须在编译时已知）。

<a id="use-with-caution"></a>

### 谨慎使用

- **泛型虚方法**：NativeAOT 必须在编译时生成所有可能的实例化。避免深度嵌套或开放式的泛型虚分派。
- **LINQ**：许多 LINQ 运算符会分配迭代器和闭包。避免在热路径（GC、调度器、中断处理程序）中使用。

---

<a id="15-documentation"></a>

## 15. 文档

<a id="license-header"></a>

### 许可证头

每个 `.cs` 文件都必须以许可证头开始（由 `.editorconfig` 强制执行）：

```csharp
// This code is licensed under the BSD 3-Clause license (see LICENSE for details)
```

<a id="xml-documentation"></a>

### XML 文档

用 `<summary>` 为所有**公共** API 编写文档。保持简洁：

```csharp
/// <summary>
/// Triggers a kernel panic with the specified message.
/// Disables interrupts and halts the CPU.
/// </summary>
/// <param name="message">The panic message describing the error.</param>
public static void Halt(string message) { ... }
```

**规则：**
- 为公共类、接口、方法和属性编写文档。
- 显而易见的成员无需文档（属性名称本身足以说明含义的 `get`/`set`）。
- 对不明显的参数使用 `<param>`。
- 当返回值需要解释时使用 `<returns>`。
- 使用 `<see cref="..."/>` 引用相关类型。
- 除非逻辑复杂，否则不要为私有实现细节编写文档。

<a id="inline-comments"></a>

### 行内注释

谨慎使用，仅在代码无法自我说明时添加：

```csharp
// Stack layout (growing downward from top):
// [StackBase + stackSize] = Top of usable stack
// [contextAddr] = Start of ThreadContext (where StackPointer points)
// [StackBase] = Bottom of stack

// Align context to 16 bytes (required for XMM operations)
contextAddr = (contextAddr + 0xF) & ~(nuint)0xF;
```

被注释掉的代码应当删除；历史记录会保留它。`TODO` 要写明缺少什么以及为什么可以推迟；对代码的疑问（`// is this correct?`）应当是一个 issue，而不是一条注释。主体为空的成员不是占位符：在它真正做事之前不要写它。

<a id="16-testing"></a>

## 16. 测试

完整的测试指南（单元测试、内核集成测试、UART 协议、CI、编写测试内核）请参见[测试](testing.md)。

不需要硬件的逻辑（`TcpConnection` 的接收缓冲区运算、地址解析）在主机进程中由 `tests/Cosmos.Kernel.Tests.System`（NUnit）进行单元测试：每个被测成员对应一个以其命名的嵌套测试夹具（fixture），测试名称采用 `WhenX_AndY_ResultZ` 的形式，值表使用 `[TestCase(..., ExpectedResult = ...)]`。该项目获得了 `Cosmos.Kernel.System` 授予的 `InternalsVisibleTo` 权限。

**代码覆盖率**：给拉取请求添加 `run-coverage` 标签即可触发覆盖率 CI。它会运行内核测试套件，并输出集成测试覆盖了哪些代码路径。

---

<a id="17-public-api-surface"></a>

## 17. 公共 API 面

有三条规则决定什么是 `public`（完整的策略及其机制见[公共 API 跟踪](public-api.md)）：

1. **一个受支持层**。`Cosmos.Kernel.System` 是内核编程所面向的 API，外加其签名所暴露的契约类型（HAL 的设备契约 `IBlockDevice` 和 `MacAddress`，以及 Core 的平台接口）。只有这一 API 面会被跟踪、编写文档，并纳入弃用周期。
2. **精心选择的实验性扩展点**。受支持层之外的扩展点是有意开放的，并标记为 `[Experimental("COSMOSxxxx")]`：现在即可使用，不作兼容性承诺，移除该特性即完成转正。绝不要仅仅通过把某样东西设为 public 来开放扩展点。
3. **其余一切都是 `internal`**。可见性不是扩展机制。第一方程序集和白盒测试内核使用 `InternalsVisibleTo`；外部代码使用 `[UnsafeAccessor]`（[访问内部成员](accessing-internals.md)），风险自负。

由此得出的实践规则：

```csharp
// Good: new user-facing capability lands as a Cosmos.Kernel.System facade
public static class MemoryDiagnostics { public static ulong FreePages => PageAllocator.FreePageCount; }

// Bad: making the Core type public so a kernel can reach it
public static class PageAllocator { ... }
```

- **新类型默认为 `internal`**。在受跟踪的项目中将符号设为 `public` 是需要评审的决定：在 `make api` 将其记录到 `PublicAPI.Unshipped.txt` 之前，构建会失败（`RS0016`），而 txt 文件的差异应放在同一次提交中。
- **由 DevKernel 负责强制检验**。`examples/DevKernel` 在没有任何 `InternalsVisibleTo` 授权的情况下编译；它需要的一切都必须来自受支持层。
- **受跟踪的 API 面必须有文档**。启用 `CosmosTrackPublicApi` 会把缺失的 XML 文档（`CS1591`）变成该项目公共符号的构建错误。
- **白盒测试内核获得 `InternalsVisibleTo` 授权**，并在授予权限的 `.csproj` 中附上注释，说明它观察的内容；它绝不强迫某个符号变为 public。

---
