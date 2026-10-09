using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Core.X64.Bridge;

internal static unsafe partial class IdtNative
{
    [LibraryImport("*", EntryPoint = "_native_x64_load_idt")]
    [SuppressGCTransition]
    public static partial void LoadIdt(void* ptr);

    [LibraryImport("*", EntryPoint = "_native_x64_get_code_selector")]
    [SuppressGCTransition]
    public static partial ulong GetCurrentCodeSelector();

    [LibraryImport("*", EntryPoint = "_native_x64_get_irq_stub")]
    [SuppressGCTransition]
    public static partial nint GetIrqStub(int index);

    [LibraryImport("*", EntryPoint = "_native_x64_get_last_error_code")]
    [SuppressGCTransition]
    public static partial ulong GetLastErrorCode();
}
