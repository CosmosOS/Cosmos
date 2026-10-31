// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Core.CPU;

#if ARCH_ARM64

/// <summary>
/// Register state saved by the ARM64 exception entry stub, as handed to
/// interrupt and exception handlers
/// (<see cref="IInterruptController.Dispatch"/>). The layout mirrors the
/// stub's save order exactly; changing it requires changing the assembly.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct IRQContext
{
    /// <summary>Saved general-purpose register x0.</summary>
    public ulong x0;
    /// <summary>Saved general-purpose register x1.</summary>
    public ulong x1;
    /// <summary>Saved general-purpose register x2.</summary>
    public ulong x2;
    /// <summary>Saved general-purpose register x3.</summary>
    public ulong x3;
    /// <summary>Saved general-purpose register x4.</summary>
    public ulong x4;
    /// <summary>Saved general-purpose register x5.</summary>
    public ulong x5;
    /// <summary>Saved general-purpose register x6.</summary>
    public ulong x6;
    /// <summary>Saved general-purpose register x7.</summary>
    public ulong x7;
    /// <summary>Saved general-purpose register x8.</summary>
    public ulong x8;
    /// <summary>Saved general-purpose register x9.</summary>
    public ulong x9;
    /// <summary>Saved general-purpose register x10.</summary>
    public ulong x10;
    /// <summary>Saved general-purpose register x11.</summary>
    public ulong x11;
    /// <summary>Saved general-purpose register x12.</summary>
    public ulong x12;
    /// <summary>Saved general-purpose register x13.</summary>
    public ulong x13;
    /// <summary>Saved general-purpose register x14.</summary>
    public ulong x14;
    /// <summary>Saved general-purpose register x15.</summary>
    public ulong x15;
    /// <summary>Saved general-purpose register x16.</summary>
    public ulong x16;
    /// <summary>Saved general-purpose register x17.</summary>
    public ulong x17;
    /// <summary>Saved general-purpose register x18.</summary>
    public ulong x18;
    /// <summary>Saved general-purpose register x19.</summary>
    public ulong x19;
    /// <summary>Saved general-purpose register x20.</summary>
    public ulong x20;
    /// <summary>Saved general-purpose register x21.</summary>
    public ulong x21;
    /// <summary>Saved general-purpose register x22.</summary>
    public ulong x22;
    /// <summary>Saved general-purpose register x23.</summary>
    public ulong x23;
    /// <summary>Saved general-purpose register x24.</summary>
    public ulong x24;
    /// <summary>Saved general-purpose register x25.</summary>
    public ulong x25;
    /// <summary>Saved general-purpose register x26.</summary>
    public ulong x26;
    /// <summary>Saved general-purpose register x27.</summary>
    public ulong x27;
    /// <summary>Saved general-purpose register x28.</summary>
    public ulong x28;
    /// <summary>Saved frame pointer (x29).</summary>
    public ulong x29;
    /// <summary>Saved link register (x30).</summary>
    public ulong x30;

    /// <summary>Interrupted stack pointer.</summary>
    public ulong sp;
    /// <summary>Exception link register: the return address (ELR_EL1).</summary>
    public ulong elr;
    /// <summary>Saved program status register (SPSR_EL1).</summary>
    public ulong spsr;

    /// <summary>Exception type: 0 sync, 1 IRQ, 2 FIQ, 3 SError.</summary>
    public ulong interrupt;
    /// <summary>Exception syndrome (ESR_EL1).</summary>
    public ulong cpu_flags;
    /// <summary>Fault address for data/instruction aborts (FAR_EL1).</summary>
    public ulong fault_address;
}

#elif ARCH_RISCV64

