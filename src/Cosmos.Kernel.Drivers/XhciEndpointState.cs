// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.Drivers;

/// <summary>Endpoint Context EP State field (xHCI 1.2 table 6-8), the one the controller keeps up to date.</summary>
internal enum XhciEndpointState : byte
{
    /// <summary>The endpoint is not configured.</summary>
    Disabled = 0,

    /// <summary>The endpoint runs transfers.</summary>
    Running = 1,

    /// <summary>An error halted the endpoint; Reset Endpoint recovers it.</summary>
    Halted = 2,

    /// <summary>A Stop Endpoint command stopped it.</summary>
    Stopped = 3,

    /// <summary>A Set TR Dequeue Pointer command is needed before it runs again.</summary>
    Error = 4
}
