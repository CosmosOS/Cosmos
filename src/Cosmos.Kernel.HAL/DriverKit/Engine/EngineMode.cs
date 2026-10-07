// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.DriverKit.Engine;

/// <summary>How the driver engine runs its jobs. Decided once, by <see cref="DriverEngine.Start"/>.</summary>
internal enum EngineMode
{
    /// <summary><see cref="DriverEngine.Start"/> has not run; jobs queue up for it.</summary>
    NotStarted,

    /// <summary>A kit worker thread runs the jobs; callers wait on completions.</summary>
    Worker,

    /// <summary>No scheduler, or the worker did not start: the thread that publishes or retracts drains the queue itself.</summary>
    Inline,
}