/// <summary>
/// Register state saved by the RISC-V trap entry stub, as handed to interrupt
/// and exception handlers (<see cref="IInterruptController.Dispatch"/>). The
/// layout mirrors the stub's save order exactly (register xN at offset 8 * N);
/// changing it requires changing the assembly.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct IRQContext
{
    /// <summary>x0: always zero, keeps xN at offset 8 * N.</summary>
    public ulong x0;
    /// <summary>x1: return address.</summary>
    public ulong ra;
    /// <summary>x2: the interrupted code's stack pointer.</summary>
    public ulong sp;
    /// <summary>x3: global pointer.</summary>
    public ulong gp;
    /// <summary>x4: thread pointer.</summary>
    public ulong tp;
    /// <summary>x5: temporary.</summary>
    public ulong t0;
    /// <summary>x6: temporary.</summary>
    public ulong t1;
    /// <summary>x7: temporary.</summary>
    public ulong t2;
    /// <summary>x8: saved register 0, the frame pointer.</summary>
    public ulong s0;
    /// <summary>x9: saved register 1.</summary>
    public ulong s1;
    /// <summary>x10: argument 0.</summary>
    public ulong a0;
    /// <summary>x11: argument 1.</summary>
    public ulong a1;
    /// <summary>x12: argument 2.</summary>
    public ulong a2;
    /// <summary>x13: argument 3.</summary>
    public ulong a3;
    /// <summary>x14: argument 4.</summary>
    public ulong a4;
    /// <summary>x15: argument 5.</summary>
    public ulong a5;
    /// <summary>x16: argument 6.</summary>
    public ulong a6;
    /// <summary>x17: argument 7.</summary>
    public ulong a7;
    /// <summary>x18: saved register 2.</summary>
    public ulong s2;
    /// <summary>x19: saved register 3.</summary>
    public ulong s3;
    /// <summary>x20: saved register 4.</summary>
    public ulong s4;
    /// <summary>x21: saved register 5.</summary>
    public ulong s5;
    /// <summary>x22: saved register 6.</summary>
    public ulong s6;
    /// <summary>x23: saved register 7.</summary>
    public ulong s7;
    /// <summary>x24: saved register 8.</summary>
    public ulong s8;
    /// <summary>x25: saved register 9.</summary>
    public ulong s9;
    /// <summary>x26: saved register 10.</summary>
    public ulong s10;
    /// <summary>x27: saved register 11.</summary>
    public ulong s11;
    /// <summary>x28: temporary.</summary>
    public ulong t3;
    /// <summary>x29: temporary.</summary>
    public ulong t4;
    /// <summary>x30: temporary.</summary>
    public ulong t5;
    /// <summary>x31: temporary.</summary>
    public ulong t6;
    /// <summary>sepc: where the trap was taken, where sret returns.</summary>
    public ulong sepc;
    /// <summary>sstatus at the trap: SPP, SPIE and FS carry the interrupted code's state.</summary>
    public ulong sstatus;
    /// <summary>0 for an exception, 1 for an interrupt, as the stub writes it; the dispatcher replaces it with the vector before calling a handler.</summary>
    public ulong interrupt;
    /// <summary>scause: the trap's cause, bit 63 set for an interrupt.</summary>
    public ulong cpu_flags;
    /// <summary>stval: the faulting address or instruction, when the cause has one.</summary>
    public ulong fault_address;
}
#else

/// <summary>
/// Register state saved by the x64 interrupt stub, as handed to interrupt
/// and exception handlers (<see cref="IInterruptController.Dispatch"/>).
/// The layout mirrors the stub's push order exactly; changing it requires
/// changing the assembly.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct IRQContext
{
    /// <summary>Saved general-purpose register r15.</summary>
    public ulong r15;
    /// <summary>Saved general-purpose register r14.</summary>
    public ulong r14;
    /// <summary>Saved general-purpose register r13.</summary>
    public ulong r13;
    /// <summary>Saved general-purpose register r12.</summary>
    public ulong r12;
    /// <summary>Saved general-purpose register r11.</summary>
    public ulong r11;
    /// <summary>Saved general-purpose register r10.</summary>
    public ulong r10;
    /// <summary>Saved general-purpose register r9.</summary>
    public ulong r9;
    /// <summary>Saved general-purpose register r8.</summary>
    public ulong r8;
    /// <summary>Saved general-purpose register rdi.</summary>
    public ulong rdi;
    /// <summary>Saved general-purpose register rsi.</summary>
    public ulong rsi;
    /// <summary>Saved frame pointer (rbp).</summary>
    public ulong rbp;
    /// <summary>Saved general-purpose register rbx.</summary>
    public ulong rbx;
    /// <summary>Saved general-purpose register rdx.</summary>
    public ulong rdx;
    /// <summary>Saved general-purpose register rcx.</summary>
    public ulong rcx;
    /// <summary>Saved general-purpose register rax.</summary>
    public ulong rax;
    /// <summary>Interrupt vector number.</summary>
    public ulong interrupt;
    /// <summary>RFLAGS, copied from the hardware interrupt frame by the stub.</summary>
    public ulong cpu_flags;
    /// <summary>CR2: page-fault linear address (valid for #PF, int 14).</summary>
    public ulong fault_address;

    // The interrupt stub's frame continues past the info block (see ThreadContext.X64 /
    // Interrupts.s): the TempRcx scratch slot followed by the hardware interrupt frame.
    // Exposing them here lets exception handlers report the faulting RIP.

    /// <summary>Context-switch scratch slot (zero otherwise).</summary>
    public ulong temp_rcx;
    /// <summary>Faulting/interrupted instruction pointer.</summary>
    public ulong rip;
    /// <summary>Code segment selector.</summary>
    public ulong cs;
    /// <summary>Saved RFLAGS.</summary>
    public ulong rflags;
    /// <summary>Interrupted stack pointer (always pushed in 64-bit mode).</summary>
    public ulong rsp;
    /// <summary>Stack segment selector.</summary>
    public ulong ss;
}

#endif
