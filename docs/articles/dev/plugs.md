# Plugs

A plug replaces members of an existing type at IL level: the patcher (`cosmos-patcher`, built on Mono.Cecil) rewrites the target assembly before ILC compiles it, so BCL and runtime code acquire kernel-specific implementations without source changes. The mechanism, the patcher pipeline, and where plugs run in the build are covered by the build pipeline docs; this page covers writing one.

---

## Attributes

Gen3 implements three plug-related attributes, all defined in [Cosmos.Build.API](https://github.com/valentinbreiz/nativeaot-patcher/tree/main/src/Cosmos.Build.API/Attributes):

| Attribute | Applied to | Effect |
|-----------|------------|--------|
| `[Plug]` | class | Marks the class as the replacement source for one target type |
| `[PlugMember]` | method, property, field | Replaces the matching member of the target type |
| `[PlatformSpecific]` | class or member | Keeps the element only when patching for the named architectures |

### `[Plug]`

Names the target type, either as `typeof(...)` when the type is publicly reachable or as a fully qualified name string for internal types (`[Plug("Internal.Runtime.CompilerHelpers.StartupCodeHelpers")]`). `IsOptional = true` skips the plug silently when the target type does not exist in the compilation; a mistyped `TargetName` on a non-optional plug fails the build. See [PlugAttribute.cs](https://github.com/valentinbreiz/nativeaot-patcher/blob/main/src/Cosmos.Build.API/Attributes/PlugAttribute.cs).

### `[PlugMember]`

Replaces the target member that matches the plug member's name and signature. A string argument overrides the name matching, which is how constructors and property accessors are addressed: `[PlugMember(".ctor")]`, `[PlugMember("get_Address")]`. Signatures must match the target member exactly, with one addition for instance members described below; a mismatch means the patcher cannot wire the member up. See [PlugMemberAttribute.cs](https://github.com/valentinbreiz/nativeaot-patcher/blob/main/src/Cosmos.Build.API/Attributes/PlugMemberAttribute.cs).

### `[PlatformSpecific]`

Filters a plug class or member to specific architectures at patch time: `[PlatformSpecific(PlatformArchitecture.X64)]`. See [PlatformSpecificAttribute.cs](https://github.com/valentinbreiz/nativeaot-patcher/blob/main/src/Cosmos.Build.API/Attributes/PlatformSpecificAttribute.cs).

---

## The `aThis` convention

Plug classes are static, so an instance member is plugged by a static method whose first parameter is the target instance, conventionally named `aThis`. The patcher rewires `this` references onto that parameter:

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
    public static bool StaticMethod() => true;   // static member: no aThis
}
```

---

## Accessing private and internal state

Gen2's `[Expose]` (inject new private members into the target type) and `[FieldAccess]` (bind a plug-method parameter to a private field of the target) are not implemented in the gen3 patcher; porting them is tracked in [#458](https://github.com/valentinbreiz/nativeaot-patcher/issues/458). Until then a plug has two options:

- **Side storage.** Keep per-instance state in the plug class itself, keyed by the instance ([IPEndPointPlug.cs](https://github.com/valentinbreiz/nativeaot-patcher/blob/main/src/Cosmos.Kernel.Plugs/System/Net/IPEndPointPlug.cs) does this with dictionaries). This replaces the target's state entirely rather than reading it, so every member that touches the state must be plugged.
- **`[UnsafeAccessor]` / `[UnsafeAccessorType]`.** The runtime's accessor mechanism reaches private and internal members directly. It is the unsupported escape hatch of the [public API policy](public-api.md); see [Accessing internals](accessing-internals.md) for what it can and cannot do (notably: it cannot byref-return a field whose type is itself inaccessible).

---

## Pitfalls

- A plug whose `TargetName` does not resolve fails the build unless `IsOptional` is set; an optional plug that silently stops matching keeps compiling while the target runs unpatched, so prefer non-optional plugs for anything correctness-critical.
- A `[Plug]` class whose members lack `[PlugMember]` patches nothing, and no error is raised: the class compiles, the target runs unpatched. This has produced real silent failures (an unplugged `NativeMemory` recursed into a triple fault), so check the attribute is on every member meant to replace something.
- Signature mismatches (including a missing or mistyped `aThis` parameter) mean the member is not wired up.
