// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.Drivers;

/// <summary>Where an interrupt pipe is in its lifecycle; written under the controller's lock or in its handler.</summary>
internal enum XhciPipeState
{
    /// <summary>Transfers are queued and reports delivered.</summary>
    Running,

    /// <summary>A Reset Endpoint command is in flight after a transfer error.</summary>
    ResettingEndpoint,

    /// <summary>A Set TR Dequeue Pointer command is in flight, moving past the abandoned transfers; a stalled pipe waits here for its CLEAR_FEATURE too.</summary>
    SettingDequeuePointer,

    /// <summary>Recovery gave up, or the pipe was closed; no transfers are queued any more.</summary>
    Stopped,

    /// <summary>A close is under way: the handler neither re-queues its buffers nor starts a recovery.</summary>
    Closing
}
