# RISC-V 64

Cosmos builds and boots RISC-V 64 kernels on the QEMU `virt` machine, next to x64 and ARM64. The port is experimental: it runs the kernel test suites listed below, on one hart, with the devices QEMU's virt machine offers. This page explains what the port needs that the other two architectures do not, how to set it up, how a kernel boots, and what is still missing.

---

## Why RISC-V needs its own toolchain step

NativeAOT supports RISC-V 64 in .NET 10, but Microsoft does not publish two of the pieces a cross build needs on nuget.org:

- the cross JIT ILC loads to emit riscv64 code (`libclrjit_unix_riscv64_x64.so`): the ILCompiler package ships the x64 and arm64 JITs only;
- the riscv64 build of CoreLib and its siblings (`System.Private.*.dll`): there is no `microsoft.netcore.app.runtime.nativeaot.linux-riscv64` package.

`.devcontainer/build-riscv64-toolchain.sh` builds both from the `dotnet/runtime` submodule, at the commit the repository pins, into `artifacts/riscv64-toolchain/`, and `src/Cosmos.Build.Ilc.RISCV64` packs them. The script runs on a Linux x64 host only. The first run takes several minutes (the JIT is a CMake build); a second run with the submodule at the same commit only copies the outputs again.

The rest of the NativeAOT runtime pack is the same IL on every Linux architecture, so a riscv64 kernel takes the framework assemblies from the `linux-x64` pack and only the `System.Private.*` assemblies from the riscv64 toolchain. `Cosmos.Sdk` builds a riscv64 kernel framework-dependent and untrimmed from the SDK's point of view, so that the SDK never asks for the riscv64 runtime packs that do not exist; ILC still trims, and `CosmosResolveFeatureSettings` hands it the feature switches the SDK's trimming targets would have collected.

## Setting it up

RISC-V is opt-in, because of the toolchain build:

```bash
make setup ARCH=riscv64        # or COSMOS_RISCV64=1 ./.devcontainer/postCreateCommand.sh
make build ARCH=riscv64
make test KERNEL=Memory ARCH=riscv64 TIMEOUT=240
```

`make setup ARCH=riscv64` builds the toolchain, packs `Cosmos.Build.Ilc.RISCV64`, `Cosmos.Kernel.Native.RISCV64`, `Cosmos.Kernel.Core.RISCV64` and `Cosmos.Kernel.HAL.RISCV64`, and adds the riscv64 slice to the multi-arch packages. Running `qemu-system-riscv64` needs EDK2 for RISC-V: the QEMU bundle `cosmos install` fetches carries `edk2-riscv-code.fd`, and the launcher also finds the image a distribution installs (`qemu-efi-riscv64` on Debian and Ubuntu) or the one `COSMOS_RISCV64_EDK2` names.

## How a kernel boots

QEMU starts OpenSBI in M-mode. OpenSBI hands S-mode to EDK2, which runs from pflash and loads Limine's `BOOTRISCV64.EFI` from the ISO. Limine maps the kernel in the top 2 GiB and jumps to `kmain` in S-mode; the kernel never runs in M-mode and asks OpenSBI, through the SBI, for what S-mode cannot do itself: programming the timer and resetting or powering off the machine.

The launcher starts the virt machine with `acpi=off`. With ACPI on, the boot does not reach the kernel on QEMU 8.2 (the cause is not tracked down yet), and the kernel does not need ACPI on RISC-V: it reads the machine from the device tree Limine hands over.

## The architecture code

| Piece | Where | What it does |
|---|---|---|
| Native stubs | `src/Cosmos.Kernel.Native.RISCV64/` | The trap entry, the context switch, the write barriers, interface dispatch, exception dispatch and the funclet calls, the SBI call, the CSR accessors, the block copies |
| Core bridges | `src/Cosmos.Kernel.Core.RISCV64/` | The imports of those stubs, the SBI calls, the PLIC, the interrupt controller, the device mapper, the CPU and power operations |
| HAL | `src/Cosmos.Kernel.HAL.RISCV64/` | The platform initializer: the virt machine description from the device tree, the PLIC line routing, the SBI timer, the Goldfish real-time clock |
| Core | `*.RISCV64.cs` files and `ARCH_RISCV64` blocks in `src/Cosmos.Kernel.Core/` | The thread context, the trap frame (`IRQContext`), the DWARF register numbering, the unwinder's register context, the GcInfo encoding, the serial port |

Everything is built for RV64GC with the LP64D calling convention, which is what ILC emits: lld refuses to link objects of different floating-point ABIs, so the assembler and the C compiler use the same `-march=rv64gc -mabi=lp64d`.

### Traps and interrupts

