using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Core.ARM64.Bridge;

internal static partial class GenericTimerNative
{
    [LibraryImport("*", EntryPoint = "_native_arm64_timer_get_frequency")]
    [SuppressGCTransition]
    public static partial ulong GetFrequency();

    [LibraryImport("*", EntryPoint = "_native_arm64_timer_get_counter")]
    [SuppressGCTransition]
    public static partial ulong GetCounter();

    [LibraryImport("*", EntryPoint = "_native_arm64_timer_set_compare")]
    [SuppressGCTransition]
    public static partial void SetCompare(ulong value);

    [LibraryImport("*", EntryPoint = "_native_arm64_timer_enable")]
    [SuppressGCTransition]
    public static partial void Enable();

    [LibraryImport("*", EntryPoint = "_native_arm64_timer_disable")]
    [SuppressGCTransition]
    public static partial void Disable();

    [LibraryImport("*", EntryPoint = "_native_arm64_timer_set_tval")]
    [SuppressGCTransition]
    public static partial void SetTval(uint ticks);

    [LibraryImport("*", EntryPoint = "_native_arm64_timer_get_ctl")]
    [SuppressGCTransition]
    public static partial uint GetCtl();
}
