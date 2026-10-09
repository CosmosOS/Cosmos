// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.DriverKit.Engine;

/// <summary>What an <see cref="EngineJob"/> asks the worker to do.</summary>
internal enum EngineJobKind
{
    /// <summary>Offer a newly published node to the matching drivers.</summary>
    NodeArrived,

    /// <summary>Tear a node down: its children, its binding, then mark it retracted.</summary>
    NodeRetracted,

    /// <summary>Run a driver's work item.</summary>
    RunWorkItem,

    /// <summary>Run nothing: a marker whose completion says every job queued before it has run.</summary>
    Fence,
}
