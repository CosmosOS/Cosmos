# Safepoint

A safepoint is a code location where the compiler's side tables (GCInfo, in NativeAOT) describe the GC state of a method exactly: which registers and stack slots hold live references at that point. Between safepoints the tables say nothing, so a [precise scan](conservative-vs-precise.md) is only sound for a thread stopped at one.

Call sites are the safepoints that matter for stack walking. While a method waits for a callee to return, its frame is frozen at the call instruction, so every return address on a thread's stack identifies a safepoint in its caller. A thread that entered the collector through a chain of calls can therefore be scanned precisely frame by frame, while a thread preempted at an arbitrary instruction cannot. Runtimes bridge that gap with return-address hijacking: force the preempted thread to run to a safepoint, then scan it.

In OrionGC only the collection-triggering thread is known to sit at safepoints; every other thread is scanned conservatively until hijacking exists ([#385](https://github.com/valentinbreiz/nativeaot-patcher/issues/385)). See [The safepoint constraint](../garbage-collector-gcinfo.md#the-safepoint-constraint).

The kernel's `CpuStop.Safepoint` borrows the word for something else: the point inside a masked wait (a spinlock's spin, the claim of the stop itself, the interrupt exit's wait on the on-CPU marker) where a CPU answers another CPU's stop ([Stopping the world](../garbage-collector.md#stopping-the-world)). It is not a GCInfo safepoint: the thread held there is scanned conservatively from the registers the safepoint captured.
