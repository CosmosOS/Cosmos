# Writing a Driver

In this article, we will discuss how to write a device driver for Cosmos Gen3: what a driver is and where it runs, how the build finds it, how it takes a device and reaches its hardware through the driver kit, how it hands the kernel a device, and how it is tested with no hardware behind it.

The main differences if you come from Gen2:

| | Gen2 | Gen3 |
|---|---|---|
| Base class | `Cosmos.HAL.Device` | `Cosmos.Kernel.HAL.DriverKit.Driver` |
| Registration | By hand, in `Cosmos.HAL.Global` or the kernel | A generated manifest registers every `[Driver]` class the kernel can see |
| Hardware access | `IOPort`, `PCIDevice`, raw memory | A `DeviceBinding`: register windows, bulk regions, DMA memory, interrupts |
| Device removal | None | The kit tears the binding down in a fixed order; the driver frees nothing itself |
| Testing | On the hardware | Over the synthetic bus, in a test kernel, identically on x64 and ARM64 |

If you find bugs or something abnormal, please [submit an issue](https://github.com/CosmosOS/Cosmos/issues/new/choose) on our repository.

## Experimental status

Every type under `Cosmos.Kernel.HAL.DriverKit` (`Driver`, `DriverAttribute`, `DeviceBinding`, `DeviceNode`, the resource, interrupt and deferred-work types), `Cosmos.Kernel.HAL.DriverKit.Devices` (the device contracts and their sinks) and `Cosmos.Kernel.HAL.DriverKit.Synthetic` (the test bus) carries `[Experimental("COSMOS0003")]`: they are usable today but make no compatibility promise, and they are promoted to the stable surface by removing the attribute once proven. Referencing them is a build error until the project acknowledges that contract:

```xml
<PropertyGroup>
  <NoWarn>$(NoWarn);COSMOS0003</NoWarn>
</PropertyGroup>
```

See [Public API Tracking](../dev/public-api.md) for how experimental seams fit the surface policy.

The kit is being built in stages. What exists today:

- **One bus kind: the synthetic bus.** A node reaches the kit only through `SyntheticBus.Publish`. PCI, virtio, USB, PS/2 and platform nodes are later stages, so a driver for real hardware cannot be bound yet, and the device drivers the kernel ships (PS/2, virtio, the NICs, the storage controllers) still live in the HAL outside the kit.
- **No kernel manager consumes a published device yet.** `PublishKeyboard` records the keyboard, logs it and hands back a working sink, but `KeyboardManager` does not subscribe to the kit, so a key reported through the sink reaches nobody until a later stage, and the log line says `(no consumer)`. The same holds for pointers, network interfaces, block devices and displays.
- **Everything else on this page is implemented and tested**: the manifest, arbitration, the binding and its ledger, both execution contexts and the guard, teardown, the diagnostics view, and the synthetic bus. The Drivers test suite exercises all of it on x64 and ARM64, and its drivers are the models for the samples below.

## What a driver is

The kit has five nouns and one verb.

| Noun | What it is | Who creates it |
|------|------------|----------------|
| **Node** (`DeviceNode`) | One piece of hardware the kernel can see: an identity on a bus, a list of resources (memory windows, port ranges, RAM windows) and interrupt sources, and a bus-specific access object | A bus (today the synthetic bus), or a bus driver publishing a child |
| **Bus kind** | The schema a node on that bus follows: what its identity looks like, how a driver matches it, what its access object can do | The kit; the synthetic bus is the one kind today |
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

The driver stage runs from `Kernel.Start`, after the module initializers have brought up the heap, the interrupt controller, the scheduler and its tick, and after interrupts are enabled. The kit logs the manifest, starts its worker thread, offers every node published so far, and returns once each driver has answered. Only then do `OnBoot` and `BeforeRun` run, so a device a driver bound is usable from there on. The serial log of a kernel with no drivers shows the stage between the two `[Kernel]` lines:

```
[Kernel] Enabling interrupts...
[Kernel] Starting drivers...
[Drivers] manifest: (empty)
[Drivers] engine started, worker thread
[Kernel] Calling OnBoot()...
```

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

A bus knows a device before any driver looks at it, and it says so in a `DeviceIdentity`: `BusName` (the bus the device sits on), `Address` (its address in the bus's own notation) and `Describe()` (the identity in words, for the log). The node's `Path` is the two joined with a colon, `synthetic:kbd`, and it is how the log and the diagnostics view name the node.

A driver's match table is a `ReadOnlySpan<DeviceMatch>`. Each `DeviceMatch` is a predicate over an identity, `Matches(DeviceIdentity)`, plus a `Specificity`: how many identity fields it constrains. Each bus kind brings its identity type and its match type as a pair, and a driver never sees a transport, only the identity.

Today there is one pair. `SyntheticIdentity` carries a `Key` the test chose, and `SyntheticMatch` has two shapes:

```csharp
// This device and no other: specificity 1.
private readonly DeviceMatch[] _matches = [SyntheticMatch.Key("kbd")];

// Every synthetic device: specificity 0.
private readonly DeviceMatch[] _matches = [SyntheticMatch.Any()];
```

Later bus kinds add their own pairs (a PCI match over any subset of vendor, device, class and subclass with masks; a virtio match by device type; a USB match by class or by vendor and product), and a driver for one of them reads the same way: a table of matches and nothing about how the node was found.

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

### Publishing a device

A device kind is the smallest interface the kernel needs, plus a kit-owned **sink** through which the driver pushes events. Lifecycle is not on the interface, because the binding owns lifecycle; publish and withdraw are enable and disable.

| Kind | The driver implements | `Publish...` returns | The driver reports through |
|------|-----------------------|----------------------|----------------------------|
| Keyboard | `IKeyboard { string Name; void SetLeds(KeyboardLeds); }` | `KeyboardSink` | `Report(scanCode, released)` |
| Pointer | `IPointer { string Name; }` | `PointerSink` | `ReportRelative(deltaX, deltaY, buttons, wheel)`, `ReportAbsolute(x, y, buttons)` |
| Network interface | `INetworkInterface { string Name; MACAddress MacAddress; bool LinkUp; bool Transmit(ReadOnlySpan<byte>); }` | `NetworkSink` | `Receive(frame)`, `LinkChanged(up)` |
| Block device | `IBlockDevice`, the existing public contract | nothing | nothing to report |
| Display | `IDisplay { DisplayMode Mode; DeviceRegion? Framebuffer; void Flush(x, y, width, height); }` | `DisplaySink` | `ModeChanged()` |

`binding.PublishKeyboard(keyboard)`, `PublishPointer`, `PublishNetwork`, `PublishBlockDevice` and `PublishDisplay` are the five calls. Every sink is allocation-free and callable from any context, a handler included; it finds the kernel's consumer for its kind at call time and drops the report when the device was withdrawn or nobody listens. A kernel built without a kind, or, today, any kernel at all, therefore gets a sink that discards rather than a throw the driver could not anticipate. The published device is withdrawn by teardown ahead of everything else the driver holds, and the log records both ends:

```
[Drivers] synthetic:kbd synthetic-keyboard published keyboard "synthetic-kbd" (no consumer)
[Drivers] synthetic:kbd synthetic-keyboard withdrew keyboard "synthetic-kbd"
```

A display driver publishes its device in whatever scanout state it found it and programs no mode of its own; the display's `Mode` is what it is, and `Framebuffer` is a `DeviceRegion` the driver mapped or `null`.

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

The `resources` array carries `DeviceResource.MemoryWindow(physicalBase, length)`, `DeviceResource.PortRange(basePort, count)` or `DeviceResource.RamWindow(virtualBase, physicalBase, length)` entries; the `interrupts` array carries `InterruptSource` implementations the bus provides, each implementing `Describe()` and the four protected members (`TryConnectCore`, `MaskCore`, `UnmaskCore`, `DisconnectCore`) that the kit calls through its own internal forwarders, so a driver holding a source can neither connect nor mask it behind the kit's back.

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

## Observing drivers

Everything the kit does is one line in the serial log, prefixed `[Drivers]`, and the same fact in the `DriverInfo` facade of `Cosmos.Kernel.System.Diagnostics`, so a test asserts on the facade and a human reads the log, and the two cannot drift.

The log lines, in the order a device's life produces them:

| Line | When |
|------|------|
| `manifest: A(prio 0) B(prio 10)` | Once, at the start of the driver stage; `(empty)` with no drivers |
| `engine started, worker thread` / `engine started, inline (no worker)` | Once, right after |
| `synthetic:k candidates: A(prio 10, spec 1) B(prio 0, spec 1)` | A node is offered |
| `synthetic:k offer A -> bound` / `-> declined: reason` / `-> failed: message` | Each offer |
| `synthetic:k no driver` | No registered driver matched |
| `synthetic:k A published keyboard "name" (consumed)` / `(no consumer)` | A device was published |
| `synthetic:k A: message` | `binding.Log` |
| `synthetic:k A interrupt handler threw: message` | A handler threw; its source is masked |
| `synthetic:k A work item threw: message` | A work item threw; it is cancelled |
| `synthetic:k A withdrew keyboard "name"` | Teardown withdrew a device |
| `synthetic:k A thread "name" did not stop in 500 ms; 3 resources leaked` | Teardown could not join a thread |
| `synthetic:k A OnDetach threw: message` | The detach hook threw |
| `synthetic:k retracted` | The node left the tree |

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

`CosmosDriverAssembly` is what makes the library's build the proof that it uses only what any kernel author can use. The analyzer package, which every kernel already gets through the SDK, brings the `CompilerVisibleProperty` that lets its rules read the property, and enforces three things on such an assembly:

- It is a **User** layer assembly whatever its name, judged on the types and members its code names rather than on the reference list restore builds: it may name what `Cosmos.Kernel.System` offers, what `Cosmos.Kernel.HAL` offers for the kit, and the device contracts in `Cosmos.Kernel.HAL.Interfaces` that the kit's surface names (`IBlockDevice`, `MACAddress`), and nothing lower. `Cosmos.Kernel.Core` sits on the reference list because the HAL and the ring were built against it, as do `Cosmos.Kernel.Boot.Limine` and the `Cosmos.Build.*` assemblies; naming a type or member from `Cosmos.Kernel.Core` is `NAOT0007`.
- `NAOT0008`: no `[UnsafeAccessor]` or `[UnsafeAccessorType]` anywhere in it, which closes the one hatch that would reach internals without a grant.
- `NAOT0009`: no `Cosmos.*` assembly it references grants it `InternalsVisibleTo`.

In the tree, `CosmosDriverAssemblyNames` in `Directory.Build.props` lists every driver assembly: today that is the Drivers suite's library, `Cosmos.Kernel.Tests.Drivers.Library`, plus the name `Cosmos.Kernel.Drivers`, reserved for the package a later stage moves the shipped drivers into. A capability a driver there needs and a third party cannot reach is a build failure, and the fix is to add it to the kit for everyone. The kernel that consumes the library adds a `ProjectReference` or `PackageReference` to it and its own `<NoWarn>` line; the library's drivers then appear in the kernel's manifest after the kernel's own, ordered by assembly name and then type name.

### Excluding and opting in

Policy lives in the kernel's `.csproj`, not in code. Names are full type names, without `global::`:

```xml
<ItemGroup>
  <!-- Drop a driver the build would otherwise register. -->
  <CosmosDriverExclude Include="Acme.Drivers.VirtioNetDriver" />
  <!-- Register a driver declared [Driver(Default = false)]. -->
  <CosmosDriverInclude Include="Acme.Drivers.ExperimentalGpu" />
</ItemGroup>
```

An excluded driver is dropped whatever its `Default`; a driver with `Default = false` is registered only when named. An item that matches no `[Driver]` class the kernel can see is reported as `COSMOSGEN002`, which is how a stale entry shows up. `[Driver(Feature = DriverFeature.X)]` is the third lever: the driver rides the kernel's feature switch and is trimmed with it. Reading the generated `DriverManifest.g.cs` under `obj/` is the quickest way to check what a kernel carries.

## Checklist

1. Derive from `Driver`, mark the class `[Driver]`, and give it `Name`, `Matches` and, when it must win a device from another driver, `Priority`.
2. Keep the driver class stateless across devices: create a state object in `Probe` and hang it off `binding.DriverState`.
3. In `Probe`, decline early on what the node is not (`ProbeResult.Declined`), acquire everything through the binding, publish the device, connect the interrupt, and arm the device last; fail on bring-up (`ProbeResult.Failed`) and let the kit unwind.
4. Keep the handler to registers, DMA memory, sinks and the three `InterruptContext` members; hand everything else to a work item. Never allocate or block there: the guard stops the call and masks the source.
5. Order DMA with `DmaBuffer.WriteBarrier()` before handing a descriptor over and `DmaBuffer.ReadBarrier()` after reading a device-written flag; register accesses carry their own.
6. Check the `Try` results: `TryRequestInterrupt`, `TryAllocateDma`, `TrySchedulePeriodic`, `TryStartThread` all say `false` when the platform or the kernel's switches cannot provide it.
7. Write driver threads as a loop on `IsDetaching` around `binding.Wait`, and return promptly once it turns true.
8. Put hardware quiescing in `OnDetach`, and only when `reason.HardwarePresent` is true.
9. Write a test kernel over the synthetic bus: publish, raise, `WaitForQueuedJobs`, retract, and assert through `DriverInfo` and the driver's own state.
10. Add `<NoWarn>$(NoWarn);COSMOS0003</NoWarn>` to the project, and `<CosmosDriverAssembly>true</CosmosDriverAssembly>` to a library, with its drivers `public`.
