---
source_hash: f5bf16108d85ebd0f707c7ab3e564739f79c9863
---
<a id="cpu-affinity-and-load-balancing"></a>

# CPU 亲和性与负载均衡

在有多个 CPU 的机器上，调度器通常为每个 CPU 配备各自的运行队列。单一的共享队列需要一把锁，每个 CPU 在每次切换时都要争夺它；而一个总是回到同一个 CPU 的线程，会发现自己的数据仍留在该 CPU 的缓存中。每 CPU 队列也带来了它们自己的两个决策。放置是指在线程被创建或唤醒时为它选择一个 CPU，通常是负载最轻的那个。负载均衡则是在之后移动线程：当一个队列排得很长、而另一个 CPU 却处于空闲时，空闲的 CPU 从繁忙的 CPU 拉取（或者说窃取）一个线程，或者由繁忙的 CPU 推走一个线程。

亲和性限制了线程可以去往的位置。硬亲和性，也称固定（pinning），把线程限制在一个 CPU 或一组 CPU 上，用于每 CPU 线程（例如空闲线程），或用于访问每 CPU 状态的代码。软亲和性只是一种偏好，即调度器拿来与负载相权衡的缓存热度。

陷阱在于随线程一起迁移的每 CPU 状态。维护每 CPU 时钟的策略，例如[公平份额](virtual-time-fair-share.md)调度器的虚拟时间，必须以目标 CPU 的时钟为基准重新设定迁移中的线程，否则该线程到达后会远远领先或远远落后于那里的所有线程。迁移还会同时触及两个 CPU 的队列，因此它必须能安全应对两者的节拍；而且均衡器绝不能窃取被固定的线程。

在 Cosmos 中，内核目前只在一个 CPU 上运行。负责放置和均衡线程的钩子 `SelectCpu` 和 `Balance` 没有调用者，`OnThreadMigrate` 只有在策略自己的 `Balance` 调用它时才会运行。尽管如此，整体结构仍是按每 CPU 设计的：每个触及运行结构的钩子都会收到它所操作的 `PerCpuState`，策略把自己的运行结构保存在该状态的 `SchedulerData` 中，`SchedulerThread.CpuId` 指明线程被分配到的 CPU，因此现在按每 CPU 编写的策略以后可以直接沿用。`SchedulerThreadFlags.Pinned` 标记不得迁移的线程；只有空闲线程带有该标志，因为内核可以读取线程的标志，却不能设置它们。Stride 的 `Balance` 在自己的队列为空、且最长的队列包含不止一个线程时，从该队列窃取队尾线程，除非队尾线程被固定；它的 `OnThreadMigrate` 以目标 CPU 的 `GlobalPass` 加上线程保存的 `Remain` 为基准，重新设定该线程的 `Pass`。参见[局限与演进](../scheduler.md#limitations-and-evolution)。
