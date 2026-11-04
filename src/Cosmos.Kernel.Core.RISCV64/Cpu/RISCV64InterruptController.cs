// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Runtime.CompilerServices;
using Cosmos.Kernel.Core.CPU;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.Core.RISCV64.Bridge;
using Cosmos.Kernel.Core.Scheduler;

namespace Cosmos.Kernel.Core.RISCV64.Cpu;

/// <summary>
/// RISC-V 64 interrupt controller. Owns the trap entry (stvec), the PLIC and
/// the S-mode dispatch path. The trap stub reports an exception or an
/// interrupt (IRQContext.interrupt 0 or 1) with scause in cpu_flags and stval
/// in fault_address. Interrupts dispatch through the dense handler table:
/// <list type="bullet">
/// <item><description>a local interrupt with scause code <c>c</c> (software 1, timer 5) at vector <see cref="LocalVectorBase"/> + <c>c</c>;</description></item>
/// <item><description>an external interrupt, claimed from the PLIC as source <c>s</c>, at vector <see cref="ExternalVectorBase"/> + <c>s</c>.</description></item>
/// </list>
/// Exceptions go to the handler at vector 0, as on ARM64. Sources above 31
/// land in the window InterruptManager.AllocateVector hands out for MSIs
/// (0x40 to 0xEE), which RISC-V delivers none of yet: a slot either side
/// holds is refused to the other, the allocator skipping a registered
/// handler and <c>PlicLineRouting</c> a taken vector.
/// </summary>
internal class RISCV64InterruptController : IInterruptController
{
    /// <summary>IRQContext.interrupt as the trap stub writes it for an exception.</summary>
    public const ulong TrapKindException = 0;
    /// <summary>IRQContext.interrupt as the trap stub writes it for an interrupt.</summary>
    public const ulong TrapKindInterrupt = 1;

    /// <summary>scause code of the supervisor software interrupt.</summary>
    public const uint CauseSupervisorSoftware = 1;
    /// <summary>scause code of the supervisor timer interrupt.</summary>
    public const uint CauseSupervisorTimer = 5;
    /// <summary>scause code of the supervisor external interrupt (the PLIC).</summary>
    public const uint CauseSupervisorExternal = 9;

    /// <summary>Vector of local interrupt 0; local interrupt <c>c</c> is at this plus <c>c</c>.</summary>
    public const byte LocalVectorBase = 16;
    /// <summary>Vector of the supervisor software interrupt: the scheduler's self-interrupt.</summary>
    public const byte RescheduleVector = LocalVectorBase + (byte)CauseSupervisorSoftware;
    /// <summary>Vector of the supervisor timer interrupt.</summary>
    public const byte TimerVector = LocalVectorBase + (byte)CauseSupervisorTimer;
    /// <summary>Vector of PLIC source 0; source <c>s</c> is at this plus <c>s</c>.</summary>
    public const byte ExternalVectorBase = 32;
    /// <summary>Highest PLIC source with a vector in the 256-entry handler table.</summary>
    public const uint MaxExternalSource = 255 - ExternalVectorBase;

    /// <summary>sie/sip bit of the supervisor software interrupt.</summary>
    public const ulong InterruptBitSoftware = 1UL << (int)CauseSupervisorSoftware;
    /// <summary>sie/sip bit of the supervisor timer interrupt.</summary>
    public const ulong InterruptBitTimer = 1UL << (int)CauseSupervisorTimer;
    /// <summary>sie/sip bit of the supervisor external interrupt.</summary>
    public const ulong InterruptBitExternal = 1UL << (int)CauseSupervisorExternal;

    /// <summary>scause bit 63: set for an interrupt, clear for an exception.</summary>
    private const ulong CauseInterruptFlag = 1UL << 63;

    /// <summary>Bytes the trap stub pushes below the IRQContext: f0-f31, fcsr and padding.</summary>
    public const int FpSaveAreaBytes = 272;

    private bool _initialized;

    public bool IsInitialized => _initialized;

    public void Initialize()
    {
        Serial.Write("[RISCV64InterruptController] Installing the trap entry...\n");
        TrapNative.InitTrapVector();

        Plic.Initialize();
        Serial.Write("[RISCV64InterruptController] PLIC at 0x");
        Serial.WriteHex(Plic.Base);
        Serial.Write(", ");
        Serial.WriteNumber(Plic.SourceCount - 1);
        Serial.Write(" sources\n");

        // A handler, though it does nothing, is what takes the software
        // interrupt down Dispatch's path to the pending reschedule.
        InterruptManager.SetHandler(RescheduleVector, HandleReschedule);

        // Software and external interrupts on; the timer's bit is the tick
        // source's to set when it starts.
        CsrNative.SetSie(InterruptBitSoftware | InterruptBitExternal);

        Serial.Write("[RISCV64InterruptController] RISC-V interrupt system ready\n");
        _initialized = true;
    }

    /// <inheritdoc/>
    public bool TryRaiseReschedule()
    {
        if (!_initialized)
        {
            return false;
        }

        // S-mode may set its own software interrupt pending bit: the
        // interrupt is taken as soon as interrupts are enabled.
        CsrNative.SetSip(InterruptBitSoftware);
        return true;
    }

