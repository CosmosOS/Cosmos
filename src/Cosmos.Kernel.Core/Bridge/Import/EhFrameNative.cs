// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Core.Bridge;

/// <summary>
/// Native imports for the linker-provided <c>.eh_frame</c> section bounds.
/// <c>get_eh_frame_start</c> / <c>get_eh_frame_end</c> are exported by the C
/// runtime (kmain.c) and resolve to the linker symbols on both architectures,
/// so no arch gating is needed here.
/// </summary>
internal static unsafe partial class EhFrameNative
{
    [LibraryImport("*", EntryPoint = "get_eh_frame_start")]
    [SuppressGCTransition]
    public static partial byte* GetStart();

    [LibraryImport("*", EntryPoint = "get_eh_frame_end")]
    [SuppressGCTransition]
    public static partial byte* GetEnd();
}
