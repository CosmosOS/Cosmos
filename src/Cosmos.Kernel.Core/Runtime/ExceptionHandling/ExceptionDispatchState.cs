using System.Runtime.CompilerServices;

namespace Cosmos.Kernel.Core.Runtime.ExceptionHandling;

/// <summary>
/// What the exception dispatcher keeps for one thread while it throws, unwinds and catches: the
/// head of the ExInfo chain <c>RhpThrowEx</c> links every throw into, the guard against a throw
/// from inside a dispatch, and the funclets running on top of the dispatcher (see
/// <see cref="ExceptionHelper"/>). A thread dispatches on its own stack, so none of it is the
/// kernel's: shared, a thread preempted inside a dispatch made the next thread's throw a
/// "recursive" one, and a <c>throw;</c> rethrew whichever thread had thrown last
/// (valentinbreiz/nativeaot-patcher#440). Each <see cref="Scheduler.SchedulerThread"/> carries
/// one, and <see cref="ExceptionHelper.State"/> finds the running thread's, or the boot code's
/// before it is a scheduler thread; the per-CPU block's
/// <see cref="CPU.PerCpuBlock.CurrentExInfoHead"/> points the native stubs at the chain head of
/// whichever one is the running code's.
/// </summary>
internal struct ExceptionDispatchState
{
    /// <summary>
    /// Funclets the dispatcher tracks at once: funclets nest, as a catch that throws from its
    /// handler runs the next catch on top of itself.
    /// </summary>
    public const int MaxRunningFunclets = 64;

    /// <summary>
    /// The innermost in-flight ExInfo of the thread, the head of the chain through each one's
    /// <c>m_pPrevExInfo</c>. The native stubs reach it in one load through the per-CPU block
    /// (<see cref="CPU.PerCpuBlock.CurrentExInfoHead"/>, which the scheduler points at the
    /// running thread's): <c>RhpThrowEx</c> links a throw in, <c>RhpCallCatchFunclet</c> unlinks
    /// the throws a catch ends, <c>RhpRethrow</c> reads the exception a <c>throw;</c> rethrows
    /// from it.
    /// </summary>
    public nuint ExInfoHead;

    /// <summary>Whether the thread is inside a dispatch: a throw there is fatal.</summary>
    public bool IsHandlingException;

    /// <summary>The funclets running on top of the dispatcher, innermost last.</summary>
    public RunningFuncletTable Funclets;

    /// <summary>How many of <see cref="Funclets"/> are in use.</summary>
    public int FuncletCount;
}

/// <summary>
/// A funclet running on top of the dispatcher: which clause of which frame it runs, entered by
/// which throw.
/// </summary>
internal struct RunningFunclet
{
    /// <summary>The frame pointer of the frame whose clause the funclet runs.</summary>
    public nuint FramePointer;

    /// <summary>The index of that clause in the frame's clause table.</summary>
    public int ClauseIndex;

    /// <summary>The ExInfo of the throw that entered the funclet.</summary>
    public nuint ExInfo;
}

/// <summary>
/// The <see cref="RunningFunclet"/> records a thread carries, inline so that recording one
/// allocates nothing.
/// </summary>
[InlineArray(ExceptionDispatchState.MaxRunningFunclets)]
internal struct RunningFuncletTable
{
    private RunningFunclet _element0;
}
