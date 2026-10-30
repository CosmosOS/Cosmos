// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cosmos.Kernel.Core.Bridge;
using Cosmos.Kernel.Core.Runtime.ExceptionHandling;

namespace Cosmos.Kernel.Core.CPU;

/// <summary>
/// The per-CPU block: what a CPU finds about itself in one load through the register the
/// architecture keeps for it, <c>IA32_GS_BASE</c> on x64 and <c>TPIDR_EL1</c> on ARM64. Core owns
/// the table of blocks (a static, so every block has a fixed address the register can hold) and
/// this struct is the layout's home: the native stubs reach the slots by offset
/// (<c>CPU/CpuOps.s</c>, <c>CPU/Interrupts.s</c>, <c>CPU/ExceptionHandling.s</c> of each native
/// package mirror the offsets they use). The boot CPU installs block zero in Core's library
/// initializer, the first managed code the boot runs; an application processor's is prepared
/// by the boot CPU before it is released and loaded into the register by the trampoline
/// (<see cref="Smp"/>). The interrupt stubs stage the context switch in the running
/// CPU's block, the exception stubs find the running thread's ExInfo chain head through it, and
/// <see cref="Scheduler.SchedulerManager.GetCurrentCpuId"/> reads its index from it.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct PerCpuBlock
{
    /// <summary>Size of one block in bytes; the table holds <see cref="MaxCpus"/> of them.</summary>
    public const int Size = 128;

    /// <summary>Blocks in the table: the MADT parser's bound, and more than Limine's list ever reports.</summary>
    public const int MaxCpus = 256;

    /// <summary>The block's own address: what the per-CPU register holds.</summary>
    public nuint Self;

    /// <summary>The index of this CPU: 0 for the boot CPU, Limine's order for the others.</summary>
    public ulong CpuIndex;

    /// <summary>
    /// The interrupt controller's identity of this CPU, written by the controller's bring-up on
    /// this CPU: the Local APIC id on x64, the MPIDR_EL1 affinity (<c>Aff3:Aff2:Aff1:Aff0</c>)
    /// on ARM64. What a message or an IPI aimed at this CPU is addressed to.
    /// </summary>
    public ulong HardwareId;

    /// <summary>Context switch staging: the saved context the interrupt exit switches to, 0 for none.</summary>
    public nuint SwitchTargetSp;

    /// <summary>Context switch staging: 1 when that context has never run (the stub loads its stack and jumps).</summary>
    public ulong SwitchIsNewThread;

    /// <summary>The stub's copy of <see cref="SwitchIsNewThread"/> across its register restore.</summary>
    public ulong TempIsNewThread;

    /// <summary>x64: the error code of the last error-code exception taken on this CPU.</summary>
    public ulong LastErrorCode;

    /// <summary>
    /// The address of the running thread's ExInfo chain head slot
    /// (<see cref="ExceptionDispatchState.ExInfoHead"/>), what <c>RhpThrowEx</c>,
    /// <c>RhpCallCatchFunclet</c> and <c>RhpRethrow</c> read; the scheduler's switch path points it
    /// at the next thread's, and it points at the boot code's
    /// (<see cref="ExceptionHelper.BootExInfoHeadSlot"/>) before a thread is current.
    /// </summary>
    public nuint CurrentExInfoHead;

    /// <summary>Top (exclusive) of the bootloader stack this CPU booted on, recorded by <c>kmain</c>.</summary>
    public nuint BootStackTop;

    /// <summary>1 once this CPU runs its scheduler tick: the boot CPU from its block's installation, an application processor once <see cref="Smp.ApMain"/> started its tick.</summary>
    public ulong Online;

    /// <summary>
    /// 1 while this CPU sits in its stop handler, held by another CPU's rendezvous
    /// (<see cref="CpuStop"/>): the policy swap's, and the collector's from SMP phase 4.
    /// </summary>
    public ulong Stopped;

    /// <summary>
    /// The RD_base of this CPU's GICv3 redistributor, found by the GICR_TYPER walk run on it;
    /// 0 before the walk, and on x64. The SGI and PPI registers are banked per redistributor,
    /// so the GICv3 code reaches the calling CPU's through its block.
    /// </summary>
    public ulong GicRedistributorBase;

    private ulong _reserved0;
    private ulong _reserved1;
    private ulong _reserved2;
    private ulong _reserved3;

    /// <summary>
    /// The table: a static, so it is zeroed data at a fixed address outside the garbage
    /// collector's heap, which is what a register that points into it needs.
    /// </summary>
    private static PerCpuTable s_table;

    /// <summary>The block of the CPU running this code.</summary>
    public static unsafe ref PerCpuBlock Current => ref *(PerCpuBlock*)CpuNative.GetCurrentBlock();

    /// <summary>
    /// The block of CPU <paramref name="index"/>, installed or not: every slot of the table
    /// exists, <see cref="Online"/> tells whether a CPU runs on it.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is <see cref="MaxCpus"/> or more.</exception>
    public static ref PerCpuBlock At(uint index)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, (uint)MaxCpus);
        return ref s_table[(int)index];
    }

    /// <summary>
    /// The index of the CPU running this code: one load through the per-CPU register, cheap
    /// enough for the paths that ask on every thread-static access and every allocation.
    /// </summary>
    public static uint CurrentIndex => (uint)CpuNative.ReadCpuIndex();

    /// <summary>
    /// Makes block zero the boot CPU's and loads the per-CPU register with it: the first thing
    /// Core's library initializer does, so that everything after it, the heap, the collector,
    /// the platform's interrupt setup, runs with the register in place. The block records the
    /// boot stack top <c>kmain</c> captured and points the exception stubs at the boot code's
    /// chain head; <see cref="Scheduler.SchedulerManager.SetupIdleThread"/> points them at the
    /// idle thread's once that thread is current.
    /// </summary>
    internal static unsafe void InstallBootCpu()
    {
        ref PerCpuBlock block = ref At(0);
        block.Self = (nuint)Unsafe.AsPointer(ref block);
        block.CpuIndex = 0;
        block.BootStackTop = BootNative.GetBootStackTop();
        block.CurrentExInfoHead = ExceptionHelper.BootExInfoHeadSlot;
        block.Online = 1;
        CpuNative.SetCurrentBlock(block.Self);
    }
}

/// <summary>The <see cref="PerCpuBlock.MaxCpus"/> blocks, inline so the table is one static.</summary>
[InlineArray(PerCpuBlock.MaxCpus)]
internal struct PerCpuTable
{
    private PerCpuBlock _element0;
}
