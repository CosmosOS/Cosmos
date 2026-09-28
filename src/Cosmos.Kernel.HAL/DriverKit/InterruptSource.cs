// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;

namespace Cosmos.Kernel.HAL.DriverKit;

/// <summary>
/// One interrupt a <see cref="DeviceNode"/> can deliver: a legacy line, one
/// message of an MSI or MSI-X block, or a synthetic source a test raises.
/// A driver picks one by index from <see cref="DeviceNode.Interrupts"/> and
/// asks the binding to connect a handler; the source knows how to route it
/// on its platform, and how to mask it at the controller. A bus implements
/// the protected members; the kit calls them through the internal ones, so a
/// driver holding a source can neither connect nor mask it behind the kit's
/// back. Every implementation keeps its connection state under one IRQ-safe
/// lock, so a raise cannot see a half-built connection and a handler never
/// runs after <see cref="Disconnect"/> returned.
/// </summary>
[Experimental(Experimentals.DriverKitSeamDiagId)]
public abstract class InterruptSource
{
    /// <summary>The source in words, for the log: "line 11", "message 0 of 4", "synthetic 0".</summary>
    public abstract string Describe();

    /// <summary>
    /// Routes the source to <paramref name="trampoline"/>, which runs the
    /// driver's handler on every delivery from the moment this returns true.
    /// </summary>
    /// <param name="trampoline">The kit's dispatcher for the handler; the bus calls its <see cref="InterruptTrampoline.Invoke"/> with interrupts masked.</param>
    /// <returns>False when the platform cannot deliver the source (no routing, no vector left), or it is already connected.</returns>
    protected abstract bool TryConnectCore(InterruptTrampoline trampoline);

    /// <summary>Stops deliveries at the controller until <see cref="UnmaskCore"/>. Allocation-free; any context.</summary>
    protected abstract void MaskCore();

    /// <summary>Lets deliveries through again. Allocation-free; any context.</summary>
    protected abstract void UnmaskCore();

    /// <summary>Masks the source and undoes <see cref="TryConnectCore"/>; the handler never runs again once this returns.</summary>
    protected abstract void DisconnectCore();

    /// <summary>Kit side of <see cref="TryConnectCore"/>; called by the binding, never by a driver.</summary>
    internal bool TryConnect(InterruptTrampoline trampoline) => TryConnectCore(trampoline);

    /// <summary>Kit side of <see cref="MaskCore"/>; called through the handle.</summary>
    internal void Mask() => MaskCore();

    /// <summary>Kit side of <see cref="UnmaskCore"/>; called through the handle.</summary>
    internal void Unmask() => UnmaskCore();

    /// <summary>Kit side of <see cref="DisconnectCore"/>; called by teardown.</summary>
    internal void Disconnect() => DisconnectCore();
}
