# Kernel Startup

In this article, we will discuss what happens between power-on and the first call to your kernel's `Run()` method: the boot chain, the initialization phases, and the `BeforeRun`/`Run`/`AfterRun` lifecycle your kernel is built on.

The main differences if you come from Gen2:

| | Gen2 | Gen3 |
|---|---|---|
| Compiler | IL2CPU | .NET NativeAOT (ILC) + the Cosmos IL patcher |
| Firmware | BIOS | UEFI, on x64 and ARM64 |
| Bootloader | Limine | Limine |
| Entry plumbing | IL2CPU emits the call to `Kernel.Start()` | A source generator emits `CosmosEntryPoint.Main()` |
| Lifecycle | `BeforeRun` / `Run` / `AfterRun` | `BeforeRun` / `Run` / `AfterRun` (unchanged) |

## The boot chain

```
UEFI firmware
    │
Limine            loads kernel.elf, maps it in the higher half, sets up the framebuffer
    │
kmain()           native C bootstrap (Cosmos.Kernel/Bootstrap/kmain.c)
    ├─ Phase 1    CPU: enable SIMD, initialize the serial port
    ├─ Phase 2    Platform: RSDP + HHDM from Limine, early ACPI parse (MADT, MCFG)
    ├─ Phase 3    Managed runtime: heap, GC, type system, library initializers
    └─ Phase 4    User kernel: Main(argc, argv) → Kernel.Start()
```

The [Limine](https://limine-bootloader.org/) bootloader loads the kernel ELF produced by the build pipeline, and jumps to `kmain()`, a small C bootstrap compiled into every kernel. From there:

- **Phase 1: CPU.** SIMD is enabled first (NativeAOT-generated code uses XMM registers from the very first instruction) and the serial port is initialized, so everything after this line is logged. On ARM64 the alignment check is disabled here too.
- **Phase 2: Platform.** The bootstrap asks Limine for the ACPI RSDP and the higher-half direct-map offset, then does an early ACPI parse: the MADT (where the interrupt controllers and CPUs are) and the MCFG (where PCIe configuration space lives).
- **Phase 3: Managed runtime.** The NativeAOT startup path runs. This is where the C# world comes alive, one package at a time (see the next section).
- **Phase 4: User kernel.** The bootstrap builds `argv` from the kernel command line and calls the managed `Main`, which ends up in your kernel's `Start()`.

## Phase 3: how the managed kernel comes up

Each Cosmos package contributes a *library initializer* that the runtime executes before any of your code. The SDK orders them in three stages: the heap first, then the runtime's own initializers, then the kernel libraries, each after the packages it references.

1. **Cosmos.Kernel.Core**: carves the heap out of the Limine memory map, initializes the garbage collector, then registers the type system (statics, eager static constructors, module initializers). Nothing allocates before this step.
2. **The runtime's own initializers** (`System.Private.CoreLib` and its companions): the preallocated `OutOfMemoryException`, the class constructor runner, the type loader and reflection callbacks, stack trace metadata. The class constructor runner is created here, so a static field whose type has a lazy static constructor can be read from this step on and not before.
3. **Cosmos.Kernel.HAL**: platform HAL, the interrupt controller, PCI enumeration over ECAM, platform hardware (APIC/GIC, timers, the PS/2 controller), then the machine's root platform nodes are published into the driver kit right after `InitializeHardware` (the PCI host: `platform:pci@cf8` on x64, the ECAM host from MCFG on ARM64; and on ARM64 one `platform:virtio_mmio@...` node per occupied slot of the virt machine's virtio-mmio window, whatever the switches say), where they wait for the driver stage; then the USB host controllers with their keyboard and mass storage drivers. The AHCI and NVMe controllers are not brought up here: their drivers live in the driver kit and bind them during the driver stage. Last, whenever graphics are on, interrupts or not, the framebuffer Limine handed over is recorded (`[KERNEL]   - Recording the firmware framebuffer...`) for the driver stage to publish as the firmware display.
4. **Cosmos.Kernel**: CPU exception handlers and the scheduler (one idle thread per CPU, preemption on a 10 ms quantum).
5. **Cosmos.Kernel.System**: the service managers `TimerManager`, `KeyboardManager`, `MouseManager`, `NetworkManager`, `DisplayManager`, `StorageManager`. `KeyboardManager`, `MouseManager`, `NetworkManager`, `DisplayManager` and `StorageManager` each install their driver kit consumer here, before the driver stage, so a keyboard, a pointer, an interface, a display or a disk a kit driver publishes reaches its manager, and so does the firmware display the driver stage publishes first; no platform network device is registered here, and the first interface a kit driver publishes becomes the primary. The storage manager registers the USB mass storage units the HAL found here; its AHCI and NVMe disks arrive through its consumer during the driver stage.

