using Cosmos.Kernel.Boot.Limine;
using Cosmos.Kernel.Core.Bridge;

namespace Cosmos.Kernel.Core.Runtime;

/// <summary>
/// Bounds of the bootloader-provided stack that the boot/idle thread runs on.
/// The top is captured by kmain before any managed code executes; the size is
/// the Limine stack-size request when honored, else the 64 KiB protocol default.
/// </summary>
internal static unsafe class BootStack
{
    /// <summary>Stack size Limine guarantees when no stack-size request is honored.</summary>
    private const nuint LimineDefaultStackSize = 64 * 1024;

    /// <summary>Highest address of the boot stack (exclusive).</summary>
    public static nuint Top => BootNative.GetBootStackTop();

    /// <summary>Boot stack size in bytes.</summary>
    public static nuint Size => Limine.StackSize.Response != null
        ? (nuint)Limine.StackSize.StackSize
        : LimineDefaultStackSize;
}
