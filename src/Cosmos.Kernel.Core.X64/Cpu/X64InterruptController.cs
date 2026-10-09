// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Runtime.CompilerServices;
using Cosmos.Kernel.Core.CPU;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.Core.Memory;
using Cosmos.Kernel.Core.Memory.VAS;
using Cosmos.Kernel.Core.Scheduler;

namespace Cosmos.Kernel.Core.X64.Cpu;

/// <summary>
/// X64 interrupt controller - manages IDT and APIC, owns the x64 dispatch
/// path (vector lookup, EOI for hardware IRQs, fatal CPU-exception halt).
/// </summary>
internal class X64InterruptController : IInterruptController
{
    /// <summary>Highest CPU-exception vector; vectors 0-31 are reserved for exceptions (SDM 3A §6.2).</summary>
    private const ulong MaxCpuExceptionVector = 31;

    /// <summary>First hardware-IRQ vector; vectors >= 32 are external interrupts that require an EOI.</summary>
    private const ulong FirstHardwareIrqVector = 32;

    /// <summary>Page-fault exception vector (#PF, SDM 3A §6.15).</summary>
    private const ulong PageFaultVector = 14;

    /// <summary>Size in bytes of the XMM save area the asm stub pushes below the IRQContext. Internal: shared with the LAPIC timer handler's RSP derivation.</summary>
    internal const int XmmSaveAreaSizeBytes = 256;

    /// <summary>
    /// The vector of the scheduler's self-IPI (<see cref="TryRaiseReschedule"/>):
    /// above the LAPIC timer's <see cref="LocalApic.TIMER_VECTOR"/>, outside
    /// the ISA lines and the dynamic range devices allocate from.
    /// </summary>
    private const byte RescheduleVector = 0xF0;

    /// <summary>
    /// True once a hardware line can be routed, which on x64 takes the I/O
    /// APIC the MADT describes. The Local APIC alone, which a machine without
    /// ACPI still has, delivers the timer and MSI messages but no line, so
    /// line sources refuse to connect and their drivers poll.
    /// </summary>
    public bool IsInitialized => ApicManager.CanRouteIrqs;

    public void Initialize()
    {
        Serial.Write("[X64InterruptController] Starting IDT initialization...\n");
        // Install the kernel GDT (ring-0 + ring-3 descriptors) before reading
        // CS for the IDT selectors. Limine's boot GDT only has ring-0
        // descriptors; we need the ring-3 selectors (0x1B/0x23) ready so
        // SYSCALL/SYSRET and ring-3 iretq can drop CPL. The reloaded CS is
        // 0x08 - matching what Limine set - so existing IRQ stubs are
        // unaffected.
        Gdt.Load();
        Idt.RegisterAllInterrupts();
        Serial.Write("[X64InterruptController] IDT initialization complete\n");

        // A handler, though it does nothing, is what takes the vector down
        // Dispatch's hardware-IRQ path: the EOI, then the pending reschedule.
        InterruptManager.SetHandler(RescheduleVector, HandleReschedule);
    }

    /// <inheritdoc/>
    public bool TryRaiseReschedule()
    {
        if (!ApicManager.IsInitialized)
        {
            return false;
        }

        LocalApic.SendSelfIpi(RescheduleVector);
        return true;
    }

    public void RouteIrq(byte irqNo, byte vector, bool startMasked)
    {
        if (ApicManager.CanRouteIrqs)
        {
            ApicManager.RouteIrq(irqNo, vector, startMasked);
        }
    }

    /// <inheritdoc/>
    public void MaskIrq(byte irqNo)
    {
        if (ApicManager.CanRouteIrqs)
        {
            ApicManager.MaskIrq(irqNo);
        }
    }

    /// <inheritdoc/>
    public void UnmaskIrq(byte irqNo)
    {
        if (ApicManager.CanRouteIrqs)
        {
            ApicManager.UnmaskIrq(irqNo);
        }
    }

