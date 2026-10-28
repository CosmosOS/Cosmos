---
source_hash: 718968d73d90c8a0b512cfad67833f0e284c429a
---
<a id="tlab-thread-local-allocation-buffer"></a>

# TLAB（线程本地分配缓冲区）

当许多线程从同一个堆中分配内存时，共享的分配游标在每次分配时都需要一把锁或一次原子操作。TLAB 消除了这一开销：每个线程通过一次共享状态操作预留堆中的一块私有区域，然后在其中[以指针碰撞方式分配](bump-allocation.md)，完全不需要同步。只有当这块区域用尽时，线程才会回到共享状态进行重新填充。

代价是浪费：每个线程 TLAB 中未使用的尾部，在该 TLAB 被归还之前，是其他线程都无法使用的内存。

在 OrionGC 中，TLAB 就是存放在每个调度器线程上的 `AllocContext`（在调度器运行之前，或调度器被编译排除时，改由单个静态上下文代替），TLAB 的默认大小为 8 KiB，并且每次垃圾回收都会先归还所有 TLAB，以便清除阶段看到一致的堆。参见 [AllocContext](../garbage-collector.md#alloccontext-tlab)、[TLAB 重新填充](../garbage-collector.md#tlab-refill)和[归还 TLAB](../garbage-collector.md#returning-tlabs)。
