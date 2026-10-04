# NativeAOT-Patcher (Cosmos Gen3)

Bare-metal C# kernel framework using NativeAOT. Ports the Cosmos plug system to NativeAOT for building x64 and ARM64 OS kernels.

## Setup

Requires .NET SDK 10.0.100+ (see `global.json`). First time or after `src/` changes:

```bash
./.devcontainer/postCreateCommand.sh
```

This builds all packages in dependency order, installs global tools (`cosmos-patcher`, `cosmos`), and configures a local NuGet source at `artifacts/package/release/`.

## Makefile (development with DevKernel)

Defaults: `ARCH=x64`, `TIMEOUT=30`, `KERNEL=HelloWorld`. Append `ARCH=arm64` to any target for ARM64.

```bash
make setup                          # Build all packages (.devcontainer/postCreateCommand.sh)
make build                          # Build DevKernel ISO
make run                            # Build + run in QEMU (x64 uses KVM)
make clean                          # Remove output-x64/, output-arm64/, artifacts/
make test KERNEL=Memory             # Run a kernel test suite (HelloWorld, Memory, TypeCasting, Timer, Network, Runtime, Threading, Math, GarbageCollector, Graphic, Storage, Fat, File, ...)
make test KERNEL=HelloWorld ARCH=arm64 TIMEOUT=90
```

Build pipeline: Source -> Patcher (IL method replacement via Mono.Cecil) -> ILC (AOT) -> GCC (linking) -> ISO (xorriso + Limine bootloader).

## Code style

- C# 14, .NET 10, 4-space indentation, Allman braces (see `.editorconfig`)
- Kernel code must be AOT-compatible: no reflection, no dynamic code generation
- `[UnmanagedCallersOnly(EntryPoint = "...")]` for native-to-managed entry points
- `[RuntimeExport("name")]` only for NativeAOT/runtime-required exports in Core (`Rh*`, `memmove`, `sqrt`, etc.)
- `[LibraryImport("*")]` for native imports
- Architecture-specific code: `#if ARCH_X64` / `#if ARCH_ARM64` to avoid + only tolerated in Cosmos.Kernel.Core or Cosmos.Kernel.Plugs.
- Private fields: `_camelCase`, static fields: `s_camelCase`, constants: `PascalCase`
- Braces required (`csharp_prefer_braces = true:error`)
- Avoid `var` - use explicit types

## Architecture

Dual-arch (x64/ARM64) with compile-time selection via `DefineConstants` and `RuntimeIdentifier`:

- **Native x64**: `src/Cosmos.Kernel.Native.X64/` - YASM `.asm` files (Runtime, WriteBarriers, InterfaceDispatch, Interrupts, etc.)
- **Native ARM64**: `src/Cosmos.Kernel.Native.ARM64/` - GAS `.s` files
- **HAL x64**: `src/Cosmos.Kernel.HAL.X64/`
- **HAL ARM64**: `src/Cosmos.Kernel.HAL.ARM64/`
- Multi-arch packages bundle both architectures; NuGet selects by RID at build time

## Feature switches

Kernel features are toggled via MSBuild properties in kernel `.csproj` files (all default to `true`):

`CosmosEnableInterrupts`, `CosmosEnableUART`, `CosmosEnablePCI`, `CosmosEnableTimer`, `CosmosEnableKeyboard`, `CosmosEnableMouse`, `CosmosEnableNetwork`, `CosmosEnableStorage`, `CosmosEnableGraphics`, `CosmosEnableScheduler`, `CosmosEnableUsb`

## Key paths

- `src/Cosmos.Sdk/Sdk/` - SDK props/targets consumed by kernel projects
- `src/Cosmos.Patcher/` - IL patcher CLI tool (Mono.Cecil-based)
- `src/Cosmos.Kernel.Core/Runtime/` - Runtime stubs (RhpThrowEx, exception handling, etc.)
- `src/Cosmos.Kernel.Core/Memory/` - Memory allocation
- `src/Cosmos.Kernel.System/` - Higher-level services (Graphics with `DisplayManager` and the canvas over the kit's display kind, Network, Input, Timer, IO, Storage with `StorageManager` and its consumer of the kit's block kind)
- `src/Cosmos.Kernel.HAL/DriverKit/` - The driver kit (bindings, bus kinds: synthetic, platform, PCI, virtio; device kinds under `Devices/`: keyboard, pointer, network, block, display with the `IDisplayModes` and `IHardwareCursor` facets)
- `src/Cosmos.Kernel.HAL/Firmware/` - The firmware framebuffer the kit publishes as the firmware display
- `src/Cosmos.Kernel.Drivers/` - Shipped drivers over the kit (PCI host, E1000E, the virtio PCI and MMIO transports, virtio-net, virtio-input, the display drivers virtio-gpu and VMware SVGA II with its SVGA3D layer, and the storage drivers AHCI and NVMe, whose disks `StorageManager` consumes); a User-layer driver assembly, one RID-less package
- `examples/DevKernel/` - Development kernel (use for testing changes)
- `tests/Kernels/` - 8 kernel test suites
- `dotnet/runtime/` - .NET runtime submodule (release/10.0 branch)
- `artifacts/` - Build outputs, NuGet packages, Limine bootloader

## GitHub

```bash
gh issue list --repo valentinbreiz/nativeaot-patcher
gh issue view <number> --repo valentinbreiz/nativeaot-patcher
```

Priority board: https://github.com/users/valentinbreiz/projects/2/views/2
