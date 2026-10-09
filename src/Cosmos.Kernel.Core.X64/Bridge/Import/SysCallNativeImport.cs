using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Core.X64.Bridge;

internal static unsafe partial class SysCallNativeImport
{
    /// <summary>
    /// One-time wiring of the x64 SYSCALL fast path: programs the
    /// STAR/LSTAR/FMASK MSRs, the KERNEL_GS_BASE per-CPU block, and the
    /// dedicated syscall kernel stack. Implemented in
    /// src/Cosmos.Kernel.Native.X64/CPU/SysCalls.s. No-op on ARM64 (the SVC
    /// vector slot routes SVC exceptions directly; no MSR init needed).
    /// </summary>
    [LibraryImport("*", EntryPoint = "_native_x64_init_syscall")]
    [SuppressGCTransition]
    public static partial void InitSysCall();
}