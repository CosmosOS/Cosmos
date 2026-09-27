// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.Drivers.BuiltIn.Storage.UsbMassStorage.Scsi;

/// <summary>
/// The SCSI operation codes this driver issues (SPC-4, SBC-3): the first
/// byte of every command block.
/// </summary>
internal enum ScsiOpcode : byte
{
    /// <summary>TEST UNIT READY: whether the unit can take a medium access command.</summary>
    TestUnitReady = 0x00,

    /// <summary>REQUEST SENSE: the sense data of the command that failed last.</summary>
    RequestSense = 0x03,

    /// <summary>INQUIRY: the standard inquiry data, which says what kind of device the unit is.</summary>
    Inquiry = 0x12,

    /// <summary>READ CAPACITY(10): the last LBA and the block length, while the LBA fits 32 bits.</summary>
    ReadCapacity10 = 0x25,

    /// <summary>READ(10).</summary>
    Read10 = 0x28,

    /// <summary>WRITE(10).</summary>
    Write10 = 0x2A,

    /// <summary>SYNCHRONIZE CACHE(10): commit the unit's volatile cache to the medium.</summary>
    SynchronizeCache10 = 0x35,

    /// <summary>READ(16).</summary>
    Read16 = 0x88,

    /// <summary>WRITE(16).</summary>
    Write16 = 0x8A,

    /// <summary>SERVICE ACTION IN(16), whose READ CAPACITY(16) service action reads a unit past 2^32 blocks.</summary>
    ServiceActionIn16 = 0x9E
}
