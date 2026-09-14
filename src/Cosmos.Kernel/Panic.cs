using Cosmos.Kernel.Core.CPU;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.Core.Memory;

namespace Cosmos.Kernel;

/// <summary>
/// Extended kernel panic handler with CPU exception support.
/// </summary>
internal static class Panic
{
    /// <summary>
    /// Triggers a CPU exception panic with context information.
    /// This method is allocation-free to avoid recursive faults.
    /// Disables interrupts and halts the CPU.
    /// </summary>
    /// <param name="exceptionName">The name of the CPU exception.</param>
    /// <param name="ctx">The interrupt context with register state.</param>
    internal static void CpuException(string exceptionName, ref IRQContext ctx)
    {
        InternalCpu.DisableInterrupts();

        // Use only Serial - allocation-free, no KernelConsole which may allocate
        Serial.WriteString("\n========================================\n");
        Serial.WriteString("CPU EXCEPTION: ");
        Serial.WriteString(exceptionName);
        Serial.WriteString("\n========================================\n");
        PrintCpuContext(ref ctx);
        PrintStackTrace(ref ctx);
        Serial.WriteString("========================================\n");
        Serial.WriteString("System halted.\n");

        HaltCpu();
    }

    private static void PrintCpuContext(ref IRQContext ctx)
    {
        Serial.WriteString("Interrupt Vector: ");
        Serial.WriteNumber(ctx.interrupt);
        Serial.WriteString("\nCPU Flags (ESR): 0x");
        Serial.WriteHex(ctx.cpu_flags);
        Serial.WriteString("\n\nRegisters:\n");
#if ARCH_ARM64
        Serial.WriteString("  X0:  0x"); Serial.WriteHex(ctx.x0);
        Serial.WriteString("  X1:  0x"); Serial.WriteHex(ctx.x1); Serial.WriteString("\n");
        Serial.WriteString("  X2:  0x"); Serial.WriteHex(ctx.x2);
        Serial.WriteString("  X3:  0x"); Serial.WriteHex(ctx.x3); Serial.WriteString("\n");
        Serial.WriteString("  X4:  0x"); Serial.WriteHex(ctx.x4);
        Serial.WriteString("  X5:  0x"); Serial.WriteHex(ctx.x5); Serial.WriteString("\n");
        Serial.WriteString("  X6:  0x"); Serial.WriteHex(ctx.x6);
        Serial.WriteString("  X7:  0x"); Serial.WriteHex(ctx.x7); Serial.WriteString("\n");
        Serial.WriteString("  X8:  0x"); Serial.WriteHex(ctx.x8);
        Serial.WriteString("  X9:  0x"); Serial.WriteHex(ctx.x9); Serial.WriteString("\n");
        Serial.WriteString("  X10: 0x"); Serial.WriteHex(ctx.x10);
        Serial.WriteString("  X11: 0x"); Serial.WriteHex(ctx.x11); Serial.WriteString("\n");
        Serial.WriteString("  X29: 0x"); Serial.WriteHex(ctx.x29);
        Serial.WriteString("  X30: 0x"); Serial.WriteHex(ctx.x30); Serial.WriteString("\n");
        Serial.WriteString("  SP:  0x"); Serial.WriteHex(ctx.sp);
        Serial.WriteString("  ELR: 0x"); Serial.WriteHex(ctx.elr); Serial.WriteString("\n");
        Serial.WriteString("  SPSR: 0x"); Serial.WriteHex(ctx.spsr); Serial.WriteString("\n");
#elif ARCH_X64
        Serial.WriteString("  RAX: 0x"); Serial.WriteHex(ctx.rax);
        Serial.WriteString("  RBX: 0x"); Serial.WriteHex(ctx.rbx); Serial.WriteString("\n");
        Serial.WriteString("  RCX: 0x"); Serial.WriteHex(ctx.rcx);
        Serial.WriteString("  RDX: 0x"); Serial.WriteHex(ctx.rdx); Serial.WriteString("\n");
        Serial.WriteString("  RSI: 0x"); Serial.WriteHex(ctx.rsi);
        Serial.WriteString("  RDI: 0x"); Serial.WriteHex(ctx.rdi); Serial.WriteString("\n");
        Serial.WriteString("  RBP: 0x"); Serial.WriteHex(ctx.rbp);
        Serial.WriteString("  R8:  0x"); Serial.WriteHex(ctx.r8); Serial.WriteString("\n");
        Serial.WriteString("  R9:  0x"); Serial.WriteHex(ctx.r9);
        Serial.WriteString("  R10: 0x"); Serial.WriteHex(ctx.r10); Serial.WriteString("\n");
        Serial.WriteString("  R11: 0x"); Serial.WriteHex(ctx.r11);
        Serial.WriteString("  R12: 0x"); Serial.WriteHex(ctx.r12); Serial.WriteString("\n");
        Serial.WriteString("  R13: 0x"); Serial.WriteHex(ctx.r13);
        Serial.WriteString("  R14: 0x"); Serial.WriteHex(ctx.r14); Serial.WriteString("\n");
        Serial.WriteString("  R15: 0x"); Serial.WriteHex(ctx.r15); Serial.WriteString("\n");
        Serial.WriteString("  RIP: 0x"); Serial.WriteHex(ctx.rip);
        Serial.WriteString("  RSP: 0x"); Serial.WriteHex(ctx.rsp); Serial.WriteString("\n");
        Serial.WriteString("  RFL: 0x"); Serial.WriteHex(ctx.rflags); Serial.WriteString("\n");
        Serial.WriteString("  CR2: 0x"); Serial.WriteHex(ctx.fault_address); Serial.WriteString("\n");
#endif
    }

