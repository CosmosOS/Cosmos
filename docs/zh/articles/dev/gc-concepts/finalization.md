---
source_hash: 28f09e332e3850f07b74a172adfb03de8db7646a
---
<a id="finalization-and-resurrection"></a>

# 终结与复活

终结器（C# 中的 `~ClassName`）在对象的内存被回收之前运行清理代码，通常用于释放原生资源。终结改变了回收器的工作：一个带有待处理终结器的不可达对象还不能被释放，因此 GC 必须将它入队，让一个专用线程运行终结器，并且要等到之后的某次垃圾回收才能回收其内存。

复活是由此带来的副作用：对象在队列中时再次变为可达（队列引用着它），它的终结器甚至可以把 `this` 存放到某个可达的位置，让一个已被判定死亡的对象永久地重新存活。两种弱引用恰恰在这一点上不同：短弱引用（`WeakReference` 默认就是这种）在对象被发现不可达时立即被置空，而跟踪复活的弱引用（`trackResurrection: true`，即 `WeakTrackResurrection` 句柄类型）会一直返回该对象，直到它真正消失为止。

> [!NOTE]
> 官方文档：[终结器](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/finalizers)。

OrionGC 把等待终结器运行的对象保存在一个队列中，该队列是每次垃圾回收的根；它在这些对象入队之前置空短弱句柄，在入队之后置空跟踪复活的弱句柄，并在垃圾回收之后运行终结器。终结器在一个专用的终结器线程上运行：当 `CosmosEnableFinalizerThread` 开关打开时（只要调度器打开，这就是默认设置），由 `Kernel.Start` 启动该线程；否则，终结器在执行垃圾回收或进行分配的线程上运行。参见[终结](../garbage-collector.md#finalization)和[标记期间的句柄](../garbage-collector.md#handles-during-marking)。
