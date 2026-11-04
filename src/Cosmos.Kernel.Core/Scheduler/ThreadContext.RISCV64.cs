using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Core.Scheduler;

/// <summary>
/// RISC-V 64 thread context for saving/restoring thread state during context switches.
/// This structure matches the frame the trap entry pushes in Interrupts.s:
/// - f0-f31 and fcsr (272 bytes with padding) at offset 0
/// - IRQContext (x0-x31 + trap CSRs) at offset 272
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal unsafe struct ThreadContext
{
    // Floating-point registers f0-f31 (32 * 8 = 256 bytes)
    public fixed byte Fp[256];

    // Floating-point control and status register
    public ulong Fcsr;

    // Keeps the IRQContext at a 16-byte boundary
    public ulong Padding;

    // General purpose registers x0-x31 (32 * 8 = 256 bytes); x0 is always 0
    public ulong X0, X1, X2, X3, X4, X5, X6, X7;
    public ulong X8, X9, X10, X11, X12, X13, X14, X15;
    public ulong X16, X17, X18, X19, X20, X21, X22, X23;
    public ulong X24, X25, X26, X27, X28, X29, X30, X31;

    // Where sret resumes (for a new thread, its entry point)
    public ulong Sepc;

    // sstatus the trap exit restores before sret
    public ulong Sstatus;

    // Trap info (for IRQContext compatibility)
    public ulong Interrupt;
    public ulong CpuFlags;      // scause
    public ulong FaultAddress;  // stval

    /// <summary>
    /// Size of the complete context in bytes: the 576-byte frame the trap
    /// entry pushes (FP area 272 + IRQContext 296, rounded to 16 bytes).
    /// </summary>
    public const int Size = 576;

    /// <summary>
    /// Offset of IRQContext within the full context (after the FP area).
    /// </summary>
    public const int IrqContextOffset = 272;

    /// <summary>sstatus.SPP: sret returns to S-mode.</summary>
    private const ulong SstatusSpp = 1UL << 8;
    /// <summary>sstatus.SPIE: sret enables interrupts (SIE = SPIE).</summary>
    private const ulong SstatusSpie = 1UL << 5;
    /// <summary>sstatus.FS = Initial: the floating-point unit is on.</summary>
    private const ulong SstatusFsInitial = 1UL << 13;
    /// <summary>sstatus.UXL = 2: U-mode XLEN 64, the value RV64 hardware holds there.</summary>
    private const ulong SstatusUxl64 = 2UL << 32;

    /// <summary>
    /// Sets up initial context for a new thread. The trap exit restores it
    /// like any saved context and sret enters the thread in S-mode, with
    /// interrupts enabled, at <paramref name="entryPoint"/>.
    /// </summary>
    /// <param name="entryPoint">Thread entry point address.</param>
    /// <param name="codeSegment">Unused on RISC-V, kept for API compatibility.</param>
    /// <param name="arg">Optional argument passed in a0.</param>
    /// <param name="stackTop">Top of the usable stack for sp.</param>
    public void Initialize(nuint entryPoint, ushort codeSegment, nuint arg = 0, nuint stackTop = 0)
    {
        fixed (byte* fp = Fp)
        {
            for (int i = 0; i < 256; i++)
            {
                fp[i] = 0;
            }
        }

        Fcsr = 0;
        Padding = 0;

        X0 = X1 = X2 = X3 = X4 = X5 = X6 = X7 = 0;
        X8 = X9 = X10 = X11 = X12 = X13 = X14 = X15 = 0;
        X16 = X17 = X18 = X19 = X20 = X21 = X22 = X23 = 0;
        X24 = X25 = X26 = X27 = X28 = X29 = X30 = X31 = 0;

        // a0: the first argument
        X10 = arg;

        // sp: 16-byte aligned (LP64D)
        X2 = stackTop & ~(nuint)0xF;

        Sepc = entryPoint;
        Sstatus = SstatusSpp | SstatusSpie | SstatusFsInitial | SstatusUxl64;

        Interrupt = 0;
        CpuFlags = 0;
        FaultAddress = 0;
    }
}
