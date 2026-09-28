// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;

namespace Cosmos.Kernel.HAL.DriverKit;

/// <summary>
/// The whole of what an <see cref="InterruptHandler"/> may do besides
/// touching its registers and DMA memory. Every member is allocation-free
/// and never blocks; the handler has no other route to the kit, which is how
/// blocking from interrupt context stays unreachable by type.
/// </summary>
[Experimental(Experimentals.DriverKitSeamDiagId)]
public sealed class InterruptContext
{
    private readonly InterruptHandle _handle;

    internal InterruptContext(InterruptHandle handle)
    {
        _handle = handle;
    }

    /// <summary>
    /// Masks this source at its controller, for a level-triggered device
    /// whose condition a thread will clear; the thread unmasks through the
    /// <see cref="InterruptHandle"/>.
    /// </summary>
    public void Mask() => _handle.Mask();

    /// <summary>Wakes a thread waiting on <paramref name="evt"/>.</summary>
    /// <param name="evt">An event the binding created.</param>
    public void Signal(DeviceEvent evt) => evt.Signal();

    /// <summary>Queues <paramref name="item"/> to run on the kit worker.</summary>
    /// <param name="item">A work item the binding created.</param>
    /// <returns>False when the item is already queued or was cancelled.</returns>
    public bool Schedule(WorkItem item) => item.Schedule();
}
