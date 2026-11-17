---
source_hash: 3e852f55a03fe1006569a40566e73a0da96812ee
---
<a id="scheduler-glossary"></a>

# 调度器术语表

这些说明为从未编写过调度器的读者提供调度概念方面的背景知识，贡献者文档中关于[调度器](scheduler.md)的文章以及[编写调度器](../user/scheduler-plugging.md)指南都以这些知识为基础。每一页先单独解释概念本身，最后说明 Cosmos 如何应用它。下表把它们分为三个部分：线程如何获得和失去 CPU，与时钟节拍并发运行的代码必须遵守什么，以及评判一个策略所依据的特性。钩子代码的规则还以驱动程序术语表中的一篇说明为基础：[中断上下文](../user/driver-concepts/interrupt-context.md)。

| 概念 | 摘要 |
|---------|---------|
| [策略与机制](sched-concepts/policy-and-mechanism.md) | 决定哪个线程运行，并与切换线程的代码分开 |
| [线程状态与运行队列](sched-concepts/run-queue.md) | 运行、就绪和阻塞的线程，就绪线程在其中等待的结构，以及空闲线程 |
| [抢占](sched-concepts/preemption.md) | 内核在中断到来时收回 CPU；线程从不需要主动让出 |
| [时钟节拍与时间片](sched-concepts/scheduler-tick.md) | 让调度器获得控制权的定时器中断、时间片，以及基于采样的记账 |
| [上下文切换](sched-concepts/context-switch.md) | 保存一个线程的寄存器并恢复另一个线程的寄存器，以及切换实际发生的时机 |
| [指令边界上的中断](sched-concepts/instruction-boundary.md) | 中断落在一条指令之后、下一条指令之前；任何更长的序列相对于它都不是原子的 |
| [自旋锁与中断屏蔽](sched-concepts/spinlocks-and-masking.md) | 为什么锁挡不住同一 CPU 上的中断处理程序，以及什么能挡住它 |
| [基于虚拟时间的公平份额](sched-concepts/virtual-time-fair-share.md) | 按权重分配的 CPU 份额，通过始终运行虚拟时钟最落后的线程来保证 |
| [饥饿与老化](sched-concepts/starvation.md) | 从不运行的就绪线程，以及防止这种情况的手段 |
| [优先级反转与优先级继承](sched-concepts/priority-inversion.md) | 高优先级线程卡在一个锁持有者之后，而该持有者又被中优先级线程抢占 |
| [实时调度](sched-concepts/real-time-scheduling.md) | 截止时间、速率单调（Rate Monotonic）与 EDF、利用率界限和准入控制 |
| [CPU 亲和性与负载均衡](sched-concepts/cpu-affinity.md) | 每 CPU 运行队列、放置、迁移和被固定的线程 |