Every step in 3-5 is gated by a feature switch (`CosmosEnableInterrupts`, `CosmosEnablePCI`, `CosmosEnableTimer`, `CosmosEnableKeyboard`, `CosmosEnableMouse`, `CosmosEnableNetwork`, `CosmosEnableStorage`, `CosmosEnableGraphics`, `CosmosEnableScheduler`, all `true` by default). Set one to `false` in your `.csproj` and the corresponding subsystem is skipped here and compiled out of the kernel.

## The generated entry point

You never write a `Main` for a Cosmos kernel. The SDK ships a source generator that emits it from the `CosmosKernelClass` project property:

```csharp
// <auto-generated/> CosmosEntryPoint.g.cs
namespace Cosmos.Kernel.System.Internal;

public static class CosmosEntryPoint
{
    public static void Main()
    {
        global::Cosmos.Kernel.System.Internal.DriverManifest.Register();
        global::Cosmos.Kernel.System.Global.RegisterKernel(new global::MyOS.Kernel());
        global::Cosmos.Kernel.System.Global.StartKernel();
    }
}
```

`DriverManifest` is the second generated file: the list of driver classes the kernel carries, registered before the kernel starts so the driver stage in `Start()` can offer them devices. A kernel with no drivers of its own still carries the ten Cosmos ships in `Cosmos.Kernel.Drivers`: the PCI host driver, the Intel E1000E driver, the virtio PCI and MMIO transport drivers, the virtio-net and virtio-input drivers, the two display drivers, virtio-gpu and VMware SVGA II, and the two storage drivers, AHCI and NVMe, eight of them behind a feature switch. The [driver manifest](../dev/build/driver-manifest.md) page describes how the list is built and how a project excludes or opts into a driver.

`CosmosKernelClass` defaults to `<RootNamespace>.Kernel`, so a class named `Kernel` in your project's root namespace is picked up automatically. To use a different type, set it explicitly:

```xml
<PropertyGroup>
  <CosmosKernelClass>MyOS.Boot.MyKernel</CosmosKernelClass>
</PropertyGroup>
```

`Global.StartKernel()` then calls `Start()` on the registered instance.

## Sys.Kernel.Start()

`Cosmos.Kernel.System.Kernel` is the abstract base class of every user kernel. Its `Start()` drives the whole lifecycle:

