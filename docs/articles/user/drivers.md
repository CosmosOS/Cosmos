# Writing a Driver

In this article, we will discuss how to write a device driver for Cosmos Gen3: what a driver is and where it runs, how the build finds it, how it takes a device and reaches its hardware through the driver kit, how it hands the kernel a device, and how it is tested with no hardware behind it.

The main differences if you come from Gen2:

| | Gen2 | Gen3 |
|---|---|---|
| Base class | `Cosmos.HAL.Device` | `Cosmos.Kernel.HAL.DriverKit.Driver` |
| Registration | By hand, in `Cosmos.HAL.Global` or the kernel | A generated manifest registers every `[Driver]` class the kernel can see |
| Hardware access | `IOPort`, `PCIDevice`, raw memory | A `DeviceBinding`: register windows, bulk regions, DMA memory, interrupts; a `PciAccess` for a PCI function's configuration space; a `VirtioAccess` for a virtio device's handshake, queues and configuration space |
| Device removal | None | The kit tears the binding down in a fixed order; the driver frees nothing itself |
| Testing | On the hardware | Over the synthetic bus, in a test kernel, identically on x64 and ARM64 |

If you find bugs or something abnormal, please [submit an issue](https://github.com/CosmosOS/Cosmos/issues/new/choose) on our repository.

## Experimental status

Every type under `Cosmos.Kernel.HAL.DriverKit` (`Driver`, `DriverAttribute`, `DeviceBinding`, `DeviceNode`, the resource, interrupt and deferred-work types), `Cosmos.Kernel.HAL.DriverKit.Devices` (the device contracts, their facets and their sinks), `Cosmos.Kernel.HAL.DriverKit.Synthetic` (the test bus) and the bus kinds under `Cosmos.Kernel.HAL.DriverKit.Platform`, `Cosmos.Kernel.HAL.DriverKit.Pci` and `Cosmos.Kernel.HAL.DriverKit.Virtio` carries `[Experimental("COSMOS0003")]`: they are usable today but make no compatibility promise, and they are promoted to the stable surface by removing the attribute once proven. The seam reaches outside the HAL in two places under the same id: `ICanvas3DFactory` in `Cosmos.Kernel.System.Graphics`, the ring-defined facet a display implements when it renders 3D, and `ISvgaAdapter` in `Cosmos.Kernel.Drivers`, the VMware SVGA II adapter's test seam. Referencing any of them is a build error until the project acknowledges that contract:

```xml
<PropertyGroup>
  <NoWarn>$(NoWarn);COSMOS0003</NoWarn>
</PropertyGroup>
```

See [Public API Tracking](../dev/public-api.md) for how experimental seams fit the surface policy.

The kit is being built in stages. What exists today:

- **Four bus kinds: the synthetic bus, the platform bus, PCI and virtio.** A test publishes synthetic nodes through `SyntheticBus.Publish`; the machine description in the arch HAL seeds the platform bus with the machine's PCI host node at boot, and on ARM64 with one node per occupied slot of the virt machine's virtio-mmio window; the PCI host driver, bound to the host node, publishes one PCI node per function it finds; and the two virtio transport drivers, bound to a virtio PCI function or a virtio-mmio slot, publish one virtio node each, which is how a driver for a virtio device is offered its device without knowing the transport ([Buses](#buses), [PCI devices](#pci-devices), [Virtio devices](#virtio-devices)). USB and PS/2 nodes are later stages, and the device drivers the kernel ships for them (PS/2, and the USB host controller with its class drivers) still live in the HAL outside the kit; the storage controllers do not: the AHCI and NVMe drivers are kit drivers ([The storage drivers](#the-storage-drivers)). A PCI function a HAL driver operates is sized like every other and otherwise left as it is unless a kit driver matches it.
- **Five kernel managers consume a published device: the keyboard, mouse, network, display and storage managers.** `KeyboardManager`, `MouseManager`, `NetworkManager`, `DisplayManager` and `StorageManager` install their consumers before the driver stage, so a keyboard a driver publishes through `PublishKeyboard` joins the keyboard manager's list, a pointer published through `PublishPointer` joins the mouse manager's, an interface published through `PublishNetwork` becomes an adapter the ring, the stack and the clients use, a display published through `PublishDisplay` joins the display manager's list, where `Canvas.GetFullScreen()` draws on the primary one, a block device published through `PublishBlockDevice` is registered with the storage manager and scanned for partitions before the call returns, and the log line says `(consumed)`. The framebuffer the bootloader handed over is a display too: the engine publishes it as the firmware display before it starts ([Publishing a device](#publishing-a-device)).
- **The drivers Cosmos ships over the kit live in `Cosmos.Kernel.Drivers`**: `PciHostDriver`, the Intel `E1000EDriver`, the two virtio transport drivers `VirtioPciTransportDriver` and `VirtioMmioTransportDriver`, the virtio leaf drivers `VirtioNetDriver`, `VirtioInputDriver` and `VirtioGpuDriver`, the VMware SVGA II display driver `VmwareSvgaDriver`, and the two storage drivers `AhciDriver` and `NvmeDriver`, written over the public seam as a third party would write them. Every kernel gets the package through `Cosmos.Kernel`, so all ten are in its manifest; [Project settings](#project-settings) says how to drop one.
- **Everything else on this page is implemented and tested**: the manifest, arbitration, the binding and its ledger, both execution contexts and the guard, teardown, the diagnostics view, and the synthetic bus. The Drivers test suite exercises all of it on x64 and ARM64, and its drivers are the models for the samples below; the Virtio suite drives the virtio drivers over both transports.

## What a driver is

The kit has five nouns and one verb.

| Noun | What it is | Who creates it |
|------|------------|----------------|
| **Node** (`DeviceNode`) | One piece of hardware the kernel can see: an identity on a bus, a list of resources (memory windows, port ranges, RAM windows) and interrupt sources, and a bus-specific access object | A bus (the synthetic bus for a test, the platform bus for the root nodes the machine description seeds), or a bus driver publishing a child (the PCI host driver, one per function; a virtio transport driver, one per device) |
| **Bus kind** | The schema a node on that bus follows: what its identity looks like, how a driver matches it, what its access object can do | The kit; four kinds today: synthetic, platform, PCI and virtio |
| **Driver** (`Driver`) | A class declaring a name, a match table, a priority, and one entry point that receives a node and either takes it or does not | You, in the kernel project or in a driver library |
| **Binding** (`DeviceBinding`) | The ownership record between one driver and one node: every resource the driver acquired, every device it published, every child node it created. The driver reaches hardware and the kernel *only* through it | The kit, once per offer |
| **Device** | What a driver hands the kernel: an object implementing one of the kit's device kinds (`IKeyboard`, `IPointer`, `INetworkInterface`, `IBlockDevice`, `IDisplay`) | The driver, through its binding |

The verb is **bind**: when a node appears, the kit lists the drivers whose match table covers it, orders them, and offers the node to each in turn until one returns `ProbeResult.Bound`. Everything a driver acquired while looking is released if it does not. When the node goes away, the kit tears the binding down in the reverse order, withdrawing devices first and freeing memory last, without the driver writing that code.

The smallest driver that compiles and binds:

```csharp
using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Synthetic;

namespace MyOS.Drivers;

[Driver]
public sealed class SampleDriver : Driver
{
    private readonly DeviceMatch[] _matches = [SyntheticMatch.Key("sample")];

    public override string Name => "sample";

    public override ReadOnlySpan<DeviceMatch> Matches => _matches;

    public override ProbeResult Probe(DeviceBinding binding)
    {
        binding.Log("bound");
        return ProbeResult.Bound;
    }
}
```

`Name` is what the log and the diagnostics view print. `Matches` is the table of identities the driver wants to be offered. `Priority` (virtual, `0` by default) decides who is offered a node first among the drivers that match it. `Probe` is the only required entry point; `OnDetach` is the optional other one.

One instance per class: the manifest constructs each driver once, and that instance is offered every node it matches. Fields on the driver are therefore shared across devices. State that belongs to one device lives on an object the driver creates in `Probe` and hangs off `binding.DriverState`, which is the one slot the binding keeps for the driver and hands back in work items, threads and `OnDetach`.

## Where a driver runs

The driver stage runs from `Kernel.Start`, after the module initializers have brought up the heap, the interrupt controller, the scheduler and its tick, and after interrupts are enabled. The kit logs the manifest, starts its worker thread, offers every node published so far, and returns once every node has been offered, the children a bus driver published from its probe included. Only then do `OnBoot` and `BeforeRun` run, so a device a driver bound is usable from there on. The serial log of an x64 kernel with no drivers of its own, launched with `cosmos run --nic virtio-net-pci`, shows the stage between the two `[Kernel]` lines: the manifest holds the ten drivers Cosmos ships, in manifest order; the framebuffer the bootloader handed over is published as the firmware display, which the display manager takes as its primary, before the engine starts; the PCI host node the platform bus seeded is offered first; the functions the host driver found are offered after it, each one bound by a driver or left `no driver` (the machine's standard VGA at `00:01.0` here, whose framebuffer the firmware display is; q35's built-in AHCI at `00:1f.2`, which carries the boot CD-ROM on its port 5 and no disk, is bound by the AHCI driver with no port published); and the virtio node the transport driver published beneath the NIC's function is offered once the functions are, since a child's offer is queued behind the job that published it:

```
[Kernel] Enabling interrupts...
[Kernel] Starting drivers...
[Drivers] manifest: AhciDriver(prio 0) E1000EDriver(prio 0) NvmeDriver(prio 0) PciHostDriver(prio 0) VirtioGpuDriver(prio 0) VirtioInputDriver(prio 0) VirtioMmioTransportDriver(prio 0) VirtioNetDriver(prio 0) VirtioPciTransportDriver(prio 0) VmwareSvgaDriver(prio 0)
[Display] primary: firmware "framebuffer" (the only display)
[Drivers] firmware published display "framebuffer" (consumed)
[Drivers] engine started, worker thread
[Drivers] platform:pci@cf8 candidates: PciHostDriver(prio 0, spec 1)
[Drivers] platform:pci@cf8 PciHostDriver: 6 functions on 1 buses
[Drivers] platform:pci@cf8 offer PciHostDriver -> bound
[Drivers] pci:0000:00:00.0 no driver
[Drivers] pci:0000:00:01.0 no driver
[Drivers] pci:0000:00:02.0 candidates: VirtioPciTransportDriver(prio 0, spec 1)
[Drivers] pci:0000:00:02.0 VirtioPciTransportDriver: virtio type 1, 4 message interrupts
[Drivers] pci:0000:00:02.0 offer VirtioPciTransportDriver -> bound
...
[Drivers] pci:0000:00:1f.2 candidates: AhciDriver(prio 0, spec 3)
[Drivers] pci:0000:00:1f.2 AhciDriver: port 0: no device (SSTS 0x0)
...
[Drivers] pci:0000:00:1f.2 AhciDriver: port 5: satapi not supported
[Drivers] pci:0000:00:1f.2 AhciDriver: 0 sata ports of 6 implemented, version 1.0, 32 slots, 64-bit
[Drivers] pci:0000:00:1f.2 offer AhciDriver -> bound
...
[Drivers] virtio:pci:0000:00:02.0 candidates: VirtioNetDriver(prio 0, spec 1)
[Drivers] virtio:pci:0000:00:02.0 VirtioNetDriver published network "virtio-net" (consumed)
[Drivers] virtio:pci:0000:00:02.0 VirtioNetDriver: mac 52:54:00:12:34:56, link up, version 1, interrupts: 4 entries
[Drivers] virtio:pci:0000:00:02.0 offer VirtioNetDriver -> bound
[Kernel] Calling OnBoot()...
```

A kernel built with `CosmosEnablePCI` off has no host node, no `PciHostDriver` and no `VirtioPciTransportDriver`; `E1000EDriver` and `VirtioNetDriver` are guarded by `CosmosEnableNetwork` instead, so they stay in the manifest while that switch is on and are offered nothing when no bus publishes their device. `VirtioGpuDriver` and `VmwareSvgaDriver` ride `CosmosEnableGraphics`, the switch that also decides whether the firmware framebuffer is recorded and published: with graphics off a kernel has no display at all, and with PCI off the SVGA driver is offered nothing while the virtio-gpu driver still binds a device behind the MMIO transport. `AhciDriver` and `NvmeDriver` ride `CosmosEnableStorage`, the switch that also decides whether the storage manager exists to consume what they publish, and which the SDK turns off with `CosmosEnablePCI`, since their controllers are PCI functions no other bus publishes. `VirtioMmioTransportDriver` and `VirtioInputDriver` ride no switch: the first is offered nothing on a machine without a virtio-mmio window, and the second checks the keyboard and mouse switches itself in its probe, because one device type is either. The manifest is `(empty)` only for a kernel that excludes every shipped driver and declares none of its own.

A node published after boot (a hot-plug, or a test publishing a synthetic node from `BeforeRun`) takes the same path: it is queued for the worker, offered there, and the publisher waits until the offer is done. See [Kernel Startup](startup.md) for the rest of the boot sequence.

Probes, teardowns and work items run one at a time on the kit worker, so a driver never sees two of them overlap; only driver threads run concurrently, with each other and with the worker.

A kernel built with `CosmosEnableScheduler` off has no worker. The engine then runs inline: the thread that publishes or retracts a node drains the queue itself, and the log says `engine started, inline (no worker)`. Probing works the same, but nothing deferred does: `WorkItem.Schedule` returns `false`, `TrySchedulePeriodic` returns `false`, and `TryStartThread` returns `false`. A driver that must work in such a kernel checks those results.

## The two execution contexts and the guard

The kit has exactly two execution contexts, and every entry point says which it is.

| Entry point | Context | May block | May allocate |
|-------------|---------|-----------|--------------|
| `Driver.Probe`, `Driver.OnDetach` | thread (the kit worker) | yes | yes |
| A work item created through the binding | thread (the kit worker) | yes | yes |
| A driver thread started through the binding | thread (its own) | yes | yes |
| An `InterruptHandler` passed to `TryRequestInterrupt` | interrupt (interrupts masked, on the interrupted stack) | **no** | **no** |
| A device-kind method the kernel calls (`SetLeds`, `Transmit`, `Flush`) | thread (the caller's) | briefly | yes |

An interrupt handler receives an `InterruptContext`, whose three members are the whole of what it may ask the kit for: `Mask()` its own source, `Signal(DeviceEvent)` a thread that is waiting, and `Schedule(WorkItem)` work that needs thread context. Besides those it may read and write its `RegisterWindow`, its `DmaBuffer.Span` and its `DeviceRegion`, report through a sink, and mask or unmask through its `InterruptHandle`; all of them are allocation-free. The context carries no binding, so a handler cannot map, allocate, publish, sleep or wait by type; one that reaches the binding through its state object hits the guard below. The pattern every driver follows is the same: the handler acknowledges the device, reports what it must, and hands the rest to a work item, which walks the completion rings in thread context.

Two rules the types cannot enforce, the handler has to keep itself: do not allocate (no `new` of a reference type, no string building, no lambda that captures) and do not block on anything outside the kit.

The rule is also enforced at run time, in every build. Every method of `DeviceBinding` is thread context; called from a driver's interrupt handler, through a binding the driver kept in its state object, the method stops. In a synthetic dispatch (a test raising the source through `SyntheticBus.RaiseInterrupt`) that is an `InvalidOperationException` naming the member; the kit's trampoline catches it, records it as a fault on the node (`FaultCount`, `LastFault`), masks the source so a handler that throws once cannot throw on every delivery, and logs `interrupt handler threw` from thread context. In a real interrupt it is a panic naming the member, because building an exception there would allocate in the very place the guard forbids it. The Drivers suite has a driver whose handler sleeps on purpose, to assert that path.

## The attribute and the manifest

`[Driver]` is what the build looks for. A source generator in the kernel project emits `DriverManifest.g.cs`, one `DriverRegistry.Register(new X())` call per driver, and the generated entry point runs it before the kernel starts; the registry is fixed once the engine has started. [Driver Manifest](../dev/build/driver-manifest.md) describes the generator, the diagnostics and the build plumbing; what a driver author needs is this:

- A driver in the **kernel project** may be `internal`. A driver in a **class library** must be `public`, and the library must reference `Cosmos.Kernel.HAL`, because that is where the generator looks.
- The class must be concrete, non-generic, derive from `Driver`, and have a parameterless constructor the kernel can call.
- **Order is deterministic.** The kernel's own drivers come first (by file path, then position in the file), then referenced assemblies (by assembly name, then full type name). Position is the last arbitration key, and the log prints the manifest at boot.
- `[Driver(Feature = DriverFeature.Keyboard)]` ties the registration to a feature switch: the `Register` call is wrapped in `if (KernelFeatures.Keyboard)`, so a kernel with `CosmosEnableKeyboard` off never constructs the driver and everything only it references is trimmed. The members of `DriverFeature` mirror the `KernelFeatures` properties by name (`Interrupts`, `Uart`, `Pci`, `Timer`, `Keyboard`, `Mouse`, `Network`, `Storage`, `Fat`, `Graphics`, `Scheduler`, `Usb`); `None`, the default, emits no guard.
- `[Driver(Default = false)]` marks a driver a kernel must opt into by name with a `CosmosDriverInclude` item; any driver can be dropped with a `CosmosDriverExclude` item. Both are shown under [Project settings](#project-settings).

## Identity and matches

A bus knows a device before any driver looks at it, and it says so in a `DeviceIdentity`: `BusName` (the bus the device sits on), `Address` (its address in the bus's own notation) and `Describe()` (the identity in words, for the log). The node's `Path` is the two joined with a colon (`synthetic:kbd`, `platform:pci@cf8`, `pci:0000:00:03.0`), and it is how the log and the diagnostics view name the node.

A driver's match table is a `ReadOnlySpan<DeviceMatch>`. Each `DeviceMatch` is a predicate over an identity, `Matches(DeviceIdentity)`, plus a `Specificity`: how many identity fields it constrains. Each bus kind brings its identity type and its match type as a pair, and a driver never sees a transport, only the identity.

There are four pairs today. `SyntheticIdentity` carries a `Key` the test chose, and `SyntheticMatch` has two shapes:

```csharp
// This device and no other: specificity 1.
private readonly DeviceMatch[] _matches = [SyntheticMatch.Key("kbd")];

// Every synthetic device: specificity 0.
private readonly DeviceMatch[] _matches = [SyntheticMatch.Any()];
```

`PlatformIdentity` and `PlatformMatch` are the pair of the platform bus, `PciIdentity` and `PciMatch` the pair of PCI, and `VirtioIdentity` and `VirtioMatch` the pair of virtio; all three are described with their buses below, and a driver for any of them reads the same way: a table of matches and nothing about how the node was found. A later bus kind (a USB match by class or by vendor and product) adds its pair the same way.

When a node appears, the kit collects every registered driver with a matching entry and orders them: by `Priority` (highest first), then by the specificity of the driver's best match (highest first), then by manifest position (earliest first). The log prints the order:

```
[Drivers] synthetic:prio candidates: HighPriorityDriver(prio 10, spec 1) LowPriorityDriver(prio 0, spec 1)
[Drivers] synthetic:prio offer HighPriorityDriver -> bound
```

Each candidate is offered the node with a fresh binding, and the first to return `Bound` keeps it. A node no driver matched is logged `no driver` and stays in the tree as `Unbound`, so the diagnostics view can show it. Priority is how a kernel's own driver overrides a framework one for the same hardware: return more than `0`; specificity only breaks a tie in priority.

## Probe and the binding

`Probe` is the only required entry point, and `DeviceBinding` is the only door. Everything the driver acquires goes through the binding and is written to its **ledger**: windows, regions, DMA buffers, interrupt handles, events, work items, periodic work, threads, published devices, child nodes. The constructors of all of those types are internal; there is no other way to get one. The driver cannot forget cleanup because it never writes any.

A probe reads the node (`binding.Node.Resources`, `binding.Node.Interrupts`, `binding.Node.Access<T>()`), acquires what it needs, creates its per-device state, publishes what the device is, and returns. This is the shape of a full driver, modelled on the keyboard driver of the Drivers suite:

```csharp
using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Synthetic;

namespace MyOS.Drivers;

[Driver(Feature = DriverFeature.Keyboard)]
public sealed class SyntheticKeyboardDriver : Driver
{
    public const string Key = "kbd";

    private readonly DeviceMatch[] _matches = [SyntheticMatch.Key(Key)];

    public override string Name => "synthetic-keyboard";

    public override ReadOnlySpan<DeviceMatch> Matches => _matches;

    public override ProbeResult Probe(DeviceBinding binding)
    {
        if (binding.Node.Resources.Count == 0 || binding.Node.Interrupts.Count == 0)
        {
            return ProbeResult.Declined("no register window or no interrupt source");
        }

        RegisterWindow window = binding.MapRegisters(0);
        DeviceEvent keyEvent = binding.CreateEvent();
        KeyboardState state = new(binding, window, keyEvent);
        binding.DriverState = state;

        state.Sink = binding.PublishKeyboard(state);

        if (!binding.TryRequestInterrupt(binding.Node.Interrupts[0], state.OnInterrupt, out InterruptHandle? handle))
        {
            return ProbeResult.Failed("interrupt 0 could not be connected");
        }

        state.Handle = handle;

        // The handler is live from here on, so the device is armed last.
        window.Write8(KeyboardState.ControlOffset, KeyboardState.EnableBit);
        return ProbeResult.Bound;
    }

    public override void OnDetach(DeviceBinding binding, DetachReason reason)
    {
        if (reason.HardwarePresent && binding.DriverState is KeyboardState state)
        {
            state.Window.Write8(KeyboardState.ControlOffset, 0);
        }
    }
}
```

The state object implements the device contract, owns the handler, and holds every kit object the probe acquired:

```csharp
using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Devices;

namespace MyOS.Drivers;

public sealed class KeyboardState : IKeyboard
{
    public const int ControlOffset = 0x00;
    public const int ScanCodeOffset = 0x10;
    public const int FlagsOffset = 0x11;
    public const byte EnableBit = 1;
    public const byte ReleasedFlag = 1;

    private readonly DeviceBinding _binding;
    private KeyboardLeds _leds;

    public KeyboardState(DeviceBinding binding, RegisterWindow window, DeviceEvent keyEvent)
    {
        _binding = binding;
        Window = window;
        KeyEvent = keyEvent;
        KeyWork = binding.CreateWorkItem(OnKeyWork);
    }

    public string Name => "synthetic-kbd";

    public RegisterWindow Window { get; }

    public DeviceEvent KeyEvent { get; }

    public WorkItem KeyWork { get; }

    public KeyboardSink? Sink { get; set; }

    public InterruptHandle? Handle { get; set; }

    public void SetLeds(KeyboardLeds leds) => _leds = leds;

    // Interrupt context: reads two registers, reports the key, wakes the
    // thread and hands the rest to the work item. Allocates nothing.
    public void OnInterrupt(InterruptContext context)
    {
        byte scanCode = Window.Read8(ScanCodeOffset);
        bool released = (Window.Read8(FlagsOffset) & ReleasedFlag) != 0;
        Sink?.Report(scanCode, released);
        context.Signal(KeyEvent);
        context.Schedule(KeyWork);
    }

    // Thread context, on the kit worker.
    public void OnKeyWork()
    {
        _binding.Log("key work ran");
    }
}
```

Note what the two classes never do: allocate a page, program an interrupt controller, register the keyboard with a manager, or free anything. The sections below take the binding's members one at a time.

### Register windows

`binding.MapRegisters(resourceIndex)` maps one entry of `Node.Resources` as registers and returns a `RegisterWindow`: `Read8` to `Read64`, `Write8` to `Write64`, each one access of that width at a byte offset, in program order, with the barriers the architecture needs inside the kit. A write is preceded by a DMA write barrier and a read is followed by a DMA read barrier, so a doorbell write lands after every earlier store to DMA memory and no later load runs ahead of a status register. Offsets are checked against the window's `Length` and the access width's alignment, in every build.

The same driver code works over every resource kind. A `MemoryWindow` is mapped as device memory; a `RamWindow` (what a synthetic node carries) is reached through the kernel's own mapping; a `PortRange` on x64 becomes `in` and `out`, and has no 64-bit access. `MapRegisters` throws `PlatformNotSupportedException` for a port range on ARM64, `InvalidOperationException` for a window that overlaps the kernel heap or cannot be mapped, and `ArgumentOutOfRangeException` for an index the node does not have.

The accessors neither allocate nor block, so an interrupt handler may use them. After teardown every access throws, so a driver still holding the window gets an exception instead of writing to a device that is no longer its own.

### Bulk regions

`binding.MapRegion(resourceIndex, RegionCaching)` maps a resource as memory rather than registers and returns a `DeviceRegion`: a framebuffer, a command queue, a descriptor area. Where a register window checks and orders every access, a region is memory: `Span` fills it, `As<T>()` views it as a span of unmanaged structs, and `Pointer` reaches what a span cannot express (a region longer than `int.MaxValue`). `Length` and `Caching` say what was mapped.

```csharp
DeviceRegion framebuffer = binding.MapRegion(1, RegionCaching.WriteCombining);
framebuffer.Span.Fill(0);
Span<uint> pixels = framebuffer.As<uint>();
pixels[0] = 0x00FF0000;
```

`RegionCaching` names what the driver wants: `Device` (uncached, every access reaches the device in order), `WriteCombining` (stores gathered into bursts, for framebuffers) or `Normal` (cacheable, for coherent RAM). Today a memory window is mapped as device memory whatever is asked, until a platform offers write-combining; the value asked for is recorded on the region. A port range cannot be mapped as a region (`ArgumentException`). A region orders nothing on its own: what needs ordering against a doorbell is ordered with the DMA barriers below.

### DMA memory

`binding.AllocateDma(length, alignment)` returns a `DmaBuffer`: zeroed, physically contiguous pages the device can address, with `PhysicalAddress` for the device, `Span` for the CPU and `Length` as requested. An alignment below a page is raised to the page; a larger one is honoured. `TryAllocateDma(length, alignment, constraints, out buffer)` is the same for a device with addressing limits: `DmaConstraints.Addressable32Bit` asks for a buffer that ends below 4 GiB, and the method returns `false` rather than throwing when the allocator cannot meet it, so the driver can decline.

```csharp
if (!binding.TryAllocateDma(RingBytes, 4096, DmaConstraints.Addressable32Bit, out DmaBuffer? ring))
{
    return ProbeResult.Declined("no DMA memory below 4 GiB");
}
```

DMA is coherent on the machines the kernel runs on, so only ordering is needed, and `DmaBuffer` carries the two barriers as static methods. `DmaBuffer.WriteBarrier()` goes between filling a descriptor and the store that hands it to the device; `DmaBuffer.ReadBarrier()` goes between reading a flag the device wrote and reading the data the flag guards. Both are no-ops on x64 and real fences on ARM64, and both are safe from a handler.

```csharp
Span<byte> descriptors = ring.Span;
descriptors[8] = 0x01;                   // fill the descriptor
DmaBuffer.WriteBarrier();
descriptors[0] = OwnedByDevice;          // then hand it over

// A register write carries its own barrier, so a doorbell needs none:
window.Write32(DoorbellOffset, 1);
```

Managed arrays are not DMA memory: the pinned heap is collected when nothing references an array, and a device holds no reference. A buffer is freed by teardown, after which `Span` throws.

### Interrupts

`binding.TryRequestInterrupt(source, handler, out handle)` connects an `InterruptHandler` to one of `Node.Interrupts`, routes it through whatever the platform has, and returns `false` when the platform cannot deliver it, so the driver declines or falls back to polling instead of silently never firing. The `source` must belong to the binding's own node (`ArgumentException` otherwise). Nothing is recorded on a `false`.

The handler is live from the moment the call returns `true`, not from the moment `Probe` returns `Bound`. That is deliberate: `Probe` runs in thread context, so a driver can connect its handler, issue a command, and wait on an event the handler signals, which is how a real driver brings a device up. The two guarantees that matter: the handler is connected before the call returns, so an interrupt the device raises while `TryRequestInterrupt` is still returning is delivered rather than lost; and on decline, failure or removal the source is masked and disconnected right after the published devices are withdrawn, before events are cancelled, threads joined or memory released, so a handler never touches memory the teardown has freed. A driver that must not be interrupted until it is ready arms the device last, as the sample does.

The `InterruptHandle` masks and unmasks from any context (`Mask()`, `Unmask()`, `IsMasked`) and names its `Source`. From inside the handler, `context.Mask()` is the same mask, for a level-triggered device whose condition a thread will clear; the thread unmasks through the handle. A handler that throws leaves its source masked, with the fault recorded on the node; the driver may unmask it again once it has looked.

### Events and waiting

`binding.CreateEvent()` returns a `DeviceEvent`: a counted signal between a handler and a thread. A handler signals it, through `context.Signal(evt)` or `evt.Signal()`; a thread waits through `binding.Wait(evt, timeoutMilliseconds)`, which is the only way to wait, so a handler holding the event cannot block on it. `Wait` returns `true` when a signal was consumed, `false` on timeout, and `false` at once and forever once teardown began (`evt.IsCancelled`).

The binding also carries `DetachEvent`, cancelled first among the events once teardown reaches them (step 5 below), so a thread that waits on it, or on any event of the binding, wakes, finds `IsDetaching` true and returns.

Two more thread-context waits live on the binding: `Delay(microseconds)` busy-waits on the platform's calibrated source, for a register that needs a moment, and `Sleep(milliseconds)` gives up the CPU (a scheduler sleep on a driver thread or the worker, a busy wait without a scheduler).

### Work items

`binding.CreateWorkItem(callback)` returns a `WorkItem` whose callback runs in thread context on the kit worker when scheduled. Scheduling it, from a handler through `context.Schedule(item)` or from anywhere through `item.Schedule()`, is allocation-free, because the item owns its queue entry; it is queued at most once at a time, and `Schedule` returns `false` when it is already queued, was cancelled by teardown, or no worker runs in this kernel. A work item that throws is logged (`work item threw`) and cancelled. Create work items in `Probe`, as the sample does in the state's constructor, and schedule them later.

### Periodic work

Polling is a driver's own fallback, and it is spelled as deferred work: `binding.TrySchedulePeriodic(intervalMilliseconds, item)` runs a work item of this binding at that interval, from the platform timer, until teardown. The interval is rounded to the timer's tick, which is tens of milliseconds on some platforms. It returns `false` when the kernel has no platform timer (`CosmosEnableTimer` off) or no worker (`CosmosEnableScheduler` off). The kit does not silently poll on a driver's behalf, because a driver that does not know it is being polled cannot size its rings for it.

With an `OnPoll` method on the state object:

```csharp
WorkItem poll = binding.CreateWorkItem(state.OnPoll);
if (!binding.TrySchedulePeriodic(20, poll))
{
    return ProbeResult.Failed("no timer to poll with");
}
```

### Driver threads

`binding.TryStartThread(name, entry, out thread)` starts a thread of the driver's own, for work that must wait rather than run to completion on the worker. It returns `false` when the scheduler is not running or the thread did not start. The body's contract is fixed: loop on the binding's events, and return once `IsDetaching` is true. Teardown cancels every event, so a waiter wakes, and then joins the thread with a bounded wait of 500 ms; a thread that outlives it is logged, and the memory it may still touch is leaked on purpose rather than handed to someone else (`LeakedResourceCount` on the node says how much).

With a `ThreadMain` method on the state object:

```csharp
public void ThreadMain()
{
    while (!_binding.IsDetaching)
    {
        if (_binding.Wait(KeyEvent, 50))
        {
            // a key arrived; drain the device in thread context
        }
    }
}
```

```csharp
if (!binding.TryStartThread("kbd-thread", state.ThreadMain, out DriverThread? thread))
{
    return ProbeResult.Failed("no scheduler to run the drain thread on");
}
```

A `DriverThread` exposes its `Name` and `HasExited`. A driver thread cannot retract its own node (the teardown would wait to join the thread that is waiting on the teardown); the kit refuses that with an exception.

### Device locks

`binding.CreateLock()` returns a `DeviceLock`, for a device that is entered from more than one context at once: a `Transmit` the ring calls from its own thread while the driver's work item, delivering a frame, re-enters it through the stack's synchronous reply; a handler and a thread sharing a register sequence. It is created in thread context and recorded on the binding: it holds no resource, does not count in `HeldResourceCount` and needs no release. `Acquire()` returns a `DeviceLockScope`, a `ref struct` a `using` binds to; the holder runs with interrupts disabled until the scope is disposed, which restores the interrupt state the acquire found, so a handler can never spin on a thread that holds the lock and one scope nests inside another. It is not reentrant (a holder that acquires again spins forever), and it is never held across `Sleep`, `Wait`, `Delay`, a sink call or a publish. The E1000E's `Transmit` runs under its lock, and its drain takes the lock only around its own ring index update, never across a `Receive`:

```csharp
public bool Transmit(ReadOnlySpan<byte> frame)
{
    using (_lock.Acquire())
    {
        // check the ring, copy the frame, write the descriptor,
        // DmaBuffer.WriteBarrier(), then advance the tail register
    }

    return true;
}
```

### Publishing a device

A device kind is the smallest interface the kernel needs, plus a kit-owned **sink** through which the driver pushes events. Lifecycle is not on the interface, because the binding owns lifecycle; publish and withdraw are enable and disable.

| Kind | The driver implements | `Publish...` returns | The driver reports through |
|------|-----------------------|----------------------|----------------------------|
| Keyboard | `IKeyboard { string Name; void SetLeds(KeyboardLeds); }` | `KeyboardSink` | `Report(scanCode, released)` |
| Pointer | `IPointer { string Name; }` | `PointerSink` | `ReportRelative(deltaX, deltaY, buttons, wheel)`, `ReportAbsolute(x, y, buttons)` |
| Network interface | `INetworkInterface { string Name; MACAddress MacAddress; bool LinkUp; bool Transmit(ReadOnlySpan<byte>); }` | `NetworkSink` | `Receive(frame)`, `LinkChanged(up)` |
| Block device | `IBlockDevice`, the existing public contract; the storage manager registers it and scans its partitions on publish | nothing | nothing to report |
| Display | `IDisplay { string Name; DisplayMode Mode; DeviceRegion? Framebuffer; void Flush(x, y, width, height); }` | `DisplaySink` | `ModeChanged()` |

`binding.PublishKeyboard(keyboard)`, `PublishPointer`, `PublishNetwork`, `PublishBlockDevice` and `PublishDisplay` are the five calls. Every sink is allocation-free; it finds the kernel's consumer for its kind at call time and drops the report when the device was withdrawn or nobody listens. A kernel built without a kind therefore gets a sink that discards rather than a throw the driver could not anticipate. The published device is withdrawn by teardown ahead of everything else the driver holds, and the log records both ends:

```
[Drivers] synthetic:kbd synthetic-keyboard published keyboard "synthetic-kbd" (consumed)
[Drivers] synthetic:kbd synthetic-keyboard withdrew keyboard "synthetic-kbd"
```

A display driver publishes its device under `IDisplay.Name` (`virtio-gpu`, `vmware-svga`; the firmware display is `framebuffer`) in whatever scanout state it found it and programs no mode of its own: `Mode` is the geometry the scanout is in, or `DisplayMode.IsEmpty` while nothing has programmed one, and `Framebuffer` is a `DeviceRegion` the driver mapped (a BAR window sliced to one frame, or a `DmaBuffer.Region` when the host scans out of DMA memory) or `null` for a display the CPU cannot draw into. Two optional facets are extra interfaces the same published object implements, found by a type test through `DisplayDevice.TryGetFacet<T>()`: `IDisplayModes` (`Modes`, the list the display accepts, and `TrySetMode(width, height, bitsPerPixel)`, which reports `ModeChanged()` on success and returns `false`, changing nothing, for a mode the display refuses) for a display that switches modes, and `IHardwareCursor` (`TryDefine(hotspotX, hotspotY, width, height, pixels)` for a premultiplied ARGB image, `Set(x, y, visible)`) for one that composes a cursor itself. A third facet is the ring's, not the kit's: a display that renders 3D implements `ICanvas3DFactory` from `Cosmos.Kernel.System.Graphics`, and `Canvas.GetFullScreen()` asks the primary display for it before building a canvas. A driver's own state type is a facet too, which is how the Graphic suite reads a driver's counters back.

The framebuffer the bootloader handed over is a display like the others, the **firmware display**: the HAL records it at boot (`[KERNEL]   - Recording the firmware framebuffer...`, under `CosmosEnableGraphics` alone), and the engine publishes it right after the manifest line, before its worker exists and before any node is offered, with `DeviceProvenance.Firmware`, no binding and no node, logged `firmware published display "framebuffer" (consumed)`. It is withdrawn by the **retirement rule**: when a driver binds a PCI function, every firmware display whose physical address lies inside one of that function's assigned memory base address registers is withdrawn, logged `firmware display "framebuffer" retired: inside pci:0000:00:01.0 bar 1`, because the driver now owns the adapter the firmware framebuffer sits in. That is what happens on a VMware SVGA II adapter, whose VRAM window is BAR 1; a virtio-gpu added beside the machine's default adapter retires nothing, and both displays stay published.

`DisplayManager` in `Cosmos.Kernel.System.Graphics` is the display kind's manager: `Count`, `Primary` and `TryGet(index, out display)` over a list kept in primary order, each entry a `DisplayDevice` carrying the published `Name`, the `DriverName` (the driver's class name, or `firmware`), `NodePath` (null for the firmware display), `IsFirmware`, the mode read live from the driver (`Width`, `Height`, `BitsPerPixel`, `Pitch`, `RefreshRate`, 60 when the driver reports none, all 0 once withdrawn), `IsWithdrawn` and `TryGetFacet<T>`, which finds nothing once the display is withdrawn. The primary rule is stable and never arrival order: a driver display before the firmware one, among driver displays the smallest `NodePath` by ordinal comparison, among firmware displays the first published. The choice is logged whenever it changes, `[Display] primary: firmware "framebuffer" (the only display)` at boot, `[Display] primary: VirtioGpuDriver "virtio-gpu" (driver published, preferred over firmware)` once a driver display arrives, `[Display] primary: none` when the last one leaves. `Canvas.GetFullScreen()` draws on the primary display: when it implements `ICanvas3DFactory` the canvas is the factory's `Canvas3D`, otherwise the framework `Canvas` over the display, which asks for the default mode through `IDisplayModes` when the display reports none and copies its buffer into `Framebuffer` on every `Display()`, followed by `Flush`. A display that publishes no `Framebuffer` receives no pixels and no `Flush` from the framework canvas: the kind has no upload call, so such a device presents only through its own `Canvas3D` via `ICanvas3DFactory`, or is listed by the manager and drawn on by nobody. The [Graphics](graphics.md) article has the canvas side.

Keyboard, pointer, network, display and block have consumers, each installed by its manager's `Initialize` through `DeviceRegistry.SetConsumer` before the driver stage runs, one consumer per kind. A published `IKeyboard` is in `KeyboardManager`'s list by the time `PublishKeyboard` returns, beside the platform's PS/2 and USB keyboards, and its keys reach the manager the way theirs do: `KeyboardSink.Report(scanCode, released)` runs the manager's scan code handler in the caller's context, an interrupt included, allocating nothing. A lock key toggled on any keyboard lights the indicators on every published keyboard through `IKeyboard.SetLeds`, from a work item on the kit worker, since `SetLeds` is thread context and the toggle may come from a PS/2 or USB interrupt. A published `IPointer` is in `MouseManager`'s list; `PointerSink.ReportRelative` moves the manager's cursor and sets its buttons as a PS/2 packet does, and `ReportAbsolute` sets the buttons and leaves the cursor where it was, the migration-period mapping of an absolute device onto a manager that only knows movement. A published `INetworkInterface` is an adapter in `NetworkManager`'s table under the interface's `Name` (`e1000e` and `virtio-net` for the shipped drivers, which is what `NetworkManager.Name` reports when that interface is the primary), and it leaves the table through `NetworkManager.UnregisterDevice` when teardown withdraws it. The first registered device is the primary: the first interface a kit driver publishes, in the order the driver stage binds them. Two rules come with the network kind. `NetworkSink.Receive` is called from thread context, the kit worker in practice, and never from an interrupt handler: the consumer copies the frame into an array and runs the stack's handler with interrupts disabled, which restores the atomicity the stack had while it ran inside the driver's interrupt handler, so a driver whose handler sees a frame hands it to a work item, as the E1000E's drain does. `LinkChanged` only writes a flag and may be reported from any context. Withdrawing a keyboard or a pointer takes it out of its manager's list at once; withdrawing an interface takes it out of the manager's table, but its IP configuration is not removed and `NetworkAdapter` handles stay positional, so a handle taken before the withdrawal may name the device that moved into its slot. A published `IDisplay` is in `DisplayManager`'s list by the time `PublishDisplay` returns, wrapped in a `DisplayDevice`, and the primary is recomputed; withdrawing it, by teardown or by the retirement rule, drops it from the list and recomputes the primary again, and a canvas still holding it copies nothing from then on. Publication and withdrawal run in thread context, on the boot thread for the firmware display and on the kit worker for a driver's, never concurrently. `DisplaySink.ModeChanged` runs in the caller's context, an interrupt included, and writes `[Display] mode changed: VmwareSvgaDriver "vmware-svga" now 1024x768x32` allocation-free; the canvas reads the new mode when it next draws. A published `IBlockDevice` is in `StorageManager`'s tables by the time `PublishBlockDevice` returns: the consumer registers it inside the publishing probe, on the kit worker, and the manager scans it for partitions there (GPT, then MBR with its EBR chain, then the FAT superfloppy probe), reading a device the driver has just made operational, so the scan's I/O lengthens the driver stage by its duration; the manager then writes `[StorageManager] sata0 registered by AhciDriver`, with ` (primary)` when its order rule put the disk first at that moment. That rule keeps `Devices`, `Partitions` and `PrimaryDevice` in one order whatever the arrival order: a kit disk before a hand-registered one (the USB mass storage units the HAL's USB stack still registers by hand), among kit disks the lowest node path by ordinal comparison, then registration order; so an internal disk is the primary over a USB stick present at boot, and a kit disk arriving after a USB stick moves ahead of it and renumbers `Partitions`, which is why a mount by `Partition` keeps its partition where a mount by index string keeps its index ([File System](filesystem.md)). A registration the manager refuses (its table of eight is full, the device is already registered, the manager is not initialized) is an `InvalidOperationException` from the consumer, so the device stays unconsumed and the publishing probe fails with that reason in its offer line, rather than binding a disk the ring never sees. Withdrawing a block device, from the binding's teardown before the driver's interrupts are disconnected and before `OnDetach`, unregisters it: it leaves `Devices` and `Partitions`, the mounts made on its partitions are detached without a flush, and the manager writes `[StorageManager] sata0 unregistered`, with ` (primary now nvme0n1)` or ` (no primary)` when it was the primary. The ring does not wrap the driver's object, so a later call through a `Partition` of a withdrawn kit disk gets the kit's `InvalidOperationException` (a window or DMA buffer torn down) or the driver's own detach exception, not `IOException`. A PCI function is never retracted today, so that path is reached only over the synthetic bus, as the Drivers suite does.

### Publishing child nodes

A bus is a driver whose binding publishes child nodes. `binding.PublishChild(identity, resources, interrupts, access)` puts a device the driver found on its own bus into the tree beneath its node, to be offered to drivers like any other; `binding.RetractChild(child, hardwarePresent)` takes one out again, tearing down whatever binding it has. Children live and die with the parent: tearing down a bus driver retracts every node it published, which in turn tears down every binding on those nodes, leaf first, and each child's driver sees `DetachCause.ParentRetracted`.

From the worker (a probe or a work item) the child's offer is queued behind the current job and runs once it returns; from a driver thread it completes before `PublishChild` returns. A bus driver brings its own identity and match pair, as the Drivers suite's bus driver does:

```csharp
using Cosmos.Kernel.HAL.DriverKit;

namespace MyOS.Drivers;

public sealed class ChildIdentity : DeviceIdentity
{
    public ChildIdentity(string name)
    {
        Name = name;
    }

    public string Name { get; }

    public override string BusName => "mybus";

    public override string Address => Name;

    public override string Describe() => string.Concat("child ", Name);
}

public sealed class ChildMatch : DeviceMatch
{
    private readonly string _name;

    public ChildMatch(string name)
    {
        _name = name;
    }

    public override int Specificity => 1;

    public override bool Matches(DeviceIdentity identity) =>
        identity is ChildIdentity child && string.Equals(_name, child.Name);
}
```

```csharp
public override ProbeResult Probe(DeviceBinding binding)
{
    DeviceNode child = binding.PublishChild(new ChildIdentity("port0"), [], [], null);
    binding.DriverState = child;
    return ProbeResult.Bound;
}
```

A bus driver may also publish a child of a bus kind the kit already defines: the shipped virtio transport drivers publish their device with the kit's `VirtioIdentity`, no resources, the nine interrupt sources of a `VirtioAccess` and that access as the access object ([Virtio devices](#virtio-devices)).

The `resources` array carries `DeviceResource.MemoryWindow(physicalBase, length)`, `DeviceResource.PortRange(basePort, count)`, `DeviceResource.RamWindow(virtualBase, physicalBase, length)` or `DeviceResource.None` entries (the last is a slot with nothing assigned, kept so the indices after it stay what the hardware numbers them; `MapRegisters` and `MapRegion` refuse it with `InvalidOperationException`); the `interrupts` array carries `InterruptSource` implementations the bus provides, each implementing `Describe()` and the four protected members (`TryConnectCore`, `MaskCore`, `UnmaskCore`, `DisconnectCore`) that the kit calls through its own internal forwarders, so a driver holding a source can neither connect nor mask it behind the kit's back.

### Logging

`binding.Log(message)` writes one line to the serial log in the kit's format, `[Drivers] synthetic:kbd synthetic-keyboard: message`, from thread context. The log is where a driver author looks first; the same facts reach a test through `DriverInfo` (below).

## Declining and failing

`ProbeResult` is an outcome the caller must tell apart, so it is a value with a kind and a reason, not a bool and not an exception:

| Result | Meaning | What the kit does |
|--------|---------|-------------------|
| `ProbeResult.Bound` | The driver took the device | Keeps the binding and everything acquired through it |
| `ProbeResult.Declined(reason)` | The driver looked and does not want it | Unwinds the ledger, records the reason, offers the next candidate |
| `ProbeResult.Failed(reason)` | The driver wanted it and could not bring it up | The same, recorded as failed |
| An exception escaping `Probe` | Treated as `Failed` with the exception's message | The same |

In every case but `Bound` the kit releases everything in the ledger in the same fixed order as a teardown (below), without the `OnDetach` hook, before offering the node to the next candidate. A work item the probe had scheduled is taken out of the queue; a handler it had connected is disconnected; a device it had published is withdrawn. The offer records how many kit resources the probe had acquired (`ReleasedResourceCount`), and the total the kit holds returns to what it was. The Drivers suite has one driver that acquires a window, a DMA buffer, an event and a work item and then declines, and one that throws, to assert both paths:

```
[Drivers] synthetic:decline offer DecliningDriver -> declined: declined on purpose
[Drivers] synthetic:decline offer AnyKeyDriver -> bound
[Drivers] synthetic:throw offer ThrowingDriver -> failed: probe threw on purpose
```

Decline when the device is not one the driver serves after all (a revision it does not know, a feature set it cannot negotiate); fail when it is and bring-up did not work. Both reasons are short strings for the log and the diagnostics view.

## Teardown and OnDetach

Removal, whether a bus retracting a node, a parent binding going away, or a test retracting a synthetic node, unwinds the ledger in this fixed order:

1. The `IsDetaching` flag is set, so nothing new can be acquired: every acquiring member of the binding throws from here on.
2. Child nodes are retracted, recursively; leaf bindings go first.
3. Published devices are withdrawn: the consumer, when there is one, is told, and every sink goes quiet.
4. Interrupt handles are masked at the controller and disconnected; no handler of this binding runs again.
5. `DetachEvent` is cancelled, periodic work is unregistered, work items are cancelled and taken out of the queue, every other event is cancelled, and driver threads are joined with the 500 ms bound.
6. `Driver.OnDetach(binding, reason)` runs.
7. Memory is released in reverse order of acquisition: DMA buffers freed, regions and windows invalidated. When a thread did not stop, this step is skipped and the node's `LeakedResourceCount` says how much stays allocated.

A step that throws is logged (`teardown step "withdraw" threw`) and the walk goes on, so a misbehaving consumer never leaves a handler connected or memory held.

`OnDetach` is the one place a driver may quiesce hardware on the way out, and it runs at the one moment that is safe: interrupts are disconnected and threads are stopped, and the windows are still valid. `DetachReason` says why (`Cause`: `DetachCause.Retracted` for the bus taking the node itself, `DetachCause.ParentRetracted` for a parent going away) and whether the hardware is still there (`HardwarePresent`). When `HardwarePresent` is `false`, as for a hot-unplug, register writes would fault or reach another device, so the driver must not touch them; the sample's `OnDetach` checks the flag before writing. Nothing runs for the driver afterwards, and `binding.DriverState` is where it finds its own objects.

```
[Drivers] synthetic:kbd synthetic-keyboard withdrew keyboard "synthetic-kbd"
[Drivers] synthetic:kbd retracted
```

## Buses

Three bus kinds carry real hardware today, and they nest: the **platform bus** holds the root nodes a machine description seeds because nothing enumerates them (the PCI host, and on ARM64 the virtio-mmio slots); the **PCI host driver**, bound to the host node, publishes the functions it finds as PCI nodes beneath it; and a **virtio transport driver**, bound to a virtio PCI function or a virtio-mmio slot, publishes the device behind it as a virtio node beneath that. A driver for a PCI device sees neither mechanism; it is offered a `pci:` node like any other. A driver for a virtio device is offered a `virtio:` node and never learns which transport carries it.

### Platform nodes

A platform node is named the way a device tree names it. `PlatformIdentity(address, compatible)` takes the node's address in `name@hex` form (`pci@cf8`, `pci@3f000000`) and at least one compatible string, most specific first; `BusName` is `platform`, so the path is `platform:pci@cf8`, and `Describe()` prints `compatible ` followed by the strings joined with commas. The strings are copied, exposed as `Compatible` and compared ordinally. `PlatformMatch` has two shapes: `PlatformMatch.Compatible("pci-host-legacy")` matches every platform node whose list contains the string (specificity 1), and `PlatformMatch.Any()` matches every platform node (specificity 0).

Only a machine description publishes platform nodes; `PlatformBus.Publish` is internal to the HAL, because the arch HAL assembly is where a machine is described. Each `IPlatformInitializer` implements `PublishPlatformNodes`, which the HAL library initializer calls right after `InitializeHardware`, with interrupts still disabled and inside a `try`/`catch` that logs `Platform nodes not published: message` and lets the boot go on. The nodes wait in the engine's queue until `Kernel.Start` runs the driver stage. What the two descriptions publish, when `CosmosEnableInterrupts` is on (the PCI host also needs `CosmosEnablePCI`; the virtio-mmio nodes need no other switch):

| Machine | Node | Resources | Interrupts | Access object |
|---------|------|-----------|------------|---------------|
| x64 | `platform:pci@cf8`, compatible `pci-host-legacy` | `PortRange(0xCF8, 8)`, documentary: the access object reaches the ports itself | none | `PciHostAccess.ForPorts(0, 0, 255)`: segment 0, buses 0 to 255 over the `0xCF8`/`0xCFC` mechanism. q35's MCFG is not used on x64 |
| ARM64 | `platform:pci@<base>`, the MCFG base in hex, compatible `pci-host-ecam-generic` | `MemoryWindow(base, length)` over the buses the host serves, one megabyte per bus | none | `PciHostAccess.ForEcam(base, segment, startBus, endBus)`: the ECAM window, mapped as device memory |
| ARM64 | `platform:virtio_mmio@<base>`, the slot base in hex, compatible `virtio,mmio`, one per occupied slot | `MemoryWindow(base, 0x200)`, the slot's registers | the slot's GIC line, `line 48` for slot 0 | none |

`ForEcam` maps the window bus by bus from the start bus up and ends the host's range at the last bus it could map: the buses before are served, the rest are logged as `pci host at 0x...: buses xx to yy not mapped, enumeration ends at bus zz`, and a window whose first bus cannot be mapped is an `InvalidOperationException` that the initializer's `catch` turns into the log line above. An ARM64 machine without an MCFG entry logs `No MCFG entry: no PCI host node published` and publishes no host, so the driver stage finds none there and no PCI driver is offered a node; its virtio-mmio nodes are published all the same.

The ARM64 description walks the 32 slots of the virt machine's virtio-mmio window, `0x0a000000` on, `0x200` bytes each, whatever the feature switches say and whether or not ACPI described anything: the window is the machine's fixed table, not an ACPI node, so an `acpi=off` boot and a kernel with PCI compiled out keep their MMIO devices. A slot whose magic register reads `virt` and whose device id is not 0 is published as `platform:virtio_mmio@a003e00` with one resource, its register window, no access object, and one interrupt source, the slot's line at the GIC (SPI 16 + slot, INTID 48 + slot), which `Describe()` prints as `line 48`. Empty slots get no node; the transport driver checks the slot again when it binds. The line is only described here and connected by the driver that binds the node: `TryRequestInterrupt` on it installs the handler and then enables the line at the GIC, level-triggered, the handler first so a line already asserted fires into it, and returns `false` when the interrupt controller is not initialized, when another handler already holds the line, or when the source is already connected. `Mask` and `Unmask` disable and enable the line at the controller.

The host node's access object is a `PciHostAccess`: `Segment`, `StartBus` and `EndBus`; `ReadConfig8`, `ReadConfig16`, `ReadConfig32` and `WriteConfig8/16/32` taking `(bus, device, function, offset)`, for raw configuration access from any context (`ArgumentOutOfRangeException` for a bus outside the host's range, a device past 31, a function past 7, or an offset past the mechanism's 256 or 4096 bytes); and `TryDescribeFunction(bus, device, function, out PciFunctionDescription)`, thread context, which returns `false` for a bus outside the range or a vendor id reading `0xFFFF` or `0x0000`, and otherwise reads the header, sizes the base address registers and hands back the four arguments of `PublishChild` as `Identity`, `Resources`, `Interrupts` and `Access`.

### The PCI host driver

`PciHostDriver` in `Cosmos.Kernel.Drivers` is `[Driver(Feature = DriverFeature.Pci)]` and matches `PlatformMatch.Compatible("pci-host-ecam-generic")` and `PlatformMatch.Compatible("pci-host-legacy")`. Its probe asks the node for its `PciHostAccess` through `binding.Node.TryGetAccess` (and declines with `the node carries no PCI host access` otherwise), then walks the buses as the HAL's legacy scan does: the host's start bus first; on each bus, devices 0 to 31, function 0 and, when the header type has the multi-function bit, functions 1 to 7; every function `TryDescribeFunction` accepts is published with `binding.PublishChild(d.Identity, d.Resources, d.Interrupts, d.Access)`. A PCI-to-PCI bridge (class `06`, subclass `04`, type 1 header) queues its secondary bus, read at offset `0x19`; on the legacy host, a host bridge (class `06`, subclass `00`) at function N of device `00:00` queues bus N. Each bus is walked once and only within the host's range. The probe logs one line, `N functions on M buses`, and returns `Bound`; the children are offered once it returns, and the driver stage does not return before all of them have been. The driver keeps no state and has no `OnDetach`: the kit retracts every child with the host node.

## PCI devices

A PCI node is what the host driver publishes for one function: an identity read from the header, six resources that are its base address registers, its interrupt sources, the legacy line first and then one per described message, one per entry of the function's MSI-X table up to 32 ([Message interrupts](#message-interrupts)), and a `PciAccess` for everything else. The shipped `E1000EDriver` is the model for a driver over one, and the snippets below are its steps.

### Identity and match

`PciIdentity` is the header as the host read it, once: `Segment`, `Bus`, `Device`, `Function`; `VendorId`, `DeviceId`; `SubsystemVendorId` and `SubsystemId` (type 0 headers only, zero otherwise); `ClassCode`, `Subclass`, `ProgIf`, `Revision`; and `HeaderType`, 0 for a device, 1 for a PCI-to-PCI bridge, 2 for a CardBus bridge, with the multi-function bit stripped. `BusName` is `pci` and `Address` is `ssss:bb:dd.f`, all hexadecimal, so the path is `pci:0000:00:03.0`. `Describe()` prints `8086:10d3 class 02.00.00`: vendor and device id, then class, subclass and programming interface, which is what `DeviceNodeInfo.Description` shows for the node.

`PciMatch` has one constructor with eight optional fields, `vendorId`, `deviceId`, `subsystemVendorId`, `subsystemId`, `classCode`, `subclass`, `progIf` and `revision`: every field given must equal the function's, every field left `null` is not looked at, and the specificity is the number of fields given.

```csharp
using Cosmos.Kernel.HAL.DriverKit.Pci;

private readonly DeviceMatch[] _matches =
[
    new PciMatch(vendorId: 0x8086, deviceId: 0x10D3),   // one chip: specificity 2
    new PciMatch(classCode: 0x02),                      // every network controller: specificity 1
];
```

A match with no field set throws `ArgumentException` (`a PCI match constrains at least one field`). That is deliberate: the kit quiesces a function before the first driver looks at it (below), and until the HAL's USB host controller driver moves into the kit, a catch-all driver would quiesce the function it operates. Prefer the chips a driver was tested on to a class match for the same reason; the shipped E1000E matches six Intel device ids and nothing else. The one vendor-wide match in the tree is the virtio PCI transport's `new PciMatch(vendorId: 0x1AF4)`, because every virtio function shares the transport whatever the device behind it ([The transport drivers](#the-transport-drivers)).

### The access object

`binding.Node.Access<PciAccess>()` is the function's configuration space and what the driver turns on in it. All of it is any context and allocation-free:

- `ReadConfig8`, `ReadConfig16`, `ReadConfig32` and `WriteConfig8/16/32` take a register offset in this function's space; an offset past the mechanism's size (256 bytes over the ports, 4096 over ECAM) or off the access width's alignment is `ArgumentOutOfRangeException`.
- `FindCapability(capabilityId, after = 0)` walks the capability list, from its head or from the entry after `after`, bounded to 48 entries, and returns the capability's offset or 0.
- `EnableBusMastering(bool)`, `EnableMemorySpace(bool)` and `EnableIoSpace(bool)` are read-modify-writes of the Command register under the mechanism's lock. A driver enables what it uses: decoding is what makes the windows answer, bus mastering is what lets the device write to DMA memory.
- `InterruptLine` and `InterruptPin` are registers `0x3C` and `0x3D` as firmware wrote them, read at describe time.
- `IsMsiXCapable` and `MessageInterruptCount` (the MSI-X table size, 0 without the capability) say what the function offers over message-signalled interrupts; the node carries the first `min(MessageInterruptCount, 32)` of them after its line, see [Message interrupts](#message-interrupts).
- `Bars` is the six base address registers as the host sized them (next).

Behind the access object, for the kit only, the function is quiesced and restored around the offers. Before the first probe, when at least one driver is a candidate, the kit snapshots Command (and MSI-X Message Control) and turns bus mastering off, disables the legacy line and, when firmware left MSI-X enabled, disables it with the function mask set. After a probe that declined, failed or threw, the same quiescing runs again before the probe's memory is freed, so a ring the probe armed cannot write into pages the kit is about to release. When every candidate declined, the snapshot is written back, so a function a HAL driver still operates keeps its bus mastering and its MSI-X. After a bound driver's teardown with the hardware present, the function is quiesced once more. A bound driver owns the state from its probe on, and a node no driver matches is not touched at all. A hook that throws is logged `bus hook "quiesce" threw: message`, and when it is the first one the node is left `Unbound` with no offer (`not offered: message`), since a driver must not see a function the bus could not quiet.

### BAR resources

`Node.Resources` of a PCI node always has six entries, one per base address register, sized at describe time. Slot `i` is `DeviceResource.MemoryWindow(base, length)` for an assigned memory register, `DeviceResource.PortRange(base, count)` for an assigned I/O register inside the 16-bit port space (a range running past it is clipped), and `DeviceResource.None` for everything else: a register firmware left at zero, the upper half of a 64-bit register, an I/O register at port zero or above `0xFFFF`, and all six slots of a bridge (header types 1 and 2 are not sized). `Node.Resources.Count` counts every slot, and `MapRegisters` or `MapRegion` on a `None` slot throws `InvalidOperationException` (`resource i is not assigned`), so a driver checks first. `pci.Bars[i]` is the register's own view, a `PciBar`: `Index`, `IsAssigned`, `IsIo`, `Is64Bit`, `IsPrefetchable`, `Base` (a physical address, or the first port) and `Length` (bytes, or ports; 0 when unassigned). The E1000E's first step reads as:

```csharp
PciAccess pci = binding.Node.Access<PciAccess>();
PciBar bar0 = pci.Bars[0];
if (!bar0.IsAssigned || bar0.IsIo || bar0.Length < 0x20000)
{
    return ProbeResult.Declined("BAR0 is not a memory window of 128 KiB");
}

RegisterWindow registers = binding.MapRegisters(0);
pci.EnableMemorySpace(true);
pci.EnableBusMastering(true);
```

Sizing writes all ones to each register and reads the mask back, so for its duration the function decodes neither memory nor I/O while bus mastering stays as it was; it runs with interrupts disabled and the early framebuffer console paused, because the serial log mirrors every byte into a framebuffer whose register may be the one being sized. A 64-bit register is sized with both halves together; a 64-bit register in the last slot is malformed and reported unassigned.

### The legacy line

`Node.Interrupts[0]` of a PCI node is the function's legacy interrupt line, which `Describe()` prints as `line 11` or `line (none)`. `TryRequestInterrupt` on it returns `false` on ARM64, where the line register names nothing; when the register is 0 or `0xFF`; when the line is below 3 or above 15 (the lowest three are the platform's own, and a higher register names no routable input); when the interrupt controller is not initialized; when another handler already holds the line (the PIT, the PS/2 controller or another function: a shared line is refused rather than shared); and when the source is already connected. On success the line is routed as the x64 platform routes an ISA IRQ, edge-triggered and active-high, the handler is installed, and only then is the function's INTx disable bit cleared, so a function whose interrupt is already pending asserts the line after the entry is open. `Mask` and `Unmask` go to the controller; disconnecting clears the handler and sets INTx disable again.

That routing is validated on QEMU and nowhere else: on a real chipset the line register is not the GSI and INTx is level-triggered, so a line that connected may never fire there. A PCI driver therefore pairs the line with a periodic drain rather than trusting it, which is the pattern the E1000E follows:

```csharp
WorkItem drain = binding.CreateWorkItem(state.Drain);
state.DrainWork = drain;

bool hasLine = binding.TryRequestInterrupt(binding.Node.Interrupts[0], state.OnInterrupt, out _);
bool polling = binding.TrySchedulePeriodic(50, drain);
if (!hasLine && !polling)
{
    return ProbeResult.Declined("no interrupt and no timer to poll with");
}
```

The drain is idempotent, so the handler and the timer can both schedule it: the handler acknowledges the device and schedules the drain, the timer runs it every period whatever happened, an edge lost while the line was masked is recovered within a period, and a line that is routed but dead still yields a working device. The E1000E's handler reads the cause register (which clears it), counts the interrupt and schedules the drain when a frame arrived, the ring ran low or the link changed; its probe logs which of the two it got, `line 11` or `no line`, and `polling every 50 ms` or `no polling`.

### Message interrupts

When the function has an MSI-X capability, `Node.Interrupts[1]` on are its message sources, one per table entry for the first `min(MessageInterruptCount, 32)` entries, each of which `Describe()` prints as `message 0 of 4`. A function with a larger table (an NVMe controller advertises up to 2048 entries) is offered its first 32; a driver that needs more is a later extension of the description. `TryRequestInterrupt` on a message source returns `false` when the platform has no message binder (the LAPIC on x64, the GICv3 ITS on ARM64, so the virt machine's default GICv2 routes none); when memory space decoding is off in the function's Command register, because the table lives in a BAR and is not decoded until then, so a driver calls `pci.EnableMemorySpace(true)` before it requests a message, as the E1000E and the virtio transport do; when the BAR holding the table is unassigned or I/O, or cannot be mapped; when the binder cannot route the function or has no slot left; and when the source is already connected. The first connect on a function maps the table, masks every entry, enables the capability and sets the function's INTx disable bit, so the line and the messages are never both live; each connect then programs its entry with the address and data the binder hands out and unmasks it. `Mask` and `Unmask` are one write of the entry's vector control bit, from any context. Disconnecting masks the entry in the table when decoding is on and through the capability's function mask when a driver turned memory space off while still holding the handle, reads the write back, gives the routing slot back, and the last disconnect disables the capability.

The virtio PCI transport is the first shipped driver over the messages: it requests entry 0 on for as many entries as the device and the platform give it, stops at the first refusal, and has no INTx fallback, so a virtio function whose messages cannot be routed is published with no interrupt entry and its leaf driver polls or declines ([Virtio devices](#virtio-devices)).

## Virtio devices

A virtio node is what a transport driver publishes for one virtio device, whichever transport carries it: an identity naming the transport and the device type, no resources, nine interrupt sources, and a `VirtioAccess` that runs the status handshake, the feature negotiation, the virtqueues and the device configuration space once, in the kit, over the transport's registers. A leaf driver such as the shipped `VirtioNetDriver` sees only the access and cannot tell PCI from MMIO; the transport drivers are described [below](#the-transport-drivers), and the snippets in this section are the net driver's steps.

### Virtio identity and match

`VirtioIdentity(transport, transportAddress, deviceType)` is what a transport driver builds: `Transport` is `pci` or `mmio`, `TransportAddress` the transport's own address for the device (a function address, a slot base in hexadecimal) and `DeviceType` a `VirtioDeviceType`, the device ids of the virtio specification (`Network` 1, `Block` 2, `Console` 3, `Entropy` 4, `Balloon` 5, `Scsi` 8, `Gpu` 16, `Input` 18, `Socket` 19, `Crypto` 20, `Sound` 25, `FileSystem` 26; a value outside the list is a type the kit has no name for). `BusName` is `virtio` and `Address` is the transport and its address joined with a colon, so the path is `virtio:pci:0000:00:02.0` or `virtio:mmio:a003e00`: the transport is visible in the path and invisible to the driver. `Describe()` prints `type 1 (network)`, or `type 42 (unknown)` for a type without a name, which is what `DeviceNodeInfo.Description` shows for the node.

`VirtioMatch` has two shapes: `VirtioMatch.DeviceType(type)` matches every virtio device of that type on any transport (specificity 1), and `VirtioMatch.Any()` matches every virtio device (specificity 0). The net driver's table:

```csharp
using Cosmos.Kernel.HAL.DriverKit.Virtio;

private readonly DeviceMatch[] _matches =
[
    VirtioMatch.DeviceType(VirtioDeviceType.Network),
];
```

### The virtio access object

`binding.Node.Access<VirtioAccess>()` is the device. Its members, in the order a probe uses them:

- `DeviceType`; `Version1Negotiated`, true once the negotiation took VIRTIO_F_VERSION_1, which the kit takes whenever the device offers it; and `InterruptEntryCount`, how many interrupt entries the transport delivers: the MSI-X messages it connected over PCI, one for the MMIO line, 0 when the leaf has to poll. Any context.
- `NegotiateFeatures(requestedLow, out negotiatedLow)`, thread context. `requestedLow` is the whole low feature word the driver understands, reserved bits included (VIRTIO_F_ANY_LAYOUT, bit 27, is the leaf's request): the device's offer is masked with it, VERSION_1 is added when offered and nothing else, the result is written back and `negotiatedLow` is its low 32 bits. On a virtio 1.x transport (PCI, MMIO version 2) the kit then sets FEATURES_OK and reads it back, and returns `false` when the bit did not stick; legacy MMIO (version 1) has no such step and only a low feature word, so `Version1Negotiated` stays false there.
- `TryCreateQueue(binding, index, preferredSize, out queue)`, thread context, described under [Queues](#queues); `ConfigInterrupt` and `QueueInterrupt(index)` (`ArgumentOutOfRangeException` at `MaxQueues`, 8, and above), the node's own sources, to pass to `binding.TryRequestInterrupt` ([Virtio interrupt sources](#virtio-interrupt-sources)).
- `ReadConfig8`, `ReadConfig16`, `ReadConfig32` and `WriteConfig8` take an offset in the device-specific configuration space; any context, allocation-free. Over PCI a function without a device configuration structure reads 0 and drops the write.
- `SetDriverOk()` sets DRIVER_OK and `SetFailed()` sets FAILED. `Reset()` writes status 0, waits up to `ResetTimeoutMilliseconds` (100) in steps of `ResetPollMicroseconds` (10) for the device to read it back as 0, then forgets the negotiated version and takes the entry away from every source. A device that never answers is logged `virtio type 1: status did not return to 0 within 100 ms after reset` and treated as reset; the handshake that follows fails visibly. Thread context.
- `Dispatch(entry)` is the transport driver's, not the leaf's: interrupt context, it reads and acknowledges the interrupt status and raises the sources assigned to that entry.

Behind the access object, for the kit only, the device is reset and brought to DRIVER state before the first probe and again after a probe that declined, failed or threw, so every candidate sees a freshly reset device with ACKNOWLEDGE and DRIVER set; a node nobody binds is left reset; and after a bound driver's teardown with the hardware present it is reset once more, after the driver's memory went back. A probe therefore starts at the negotiation:

```csharp
VirtioAccess dev = binding.Node.Access<VirtioAccess>();
if (!dev.NegotiateFeatures(FeatureMac | FeatureStatus | FeatureAnyLayout, out uint features))
{
    return ProbeResult.Failed("the device rejected the feature set");
}

int headerSize = dev.Version1Negotiated ? ModernHeaderBytes : LegacyHeaderBytes;
bool anyLayout = (features & FeatureAnyLayout) != 0;
if (!dev.Version1Negotiated && !anyLayout)
{
    return ProbeResult.Declined("legacy device without VIRTIO_F_ANY_LAYOUT");
}
```

The second result is declined, not failed: the device is healthy, its framing is one the driver does not implement (the net header and the frame share one descriptor, which is conformant with VERSION_1 or with any-layout and never used without one; the legacy split-header framing is not implemented), and the kit resets the device for the next candidate.

### Queues

`dev.TryCreateQueue(binding, index, preferredSize, out Virtqueue? queue)` creates one split virtqueue in DMA memory on the leaf's own ledger (`ArgumentException` when the binding is bound to another node, `ArgumentOutOfRangeException` for a size of 0) and activates it on the device. The size is the smaller of the device's maximum and `preferredSize`; both are powers of two by the specification (QEMU offers 256 or 1024) and the kit does not round. The descriptor table sits at the start of a page-aligned block, the available ring right after it and the used ring on the next page boundary, the whole rounded up to pages and allocated through `binding.AllocateDma`, so it is freed by teardown with everything else. It returns `false` when the index is 8 or above, when the queue does not exist (its maximum reads 0), when it is already ready, or when the transport refused the layout; a refused layout leaves the memory on the ledger for the unwind. The net driver asks for two queues of 128:

```csharp
if (!dev.TryCreateQueue(binding, VirtioNetState.ReceiveQueue, QueueSize, out Virtqueue? receiveQueue))
{
    return ProbeResult.Failed("no receive queue");
}
```

A `Virtqueue` is the ring as a driver drives it: `Index` and `Size`; `TryAllocateDescriptor(out index)` and `FreeDescriptor(index)` over a free list the kit keeps, with `FreeDescriptorCount`; `SetDescriptor(index, physicalAddress, length, flags, next = 0)`, whose `VirtqueueDescriptorFlags` are `Next` for a chain, `Write` for a buffer the device fills and `Indirect`; `Submit(head)`, which puts a chain in the available ring behind a write barrier; `HasUsed`, a barrier-free check for a work-item loop that returns (a driver that must wait waits on its interrupt or spins with `DmaBuffer.ReadBarrier()`), and `TryTakeUsed(out id, out length)`, which takes back what the device finished behind a read barrier, skipping an element whose id is out of range and counting it in `DroppedUsedElements` rather than throwing, since the kit cancels a work item that throws and the drain would never run again; and `Notify()`, the doorbell. Every member is allocation-free, so a handler may use them; none is thread-safe, so a driver serializes its use with a `DeviceLock`, as the net driver does between `Transmit` and its drain. Once the binding that created the queue is torn down every ring member throws `InvalidOperationException`, in every build, so a consumer that kept a reference across a withdrawal gets the exception and never a write into freed pages.

One rule of the specification orders the probe: buffers may be submitted before DRIVER_OK, the doorbell may not be rung before it. The net driver posts every receive slot with `Submit` alone and rings once, after `SetDriverOk`, before it publishes:

```csharp
for (int i = 0; i < receiveQueue.Size; i++)
{
    if (!receiveQueue.TryAllocateDescriptor(out ushort slot))
    {
        return ProbeResult.Failed("the receive queue ran out of descriptors while posting");
    }

    ulong physical = receiveBuffers.PhysicalAddress + (ulong)(slot * VirtioNetState.BufferBytes);
    receiveQueue.SetDescriptor(slot, physical, VirtioNetState.BufferBytes, VirtqueueDescriptorFlags.Write);
    receiveQueue.Submit(slot);
}

// ... the station address, the state, the drain and the interrupts ...

dev.SetDriverOk();
receiveQueue.Notify();
state.Sink = binding.PublishNetwork(state);
```

Its `Transmit` runs under the lock: it reclaims the used transmit descriptors, takes one, copies a zeroed net header and the frame into that descriptor's slot, writes the descriptor, submits and notifies. Its drain, a work item on the kit worker, takes each used receive element under the lock, hands the frame behind the header to the sink outside the lock (the consumer copies it before returning, and the slot is re-posted only afterwards), and notifies once when anything was taken.

### Virtio interrupt sources

`Node.Interrupts` of a virtio node is always nine sources, the access's own objects: index 0 the configuration change, which `Describe()` prints as `config`, and index 1 + n queue n, `queue 0` on. They are virtual: the transport delivers a few entries (the messages it connected, or the one MMIO line) and the access multiplexes the sources over them. The configuration source gets entry 0 when the handshake starts, if the transport has an entry and the device accepts it; queue n gets entry n + 1 when the transport has that many entries, otherwise entry 0 shared with the rest, otherwise none, assigned by a successful `TryCreateQueue`. A source with no entry refuses to connect, so `binding.TryRequestInterrupt(dev.QueueInterrupt(0), handler, out _)` returns `false` on a device the transport could not route and the driver falls back to polling, as it does with the PCI line:

```csharp
bool receiveConnected = binding.TryRequestInterrupt(dev.QueueInterrupt(VirtioNetState.ReceiveQueue), state.OnInterrupt, out _);
binding.TryRequestInterrupt(dev.QueueInterrupt(VirtioNetState.TransmitQueue), state.OnInterrupt, out _);
binding.TryRequestInterrupt(dev.ConfigInterrupt, state.OnInterrupt, out _);
state.HasInterrupt = receiveConnected;
if (!receiveConnected && !binding.TrySchedulePeriodic(DrainPeriodMilliseconds, drain))
{
    return ProbeResult.Declined("no interrupt and no timer to poll with");
}
```

A raise runs the leaf's handler inside the transport's own dispatch, in interrupt context, and the handler does what every handler does: it counts and schedules the drain. Because the entry is shared with the other sources of the node, `Mask` on a virtio handle drops deliveries at the kit, not at the controller. A reset takes the entry away from every source, so a leaf's `OnDetach` may call `dev.Reset()` to stop the device before the kit frees its rings; that is safe there because the binding's handles are already disconnected, and the net driver follows it with `binding.Delay(100)` so DMA in flight lands first:

```csharp
public override void OnDetach(DeviceBinding binding, DetachReason reason)
{
    if (binding.DriverState is not VirtioNetState || !reason.HardwarePresent)
    {
        return;
    }

    binding.Node.Access<VirtioAccess>().Reset();
    binding.Delay(QuiesceMicroseconds);
}
```

### The transport drivers

`VirtioPciTransportDriver` is `[Driver(Feature = DriverFeature.Pci)]` and matches `new PciMatch(vendorId: 0x1AF4)`, every virtio function, so a leaf driver a kernel author writes for another virtio type needs no change here. Its probe derives the device type from the function's ids (a device id of `0x1040` or above is modern and the type is the id minus `0x1040`; `0x1000` to `0x103F` is transitional and the type is the subsystem id; anything else is declined `not a virtio function`), walks the vendor capabilities for the common, notify, ISR and device configuration structures (declined `no modern virtio capabilities (a legacy-only function)` without the first three, with the reason when a structure names a BAR past 5, an unassigned or I/O BAR, or one it does not fit, and `the notify structure or notify_off_multiplier is not 2-byte aligned` when either is odd), maps each BAR it needs once, turns on memory space and bus mastering, and requests the function's message sources for entry 0 on, at most nine (the configuration change and the eight queues), stopping at the first the platform cannot route. Then it publishes the child with the type, the function's address and the access's nine sources, and logs `virtio type 1, 4 message interrupts`. It has no legacy (I/O BAR) interface and no INTx fallback, so a function whose messages cannot be routed (ARM64 without an ITS, the virt machine's default GICv2) is published with no interrupt entry and its leaf polls or declines. `OnDetach` turns bus mastering off once the children are torn down.

`VirtioMmioTransportDriver` is `[Driver]` with no feature and matches `PlatformMatch.Compatible("virtio,mmio")`, the nodes the ARM64 description publishes for the virt machine's slots. Its probe maps the slot's window (declined `no register window` without one, or `the register window spans 256 bytes, less than 512` when it is too short), checks the magic register (`no virtio device in the slot`), the device id (`empty slot`) and the version (`unknown virtio-mmio version 3`; version 1, the legacy interface, and version 2 are taken), requests the slot's line when the node has one, publishes the child with the type and the slot's base in hexadecimal, and logs `virtio type 1, version 1, line` or `no line`. It has no `OnDetach`: the child's hooks reset the device and the kit invalidates the window.

Both are ordinary bus drivers written over the public seam: the transport class each builds (`VirtioPciTransport`, `VirtioMmioTransport`) derives from the kit's `VirtioTransport`, whose members the access calls, and the kit never sees a register itself.

### The shipped leaf drivers

`VirtioNetDriver` (`[Driver(Feature = DriverFeature.Network)]`, `VirtioMatch.DeviceType(VirtioDeviceType.Network)`) negotiates the MAC, status and any-layout features, declines a legacy device without any-layout as shown above, creates the receive and transmit queues, posts one 2048-byte buffer per receive descriptor, reads the station address (declined `no MAC address` when the feature is absent or the address is all zeros), connects the two queue sources and the configuration source or polls every 50 ms, sets DRIVER_OK, rings the receive queue and publishes an interface named `virtio-net`. Its log line reads `mac 52:54:00:12:34:56, link up, version 1, interrupts: 4 entries`, or `legacy any-layout` and `polling every 50 ms` on a legacy MMIO device whose line the platform did not route.

`VirtioInputDriver` (`[Driver]`, `VirtioMatch.DeviceType(VirtioDeviceType.Input)`) carries no feature because the same device type is a keyboard or a mouse: its probe asks the configuration space which event types the device reports, declines `no key events` without keys, takes a device with relative axes for a mouse, and only then checks the kernel's switches (`mouse support is compiled out`, `keyboard support is compiled out`), each one alone. It creates the event queue, posts 32 eight-byte event buffers, connects the queue's source or polls every 20 ms, sets DRIVER_OK, rings the queue and publishes a keyboard named `virtio-keyboard` (Linux key codes converted to set 1 scan codes) or a pointer named `virtio-mouse` (axis and button events folded into one report per sync event); the log line is `keyboard, interrupt` or `pointer, polling every 20 ms`. It does not drive the status queue, so a virtio keyboard's indicators stay as they are. Both leaf drivers decline `no kit worker to run the drain on` in a kernel without a worker, and both reset the device in `OnDetach`.

### The display drivers

`VirtioGpuDriver` (`[Driver(Feature = DriverFeature.Graphics)]`, `VirtioMatch.DeviceType(VirtioDeviceType.Gpu)`) is a virtio leaf like the two above, so it binds a virtio-gpu over PCI on both architectures and over MMIO on ARM64 (`-device virtio-gpu-device`), 2D path only: the guest renders, the host composites. Its probe negotiates no feature beyond the VERSION_1 the kit takes (failed `the device rejected the feature set`), creates the control queue (failed `no control queue`) and the cursor queue when the device has one (logged `no cursor queue; the cursor is not driven` otherwise; the cursor queue is created but not driven), allocates one page of scratch DMA for the command, the memory entry and the response, connects the control queue's source or falls back to polling the used ring every 10 microseconds, sets DRIVER_OK before the first command, reads the scanout count from the configuration space (0 read as 1), asks GET_DISPLAY_INFO for the first scanout's rectangle (1024x768 when it is disabled or empty), allocates a page-aligned DMA framebuffer of that size, and creates the host resource with RESOURCE_CREATE_2D, backs it with RESOURCE_ATTACH_BACKING and binds it to scanout 0 with SET_SCANOUT, failing with the command's name when one is refused or, after 1000 ms, unanswered. Then it publishes a display named `virtio-gpu` whose `Framebuffer` is the DMA buffer's region and logs `1280x800, 1 scanouts, interrupt` (or `polling`). Every `Flush` is a TRANSFER_TO_HOST_2D of the clipped rectangle and a RESOURCE_FLUSH, both synchronous; one command is in flight at a time, the lock held only around the ring operations and never across a wait, so the ring's canvas may flush from any thread. A device that stops answering marks the state faulted, logs `the device stopped answering; flushes are dropped` once, and every later command is dropped. The display offers no facet beyond its own state type, `VirtioGpuState`, whose counters (`CommandsSent`, `FlushCount`, `InterruptCount`, `HasInterrupt`, `IsPolling`, `IsFaulted`, `ScanoutCount`) the Graphic suite reads through `DisplayManager.Primary.TryGetFacet`. `OnDetach` resets the device and waits 100 microseconds for DMA in flight, like virtio-net.

`VmwareSvgaDriver` (`[Driver(Feature = DriverFeature.Graphics)]`, `new PciMatch(vendorId: 0x15AD, deviceId: 0x0405)`) is a PCI driver for the VMware SVGA II adapter, programmed through port I/O, so it declines `the adapter is programmed through port I/O, which this platform has not` on ARM64. Its probe turns I/O and memory decoding on first (QEMU answers the `FrameBufferStart` and `MemStart` registers only while it is), maps BAR 0 as the index and value ports and writes the version 2 protocol id (failed `the adapter did not accept the version 2 protocol` when it does not read back), maps BAR 1 as VRAM with write combining and BAR 2 as the command FIFO, checks that the two registers name the BARs (failed `register FrameBufferStart does not match BAR 1`), initialises the FIFO with the guest's 3D version declaration and the SVGA3D negotiation, and reads the scanout as the firmware left it. A scanout the firmware enabled is the display's mode; one it left off, which is what QEMU does, leaves the mode empty until the canvas programs one, since the width and height registers then echo the firmware's VGA surface. It publishes a display named `vmware-svga`, a `VmwareSvga3DState` implementing `ICanvas3DFactory` when the host negotiated 3D and a `VmwareSvgaState` otherwise, and logs `640x480x32 disabled, caps 0x3, svga3d none, vram 16 MiB, fifo 64 KiB`, where `svga3d` is `none` or the negotiated version as `major.minor`. The display's `Framebuffer` is one frame of VRAM sliced at the adapter's frame offset, recomputed on every mode set, and `Flush` is a FIFO UPDATE plus a sync, skipped while no mode is programmed or the scanout is off. It offers both kit facets: `IDisplayModes` with the classic VMware list from 320x200 to 3840x2400 at 32 bits per pixel, whose `TrySetMode` programs the registers, re-initialises the FIFO and reports the change, and `IHardwareCursor`, whose `TryDefine` needs the alpha cursor capability (absent on QEMU, so it returns `false` there) and whose `Set` writes the cursor registers. It also offers `ISvgaAdapter`, an experimental (`COSMOS0003`) test and tooling seam rather than a stability promise: the capabilities, the negotiation facts, the enable bit (`SetEnabled(false)` stops the host consuming the FIFO, so what is written afterwards can be inspected), the FIFO positions (`FifoMin`, `FifoMax`, `NextCommand`, `ReadFifo`) and a `CreateCanvas3D` that hands out the SVGA3D canvas whether or not 3D was negotiated, which is how the Graphic suite pins the 3D command layer's wire format on QEMU. The firmware framebuffer sits in the VRAM window, so binding this driver retires it; the log of the vmware-svga cell reads, in order:

```
[Display] primary: VmwareSvgaDriver "vmware-svga" (driver published, preferred over firmware)
[Drivers] pci:0000:00:01.0 VmwareSvgaDriver published display "vmware-svga" (consumed)
[Drivers] pci:0000:00:01.0 VmwareSvgaDriver: 640x480x32 disabled, caps 0x3, svga3d none, vram 16 MiB, fifo 64 KiB
[Drivers] pci:0000:00:01.0 offer VmwareSvgaDriver -> bound
[Drivers] firmware display "framebuffer" retired: inside pci:0000:00:01.0 bar 1
[Display] mode changed: VmwareSvgaDriver "vmware-svga" now 1024x768x32
```

The primary line says `preferred over firmware` because the display is published during the probe, while the firmware display is still there, and the retirement runs after the bind; the retirement itself changes no primary, so it writes no second line. The last line is the console programming the default mode. `OnDetach` turns the scanout off so the host stops reading VRAM the ring may still hold a region over.

### The storage drivers

`AhciDriver` (`[Driver(Feature = DriverFeature.Storage)]`, `new PciMatch(classCode: 0x01, subclass: 0x06, progIf: 0x01)`: every SATA controller in AHCI mode; the legacy IDE-mode function has another programming interface and is never offered) is a PCI driver for an AHCI 1.3.1 host bus adapter. Its probe checks BAR5 as ABAR (declined `BAR5 is not a memory window` when it is unassigned, I/O, or shorter than the generic registers plus port 0's bank), maps it, turns on memory space and bus mastering, allocates one command region of 0x4A000 bytes in DMA memory holding the command list, the received FIS area and the 32 command tables of every port index, sets GHC.AE, and when firmware left the port map empty resets the HBA (failed `the HBA reset did not complete`) and derives the map from CAP.NP; a 32-bit HBA whose region landed above 4 GiB is declined `the controller addresses 32 bits and the command region lies above 4 GiB`. Then it walks each implemented port in ascending order: a port whose PHY reports no device gets one COMRESET, and one still without a device logs `port 1: no device (SSTS 0x0)` and is skipped; a port with a device has its engine stopped (or the port reset when the engine will not stop, and `port 1: engine still running, skipped` when neither works), is rebased onto the region with its interrupts masked, started (`port 1: engine did not start, skipped` after one reset and retry) and classified by its signature: a SATA disk becomes an `AhciPort`, whose constructor refuses an ATAPI device, allocates the port's bounce page (below 4 GiB on a 32-bit HBA) and issues IDENTIFY DEVICE, a failure being logged `port 1: bring-up failed: message`; a SATAPI, SEMB or port multiplier signature logs `port 5: satapi not supported` (`semb`, `port multiplier`), and an unknown one `port 1: unknown signature 0x..., skipped`. A rebased port that is not published is stopped again, so no engine keeps a FIS area inside memory the kit frees. The probe hangs an `AhciState` off `binding.DriverState` (`Index`, `PortCount`, `Version`, `CommandSlots`, `Supports64Bit`, `SupportsCommandListOverride`, `ImplementedPorts`, `CommandsIssued`, `Timeouts`), publishes each port through `PublishBlockDevice` as `sata{n}`, where `n` is global across controllers and consumed by every port whose bring-up was attempted, never reused, and logs `1 sata ports of 6 implemented, version 1.0, 32 slots, 64-bit`. A controller with no usable port still binds, which is what q35's built-in AHCI at `00:1f.2` does on every x64 machine: its port 5 holds the boot CD-ROM (`port 5: satapi not supported`) and it logs `0 sata ports of 6 implemented`. The driver stays strictly polled: no interrupt is requested, GHC.IE is never set and every PxIE stays 0. An `AhciPort` (`IBlockDevice`, with `Model`, `Serial`, `Firmware`, `PortNumber` and `Controller`) moves every transfer through its one bounce page, at most 8 sectors of 512 bytes per command, and keeps one command in flight per port: a caller claims the port's busy flag under the controller's `DeviceLock`, held only around the flag, so the chunks of two callers interleave but never overlap; a task file error is `IOException("SATA Fatal error: Command aborted")`, a stuck port or a command unanswered for 5 s an `InvalidOperationException` (`SATA: port stuck busy (TFD BSY/DRQ) before command issue.`, `SATA: command completion timeout.`). `OnDetach` stops the engine of every published port and turns bus mastering off.

`NvmeDriver` (`[Driver(Feature = DriverFeature.Storage)]`, `new PciMatch(classCode: 0x01, subclass: 0x08, progIf: 0x02)`) is a PCI driver for an NVM Express controller. Its probe checks BAR0 (declined `BAR0 is not a memory window` when it is unassigned, I/O, or shorter than the registers plus the doorbell page), maps it, turns on memory space (before any message request, since the kit refuses a message while decoding is off) and bus mastering, reads CAP (failed `the controller's queue size is below the driver's queue depth` or `the doorbell stride does not fit BAR0`), disables the controller and waits for it (failed `the controller did not leave the ready state`), programs the admin queue pair in two DMA pages, enables it (failed `the controller did not become ready`), masks every vector, and identifies the controller, the active namespace list and each namespace through one scratch page, all on the admin queue, which is always polled: an admin command unanswered for 5 s fails the probe with `identify controller did not complete` and the like, a refused one with `identify controller failed` after a log line carrying the status; a namespace with no blocks is skipped silently, and one with metadata, a block below 512 bytes or a block above a page is logged `namespace 1 skipped (block size 8192, metadata 0)`. It allocates seven command slots (a bounce page and a `DeviceEvent` each) and the lock, then requests message 1 when the function's MSI-X table has two entries or more (message 0 carries the admin completions and must interrupt nobody), message 0 on a single-entry table, and polls when the function has no table or the platform cannot route the message (ARM64 with GICv2); the legacy line is never requested, and the handler is live from the request on but does nothing until the I/O queue exists. It creates the I/O completion queue, on that vector when one is connected, and the I/O submission queue (failed `create I/O completion queue failed` or `create I/O submission queue failed`), hangs an `NvmeState` off `binding.DriverState` (`Index`, `HasInterrupt`, `IsPolling`, `NamespaceCount`, `CommandsSent`, `InterruptCount`, `Timeouts`, `DoorbellStride`, which the Storage suite reads through a published namespace's `Controller`), publishes every namespace as `nvme{i}n{nsid}` (the controller number, then the namespace id) and logs `1 namespaces, interrupt, queue depth 8`, or `polling`. An `NvmeNamespace` (`IBlockDevice`, with `NamespaceId` and `Controller`) issues one command per logical block through a slot's bounce page, so callers on the same or another namespace of the controller run in parallel up to the seven slots; the submission runs under the lock and the wait outside it: with an interrupt the caller waits on the slot's event and the handler drains the completion queue in interrupt context, without the lock (a lock holder runs with interrupts disabled, so the two never overlap), signalling each completed slot through `context.Signal`; without one the caller drains under the lock between delays. A failed command is `IOException("NVMe Read error")`, `Write` or `Flush`; one unanswered for 5 s is `IOException("NVMe command timeout")` and a teardown mid-command `IOException("NVMe device detached")`, and either quarantines the slot (`quarantined I/O slot 3 (command may still be outstanding)`), never handed out again since a late completion would write into a recycled page; a caller that finds no free slot for 5 s gets `InvalidOperationException("NVMe I/O slots exhausted.")`. `OnDetach` disables the controller and turns bus mastering off.

Both drivers publish through `PublishBlockDevice`, so the storage manager registers and scans each disk inside the probe ([Publishing a device](#publishing-a-device)). The ahci cell of the Storage suite, whose disk hangs off an ich9-ahci added at `00:03.0`, logs in this order:

```
[Drivers] pci:0000:00:03.0 candidates: AhciDriver(prio 0, spec 3)
[Drivers] pci:0000:00:03.0 AhciDriver: port 1: no device (SSTS 0x0)
...
[Drivers] pci:0000:00:03.0 AhciDriver: port 5: no device (SSTS 0x0)
[StorageManager] sata0 registered by AhciDriver (primary)
[Drivers] pci:0000:00:03.0 AhciDriver published block "sata0" (consumed)
[Drivers] pci:0000:00:03.0 AhciDriver: 1 sata ports of 6 implemented, version 1.0, 32 slots, 64-bit
[Drivers] pci:0000:00:03.0 offer AhciDriver -> bound
```

and the nvme cell:

```
[Drivers] pci:0000:00:03.0 candidates: NvmeDriver(prio 0, spec 3)
[StorageManager] nvme0n1 registered by NvmeDriver (primary)
[Drivers] pci:0000:00:03.0 NvmeDriver published block "nvme0n1" (consumed)
[Drivers] pci:0000:00:03.0 NvmeDriver: 1 namespaces, interrupt, queue depth 8
[Drivers] pci:0000:00:03.0 offer NvmeDriver -> bound
```

On a formatted disk the manager's scan line (`[StorageManager] GPT detected on sata0`) comes before the registered line, since the scan runs inside the registration. The suites read the drivers through the ring, never the log: `Manager_DeviceListedInDriverInfo` finds the cell's disk in `DriverInfo` as a consumed block device under `AhciDriver` or `NvmeDriver`, and `Profile_NvmeInterruptModeMatches` reads `HasInterrupt` off the namespace's `Controller`.

## Observing drivers

Everything the kit does is one line in the serial log, prefixed `[Drivers]`, with the two `[StorageManager]` lines a block device adds, and the same fact in the `DriverInfo` facade of `Cosmos.Kernel.System.Diagnostics`, so a test asserts on the facade and a human reads the log, and the two cannot drift.

The log lines, in the order a device's life produces them:

| Line | When |
|------|------|
| `manifest: A(prio 0) B(prio 10)` | Once, at the start of the driver stage; `(empty)` with no drivers |
| `firmware published display "framebuffer" (consumed)` / `(no consumer)` | Once, between the manifest and the engine lines, when the bootloader handed over a framebuffer and graphics are on |
| `engine started, worker thread` / `engine started, inline (no worker)` | Once, right after |
| `synthetic:k candidates: A(prio 10, spec 1) B(prio 0, spec 1)` | A node is offered |
| `synthetic:k offer A -> bound` / `-> declined: reason` / `-> failed: message` | Each offer |
| `synthetic:k no driver` | No registered driver matched |
| `[StorageManager] sata0 registered by AhciDriver (primary)` | The storage manager consumed a published block device and scanned it, inside the publish, so it precedes the `published block` line; the suffix says the manager's order rule made the disk the primary at that moment, and a later line carrying it supersedes this one |
| `synthetic:k A published keyboard "name" (consumed)` / `(no consumer)` | A device was published |
| `firmware display "framebuffer" retired: inside pci:0000:00:01.0 bar 1` | A driver bound the function whose memory window holds the firmware framebuffer; the firmware display is withdrawn |
| `synthetic:k A: message` | `binding.Log` |
| `synthetic:k A interrupt handler threw: message` | A handler threw; its source is masked |
| `synthetic:k A work item threw: message` | A work item threw; it is cancelled |
| `[StorageManager] sata0 unregistered (primary now nvme0n1)` / `(no primary)` | Teardown withdrew a block device and the storage manager dropped it, inside the withdrawal, so it precedes the `withdrew block` line; the suffix appears only when it was the primary |
| `synthetic:k A withdrew keyboard "name"` | Teardown withdrew a device |
| `synthetic:k A thread "name" did not stop in 500 ms; 3 resources leaked` | Teardown could not join a thread |
| `synthetic:k A OnDetach threw: message` | The detach hook threw |
| `synthetic:k retracted` | The node left the tree |
| `pci:0000:00:03.0 bus hook "quiesce" threw: message` | A bus hook threw (`quiesce`, `quiet`, `restore` or `after teardown`); after `quiesce` the node is `not offered: message` |
| `pci host at 0x...: buses xx to yy not mapped, enumeration ends at bus zz` | An ECAM window could not be mapped whole; the host serves the buses before it |
| `virtio type 1: status did not return to 0 within 100 ms after reset` | A virtio device did not acknowledge a reset in time; the kit went on as if it had |

`DriverInfo` is read-only, allocation-free and safe to poll: `IsStarted`, `HasWorker`, `DriverCount`, `NodeCount`, `DeviceCount`, `GetTotalHeldResourceCount()`, and four `Try` reads that snapshot one entry by index. `TryGetDriver(index, out DriverEntryInfo)` walks the manifest (`Name`, `Priority`). `TryGetNode(index, out DeviceNodeInfo)` walks every node ever published, retracted ones included: `Path`, `BusName`, `Description`, `DriverName`, `State` (`Pending`, `Bound`, `Unbound`, `Retracted`), `ParentPath`, the resource, interrupt, offer and child counts, `HeldResourceCount`, `PublishedDeviceCount`, `LeakedResourceCount`, `FaultCount` and `LastFault`. `TryGetOffer(nodeIndex, offerIndex, out DeviceOfferInfo)` replays a node's arbitration (`DriverName`, `Priority`, `Specificity`, `Outcome`, `Reason`, `ReleasedResourceCount`). `TryGetDevice(index, out PublishedDeviceInfo)` lists what is published (`Kind`, `Name`, `NodePath`, `DriverName`, `IsConsumed`, `IsWithdrawn`).

```csharp
using Cosmos.Kernel.System.Diagnostics;

for (int i = 0; i < DriverInfo.NodeCount; i++)
{
    if (DriverInfo.TryGetNode(i, out DeviceNodeInfo node))
    {
        Console.WriteLine($"{node.Path} {node.DriverName ?? "no driver"} held {node.HeldResourceCount}");
    }
}
```

A snapshot is taken without locking the kit, so a node whose offer or teardown is running on the worker may read one job stale. Nothing in `DriverInfo` acts on the kit, so nothing there throws.

## Testing a driver over the synthetic bus

The synthetic bus exists so that a driver's binding, arbitration, decline and removal behaviour can be tested with no hardware at all, in a test kernel, identically on both architectures. `SyntheticBus` has four members, all thread context:

| Member | What it does |
|--------|--------------|
| `Publish(key, data, interruptCount = 0, windowBytes = 0)` | Publishes a node at `synthetic:key`. `data` is a byte array the driver can read through the node's `SyntheticAccess`; `interruptCount` is how many sources the node has; `windowBytes` (up to one page) gives it a RAM-backed register window as resource 0. After the driver stage it returns once the node was offered; before it, it returns at once and the stage offers the node |
| `Retract(node, hardwarePresent = false)` | Takes the node away; returns once its binding was torn down. `hardwarePresent` is what the driver's `DetachReason.HardwarePresent` says, false as for a hot-unplug by default |
| `RaiseInterrupt(node, index)` | Runs the handler connected to that source in a synthetic dispatch, with interrupts masked and the guard on; returns `false` before the driver stage has run, when nothing is connected, or when the source is masked |
| `WaitForQueuedJobs()` | Returns once every kit job queued before the call has run: a work item a handler scheduled, a teardown a driver queued. The hook for asserting after `RaiseInterrupt` |

The node's access object, `node.Access<SyntheticAccess>()`, is how the test sees the device from the other side: `Data` are the bytes it attached, `HasWindow` says whether resource 0 exists, and `Window` is the register window's memory as the test sees it, so a test can write what the driver's handler will read and read what the probe wrote. `Window` is empty once the node was retracted and the page released.

A test of the keyboard driver above, in a test kernel's `BeforeRun`:

```csharp
using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Synthetic;
using Cosmos.Kernel.System.Diagnostics;
using MyOS.Drivers;

// Publish: returns once the node was offered and bound.
DeviceNode node = SyntheticBus.Publish(SyntheticKeyboardDriver.Key, [], interruptCount: 1, windowBytes: 64);
KeyboardState? state = node.Binding?.DriverState as KeyboardState;
SyntheticAccess access = node.Access<SyntheticAccess>();

// The probe armed the device through its window; the test sees the write.
bool armed = access.Window[KeyboardState.ControlOffset] == KeyboardState.EnableBit;

// Stage a key, raise the interrupt, wait for the work item it scheduled.
access.Window[KeyboardState.ScanCodeOffset] = 0x1E;
access.Window[KeyboardState.FlagsOffset] = 0;
bool raised = SyntheticBus.RaiseInterrupt(node, 0);
SyntheticBus.WaitForQueuedJobs();

// Retract: returns once the binding was torn down.
int heldBefore = DriverInfo.GetTotalHeldResourceCount();
SyntheticBus.Retract(node);
bool released = DriverInfo.GetTotalHeldResourceCount() < heldBefore && access.Window.IsEmpty;
bool stale = SyntheticBus.RaiseInterrupt(node, 0);   // false: nothing is connected any more
```

What such a test asserts, and where it reads it from, follows the Drivers suite: the node's state, driver and counts through `DriverInfo.TryGetNode`; the arbitration through `TryGetOffer`; what the handler and the work item did through the driver's own state object, reached through `node.Binding.DriverState`; and after a retraction, that the held total went back down, the window is empty, the driver saw `DetachCause.Retracted` in `OnDetach`, and a raise no longer runs the handler. A node published from the kernel's constructor, before the driver stage, covers the boot path; one published from a test covers hot-plug. See [Testing](../dev/testing.md) for how a test kernel is built and run.

## Project settings

### A driver inside the kernel project

A kernel project built with `Cosmos.Sdk` needs one line to write drivers, the acknowledgement of the experimental seam. The driver class may be `internal`:

```xml
<PropertyGroup>
  <CosmosKernelClass>MyOS.Kernel</CosmosKernelClass>
  <NoWarn>$(NoWarn);COSMOS0003</NoWarn>
</PropertyGroup>
```

### A driver library

A driver that ships on its own is a class library that references `Cosmos.Kernel.HAL` for the kit and `Cosmos.Kernel.System` for the kernel API, declares itself a driver assembly, and suppresses the seam's id. Its `[Driver]` classes are `public`, because the manifest generator in the consuming kernel sees a library's drivers through metadata:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <CosmosDriverAssembly>true</CosmosDriverAssembly>
    <NoWarn>$(NoWarn);COSMOS0003</NoWarn>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Cosmos.Kernel.HAL" />
    <PackageReference Include="Cosmos.Kernel.System" />
    <PackageReference Include="Cosmos.Build.Analyzer.Patcher" />
  </ItemGroup>

</Project>
```

The sample leaves package versions to central package management, as the kernel projects in the tree do; a library without a `Directory.Packages.props` gives each reference the `Version` its kernel uses.

`CosmosDriverAssembly` is what makes the library's build the proof that it uses only what any kernel author can use. The analyzer package brings the `CompilerVisibleProperty` that lets its rules read the property; a kernel gets the package through the SDK, and a library references it itself, as the sample does, since without it the property is inert. The package enforces three things on such an assembly:

- It is a **User** layer assembly whatever its name, judged on the types and members its code names rather than on the reference list restore builds: it may name what `Cosmos.Kernel.System` offers, what `Cosmos.Kernel.HAL` offers for the kit, and the device contracts in `Cosmos.Kernel.HAL.Interfaces` that the kit's surface names (`IBlockDevice`, `MACAddress`), and nothing lower. `Cosmos.Kernel.Core` sits on the reference list because the HAL and the ring were built against it, as do `Cosmos.Kernel.Boot.Limine` and the `Cosmos.Build.*` assemblies; naming a type or member from `Cosmos.Kernel.Core` is `NAOT0007`.
- `NAOT0008`: no `[UnsafeAccessor]` or `[UnsafeAccessorType]` anywhere in it, which closes the one hatch that would reach internals without a grant.
- `NAOT0009`: no `Cosmos.*` assembly it references grants it `InternalsVisibleTo`.

In the tree, `CosmosDriverAssemblyNames` in `Directory.Build.props` lists every driver assembly: today that is `Cosmos.Kernel.Drivers`, the package the shipped drivers live in, and the Drivers suite's library, `Cosmos.Kernel.Tests.Drivers.Library`. Both are built under `CosmosDriverAssembly` with no grant from any project, so the compiler is the proof that the shipped drivers call only what a kernel author can call; a capability a driver there needs and a third party cannot reach is a build failure, and the fix is to add it to the kit for everyone. `Cosmos.Kernel.Drivers` is one RID-less `lib/net10.0` package, because an assembly written over the seam holds no architecture-specific code by construction, and `Cosmos.Kernel` references it, so a kernel gets it with the aggregator and needs no reference of its own. The kernel that consumes the library adds a `ProjectReference` or `PackageReference` to it and its own `<NoWarn>` line; the library's drivers then appear in the kernel's manifest after the kernel's own, ordered by assembly name and then type name.

### Excluding and opting in

Policy lives in the kernel's `.csproj`, not in code. Names are full type names, without `global::`:

```xml
<ItemGroup>
  <!-- Drop a driver the build would otherwise register. -->
  <CosmosDriverExclude Include="Acme.Drivers.AcmeNicDriver" />
  <!-- Register a driver declared [Driver(Default = false)]. -->
  <CosmosDriverInclude Include="Acme.Drivers.ExperimentalGpu" />
</ItemGroup>
```

The drivers Cosmos ships are dropped the same way, by full type name. A kernel that brings its own Ethernet driver, or wants the function left as firmware set it up, keeps the Intel driver out of its manifest:

```xml
<ItemGroup>
  <CosmosDriverExclude Include="Cosmos.Kernel.Drivers.E1000EDriver" />
</ItemGroup>
```

The Threading suite drops `Cosmos.Kernel.Drivers.VirtioNetDriver` the same way, to keep a polled NIC's periodic drain off the worker it measures. A kernel that wants to draw on the firmware framebuffer even on a VMware SVGA II adapter drops `Cosmos.Kernel.Drivers.VmwareSvgaDriver`: no driver binds the function then, so the firmware display is not retired and stays the primary. Excluding `Cosmos.Kernel.Drivers.PciHostDriver` goes further: no PCI node is published then, and every PCI driver in the manifest stays idle; excluding a transport driver leaves the devices behind it unpublished the same way.

An excluded driver is dropped whatever its `Default`; a driver with `Default = false` is registered only when named. An item that matches no `[Driver]` class the kernel can see is reported as `COSMOSGEN002`, which is how a stale entry shows up. `[Driver(Feature = DriverFeature.X)]` is the third lever: the driver rides the kernel's feature switch and is trimmed with it. Reading the generated `DriverManifest.g.cs` under `obj/` is the quickest way to check what a kernel carries.

## Checklist

1. Derive from `Driver`, mark the class `[Driver]`, and give it `Name`, `Matches` and, when it must win a device from another driver, `Priority`.
2. Keep the driver class stateless across devices: create a state object in `Probe` and hang it off `binding.DriverState`.
3. In `Probe`, decline early on what the node is not (`ProbeResult.Declined`), acquire everything through the binding, publish the device, connect the interrupt, and arm the device last; fail on bring-up (`ProbeResult.Failed`) and let the kit unwind.
4. Keep the handler to registers, DMA memory, sinks and the three `InterruptContext` members; hand everything else to a work item. Never allocate or block there: the guard stops the call and masks the source.
5. Order DMA with `DmaBuffer.WriteBarrier()` before handing a descriptor over and `DmaBuffer.ReadBarrier()` after reading a device-written flag; register accesses carry their own.
6. Check the `Try` results: `TryRequestInterrupt`, `TryAllocateDma`, `TrySchedulePeriodic`, `TryStartThread` all say `false` when the platform or the kernel's switches cannot provide it.
7. For a PCI function, decline on `PciAccess.Bars` before mapping, turn on decoding and bus mastering through `PciAccess` yourself, request its message sources only after decoding is on, and pair the legacy line with a periodic drain, since neither the line nor the messages are routable everywhere.
8. For a virtio device, negotiate, create the queues, submit the buffers and connect the queue sources first, then `SetDriverOk` and only then `Notify`; reset the device in `OnDetach`.
9. Write driver threads as a loop on `IsDetaching` around `binding.Wait`, and return promptly once it turns true.
10. Put hardware quiescing in `OnDetach`, and only when `reason.HardwarePresent` is true.
11. Write a test kernel over the synthetic bus: publish, raise, `WaitForQueuedJobs`, retract, and assert through `DriverInfo` and the driver's own state.
12. Add `<NoWarn>$(NoWarn);COSMOS0003</NoWarn>` to the project, and `<CosmosDriverAssembly>true</CosmosDriverAssembly>` to a library, with its drivers `public`.
