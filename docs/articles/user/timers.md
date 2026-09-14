# Timers and alarms

In this article, we will discuss delayed and repeating work on Cosmos Gen3: blocking for a while, running a callback later, and choosing between the two managers that offer to run one.

The two managers are the thing to read first. `TimerManager` runs a callback from the timer interrupt; `AlarmManager` runs one on a kernel thread. They take the same arguments and read almost identically at the call site, and picking the wrong one hangs the kernel.

| | `TimerManager` | `AlarmManager` |
|---|---|---|
| Callback runs in | Interrupt context | Thread context |
| The callback may block, allocate or take a lock | No | Yes |
| Scheduling calls may be made from | Anywhere, interrupt handlers included | Thread context only |
| Needs the scheduler | No | Yes |
| Resolution | The timer device tick | The scheduler tick |
| `Schedule` hands back | A `SoftwareTimer` handle, or null | An alarm id, or 0 |
| Feature switch | `CosmosEnableTimer` | `CosmosEnableScheduler` |

If you find bugs or something abnormal, please [submit an issue](https://github.com/valentinbreiz/nativeaot-patcher/issues/new) on our repository.

---

## Enable the timer in your kernel

Timer support is behind a feature switch. Make sure your kernel's `.csproj` does not turn it off (it defaults to `true`), and leave the scheduler on as well if you want alarms:

```xml
<PropertyGroup>
  <CosmosEnableTimer>true</CosmosEnableTimer>
  <CosmosEnableScheduler>true</CosmosEnableScheduler>
</PropertyGroup>
```

These are the `using`s the snippets below rely on:

```csharp
using System;
using Cosmos.Kernel.HAL.Interfaces.Devices;
using Cosmos.Kernel.System.Diagnostics;
using Cosmos.Kernel.System.Timer;
```

The timer device is found and registered at boot, so `TimerManager` is ready as soon as your kernel runs. `TimerManager.IsEnabled` answers the compile-time question and `TimerManager.IsInitialized` the runtime one.

---

## Waiting

`TimerManager.Wait` blocks the calling thread for a number of milliseconds. It is the simplest thing on this page and the one most kernels reach for, usually to pace a poll loop:

```csharp
for (int i = 0; i < 10; i++)
{
    Console.WriteLine("tick " + i);
    TimerManager.Wait(500);
}
```

With `CosmosEnableTimer=false` this throws `InvalidOperationException` naming the switch, rather than returning at once and leaving a loop spinning.

---

## Running a callback later

`TimerManager.Schedule` runs a callback once after a delay, and `ScheduleRecurring` runs one every period. Both hand back a `SoftwareTimer` handle:

```csharp
bool cursorVisible = false;

/* Flipping a field is the shape an interrupt-context callback should have:
   the render loop reads it and does the drawing. */
SoftwareTimer? blink = TimerManager.ScheduleRecurring(
    () => cursorVisible = !cursorVisible,
    TimeSpan.FromMilliseconds(250));

/* ... later ... */
TimerManager.Cancel(blink);
```

The handle is read-only: `TimeoutNs`, `Recurring` and `IsActive` are what a caller can ask about a pending timer. Hand it back to `Cancel` to stop it. `Cancel` returns `true` only when the timer was still pending, so calling it twice, or on a one-shot that has already fired, is not an error.

**The callback runs in interrupt context.** It must not block, must not allocate, and must not take a lock, because the thread it interrupted may be holding one. It must also not let an exception escape: nothing above the interrupt dispatch catches one, and the kernel halts.

`Schedule` and `ScheduleRecurring` return `null` when the timer is compiled out or no device registered, which is why the handle is nullable. `Cancel` accepts null and answers `false`.

---

## Running a callback that needs to do real work

Anything the paragraph above forbids belongs on an alarm. `AlarmManager` keeps its own kernel thread and runs callbacks there, so an alarm callback may block, allocate, use scheduler primitives and take locks:

```csharp
ulong id = AlarmManager.ScheduleRecurring(
    () => Console.WriteLine("free pages: " + MemoryInfo.FreePages),
    TimeSpan.FromSeconds(5));

/* ... later ... */
AlarmManager.Cancel(id);
```

The shape is deliberately the same as `TimerManager`'s so that switching between them is one word. What differs is what you get back: an alarm belongs to the alarm system rather than to a device registry, so it is identified by a `ulong` id. Zero means the alarm was not scheduled, which happens when the scheduler is not running or the period is not positive.

**The scheduling calls themselves are thread-context only.** Every `AlarmManager` member takes the alarm list's mutex and parks if it is held. Calling one from an interrupt handler parks inside the handler and hangs, and a `TimerManager` callback is an interrupt handler. The `TimerManager` members mask interrupts instead of parking and carry no such restriction, so a timer callback that needs to do real work should signal a thread rather than schedule an alarm.

A recurring alarm's period restarts when the callback fires, not when it returns, so a callback that runs longer than its period leaves the next firing already due.

---

## Changing the tick rate

`TimerManager.Frequency` reads and writes the timer device's tick rate in hertz. Each device divides a fixed counter, so each accepts a bounded range: 19 Hz to 1193180 Hz on the x64 PIT, and 1 Hz up to `CNTFRQ_EL0` on the ARM64 generic timer. A value outside its device's range throws `ArgumentOutOfRangeException` rather than being quietly ignored:

```csharp
Console.WriteLine("timer runs at " + TimerManager.Frequency + " Hz");
TimerManager.Frequency = 1000;
```

Raising it makes scheduled callbacks fire closer to their deadline and makes the interrupt itself more expensive. There is rarely a reason to change it.

---

## Summary

| Task | Call |
|---|---|
| Block for a while | `TimerManager.Wait(ms)` |
| Run a small callback later | `TimerManager.Schedule(callback, delay)` |
| Run a small callback repeatedly | `TimerManager.ScheduleRecurring(callback, period)` |
| Stop one | `TimerManager.Cancel(timer)` |
| Run a callback that blocks or allocates | `AlarmManager.Schedule(callback, delay)` |
| Run such a callback repeatedly | `AlarmManager.ScheduleRecurring(callback, period)` |
| Stop one | `AlarmManager.Cancel(id)` |
| Read or set the tick rate | `TimerManager.Frequency` |
