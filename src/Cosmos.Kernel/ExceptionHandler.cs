using Cosmos.Kernel.Core.CPU;
using Cosmos.Kernel.Core.IO;

namespace Cosmos.Kernel;

/// <summary>
/// Handles CPU exceptions.
/// </summary>
internal static class ExceptionHandler
{
    /// <summary>
    /// Initializes CPU exception handlers.
    /// Must be called explicitly after InterruptManager is initialized.
    /// </summary>
    internal static void Initialize()
    {
#if ARCH_ARM64
        // ARM64 exception types: 0=Sync, 1=IRQ, 2=FIQ, 3=SError
        InterruptManager.SetHandler(0x00, SynchronousException);
        InterruptManager.SetHandler(0x01, IrqException);
        InterruptManager.SetHandler(0x02, FiqException);
        InterruptManager.SetHandler(0x03, SErrorException);

        Serial.WriteString("[ExceptionHandler] ARM64 exception handlers registered\n");
#elif ARCH_X64
        // Register handlers for common CPU exceptions
        // Vector 0x00: Divide by Zero
        InterruptManager.SetHandler(0x00, DivideByZero);

        // Vector 0x01: Debug - SKIP (can trigger debug exceptions during registration)
        // InterruptManager.SetHandler(0x01, Debug);

        // Vector 0x02: Non-Maskable Interrupt (NMI)
        InterruptManager.SetHandler(0x02, NonMaskableInterrupt);

        // Vector 0x03: Breakpoint
        InterruptManager.SetHandler(0x03, Breakpoint);

        // Vector 0x04: Overflow
        InterruptManager.SetHandler(0x04, Overflow);

        // Vector 0x05: Bounds Check
        InterruptManager.SetHandler(0x05, BoundsCheck);

        // Vector 0x06: Invalid Opcode
        InterruptManager.SetHandler(0x06, InvalidOpcode);

        // Vector 0x07: Device Not Available (FPU)
        InterruptManager.SetHandler(0x07, DeviceNotAvailable);

        // Vector 0x08: Double Fault
        InterruptManager.SetHandler(0x08, DoubleFault);

        // Vector 0x0A: Invalid TSS
        InterruptManager.SetHandler(0x0A, InvalidTss);

        // Vector 0x0B: Segment Not Present
        InterruptManager.SetHandler(0x0B, SegmentNotPresent);

        // Vector 0x0C: Stack Segment Fault
        InterruptManager.SetHandler(0x0C, StackSegmentFault);

        // Vector 0x0D: General Protection Fault (GPF)
        InterruptManager.SetHandler(0x0D, GeneralProtectionFault);

        // Vector 0x0E: Page Fault
        InterruptManager.SetHandler(0x0E, PageFault);

        // Vector 0x10: Floating Point Exception
        InterruptManager.SetHandler(0x10, FloatingPointException);

        // Vector 0x11: Alignment Check
        InterruptManager.SetHandler(0x11, AlignmentCheck);

        // Vector 0x12: Machine Check
        InterruptManager.SetHandler(0x12, MachineCheck);

        // Vector 0x13: SIMD Floating Point
        InterruptManager.SetHandler(0x13, SimdFloatingPoint);

        Serial.WriteString("[ExceptionHandler] CPU exception handlers registered\n");
#else
        Serial.WriteString("[ExceptionHandler] No exception handlers for this architecture\n");
#endif
    }

#if ARCH_ARM64
    private static void SynchronousException(ref IRQContext ctx)
    {
        Panic.CpuException("Synchronous", ref ctx);
    }

    private static void IrqException(ref IRQContext ctx)
    {
        // IRQ is not a fatal exception - just log it
        WriteDebugLine("");
        WriteDebugLine("========================================");
        WriteDebugLine("ARM64 EXCEPTION: IRQ");
        WriteDebugLine("========================================");
        PrintExceptionInfo(ref ctx);
    }

    private static void FiqException(ref IRQContext ctx)
    {
        // FIQ is not a fatal exception - just log it
        WriteDebugLine("");
        WriteDebugLine("========================================");
        WriteDebugLine("ARM64 EXCEPTION: FIQ");
        WriteDebugLine("========================================");
        PrintExceptionInfo(ref ctx);
    }

    private static void SErrorException(ref IRQContext ctx)
    {
        Panic.CpuException("SError", ref ctx);
    }
#endif

#if ARCH_X64
    private static void DivideByZero(ref IRQContext ctx)
    {
        Panic.CpuException("Divide by Zero (#DE)", ref ctx);
    }

    private static void Debug(ref IRQContext ctx)
    {
        Panic.CpuException("Debug (#DB)", ref ctx);
    }

    private static void NonMaskableInterrupt(ref IRQContext ctx)
    {
        Panic.CpuException("Non-Maskable Interrupt (NMI)", ref ctx);
    }

    private static void Breakpoint(ref IRQContext ctx)
    {
        Panic.CpuException("Breakpoint (#BP)", ref ctx);
    }

    private static void Overflow(ref IRQContext ctx)
    {
        Panic.CpuException("Overflow (#OF)", ref ctx);
    }

