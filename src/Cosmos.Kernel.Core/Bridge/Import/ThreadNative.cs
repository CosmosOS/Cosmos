
using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Core.Bridge;

public static partial class ThreadNative
{
    [LibraryImport("*", EntryPoint = "GetTlsRegion")]
    [SuppressGCTransition]
    internal static unsafe partial uint GetTlsRegion(out byte* pTlsStart);

    [LibraryImport("*", EntryPoint = "_native_set_tls")]
    [SuppressGCTransition]
    public static partial void SetThreadLocalStorage(nint tls);
}