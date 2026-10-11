// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;

namespace Cosmos.Kernel.HAL.DriverKit;

/// <summary>
/// Base class of every driver the kit can bind. A driver declares what it
/// matches, is offered nodes that match, and in <see cref="Probe"/> either
/// takes a node, acquiring what it needs through the binding, or passes.
/// <para>
/// The manifest constructs one instance per driver class, and that instance
/// is offered every node it matches, so fields on the driver are shared
/// across bindings: state that belongs to one device lives on objects the
/// driver creates in <see cref="Probe"/> and hangs off
/// <see cref="DeviceBinding.DriverState"/>. Probes, teardowns and work items
/// run one at a time on the kit worker; only driver threads run concurrently,
/// with each other and with the worker.
/// </para>
/// <para>
/// Execution contexts: <see cref="Probe"/> and <see cref="OnDetach"/> run in
/// thread context on the kit worker and may block through the binding.
/// Interrupt handlers requested through the binding run in interrupt
/// context and reach only what the binding says they may.
/// </para>
/// </summary>
[Experimental(Experimentals.DriverKitSeamDiagId)]
public abstract class Driver
{
    /// <summary>The driver's name, as it appears in the log and the diagnostics view.</summary>
    public abstract string Name { get; }

    /// <summary>The identities the driver wants to be offered.</summary>
    public abstract ReadOnlySpan<DeviceMatch> Matches { get; }

    /// <summary>
    /// Arbitration priority among drivers matching the same node: higher is
    /// offered first. Zero by default; a kernel's own driver overrides a
    /// framework one for the same hardware by returning more.
    /// </summary>
    public virtual int Priority => 0;

    /// <summary>
    /// Looks at the device behind <paramref name="binding"/> and decides. A
    /// bound result keeps the binding and everything acquired through it; a
    /// declined or failed result, or an exception, makes the kit release all
    /// of it before offering the node to the next candidate.
    /// </summary>
    /// <param name="binding">The device and the kit facilities for it; valid for the driver's use only after a bound result.</param>
    public abstract ProbeResult Probe(DeviceBinding binding);

    /// <summary>
    /// Called once when a bound device is being taken away, after its
    /// interrupts are disconnected and its threads stopped, before its
    /// windows and DMA memory are released. The one place a driver may
    /// quiesce hardware on the way out; nothing runs for it afterwards.
    /// </summary>
    /// <param name="binding">The binding being torn down; its windows are still valid.</param>
    /// <param name="reason">Why, and whether the hardware is still there.</param>
    public virtual void OnDetach(DeviceBinding binding, DetachReason reason)
    {
    }
}