    /// <summary>
    /// Prints the call stack of the interrupted code by walking the frame-pointer
    /// chain. Allocation-free. Addresses are raw — symbolicate against the kernel
    /// ELF with nm/addr2line.
    /// </summary>
    /// <remarks>
    /// There is no page-table walker available here, so every dereference is
    /// gated on plausibility checks (higher-half, 8-byte aligned, within a bounded
    /// window above the faulting stack pointer, monotonically increasing). This is
    /// printed last so that a worst-case double fault cannot swallow the register
    /// dump above it.
    /// </remarks>
    private static unsafe void PrintStackTrace(ref IRQContext ctx)
    {
#if ARCH_ARM64
        ulong ip = ctx.elr;
        ulong sp = ctx.sp;
        ulong fp = ctx.x29;
#else
        ulong ip = ctx.rip;
        ulong sp = ctx.rsp;
        ulong fp = ctx.rbp;
#endif
        Serial.WriteString("\nStack trace (raw return addresses, symbolicate with nm):\n");
        Serial.WriteString("  ip:  0x"); Serial.WriteHex(ip); Serial.WriteString("\n");

        // A fault on (or just past) the stack itself means its guard/unmapped
        // page is adjacent — any read below could double-fault, so don't walk.
        if (ctx.fault_address >= sp - 0x10000 && ctx.fault_address < sp + 0x1000)
        {
            Serial.WriteString("  (fault address is near the stack pointer - possible stack overflow, not walking)\n");
            return;
        }

        if (!IsPlausibleStackAddress(sp))
        {
            return;
        }

#if ARCH_ARM64
        // On ARM64 the immediate caller of a leaf is in the link register.
        Serial.WriteString("  lr:  0x"); Serial.WriteHex(ctx.x30); Serial.WriteString("\n");
#else
        // Frameless leaf helpers (write barriers, memcpy, ...) leave their
        // caller's return address at [rsp]; ignore this line if ip is not a leaf.
        Serial.WriteString("  sp0: 0x"); Serial.WriteHex(*(ulong*)sp);
        Serial.WriteString(" (return address if ip is a frameless leaf)\n");
#endif

        // Both SysV x64 and AAPCS64 lay frames out as [fp] = caller fp,
        // [fp+8] = return address.
        int frame = 0;
        while (frame < 32 &&
               IsPlausibleStackAddress(fp) &&
               fp >= sp && fp - sp < 0x100000)
        {
            ulong nextFp = *(ulong*)fp;
            ulong ret = *((ulong*)fp + 1);

            if (ret == 0)
            {
                break;
            }

            Serial.WriteString("  [");
            Serial.WriteNumber((uint)frame);
            Serial.WriteString("]  0x");
            Serial.WriteHex(ret);
            Serial.WriteString("\n");

            if (nextFp <= fp)
            {
                break;
            }

            fp = nextFp;
            frame++;
        }
    }

    private static bool IsPlausibleStackAddress(ulong address) =>
        address >= AddressSpace.KernelSpaceStart && (address & 7) == 0;

    private static void HaltCpu()
    {
        // Infinite loop with halt to save power
        while (true)
        {
            InternalCpu.Halt();
        }
    }
}
