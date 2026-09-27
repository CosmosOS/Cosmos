# Drivers

In this article, we will discuss the driver kit: how a kernel writes its own PCI and USB drivers, registers them, and hands what they drive to the rest of the kernel, so that a NIC the built-in drivers do not know about carries the [network stack](network.md), or a USB mouse moves the [pointer](mouse.md).

The kit is an **experimental seam** (diagnostic `COSMOS0003`, see [Public API Tracking](../dev/public-api.md)): usable today, but with no compatibility promise until it is promoted to stable. Everything below is how it works in this version.

The main differences if you come from Gen2:

| | Gen2 | Gen3 |
|---|---|---|
| Hardware access | Kernel code drives PCI, I/O ports and interrupts itself | A registered driver is handed one device at a time, through a context that owns every resource it acquires |
| Binding | The kernel looks its device up and starts the driver | The kit offers each PCI function and USB interface HAL's built-in drivers left to the best-matching registered driver, the kit's own built-ins (AHCI, NVMe, USB mass storage) among them |
| Cleanup | The driver's own code | The kit's: a failed probe, and a USB device pulled out, are torn down for the driver |

If you find bugs or something abnormal, please [submit an issue](https://github.com/CosmosOS/Cosmos/issues/new/choose) on our repository.

## Enable the driver kit in your kernel

Referencing the kit is a build error until the kernel project acknowledges that it carries no compatibility promise. Add the suppression to your kernel's `.csproj`:

```xml
<PropertyGroup>
  <!-- Drivers/ uses the experimental driver kit; see docs/articles/user/drivers.md. -->
  <NoWarn>$(NoWarn);COSMOS0003</NoWarn>
</PropertyGroup>
```

The diagnostic is raised where your code names a kit type or calls `DriverManager`, not where it overrides a kit member: an empty `RegisterDrivers` override compiles without the suppression, and the first `DriverManager.Register` in it does not.

The kit needs PCI (`CosmosEnablePCI`, on by default). A USB driver also needs `CosmosEnableUsb`, which is on by default only when `CosmosEnableKeyboard` or `CosmosEnableStorage` is on: a kernel with neither that wants USB drivers sets it to `true`. What a driver publishes needs its subsystem too: `CosmosEnableMouse` for a mouse, `CosmosEnableNetwork` for a network link, `CosmosEnableStorage` for a disk, and a work item needs `CosmosEnableScheduler`.

Nothing else is referenced: the kit lives in `Cosmos.Kernel.HAL`, which every kernel gets through `Cosmos.Kernel.System`, and the layer rules let a kernel use it directly for this. These are the `using`s the snippets below rely on:

```csharp
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using Cosmos.Kernel.HAL.Drivers;
using Cosmos.Kernel.HAL.Drivers.Pci;
using Cosmos.Kernel.HAL.Drivers.Usb;
using Cosmos.Kernel.HAL.Interfaces.Devices;
using Cosmos.Kernel.System;
using Cosmos.Kernel.System.Drivers;
using Cosmos.Kernel.System.Storage;
```

## Quick start

A driver is a class deriving from `PciDriver` or `UsbDriver`, with one abstract member, `Probe`. The kit creates one instance per device it offers the driver, through a factory you give it, and calls `Probe` once with a context for that device. `Probe` answers `Bound` to keep the device, `Declined` when the device is not one it handles after all, or `Failed` when it could not bring it up.

This is the whole of DevKernel's USB boot mouse driver, `examples/DevKernel/Drivers/UsbBootMouseDriver.cs`, without its documentation comments. No built-in driver takes a boot mouse interface (the built-in keyboard driver takes the boot keyboard one), so the kit offers it to this driver:

```csharp
internal sealed class UsbBootMouseDriver : UsbDriver
{
    public const string Name = "usb-boot-mouse";

    private const byte HidClass = 0x03;
    private const byte BootSubclass = 0x01;
    private const byte MouseProtocol = 0x02;
    private const byte SetIdleRequest = 0x0A;
    private const byte SetProtocolRequest = 0x0B;
    private const ushort BootProtocol = 0;
    private const ushort ReportOnChange = 0;
    private const int MinimumReportLength = 3;
    private const int ButtonBits = 0x07;

    private MouseReporter? _mouse;

    public static UsbDriverRegistration CreateRegistration() =>
        new(Name, static () => new UsbBootMouseDriver(), UsbMatch.Interface(HidClass, BootSubclass, MouseProtocol));

    protected override ProbeResult Probe(UsbDeviceContext context)
    {
        UsbInterfaceInfo usbInterface = context.Interface;
        if (!usbInterface.TryFindEndpoint(UsbEndpointType.Interrupt, UsbDirection.In, out UsbEndpointInfo endpoint))
        {
            return ProbeResult.Declined;
        }

        if (context.ControlOut(UsbRequestKind.Class, UsbRecipient.Interface, SetProtocolRequest, BootProtocol,
                usbInterface.Number) != UsbTransferStatus.Success)
        {
            return ProbeResult.Failed;
        }

        // Some mice stall SET_IDLE and still work, so its status is ignored.
        _ = context.ControlOut(UsbRequestKind.Class, UsbRecipient.Interface, SetIdleRequest, ReportOnChange,
            usbInterface.Number);

        _mouse = context.PublishMouse();
        return context.OpenInterruptIn(endpoint, OnReport) ? ProbeResult.Bound : ProbeResult.Failed;
    }

    // Interrupt context: no allocation, no throw, no string, no lock.
    private void OnReport(ReadOnlySpan<byte> report)
    {
        if (report.Length < MinimumReportLength || _mouse is not { } mouse)
        {
            return;
        }

        MouseButtons buttons = (MouseButtons)(report[0] & ButtonBits);
        int wheel = report.Length > MinimumReportLength ? -(sbyte)report[3] : 0;   // HID counts up as positive
        mouse.Report((sbyte)report[1], (sbyte)report[2], wheel, buttons);
    }
}
```

The kernel registers it by overriding `RegisterDrivers`:

```csharp
public class Kernel : Cosmos.Kernel.System.Kernel
{
    protected override void RegisterDrivers()
    {
        if (KernelFeatures.Usb)
        {
            if (KernelFeatures.Mouse)
            {
                DriverManager.Register(UsbBootMouseDriver.CreateRegistration());
            }
        }
    }

    protected override void Run()
    {
        // The kernel's main loop, as in any kernel.
    }
}
```

Boot it with a USB mouse behind an xHCI controller (`cosmos run` has no switch for an xHCI without a USB disk, so pass these to QEMU yourself):

```console
-device qemu-xhci,id=xhci -device usb-mouse,bus=xhci.0
```

and the serial log shows the kit at work:

```
[Drivers] Registered usb-boot-mouse
[Drivers] usb-boot-mouse usb/1-5:1.0: published a mouse
[Drivers] usb/1-5:1.0 -> usb-boot-mouse (interface match)
```

From then on the USB mouse moves `MouseManager`'s pointer like a built-in one, and one plugged in later is bound the same way. DevKernel also carries a PCI driver, `examples/DevKernel/Drivers/Rtl8139Driver.cs`, for the Realtek RTL8139 NIC (`cosmos run --nic rtl8139`); the sections below quote it.

## Registering drivers

### When registration happens

`Global.StartKernel` calls `RegisterDrivers` once, on the boot thread, with interrupts on: after the built-in drivers HAL brings up bound their devices during HAL bring-up, after it registered the built-in drivers written against the kit (see [Built-in drivers](#built-in-drivers)), and before the driver pass offers all those drivers what HAL's built-ins left, before USB hot-plug starts and before `OnBoot`. See [Kernel Startup](startup.md) for the whole order. A kernel built without PCI never calls it, so ILC trims the override and every driver only it registers.

Registration closes as the pass starts. `DriverManager.Register` called later, from any thread, throws `InvalidOperationException`, and so does a call from inside a driver's factory, `Probe` or `Remove`. There is no way to add a driver once the kernel runs.

Registering from the kernel's constructor works too. The pass ranks those registrations together with the ones `RegisterDrivers` makes, and a constructor registration, being earlier, wins a tie against them, though never against a built-in. Prefer `RegisterDrivers`: the constructor runs in every build, with interrupts in no defined state, while `RegisterDrivers` runs only where PCI is compiled in, with interrupts on.

### What Register answers

| Result | When |
|---|---|
| `true` | The driver takes part in the pass (and, for a USB driver, in every hot-plug after it) |
| `false` | The kernel is built without PCI, or, for a USB registration, without USB; or the name is taken |
| `ArgumentNullException` | The registration is null |
| `InvalidOperationException` | Registration has closed, or the call comes from a driver's factory, `Probe` or `Remove` |

A registration's name is the owner recorded on every device the driver binds and the prefix of its log lines: make it short and lowercase, like `rtl8139`. PCI and USB registrations share one set of names, which also holds every built-in driver's (`e1000e`, `virtio-net`, `virtio-input`, `virtio-gpu`, `xhci`, `ahci`, `nvme`, `vmware-svga`, `hub`, `HID boot keyboard`, `mass storage`) and the boot display's `gop` reservation, so none of those can be registered. The registration constructors throw `ArgumentNullException` for a null name or factory, and `ArgumentException` for an empty name, an empty match list or a `default` match entry.

### Feature guards

Guard each registration with the `KernelFeatures` switches the driver needs, **one switch per `if`**. ILC folds a single switch to a constant and drops the branch it rules out, and with it the driver; a compound condition such as `KernelFeatures.Usb && KernelFeatures.Mouse` is not folded in Debug builds, so the driver stays in the kernel even when it can never run. `RegisterDrivers` itself needs no PCI guard, since only a kernel with PCI calls it. DevKernel's:

```csharp
protected override void RegisterDrivers()
{
    if (KernelFeatures.Network)
    {
        DriverManager.Register(Rtl8139Driver.CreateRegistration());
    }

    if (KernelFeatures.Usb)
    {
        if (KernelFeatures.Mouse)
        {
            DriverManager.Register(UsbBootMouseDriver.CreateRegistration());
        }
    }
}
```

Give each driver a static `CreateRegistration()` and call it from the kernel, as above, rather than registering from a `[ModuleInitializer]` in the driver's assembly: the kernel's call is what keeps the driver past trimming and what puts it behind the kernel's own switches, while a module initializer would sit outside them, and no test covers registering from one.

## Matching and ranking

A registration lists the devices it wants to be offered. For PCI:

| Entry | Matches |
|---|---|
| `PciMatch.Device(vendorId, deviceId)` | That vendor and device ID |
| `PciMatch.Class(baseClass, subclass, programmingInterface)` | That class triple |
| `PciMatch.Class(baseClass, subclass)` | That base class and subclass, any programming interface |

For USB, where a driver binds one interface at a time:

| Entry | Matches |
|---|---|
| `UsbMatch.Device(vendorId, productId)` | Each interface of that product, in turn |
| `UsbMatch.Interface(class, subclass, protocol)` | Interfaces with that class, subclass and protocol |
| `UsbMatch.Interface(class, subclass)` | Interfaces with that class and subclass |
| `UsbMatch.Interface(class)` | Interfaces with that class |

When several registrations match a device, the one whose best entry is most specific is offered it first, in the order of the tables above. A tie goes to a built-in driver the kit binds, then to the earlier registration. The device goes down that list until a `Probe` returns `Bound`. `Declined`, `Failed`, an exception thrown from the factory or `Probe`, and a factory that returns null all tear the attempt down and pass the device to the next candidate (for USB, only while the attempt opened no endpoint, see [USB drivers](#usb-drivers)). Every decision is logged:

```
[Drivers] pci/0000:00:03.0 -> rtl8139 (device match)
[Drivers] pci/0000:00:04.0 kept by xhci
[Drivers] pci/0000:00:05.0 -> my-driver declined
[Drivers] pci/0000:00:05.0 -> no driver
```

### Built-in drivers

The built-in drivers come in two kinds.

Some are brought up by HAL itself, during HAL bring-up, before any of your code runs: virtio, and E1000E on x64. HAL's USB core adds its hub and keyboard class drivers, which bind during the pass, as soon as the built-in xHCI driver publishes a controller to it. They keep what they take: the pass skips every PCI function with an owner, and USB offers every interface to the hub and keyboard drivers before the kit's. A registered driver cannot take a device from one of these at run time. To drive a device one of them claims, build the kernel without the switch that brings it up.

The others are kit drivers, written against the same public seam as yours, in the `Cosmos.Kernel.HAL.Drivers` assembly: today xHCI, which matches `PciMatch.Class(0x0C, 0x03, 0x30)` and publishes the host controller the USB core enumerates every USB device behind; AHCI, which matches `PciMatch.Class(0x01, 0x06, 0x01)` and publishes each SATA disk it finds; NVMe, which matches `PciMatch.Class(0x01, 0x08, 0x02)` and publishes each namespace it can drive; and USB mass storage, which matches `UsbMatch.Interface(0x08, 0x06, 0x50)`, SCSI over the Bulk-Only Transport, and publishes each logical unit with a medium as `usb0`, `usb1`, ..., the lowest number no unit present uses. `Global.StartKernel` registers them behind the kernel's feature switches (xHCI with `CosmosEnableUsb`, AHCI and NVMe with `CosmosEnableStorage`, mass storage with `CosmosEnableStorage` and `CosmosEnableUsb`), before it calls `RegisterDrivers`, and they bind like any registration, in the pass and, for mass storage, on the hot-plug thread for the sticks plugged in later, with two differences: their names are the reserved built-in names (`xhci`, `ahci`, `nvme`, `mass storage`), and they win every tie, even against a driver your constructor registered earlier. Only a strictly more specific match takes a device from one: a `PciMatch.Device(vendorId, deviceId)` entry for your controller or a `UsbMatch.Device(vendorId, productId)` entry for your stick, or a class triple against a class-only built-in. That is how a kernel replaces a built-in on one device it knows better, with no rebuild of the built-in. A kernel built without `CosmosEnableStorage` registers none of them, and ILC keeps none of their code: the Drivers test suite builds that way, which leaves the NVMe controller to its own sample driver.

Two of HAL's built-ins reach further than their name suggests:

- On x64, the built-in E1000E driver takes every Intel Ethernet controller (class `02/00/00`) it finds, not only the 82574 family, so QEMU's `e1000` is never offered to a registered driver there.
- The display function the boot framebuffer lives in is reserved as `gop` when PCI is enumerated, so no registered driver can resize or reprogram the BAR the console draws into.

A PCI-to-PCI bridge is never offered either: the kit binds endpoints only.

## Where each callback runs

A driver's code runs in five places, and what it may do depends on which:

| Callback | Runs on | Interrupts | May | Must not |
|---|---|---|---|---|
| Factory, `Probe` | The boot thread during the pass, which is CPU 0's idle thread; the USB hot-plug thread for a USB device plugged in later. Do not depend on which | On | Allocate, acquire resources through the context, synchronous USB transfers, `Delay`, `WriteLog`, throw (counts as `Failed`) | Sleep or block, including `DeviceEvent.Wait`: it throws on an event this `Probe` created, and on any other event it would block the probing thread; `DriverManager.Register`, which throws |
| `UsbDriver.Remove` | The USB hot-plug thread | On | USB transfers (they answer `Disconnected`), `StorageManager.UnregisterDevice`, `WriteLog` | Acquire resources (the Probe-only members throw); `Register` |
| `DeviceInterruptHandler`, `UsbReportHandler` | Interrupt context: the device's MSI-X vector, the timer interrupt when polled, the xHCI interrupt, or a thread draining the xHCI's events with interrupts masked | Masked | Region accessors, `DmaBuffer.Span`, `IrqSafeLock`, `DeviceWorkItem.Schedule`, `DeviceEvent.Signal`, `MouseReporter.Report`, `NetworkLink.SetLinkState`, `IsPresent` | Allocate, throw, block, take any lock but an `IrqSafeLock`, call through an interface, build a string (so no `WriteLog`) |
| `DeviceWorkItem` callback | The kit's `driver-work` thread, one item at a time | On | Anything a thread may do: allocate, `DeviceEvent.Wait`, transfers, `NetworkLink.Deliver`, `PublishBlockDevice` | Acquire resources; `Register` |
| `NetworkTransmitHandler` | The thread the network stack sends from, one call at a time | Masked | Region accessors, `DmaBuffer`, `IrqSafeLock` | Block |

The handlers never run before `Probe` returned `Bound`. A polling tick or a USB report that comes during `Probe` is dropped. An MSI-X message the device raises during `Probe` is not: it waits in the masked entry and reaches the handler once `Probe` returned `Bound` (see [Interrupts](#interrupts)), so a handler reads the device's own status rather than assuming one call per event, and must expect a call for something `Probe` already handled. Nothing reaches a handler once its attempt is torn down.

The interrupt-context rules are not checked by the compiler, and breaking them has no soft landing: an exception thrown from a handler halts the kernel, after the serial log names the driver and the device (`[Drivers] rtl8139 pci/0000:00:03.0: the interrupt handler threw`). A handler reads the device, acknowledges it, and hands anything longer to a work item:

```csharp
// Interrupt context: no allocation, no throw, no string, no lock.
private void OnInterrupt(int vector)
{
    if (_registers is not { } registers)
    {
        return;
    }

    ushort status = registers.Read16(InterruptStatus);
    if (status == 0)
    {
        return;
    }

    // Write 1 to clear.
    registers.Write16(InterruptStatus, status);
    if ((status & ReceiveCauses) != 0)
    {
        _receiveWork?.Schedule();
    }
}
```

An exception from a work item is logged with the driver's name and the device's path, and that work item never runs again. An exception from `Remove` is logged and the unplug goes on; one from a transmit handler is logged and the send counts as failed.

## PCI drivers

### What the context hands out

Everything a driver acquires comes from its `PciDeviceContext`, during `Probe` only: the acquiring members throw `InvalidOperationException` anywhere else. Before `Probe` runs, the kit has sized every BAR (with decoding off), saved the Command register, and turned the function's INTx line and bus mastering off, and MSI-X too if firmware left it on: MSI-X Enable is set only while the kit delivers the driver's interrupts through it.

| Member | What it gives |
|---|---|
| `Function` | The IDs, `RevisionId` and class triple, readable in any context, and `ReadConfig8/16/32` and `TryFindCapability`, for thread context |
| `TryMapBar(index, out region)` | A memory BAR as an `MmioRegion`, with memory decoding on; false for an I/O BAR, an empty slot, the upper half of a 64-bit BAR, one firmware left unassigned, or one the platform cannot map |
| `TryMapIOBar(index, out region)` | An I/O BAR as a `PortRegion`, with I/O decoding on; always false on ARM64, which has no port space |
| `TryAllocateDma(length, maximumDeviceAddress, out buffer)` | Zeroed, physically contiguous pages as a `DmaBuffer`, or false when they would end above `maximumDeviceAddress` |
| `EnableBusMastering()` | Lets the function master the bus. Call it once the device is reset and its DMA addresses are programmed |
| `TryRequestInterrupts(handler)` | The function's interrupts, see [Interrupts](#interrupts) |
| `CreateEvent()`, `TryCreateWorkItem(callback, out item)` | See [Work items and events](#work-items-and-events) |
| `PublishMouse()`, `PublishNetworkLink(address, transmit)`, `PublishBlockDevice(device)` | See [Publishing to the kernel](#publishing-to-the-kernel) |
| `WriteConfig8/16/32(offset, value)` | Config writes at offset `0x40` and above, in thread context, during `Probe` and after it; the header below `0x40` is the kit's, and a write there throws `ArgumentOutOfRangeException` |

`MmioRegion` and `PortRegion` check every access: an offset past the end, or not a multiple of the access size, throws `ArgumentOutOfRangeException`, and every access to a region of an attempt that was torn down throws `InvalidOperationException`. They also order the device's view of memory for you: each write is preceded by a DMA write barrier, so the device sees the descriptors the CPU filled before the register write that tells it to look, and each read is followed by a DMA read barrier, so nothing read from DMA memory afterwards runs ahead of the register that said it is there. Between two DMA-memory accesses with no register access in between, use `DmaBuffer.WriteBarrier()` and `DmaBuffer.ReadBarrier()`.

This is how the RTL8139 driver brings its chip up, from `Rtl8139Driver.Probe`:

```csharp
if (!context.TryMapBar(RegisterBar, out MmioRegion? registers))
{
    return ProbeResult.Declined;
}

registers.Write8(CommandRegister, CommandReset);
int polls = 0;
while ((registers.Read8(CommandRegister) & CommandReset) != 0)
{
    if (++polls == ResetPolls)
    {
        context.WriteLog("the reset did not complete");
        return ProbeResult.Failed;
    }

    context.Delay(TimeSpan.FromMicroseconds(10));
}

// The chip takes 32-bit bus addresses.
if (!context.TryAllocateDma(ReceiveBufferLength, ThirtyTwoBitBus, out DmaBuffer? receiveBuffer)
    || !context.TryAllocateDma(TransmitSlotCount * TransmitSlotLength, ThirtyTwoBitBus, out DmaBuffer? transmitBuffer))
{
    context.WriteLog("no DMA memory below 4 GiB");
    return ProbeResult.Failed;
}

if (!context.TryCreateWorkItem(DrainReceiveRing, out DeviceWorkItem? receiveWork))
{
    context.WriteLog("needs the scheduler for its receive path");
    return ProbeResult.Failed;
}

if (!context.TryRequestInterrupts(OnInterrupt))
{
    context.WriteLog("no MSI-X and no ticking timer to poll with");
    return ProbeResult.Failed;
}

registers.Write32(ReceiveBufferStart, (uint)receiveBuffer.DeviceAddress);
for (int slot = 0; slot < TransmitSlotCount; slot++)
{
    registers.Write32(TransmitAddress0 + (ulong)(slot * sizeof(uint)),
        (uint)transmitBuffer.DeviceAddress + (uint)(slot * TransmitSlotLength));
}

// Only once the chip is reset and knows where its buffers are, so it
// can never master through an address firmware left behind; and
// before the receiver and transmitter are enabled below, which is
// when it starts to.
context.EnableBusMastering();

// RCR before the receiver is enabled: writing it also starts the
// chip's ring at the buffer's first byte, where _receiveOffset starts.
registers.Write32(ReceiveConfig, ReceiveAcceptAll | ReceiveWrap);
registers.Write8(CommandRegister, CommandReceiveEnable | CommandTransmitEnable);
```

`Delay` busy-waits, which is all a probe may do to wait: during the pass it runs on the idle thread, which cannot sleep. `WriteLog` writes one serial line, prefixed with `[Drivers]`, the driver's name and the device's path.

DMA memory comes from the kernel's page allocator, which manages the largest usable region of the firmware's memory map and has no separate low-memory pool. A device that addresses less than the machine's memory can therefore fail to get a buffer: on QEMU's ARM64 `virt` machine RAM starts at 1 GiB, so a device limited to 28 bits never gets one, and on a machine whose largest region lies above 4 GiB, a 32-bit device does not either. `DeviceAddress` is the physical address today; the name leaves room for an IOMMU.

The PCI configuration space the kit reaches is the first 256 bytes. The subsystem IDs have no member of their own: read them with `Function.ReadConfig16(0x2C)` and `ReadConfig16(0x2E)`.

### Interrupts

`TryRequestInterrupts(handler)`, once per `Probe`, routes the function's interrupts to `handler` from the moment `Probe` returns `Bound`, never before. The kit picks the source:

- **MSI-X**, when the function has the capability and the platform can route it: the local APIC on x64, a GICv3 ITS on ARM64. The kit programs entry 0 masked during `Probe`; on `Bound` it turns bus mastering on, which an MSI-X message needs, and unmasks the entry, so a message the device raised meanwhile is delivered then.
- **Polling** otherwise: the kit calls the handler from the platform timer's interrupt on every tick, about every 55 ms on x64 and every 10 ms on ARM64. The handler runs whether or not the device raised anything, so it reads the device's own status to find out, as `OnInterrupt` above does.

INTx and plain MSI are never used. `TryRequestInterrupts` returns false when the function has neither MSI-X the platform can route nor a ticking timer to poll from: on ARM64 without the scheduler, on x64 with ACPI off, and in a kernel without the timer. Whether the timer ticks is measured once, before the first probe of the pass. The handler's `vector` argument is always 0: this version grants one interrupt per binding.

### Work items and events

`TryCreateWorkItem(callback, out item)` creates a `DeviceWorkItem` whose callback runs on the kit's `driver-work` thread each time the item is scheduled. `Schedule()` may be called from anywhere, the interrupt handler included; it returns false when the item already waits to run or can never run again. A work item scheduled during `Probe` runs once `Probe` returned `Bound`, and never if the attempt is declined or fails. The first `TryCreateWorkItem` starts the thread; it returns false when there is no scheduler to run it.

`CreateEvent()` creates a `DeviceEvent`: the handler `Signal()`s it, a thread `Wait()`s for it. Signals are counted. `Wait()` returns false once the binding is gone, and throws inside the `Probe` that created the event, since the interrupts that would signal it are armed only after `Bound`. On the idle thread it polls instead of blocking. `Wait(timeout)` does the same, and also returns false once `timeout` passes without a signal: the wait for a driver that must turn a lost interrupt into an error rather than a hang, as the built-in NVMe driver does. A blocked thread still gives the CPU up, and the scheduler wakes it on the first tick after the timeout.

This is the RTL8139's receive work item, which hands every frame in the ring to the network stack:

```csharp
// The receive work item: driver-work thread, where it may allocate.
private void DrainReceiveRing()
{
    if (_registers is not { } registers || _receiveBuffer is not { } buffer || _link is not { } link)
    {
        return;
    }

    Span<byte> ring = buffer.Span;

    // Each Command read is followed by a DMA read barrier, so the ring
    // loads below see what the chip wrote before it cleared BUFE.
    while ((registers.Read8(CommandRegister) & CommandBufferEmpty) == 0)
    {
        ushort status = BinaryPrimitives.ReadUInt16LittleEndian(ring[_receiveOffset..]);
        int length = BinaryPrimitives.ReadUInt16LittleEndian(ring[(_receiveOffset + 2)..]);
        if ((status & ReceiveStatusOk) == 0 || length < MinimumReceivedLength || length > MaximumReceivedLength)
        {
            break;
        }

        link.Deliver(ring.Slice(_receiveOffset + ReceiveHeaderLength, length - CrcLength));
        // ... advance the ring's read pointer
    }
}
```

### When an attempt fails

A driver writes no teardown code. When `Probe` declines, fails or throws, the kit undoes the attempt in a fixed order: it disarms the interrupts (masks the MSI-X entry, stops the polling), drops what `Probe` published and the work items it scheduled and cancels its events, writes the Command register back as it found it but with bus mastering off, frees the DMA memory, turns MSI-X off and gives the vector back, and makes the regions throw. Then the next candidate is offered the function.

A PCI function a driver bound stays bound for the life of the kernel: there is no unbind, rebind or remove for PCI in this version.

## Publishing to the kernel

A driver does not talk to the kernel's managers itself: it publishes what its device is, and the kit delivers it right after `Probe` returns `Bound`, before the interrupts are armed, on the thread that ran the probe. What a failed attempt published is dropped and never reaches a manager. A disk may also be published later, from a work item, see below.

**A mouse.** `PublishMouse()` returns a `MouseReporter`. Each `Report(deltaX, deltaY, wheel, buttons)` moves `MouseManager`'s pointer exactly as a built-in mouse does: positive `deltaY` moves down, a negative `wheel` scrolls up, and `buttons` is a `MouseButtons` of the buttons held after the move. `Report` allocates nothing and may be called from the interrupt handler. See [Mouse](mouse.md) for what the kernel reads back.

**A network interface.** `PublishNetworkLink(address, transmit)` publishes a device with its MAC address; the network stack sends through `transmit` and the driver hands it received frames through the returned `NetworkLink`. The kit registers it with `NetworkManager` after the devices the built-in drivers registered, so the primary device does not change unless there was none, and from then on DHCP, UDP and TCP run over it (see [Network](network.md)). `Deliver(frame)` is for thread context, typically a work item: it copies the frame into a new array the stack keeps, runs the stack's receive path with interrupts masked, and drops the frame while the stack has not configured the link. `SetLinkState(isUp)` may be called from anywhere. The transmit handler gets one frame at a time, with interrupts masked, and answers false when the device cannot take it now:

```csharp
// Interrupts masked, one call at a time: the kit serializes it.
private bool Transmit(ReadOnlySpan<byte> frame)
{
    if (_registers is not { } registers || _transmitBuffer is not { } buffer || frame.Length > TransmitSlotLength)
    {
        return false;
    }

    int slot = _nextTransmitSlot;
    ulong status = TransmitStatus0 + (ulong)(slot * sizeof(uint));
    if ((registers.Read32(status) & TransmitHostOwns) == 0)
    {
        return false;   // the chip is still copying this slot
    }

    Span<byte> data = buffer.Span.Slice(slot * TransmitSlotLength, TransmitSlotLength);
    frame.CopyTo(data);
    int length = Math.Max(frame.Length, MinimumFrameLength);
    data[frame.Length..length].Clear();

    // OWN = 0 starts the copy; the register write's DMA write barrier orders it after the stores above.
    registers.Write32(status, (uint)length);
    _nextTransmitSlot = (slot + 1) % TransmitSlotCount;
    return true;
}
```

`PublishMouse` and `PublishNetworkLink` throw `InvalidOperationException` in a kernel built without mouse or network support, which is why DevKernel registers each driver behind the switch it publishes to.

**A disk.** `PublishBlockDevice(device)` publishes a disk the driver implements as an `IBlockDevice`, the way the built-in AHCI, NVMe and USB mass storage drivers publish each SATA disk, NVMe namespace and logical unit they find. The kit registers it with `StorageManager`, which reads its partition table through it straight away, with real reads (see [File System](filesystem.md)), on the thread that delivers it, and ranks it among the kernel's disks by the primary-disk rule: a disk that cannot leave the machine before one that can (a USB driver's is one that can), then AHCI disks, then NVMe namespaces, then any other, then by the PCI function behind them, then in registration order. Where the driver publishes it decides when those reads happen:

- From `Probe`, the disk is delivered right after `Probe` returns `Bound`, before the interrupts are armed, so its I/O must complete without them, by polling the device, at least until the handler first runs. AHCI always polls, and so do a USB driver's bulk transfers, which are synchronous. NVMe completes through MSI-X, but until its handler first runs, which the kit lets happen only once it armed it, the thread that issued a command polls the completion queue itself: the partition scan the delivery starts completes that way, and the commands after it wait for the interrupt. The disk is registered when the pass returns, with no thread to wait for.
- From one of the binding's own work items, once `Bound`, the disk is delivered before the call returns, on the `driver-work` thread, with the interrupts armed. An interrupt-driven driver, whose I/O waits for its completion interrupt, publishes from there: from a work item scheduled in `Probe`, which runs once `Probe` returned `Bound`. The partition scan holds the `driver-work` thread meanwhile, and it runs one work item at a time, so the disk's completions must reach its I/O from the interrupt handler, with `DeviceEvent.Signal`, never through another work item, which would wait for the scan that waits for it.

Anywhere else, `PublishBlockDevice` throws `InvalidOperationException`, and so it does in a kernel built without storage support, so guard the driver with `KernelFeatures.Storage`. A disk an attempt published from `Probe` is dropped if the attempt then fails, and never reaches `StorageManager`. When a USB device leaves the bus, the kit takes its disks back out of `StorageManager` before the driver's `Remove` runs, which detaches the filesystems mounted from them without a flush, and every read, write or flush through them throws `IOException` from then on. The disk's `Name` must be unique among the kernel's disks, such as `sata0`, since its partitions are named after it.

A driver in the kernel project, which references `Cosmos.Kernel.System`, can still register a disk with the public `StorageManager.RegisterDevice` itself and take it out in `Remove` with `StorageManager.UnregisterDevice`; publishing does both for it, and is the only way for a driver library that references the kit alone.

Keyboards and graphics have no publication: a driver cannot feed `KeyboardManager`, and a GPU driver can hand out its own `Canvas` but cannot become the console.

## USB drivers

USB drivers bind interfaces, not devices. The USB stack offers every interface of a configured device to HAL's class drivers first (hub, boot keyboard), then to the kit, which ranks the registered drivers, the built-in mass storage driver among them: at boot for the devices already there, during the pass, and on the USB hot-plug thread for every device plugged in later. Neither the host controller nor HAL's class drivers know a registered driver exists.

`UsbDeviceContext` gives the driver:

| Member | What it does |
|---|---|
| `Device`, `Interface` | The device descriptor's IDs and class, and every interface of the active configuration with its endpoints (alternate setting 0 of each); `Interface` is the one on offer. Readable in any context |
| `ControlIn`, `ControlOut` | Control requests on the device's default pipe, in thread context, with at most 4096 bytes of data. `ControlIn` reports how many bytes the device sent |
| `OpenInterruptIn(endpoint, handler)` | Opens an interrupt IN endpoint of the interface; every report goes to `handler`, in interrupt context, from `Bound` on. Probe only |
| `TryOpenBulk(endpoint, out pipe)` | Opens a bulk endpoint of the interface as a `UsbBulkPipe`: `Read`, `Write` and `ClearHalt`, synchronous, thread context, one transfer at a time. Probe only |

Find an endpoint with `Interface.TryFindEndpoint(type, direction, out endpoint)` and hand it back to the open call; an endpoint of another interface is refused.

**Opening an endpoint commits the interface.** The host controller cannot close an endpoint, and an interrupt IN endpoint starts transferring as soon as it is open. So when an attempt that opened one declines or fails, the kit offers the interface to no other driver, and it stays without one:

```
[Drivers] usb/1-6:1.0 -> no driver: usb-tablet-fails opened an endpoint, which cannot be closed, so no other driver is offered the interface
```

Check everything you can before opening, the descriptors and the control requests, and open endpoints last, as the boot mouse driver does. An attempt that opened nothing passes the interface on like a PCI one.

**When the device is pulled out**, the kit ends the binding on the hot-plug thread, before the USB stack frees the device's pipes, in this order:

1. `IsPresent` turns false, and the report handlers stop being called.
2. What the driver published leaves its manager: the mouse is taken out of `MouseManager`, releasing the buttons it held, the network link out of `NetworkManager`, the stack forgetting its addresses, and the disks out of `StorageManager`, their filesystems detached.
3. The work items are dropped and the events cancelled; a work item already running gets up to a second to return, and an overrun is logged.
4. The driver's `Remove(context)` runs, for what the kit does not know about, such as a disk it registered with `StorageManager` itself. The default does nothing.
5. The context is left answering: every transfer, through it or its bulk pipes, returns `UsbTransferStatus.Disconnected`, and the Probe-only members throw.

The next time the device is plugged in, the factory creates a new driver instance for it, with a new context.

A host controller without MSI-X is polled: the hot-plug thread asks it every 250 ms for port changes and for the events that carry reports, so a report handler behind such a controller runs at that pace. That is the case on ARM64 with GICv2, which is QEMU's default machine; with GICv3, and on x64, the xHCI interrupts.

## Listing devices

`DriverManager.Devices` lists every PCI function, in bus order, then every interface of every configured USB device, each as a `DeviceInfo`: its `Path`, `DriverName`, `VendorId`, `DeviceId` (a USB device's product ID), `Class`, `Subclass` and `Protocol` (for PCI, the programming interface). The built-in drivers' devices are listed too. `DriverName` is null for a device no driver owns, and `gop` for the reserved boot display. The list is empty before the pass and in a kernel built without PCI; after boot it only changes when a USB device comes or goes, and each change replaces it, so reading it again while nothing changed returns the same list.

```csharp
foreach (DeviceInfo device in DriverManager.Devices)
{
    Console.WriteLine($"{device.Path}  {device.VendorId:x4}:{device.DeviceId:x4}  {device.DriverName ?? "-"}");
}
```

Paths follow Linux's naming: `pci/0000:00:03.0` is segment, bus, device and function in hexadecimal, and `usb/1-2.1:1.0` is the host controller, the root port and each hub port below it, then the configuration value and the interface number, in decimal. A context's `Path` is the same string.

DevKernel's `lsdev` command prints the list. Booted with an RTL8139 and a USB mouse (`-netdev user,id=n1 -device rtl8139,netdev=n1 -device qemu-xhci,id=xhci -device usb-mouse,bus=xhci.0`):

```
cosmos:/$ lsdev
Devices:
  PATH              ID         CLASS     DRIVER
  pci/0000:00:00.0  8086:1237  06/00/00  -
  pci/0000:00:01.0  8086:7000  06/01/00  -
  pci/0000:00:01.1  8086:7010  01/01/80  -
  pci/0000:00:01.3  8086:7113  06/80/00  -
  pci/0000:00:02.0  1234:1111  03/00/00  gop
  pci/0000:00:03.0  10ec:8139  02/00/00  rtl8139
  pci/0000:00:04.0  1b36:000d  0c/03/30  xhci
  usb/1-5:1.0       0627:0001  03/01/02  usb-boot-mouse

8 devices, 3 with a driver.
'gop' marks the boot display: reserved, so no registered driver is offered it.
```

A driver that matched a device but declined or failed leaves no trace in the list: read the `[Drivers]` lines of the serial log for why (see [Debugging](debugging.md#debugging-a-driver)).

## Drivers in a library

A driver does not have to live in the kernel project. A driver library is a plain class library, with no Cosmos SDK, that references the kit at the **exact** version of the kernel's `Cosmos.Kernel.System` package reference:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <!-- The drivers are written against the experimental driver kit (COSMOS0003). -->
    <NoWarn>$(NoWarn);COSMOS0003</NoWarn>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Cosmos.Kernel.HAL" Version="[3.0.89]" />
  </ItemGroup>
</Project>
```

Pin the version with brackets. ILC compiles the kernel without `--resilient`, so a driver built against another version of the kit, calling a member the kernel's version does not have, fails the kernel's build with ILC error IL1005, "Method will always throw". The library exposes each driver's `CreateRegistration()`, and the kernel registers it, behind its own switches, as for a driver of its own.

Override `Probe` and `Remove` as `protected override`. Only an assembly that `Cosmos.Kernel.HAL` grants its internals to (`InternalsVisibleTo`: `Cosmos.Kernel.System`, `Cosmos.Kernel`, the arch HALs and a few of the repository's test kernels) sees them as `protected internal`, and has to override them that way, or the build fails with CS0507.

The repository builds drivers this way twice. The built-in drivers written against the kit, in `src/Cosmos.Kernel.HAL.Drivers`, take no `InternalsVisibleTo` grant, so they reach the kit through its public seam only, exactly as your library does; `Global.StartKernel` registers them. The Drivers test suite builds its drivers as the library `tests/Kernels/SampleDrivers`, referenced as a project by the kernel. A driver library taken from a NuGet feed is not covered by a test yet.

## Architecture differences

Drivers written against the kit run on x64 and ARM64 unchanged, within these limits of ARM64:

- **No port I/O.** `TryMapIOBar` always returns false: use the device's memory BARs. The RTL8139 driver reaches its registers through memory BAR 1 for this reason, where an x64-only driver could use I/O BAR 0.
- **MSI-X needs a GICv3 ITS.** Without one, as on QEMU's default GICv2 `virt` machine (`cosmos run -a arm64`), interrupts are polled and the xHCI is too. Run QEMU with `-M virt,gic-version=3` for MSI-X.
- **PCI needs ACPI's MCFG table**, which says where the configuration space is. Without it, as with QEMU's `acpi=off`, PCI does not work, and neither does the kit.
- **DMA is assumed coherent**, as it is on x64 and on QEMU: the kit does no cache maintenance, only ordering. Boards whose devices do not snoop the CPU caches are not supported.

The interrupt polling interval differs too, about 55 ms on x64 against 10 ms on ARM64.

## Current limitations

- The kit is experimental: its API can change in any release, which is why a driver library pins the exact version.
- Registration closes when the pass starts. A driver cannot be registered once the kernel runs.
- A registered driver cannot displace one of the built-ins HAL brings up at run time; build without the built-in's switch instead. It can take a device from a kit built-in (AHCI, NVMe, USB mass storage) with a more specific match.
- A PCI binding is never released: no remove, rebind or shutdown for PCI.
- There is no shutdown quiesce: `Power.Shutdown` and `Power.Reboot` give drivers no callback, so a device can still be doing DMA when the machine goes down, and nothing is flushed first.
- The kernel runs on one CPU. `IrqSafeLock` masks interrupts and spins, which is all a single CPU needs.
- One interrupt per binding, with no way to mask it; no INTx or plain MSI.
- No keyboard or graphics publication.
- USB: one interface per driver, alternate setting 0 only, no isochronous or interrupt OUT endpoints, at most 4096 bytes per control request, and no second candidate once an attempt opened an endpoint.
- `DeviceInfo` does not say why a device has no driver; the serial log does.
- Nothing checks the interrupt-context rules at build time.
- In the **kernel project**, a `const` field or a parameter's default value whose type is one of the kit's enums (`MouseButtons`, `UsbEndpointType`, ...), nullable or not (`MouseButtons? buttons = MouseButtons.Left`), breaks the build: the IL patcher, which rewrites the kernel assembly, cannot resolve `Cosmos.Kernel.HAL` to find the enum's underlying type when it writes the constant back (`AssemblyResolutionException` in the build log), and ILC then fails on the empty file it left ("A positive capacity must be specified for a Memory Mapped File backed by an empty file"). For a field, use `static readonly`, or a literal. For a parameter, drop the default: add an overload without the parameter, or pass the value at every call. A local `const` is not affected, and neither is a driver library, which the patcher does not rewrite.
- Plugging the kit's internals, or the USB and PCI managers behind it, is unsupported: they change without notice. Register a driver instead of plugging `UsbManager` to add one.

## How it works

HAL's built-in drivers bind during HAL bring-up, before any kernel code runs. The kit's built-in drivers and the drivers you register bind late, in one pass that `Global.StartKernel` runs on the boot thread, then on the USB hot-plug thread for every device plugged in afterwards:

```
HAL bring-up         HAL's built-in drivers bind: virtio, xHCI (+ hub, keyboard), E1000E (x64)
        │
Global.StartKernel   interrupts on
        ├─ built-in kit drivers         AHCI, NVMe (with storage), USB mass storage (with storage and
        │                               USB), registered ahead of yours (only with PCI)
        ├─ Kernel.RegisterDrivers()     your registrations (only with PCI)
        ├─ the driver pass              every free PCI function, then every free USB interface,
        │                               offered to the matching drivers, best match first
        ├─ USB hot-plug thread starts   later devices: HAL's class drivers first, then the kit's;
        │                               unplug runs the teardown and Remove
        └─ Kernel.Start()               OnBoot, BeforeRun, Run
```

Each binding attempt gets a context that records everything the driver acquires through it, which is what lets the kit undo a failed attempt and end a USB binding without the driver's help. The kit lives in the assembly `Cosmos.Kernel.HAL` (namespaces `Cosmos.Kernel.HAL.Drivers`, `.Pci` and `.Usb`), its registration side in `Cosmos.Kernel.System.Drivers`, and the built-in drivers written against it in the assembly `Cosmos.Kernel.HAL.Drivers`, one namespace per bus and driver below `Cosmos.Kernel.HAL.Drivers.BuiltIn` (`.BuiltIn.Pci.Ahci` and `.BuiltIn.Pci.Nvme` for the drivers that bind a PCI function, `.BuiltIn.Usb.Xhci` and `.BuiltIn.Usb.MassStorage` for the USB stack's host controller and class driver).
