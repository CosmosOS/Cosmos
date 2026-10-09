using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.Core.ARM64.Bridge;
using Cosmos.Kernel.Core.CPU;
using Cosmos.Kernel.Core.Power;

namespace Cosmos.Kernel.Core.ARM64.Power;

internal class ARM64PowerOps : IPowerOps
{
    [DoesNotReturn]
    public void Reboot()
    {
        InternalCpu.DisableInterrupts();
        PsciNative.SystemReset();

        while (true)
        {
            InternalCpu.Halt();
        }
    }

    [DoesNotReturn]
    public void Shutdown()
    {
        InternalCpu.DisableInterrupts();
        PsciNative.SystemOff();

        while (true)
        {
            InternalCpu.Halt();
        }
    }
}
