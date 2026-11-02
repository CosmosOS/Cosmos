---
source_hash: bc6ddd1dddb90bf5c519f048164d2f7cfc9bb955
---
<a id="timers-and-alarms"></a>

# 定时器与闹钟

本文将介绍 Cosmos Gen3 上的延迟工作与周期性工作：阻塞一段时间、稍后运行回调，以及如何在提供这一功能的两个管理器之间做出选择。

首先要了解的就是这两个管理器。`TimerManager` 在定时器中断中运行回调；`AlarmManager` 在一个内核线程上运行回调。它们接受相同的参数，在调用处看起来几乎一模一样，而选错一个会让内核卡死。

| | `TimerManager` | `AlarmManager` |
|---|---|---|
| 回调运行于 | 中断上下文 | 线程上下文 |
| 回调可以阻塞、分配内存或获取锁 | 否 | 是 |
| 可以从何处发起调度调用 | 任何地方，包括中断处理程序 | 仅限线程上下文 |
| 需要调度器 | 否 | 是 |
| 精度 | 节拍源的周期 | 调度器节拍 |
| `Schedule` 的返回值 | 一个 `SoftwareTimer` 句柄，或 null | 一个闹钟 ID，或 0 |
| 功能开关 | `CosmosEnableTimer` | `CosmosEnableScheduler` |

如果你发现 bug 或任何异常，请在我们的仓库中[提交 issue](https://github.com/CosmosOS/Cosmos/issues/new/choose)。

---

<a id="enable-the-timer-in-your-kernel"></a>

## 在内核中启用定时器

定时器支持由一个功能开关控制。请确保内核的 `.csproj` 没有将其关闭（默认值为 `true`）；如果需要闹钟，也请保持调度器开启：

```xml
<PropertyGroup>
  <CosmosEnableTimer>true</CosmosEnableTimer>
  <CosmosEnableScheduler>true</CosmosEnableScheduler>
</PropertyGroup>
```

下面的代码片段依赖以下 `using` 指令：

```csharp
using System;
using Cosmos.Kernel.System.Diagnostics;
using Cosmos.Kernel.System.Timers;
```

节拍源在启动时就会被发现并注册，因此内核一开始运行，`TimerManager` 就已就绪。`TimerManager.IsEnabled` 回答编译时的问题，`TimerManager.IsInitialized` 回答运行时的问题。

---

<a id="waiting"></a>

## 等待

`TimerManager.Wait` 会让调用线程阻塞指定的毫秒数。它是本页最简单的功能，也是大多数内核最常用的功能，通常用来控制轮询循环的节奏：

```csharp
for (int i = 0; i < 10; i++)
{
    Console.WriteLine("tick " + i);
    TimerManager.Wait(500);
}
```

当 `CosmosEnableTimer=false` 时，它会抛出指明该开关的 `InvalidOperationException`，而不是立即返回、让循环空转。

---

<a id="running-a-callback-later"></a>

## 稍后运行回调

`TimerManager.Schedule` 在一段延迟后运行一次回调，`ScheduleRecurring` 则每隔一个周期运行一次回调。两者都会返回一个 `SoftwareTimer` 句柄：

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

句柄是只读的：调用方可以通过 `TimeoutNs`、`Recurring` 和 `IsActive` 查询一个尚未触发的定时器。把句柄交给 `Cancel` 即可停止该定时器。只有当定时器仍未触发时，`Cancel` 才返回 `true`，因此调用两次，或对已经触发过的一次性定时器调用，都不算错误。

**回调运行在中断上下文中。** 它不能阻塞，不能分配内存，也不能获取锁，因为被它中断的线程可能正持有某个锁。它也不能让异常逃逸：中断分发之上没有任何代码会捕获异常，内核会因此停机。

当定时器被编译排除，或者没有注册任何节拍源时，`Schedule` 和 `ScheduleRecurring` 返回 `null`，这就是句柄可为空的原因。`Cancel` 接受 null 并返回 `false`。

---

<a id="running-a-callback-that-needs-to-do-real-work"></a>

## 运行需要做实际工作的回调

上一段所禁止的一切都应交给闹钟来做。`AlarmManager` 拥有自己的内核线程，并在该线程上运行回调，因此闹钟回调可以阻塞、分配内存、使用调度器原语并获取锁：

```csharp
ulong id = AlarmManager.ScheduleRecurring(
    () => Console.WriteLine("free pages: " + MemoryDiagnostics.FreePages),
    TimeSpan.FromSeconds(5));

/* ... later ... */
AlarmManager.Cancel(id);
```

它的形式有意与 `TimerManager` 保持一致，这样在两者之间切换只需改一个词。不同之处在于返回值：闹钟属于闹钟系统，而不属于某个节拍源的注册表，因此它由一个 `ulong` ID 标识。零表示闹钟没有被调度，这会在调度器未运行或周期不为正数时发生。

**调度调用本身只能在线程上下文中进行。** `AlarmManager` 的每个成员都会获取闹钟列表的互斥锁，如果该锁已被持有，就会休眠等待。在中断处理程序中调用其中任何一个成员，都会在处理程序内部休眠等待并卡死，而 `TimerManager` 的回调正是中断处理程序。`TimerManager` 的成员采用屏蔽中断而不是休眠等待的方式，因此没有这一限制；所以，需要做实际工作的定时器回调应当通知一个线程，而不是调度一个闹钟。

周期性闹钟的周期从回调触发时重新开始计算，而不是从回调返回时开始，因此运行时间超过其周期的回调返回时，下一次触发已经到期。

---

<a id="changing-the-tick-rate"></a>

## 更改节拍频率

`TimerManager.Frequency` 以赫兹为单位读取和设置节拍源的频率。两个节拍源都以 100 Hz 启动，即每个节拍 10 ms，这就是 `TimerManager.Wait` 和已调度回调的精度。每个节拍源都是对一个固定计数器进行分频，因此各自只接受一个有限的范围：在 x64 PIT 上为 19 Hz 到 1193180 Hz，在 ARM64 通用定时器上为 1 Hz 到 `CNTFRQ_EL0`。超出范围的值会抛出 `ArgumentOutOfRangeException`，而不是被悄悄忽略：

```csharp
Console.WriteLine("timer runs at " + TimerManager.Frequency + " Hz");
TimerManager.Frequency = 1000;
```

提高频率会让已调度的回调触发得更接近其截止时间，同时也会让中断本身的开销更大。通常很少有理由修改它。

---

<a id="summary"></a>

## 总结

| 任务 | 调用 |
|---|---|
| 阻塞一段时间 | `TimerManager.Wait(ms)` |
| 稍后运行一个小回调 | `TimerManager.Schedule(callback, delay)` |
| 重复运行一个小回调 | `TimerManager.ScheduleRecurring(callback, period)` |
| 停止定时器 | `TimerManager.Cancel(timer)` |
| 运行会阻塞或分配内存的回调 | `AlarmManager.Schedule(callback, delay)` |
| 重复运行此类回调 | `AlarmManager.ScheduleRecurring(callback, period)` |
| 停止闹钟 | `AlarmManager.Cancel(id)` |
| 读取或设置节拍频率 | `TimerManager.Frequency` |
