// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Runtime.CompilerServices;
using Cosmos.Kernel.Core.Memory.VAS;
using Cosmos.Kernel.Core.X64.Bridge;
using Cosmos.Kernel.Core.X64.Memory;
using Cosmos.Kernel.HAL.Boot;

namespace Cosmos.Kernel.HAL.X64;

/// <summary>
/// Eager Constructor that registers the X64 platform initializer and the
/// x64 virtual memory mapper. This runs automatically when the assembly is loaded.
/// </summary>
[EagerStaticClassConstruction]
internal static class EagerCtor
{
    static EagerCtor()
    {
        PlatformHAL.SetInitializer(new X64PlatformInitializer());
        VirtualMemoryProvider.Mapper = X64VirtualMemoryMapper.Instance;

        // Record the kernel CR3 so interrupt stubs can switch back to the full
        // kernel address space (including identity-mapped MMIO) when entering from
        // a process context.
        X64CpuNative.SetKernelCr3(X64CpuNative.ReadCr3());
    }
}