1. Enables hardware interrupts (everything before this point ran with interrupts off).
2. Runs the driver stage: the kit logs its manifest, publishes the firmware framebuffer recorded in phase 3 as the firmware display (the display manager's primary until a display driver publishes one), and then the drivers listed in the kernel's manifest are offered every node the buses have published, the platform nodes seeded in phase 3 first, then every PCI function the PCI host driver found, then every virtio device a transport driver published beneath a function or a slot, and the step returns once each node, children included, has been offered; the AHCI and NVMe drivers publish the disks they find here, and the storage manager registers and scans each one inside the publish, so `StorageManager.Partitions` is filled by the time the step returns. Then starts the USB hot-plug thread, which needs the scheduler's timer ticking.
3. Calls `OnBoot()`, whose default implementation initializes the graphical `KernelConsole` on whatever display is primary by then, which is what makes `Console.WriteLine` work.
4. Turns off the early-boot text renderer: up to here, the boot log you see on screen is the serial log mirrored by a minimal framebuffer writer; from now on the screen belongs to `Console` and the [Canvas](graphics.md).
5. Calls `BeforeRun()` once.
6. Calls `Run()` in a loop until `Stop()` is called.
7. Calls `AfterRun()` once.
8. Halts the CPU. There is no operating system to return to: a kernel never exits.

## Stopping the machine

Step 8 above is where a kernel ends up on its own. `Power` is how you get there deliberately, and the three members differ in how far they go:

```csharp
using Cosmos.Kernel.System;

Power.Halt();      // park this CPU until an interrupt wakes it
Power.Reboot();    // restart the machine; does not return
Power.Shutdown();  // power off; does not return
```

`Halt()` is the one that returns. It parks the CPU rather than spinning, so it is what an idle loop should call instead of `while (true) { }`, which burns a core and, on a single-CPU kernel, keeps the scheduler from making progress.

`Reboot()` and `Shutdown()` do not return on success. Both route through the platform's power operations, and where the firmware offers none they fall back to parking the CPU forever rather than continuing, which is why the compiler treats them as never returning.

To end the main loop without ending the machine, call `Stop()` on your kernel. `Run()` stops being called, `AfterRun()` runs once, and the CPU halts.

Static code that has no `this` to call it on reaches the running instance through `Global.CurrentKernel`, which the generated entry point sets before your kernel starts:

```csharp
Global.CurrentKernel?.Stop();
```

## A minimal kernel

```csharp
using System;
using Sys = Cosmos.Kernel.System;

namespace MyOS;

public class Kernel : Sys.Kernel
{
    protected override void BeforeRun()
    {
        Console.WriteLine("Cosmos booted successfully!");
    }

    protected override void Run()
    {
        Console.Write("Input: ");
        string? input = Console.ReadLine();
        Console.Write("Text typed: ");
        Console.WriteLine(input);
    }
}
```

- `BeforeRun()`: one-time setup (mount a [filesystem](filesystem.md), configure the [network](network.md), draw a splash screen).
- `Run()`: your main loop body. It is called again as soon as it returns, so it does not need to loop itself; keep it re-entrant.
- `Stop()`: call it from anywhere to exit the loop after the current `Run()` completes.
- `AfterRun()`: optional cleanup once the loop has ended.

An uncaught exception inside `Run()` propagates out of the loop, so wrap the body in `try`/`catch` if a command failing should not take the kernel down.

## Customizing startup

`OnBoot()` runs before the console exists, the right place for early setup of your own. Interrupts are already enabled and the driver stage has run, so a device a driver bound is usable from here on; code that must run with interrupts off belongs in an override of `Start()` itself:

```csharp
protected override void OnBoot()
{
    base.OnBoot();   // keep the KernelConsole setup; drop this line to boot headless

    // your early initialization here
}
```

A headless kernel that later wants `Console` output calls `KernelConsole.Initialize()` itself: it is the only route on the ring, `Console.WriteLine` does not bring the console up on its own. The call is idempotent, so it is safe whether or not `base.OnBoot()` already ran, and it returns `false` when graphics are compiled out, no display is published, or the display has no mode.

For total control you can override `Start()` itself and take over the lifecycle: the default implementation in [`Cosmos.Kernel.System/Kernel.cs`](https://github.com/CosmosOS/Cosmos/blob/gen3/src/Cosmos.Kernel.System/Kernel.cs) is small and a good starting point to copy from.

## The kernel command line

Limine passes a command line to the kernel: the `cmdline:` entry of the `Bootloader/limine.conf` file in your kernel project. It comes out the standard way:

```csharp
foreach (string arg in Environment.GetCommandLineArgs())
{
    Console.WriteLine(arg);
}
```

## Watching a boot

Every phase above logs to the serial port (COM1), which `cosmos run` connects to your terminal, the first thing to read when a kernel does not come up:

```
========================================
  CosmosOS v3.0.62 (gen3)
  Architecture: x86-64
========================================
[KMAIN] Phase 1: CPU initialization
[KMAIN] Phase 2: Platform initialization
[KMAIN]   - RSDP found at: 0xFFFF8000000F52D0
[KMAIN]   - Initializing ACPI...
[KMAIN] Phase 3: Managed kernel initialization
[KERNEL]   - Initializing heap...
[KERNEL]   - Initializing garbage collector...
[KERNEL]   - Initializing HAL...
[KERNEL]   - Initializing interrupts...
[KERNEL]   - Initializing PCI...
[KERNEL]   - Recording the firmware framebuffer...
[KERNEL]   - Initializing scheduler...
[KMAIN] Phase 4: User kernel
[Global] Registering kernel
[Kernel] Enabling interrupts...
[Kernel] Starting drivers...
[Drivers] manifest: AhciDriver(prio 0) E1000EDriver(prio 0) NvmeDriver(prio 0) PciHostDriver(prio 0) VirtioGpuDriver(prio 0) VirtioInputDriver(prio 0) VirtioMmioTransportDriver(prio 0) VirtioNetDriver(prio 0) VirtioPciTransportDriver(prio 0) VmwareSvgaDriver(prio 0)
[Display] primary: firmware "framebuffer" (the only display)
[Drivers] firmware published display "framebuffer" (consumed)
[Drivers] engine started, worker thread
...
[StorageManager] sata0 registered by AhciDriver (primary)
[Drivers] pci:0000:00:03.0 AhciDriver published block "sata0" (consumed)
...
[Kernel] Calling OnBoot()...
[Kernel] Calling BeforeRun()...
[Kernel] Entering main loop...
[Kernel] Calling Run()...
```

The two `sata0` lines appear when a disk is attached (`cosmos run --disk disk.img`): the kit's AHCI driver publishes it during the driver stage, and the storage manager registers it inside the publish, scanning its partitions, which is why the manager's line comes before the kit's.

For interactive debugging on top of the serial log, see [Debugging with VSCode and QEMU](debugging.md).
