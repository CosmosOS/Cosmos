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

## Known limitations

- Source-link and variable-inspection bugs exist in the VS Code debugging experience (see the [roadmap](../../roadmap.md)); stepping and breakpoints work, but inspecting some locals can show wrong or missing values.
- Debugging assumes QEMU. VMware, VirtualBox and Hyper-V are untested targets.
- ARM64 debugging under TCG emulation is slow; expect multi-second pauses on step operations on large kernels.
