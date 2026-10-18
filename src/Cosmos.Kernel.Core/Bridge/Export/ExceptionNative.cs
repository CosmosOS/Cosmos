using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cosmos.Kernel.Core.Runtime.ExceptionHandling;

namespace Cosmos.Kernel.Core.Bridge;

/// <summary>
/// Bridge function that hands the native exception stubs the running thread's dispatcher state.
/// </summary>
internal static unsafe class ExceptionNative
{
    /// <summary>
    /// The slot holding the head of the running thread's ExInfo chain: <c>RhpThrowEx</c> links
    /// a throw into it, <c>RhpCallCatchFunclet</c> unlinks the throws a catch ends, and
    /// <c>RhpRethrow</c> reads the exception a <c>throw;</c> rethrows from it. The slot lives in
    /// the thread's <see cref="ExceptionDispatchState"/>, which the garbage collector does not
    /// move.
    /// </summary>
    [UnmanagedCallersOnly(EntryPoint = "__cosmos_exinfo_head")]
    public static nuint* GetExInfoHead()
    {
        return (nuint*)Unsafe.AsPointer(ref ExceptionHelper.State.ExInfoHead);
    }
}
