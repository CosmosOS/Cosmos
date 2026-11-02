---
source_hash: 4ee8d070cc9a05ba415e67fdfdb1e34456614b93
---
<a id="garbage-collector-glossary"></a>

# 垃圾回收器术语表

这些简短的背景说明介绍通用的 GC 概念，[垃圾回收器](garbage-collector.md)和[精确栈扫描](garbage-collector-gcinfo.md)两篇文章都以这些概念为基础。每一页先单独解释概念本身，最后用一句话说明 OrionGC 如何应用它。调度方面的对应说明见[调度器术语表](scheduler-glossary.md)。

| 概念 | 摘要 |
|---------|---------|
| [全局停顿（stop-the-world）](gc-concepts/stop-the-world.md) | 在整个垃圾回收期间，所有线程都处于暂停状态 |
| [标记-清除](gc-concepts/mark-and-sweep.md) | 标记可达的对象，就地清除其余对象 |
| [移动式与非移动式](gc-concepts/non-moving.md) | 存活对象是否可能被重新定位 |
| [分代](gc-concepts/gc-generations.md) | 按对象年龄划分的堆，以及单一代能避免什么 |
| [指针碰撞分配](gc-concepts/bump-allocation.md) | 通过在全新的空间中向前推进指针来分配 |
| [TLAB](gc-concepts/tlab.md) | 每线程缓冲区，使分配的快速路径无需任何同步 |
| [GC 根](gc-concepts/gc-roots.md) | 可达性的起点 |
| [精确扫描与保守扫描](gc-concepts/conservative-vs-precise.md) | 确切知道栈上哪些字是引用，还是靠猜测 |
| [安全点](gc-concepts/safepoint.md) | 编译器生成的表能准确描述 GC 状态的代码位置 |
| [Funclet](gc-concepts/funclet.md) | 被编译成独立小函数、拥有各自栈帧的异常处理程序 |
| [终结与复活](gc-concepts/finalization.md) | 回收前运行的清理代码，以及死对象重新变得可达 |
| [运行时对象头](gc-concepts/object-header.md) | 紧挨在每个对象之前的簿记字（哈希码、瘦锁） |
| [ABA 问题](gc-concepts/aba-problem.md) | 当值可能被回收再利用时，无锁代码为什么需要版本标签 |