    private static void BoundsCheck(ref IRQContext ctx)
    {
        Panic.CpuException("Bounds Check (#BR)", ref ctx);
    }

    private static void InvalidOpcode(ref IRQContext ctx)
    {
        Panic.CpuException("Invalid Opcode (#UD)", ref ctx);
    }

    private static void DeviceNotAvailable(ref IRQContext ctx)
    {
        Panic.CpuException("Device Not Available (#NM)", ref ctx);
    }

    private static void DoubleFault(ref IRQContext ctx)
    {
        Panic.CpuException("Double Fault (#DF)", ref ctx);
    }

    private static void InvalidTss(ref IRQContext ctx)
    {
        Panic.CpuException("Invalid TSS (#TS)", ref ctx);
    }

    private static void SegmentNotPresent(ref IRQContext ctx)
    {
        Panic.CpuException("Segment Not Present (#NP)", ref ctx);
    }

    private static void StackSegmentFault(ref IRQContext ctx)
    {
        Panic.CpuException("Stack Segment Fault (#SS)", ref ctx);
    }

    private static void GeneralProtectionFault(ref IRQContext ctx)
    {
        Panic.CpuException("General Protection Fault (#GP)", ref ctx);
    }

    private static void PageFault(ref IRQContext ctx)
    {
        Panic.CpuException("Page Fault (#PF)", ref ctx);
    }

    private static void FloatingPointException(ref IRQContext ctx)
    {
        Panic.CpuException("Floating Point Exception (#MF)", ref ctx);
    }

    private static void AlignmentCheck(ref IRQContext ctx)
    {
        Panic.CpuException("Alignment Check (#AC)", ref ctx);
    }

    private static void MachineCheck(ref IRQContext ctx)
    {
        Panic.CpuException("Machine Check (#MC)", ref ctx);
    }

    private static void SimdFloatingPoint(ref IRQContext ctx)
    {
        Panic.CpuException("SIMD Floating Point Exception (#XM)", ref ctx);
    }
#endif

    private static void PrintExceptionInfo(ref IRQContext ctx)
    {
        // Output to serial
        WriteDebugLine("Interrupt Vector: " + ctx.interrupt.ToString());
        WriteDebugLine("CPU Flags (ESR): 0x" + ctx.cpu_flags.ToString("X"));
        WriteDebugLine("");
        WriteDebugLine("Registers:");
#if ARCH_ARM64
        WriteDebugLine("  X0:  0x" + ctx.x0.ToString("X16") + "  X1:  0x" + ctx.x1.ToString("X16"));
        WriteDebugLine("  X2:  0x" + ctx.x2.ToString("X16") + "  X3:  0x" + ctx.x3.ToString("X16"));
        WriteDebugLine("  X4:  0x" + ctx.x4.ToString("X16") + "  X5:  0x" + ctx.x5.ToString("X16"));
        WriteDebugLine("  X6:  0x" + ctx.x6.ToString("X16") + "  X7:  0x" + ctx.x7.ToString("X16"));
        WriteDebugLine("  X8:  0x" + ctx.x8.ToString("X16") + "  X9:  0x" + ctx.x9.ToString("X16"));
        WriteDebugLine("  X10: 0x" + ctx.x10.ToString("X16") + "  X11: 0x" + ctx.x11.ToString("X16"));
        WriteDebugLine("  X29: 0x" + ctx.x29.ToString("X16") + "  X30: 0x" + ctx.x30.ToString("X16"));
        WriteDebugLine("  SP:  0x" + ctx.sp.ToString("X16") + "  ELR: 0x" + ctx.elr.ToString("X16"));
        WriteDebugLine("  SPSR: 0x" + ctx.spsr.ToString("X16"));
#else
        WriteDebugLine("  RAX: 0x" + ctx.rax.ToString("X16") + "  RBX: 0x" + ctx.rbx.ToString("X16"));
        WriteDebugLine("  RCX: 0x" + ctx.rcx.ToString("X16") + "  RDX: 0x" + ctx.rdx.ToString("X16"));
        WriteDebugLine("  RSI: 0x" + ctx.rsi.ToString("X16") + "  RDI: 0x" + ctx.rdi.ToString("X16"));
        WriteDebugLine("  RBP: 0x" + ctx.rbp.ToString("X16") + "  R8:  0x" + ctx.r8.ToString("X16"));
        WriteDebugLine("  R9:  0x" + ctx.r9.ToString("X16") + "  R10: 0x" + ctx.r10.ToString("X16"));
        WriteDebugLine("  R11: 0x" + ctx.r11.ToString("X16") + "  R12: 0x" + ctx.r12.ToString("X16"));
        WriteDebugLine("  R13: 0x" + ctx.r13.ToString("X16") + "  R14: 0x" + ctx.r14.ToString("X16"));
        WriteDebugLine("  R15: 0x" + ctx.r15.ToString("X16"));
#endif
        WriteDebugLine("========================================");
    }

    private static void WriteDebugLine(string message)
    {
        // Write to serial
        Serial.WriteString(message);
        Serial.WriteString("\n");

        // Write to screen
        //KernelConsole.WriteLine(message);
    }
}
