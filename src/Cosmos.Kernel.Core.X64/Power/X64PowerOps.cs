using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.Core.Bridge;
using Cosmos.Kernel.Core.CPU;
using Cosmos.Kernel.Core.Power;
using Cosmos.Kernel.Core.X64.Bridge;

namespace Cosmos.Kernel.Core.X64.Power;

internal class X64PowerOps : IPowerOps
{
    // 8042 keyboard controller ports.
    private const ushort Kbc_StatusPort = 0x64;
    private const ushort Kbc_CommandPort = 0x64;
    private const byte Kbc_InputBufferFull = 0x02;
    private const byte Kbc_PulseResetCommand = 0xFE;

    // Emulator ACPI shutdown ports - used as fallback when LAI/ACPI _S5 is
    // unavailable (e.g. firmware without DSDT, or namespace creation failed).
    private const ushort Qemu_ShutdownPort = 0x604;
    private const ushort Bochs_ShutdownPort = 0xB004;
    private const ushort Vbox_ShutdownPort = 0x4004;

    [DoesNotReturn]
    public void Reboot()
    {
        InternalCpu.DisableInterrupts();

        // 1. ACPI reset via FADT reset register (works on real hardware that
        //    advertises support; doesn't require AML namespace).
        AcpiPmNative.Reset();

        // 2. 8042 keyboard-controller pulse-reset (works on most PCs and QEMU).
        for (int i = 0; i < 0x10000; i++)
        {
            byte status = PortIoNative.ReadByte(Kbc_StatusPort);
            if ((status & Kbc_InputBufferFull) == 0)
            {
                break;
            }
        }
        PortIoNative.WriteByte(Kbc_CommandPort, Kbc_PulseResetCommand);
        for (int i = 0; i < 0x100000; i++)
        {
            PortIoNative.ReadByte(Kbc_StatusPort);
        }

        // 3. Last resort: triple fault.
        X64PowerNative.TripleFault();

        while (true)
        {
            InternalCpu.Halt();
        }
    }

    [DoesNotReturn]
    public void Shutdown()
    {
        InternalCpu.DisableInterrupts();

        // 1. ACPI S5 via LAI (real-hardware path; also works on QEMU/KVM with
        //    the firmware-provided DSDT). Builds the AML namespace on first call.
        AcpiPmNative.Shutdown();

        // 2. Emulator-specific ACPI shutdown ports for environments where
        //    namespace creation isn't viable.
        PortIoNative.WriteWord(Qemu_ShutdownPort, 0x2000);
        PortIoNative.WriteWord(Bochs_ShutdownPort, 0x2000);
        PortIoNative.WriteWord(Vbox_ShutdownPort, 0x3400);

        // 3. Nothing took us out - park the CPU.
        while (true)
        {
            InternalCpu.Halt();
        }
    }
}
