using Cosmos.Kernel.Core.CPU;

namespace Cosmos.Kernel.Core.RISCV64.Cpu;

internal partial class RISCV64CpuOps : ICpuOps
{
    public void Halt() => InternalCpu.Halt();

    public void DisableInterrupts() => InternalCpu.DisableInterrupts();

    public void EnableInterrupts() => InternalCpu.EnableInterrupts();
}
