// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.Drivers.BuiltIn.Storage.UsbMassStorage.BulkOnly;

/// <summary>
/// The class requests of the Bulk-Only Transport (BOT 1.0 s3), sent on
/// the default pipe to the mass storage interface.
/// </summary>
internal enum BulkOnlyRequest : byte
{
    /// <summary>GET MAX LUN (s3.2): the highest logical unit number, in one byte.</summary>
    GetMaxLun = 0xFE,

    /// <summary>Bulk-Only Mass Storage Reset (s3.1): readies the interface for the next Command Block Wrapper.</summary>
    Reset = 0xFF
}