    /// <summary>
    /// Routes PLIC source <paramref name="irqNo"/>: a priority above the
    /// threshold, and the enable bit unless <paramref name="startMasked"/>.
    /// Its handler is the one at <see cref="ExternalVectorBase"/> plus the source.
    /// </summary>
    /// <param name="irqNo">The PLIC source.</param>
    /// <param name="vector">The vector the caller registered the handler at.</param>
    /// <param name="startMasked">Whether the source stays masked until unmasked.</param>
    public void RouteIrq(byte irqNo, byte vector, bool startMasked)
    {
        Plic.SetPriority(irqNo, Plic.DefaultPriority);
        if (!startMasked)
        {
            Plic.Enable(irqNo);
        }

        Serial.Write("[RISCV64InterruptController] Routed PLIC source ");
        Serial.WriteNumber(irqNo);
        Serial.Write(" -> vector ");
        Serial.WriteNumber(vector);
        Serial.Write("\n");
    }

    /// <summary>Masks PLIC source <paramref name="irqNo"/>. Allocation-free; any context.</summary>
    /// <param name="irqNo">The PLIC source.</param>
    public void MaskIrq(byte irqNo) => Plic.Disable(irqNo);

    /// <summary>Unmasks PLIC source <paramref name="irqNo"/>. Allocation-free; any context.</summary>
    /// <param name="irqNo">The PLIC source.</param>
    public void UnmaskIrq(byte irqNo) => Plic.Enable(irqNo);

    public void Dispatch(ref IRQContext ctx)
    {
        if (ctx.interrupt == TrapKindInterrupt)
        {
            uint cause = (uint)(ctx.cpu_flags & ~CauseInterruptFlag);
            InterruptManager.IrqDelegate?[]? handlers = InterruptManager.s_irqHandlers;

            if (cause == CauseSupervisorExternal)
            {
                uint source = Plic.Claim();
                if (source == 0)
                {
                    // Claimed by nobody: another context took it, or spurious.
                    return;
                }

                InterruptManager.IrqDelegate? handler = null;
                uint vector = ExternalVectorBase + source;
                if (handlers != null && source <= MaxExternalSource && vector < (uint)handlers.Length)
                {
                    handler = handlers[(int)vector];
                }

                if (handler != null)
                {
                    ctx.interrupt = vector;
                    handler(ref ctx);
                }

                Plic.Complete(source);

                if (handler != null)
                {
                    RunPendingReschedule(ref ctx);
                }

                return;
            }

            if (cause == CauseSupervisorSoftware)
            {
                CsrNative.ClearSip(InterruptBitSoftware);
            }

            uint localVector = LocalVectorBase + cause;
            InterruptManager.IrqDelegate? localHandler = null;
            if (handlers != null && localVector < (uint)handlers.Length)
            {
                localHandler = handlers[(int)localVector];
            }

            if (localHandler == null)
            {
                if (cause == CauseSupervisorTimer)
                {
                    // Nobody owns the timer: mask it, or its pending bit traps forever.
                    CsrNative.ClearSie(InterruptBitTimer);
                }

                return;
            }

            ctx.interrupt = localVector;
            localHandler(ref ctx);
            RunPendingReschedule(ref ctx);
            return;
        }

        // Synchronous exception
        InterruptManager.IrqDelegate?[]? exceptionHandlers = InterruptManager.s_irqHandlers;
        if (exceptionHandlers != null)
        {
            InterruptManager.IrqDelegate? handler = exceptionHandlers[0];
            if (handler != null)
            {
                handler(ref ctx);
                return;
            }
        }

        HandleFatalException(ref ctx);
    }

    /// <summary>
    /// The stack pointer of the context the trap stub saved: the start of the
    /// floating-point save area below <paramref name="ctx"/>, which is what the
    /// scheduler switches from. Interrupt context.
    /// </summary>
    /// <param name="ctx">The trap's context, as the stub passed it.</param>
    public static unsafe nuint SavedContextStackPointer(ref IRQContext ctx)
    {
        return (nuint)Unsafe.AsPointer(ref ctx) - FpSaveAreaBytes;
    }

    /// <summary>The name of an exception scause code (RISC-V privileged specification, table 3.6).</summary>
    /// <param name="cause">The exception code.</param>
    public static string ExceptionName(ulong cause)
    {
        return cause switch
        {
            0 => "Instruction address misaligned",
            1 => "Instruction access fault",
            2 => "Illegal instruction",
            3 => "Breakpoint",
            4 => "Load address misaligned",
            5 => "Load access fault",
            6 => "Store/AMO address misaligned",
            7 => "Store/AMO access fault",
            8 => "Environment call from U-mode",
            9 => "Environment call from S-mode",
            12 => "Instruction page fault",
            13 => "Load page fault",
            15 => "Store/AMO page fault",
            18 => "Software check",
            19 => "Hardware error",
            _ => "Reserved exception",
        };
    }

    /// <summary>The scheduler's self-interrupt: nothing to do, Dispatch runs the reschedule on its exit.</summary>
    private static void HandleReschedule(ref IRQContext ctx)
    {
    }

    private static void RunPendingReschedule(ref IRQContext ctx)
    {
        SchedulerManager.ReschedulePendingFromIrq(0, SavedContextStackPointer(ref ctx));
    }

    private static void HandleFatalException(ref IRQContext ctx)
    {
        Serial.Write("[INT] FATAL: ");
        Serial.Write(ExceptionName(ctx.cpu_flags));
        Serial.Write("\n[INT] scause: 0x");
        Serial.WriteHex(ctx.cpu_flags);
        Serial.Write("  stval: 0x");
        Serial.WriteHex(ctx.fault_address);
        Serial.Write("\n[INT] System halted.\n");
        while (true)
        {
        }
    }
}
