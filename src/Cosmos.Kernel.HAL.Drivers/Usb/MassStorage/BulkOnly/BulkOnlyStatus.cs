// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.Drivers.BuiltIn.Usb.MassStorage.BulkOnly;

/// <summary>How a command ended on the Bulk-Only Transport.</summary>
internal enum BulkOnlyStatus
{
    /// <summary>The device ran the command.</summary>
    Passed,

    /// <summary>The device reports the command failed; its sense data says why.</summary>
    Failed,

    /// <summary>The command did not make it through the bus; the device was reset, and is ready for the next one.</summary>
    TransportError,

    /// <summary>The device left the bus: nothing reaches it any more.</summary>
    Disconnected
}
