// This code is licensed under MIT license (see LICENSE for details)

using System.Runtime.InteropServices;
using Cosmos.Kernel.Core.SysCalls;

namespace Cosmos.Kernel.Core.Bridge.Export;

/// <summary>
/// Native entry point invoked from the architecture-specific syscall trap
/// stubs (SYSCALL on x64, SVC on ARM64). Forwards straight into
/// <see cref="SysCallDispatcher.Dispatch"/> — no indirection, mirroring the
/// <c>Bridge/Export/IrqNative</c> IRQ pattern. The packed long return value
/// follows the C convention: <c>&gt;= 0</c> on success, <c>-(long)errno</c>
/// on failure (see <see cref="SysCallResult.Pack"/>).
/// </summary>
internal static unsafe class SysCallNative
{
    [UnmanagedCallersOnly(EntryPoint = "__managed__syscall")]
    public static long Dispatch(SysCallContext* context)
    {
        if (!CosmosFeatures.SysCallsEnabled)
        {
            Panic.Halt("SysCall invoked while SysCalls disabled");
        }

        SysCallResult result = SysCallDispatcher.Dispatch(ref *context);
        return result.Pack();
    }
}
