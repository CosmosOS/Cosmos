# Contributing

This document covers how to report issues, collect the diagnostics a report needs, set up a development environment, and submit changes to Cosmos gen3.

---

## Reporting Issues

Open issues through the [issue templates](https://github.com/CosmosOS/Cosmos/issues/new/choose). An actionable report contains three things: the **exact command** that was run, the **versions** involved, and the **full output**: a build log or serial log, not a screenshot of the last line.

### Build or publish failures

Include:

- The exact command and the directory it was run from (`cosmos build`, a VSCode task, or the full `dotnet publish` command line)
- The output of `dotnet --version` and the host OS
- The Cosmos.Sdk version: the `Sdk="Cosmos.Sdk/X.Y.Z"` line and `PackageReference` versions from the `.csproj`
- The full build output; re-run with `--verbosity normal` if the failing step is not visible in the minimal log
- The output of `cosmos check` if the failure looks toolchain-related (missing linker, assembler, QEMU)

### Kernel crashes

Include:

- The full serial log from boot to crash, not only the exception block: GC activity and driver init lines before the crash are often the actual clue
- The stack trace symbolicated to function names (see [Symbolicating a Stack Trace](#symbolicating-a-stack-trace)), or the kernel `.elf` attached as a zip
- How the kernel was launched: `cosmos run`, a VSCode task, or the manual QEMU command line (attached disks, memory size, and input devices all affect reproduction)
- A link to a repository that reproduces the crash, when possible

---

## Collecting Diagnostics

### Serial Log

The kernel logs boot progress, GC activity, and exception dumps to the serial port.

- `cosmos run` prints serial output to the terminal.
- The default VSCode tasks pass `-serial stdio` to QEMU.
- When running QEMU manually, add `-serial stdio` (print to terminal) or `-serial file:serial.log` (write to a file).

### Symbolicating a Stack Trace

A CPU exception dump prints raw return addresses:

```
Stack trace (raw return addresses, symbolicate with nm):
  ip:  0xFFFFFFFF80050A1E
  [0]  0xFFFFFFFF80050947
```

The built kernel ELF at `bin/<Config>/net10.0/linux-<arch>/<KernelName>.elf` contains the symbols to resolve them:

```bash
addr2line -e bin/Debug/net10.0/linux-x64/MyKernel.elf -f -C 0xFFFFFFFF80050A1E 0xFFFFFFFF80050947
```

The `ip` address is where the CPU faulted; the numbered entries are the call chain. Paste the resolved names into the issue, or attach the `.elf` file so maintainers can resolve them.

### Common Issues

| Symptom | Resolution |
|---------|------------|
| Framework build fails right after cloning | Run `./.devcontainer/postCreateCommand.sh` (or `make setup`) first: it builds all packages and registers the local NuGet source |
| `undefined symbol: _native_*` / `Rhp*` at link time | The architecture-native package was not restored. Fixed after 3.0.68; on older SDKs pass `-p:CosmosArch=x64` (or `arm64`) to `dotnet publish`, or use `cosmos build` |
| QEMU shows nothing or boots to the BIOS | Check the ISO architecture matches the QEMU binary, and force CD boot with `-boot d` when a disk is also attached |
| An API that works in a normal .NET app throws in the kernel | Kernels are AOT-compiled (see [NativeAOT limitations](https://github.com/dotnet/runtime/blob/main/src/coreclr/nativeaot/docs/limitations.md)) and not all of the base library is plugged yet |

---

## Development Setup

Requires .NET SDK 10.0.100+ (see `global.json`). See [Dev Container Setup](docs/articles/dev/install-dev.md) for the full description of the bootstrap script.

```bash
./.devcontainer/postCreateCommand.sh    # build all packages, install the cosmos and cosmos-patcher tools
make run                                # build the DevKernel ISO and boot it in QEMU
make run ARCH=arm64                     # same, for ARM64
```

---

## Code Style

The full rules are in the [Coding Guidelines](docs/articles/dev/coding-guidelines.md); naming and formatting are enforced by `.editorconfig`. In short:

- C# 14 / .NET 10, 4-space indentation, Allman braces
- Braces are always required; use explicit types instead of `var`
- Private fields `_camelCase`, static fields `s_camelCase`, constants `PascalCase`
- Nullable reference types are enabled solution-wide; do not introduce nullable warnings: declare `?` honestly, guard late-initialized state with `[MemberNotNull]` helpers, and avoid the null-forgiving `!`
- Kernel code must be AOT-compatible: no reflection, no dynamic code generation
- Architecture-specific code goes behind `#if ARCH_X64` / `#if ARCH_ARM64`, only in `Cosmos.Kernel.Core` or `Cosmos.Kernel.Plugs`

---

## Testing

See [Testing](docs/articles/dev/testing.md) for the two test layers.

```bash
dotnet test tests/Cosmos.Tests.Patcher            # one build-toolchain suite; Scanner, Tools, Build.* and BuildCache sit beside it
dotnet test src/tests/Cosmos.Kernel.Tests.System   # host-side unit tests of Cosmos.Kernel.System
make test KERNEL=Memory         # kernel integration suite in QEMU
make test KERNEL=Memory ARCH=arm64
```

Before submitting a PR, run the suites relevant to the change. CI runs both architectures; a PR needs both green.

---

## Pull Requests

- Start commit messages with a [gitmoji](https://gitmoji.dev/) matching the change: 🐛 fix, ✨ feature, 📝 docs, ♻️ refactor, ✅ tests
- Reference the issue number in the PR description
- Keep changes focused; split unrelated fixes into separate PRs
- Update documentation when behavior changes

---

## Getting Help

- [Documentation site](https://cosmosos.github.io/index.html): User Guide and Developer Docs
- [Discord](https://discord.com/invite/kwtBwv6jhD)
- [Existing issues](https://github.com/CosmosOS/Cosmos/issues)
