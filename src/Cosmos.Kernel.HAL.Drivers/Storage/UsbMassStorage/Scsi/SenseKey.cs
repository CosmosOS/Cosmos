// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.Drivers.BuiltIn.Storage.UsbMassStorage.Scsi;

/// <summary>
/// The sense keys this driver acts on (SPC-4 s4.5.6): the general reason
/// a command failed, from the unit's sense data.
/// </summary>
internal enum SenseKey : byte
{
    /// <summary>The unit cannot be accessed now, such as a medium that is spinning up or missing.</summary>
    NotReady = 0x02,

    /// <summary>The command, or one of its fields, is not supported.</summary>
    IllegalRequest = 0x05,

    /// <summary>The unit reports an event, such as a reset or a medium change, and failed the command to say so.</summary>
    UnitAttention = 0x06
}
