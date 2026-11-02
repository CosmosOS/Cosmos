---
source_hash: 7de2922808c9c174261bed64f942bd6026aa8967
---
<a id="runtime-object-header"></a>

# 运行时对象头

在 .NET 中，除了位于偏移 0 的 `MethodTable` 指针之外，每个对象还在对象引用之前紧挨着的负偏移处带有一个头字。运行时用它保存那些没有专属字段的簿记信息：缓存的标识哈希码（对普通对象调用 `GetHashCode` 时返回的值），以及 `Monitor.Enter`（即 `lock` 语句）在无竞争的对象上获取的瘦锁。

垃圾回收器从不读取哈希码或锁，但必须为这个字留出空间：运行时会写入 `objRef - 4`，因此每个对象之前都必须有可写的字节，包括段中最前面的第一个对象，以及任何紧跟在回收再利用的空闲空间之后的对象。CoreLib 把这个字的最高三位保留给回收器，它的比较并交换（compare-exchange）循环会原样保留这三位；OrionGC 使用其中一位，即第 30 位（.NET 的 `BIT_SBLK_FINALIZER_RUN`），作为 `GC.SuppressFinalize` 设置的标志，参见[终结](../garbage-collector.md#finalization)。

在 OrionGC 中，这就是每个段在其第一个对象之前保留 8 个字节、每个空闲块把最后 8 个字节排除在分配之外的原因。这个头不同于从对象引用本身开始的 [GCObject 布局](../garbage-collector.md#object-header)。参见[段](../garbage-collector.md#segments)和[空闲链表](../garbage-collector.md#free-lists)。
