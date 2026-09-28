// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit;

namespace Cosmos.Kernel.Tests.Drivers.Library;

/// <summary>
/// What <see cref="BlockingHandlerDriver"/> holds for one bound device: the
/// binding its handler wrongly sleeps through, the connected interrupt, and a
/// count of handler runs.
/// </summary>
public sealed class BlockingHandlerState
{
    private const uint SleepMilliseconds = 1;

    private readonly DeviceBinding _binding;
    private volatile int _handlerRuns;

    internal BlockingHandlerState(DeviceBinding binding)
    {
        _binding = binding;
    }

    /// <summary>The connected interrupt; set once the handler is requested.</summary>
    public InterruptHandle? Handle { get; internal set; }

    /// <summary>How many times the handler was entered.</summary>
    public int HandlerRuns => _handlerRuns;

    /// <summary>
    /// The handler's body: counts the run, then calls a thread-context member
    /// of the binding, which the kit's guard must refuse with an exception the
    /// trampoline records as a fault. Wants nothing from the interrupt context,
    /// which is the point: what it does is what a handler may not.
    /// </summary>
    public void OnInterrupt()
    {
        _handlerRuns++;
        _binding.Sleep(SleepMilliseconds);
    }
}
