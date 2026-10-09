using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.Core.Memory;
using Cosmos.Kernel.Core.Memory.VAS;

namespace Cosmos.Kernel.Core.Scheduler;

/// <summary>
/// A ring-0 / EL1 user-space process. The process itself does not execute;
/// execution happens through its threads. The caller is responsible for building
/// the address space, mapping memory, and creating threads.
/// </summary>
public class Process
{
    /// <summary>
    /// Unique process identifier.
    /// </summary>
    public ulong Id { get; internal set; }

    /// <summary>
    /// Virtual address space shared by all threads in this process.
    /// </summary>
    public AddressSpace? AddressSpace { get; set; }

    /// <summary>
    /// Privilege level the process threads run at. 0 = kernel / ring 0 / EL1.
    /// 3 = user / ring 3 / EL0. Only ring 0 is supported
    /// all threads started by this process must be in the same ring.
    /// </summary>
    public byte Ring { get; set; }

    /// <summary>
    /// Next free user-space stack top, carved downward from
    /// <see cref="UserStackBase"/>. Each ring-3 thread's stack is mapped at a
    /// fresh VA so concurrent threads don't alias. Page-aligned; PoC: never
    /// recycled (released wholesale when the address space dies).
    /// </summary>
    private ulong _userStackCursor = UserStackBase;

    /// <summary>
    /// Base of the user stack region - well below the x64 canonical user limit
    /// (<c>0x0000800000000000</c> used by <c>PageFaultHandler</c>) and below
    /// any typical process image layout, so stacks grow downward into free
    /// space. ARM64's low-half user region is unbounded, so the same base is
    /// fine on both architectures.
    /// </summary>
    private const ulong UserStackBase = 0x0000400000000000UL;

    /// <summary>
    /// Allocates <paramref name="size"/> bytes of user virtual address space
    /// for a ring-3 thread stack, returning the (exclusive) top VA. The
    /// caller maps physical pages against this range and points the thread's
    /// RSP/SP at the returned top. Page-aligned; PoC: never recycled.
    /// </summary>
    public nuint AllocateUserStack(nuint size)
    {
        // Page-align the size up; carve downward from the cursor.
        ulong pages = ((ulong)size + PageAllocator.PageSize - 1) / PageAllocator.PageSize;
        ulong bytes = pages * PageAllocator.PageSize;
        _userStackCursor -= bytes;
        // Page-align the cursor top (it already is - bytes is a multiple of
        // the page size - but keep the invariant explicit).
        _userStackCursor &= ~(ulong)(PageAllocator.PageSize - 1);
        return (nuint)_userStackCursor;
    }

    /// <summary>
    /// Threads belonging to this process.
    /// </summary>
    public List<SchedulerThread> Threads { get; } = new List<SchedulerThread>();

    /// <summary>
    /// Process state.
    /// </summary>
    public ProcessState State { get; set; }

    /// <summary>
    /// Exit code when the process terminates.
    /// </summary>
    public int ExitCode { get; private set; }

    public void StartThread(SchedulerThread thread)
    {
        thread.AddressSpace = AddressSpace;

        Threads.Add(thread);
    }

    public void Kill(int exitCode)
    {
        ExitCode = exitCode;
        State = ProcessState.Dead;

        Serial.WriteString("[ProcessManager] Process ");
        Serial.WriteNumber(Id);
        Serial.WriteString(" terminated with code ");
        Serial.WriteNumber((uint)exitCode);
        Serial.WriteString("\n");

        foreach (SchedulerThread thread in Threads)
        {
            SchedulerManager.ExitThread(thread.CpuId, thread); // kill them all
        }

        AddressSpace?.ReleaseReference();
        AddressSpace = null;

    }

}



/// <summary>
/// Lifecycle state of a process.
/// </summary>
public enum ProcessState : byte
{
    /// <summary>Process is being constructed.</summary>
    Created,

    /// <summary>Process is running (has at least one runnable thread).</summary>
    Running,

    /// <summary>Process is being terminated.</summary>
    Dying,

    /// <summary>Process has exited.</summary>
    Dead
}
