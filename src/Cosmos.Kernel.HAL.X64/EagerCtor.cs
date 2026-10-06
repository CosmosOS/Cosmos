// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Runtime.CompilerServices;
using Cosmos.Kernel.HAL.Platform;

namespace Cosmos.Kernel.HAL.X64;

/// <summary>
/// Eager Constructor that registers the X64 platform initializer.
/// This runs automatically when the assembly is loaded.
/// </summary>
[EagerStaticClassConstruction]
internal static class EagerCtor
{
    static EagerCtor()
    {
        PlatformHAL.SetInitializer(new X64PlatformInitializer());
    }
}
