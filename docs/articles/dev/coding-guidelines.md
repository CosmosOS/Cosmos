# Coding Guidelines

This document establishes the coding style and architecture patterns for Cosmos Gen3. These guidelines ensure a clean, modern, and consistent codebase across all contributors.

## Table of Contents

- [1. Project Architecture](#1-project-architecture)
- [2. Naming Conventions](#2-naming-conventions)
- [3. File Organization](#3-file-organization)
- [4. Class & Type Design](#4-class--type-design)
- [5. Kernel Lifecycle](#5-kernel-lifecycle)
- [6. HAL Architecture](#6-hal-architecture)
- [7. Plug System](#7-plug-system)
- [8. Native Interop](#8-native-interop)
- [9. Architecture-Specific Code](#9-architecture-specific-code)
- [10. Memory & Safety](#10-memory--safety)
- [11. Error Handling](#11-error-handling)
- [12. Nullable Reference Types](#12-nullable-reference-types)
- [13. Modern C# / .NET 10 Idioms](#13-modern-c--net-10-idioms)
- [14. AOT Constraints](#14-aot-constraints)
- [15. Documentation](#15-documentation)
- [16. Testing](#16-testing)
- [17. Public API Surface](#17-public-api-surface)

---

## 1. Project Architecture

### Layer Dependency Rules

The project is split into strict layers. Dependencies flow **downward only**. These rules are **enforced at compile time** by the `LayerAnalyzer` Roslyn analyzer in `Cosmos.Build.Analyzer.Patcher`.

```
User Kernel (DevKernel, test kernels)
    └── Cosmos.Kernel.System        ← high-level OS APIs (Console, Graphics, Network)
         └── Cosmos.Kernel.HAL      ← hardware abstraction (shared logic)
              ├── Cosmos.Kernel.HAL.X64        ← x64-specific HAL implementations
              ├── Cosmos.Kernel.HAL.ARM64      ← ARM64-specific HAL implementations
              └── Cosmos.Kernel.HAL.Interfaces ← pure interfaces, no implementations
                   └── Cosmos.Kernel.Core   ← low-level runtime (memory, scheduler, serial)
                        ├── Cosmos.Kernel.Native.X64       ← x64 assembly (.s)
                        ├── Cosmos.Kernel.Native.ARM64     ← ARM64 assembly (.s)
                        └── Cosmos.Kernel.Native.MultiArch ← cross-platform native C code (ACPI, libc stubs)
```

For the full dependency graph, project descriptions, and rules, see [Kernel Project Layout](kernel-project-layout.md).

### When to Create a New Project

- New hardware device category → new interface in `Cosmos.Kernel.HAL.Interfaces`, implementations in `Cosmos.Kernel.HAL.X64`/`Cosmos.Kernel.HAL.ARM64`. Cross-platform HAL devices go to `Cosmos.Kernel.HAL`.
- New OS-level feature, user API exposed → in `Cosmos.Kernel.System`.
- New low-level runtime concern → in `Cosmos.Kernel.Core`.

---

## 2. Naming Conventions

Some core naming rules are enforced by `.editorconfig`; the table below documents the full naming guidelines:

| Element | Convention | Example |
|---------|-----------|---------|
| Namespace | `Cosmos.Kernel.{Layer}.{Category}` | `Cosmos.Kernel.Core.Scheduler` |
| Public class/struct | PascalCase | `GarbageCollector`, `Thread` |
| Interface | `I` + PascalCase | `IScheduler`, `IPlatformInitializer` |
| Public method/property | PascalCase | `InitializeStack()`, `StackPointer` |
| Private/internal field | `_camelCase` | `_currentScheduler`, `_kernel` |
| Private/internal static field | `s_camelCase` | `s_cursorPattern`, `s_nextThreadId` |
| Constant | PascalCase | `DefaultStackSize`, `MaxThreadCount` |
| Private constant | PascalCase | `COM1_BASE` (hardware regs use UPPER_SNAKE for readability) |
| Local variable | camelCase | `stackTop`, `entryPoint` |
| Parameter | camelCase | `cpuState`, `threadId` |
| Type parameter | `T` + PascalCase | `TValue`, `TKey` |
| Enum member | PascalCase | `ThreadState.Running` |

### Avoid

- Hungarian notation (`m_`, `p_`, `g_`), use `_` prefix for private fields only.
- Legacy `mStopped` style, use `_stopped`.
- Abbreviations unless universally understood (`GC`, `CPU`, `HAL`, `IO`, `IP`, `MAC`).

---

## 3. File Organization

### One Type Per File

Each public type gets its own file, named after the type:

```
Cosmos.Kernel.Core/
  Scheduler/
    SchedulerThread.cs
    SchedulerThreadState.cs
    IScheduler.cs
    SchedulerManager.cs
    PerCpuState.cs
```

Two kinds of companion share the primary type's file, which keeps the primary
type's name: a type's own tightly-coupled companion, as `Gpt` has
`GptPartitionEntry` and `TcpPacket` has `TcpFlags` and `TcpOption`, and a
subclass family, as `IcmpPacket` has `IcmpEchoRequest` and `IcmpEchoReply`.
An implementation is not a companion of its interface: 24 files declare a
top-level interface and each one declares that interface and nothing else.

### Partial Classes for Large Types

Split large classes across multiple files using `partial`. Name the files `{ClassName}.{Aspect}.cs`:

```
Memory/
  GarbageCollector.cs              ← core structure, fields, initialization
  GarbageCollector.Alloc.cs        ← allocation methods
  GarbageCollector.Mark.cs         ← mark phase
  GarbageCollector.Sweep.cs        ← sweep phase
  GarbageCollector.GCHandler.cs    ← GC handle table
  GarbageCollector.Frozen.cs       ← frozen segment support
```

### Architecture-Specific Files

For types that need per-architecture implementations within the same project, use a suffix:

```
Scheduler/
  ThreadContext.X64.cs             ← x64-specific context layout
  ThreadContext.ARM64.cs           ← ARM64-specific context layout
```

These are conditionally compiled via `.csproj`:

```xml
<ItemGroup Condition="'$(CosmosArch)' != 'arm64'">
    <Compile Remove="Scheduler\ThreadContext.ARM64.cs" />
</ItemGroup>
```

This is only allowed now in `Cosmos.Kernel.Core` but this may change in the future.

### Member Order

There is no separator convention to follow. Four garbage-collector files carry
`// --- Section Name ---` headers, written three weeks before this page was,
and one driver file was later written to match them. The other 353 non-vendored
files in the four tracked assemblies do not, no file has ever been converted,
and no analyzer checks it. Neither is there a member order to describe: across
131 types with three or more kinds of member there are 93 distinct orders, and
the best-fitting single order covers 43 of them.

Two orderings are still worth following, because a reader checks them inside
one screen:

- Fields and constants come before the members that read them, not after the
  first method.
- Constructors come after fields and properties and before methods, and a
  static factory sits with the constructors it stands in for.

If you want a model for the full separator form in a new file,
`Cosmos.Kernel.HAL/Devices/Network/VirtioNet.cs` is the one file that
demonstrates it. Do not convert an existing file to it.

### Using Directives

- Place `using` directives **outside** the namespace.
- Sort `System` namespaces first.
- Use file-scoped namespaces (eg. `namespace Cosmos.Kernel.Core.Scheduler;`).
  The vendored trees (BigGustave, SharpZipLib, LunarFonts) and the
  dotnet/runtime mirrors keep the block form so they stay diffable against
  upstream, and `Core/Runtime/Stdllib.cs` cannot take the file-scoped form
  at all: it declares four namespaces, one of them nested, which is CS8955.

---

## 4. Class & Type Design

### Static Utility Classes

Use `static class` for stateless kernel utilities that have no per-instance state.

### Managers

Use a `static class`. Managers coordinate subsystem state without requiring an instance. Boot-path setup is an `internal static void Initialize()` called from `LibraryInitializer`, never from a kernel:

```csharp
// Simple manager: no underlying instance to expose
public static class TimerManager
{
    private static ITimerDevice? s_timer;

    public static bool IsInitialized => s_timer is not null;

    internal static void RegisterTimer(ITimerDevice timer) { ... }
    public static void Wait(uint ms) { ... }
}

// Manager wrapping a pluggable implementation: expose via Current
public static class SchedulerManager
{
    private static IScheduler? s_currentScheduler;

    public static IScheduler? Current => s_currentScheduler;

    internal static void Initialize(uint cpuCount) { ... }
}
```

Only add a `Current` property when the manager wraps a pluggable implementation (eg. `IScheduler` for multiple scheduling algorithms). Most managers don't need one.

Do not carry a separate `_initialized` flag next to the state it stands for. Derive `IsInitialized` from that state, and have `Initialize` guard re-entry on the same field, assigning it last so no reader sees `IsInitialized` go true before the state behind it exists.

When a manager sits behind a feature switch, its members follow the rule in [Public API Tracking](public-api.md#behaviour-when-a-feature-is-compiled-out): reads answer honestly with the feature off, actions throw and name the switch.

### Structs for Low-Level Data

Use `struct` with `[StructLayout(LayoutKind.Sequential)]` for data that maps to hardware or memory layouts:

```csharp
[StructLayout(LayoutKind.Sequential)]
internal struct FreeBlock
{
    public unsafe MethodTable* MethodTable;
    public nuint Size;
    public unsafe FreeBlock* Next;
}
```

### Enums

Use `enum` for state machines and flags. Apply `[Flags]` where appropriate:

```csharp
public enum ThreadState
{
    Ready,
    Running,
    Blocked,
    Sleeping,
    Dead
}

[Flags]
public enum ThreadFlags
{
    None = 0,
    IsIdle = 1 << 0,
    IsKernel = 1 << 1
}
```

---

## 5. Kernel Lifecycle

### User Kernel Pattern

User kernels inherit from `Cosmos.Kernel.System.Kernel` and override lifecycle methods:

```csharp
using Cosmos.Kernel.System;

namespace MyKernel;

public class Kernel : Cosmos.Kernel.System.Kernel
{
    protected override void BeforeRun()
    {
        // One-time setup (after boot, before main loop)
        Console.WriteLine("Booted!");
    }

    protected override void Run()
    {
        // Called in a loop until Stop() is called
        string? input = Console.ReadLine();
        ProcessCommand(input);
    }

    protected override void AfterRun()
    {
        // Cleanup (after main loop exits)
    }
}
```

**Rules:**
- Override `OnBoot()` only to customize boot (the default brings up `KernelConsole`).
- Override `BeforeRun()` for one-time setup after the system is ready.
- `Run()` is the main loop body, keep it focused.
- Call `Stop()` to exit the main loop cleanly.
- Return from the kernel should be avoided, the base class halts the CPU after `AfterRun()`.

---

## 6. HAL Architecture


### Interface-Driven HAL

All hardware interaction goes through interfaces. Implementations are registered at boot:

```csharp
// Interface (Cosmos.Kernel.Core, internal: no kernel registers or obtains one)
internal interface ICpuOps
{
    void Halt();
    void DisableInterrupts();
    void EnableInterrupts();
}

// Implementation (Cosmos.Kernel.HAL.X64)
public class X64CpuOps : ICpuOps
{
    public void Halt() => Native.Cpu.Halt();
    // ...
}
```

### Platform Initializer Pattern

Each architecture provides a factory that creates all platform-specific components. The factory, the contract and everything it returns are internal to the HAL: a kernel never installs one.

```csharp
internal class X64PlatformInitializer : IPlatformInitializer
{
    public string PlatformName => "x86-64";
    public PlatformArchitecture Architecture => PlatformArchitecture.X64;

    public IPortIO CreatePortIO() => new X64PortIO();
    public ICpuOps CreateCpuOps() => new X64CpuOps();
    public IInterruptController CreateInterruptController() => new X64InterruptController();
    public ITimerDevice CreateTimer() => new X64Timer();
    public IKeyboardDevice[] GetKeyboardDevices() => [new PS2Keyboard()];
    public IMouseDevice[] GetMouseDevices() => [new PS2Mouse()];
    public INetworkDevice? GetNetworkDevice() => /* PCI probe */ null;
    public uint GetCpuCount() => /* ACPI/MADT */ 1;

    public void InitializeHardware()
    {
        // PCI, ACPI, APIC initialization
    }

    public void StartSchedulerTimer(uint quantumMs)
    {
        // Configure PIT/APIC timer for preemptive scheduling
    }
}
```

### Adding a New Device

1. Define the interface in `Cosmos.Kernel.HAL.Interfaces/Devices/`.
2. Implement in `Cosmos.Kernel.HAL.X64/` and `Cosmos.Kernel.HAL.ARM64/`.
3. Add factory method to `IPlatformInitializer`.
4. Register during `InitializeHardware()` or via the platform initializer.

### HAL Registration

```csharp
// At boot (from the library initializer, or from OnBoot):
PlatformHAL.Initialize(new X64PlatformInitializer());
```

---

## 7. Plug System

Plugs replace BCL methods at the IL level. The patcher rewires calls at build time. For full documentation on plug attributes (`[Plug]`, `[PlugMember]`, `[PlatformSpecific]`) and the plug template, see [Plugs](plugs.md).

### When to Use Plugs vs. Other Approaches

| Scenario | Approach |
|----------|----------|
| Replace a BCL method | Plug |
| Add kernel-level API | New class in `Cosmos.Kernel.System` |
| Provide runtime stub | `[RuntimeExport]` in Core |
| Call native code | `[LibraryImport]` in Core |

---

## 8. Native Interop

### C# callable from native

There are two patterns depending on the caller:

**`[RuntimeExport]`**: for NativeAOT runtime stubs (called by the runtime itself). These match the `[RuntimeImport]` declarations in [`System.Private.CoreLib/RuntimeImports.cs`](https://github.com/dotnet/runtime/blob/main/src/coreclr/nativeaot/System.Private.CoreLib/src/System/Runtime/RuntimeImports.cs):

```csharp
[RuntimeExport("RhNewArray")]
internal static unsafe void* RhNewArray(MethodTable* pEEType, int length)
{
    return GarbageCollector.AllocArray(pEEType, length);
}
```

**`[UnmanagedCallersOnly]`**: for C# methods callable from native C code (bridges):

```csharp
[UnmanagedCallersOnly(EntryPoint = "__cosmos_serial_write")]
public static void CosmosSerialWrite(byte* str)
{
    // C code calls this via the symbol __cosmos_serial_write
}
```

Use `[RuntimeExport]` for runtime/ABI-required exports (for example `Rh*` stubs and libc/math or memory symbols such as `ceil`, `sqrt`, `memmove`, `memset`). Prefer `[UnmanagedCallersOnly]` for other native-to-managed callbacks (interrupt handlers, C library bridges, etc.).

### Native callable from C#

Modern P/Invoke pattern for calling assembly routines:

```csharp
[LibraryImport("*", EntryPoint = "_native_cpu_rdtsc")]
[SuppressGCTransition]
private static partial ulong NativeReadTSC();
```

**Rules:**
- Use `LibraryImport` (not `DllImport`), it's source-generated and AOT-compatible.
- Use `"*"` as the library name (links to the kernel binary itself).
- Add `[SuppressGCTransition]` for hot-path calls that don't trigger GC.
- Keep native interop methods `private` and wrap them in a clean public API.
- Assembly entry point names use `_snake_case` with a category prefix: `_native_cpu_rdtsc`, `_native_io_write_byte`.

---

## 9. Architecture-Specific Code

### Conditional Compilation

Use `#if ARCH_X64` / `#if ARCH_ARM64` for code that differs by architecture. For now this is only tolerated in `Cosmos.Kernel.Core` but this aims to disappear.

```csharp
public static void ComWrite(byte value)
{
#if ARCH_ARM64
    while ((Native.MMIO.Read32(PL011_BASE + PL011_FR) & FR_TXFF) != 0) ;
    Native.MMIO.Write8(PL011_BASE + PL011_DR, value);
#else
    while ((Native.IO.Read8(COM1_BASE + REG_LSR) & LSR_TX_EMPTY) == 0) ;
    Native.IO.Write8(COM1_BASE + REG_DATA, value);
#endif
}
```

### When to Use What

| Pattern | When |
|---------|------|
| `#if ARCH_X64` | Same class, small code differences (eg. serial driver) |
| Separate files (`*.X64.cs`) | Same class, large per-arch blocks (eg.  ThreadContext) |
| Separate HAL projects | Different implementations behind a shared interface |
| Separate Native projects | Assembly code (`.s`) |

### Guard Rules

- The default `#else` path should be x64 (most common development target).
- Always have both paths, no dangling `#if` without the other arch.
- Consider whether the code belongs in a HAL implementation instead of `#if`.

---

## 10. Memory & Safety

### Unsafe Code

Unsafe code is allowed (`<AllowUnsafeBlocks>true</AllowUnsafeBlocks>`) but should be contained in `Cosmos.Kernel.Core`:

```csharp
// Good: unsafe scoped to where needed
public static unsafe void WriteString(string str)
{
    fixed (char* ptr = str)
    {
        for (int i = 0; i < str.Length; i++)
        {
            ComWrite((byte)ptr[i]);
        }
    }
}

// Good: unsafe class for types that inherently work with pointers
public unsafe class Thread : SchedulerExtensible
{
    public ThreadContext* GetContext() => (ThreadContext*)StackPointer;
}
```

### Memory Allocation

```csharp
// Kernel heap allocation (manual, non-GC)
void* ptr = MemoryOp.Alloc(size);
MemoryOp.Free(ptr);

// GC-managed allocation (via RuntimeExport stubs)
// Happens automatically through normal C# object creation
List<int> list = new();  // uses RhAllocateNewArray under the hood
```

### Pointer Safety

- Always check pointers before dereferencing in GC/scanning code.
- Use `nuint` for addresses (not `uint`, which avoids 64-bit truncation).
- Use `nint`/`nuint` for pointer arithmetic, not `int`/`uint`.
- Use `stackalloc` for small, short-lived buffers instead of heap allocation.

### Critical Sections

```csharp
// Preferred: scoped interrupt disable
using (InternalCpu.DisableInterruptsScope())
{
    // Interrupts disabled here
    // Re-enabled automatically at scope exit
}
```

---

## 11. Error Handling

### Kernel Panic

For unrecoverable errors, use `Panic.Halt()`. It disables interrupts, prints
the message and the calling method, file and line, then halts the CPU:

```csharp
if (ptr == null)
    Panic.Halt("Memory allocation failed");
```

The caller information is filled in by the compiler, so pass the message and
nothing else. `Panic.Halt` is `[DoesNotReturn]`, which is what lets the code
after a null check dereference the value it just rejected.

### Exceptions

Exceptions work, but use them judiciously:

```csharp
// Good: validate at API boundaries
public static void RescanPartitions(IBlockDevice device)
{
    ArgumentNullException.ThrowIfNull(device);
    // ...
}

// Good: feature guards
private static void ThrowIfKeyboardDisabled()
{
    if (!CosmosFeatures.KeyboardEnabled)
        throw new InvalidOperationException("Console input requires keyboard feature.");
}

// Bad: exceptions in interrupt handlers, GC, or scheduler hot paths
// These paths cannot allocate (exception objects are heap-allocated)
```

### When to Panic vs. Throw

| Situation | Action |
|-----------|--------|
| Hardware failure, corrupted state | `Panic.Halt()` |
| GC/allocator internal error | `Panic.Halt()` |
| Invalid API usage | `throw` appropriate exception |
| Missing feature at runtime, in a member that acts on it | `throw InvalidOperationException` naming the switch |
| Missing feature at runtime, in a member that answers a question | Return `0`/`null`/`false`/empty |
| User-facing error in kernel shell | `try/catch` + print error message |

---

## 12. Nullable Reference Types

Nullable reference types are enabled solution-wide (`<Nullable>enable</Nullable>` in `Directory.Build.props`). Annotations are part of the API contract: a reference type without `?` is a promise that it is never null. New and modified code must not introduce nullable warnings: annotate honestly instead of silencing.

### Rules

| Pattern | Use for |
|---------|---------|
| `T?` on fields, parameters, returns | Any value that can legitimately be null |
| `?? throw` | Converting a nullable into a required value |
| `ArgumentNullException.ThrowIfNull(arg)` | Argument validation at API boundaries |
| `[MemberNotNull(...)]` guard helpers | Initialization checks the compiler can follow |
| `is null` / `is not null` | All null tests (not `== null` / `!= null`). A pointer keeps `== null`: patterns are not allowed on pointer types |
| Sentinel initialization (`= []`) | Fields where null and empty mean the same thing |

### Honest Annotations

A member that can return `null` is declared with `?`: never hide it behind `!` to preserve an old signature. Conversely, do not add runtime null checks for values the annotations already guarantee: no `if (buffer == null) throw` on a non-nullable parameter, no `?? "fallback"` on a non-nullable return.

```csharp
// Declare what can actually be null: callers are forced to handle it
public static IScheduler? Current => _currentScheduler;
public static PerCpuState? GetCpuState(uint cpuId) => _cpuStates?[cpuId];

// Prefer a sentinel over a nullable field when null and empty mean the same thing
private byte[] _window = [];                 // not: private byte[]? _window;
```

### Guard Helpers

Managers with deferred initialization expose `ThrowIf*NotInitialized()` guards annotated with `[MemberNotNull(...)]` and call them at the top of public entry points (see [Managers](#4-class--type-design)). One call proves the field non-null for the rest of the method, with no `!` and no per-line checks:

```csharp
private static PerCpuState[]? _cpuStates;

[MemberNotNull(nameof(_cpuStates))]
private static void ThrowIfCpuStateNotInitialized()
{
    if (_cpuStates is null)
    {
        throw new InvalidOperationException($"{nameof(SchedulerManager)} not initialized");
    }
}

public static void CreateThread(uint cpuId, Thread thread)
{
    ThrowIfCpuStateNotInitialized();

    PerCpuState state = _cpuStates[cpuId];   // no warning, no !
    // ...
}
```

### Required Values

Use `?? throw` where a null result is a programming error:

```csharp
Address source = IPConfig.FindNetwork(destination)
    ?? throw new InvalidOperationException("No network route to destination");

public MACAddress MacAddress => _macAddress
    ?? throw new InvalidOperationException($"{nameof(_macAddress)} is null");
```

### Constructors

Every constructor must leave all non-nullable members initialized. Delete unused parameterless constructors instead of letting them poison the type:

```csharp
// Bad: leaves RawData null and forces nullable noise on every user of the class
internal IcmpPacket()
{
}
```

### The `!` Operator

Null-forgiving `!` is a last resort for invariants flow analysis cannot see (eg. a field guaranteed non-null while a guard flag is `true`). If the invariant can be expressed with `[MemberNotNull]` or `[NotNullWhen]`, use the attribute instead.

### Enforcement

The format CI (`dotnet format --severity error`) enforces the null-handling style rules in `.editorconfig`: `csharp_style_throw_expression`, `dotnet_style_coalesce_expression`, `dotnet_style_null_propagation`, `csharp_style_prefer_null_check_over_type_check`, `csharp_style_pattern_matching_over_as_with_null_check`, `dotnet_style_prefer_is_null_check_over_reference_equality_method`, and `csharp_style_conditional_delegate_call`.

Nullable warnings (`CS86xx`) are not errors yet: do not introduce new ones.

---

## 13. Modern C# / .NET 10 Idioms

The project targets `<LangVersion>latest</LangVersion>` and .NET 10. Use modern language features throughout.

### Modern Patterns

```csharp
// File-scoped namespaces
namespace Cosmos.Kernel.Core.Scheduler;

// Expression-bodied members for trivial implementations
public string PlatformName => "x86-64";
public void Halt() => Native.Cpu.Halt();

// Pattern matching
switch (args[i])
{
    case null:
        WriteString("null");
        break;
    case string s:
        WriteString(s);
        break;
    case int n:
        WriteNumber(n);
        break;
}

// Null-conditional and coalescing
IBlockDevice? device = StorageManager.PrimaryDevice;
if (device?.BlockSize is not > 0)
{
    return;
}

Address ip = config?.Address ?? defaultAddress;

// Collection expressions
public IKeyboardDevice[] GetKeyboardDevices() => [new PS2Keyboard()];

// Target-typed new
Thread thread = new();
List<int> items = new();

// Numeric separators for readability
public const nuint DefaultStackSize = 64 * 1024;
public static long TscFrequency { get; set; } = 1_000_000_000;

// stackalloc for small buffers (no heap allocation)
Span<byte> buffer = stackalloc byte[64];

// field keyword: auto-property with validation without a manual backing field
public int Priority
{
    get => field;
    set => field = value >= 0 ? value : throw new ArgumentOutOfRangeException();
}
```

### Avoid

```csharp
// Don't use var when type isn't obvious
var x = GetValue();           // Bad: what type is x?
int count = GetValue();       // Good: explicit type

// Don't use var for built-in types
var i = 0;                    // Bad
int i = 0;                    // Good

// Don't use this. qualification
this._field = value;          // Bad
_field = value;               // Good

// Don't allocate arrays in hot paths when Span works
byte[] temp = new byte[16];       // Bad in hot path: heap allocation
Span<byte> temp = stackalloc byte[16]; // Good: stack allocation

// Always use braces, no single-line if/for/while without braces
if (ptr == null) return;          // Bad
if (ptr == null)                  // Bad
    return;

if (ptr == null)                  // Good
{
    return;
}
```

> Braces are enforced by `.editorconfig` (`csharp_prefer_braces = true:error`) and by CI (`dotnet format style --severity error`).

### Feature Switches

Use `[FeatureSwitchDefinition]` for compile-time feature toggling (trimmed by NativeAOT linker). All feature flags live in `Cosmos.Kernel.Core.CosmosFeatures`:

```csharp
// In CosmosFeatures.cs: one property per feature
[FeatureSwitchDefinition("Cosmos.Kernel.HAL.Interrupts.Enabled")]
public static bool InterruptsEnabled =>
    AppContext.TryGetSwitch("Cosmos.Kernel.HAL.Interrupts.Enabled", out bool enabled)
        ? enabled : true;

// The ILC linker trims the dead branch entirely
if (CosmosFeatures.KeyboardEnabled)
{
    // This code is removed from the binary when keyboard is disabled
}
```

---

## 14. AOT Constraints

NativeAOT imposes strict limitations. **All kernel code must be AOT-compatible.**

.NET 10 has improved NativeAOT with smaller binaries and broader platform support, but the fundamental constraints remain.

### Forbidden

- `System.Reflection.Emit` (no runtime code generation).
- `dynamic` keyword.
- `Assembly.Load` at runtime.
- `Type.GetType("...")` by string at runtime (types must be statically reachable).
- `MakeGenericType` / `MakeGenericMethod` at runtime (generic instantiations must be known at compile time).

### Use with Caution

- **Generic virtual methods**, NativeAOT must generate all possible instantiations at compile time. Avoid deeply nested or open-ended generic virtual dispatch.
- **LINQ**, many LINQ operators allocate iterators and closures. Avoid in hot paths (GC, scheduler, interrupt handlers).

---

## 15. Documentation

### License Header

Every `.cs` file must start with the license header (enforced by `.editorconfig`):

```csharp
// This code is licensed under the BSD 3-Clause license (see LICENSE for details)
```

### XML Documentation

Document all **public** APIs with `<summary>`. Keep it concise:

```csharp
/// <summary>
/// Triggers a kernel panic with the specified message.
/// Disables interrupts and halts the CPU.
/// </summary>
/// <param name="message">The panic message describing the error.</param>
public static void Halt(string message) { ... }
```

**Rules:**
- Document public classes, interfaces, methods, properties.
- Skip documentation on obvious members (`get`/`set` for self-documenting property names).
- Use `<param>` for non-obvious parameters.
- Use `<returns>` when return value needs explanation.
- Use `<see cref="..."/>` to reference related types.
- Don't document private implementation details unless the logic is complex.

### Inline Comments

Use sparingly, only when the code isn't self-explanatory:

```csharp
// Stack layout (growing downward from top):
// [StackBase + stackSize] = Top of usable stack
// [contextAddr] = Start of ThreadContext (where StackPointer points)
// [StackBase] = Bottom of stack

// Align context to 16 bytes (required for XMM operations)
contextAddr = (contextAddr + 0xF) & ~(nuint)0xF;
```

## 16. Testing

For the full testing guide (unit tests, kernel integration tests, UART protocol, CI, writing test kernels), see [Testing](testing.md).

**Code coverage:** Add the `run-coverage` label to a PR to trigger the coverage CI. It runs the kernel test suites and outputs which code paths are covered by the integration tests.

---

## 17. Public API Surface

Three rules decide what is `public` (the full policy and its mechanisms live in [Public API Tracking](public-api.md)):

1. **One supported ring.** `Cosmos.Kernel.System` is the API kernels program against, plus the contract types its signatures expose (the `HAL.Interfaces` device interfaces, the `HAL.Vfs` contracts, Core's platform interfaces). Only that surface is tracked, documented, and covered by deprecation cycles.
2. **Chosen experimental seams.** An extension point outside the ring is opened deliberately and marked `[Experimental("COSMOSxxxx")]`: usable now, no compatibility promise, promoted by removing the attribute. Never open a seam by just making something public.
3. **Everything else is `internal`.** Visibility is not the extension mechanism. First-party assemblies and white-box test kernels use `InternalsVisibleTo`; external code uses `[UnsafeAccessor]` ([Accessing internals](accessing-internals.md)) at its own risk.

Practical rules that follow:

```csharp
// Good: new user-facing capability lands as a Cosmos.Kernel.System facade
public static class MemoryInfo { public static ulong FreePages => PageAllocator.FreePageCount; }

// Bad: making the Core type public so a kernel can reach it
public static class PageAllocator { ... }
```

- **New types default to `internal`.** Making a symbol `public` in a tracked project is a reviewed decision: the build fails (`RS0016`) until `make api` records it in `PublicAPI.Unshipped.txt`, and the txt diff belongs in the same commit.
- **The enforcement test is DevKernel.** `examples/DevKernel` compiles with no `InternalsVisibleTo` grant; anything it needs must come from the supported ring.
- **Tracked surface must be documented.** Enabling `CosmosTrackPublicApi` turns missing XML docs (`CS1591`) into build errors for the project's public symbols.
- **A white-box test kernel gets an `InternalsVisibleTo` grant**, with a comment in the granting `.csproj` saying what it observes; it never forces a symbol public.

---
