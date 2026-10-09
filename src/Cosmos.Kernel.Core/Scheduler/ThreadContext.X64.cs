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
    /// <param name="entryPoint">Thread entry point function address.</param>
    /// <param name="codeSegment">
    /// Ignored when <paramref name="ring"/> is 3. Kept on the ring-0 path for
    /// legacy callers that pass the running CS; new code should pass 0 and
    /// let the ring select the selector.
    /// </param>
    /// <param name="arg">Optional argument passed in RDI.</param>
    /// <param name="stackTop">Top of the usable stack (for RSP after iretq).</param>
    /// <param name="ring">
    /// Privilege level: 0 = kernel (ring 0), 3 = user (ring 3). Ring-3 builds
    /// a CPU frame the IRQ-stub exit path drops CPL with via iretq: CS=0x1B,
    /// SS=0x23, RSP=<paramref name="stackTop"/>. Ring-0 keeps CS=0x08 and the
    /// manual RSP+jmp exit path.
    /// </param>
    public void Initialize(nuint entryPoint, ushort codeSegment, nuint arg = 0, nuint stackTop = 0, byte ring = 0)
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
        Rflags = 0x202;  // IF=1 (interrupts enabled), bit 1 always set

        // Set up stack for the new thread
        // RSP should be 16-byte aligned, then 8 off for call convention
        Rsp = (stackTop & ~(nuint)0xF) - 8;  // Align and subtract 8 for call ABI
        Ss = 0;  // Unused

        // Ring selection: ring-3 selectors must match the custom GDT installed
        // by CPU/Gdt.s (0x1B user code, 0x23 user data). The IRQ stub exit
        // path discriminates on CS RPL (see Interrupts.s .Lnew_thread_iretq):
        // a ring-3 frame (CS.RPL=3, RSP/SS present) is returned from via
        // iretq, dropping CPL; a ring-0 frame uses the manual RSP+jmp path.
        if (ring == 3)
        {
            Cs = 0x1B;  // ring-3 64-bit code selector (RPL=3)
            Ss = 0x23;  // ring-3 64-bit data selector
        }
        else
        {
            Cs = codeSegment == 0 ? (ushort)0x08 : codeSegment;
            // Ss stays 0 - the ring-0 exit path pops RIP/CS/RFLAGS only and
            // loads RSP separately, so SS is never consumed.
        }

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
