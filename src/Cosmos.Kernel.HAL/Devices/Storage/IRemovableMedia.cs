// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;

namespace Cosmos.Kernel.HAL.Devices.Storage;

/// <summary>
/// A block device whose medium is read-only, removable, or both: a CD-ROM
/// drive, a write-protected card. Optional facet on an
/// <see cref="IBlockDevice"/>, found by a type test on the published
/// object. The storage manager reads it to carry a read-only medium as one
/// whole-medium partition without scanning it for a table, and to order it
/// after every writable disk; a kernel reads it to tell a CD from a disk.
/// Any context.
/// </summary>
[Experimental(Experimentals.DriverKitSeamDiagId)]
public interface IRemovableMedia
{
    /// <summary>True when the medium takes no writes: <see cref="IBlockDevice.WriteBlock"/> throws <see cref="IOException"/>.</summary>
    bool IsReadOnly { get; }

    /// <summary>True when the medium can be taken out of the device: a CD, not a fixed disk.</summary>
    bool IsRemovable { get; }

    /// <summary>True when a medium is in the device, as far as the driver knows: a driver that does not watch for the medium being taken out reports what it found when it brought the device up. A read on a device without one throws <see cref="IOException"/>.</summary>
    bool HasMedia { get; }
}
