// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Runtime.CompilerServices;
using Cosmos.Kernel.Core.ARM64.Bridge;
using Cosmos.Kernel.Core.CPU;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.Core.Scheduler;

namespace Cosmos.Kernel.Core.ARM64.Cpu;

/// <summary>
/// ARM64 interrupt controller. Owns exception vectors, GIC bring-up, and
/// the ARM64 dispatch path: GIC acknowledge, dense SPI/PPI/SGI lookup,
/// sparse GICv3 LPI lookup (INTID &gt;= 8192), EOI, and synchronous-exception
/// fatal handling. Native imports live in Cosmos.Kernel.Core.ARM64/Bridge/Import/Arm64ExceptionVectorNative.cs.
/// </summary>
internal class ARM64InterruptController : IInterruptController
{
    private bool _initialized;
    private uint _lastAckedIntId;

    // ARM64 LPIs (GICv3 ITS) live in INTID space starting at 8192 (see
    // GIC.LPI_START). The dense InterruptManager.s_irqHandlers[256] table
    // can't index them — we keep a separate sparse handler array.
    /// <summary>First LPI INTID; public alias of <see cref="GIC.LPI_START"/>.</summary>
    public const uint LpiBase = GIC.LPI_START;
    /// <summary>Entries in the kernel's sparse LPI handler table (dispatchable window LpiBase..LpiBase + LpiHandlerCount - 1). Kernel dispatch policy only: 1024 covers many devices' worth of MSI-X vectors and is deliberately smaller than the 8192-INTID window the GICv3Lpi prop table covers.</summary>
    public const int LpiHandlerCount = 1024;

    /// <summary>First GICv3-reserved special INTID (1020..1023, 1023 = spurious) per ARM IHI 0069G.</summary>
    private const uint SpecialIntIdBase = 1020;
    /// <summary>Exclusive upper bound of the GICv3-reserved special INTID range (1020..1023).</summary>
    private const uint SpecialIntIdLimit = 1024;

    /// <summary>ARM64 exception type reported by the vector stub: synchronous exception.</summary>
    private const ulong ExceptionTypeSync = 0;
    /// <summary>ARM64 exception type reported by the vector stub: IRQ (GIC).</summary>
    private const ulong ExceptionTypeIrq = 1;

    /// <summary>GIC priority for the timer PPI (lower value = higher priority; 0x80 = medium).</summary>
    private const byte TimerPriorityMedium = 0x80;

    /// <summary>Size in bytes of the NEON save area the vector stub pushes below the IRQContext (public: GenericTimer derives the saved-context SP from it).</summary>
    public const int NeonSaveAreaBytes = 512;

    /// <summary>Bit position of the EC (Exception Class) field in ESR_EL1 (bits [31:26]).</summary>
    private const int EsrEcShift = 26;
    /// <summary>Mask for the 6-bit EC (Exception Class) field of ESR_EL1.</summary>
    private const uint EsrEcMask = 0x3F;

    /// <summary>ESR_EL1 EC: unknown reason (ARM DDI 0487, ESR_EL1.EC = 0b000000).</summary>
    private const uint EcUnknown = 0x00;
    /// <summary>ESR_EL1 EC: SVC instruction execution in AArch64 state (EC = 0b010101).</summary>
    private const uint EcSvcAArch64 = 0x15;
    /// <summary>ESR_EL1 EC: instruction abort from a lower Exception level (EC = 0b100000).</summary>
    private const uint EcInstrAbortLowerEl = 0x20;
    /// <summary>ESR_EL1 EC: instruction abort taken without a change in Exception level (EC = 0b100001).</summary>
    private const uint EcInstrAbortCurrentEl = 0x21;
    /// <summary>ESR_EL1 EC: PC alignment fault (EC = 0b100010).</summary>
    private const uint EcPcAlignmentFault = 0x22;
    /// <summary>ESR_EL1 EC: data abort from a lower Exception level (EC = 0b100100).</summary>
    private const uint EcDataAbortLowerEl = 0x24;
    /// <summary>ESR_EL1 EC: data abort taken without a change in Exception level (EC = 0b100101).</summary>
    private const uint EcDataAbortCurrentEl = 0x25;
    /// <summary>ESR_EL1 EC: SP alignment fault (EC = 0b100110).</summary>
    private const uint EcSpAlignmentFault = 0x26;
    private static InterruptManager.IrqDelegate[]? s_lpiHandlers;
    private static int s_nextLpiOffset;

