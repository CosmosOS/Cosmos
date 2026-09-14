# Accessing internals

Everything outside the supported surface described in [Public API Tracking](public-api.md) is `internal` by design: visibility is not the extension mechanism, seams are. When no seam covers a need, `[UnsafeAccessor]` and `[UnsafeAccessorType]` reach internal state directly, without an `InternalsVisibleTo` grant. That hatch is deliberately available and deliberately unsupported: internals change in any release without notice, and code using them is expected to break.

---

## The mechanism

`[UnsafeAccessor]` (`System.Runtime.CompilerServices`) declares an `extern` method that the runtime binds to an otherwise inaccessible member; `[UnsafeAccessorType]` extends it to types that are themselves inaccessible, addressed by assembly-qualified name. Both are resolved statically by ILC, so they are AOT-safe, unlike reflection, which the kernel cannot use.

Accessing an internal static member of an internal type:

```csharp
using System.Runtime.CompilerServices;

internal static class CoreAccessors
{
    // Binds to Cosmos.Kernel.Core.Memory.Heap.Heap.Collect(). The first
    // parameter carries the target type by name and is passed as null for
    // static members.
    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "Collect")]
    internal static extern int HeapCollect(
        [UnsafeAccessorType("Cosmos.Kernel.Core.Memory.Heap.Heap, Cosmos.Kernel.Core")] object? heap);
}

int freed = CoreAccessors.HeapCollect(null);
```

Accessing a private instance field of an accessible type:

```csharp
[UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_name")]
private static extern ref string GetName(Partition partition);
```

`UnsafeAccessorKind` covers methods, static methods, constructors, and fields; property accessors are addressed as methods named `get_X`/`set_X`.

---

## Limits

- **Byref returns of inaccessible types throw.** An accessor cannot `ref`-return a field whose own type is inaccessible to the declaring assembly; the runtime rejects it by spec. This is why `ThreadPlug` does not plug `Thread.CreateThread`: the upstream body reads the private `StartHelper`, whose type cannot be byref-returned, so the seam runs below it instead (see the comment in [ThreadPlug.cs](https://github.com/valentinbreiz/nativeaot-patcher/blob/main/src/Cosmos.Kernel.Plugs/System/Threading/ThreadPlug.cs)).
- **Resolution failures surface at the call site**, not as a compile error: a renamed or removed target member turns the accessor into a throwing stub.
- **Signatures must match exactly**, including custom modifiers on the rare members that carry them.

---

## When to use it

In order of preference:

1. The supported surface (`Cosmos.Kernel.System` plus the contract interfaces). If it is missing something a kernel legitimately needs, open an issue: extending the ring is the intended fix.
2. An `[Experimental]` seam where one exists (the scheduler seam, [Scheduler - Writing a Scheduler](scheduler-plugging.md)).
3. A [plug](plugs.md), when the goal is replacing behavior rather than reaching state.
4. `[UnsafeAccessor]`/`[UnsafeAccessorType]`, accepting that any release may break it.
