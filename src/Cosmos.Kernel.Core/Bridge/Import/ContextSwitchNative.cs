// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Core.Bridge;

/// <summary>
/// Native imports for the low-level context switch primitives.
/// Symbols are unified across architectures: Cosmos.Kernel.Native.X64
/// (Interrupts.s) and Cosmos.Kernel.Native.ARM64 (ContextSwitch.s) both
/// export the same names, so no arch gating is needed here.
/// </summary>
internal static unsafe partial class ContextSwitchNative
{
    [LibraryImport("*", EntryPoint = "_native_set_context_switch_sp")]
    [SuppressGCTransition]
    public static partial void SetContextSwitchSp(nuint newSp);

    [LibraryImport("*", EntryPoint = "_native_get_context_switch_sp")]
    [SuppressGCTransition]
    public static partial nuint GetContextSwitchSp();

    [LibraryImport("*", EntryPoint = "_native_get_sp")]
    [SuppressGCTransition]
    public static partial nuint GetSp();

    [LibraryImport("*", EntryPoint = "_native_set_context_switch_new_thread")]
    [SuppressGCTransition]
    public static partial void SetContextSwitchNewThread(int isNew);

    /// <summary>
    /// Captures the calling frame's callee-saved registers + stack pointer into the supplied
    /// <c>REGDISPLAY</c> and returns the caller's return address (IP). Used by the precise GC
    /// stack scan to bootstrap a per-frame walk of the GC-triggering thread (issue #346).
    /// </summary>
    [LibraryImport("*", EntryPoint = "_native_capture_regdisplay")]
    [SuppressGCTransition]
    public static partial nuint CaptureRegDisplay(void* regDisplay);
}
