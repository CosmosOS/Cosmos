using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Cosmos.Kernel.Core.Memory.GarbageCollector;

namespace Cosmos.Kernel.Core.Scheduler;

/// <summary>
/// Thread Control Block for scheduling. A kernel that only wants to read
/// thread state does not come here: <c>SchedulerInfo</c> on the ring hands
/// out a <c>KernelThreadInfo</c> snapshot of the fields that survive an
/// unlocked read, and reaching this type means acknowledging the seam's
/// diagnostic first.
/// </summary>
[Experimental(Experimentals.SchedulerSeamDiagId)]
public sealed unsafe class SchedulerThread : SchedulerExtensible
{
    /// <summary>
    /// Threads are created by <see cref="SchedulerManager"/>, which allocates
    /// the stack and the id. A scheduler receives one through its
    /// <see cref="IScheduler"/> hooks and attaches its own state to
    /// <see cref="SchedulerExtensible.SchedulerData"/>.
    /// </summary>
    internal SchedulerThread()
    {
    }

    // ===== Identity =====

    /// <summary>
    /// Unique thread identifier. The idle thread has ID 0.
    /// </summary>
    public uint Id { get; internal set; }

    /// <summary>
    /// CPU this thread is assigned to.
    /// </summary>
    public uint CpuId { get; internal set; }

    // ===== State =====

    /// <summary>
    /// Current lifecycle state. State transitions are performed by
    /// <see cref="SchedulerManager"/>; schedulers observe the state but do
    /// not change it.
    /// </summary>
    public SchedulerThreadState State { get; internal set; }

    /// <summary>
    /// Thread attribute flags.
    /// </summary>
    public SchedulerThreadFlags Flags { get; internal set; }

    // ===== Context (architecture-specific values) =====

    /// <summary>
    /// Saved stack pointer while the thread is not running.
    /// </summary>
    public nuint StackPointer { get; internal set; }

    /// <summary>
    /// Entry point address the thread was created with.
    /// </summary>
    public nuint InstructionPointer { get; internal set; }

    /// <summary>
    /// Lowest address of the thread's stack allocation.
    /// </summary>
    public nuint StackBase { get; internal set; }

    /// <summary>
    /// Size of the thread's stack in bytes.
    /// </summary>
    public nuint StackSize { get; internal set; }

    // ===== Generic Timing =====

    /// <summary>
    /// Accumulated CPU time in nanoseconds. The active scheduler charges
    /// elapsed time to the current thread from its
    /// <see cref="IScheduler.OnTick"/> hook.
    /// </summary>
    public ulong TotalRuntime { get; set; }

    /// <summary>
    /// Timestamp at which the thread last became the current thread.
    /// </summary>
    public ulong LastScheduledAt { get; internal set; }

    /// <summary>
    /// Deadline of the current timed wait, in timestamp ticks. Zero when
    /// the thread is not sleeping.
    /// </summary>
    public ulong WakeupTime { get; internal set; }

    // ===== GC Allocation Context (TLAB) =====
    internal AllocContext _allocContext;

    /// <summary>
    /// Strong handle to the <see cref="System.Threading.Thread"/> this
    /// control block runs, taken when the thread is created and released
    /// by <see cref="SchedulerManager.ExitThread"/>. CoreLib frees its own
    /// handle as soon as the thread reports itself started, so this is the
    /// only reference the mechanism keeps: it is what lets a thread that is
    /// killed while queued be stopped on the managed side too. Unallocated
    /// on a thread that runs a free delegate.
    /// </summary>
    internal GCHandle<System.Threading.Thread> ManagedThread;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private object[][] _threadStaticStorage;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.


    /// <summary>
    /// Default stack size for new threads (256KB). Must stay above CoreLib's
    /// 128KB MinExecutionStackSize (64-bit) or
    /// RuntimeHelpers.EnsureSufficientExecutionStack — called by generated
    /// record ToString/PrintMembers among others — throws
    /// InsufficientExecutionStackException on every call (#433).
    /// </summary>
    public const nuint DefaultStackSize = 256 * 1024;

    /// <summary>
    /// Maximum number of threads tracked by the global thread registry.
    /// </summary>
    public const int MaxThreadCount = 256;

    /// <summary>
    /// Allocates and initializes the thread stack with initial context.
    /// After this call, the thread is ready to be scheduled.
    /// </summary>
    /// <param name="entryPoint">Thread entry point function address.</param>
    /// <param name="codeSegment">Code segment selector (CS).</param>
    /// <param name="arg">Optional argument passed to entry point.</param>
    /// <param name="stackSize">Stack size in bytes.</param>
    internal void InitializeStack(nuint entryPoint, ushort codeSegment, nuint arg = 0, nuint stackSize = DefaultStackSize)
    {
        // Allocate stack memory
        StackSize = stackSize;
        StackBase = (nuint)Memory.MemoryOp.Alloc((uint)stackSize);

        // Stack layout (growing downward from top):
        // [StackBase + stackSize] = Top of usable stack
        // ... usable stack space for function calls ...
        // [contextAddr + ThreadContext.Size] = End of context
        // [contextAddr] = Start of ThreadContext (where StackPointer points)
        // [StackBase] = Bottom of stack

        nuint stackTop = StackBase + stackSize;

        // Place ThreadContext at the BOTTOM of the stack
        // The usable stack space is above it
        nuint contextAddr = StackBase;

        // Align context to 16 bytes (required for XMM operations)
        contextAddr = (contextAddr + 0xF) & ~(nuint)0xF;

        // Calculate usable stack top (above the context)
        nuint usableStackTop = stackTop;

        // Initialize the context with the usable stack top
        ThreadContext* context = (ThreadContext*)contextAddr;
        context->Initialize(entryPoint, codeSegment, arg, usableStackTop);

        // The StackPointer points to the start of the context
        // (where XMM registers are, as expected by the IRQ stub)
        StackPointer = contextAddr;
        InstructionPointer = entryPoint;
        State = SchedulerThreadState.Created;
    }

    internal ref object[][] GetThreadStaticStorage()
    {
        return ref _threadStaticStorage;
    }

    /// <summary>
    /// Gets a pointer to the thread's saved context.
    /// Only valid when thread is not running.
    /// </summary>
    internal ThreadContext* GetContext()
    {
        return (ThreadContext*)StackPointer;
    }
}
