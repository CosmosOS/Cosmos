using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.Core.X64.Bridge;

namespace Cosmos.Kernel.Core.X64.Cpu;

/// <summary>
/// Loads the fixed kernel GDT during HAL init. The GDT body and pointer
/// image live in native memory (src/Cosmos.Kernel.Native.X64/CPU/Gdt.s);
/// this class is just the managed call site. Selectors:
///   0x08 ring-0 code, 0x10 ring-0 data,
///   0x1B ring-3 code, 0x23 ring-3 data.
/// </summary>
internal static unsafe class Gdt
{
    /// <summary>Ring-0 64-bit code selector (DPL=0).</summary>
    public const ushort KernelCodeSelector = 0x08;

    /// <summary>Ring-0 64-bit data selector (DPL=0).</summary>
    public const ushort KernelDataSelector = 0x10;

    /// <summary>Ring-3 64-bit code selector (DPL=3) - used by SYSRET and
    /// ring-3 iretq to drop to user mode.</summary>
    public const ushort UserCodeSelector = 0x1B;

    /// <summary>Ring-3 64-bit data selector (DPL=3) - SYSRET derives SS as
    /// user CS + 8; ring-3 iretq pops SS from the frame.</summary>
    public const ushort UserDataSelector = 0x23;

    /// <summary>
    /// Loads the kernel GDT. Called once during HAL init, before
    /// <c>Idt.RegisterAllInterrupts</c> reads CS for the IDT selectors.
    /// Idempotent against repeated calls - the loaded CS (0x08) matches what
    /// Limine set, so existing IRQ stubs and the IDT keep working unchanged.
    /// </summary>
    public static void Load()
    {
        Serial.WriteString("[GDT] Loading kernel GDT (ring-0 + ring-3)...\n");
        GdtNative.LoadGdt();
        Serial.WriteString("[GDT] GDT loaded; CS=0x08, ring-3 selectors 0x1B/0x23 available\n");
    }
}