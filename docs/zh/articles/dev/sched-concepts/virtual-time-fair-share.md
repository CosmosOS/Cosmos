---
source_hash: 61efc3aa0889868f154374a0ada65b6be50881f7
---
<a id="virtual-time-fair-share"></a>

# 基于虚拟时间的公平份额

公平份额（fair-share）调度，又称比例份额调度，按权重而不是按等级来划分 CPU 时间。每个线程持有一个权重，调度器的目标是在任意时间窗口内，各线程获得的 CPU 时间都与其权重成正比：权重为两倍的线程得到两倍的时间，而权重再小的线程也不会陷入饥饿。固定优先级调度回答的是另一个问题，即谁先运行；公平份额回答的则是每个线程各得多少。

虚拟时间就是保证这一点的记账方式。每个线程都带有自己的时钟，这个时钟只在线程运行时前进，且前进速率与其权重成反比：权重大的线程时钟走得慢，权重小的线程时钟走得快。调度器总是运行时钟最落后的线程，因此权重大的线程在时钟追上来之前会积累更多的真实时间。一个全局虚拟时钟标记“现在”：从睡眠中醒来的线程会以它为基准重新同步，位置近到很快就能运行，又不至于落后太远，以致为了偿还睡眠时间而独占 CPU。Stride 调度和 Linux 的 CFS 是这一类调度中最知名的成员。

在 Cosmos 内核中，默认策略 [`StrideScheduler`](https://github.com/CosmosOS/Cosmos/blob/gen3/src/Cosmos.Kernel.Core/Scheduler/Stride/StrideScheduler.cs) 正是这种方案：权重是 `Tickets`，时钟速率是步长（stride，即一个常量除以票数），每个线程自己的时钟是 `Pass`，而 `PickNext` 总是从按行程值排序的队列中弹出行程值最小的线程。参见 [Stride 策略](../scheduler.md#the-stride-policy)。
