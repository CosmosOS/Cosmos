using System.Runtime.InteropServices;
using Cosmos.Kernel.Core.CPU;

namespace Cosmos.Kernel.Core.Bridge;

/// <summary>
/// Native entry point invoked from the architecture-specific IRQ assembly stubs.
/// Forwards straight into <see cref="InterruptManager.Dispatch"/> — no indirection.
/// </summary>
internal static unsafe class IrqNative
{
    [UnmanagedCallersOnly(EntryPoint = "__managed__irq")]
    public static void Handler(IRQContext* ctx)
    {
        InterruptManager.Dispatch(ref *ctx);
    }
}
