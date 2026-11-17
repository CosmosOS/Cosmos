---
source_hash: b2df6833e632242a20abaaf6e64a84fd10ad53c3
---
<a id="accessing-internals"></a>

# 访问内部成员

[公共 API 跟踪](public-api.md)中所描述的受支持 API 面之外的一切，在设计上都是 `internal` 的：可见性不是扩展机制，扩展点（seam）才是。当没有任何扩展点能满足某个需求时，`[UnsafeAccessor]` 和 `[UnsafeAccessorType]` 可以直接访问内部状态，而无需 `InternalsVisibleTo` 授权。这个应急手段是有意提供的，也是有意不受支持的：内部成员在任何版本中都可能不经通知就发生变化，使用它们的代码预计会失效。

---

<a id="the-mechanism"></a>

## 机制

`[UnsafeAccessor]`（`System.Runtime.CompilerServices`）声明一个 `extern` 方法，由运行时将其绑定到一个原本无法访问的成员；`[UnsafeAccessorType]` 把这一机制扩展到本身就无法访问的类型，这些类型通过程序集限定名来指定。两者都由 ILC 静态解析，因此是 AOT 安全的，这一点与反射不同，而内核无法使用反射。

访问内部类型的内部静态成员：

```csharp
using System.Runtime.CompilerServices;

internal static class CoreAccessors
{
    // Binds to Cosmos.Kernel.Core.Memory.Heap.Heap.Collect(). The first
    // parameter carries the target type by name and is passed as null for
    // static members.
    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "Collect")]
    internal static extern int HeapCollect(
        [UnsafeAccessorType("Cosmos.Kernel.Core.Memory.Heap.Heap, Cosmos.Kernel.Core")] object? heap);
}

int freed = CoreAccessors.HeapCollect(null);
```

访问可访问类型的私有实例字段：

```csharp
[UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_name")]
private static extern ref string GetName(Partition partition);
```

`UnsafeAccessorKind` 涵盖方法、静态方法、构造函数和字段；属性访问器则作为名为 `get_X`/`set_X` 的方法来指定。

---

<a id="limits"></a>

## 限制

- **按引用返回不可访问类型会抛出异常。** 访问器不能以 `ref` 方式返回一个自身类型对声明程序集不可访问的字段；运行时按规范会拒绝这种做法。这就是 `Thread.CreateThread` 没有用 plug 替换的原因：上游的方法体会读取私有的 `StartHelper`，而它的类型无法按引用返回，因此扩展点改为位于它的下层（参见 [libSystemNative.cs](https://github.com/CosmosOS/Cosmos/blob/gen3/src/Cosmos.Kernel.Core/Bridge/Interop/libSystemNative.cs) 中的 `SystemNative_CreateThread`）。
- **解析失败会在调用点暴露出来**，而不是表现为编译错误：目标成员一旦被重命名或删除，访问器就会变成一个抛出异常的桩代码。
- **签名必须完全匹配**，包括少数带有自定义修饰符的成员上的那些修饰符。

---

<a id="when-to-use-it"></a>

## 何时使用

按优先顺序：

1. 受支持的 API 面（`Cosmos.Kernel.System` 加上各个契约接口）。如果其中缺少内核确实需要的东西，请提交 issue：扩展受支持层才是预期的解决方式。
2. 在存在 `[Experimental]` 扩展点的地方使用它（调度器扩展点，见[编写调度器](../user/scheduler-plugging.md)；驱动工具包，见[编写驱动程序](../user/drivers.md)）。
3. 当目标是替换行为而不是访问状态时，使用 [plug](plugs.md)。
4. `[UnsafeAccessor]`/`[UnsafeAccessorType]`，并接受任何版本都可能使其失效。
