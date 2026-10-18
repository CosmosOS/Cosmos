# Finalization and resurrection

A finalizer (`~ClassName` in C#) runs cleanup code before an object's memory is reclaimed, usually to release a native resource. Finalization changes the collector's job: an unreachable object with a pending finalizer cannot be freed yet, so the GC must queue it, let a dedicated thread run the finalizer, and only reclaim the memory in a later collection.

Resurrection is the side effect: while queued, the object is reachable again (the queue references it), and its finalizer can even store `this` somewhere reachable, making an object that was found dead permanently live again. The two flavors of weak reference differ exactly here: a short weak reference (`WeakReference` by default) is cleared as soon as the object is found unreachable, while a resurrection-tracking one (`trackResurrection: true`, the `WeakTrackResurrection` handle type) keeps reporting the object until it is truly gone.

> [!NOTE]
> Official docs: [Finalizers](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/finalizers).

OrionGC keeps the objects awaiting their finalizer in a queue that is a root of every collection, clears the short weak handles before queuing them and the resurrection-tracking ones after, and runs the finalizers after the collection. By default they run on the thread that collected, and on a dedicated finalizer thread only when Core turns `GarbageCollector.ThreadedFinalizer` on. See [Finalization](../garbage-collector.md#finalization) and [Handles during marking](../garbage-collector.md#handles-during-marking).
