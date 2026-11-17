// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.Drivers.Pci.Bus.Ohci;

/// <summary>Where an interrupt pipe is in its lifecycle; written under the controller's lock or in its handler.</summary>
internal enum OhciPipeState
{
    /// <summary>Transfers are queued and reports delivered.</summary>
    Running,

    /// <summary>A transfer error halted the endpoint; the hot-plug thread recovers it.</summary>
    Recovering,

    /// <summary>Recovery gave up, or the pipe was closed; no transfers are queued any more.</summary>
    Stopped,

    /// <summary>A close is under way: the drain neither re-queues its buffers nor starts a recovery.</summary>
    Closing
}
