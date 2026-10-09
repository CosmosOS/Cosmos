# Vendored: dotnet/runtime internals

Eleven files (about 10k lines, nearly 8k of them one generated reader) copied
from [dotnet/runtime](https://github.com/dotnet/runtime) (MIT), pinned at the
`dotnet/runtime` submodule commit `baf4eb1e0b672338897fc20de49daffe5f84569c`
(`v10.0.1-7-gbaf4eb1e0b67`). They are copied rather than linked from the
submodule, as most of `Cosmos.Kernel.Core` does, because upstream declares
their types `public` with no define to flip them, and this assembly tracks its
public surface in `PublicAPI.Unshipped.txt`.

Re-sync by copying the upstream file again and re-applying the local change
listed below. Anything in the first group is a keyword substitution; anything
in the second is a rewrite, so diff it rather than overwrite it.

---

## Visibility only

Upstream text with `public` changed to `internal` on the declarations, and
nothing else. `dotnet format` is not run on these.

| File | Upstream path under `src/coreclr/` |
| --- | --- |
| `NativeFormat/Metadata/MdBinaryReader.cs` | `tools/Common/Internal/Metadata/NativeFormat/MdBinaryReader.cs` |
| `NativeFormat/Metadata/MdBinaryReaderGen.cs` | `tools/Common/Internal/Metadata/NativeFormat/MdBinaryReaderGen.cs` |
| `NativeFormat/Metadata/MetadataTypeHashingAlgorithms.cs` | `tools/Common/Internal/Metadata/NativeFormat/MetadataTypeHashingAlgorithms.cs` |
| `NativeFormat/Metadata/NativeFormatReaderCommonGen.cs` | `tools/Common/Internal/Metadata/NativeFormat/NativeFormatReaderCommonGen.cs` |
| `NativeFormat/Metadata/NativeFormatReaderGen.cs` | `tools/Common/Internal/Metadata/NativeFormat/NativeFormatReaderGen.cs` |
| `NativeFormat/Metadata/NativeMetadataReader.cs` | `tools/Common/Internal/Metadata/NativeFormat/NativeMetadataReader.cs` |
| `NativeFormat/TypeHashingAlgorithms.cs` | `tools/Common/TypeSystem/Common/TypeHashingAlgorithms.cs` |
| `Runtime/ExceptionIDs.cs` | `nativeaot/Runtime.Base/src/System/Runtime/ExceptionIDs.cs` |

The first three are byte-identical to upstream: their types were already
`internal`. `ExceptionIDs.cs` collapses an `#if NATIVEAOT` ladder that picks
between `public` and `internal` down to the `internal` arm.

---

## Rewritten

Same type names and upstream layout contracts, different bodies. Treat
upstream as a reference, not as the source of truth.

| File | Upstream path under `src/coreclr/` | Local change |
| --- | --- | --- |
| `Runtime/ReadyToRunHeader.cs` | `tools/Common/Internal/Runtime/ModuleHeaders.cs` | Drops the `#if READYTORUN` arm of `ReadyToRunHeader`, and opens the header fields to the assembly (`private` to `internal`) so the boot path can read them. |
| `Runtime/MethodTable.Runtime.cs` | `nativeaot/Runtime.Base/src/System/Runtime/MethodTable.Runtime.cs` | Classlib lookups go through Cosmos `ModuleHelpers` instead of `InternalCalls`. `GetClasslibException` is commented out and `WellKnownEETypes` dropped, both unported. Adds `ModuleInfoRow`, which has no upstream counterpart. |
| `Runtime/TypeManagerHandle.cs` | `nativeaot/Common/src/Internal/Runtime/TypeManagerHandle.cs` | Keeps the handle wrapper and replaces upstream's two-field placeholder `TypeManager` with the full layout the native side publishes, plus `TypeManagerSlot` and the section lookups the module loader needs. |

`MethodTable.Runtime.cs` and `TypeManagerHandle.cs` diverged far enough that
they read as Cosmos files; they are listed here because their type names and
memory layouts still have to match what ILC emits and what
`src/coreclr/nativeaot/Runtime/inc/ModuleHeaders.h` declares on the native
side. Change either only together with its native counterpart.

See `docs/credits.md` for the full third-party list.
