using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Core.Scheduler;

/// <summary>
/// Complete thread context saved on stack during interrupt.
/// This represents the full stack layout after IRQ stub saves all registers.
/// RSP points to the start of this structure after save.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal unsafe struct ThreadContext
{
    // XMM registers (256 bytes) - SSE/SIMD state
    public fixed byte Xmm[256];

    // General purpose registers (pushed in reverse order)
    public ulong R15;
    public ulong R14;
    public ulong R13;
    public ulong R12;
    public ulong R11;
    public ulong R10;
    public ulong R9;
    public ulong R8;
    public ulong Rdi;
    public ulong Rsi;
    public ulong Rbp;
    public ulong Rbx;
    public ulong Rdx;
    public ulong Rcx;
    public ulong Rax;

    // Interrupt info
    public ulong Interrupt;
    public ulong CpuFlags;
    public ulong Cr2;

    // Temp storage (skipped during restore with add rsp, 32)
    public ulong TempRcx;

    // CPU interrupt frame / Thread entry point setup
    // For resumed threads: RIP, CS, RFLAGS come from iretq
    // For new threads: RIP = entry point, RFLAGS = initial flags, RSP = thread stack
    public ulong Rip;     // Return address / entry point
    public ulong Cs;      // Code segment
    public ulong Rflags;  // Flags register
    public ulong Rsp;     // Stack pointer for new threads (loaded before jump)
    public ulong Ss;      // Unused (kept for alignment)

    /// <summary>
    /// Size of the complete context in bytes.
    /// </summary>
    public const int Size = 256 + (15 * 8) + (3 * 8) + 8 + (5 * 8);  // XMM + GPRs + info + temp + full CPU frame

    /// <summary>
    /// Sets up initial context for a new thread.
    /// </summary>
    /// <param name="entryPoint">Thread entry point address.</param>
    /// <param name="codeSegment">Code segment selector.</param>
    /// <param name="arg">Optional argument passed in RDI.</param>
    /// <param name="stackTop">Top of the usable stack (for RSP after iretq).</param>
    public void Initialize(nuint entryPoint, ushort codeSegment, nuint arg = 0, nuint stackTop = 0)
    {
        // Clear everything
        R15 = R14 = R13 = R12 = R11 = R10 = R9 = R8 = 0;
        Rdi = arg;  // First argument in x64 calling convention
        Rsi = Rbx = Rdx = Rcx = Rax = 0;
        Interrupt = 0;
        CpuFlags = 0;
        Cr2 = 0;
        TempRcx = 0;

        // Set up entry point and flags
        Rip = entryPoint;
        Cs = codeSegment;
        Rflags = 0x202;  // IF=1 (interrupts enabled), bit 1 always set

        // Set up stack for the new thread
        // RSP should be 16-byte aligned, then 8 off for call convention
        Rsp = (stackTop & ~(nuint)0xF) - 8;  // Align and subtract 8 for call ABI
        Ss = 0;  // Unused

        // Set RBP to 0 (clean frame pointer for new thread)
        Rbp = 0;

        // XMM registers are zeroed by default (uninitialized)
        fixed (byte* xmm = Xmm)
        {
            for (int i = 0; i < 256; i++)
            {
                xmm[i] = 0;
            }
        }
    }
}
