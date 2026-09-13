## Overview

This guide shows how to replace the kernel's scheduling policy. The mechanism side (context switching, the thread registry, the timer entry, the synchronization primitives, the GC bridge) never changes; everything an algorithm decides goes through one interface, [`IScheduler`](https://github.com/valentinbreiz/nativeaot-patcher/blob/main/src/Cosmos.Kernel.Core/Scheduler/IScheduler.cs). How the mechanism works, and how the default Stride policy uses this interface, is covered in [the scheduler article](scheduler.md); this page assumes it.

Replacing the policy takes three steps:

1. Implement `IScheduler`, using the per-thread and per-CPU data slots for the algorithm's bookkeeping. The seam is public: a policy lives in your own kernel project, with no access to `Cosmos.Kernel.Core` internals.
2. Respect the [kernel constraints](#kernel-constraints): the hooks run in interrupt context or under disabled interrupts, on live scheduler state.
3. Install it with `SchedulerManager.SetScheduler(new MyScheduler())`. The manager calls `ShutdownCpu` on the outgoing policy and `InitializeCpu` on the incoming one for every CPU. Call it from your kernel entry point at a quiescent point, before starting your own threads: nothing migrates queued threads or their attached bookkeeping into the new policy. You cannot get in ahead of the default: the kernel installs Stride during its own startup, so by the time any of your code runs, the boot thread is already registered and carrying a `StrideThreadData`. [Reading the data slots](#attaching-state) covers what that means for your hooks.

---

## Experimental status

The seam types (`IScheduler`, `SchedulerManager`, `SchedulerThread`, `PerCpuState`, `SchedulerExtensible`, `InterruptMaskScope`, `SchedulerThreadState`, `SchedulerThreadFlags`) carry `[Experimental("COSMOS0001")]`: they are usable today but make no compatibility promise, and they are promoted to the stable surface by removing the attribute once proven. Referencing them is a build error until the project acknowledges that contract:

```xml
<PropertyGroup>
  <NoWarn>$(NoWarn);COSMOS0001</NoWarn>
</PropertyGroup>
```

See [Public API Tracking](public-api.md) for how experimental seams fit the surface policy.

---

## The interface

Most hooks receive the `PerCpuState` they operate on, and run either under the manager's interrupt-masked lifecycle entries or in interrupt context itself; the exceptions are noted below. A policy that does not need a hook leaves it a no-op.

| Member | Called from | Contract |
|--------|-------------|----------|
| `Name` | logging | A display name for boot logs |
| `InitializeCpu(state)` | `SetScheduler` | Allocate the per-CPU bookkeeping into `state.SchedulerData` |
| `ShutdownCpu(state)` | `SetScheduler` | Release it (the incoming policy gets a clean slot) |
| `OnThreadCreate(state, thread)` | `CreateThread` | Allocate the per-thread bookkeeping into `thread.SchedulerData`; do not queue the thread yet |
| `OnThreadReady(state, thread)` | `ReadyThread` (wakes, first start, sleep expiry) | Make the thread runnable: place it and insert it into the run structure |
| `OnThreadBlocked(state, thread)` | `BlockThread` and `MarkSleeping` | Remove the thread from the run structure; save whatever must survive the park |
| `OnThreadYield(state, thread)` | `ScheduleFromInterrupt` (the preempted thread, if it stayed `Ready`) and `YieldThread` | Re-insert a thread that gave up the CPU |
| `OnThreadExit(state, thread)` | `ExitThread` | Remove it everywhere and drop its bookkeeping |
| `OnTick(state, current, elapsedNs)` | the timer interrupt | Account the elapsed time; return `true` to request a reschedule. `elapsedNs` is the configured tick interval, not a measurement |
| `PickNext(state)` | `ScheduleFromInterrupt` | Return the next thread to run, or `null` to run the idle thread |
| `OnPickFailed(state, thread)` | nothing yet | Declared for a pick the mechanism cannot honor; put the thread back. No caller today |
| `SelectCpu(thread, currentCpu, cpuCount)` | nothing yet | Choose a starting CPU for a thread; honor `ThreadFlags.Pinned` |
| `OnThreadMigrate(thread, fromState, toState)` | `Balance` implementations | Move the thread's bookkeeping (and any virtual-time base) between CPUs |
| `Balance(state, allCpuStates)` | nothing yet | Rebalance load across CPUs; honor `Pinned` |
| `SetPriority(state, thread, priority)` / `GetPriority(thread)` | `SchedulerManager.SetPriority`; the `SchedulerInfo` facade reads `GetPriority` on every thread snapshot | Priority is policy-defined: Stride reads it as tickets, a real-time policy would read it as a priority level. Neither is called with interrupts masked, and the spinlock around `SetPriority` does not exclude the tick |
| `GetRunQueueCount(state)` / `GetRunQueueThread(state, index)` | `SchedulerInfo` diagnostics | Read-only introspection of the run structure; guard it yourself (see [kernel constraints](#kernel-constraints)) |

`SelectCpu`, `Balance`, and `OnPickFailed` have no caller at all today: the kernel runs on one CPU, and the manager's `SelectCpu` and `Balance` wrappers are themselves dead. `SetPriority` does have a live caller, `SchedulerManager.SetPriority`, but nothing in the kernel mechanism reaches it. Implement all four for completeness, but do not rely on them being exercised. Note also that `Name`, `SelectCpu`, and `GetPriority` receive no `PerCpuState`, and that `InitializeCpu`/`ShutdownCpu` run from `SetScheduler` in thread context, with interrupts masked across the whole swap.

---

## Attaching state

`SchedulerThread` and `PerCpuState` both inherit `SchedulerExtensible`, which carries exactly one `object?` slot, `SchedulerData`, reserved for the active policy. Allocate in the creation hooks, and read the slot with `as`:

```csharp
public sealed class MyThreadData { public ulong Deadline; }
public sealed class MyCpuData { public List<SchedulerThread> Queue { get; } = new(); }

public void OnThreadCreate(PerCpuState state, SchedulerThread thread)
    => thread.SchedulerData = new MyThreadData();

public bool OnTick(PerCpuState state, SchedulerThread current, ulong elapsedNs)
{
    MyThreadData? data = current.SchedulerData as MyThreadData;
    if (data == null) { return true; }   // not ours, or already exited
    ...
}
```

Read with `as`, never a cast, and handle `null` on every hook. `OnThreadExit` clears the slot, so a thread can lose its record between a tick and the hook that observes it, and a cast that fails there does so inside the timer interrupt. `as` degrades that case to `null`, which every hook has to handle anyway.

A record written by another policy is not something a hook has to handle. `SetScheduler` moves every live thread with the swap: the outgoing policy sees `OnThreadExit` for each one, then the incoming policy sees `OnThreadCreate` for each one and `OnThreadReady` for those that were waiting in a run queue. A thread that is running at the moment of the swap (the boot thread at startup, or whichever thread called `SetScheduler`) arrives in `OnThreadCreate` in the `Running` state and must be accounted as runnable without being queued; Stride counts its tickets there. The seam used to publish a typed `GetSchedulerData<T>()` accessor; it was removed because the shipped Stride policy was its biggest user and a failed cast in a tick hook takes the kernel down.

One slot per object is the whole budget. A policy that needs several values defines one class holding them, as `StrideThreadData` and `StrideCpuData` do.

---

## Kernel constraints

The hooks run inside the kernel's most sensitive window, so four rules are not optional:

- **You are in interrupt context.** `OnTick` and `PickNext` run inside the timer interrupt; the lifecycle hooks run under `DisableInterruptsScope` from whatever thread called the manager. Nothing may block, park, or wait in a hook.
- **Do not allocate on the tick path.** Allocation is technically interrupt-safe in this kernel, but an allocation in `OnTick` or `PickNext` can trigger a collection inside the tick. Allocate in `OnThreadCreate` and `InitializeCpu`, where creation already pays for it, and pre-size collections there.
- **No `List<T>.Remove`, `Contains`, or `IndexOf` on scheduler paths.** They route through `EqualityComparer<T>.Default`, which needs runtime helpers the kernel does not provide. Scan with `ReferenceEquals` and use `RemoveAt`, as `StrideScheduler.RemoveThreadFromQueue` does.
- **Guard structure mutations against the tick.** A hook mutating the run structure can itself be interrupted by the timer unless interrupts are masked. The lifecycle hooks, the tick hooks and the two per-CPU hooks get that masking from the manager. Four do not: `SetPriority`, `GetPriority`, `GetRunQueueCount` and `GetRunQueueThread`. The spinlock around `SetPriority` is no help here, because the tick path takes no lock at all. Those, and any *additional* entry point a policy exposes (a tuning setter, a stats read), must take `SchedulerManager.MaskInterrupts()` themselves. Masking inside a hook makes one call atomic and no more: a caller that reads `GetRunQueueCount` and then walks the indices needs its own mask around the whole walk.

Bookkeeping the mechanism already does, so a policy does not have to: `SchedulerThread.State` transitions, the thread registry, `_needReschedule` on wakes, TLAB return on exit, and the idle-thread fallback when `PickNext` returns `null`.

---

## Worked sketches

The same questions recur for every algorithm: what to store per thread and per CPU, what shape the run structure takes, what triggers preemption in `OnTick`, and what must survive a park. The sketches below answer them for the classic algorithms.

### Round-Robin

A FIFO queue with fixed-quantum preemption.

| Hook | Behavior |
|------|----------|
| `PerCpuState.SchedulerData` | A queue of threads |
| `SchedulerThread.SchedulerData` | A remaining-quantum counter |
| `OnThreadReady` | Enqueue at the tail |
| `OnThreadBlocked` | Remove from the queue (a running thread is not in it; removal covers a queued thread going to sleep) |
| `OnTick` | Charge `elapsedNs` against the quantum; return `true` at zero |
| `OnThreadYield` | Re-enqueue at the tail, reset the quantum |
| `PickNext` | Dequeue the head |

FIFO order already bounds latency at `quantum * queue depth`, so Round-Robin needs no wakeup placement logic at all.

This sketch exists in-tree as a working policy: [`RoundRobinScheduler`](https://github.com/valentinbreiz/nativeaot-patcher/blob/main/tests/Kernels/Cosmos.Kernel.Tests.Threading/RoundRobinScheduler.cs) lives in the Threading suite exactly as a user policy would, over the public seam only. The suite validates it two ways, and the split is worth copying. The run-structure invariants (tail enqueue, head pick, quantum accounting, block/yield/exit) are asserted by driving the hooks directly on a throwaway `PerCpuState` and `SchedulerThread`, which needs no timer and no dispatch and so cannot flake; the policy keeps all its state in the data slots, so a throwaway instance exercises the real logic. Only what genuinely needs a running kernel (that threads get dispatched, that a spinner is preempted at quantum expiry, that shares come out equal whatever priority is requested, that the run queue tracks blocking and waking) is measured live, after swapping the policy in at a quiescent point. Note what the live half deliberately does *not* assert: the order threads reach their delegate is not the order they became ready, because a thread preempted inside its dispatch preamble is re-queued at the tail.

Both swap directions are worth copying as well. Going out, `RoundRobinScheduler` receives the boot thread and every other live thread through `OnThreadCreate` as `SetScheduler` re-homes them, and still reads every slot with `as` for the reason [above](#attaching-state). Coming back, the stock Stride policy is re-homed the same way, so a Round-Robin worker that is still alive simply becomes a Stride thread; the suite keeps a spinner alive across both swaps and checks that it keeps making progress under each policy.

### Multi-Level Feedback Queue (MLFQ)

Several priority levels; threads demote when they burn a full quantum and promote when they block early.

| Hook | Behavior |
|------|----------|
| `PerCpuState.SchedulerData` | An array of queues, one per level |
| `SchedulerThread.SchedulerData` | Current level and quantum-used counter |
| `OnThreadReady` | Enqueue at the thread's current level |
| `OnThreadBlocked` | Promote one level (it blocked before its quantum ran out: treat as interactive) |
| `OnTick` | Charge time; a full quantum at this level demotes on the next yield |
| `PickNext` | Scan levels top-down, dequeue the first non-empty head |
| periodic (e.g. every N ticks in `OnTick`) | Reset all threads to the top level, the classic anti-starvation boost |

MLFQ tracks no virtual time; its whole bookkeeping is integer levels.

### Fixed-priority preemptive (FPP)

The default policy of most RTOSes (FreeRTOS, Zephyr, ThreadX): the highest-priority runnable thread always runs, FIFO within a level.

| Hook | Behavior |
|------|----------|
| `PerCpuState.SchedulerData` | An array of queues indexed by priority |
| `SchedulerThread.SchedulerData` | A static priority |
| `OnThreadReady` | Enqueue at the thread's level; the `_needReschedule` the manager sets makes a higher-priority wake preempt on the next interrupt exit |
| `OnTick` | Return `true` if any level above the current thread's is non-empty (pure priority, no quantum) |
| `PickNext` | Top-down scan, dequeue the first head |
| `SetPriority` | Move the thread between levels |

**Rate Monotonic** is FPP with one extra rule in `OnThreadCreate`: assign priority from `1 / period` (shorter period, higher priority), and reject the thread if total utilization crosses the schedulability bound.

### Earliest-Deadline-First (EDF)

Dynamic priority by absolute deadline; optimal on one CPU (100% utilization against Rate Monotonic's ~69%), harder to reason about under overload.

| Hook | Behavior |
|------|----------|
| `PerCpuState.SchedulerData` | A min-heap keyed on absolute deadline |
| `SchedulerThread.SchedulerData` | Period, relative deadline, absolute deadline |
| `OnThreadReady` | `absolute = now + relative`, insert into the heap |
| `OnTick` | Return `true` if the heap root's deadline is earlier than the current thread's |
| `PickNext` | Pop the root |

### FIFO (cooperative)

A debugging policy: one queue, `OnTick` always returns `false`, threads run until they block or exit. Useful when chasing a race that disappears under preemption. Note the limits of "cooperative" here: with no working voluntary switch, a compute-bound thread that never blocks never leaves the CPU.

---

## Real-time notes

The policy/mechanism split makes the framework a plausible base for a real-time kernel: the context switch is deterministic (no allocation on the switch path), `Pinned` gives per-thread affinity, `Sleep` provides the wakeup deadline a periodic task needs, and `SetPriority` is the handle a priority protocol would use. What a hard-RT build still has to add sits on both sides of the interface:

1. **Bounded hook cost.** Everything in `OnTick` and `PickNext` is worst-case interrupt latency. Stride's linear sorted insert would not qualify; per-priority FIFOs or a heap keep the hooks O(log n) or better.
2. **Priority inheritance.** The kernel `Mutex` wakes FIFO and hands ownership directly to the head waiter, with no priority boost for the holder. Fair, but it inverts priorities. The inheritance protocol (boost the holder to the highest waiter's priority via `SetPriority`, restore on release) has its hook available and no implementation.
3. **A deadline-driven tick.** The scheduler tick is a fixed 10 ms interval. On ARM64 its driver (the Generic Timer) already re-arms a one-shot every interrupt; on x64 the tick is the hardware-periodic LAPIC timer, which would need switching to one-shot re-arm. On top of either sits the missing piece: the policy feedback that programs the next interrupt to the next deadline instead of a fixed period, a scheduler-to-timer channel that does not exist yet.
4. **Admission control.** Nothing stops oversubscription; a Rate Monotonic or EDF policy has to enforce its own utilization bound in `OnThreadCreate`.

---

## Checklist

1. Define the per-thread and per-CPU records; allocate them in `OnThreadCreate` and `InitializeCpu`, read them with `SchedulerData as MyRecord`, handle `null`.
2. Pick the run structure (queue, sorted list, heap, multi-level). The mechanism only ever asks `PickNext`.
3. Put the preemption decision, and nothing slow, in `OnTick`'s return value.
4. Decide what survives a park: whatever `OnThreadBlocked` saves is what wakeup placement in `OnThreadReady` has to work with.
5. Use `ReferenceEquals` scans, pre-sized collections, and `SchedulerManager.MaskInterrupts()` on any entry the manager does not already guard.
6. Implement `SelectCpu`, `OnThreadMigrate`, and `Balance` honoring `Pinned`, and treat them as dormant until SMP lands.
