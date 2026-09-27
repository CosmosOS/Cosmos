# Debugging with VS Code and QEMU

Kernel debugging uses remote GDB: QEMU exposes a GDB server on `localhost:1234`, VS Code connects to it with `cppdbg`, and breakpoints are set directly in the editor. The `cosmos new` template ships the required `.vscode/launch.json` and `.vscode/tasks.json` preconfigured.

---

## Prerequisites

| Tool | Purpose | Notes |
|------|---------|-------|
| `gdb` | x64 debugging | Must be on `PATH`; the launch config invokes it as `gdb` |
| `gdb-multiarch` | ARM64 debugging | Must be on `PATH`; required for aarch64 targets |
| QEMU | Runs the kernel | Installed by `cosmos install` |
| VS Code C/C++ extension | `cppdbg` debug adapter | `ms-vscode.cpptools` |

`cosmos check` verifies the toolchain. On Debian/Ubuntu, `apt install gdb gdb-multiarch` covers both debuggers.

---

## Debugging a kernel created with `cosmos new`

1. Open the kernel folder in VS Code.
2. Set breakpoints in your kernel source.
3. Open the **Run and Debug** view and select **Debug x64 Kernel** (or **Debug ARM64 Kernel**).
4. Press F5. The pre-launch task builds the kernel, starts QEMU with `-s -S` (GDB server, frozen at startup), and the debugger attaches. Execution stops at your breakpoints.

The configuration names above are what the template generates. When working on the framework repository itself, the equivalent configurations are named **Debug x64 DevKernel** / **Debug ARM64 DevKernel**.

---

## Serial log

Every kernel boot phase logs to the serial port (COM1 on x64, PL011 on ARM64), which `cosmos run` connects to your terminal. When a kernel does not come up, or crashes before the debugger is useful, the serial log is the first thing to read; see [Kernel Startup](startup.md) for a phase-by-phase walkthrough and how to symbolicate crash addresses.

---

## Writing to the serial log

`Log` puts your own output on that same stream. It is the counterpart to `Console.WriteLine`, and the difference matters in exactly the cases you reach for a log: `Console` draws onto the framebuffer canvas, so it needs graphics to be up, it repaints the screen on every write, and it allocates. `Log` writes straight to the serial port, synchronously, without allocating for strings and numbers, and works from the first line of `BeforeRun` onward, before there is a console to write to and after a crash has taken the framebuffer with it.

```csharp
using Cosmos.Kernel.System.Diagnostics;
using Cosmos.Kernel.System.Storage;

Log.WriteString("storage: ");
Log.WriteNumber(StorageManager.DeviceCount);
Log.WriteString(" device(s), ");
Log.WriteNumber(StorageManager.Partitions.Count);
Log.WriteString(" partition(s)\n");
```

There is no `WriteLine`: append `"\n"` yourself, which is what the kernel does throughout.

| Call | Writes |
|---|---|
| `Log.WriteString(text)` | A string. Takes a non-null argument |
| `Log.Write(a, b, c)` | Each argument's `ToString()`, in order. Accepts null and writes `null` for it |
| `Log.WriteNumber(n)` | A number in decimal. Overloads for `int`, `uint`, `long` and `ulong` |
| `Log.WriteHex(n)` | A number in hexadecimal |
| `Log.WriteHexWithPrefix(n)` | The same with a leading `0x` |
| `Log.WriteBytes(span)` | Raw bytes, unformatted |

Reach for `WriteString` and `WriteNumber` over `Write` on any path that runs often or runs early. The argument list of `Write` is built on the stack and costs nothing, but a number or a `bool` passed to it is boxed first, and that box is a heap allocation the typed overloads do not make.

The serial port itself is behind the `CosmosEnableUART` feature switch. With it off the calls still return normally, but nothing reaches the serial stream, so a kernel that turns the switch off should not rely on `Log` for anything it needs to read back.

---

## Debugging a driver

