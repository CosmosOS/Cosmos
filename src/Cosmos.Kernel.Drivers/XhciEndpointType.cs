// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.Drivers;

/// <summary>Endpoint Context EP Type field (xHCI 1.2 table 6-9).</summary>
internal enum XhciEndpointType : byte
{
    /// <summary>Isochronous OUT.</summary>
    IsochOut = 1,

    /// <summary>Bulk OUT.</summary>
    BulkOut = 2,

    /// <summary>Interrupt OUT.</summary>
    InterruptOut = 3,

    /// <summary>The default control endpoint.</summary>
    Control = 4,

    /// <summary>Isochronous IN.</summary>
    IsochIn = 5,

    /// <summary>Bulk IN.</summary>
    BulkIn = 6,

    /// <summary>Interrupt IN.</summary>
    InterruptIn = 7
}
