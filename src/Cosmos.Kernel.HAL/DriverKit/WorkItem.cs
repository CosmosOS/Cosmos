// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.HAL.DriverKit.Engine;

namespace Cosmos.Kernel.HAL.DriverKit;

/// <summary>
/// A callback that runs in thread context on the kit worker, created through
/// <see cref="DeviceBinding.CreateWorkItem"/>. Scheduling it from a handler
/// is how interrupt work reaches a context that may block: the item is
/// queued at most once at a time and allocation-free, because it owns its
/// queue entry. Teardown cancels it; a cancelled item never runs again.
/// </summary>
[Experimental(Experimentals.DriverKitSeamDiagId)]
public sealed class WorkItem
{
    private readonly Action _callback;
    private readonly DeviceBinding? _binding;
    private volatile bool _cancelled;

    internal WorkItem(Action callback, DeviceBinding? binding)
    {
        _callback = callback;
        _binding = binding;
        Job = new EngineJob(EngineJobKind.RunWorkItem) { Item = this };
    }

    /// <summary>The item's queue entry.</summary>
    internal EngineJob Job { get; }

    /// <summary>The binding the item belongs to; null for the kit's own items.</summary>
    internal DeviceBinding? Binding => _binding;

    /// <summary>True once cancelled by teardown.</summary>
    public bool IsCancelled => _cancelled;

    /// <summary>
    /// Queues the item to run on the worker. Allocation-free; any context.
    /// </summary>
    /// <returns>False when the item is already queued, was cancelled, or no worker runs in this kernel.</returns>
    public bool Schedule()
    {
        if (_cancelled)
        {
            return false;
        }

        return DriverEngine.TryEnqueueWorkItem(Job);
    }

    /// <summary>Runs the callback. Worker only; an exception is the caller's to log.</summary>
    internal void Run()
    {
        if (_cancelled)
        {
            return;
        }

        _callback();
    }

    /// <summary>Stops the item from running again and unlinks it if queued. Teardown only.</summary>
    internal void Cancel()
    {
        _cancelled = true;
        DriverEngine.Dequeue(Job);
    }
}