A driver written with the [driver kit](drivers.md) is easiest to follow through the serial log: the kit writes a `[Drivers]` line for every step it takes on the driver's behalf, and the driver's own `context.WriteLog(message)` lines carry the same prefix, followed by the driver's registered name and the device's path.

| Line | Means |
|---|---|
| `[Drivers] Registered rtl8139` | `DriverManager.Register` accepted the registration |
| `[Drivers] Registered built-in ahci` | `Global.StartKernel` registered one of the built-in drivers written against the kit, ahead of the kernel's |
| `[Drivers] Refused to register rtl8139: the name is taken` | Another registration, or a built-in driver, already has the name; `Register` returned false |
| `[Drivers] No driver registered` | The pass ran with nothing to offer devices to |
| `[Drivers] pci/0000:00:04.0 kept by xhci` | A built-in driver HAL brought up (or the `gop` reservation) owns the function, so no registered driver is offered it |
| `[Drivers] pci/0000:00:03.0 -> rtl8139 (device match)` | The driver bound the device, and which kind of match ranked it |
| `[Drivers] pci/0000:00:03.0 -> rtl8139 declined` | `Probe` returned `Declined`; the next candidate is offered the device |
| `[Drivers] pci/0000:00:03.0 -> rtl8139 failed: ...` | The attempt failed, and why: `Probe returned Failed`, the message of an exception the factory or `Probe` threw, or `the factory returned null` |
| `[Drivers] pci/0000:00:05.0 -> no driver` | No registration matched the device, or every candidate declined or failed |
| `[Drivers] rtl8139 pci/0000:00:03.0: interrupts polled from the timer` | What `TryRequestInterrupts` got: `interrupts through MSI-X`, this, or `no interrupts: ...` |
| `[Drivers] nvme pci/0000:00:04.0: turned off the MSI-X firmware left enabled` | The function came with MSI-X on; the kit turned it off before `Probe`, and turns it back on only if the driver's interrupts go through it |
| `[Drivers] rtl8139 pci/0000:00:03.0: BAR 0 is not an assigned memory BAR` | Why a `Try` member of the context returned false; DMA and endpoint refusals are logged the same way |
| `[Drivers] rtl8139 pci/0000:00:03.0: work item threw and will not run again: ...` | A work item's callback threw; it is disarmed |
| `[Drivers] nvme pci/0000:00:04.0: I/O completions through MSI-X vector 0` | How the built-in NVMe driver completes I/O once its handler runs: this, or `I/O completions polled by the waiting thread` where the kit could only poll the handler from the timer, or grant no interrupt |
| `[Drivers] ahci pci/0000:00:03.0: published disk sata0` | `StorageManager` took a disk the driver published, after reading its partition table; a USB driver's disk logs `withdrew disk <name>` when it leaves with its device |
| `[Drivers] usb/1-5:1.0 -> usb-boot-mouse removed: the device left the bus` | A USB binding ended on unplug, after the driver's `Remove` |

`DriverManager.Devices` shows the outcome, every PCI function and USB interface with the driver that owns it; DevKernel's `lsdev` command prints it.

`Probe`, work items and `Remove` run in thread context, and a breakpoint in them stops as it does anywhere else in the kernel. The interrupt and report handlers are harder: they run with interrupts masked, a polled one on every timer tick, and they cannot log, since `WriteLog` builds a string. Count what a handler sees in fields, and log the counts from a work item or from the kernel. A handler that throws halts the kernel, and the panic names the driver and the device: `[Drivers] rtl8139 pci/0000:00:03.0: the interrupt handler threw`, after the exception's message.

---

## Known limitations

- Source-link and variable-inspection bugs exist in the VS Code debugging experience (see the [roadmap](../../roadmap.md)); stepping and breakpoints work, but inspecting some locals can show wrong or missing values.
- Debugging assumes QEMU. VMware, VirtualBox and Hyper-V are untested targets.
- ARM64 debugging under TCG emulation is slow; expect multi-second pauses on step operations on large kernels.
