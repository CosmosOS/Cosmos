// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Core;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.Core.X64.Bridge;
using Cosmos.Kernel.Core.X64.Cpu.Data;

namespace Cosmos.Kernel.Core.X64.Cpu;

/// <summary>
/// Handles Interrupt Descriptor Table initialization for x86_64.
/// Native imports live in Cosmos.Kernel.Core.X64/Bridge/Import/IdtNative.cs.
/// </summary>
public static unsafe class Idt
{
    public static ulong GetCurrentCodeSelector() => IdtNative.GetCurrentCodeSelector();

    private static IdtEntry[]? s_idtEntries;

    /// <summary>
    /// Registers all IRQ stubs in the IDT.
    /// </summary>
    public static void RegisterAllInterrupts()
    {
        // Get the current code selector from the bootloader (Limine)
        ulong cs = GetCurrentCodeSelector();
        Serial.Write("[IDT] Initializing with CS=0x", cs.ToString("X"), "\n");

        // Allocate IDT entries array if not already done
        if (s_idtEntries == null)
        {
            Serial.WriteString("[IDT] Allocating s_idtEntries array...\n");
            s_idtEntries = new IdtEntry[256];
        }
        Serial.WriteString("[IDT] s_idtEntries length: ");
        Serial.WriteNumber((ulong)s_idtEntries.Length);
        Serial.WriteString("\n");

        // Initialize all 256 IDT entries
        for (int i = 0; i < 256; i++)
        {
            if (i == 0 || i == 255)
            {
                Serial.WriteString("[IDT] Setting entry ");
                Serial.WriteNumber((ulong)i);
                Serial.WriteString("\n");
            }
            s_idtEntries[i].Selector = (ushort)cs;
            s_idtEntries[i].RawFlags = 0x8E00;  // P=1, DPL=0, Type=Interrupt Gate
            s_idtEntries[i].Offset = (ulong)GetStub(i);
        }

        // Load the IDT into the CPU
        fixed (void* ptr = &s_idtEntries[0])
        {
            IdtPointer idtPtr = new IdtPointer
            {
                Limit = (ushort)(256 * 16 - 1),
                Base = (ulong)ptr
            };

            Serial.Write("[IDT] IDT base: 0x", idtPtr.Base.ToString("X"), "\n");
            Serial.Write("[IDT] IDT Limit: 0x", ((ulong)idtPtr.Limit).ToString("X"), "\n");

            // Load the IDT
            IdtNative.LoadIdt((void*)&idtPtr);
            Serial.Write("[IDT] Loaded 256 interrupt vectors at 0x", idtPtr.Base.ToString("X"), "\n");
        }

        Serial.Write("[IDT] IDT loaded.\n");
    }

    /// <summary>
    /// Loads the table <see cref="RegisterAllInterrupts"/> built into the calling CPU's IDTR,
    /// leaving its interrupt flag alone: an application processor, from its managed entry with
    /// interrupts masked (SMP phase 2). The table is one array for every CPU; only the
    /// register that points at it is per CPU. Allocation-free.
    /// </summary>
    internal static void LoadOnCurrentCpu()
    {
        if (s_idtEntries is null)
        {
            Panic.Halt("The IDT was not built on the boot CPU before an application processor loaded it.");
        }

        fixed (void* ptr = &s_idtEntries[0])
        {
            IdtPointer idtPtr = new IdtPointer
            {
                Limit = (ushort)(256 * 16 - 1),
                Base = (ulong)ptr
            };

            IdtNative.LoadIdtKeepFlags((void*)&idtPtr);
        }
    }

    public static delegate* unmanaged<void> GetStub(int index)
    {
        if ((uint)index >= 256)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        nint stubAddr = IdtNative.GetIrqStub(index);

        return (delegate* unmanaged<void>)stubAddr;
    }
}
