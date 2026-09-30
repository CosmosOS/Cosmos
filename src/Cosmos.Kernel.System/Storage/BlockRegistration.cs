// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.System.Storage;

/// <summary>
/// What <see cref="StorageManager.TryRegister"/> did with a block device,
/// so the kit's block consumer can refuse a device the manager could not
/// take and the publishing probe fails with that reason.
/// </summary>
internal enum BlockRegistration
{
    /// <summary>The device is now in the table and was scanned for partitions.</summary>
    Added,

    /// <summary>The same object is already registered; nothing changed.</summary>
    Duplicate,

    /// <summary>The table already holds as many devices as the manager can take.</summary>
    Full,

    /// <summary>Storage is compiled out, the manager is not initialized, or the device is null.</summary>
    Unavailable,
}