    // Guards s_lpiHandlers RMW in AllocateLpi so concurrent device probes
    // can't both grab the same slot and silently drop one handler.
    private static Scheduler.SpinLock s_lpiLock;

    public bool IsInitialized => _initialized;

    public void Initialize()
    {
        Serial.Write("[ARM64InterruptController] Starting exception vector initialization...\n");

        // Initialize exception vectors (VBAR_EL1)
        Arm64ExceptionVectorNative.InitExceptionVectors();
        Serial.Write("[ARM64InterruptController] Exception vectors initialized\n");

        // Allocate the LPI handler table before the GIC (and therefore ITS)
        // comes online — Arm64MsiBinder can call AllocateLpi mid-bring-up.
        s_lpiHandlers = new InterruptManager.IrqDelegate[LpiHandlerCount];

        // Initialize the GIC (Generic Interrupt Controller)
        GIC.Initialize();

        // Enable timer interrupts (PPI 30 = non-secure physical timer)
        GIC.SetPriority(GIC.TIMER_NONSEC_PHYS, TimerPriorityMedium);  // Medium priority
        GIC.EnableInterrupt(GIC.TIMER_NONSEC_PHYS);

        Serial.Write("[ARM64InterruptController] ARM64 interrupt system ready\n");

        _initialized = true;
    }

    public void RouteIrq(byte irqNo, byte vector, bool startMasked)
    {
        // On ARM64, irqNo is the GIC interrupt ID.
        if (!startMasked)
        {
            GIC.EnableInterrupt(irqNo);
        }

        Serial.Write("[ARM64InterruptController] Routed IRQ ");
        Serial.WriteNumber(irqNo);
        Serial.Write(" -> vector ");
        Serial.WriteNumber(vector);
        Serial.Write("\n");
    }

    /// <summary>
    /// Allocates an unused LPI (GICv3 ITS), registers <paramref name="handler"/>,
    /// and returns the absolute INTID (&gt;= <see cref="LpiBase"/>). The matching
    /// LPI must still be enabled in the redistributor's PROPBASER table by
    /// <see cref="GICv3Lpi"/> before it can fire. Throws on exhaustion.
    /// </summary>
    internal static uint AllocateLpi(InterruptManager.IrqDelegate handler)
    {
        if (s_lpiHandlers == null)
        {
            throw new System.InvalidOperationException("ARM64InterruptController.Initialize must be called before AllocateLpi");
        }

        s_lpiLock.Acquire();
        try
        {
            for (int o = s_nextLpiOffset; o < s_lpiHandlers.Length; o++)
            {
                if (s_lpiHandlers[o] == null)
                {
                    s_lpiHandlers[o] = handler;
                    s_nextLpiOffset = o + 1;
                    return LpiBase + (uint)o;
                }
            }
            for (int o = 0; o < s_nextLpiOffset; o++)
            {
                if (s_lpiHandlers[o] == null)
                {
                    s_lpiHandlers[o] = handler;
                    s_nextLpiOffset = o + 1;
                    return LpiBase + (uint)o;
                }
            }
        }
        finally
        {
            s_lpiLock.Release();
        }
        throw new System.InvalidOperationException("ARM64InterruptController: LPI range exhausted");
    }

