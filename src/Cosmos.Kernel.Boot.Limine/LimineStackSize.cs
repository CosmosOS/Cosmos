using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Boot.Limine;

/// <summary>
/// Limine Stack Size request.
/// Asks the bootloader to provide at least <see cref="StackSize"/> bytes of
/// stack to the kernel entry point instead of the 64 KiB protocol default.
/// The boot stack hosts the whole main kernel thread, and
/// <c>RuntimeHelpers.EnsureSufficientExecutionStack</c> requires stacks larger
/// than 128 KiB on 64-bit to ever succeed. The loader gives every application
/// processor it starts (the MP request) a stack of the same size
/// (<c>common/protos/limine.c</c> in the v10 tree), so this one number sizes
/// the boot stack of every CPU.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public readonly unsafe struct LimineStackSizeRequest(ulong stackSize)
{
    /// <summary>Boot stack size requested by Cosmos (1 MiB, the .NET main-thread convention).</summary>
    public const ulong DefaultRequestedSize = 1024 * 1024;

    public readonly LimineID ID = new(0x224ef0460a8e8926, 0xe1cb0fc25f46ea3d);
    public readonly ulong Revision = 0;
    public readonly LimineStackSizeResponse* Response;

    /// <summary>Requested stack size in bytes; honored when <see cref="Response"/> is non-null.</summary>
    public readonly ulong StackSize = stackSize;
}

[StructLayout(LayoutKind.Sequential)]
public readonly struct LimineStackSizeResponse
{
    public readonly ulong Revision;
}
