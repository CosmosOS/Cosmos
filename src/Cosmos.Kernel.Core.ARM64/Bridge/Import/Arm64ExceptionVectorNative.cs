using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Core.ARM64.Bridge;

/// <summary>
/// ARM64 exception vector setup import (writes VBAR_EL1 to install the
/// exception vector table defined in Cosmos.Kernel.Native.ARM64).
/// </summary>
internal static partial class Arm64ExceptionVectorNative
{
    [LibraryImport("*", EntryPoint = "_native_arm64_init_exception_vectors")]
    [SuppressGCTransition]
    public static partial void InitExceptionVectors();
}
