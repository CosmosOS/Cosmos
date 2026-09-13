using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Core.Scheduler;

/// <summary>
/// ARM64 thread context for saving/restoring thread state during context switches.
/// This structure matches the layout saved by the interrupt handler in Interrupts.s:
/// - NEON registers Q0-Q31 (512 bytes) at offset 0
/// - IRQContext (GPRs + system regs) at offset 512
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal unsafe struct ThreadContext
{
    // NEON/SIMD registers Q0-Q31 (32 * 16 = 512 bytes)
    public fixed byte Neon[512];

    // General purpose registers X0-X30 (31 * 8 = 248 bytes)
    public ulong X0, X1, X2, X3, X4, X5, X6, X7;
    public ulong X8, X9, X10, X11, X12, X13, X14, X15;
    public ulong X16, X17, X18, X19, X20, X21, X22, X23;
    public ulong X24, X25, X26, X27, X28, X29, X30;

    // Stack pointer (for new threads, this is the thread's stack)
    public ulong Sp;

    // Exception link register (entry point / return address)
    public ulong Elr;

    // Saved program status register
    public ulong Spsr;

    // Exception info (for IRQContext compatibility)
    public ulong Interrupt;
    public ulong CpuFlags;  // ESR_EL1
    public ulong Far;       // FAR_EL1

    /// <summary>
    /// Size of the complete context in bytes.
    /// NEON (512) + GPRs (248) + SP (8) + ELR (8) + SPSR (8) + interrupt (8) + cpu_flags (8) + far (8) = 808 bytes
    /// Rounded to 816 for alignment (matches interrupt handler)
    /// </summary>
    public const int Size = 816;

    /// <summary>
    /// Offset of IRQContext within the full context (after NEON registers).
    /// </summary>
    public const int IrqContextOffset = 512;

    /// <summary>
    /// Sets up initial context for a new thread.
    /// </summary>
    /// <param name="entryPoint">Thread entry point address.</param>
    /// <param name="codeSegment">Unused on ARM64, kept for API compatibility.</param>
    /// <param name="arg">Optional argument passed in X0.</param>
    /// <param name="stackTop">Top of the usable stack for SP.</param>
    public void Initialize(nuint entryPoint, ushort codeSegment, nuint arg = 0, nuint stackTop = 0)
    {
        // Clear NEON registers
        fixed (byte* neon = Neon)
        {
            for (int i = 0; i < 512; i++)
            {
                neon[i] = 0;
            }
        }

        // Clear all general purpose registers
        X0 = arg;  // First argument in ARM64 calling convention
        X1 = X2 = X3 = X4 = X5 = X6 = X7 = 0;
        X8 = X9 = X10 = X11 = X12 = X13 = X14 = X15 = 0;
        X16 = X17 = X18 = X19 = X20 = X21 = X22 = X23 = 0;
        X24 = X25 = X26 = X27 = X28 = X29 = X30 = 0;

        // Set up stack pointer (16-byte aligned on ARM64)
        Sp = stackTop & ~(nuint)0xF;

        // Set up entry point
        Elr = entryPoint;

        // SPSR: EL1h mode with interrupts enabled
        // Bits: D=0 (debug), A=0 (SError), I=0 (IRQ), F=0 (FIQ), M[4:0]=0b00101 (EL1h)
        Spsr = 0x3C5;

        // Clear exception info
        Interrupt = 0;
        CpuFlags = 0;
        Far = 0;
    }
}