    public void Dispatch(ref IRQContext ctx)
    {
        // ARM64 exception types: 0 = sync, 1 = IRQ (GIC), 2 = FIQ, 3 = SError.
        if (ctx.interrupt == ExceptionTypeIrq)
        {
            uint intId = GIC.AcknowledgeInterrupt();
            _lastAckedIntId = intId;

            // INTIDs 1020..1023 are GICv3-reserved (1023 = spurious); LPIs
            // start at 8192 and must NOT be treated as spurious.
            if (intId >= SpecialIntIdBase && intId < LpiBase)
            {
                return;
            }

            // SPI/PPI/SGI -> dense InterruptManager handlers; LPI -> sparse local table.
            InterruptManager.IrqDelegate? handler = null;
            if (intId >= LpiBase)
            {
                uint off = intId - LpiBase;
                if (s_lpiHandlers != null && off < (uint)s_lpiHandlers.Length)
                {
                    handler = s_lpiHandlers[(int)off];
                }
            }
            else
            {
                InterruptManager.IrqDelegate?[]? handlers = InterruptManager.s_irqHandlers;
                if (handlers != null && intId < (uint)handlers.Length)
                {
                    handler = handlers[(int)intId];
                }
            }

            if (handler != null)
            {
                ctx.interrupt = intId;
                handler(ref ctx);
            }

            SendEOI();

            if (handler != null)
            {
                // Mirror of the x64 IRQ-exit reschedule (see
                // X64InterruptController.Dispatch): the ARM64 vector stub also
                // applies the staged context switch on every exit. Single-CPU
                // ARM64 for now (cpuId 0); the saved context sits 512 bytes
                // (NEON save area) below the IRQContext, same derivation as
                // the GenericTimer handler.
                RunPendingReschedule(ref ctx);
            }
            return;
        }

        if (ctx.interrupt == ExceptionTypeSync)  // Synchronous exception
        {
            InterruptManager.IrqDelegate?[]? handlers = InterruptManager.s_irqHandlers;
            if (handlers != null)
            {
                InterruptManager.IrqDelegate? handler = handlers[0];
                if (handler != null)
                {
                    handler(ref ctx);
                    return;
                }
            }

            HandleFatalException(ctx.interrupt, ctx.cpu_flags, ctx.fault_address);
            return;
        }

        // FIQ or SError - log and halt
        Serial.Write("[INT] Unexpected exception type: ", ctx.interrupt, "\n");
        HandleFatalException(ctx.interrupt, ctx.cpu_flags, ctx.fault_address);
    }

    private static unsafe void RunPendingReschedule(ref IRQContext ctx)
    {
        SchedulerManager.ReschedulePendingFromIrq(0, (nuint)Unsafe.AsPointer(ref ctx) - NeonSaveAreaBytes);
    }

    private void SendEOI()
    {
        // INTIDs 1020..1023 are GIC-reserved (1023 = spurious); writing those
        // to ICC_EOIR1_EL1 is UNPREDICTABLE per ARM IHI 0069G. Everything
        // else — SPI/PPI/SGI in [0..1019] *and* LPIs in [8192..16777215]
        // delivered by the ITS — must be EOI'd or the CPU interface keeps
        // the priority active and silently drops every subsequent IRQ at
        // equal/lower priority.
        if (_lastAckedIntId < SpecialIntIdBase || _lastAckedIntId >= SpecialIntIdLimit)
        {
            GIC.EndOfInterrupt(_lastAckedIntId);
        }
    }

    private static void HandleFatalException(ulong interrupt, ulong cpuFlags, ulong faultAddress)
    {
        // Decode ESR_EL1 to get exception class.
        uint ec = (uint)(cpuFlags >> EsrEcShift) & EsrEcMask;

        Serial.Write("[INT] FATAL: Synchronous exception\n");
        Serial.Write("[INT] ESR_EL1: 0x");
        Serial.WriteHex(cpuFlags);
        Serial.Write(" (EC=0x");
        Serial.WriteHex(ec);
        Serial.Write(")\n");
        Serial.Write("[INT] FAR_EL1: 0x");
        Serial.WriteHex(faultAddress);
        Serial.Write("\n");

        switch (ec)
        {
            case EcUnknown: Serial.Write("[INT] Unknown exception\n"); break;
            case EcSvcAArch64: Serial.Write("[INT] SVC from AArch64\n"); break;
            case EcInstrAbortLowerEl: Serial.Write("[INT] Instruction abort from lower EL\n"); break;
            case EcInstrAbortCurrentEl: Serial.Write("[INT] Instruction abort from current EL\n"); break;
            case EcPcAlignmentFault: Serial.Write("[INT] PC alignment fault\n"); break;
            case EcDataAbortLowerEl: Serial.Write("[INT] Data abort from lower EL\n"); break;
            case EcDataAbortCurrentEl: Serial.Write("[INT] Data abort from current EL\n"); break;
            case EcSpAlignmentFault: Serial.Write("[INT] SP alignment fault\n"); break;
            default: Serial.Write("[INT] Exception class: 0x"); Serial.WriteHex(ec); Serial.Write("\n"); break;
        }

        Serial.Write("[INT] System halted.\n");
        while (true) { }
    }
}
