// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Runtime.CompilerServices;
using Cosmos.Kernel.Core.ARM64.Memory;
using Cosmos.Kernel.Core.Memory.VAS;
using Cosmos.Kernel.HAL.Boot;

namespace Cosmos.Kernel.HAL.ARM64;

/// <summary>
/// Eager Constructor that registers the ARM64 platform initializer and the
/// ARM64 virtual memory mapper. This runs automatically when the assembly is loaded.
/// </summary>
[EagerStaticClassConstruction]
internal static class EagerCtor
{
    static EagerCtor()
    {
        PlatformHAL.SetInitializer(new ARM64PlatformInitializer());
        VirtualMemoryProvider.Mapper = ARM64VirtualMemoryMapper.Instance;
    }
}
