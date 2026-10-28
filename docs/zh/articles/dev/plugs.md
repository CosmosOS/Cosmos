---
source_hash: eae72e86c2918abf1159faad629ffe89dcd7bb51
---
<a id="plugs"></a>

# Plug

Plug 在 IL 层面替换现有类型的成员：patcher（`cosmos-patcher`，基于 Mono.Cecil 构建）在 ILC 编译目标程序集之前重写该程序集，因此 BCL 和运行时代码无需修改源代码即可获得内核专用的实现。其机制、patcher 的流水线以及 plug 在构建中的运行位置由构建流水线文档介绍；本页介绍如何编写 plug。

---

<a id="attributes"></a>

## 特性

Gen3 实现了三个与 plug 相关的特性，全部定义在 [Cosmos.Build.API](https://github.com/CosmosOS/Cosmos/tree/gen3/src/Cosmos.Build.API/Attributes) 中：

| 特性 | 应用于 | 作用 |
|-----------|------------|--------|
| `[Plug]` | 类 | 将该类标记为某个目标类型的替换来源 |
| `[PlugMember]` | 方法、属性、字段 | 替换目标类型中与之匹配的成员 |
| `[PlatformSpecific]` | 类或成员 | 仅在针对所指定的架构打补丁时保留该元素 |

<a id="plug"></a>

### `[Plug]`

指定目标类型：当该类型可公开访问时，写成 `typeof(...)`；对于内部类型，则写成完全限定名字符串（`[Plug("Internal.Runtime.CompilerHelpers.StartupCodeHelpers")]`）。`IsOptional = true` 会在编译中不存在目标类型时静默跳过该 plug；非可选 plug 的 `TargetName` 拼写错误会导致构建失败。参见 [PlugAttribute.cs](https://github.com/CosmosOS/Cosmos/blob/gen3/src/Cosmos.Build.API/Attributes/PlugAttribute.cs)。

### `[PlugMember]`

替换名称和签名都与 plug 成员相匹配的目标成员。字符串参数会覆盖按名称进行的匹配，构造函数和属性访问器就是这样指定的：`[PlugMember(".ctor")]`、`[PlugMember("get_Address")]`。签名必须与目标成员完全一致，只有实例成员多出一处，见下文；签名不匹配意味着 patcher 无法接入该成员。参见 [PlugMemberAttribute.cs](https://github.com/CosmosOS/Cosmos/blob/gen3/src/Cosmos.Build.API/Attributes/PlugMemberAttribute.cs)。

### `[PlatformSpecific]`

在打补丁时将 plug 类或成员限定于特定架构：`[PlatformSpecific(PlatformArchitecture.X64)]`。参见 [PlatformSpecificAttribute.cs](https://github.com/CosmosOS/Cosmos/blob/gen3/src/Cosmos.Build.API/Attributes/PlatformSpecificAttribute.cs)。

---

<a id="the-athis-convention"></a>

## `aThis` 约定

Plug 类是静态类，因此实例成员由一个静态方法替换，该方法的第一个参数是目标实例，按约定命名为 `aThis`。patcher 会把对 `this` 的引用重新指向这个参数：

```csharp
using System.Net;
using Cosmos.Build.API.Attributes;

namespace Cosmos.Kernel.Plugs.System.Net;

[Plug(typeof(IPEndPoint))]
public static class IPEndPointPlug
{
    [PlugMember(".ctor")]
    public static void Ctor(IPEndPoint aThis, IPAddress address, int port)
    {
        // runs in place of the target constructor
    }

    [PlugMember("get_Address")]
    public static IPAddress? get_Address(IPEndPoint aThis)
    {
        // runs in place of the property getter
        return null;
    }

    [PlugMember]
    public static long Seek(IPEndPoint aThis, long offset)
    {
        // instance method: name and remaining signature match the target
        return 0;
    }

    [PlugMember]
    public static bool StaticMethod()
    {
        // static member: no aThis
        return true;
    }
}
```

---

<a id="accessing-private-and-internal-state"></a>

## 访问私有和内部状态

Gen2 的 `[Expose]`（向目标类型注入新的私有成员）和 `[FieldAccess]`（将 plug 方法的参数绑定到目标的私有字段）在 gen3 patcher 中尚未实现；移植工作在 [#458](https://github.com/valentinbreiz/nativeaot-patcher/issues/458) 中跟踪。在此之前，plug 有两种选择：

- **旁路存储。** 把每个实例的状态保存在 plug 类自身中，以实例为键（[IPEndPointPlug.cs](https://github.com/CosmosOS/Cosmos/blob/gen3/src/Cosmos.Kernel.Plugs/System/Net/IPEndPointPlug.cs) 就是用字典这样做的）。这种做法完全取代目标的状态，而不是读取它，因此每个涉及该状态的成员都必须用 plug 替换。
- **`[UnsafeAccessor]` / `[UnsafeAccessorType]`。** 运行时的访问器机制可以直接访问私有和内部成员。它是[公共 API 策略](public-api.md)中不受支持的应急手段；它能做什么、不能做什么，参见[访问内部成员](accessing-internals.md)（特别是：它无法按引用返回一个类型本身不可访问的字段）。

---

<a id="pitfalls"></a>

## 陷阱

- `TargetName` 无法解析的 plug 会导致构建失败，除非设置了 `IsOptional`；可选 plug 若悄无声息地不再匹配，仍会照常编译，而目标则在未打补丁的状态下运行，因此凡是关乎正确性的内容，都应优先使用非可选 plug。
- 如果 `[Plug]` 类的成员缺少 `[PlugMember]`，该类不会修补任何内容，也不会报错：类照常编译，目标在未打补丁的状态下运行。这已经造成过真实的静默失败（一个没有 plug 的 `NativeMemory` 不断递归，最终导致三重故障），所以请确认每个意在替换某些内容的成员上都带有该特性。
- 签名不匹配（包括 `aThis` 参数缺失或类型错误）意味着该成员不会被接入。
