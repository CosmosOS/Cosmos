using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cosmos.Kernel.Core.Memory.GarbageCollector;
using Cosmos.Kernel.Core.Runtime.ExceptionHandling;

namespace Cosmos.Kernel.Core.Scheduler;

/// <summary>
/// Thread Control Block for scheduling. A kernel that only wants to read
/// thread state does not come here: <c>SchedulerDiagnostics</c> on the ring hands
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

    private uint _cpuId;
    private SchedulerThreadState _state;

    /// <summary>
    /// CPU this thread is assigned to: the CPU whose run structure holds it
    /// while it is <see cref="SchedulerThreadState.Ready"/>, the CPU it runs
    /// on while <see cref="SchedulerThreadState.Running"/>. Rewritten by a
    /// migration under both CPUs' queue locks, so a reader that needs the
    /// value to stay true takes the CPU's queue lock and reads it again, as
    /// the mechanism does; a bare read is a snapshot. Volatile: read on one
    /// CPU, written on another.
    /// </summary>
    public uint CpuId
    {
        get => Volatile.Read(ref _cpuId);
        internal set => Volatile.Write(ref _cpuId, value);
    }

    // ===== State =====

    /// <summary>
    /// Current lifecycle state. State transitions are performed by
    /// <see cref="SchedulerManager"/>; schedulers observe the state but do
    /// not change it. Volatile: a wake on one CPU writes it and the thread's
    /// park loop on another reads it, and the pair is what carries the wake
    /// across (the write after the policy queued the thread, the read before
    /// the parked thread goes on).
    /// </summary>
    public SchedulerThreadState State
    {
        get => (SchedulerThreadState)Volatile.Read(ref Unsafe.As<SchedulerThreadState, byte>(ref _state));
        internal set => Volatile.Write(ref Unsafe.As<SchedulerThreadState, byte>(ref _state), (byte)value);
    }

    /// <summary>
    /// Thread attribute flags.
    /// </summary>
    public SchedulerThreadFlags Flags { get; internal set; }

    /// <summary>
    /// Whether this is a CPU's idle thread: the boot thread itself on the boot CPU, the
    /// scheduler's fallback when <see cref="IScheduler.PickNext"/> returns null. The blocking
    /// primitives make it spin or poll instead of parking, since a parked idle thread comes
    /// back through that fallback as Running without the wake that returns its tickets.
    /// </summary>
    public bool IsIdle => (Flags & SchedulerThreadFlags.IdleThread) != 0;

    // ===== Affinity =====

    /// <summary>
    /// The CPUs this thread may run on. Never empty: a new thread takes its creator's set,
    /// or the kernel's default when the creator is an idle thread, and a kernel changes it
    /// through the ring. Written under the queue lock of the thread's CPU and read under it
    /// wherever it decides a move (the switch-out, a set change, the creation); a bare read
    /// is a snapshot. A set that names no online CPU is treated as every online CPU.
    /// </summary>
    public CpuSet Affinity { get; internal set; } = CpuSet.Single(0);

    /// <summary>
    /// The CPU the policy prefers for this thread, always inside <see cref="Affinity"/>: the
    /// CPU the thread was created on until a kernel sets it, and the CPU the thread moves to
    /// when a set change excludes it. What <see cref="IScheduler.SelectCpu"/> reads first.
    /// </summary>
    public uint IdealCpu { get; internal set; }

    /// <summary>
    /// 1 while a CPU has staged a switch to this thread or still runs on its stack, 0 once
    /// that CPU's interrupt stub has switched to another stack. The resuming CPU sets it
    /// after its queue lock is released, having waited for 0; the stub clears it through
    /// <see cref="CPU.PerCpuBlock.SwitchPrevOnCpu"/>. The thread's state says nothing about
    /// this: a thread turns Ready, Blocked or Sleeping before its CPU's handler has returned
    /// into the stub, so a resume on another CPU waits on this word, not on the state.
    /// </summary>
    internal ulong _onCpu;

    /// <summary>Whether a CPU is still on this thread's stack (<see cref="_onCpu"/>).</summary>
    internal bool IsOnCpu => Volatile.Read(ref _onCpu) != 0;

    /// <summary>
    /// The address of <see cref="_onCpu"/>: what the switch-out stages into the per-CPU
    /// block for the stub to clear. Stable: the collector does not move objects.
    /// </summary>
    internal nuint OnCpuSlot => (nuint)Unsafe.AsPointer(ref _onCpu);

    // ===== Context (architecture-specific values) =====

    /// <summary>
    /// Saved stack pointer while the thread is not running.
    /// </summary>
    public nuint StackPointer { get; internal set; }

    /// <summary>
    /// Entry point address the thread was created with. Recorded by
    /// <see cref="InitializeStack"/> for a debugger reading the control
    /// block; the initial context carries its own copy, so nothing on the
    /// switch path reads this one.
    /// </summary>
    internal nuint InstructionPointer { get; set; }

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
    /// Timestamp at which the thread last became the current thread. Kept by
    /// the manager; a policy that wants to age threads keeps its own stamp on
    /// <see cref="SchedulerExtensible.SchedulerData"/> rather than reading
    /// this one.
    /// </summary>
    internal ulong LastScheduledAt { get; set; }

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
    internal GCHandle<System.Threading.Thread> _managedThread;

    /// <summary>
    /// Backing store for the thread's <c>[ThreadStatic]</c> fields, handed to
    /// CoreLib by reference. It starts null on purpose: CoreLib's
    /// <c>ThreadStatics</c> tests the reference and allocates the first
    /// module's row itself, so a thread that never touches a thread static
    /// never pays for one.
    /// </summary>
    private object[][]? _threadStaticStorage;

    /// <summary>
    /// The exception dispatcher's state for this thread: the chain of its in-flight throws, its
    /// dispatch guard and the funclets running on it, handed out by reference through
    /// <see cref="GetExceptionDispatchState"/>. The boot code, before it is a scheduler thread,
    /// uses the dispatcher's own.
    /// </summary>
    private ExceptionDispatchState _exceptionDispatchState;

    /// <summary>
    /// Default stack size for new threads (256KB). Must stay above CoreLib's
    /// 128KB MinExecutionStackSize (64-bit) or
    /// RuntimeHelpers.EnsureSufficientExecutionStack, called by generated
    /// record ToString/PrintMembers among others, throws
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

    /// <summary>
    /// Frees the stack <see cref="InitializeStack"/> allocated, for a thread the registry
    /// refused before it ever ran. Nothing of the thread lives on that stack yet.
    /// </summary>
    internal void ReleaseStack()
    {
        if (StackBase != 0)
        {
            Memory.MemoryOp.Free((void*)StackBase);
        }

        StackBase = 0;
        StackSize = 0;
        StackPointer = 0;
    }

    /// <summary>
    /// Hands CoreLib the thread's thread-static storage slot by reference so
    /// it can allocate and grow it in place.
    /// </summary>
    internal ref object[][]? GetThreadStaticStorage()
    {
        return ref _threadStaticStorage;
    }

    /// <summary>
    /// Hands the exception dispatcher this thread's state by reference.
    /// </summary>
    internal ref ExceptionDispatchState GetExceptionDispatchState()
    {
        return ref _exceptionDispatchState;
    }

    /// <summary>
    /// The address of this thread's ExInfo chain head slot
    /// (<see cref="ExceptionDispatchState.ExInfoHead"/>): what the scheduler writes into the
    /// per-CPU block's <c>CurrentExInfoHead</c> when the thread becomes current, so the native
    /// stubs link a throw into this thread's chain. Stable: the collector does not move objects.
    /// </summary>
    internal nuint ExInfoHeadSlot => (nuint)Unsafe.AsPointer(ref _exceptionDispatchState.ExInfoHead);

    /// <summary>
    /// Gets a pointer to the thread's saved context.
    /// Only valid when thread is not running.
    /// </summary>
    internal ThreadContext* GetContext()
    {
        return (ThreadContext*)StackPointer;
    }
}