`stvec` points at one entry in direct mode. The entry pushes the floating-point registers, `fcsr` and an `IRQContext` (x0 to x31 at offset 8 times the register number, then `sepc`, `sstatus`, the trap kind, `scause` and `stval`) on the interrupted stack, the layout `ThreadContext.RISCV64.cs` describes, and calls the managed dispatcher. Exceptions go to the handler at vector 0. A local interrupt with `scause` code `c` dispatches at vector 16 plus `c` (the timer at 21, the software interrupt the scheduler raises at 17), and an external interrupt, claimed from the PLIC as source `s`, at vector 32 plus `s`.

### The timer

The time CSR counts at the device tree's `timebase-frequency`, 10 MHz on QEMU. The tick source programs the next interrupt through the SBI TIME extension, which writes `stimecmp` itself on a hart with the Sstc extension; at the scheduler's 100 Hz, one ecall per tick is not worth detecting Sstc for.

### Write barriers

ILC's riscv64 code calls `RhpAssignRefRiscV64`, `RhpCheckedAssignRef` and `RhpByRefAssignRef` with the destination in `t3`, the reference in `t4` and the by-reference source in `t5`. The collector is neither generational nor concurrent, so each barrier is the store alone. Because `RhpCheckedAssignRef` is the JIT's helper on RISC-V, Core does not export its managed method of that name there; `RhpAssignRef`, the standard calling convention form, stays Core's managed export.

## What runs

Every kernel suite runs on QEMU 8.2's virt machine under TCG through `make test ARCH=riscv64`, with no failure; the skipped tests are the x64-only ones (port I/O, the LAPIC, the PIT) and the ones a cell does not apply to.

| Suite | Passed / total | Suite | Passed / total |
|---|---|---|---|
| HelloWorld | 3 / 3 | Pci | 8 / 8 |
| Runtime | 107 / 107 | Virtio | 24 / 28 |
| TypeCasting | 26 / 26 | Storage | 510 / 588 |
| Memory | 74 / 74 | Fat | 42 / 42 |
| GarbageCollector | 96 / 96 | File | 39 / 39 |
| Math | 30 / 30 | Iso9660 | 47 / 51 |
| Threading | 89 / 89 | Network | 52 / 52 |
| Timer | 21 / 21 | Graphic | 31 / 50 |
| Random | 16 / 16 | Audio | 25 / 44 |
| Interrupts | 8 / 16 | Drivers | 326 / 567 |
| Power | 4 / 4 | | |

The DevKernel boots to its shell on the ramfb display, takes keys from a virtio keyboard and runs Lua.

The `acpi-off` test modifier does not apply to riscv64: the launcher always starts its virt machine with ACPI off, so the modifier would only run the bare cell a second time.

## CI

Kernel Tests run every suite on riscv64 next to x64 and arm64, and a suite's results comment has a riscv64 column. The setup job builds the packages with `COSMOS_RISCV64=1`. It caches the toolchain build's outputs on the `dotnet/runtime` commit and on `build-riscv64-toolchain.sh`, so the cross JIT is built once per runtime bump and the script skips the build on every other run. A riscv64 leg takes a suite's arm64 QEMU timeout unless the suite passes `timeout_riscv64`, since both run under TCG at about the same speed.

The tools release does not carry `qemu-system-riscv64` until `build-tools.yml` runs again with the riscv64 target. Until then, a riscv64 leg installs Ubuntu's QEMU 8.2: `qemu-system-misc` (with OpenSBI and the virtio-gpu module through its dependencies), `qemu-efi-riscv64` for the EDK2 image and `ipxe-qemu` for the network cards' option ROMs. The leg uses the bundle by itself once the bundle has both the binary and `edk2-riscv-code.fd`.

The .NET Tests workflow builds the DevKernel ISO for riscv64 in `linux-iso-tests`, beside the Windows and macOS ISO builds for x64 and arm64, since the toolchain builds on a Linux x64 host only. Kernel Coverage runs riscv64 as well, and its report leaves the other architectures' HALs out of the riscv64 figures.

## What is missing

- One hart: the kernel starts no secondary hart, and the PLIC context is hart 0's S-mode context.
- No PCI interrupts: the virt machine delivers MSIs through the AIA's IMSIC, which the kernel does not drive yet, and the PCI host's INTx lines are not routed to the PLIC either, so PCI drivers run polled. virtio-mmio devices take their PLIC line.
- No port I/O, as on ARM64: the port accessors read 0 and drop writes.
- The toolchain builds on a Linux x64 host only.
- A kernel cannot reference an AOT-compatible library project (`IsAotCompatible`): built with the kernel's RID, such a project restores the linux-riscv64 runtime pack, which nuget.org does not have. Its package works, since a package is IL only; this is why the DevKernel takes the FTP, Telnet and Lua packages on riscv64 even next to clones of those repositories.
- The kernel template's getting-started text and VS Code tasks cover x64 and ARM64 only. The project it creates picks its architecture from the RID, so it builds for riscv64 with `cosmos build -a riscv64` once the riscv64 packages are set up.
