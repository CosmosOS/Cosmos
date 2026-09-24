## Gen2/Gen3 Feature Comparison

| Feature | Gen2 | Gen3 | Notes |
|---------|------|-------------|-------|
| Low level assembly access | ✅ | ✅ | Before X# now x64 NASM + ARM64 GAS assembly. |
| ACPI | ✅ | ✅ | LAI (Lightweight ACPI Implementation) via C interop.  |
| Interrupt Handling | ✅  | ✅  | x64: APIC (Local + I/O). ARM64: GIC. |
| Memory Management | ✅ | ✅ ||
| Driver support | ✅ | ✅ | PCI and MMIO |
| Garbage Collection | ✅ | ✅ | Mark-and-sweep GC |
| Filesystem | ✅ | ✅ | FAT12/16/32 on a Unix-style VFS (mount, superblocks, inodes) with formatting; MBR/GPT/EBR partitioning. Exposed through `System.IO.File`/`FileStream`. |
| .NET core library features | 🟡 | 🟡 Partial | Core types work (String, Collections, List, Dictionary). Math, Console, DateTime, Random, BitOperations, `System.IO.File` plugged. |
| Plug system | ✅ | ✅  |  |
| Test Framework | ✅ | ✅  |  |
| Debugger| ✅ | 🟡 Partial | Source link + variables bugs in vscode |
| CPU/FPU accelerated math | ✅ | ✅ | x64 only: x87 FPU (sin/cos/tan/exp/log/atan) |
| Cosmos Graphic Subsystem | ✅ | ✅ | UEFI GOP framebuffer via Limine only. |
| Network interface | ✅ | ✅ | |
| Timer / Clock | ✅ | ✅ | |
| Keyboard Input | ✅ | ✅ | |
| Mouse Input | ✅ | ✅ | |
| Audio interface | 🟡 | ❌ | No audio, sound, or speaker support. |

## Additional Gen3 Features

Beyond Gen2 parity, Gen3 brings new capabilities:

| Feature | Status | Notes |
|---------|--------|-------|
| **NativeAOT Runtime** | 🟡 Partial | Full NativeAOT compilation with runtime, no IL2CPU. |
| **ARM64 Support** | ✅  |  |
| **Limine Boot Protocol** | ✅ |  |
| **Threading & Scheduler** | ✅ | Priority-based stride scheduler (x64 + ARM64). `lock` keyword supported. |
| **Feature Flags** | ✅ |  |
| **Cosmos Vs Code Extension** | ✅ |  |
| **USB Support** | ✅ | xHCI host controller with hubs and hot-plug (x64 + ARM64). HID boot keyboard and mass storage (Bulk-Only Transport). No USB mouse or EHCI yet. |

## Future Releases

Features planned after first release:

| Feature | Status | Notes |
|---------|--------|-------|
| **SMP (Symmetric Multiprocessing)** | ❌ Not Started | Multi-core AP boot, per-CPU scheduling, load balancer. |
| **HTTPS** | ❌ Not Started | TLS/SSL implementation, certificate handling, secure sockets. |
| **Generational GC** | ❌ Not Started | Replace current mark-and-sweep with generational collector (Gen0/Gen1/Gen2) for better performance. |
| **Code execution** | ❌ Not Started | Userland WASM VM |

