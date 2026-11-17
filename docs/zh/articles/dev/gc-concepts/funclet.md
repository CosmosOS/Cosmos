---
source_hash: 4f1e0b4cc82ee1adba023c4016ca2067326c9954
---
# Funclet

.NET 编译器不会把 `catch`、`filter` 或 `finally` 块内联编译在其所属的方法中。每个处理程序都会成为一个 funclet：一个独立的小函数，在处理程序运行时由异常分派器调用，拥有自己的栈帧，但可以通过父栈帧直接访问父方法的局部变量。

栈扫描必须了解这一点。处理程序运行期间，栈上存在 funclet 栈帧，它们的存活引用由父方法的元数据描述，而不是由它们自身的任何信息描述；与此同时，位于栈中更深处的父栈帧也仍然存活。如果扫描器把 funclet 当作普通方法对待，就会找不到它的 [GCInfo](conservative-vs-precise.md)（编译器生成的存活引用表），从而丢失对存活引用的跟踪。

在 OrionGC 中，funclet 的元数据会重定向到父方法的 GCInfo 记录，扫描 funclet 时，会把父方法的 GCInfo 应用到 funclet 实际运行的栈帧上。参见[精确扫描如何遍历线程](../garbage-collector-gcinfo.md#how-the-precise-scan-walks-a-thread)。
