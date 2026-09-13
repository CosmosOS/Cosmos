using Cosmos.Kernel.Core.CPU;

namespace Cosmos.Kernel.Core.ARM64.Cpu;

internal partial class ARM64CpuOps : ICpuOps
{
    public void Halt() => InternalCpu.Halt();

    public void DisableInterrupts() => InternalCpu.DisableInterrupts();

    public void EnableInterrupts() => InternalCpu.EnableInterrupts();
}
