// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;
using SchedSpinLock = Cosmos.Kernel.Core.Scheduler.SpinLock;

namespace Cosmos.Kernel.HAL.DriverKit;

/// <summary>
/// A lock the kit sanctions for a driver whose device is entered from more
/// than one context at once: a transmit path the ring calls from its own
/// thread while the driver's work item, delivering a frame, re-enters it
/// through the stack's synchronous reply; a handler and a thread sharing a
/// register sequence. Created through <see cref="DeviceBinding.CreateLock"/>;
/// it holds no resource, is not counted among them and needs no release.
/// Any context: the holder runs with interrupts disabled, so a handler can
/// never spin on a thread that holds it.
/// </summary>
[Experimental(Experimentals.DriverKitSeamDiagId)]
public sealed class DeviceLock
{
    private SchedSpinLock _lock;

    internal DeviceLock()
    {
    }

    /// <summary>
    /// Takes the lock. Any context; the holder runs with interrupts disabled
    /// until the scope is disposed, so the lock is never held across
    /// <see cref="DeviceBinding.Sleep"/>, <see cref="DeviceBinding.Wait"/>,
    /// <see cref="DeviceBinding.Delay"/>, a sink call or a publish. A bus
    /// driver that drains its event ring under the lock may call a report
    /// handler there; such a handler is bound by the
    /// <see cref="Usb.UsbReportHandler"/> contract (allocation-free, sinks
    /// and Interlocked only), which is the one sanctioned sink call under a
    /// lock. Not reentrant: a holder that acquires again spins forever.
    /// </summary>
    /// <returns>The held state; a <c>using</c> binds to it and releases the lock.</returns>
    public DeviceLockScope Acquire() => new(_lock.AcquireIrqSafe());
}
