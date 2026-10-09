// This code is licensed under MIT license (see LICENSE for details)

using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Core.SysCalls;

#if ARCH_ARM64

/// <summary>
/// Blittable register frame passed from the ARM64 SVC trap stub into the
/// managed syscall dispatcher. Mirrors the layout the native stub pushes on
/// entry (see src/Cosmos.Kernel.Native.ARM64/CPU/SysCalls.s); the only fields
/// the dispatcher reads are <see cref="Number"/> and <see cref="Arg0"/>..Arg5.
/// The remaining fields are captured for tracing / caller validation and
/// never interpreted by arch-neutral code.
/// </summary>
/// <remarks>
/// Natural (8-byte) alignment is required so the SVC stub can build the frame
/// with aligned <c>str x</c>/<c>stp</c> stores — a 4-byte <see cref="Number"/>
/// followed by an 8-byte <c>ulong</c> would otherwise land <see cref="Arg0"/>
/// at byte 4 and fault on AArch64 strict-alignment load/store.
/// </remarks>
[StructLayout(LayoutKind.Sequential)]
internal unsafe struct SysCallContext
{
    /// <summary>Syscall number (from x8 on AArch64). 4 bytes followed by 4
    /// bytes of alignment padding before <see cref="Arg0"/>.</summary>
    public uint Number;

    /// <summary>Saved general-purpose argument registers (x0..x5).</summary>
    public ulong Arg0;
    public ulong Arg1;
    public ulong Arg2;
    public ulong Arg3;
    public ulong Arg4;
    public ulong Arg5;

    /// <summary>Calling user stack pointer (sp on entry).</summary>
    public ulong Sp;

    /// <summary>Exception link register — user return address (elr_el1).</summary>
    public ulong Elr;

    /// <summary>Saved program status register (spsr_el1).</summary>
    public ulong Spsr;

    /// <summary>Calling thread pointer, populated by the stub from the per-CPU
    /// scheduler state — never trusted from user-supplied registers.</summary>
    public void* Thread;
}

#else

/// <summary>
/// Blittable register frame passed from the x64 SYSCALL trap stub into the
/// managed syscall dispatcher. Mirrors the layout the native stub pushes on
/// entry (see src/Cosmos.Kernel.Native.X64/CPU/SysCalls.s); the only fields
/// the dispatcher reads are <see cref="Number"/> and <see cref="Arg0"/>..Arg5.
/// The remaining fields are captured for tracing / caller validation and
/// never interpreted by arch-neutral code.
/// </summary>
/// <remarks>
/// Natural (8-byte) alignment is required so the SYSCALL stub can build the
/// frame with aligned <c>mov [rsp+n], r64</c> stores — a 4-byte
/// <see cref="Number"/> followed by an 8-byte <c>ulong</c> would otherwise
/// land <see cref="Arg0"/> at byte 4 as an unaligned 8-byte slot.
/// </remarks>
[StructLayout(LayoutKind.Sequential)]
internal unsafe struct SysCallContext
{
    /// <summary>Syscall number (from rax on x86-64). 4 bytes followed by 4
    /// bytes of alignment padding before <see cref="Arg0"/>.</summary>
    public uint Number;

    /// <summary>Saved general-purpose argument registers (rdi, rsi, rdx, r10, r8, r9).</summary>
    public ulong Arg0;
    public ulong Arg1;
    public ulong Arg2;
    public ulong Arg3;
    public ulong Arg4;
    public ulong Arg5;

    /// <summary>Calling user stack pointer (rsp on entry, before the kernel transition).</summary>
    public ulong Rsp;

    /// <summary>Exception return address (rcx on SYSCALL; user RIP).</summary>
    public ulong Rip;

    /// <summary>Saved RFLAGS (r11 on SYSCALL).</summary>
    public ulong Rflags;

    /// <summary>Calling thread pointer, populated by the stub from the per-CPU
    /// scheduler state — never trusted from user-supplied registers.</summary>
    public void* Thread;
}

#endif