    public unsafe void Dispatch(ref IRQContext ctx)
    {
        InterruptManager.IrqDelegate?[]? handlers = InterruptManager.s_irqHandlers;
        if (handlers != null && ctx.interrupt < (ulong)handlers.Length)
        {
            InterruptManager.IrqDelegate? handler = handlers[(int)ctx.interrupt];
            if (handler != null)
            {
                handler(ref ctx);

                // Send EOI for hardware IRQs (vector >= 32) — but never for
                // the APIC spurious vector: a spurious delivery sets no ISR
                // bit, so an EOI here would retire whichever real interrupt
                // is currently in service (SDM 3A §11.9).
                if (ctx.interrupt >= FirstHardwareIrqVector && ctx.interrupt != LocalApic.SPURIOUS_VECTOR && ApicManager.IsInitialized)
                {
                    SendEOI();

                    // A handler-side ReadyThread (e.g. InterruptEvent.Signal
                    // from a device ISR) requests a reschedule; honor it now —
                    // the common asm stub applies the staged context switch on
                    // every interrupt exit, not just timer ticks. Same RSP
                    // derivation (and kernel-space sanity check) as the LAPIC
                    // timer handler: the saved context sits 256 bytes (XMM
                    // save area) below the IRQContext.
                    nuint currentRsp = (nuint)Unsafe.AsPointer(ref ctx) - XmmSaveAreaSizeBytes;
                    if ((currentRsp & AddressSpaceConst.KernelSpaceCanonicalMask) == AddressSpaceConst.KernelSpaceCanonicalMask)
                    {
                        SchedulerManager.ReschedulePendingFromIrq(LocalApic.GetId(), currentRsp);
                    }
                }
                return;
            }
        }

        // No managed handler - for CPU exceptions (0-31), fall through to fatal halt
        if (ctx.interrupt <= MaxCpuExceptionVector)
        {
            HandleFatalException(ctx.interrupt, ctx.cpu_flags, ctx.fault_address);
            return;
        }

        // Send EOI even for unhandled hardware interrupts to prevent lockup.
        // The spurious vector is the exception (see above): it arrives here
        // because nothing registers a handler for it, and it must be
        // dismissed without EOI.
        if (ctx.interrupt >= FirstHardwareIrqVector && ctx.interrupt != LocalApic.SPURIOUS_VECTOR && ApicManager.IsInitialized)
        {
            SendEOI();
        }
    }

    /// <summary>The reschedule self-IPI's handler: nothing to do, Dispatch runs the reschedule on its exit.</summary>
    private static void HandleReschedule(ref IRQContext ctx)
    {
    }

    private static void SendEOI()
    {
        if (ApicManager.IsInitialized)
        {
            ApicManager.SendEOI();
        }
    }

    /// <summary>CPU exceptions that push an error code (SDM 3A §6.15); the asm stub stashes it for <see cref="Bridge.IdtNative.GetLastErrorCode"/>.</summary>
    private static bool HasErrorCode(ulong interrupt) =>
        interrupt is 8 or (>= 10 and <= 14) or 17 or 21 or 29 or 30;

    private static void HandleFatalException(ulong interrupt, ulong cpuFlags, ulong faultAddress)
    {
        // For page faults, give the managed handler a chance to resolve or kill the process.
        if (interrupt == PageFaultVector)
        {
            ulong errorCode = 0;
            if (HasErrorCode(interrupt))
            {
                errorCode = Cosmos.Kernel.Core.X64.Bridge.IdtNative.GetLastErrorCode();
            }

            bool wasWrite = (errorCode & (1UL << 1)) != 0;
            bool wasInstructionFetch = (errorCode & (1UL << 4)) != 0;

            var info = new PageFaultInfo(faultAddress, errorCode, wasWrite, wasInstructionFetch);
            if (PageFaultHandler.Handle(info))
            {
                // Handler resolved the fault (e.g. terminated the process).
                // The scheduler will pick another thread on the next tick.
                return;
            }
        }

        Serial.Write("[INT] FATAL: Exception ", interrupt, "\n");

        if (HasErrorCode(interrupt))
        {
            Serial.Write("[INT] Error code: 0x");
            Serial.WriteHex(Bridge.IdtNative.GetLastErrorCode());
            Serial.Write("\n");
        }

        Serial.Write("[INT] RFLAGS: 0x");
        Serial.WriteHex(cpuFlags);
        Serial.Write("\n");

        // For page faults (#PF = 14), show the faulting address
        if (interrupt == PageFaultVector)
        {
            Serial.Write("[INT] Fault address (CR2): 0x");
            Serial.WriteHex(faultAddress);
            Serial.Write("\n");
        }

        while (true) { }
    }
}
