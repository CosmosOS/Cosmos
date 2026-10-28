---
source_hash: 8bfd475448048c47c4d4ddc44c203f5f2653fd2d
---
<a id="safepoint"></a>

# 安全点

安全点（safepoint）是代码中的一个位置，编译器的附属表（在 NativeAOT 中为 GCInfo）在该位置精确描述方法的 GC 状态：哪些寄存器和栈槽在这一点持有存活引用。在两个安全点之间，这些表不提供任何信息，因此只有对停在某个安全点上的线程，[精确扫描](conservative-vs-precise.md)才是可靠的。

对栈遍历而言，真正重要的安全点是调用点。方法在等待被调用者返回时，其栈帧冻结在调用指令处，因此线程栈上的每个返回地址都标识了其调用者中的一个安全点。于是，通过一连串调用进入垃圾回收器的线程可以逐帧精确扫描，而在任意指令处被抢占的线程则不能。运行时用返回地址劫持（return-address hijacking）弥合这一差距：强制被抢占的线程运行到某个安全点，然后再扫描它。

在 OrionGC 中，只有触发垃圾回收的线程确定停在安全点上；在实现劫持之前，其他所有线程都以保守方式扫描（[#385](https://github.com/valentinbreiz/nativeaot-patcher/issues/385)）。参见[安全点约束](../garbage-collector-gcinfo.md#the-safepoint-constraint)。
