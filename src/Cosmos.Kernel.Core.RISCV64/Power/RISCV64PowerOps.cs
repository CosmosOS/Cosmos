using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.Core.CPU;
using Cosmos.Kernel.Core.Power;
using Cosmos.Kernel.Core.RISCV64.Cpu;

namespace Cosmos.Kernel.Core.RISCV64.Power;

/// <summary>
/// Shutdown and reboot through the SBI system reset extension (the legacy
/// shutdown call when the firmware lacks it).
/// </summary>
internal class RISCV64PowerOps : IPowerOps
{
    [DoesNotReturn]
    public void Reboot()
    {
        InternalCpu.DisableInterrupts();
        Sbi.SystemReset(Sbi.ResetTypeColdReboot, Sbi.ResetReasonNone);

        while (true)
        {
            InternalCpu.Halt();
        }
    }

    [DoesNotReturn]
    public void Shutdown()
    {
        InternalCpu.DisableInterrupts();
        Sbi.SystemReset(Sbi.ResetTypeShutdown, Sbi.ResetReasonNone);

        while (true)
        {
            InternalCpu.Halt();
        }
    }
}
