// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.Drivers.Pci.Bus.Ohci;

/// <summary>Completion codes a retired TD carries in its CC field (OHCI 1.0a table 4-7).</summary>
internal enum OhciConditionCode : byte
{
    /// <summary>The TD completed.</summary>
    NoError = 0,

    /// <summary>The CRC of the last packet failed.</summary>
    Crc = 1,

    /// <summary>A bit stuffing violation.</summary>
    BitStuffing = 2,

    /// <summary>The data toggle did not match what the endpoint expected.</summary>
    DataToggleMismatch = 3,

    /// <summary>The device answered STALL.</summary>
    Stall = 4,

    /// <summary>The device did not answer (a pulled device fails this way).</summary>
    DeviceNotResponding = 5,

    /// <summary>The PID check of a received packet failed.</summary>
    PidCheckFailure = 6,

    /// <summary>A PID the host did not expect.</summary>
    UnexpectedPid = 7,

    /// <summary>The device sent more than the packet size allows.</summary>
    DataOverrun = 8,

    /// <summary>The device sent less than the packet size with buffer rounding off.</summary>
    DataUnderrun = 9,

    /// <summary>The host could not write the data to memory in time.</summary>
    BufferOverrun = 12,

    /// <summary>The host could not read the data from memory in time.</summary>
    BufferUnderrun = 13,

    /// <summary>The controller never touched the TD: the value every TD the driver builds starts with.</summary>
    NotAccessed = 15
}
