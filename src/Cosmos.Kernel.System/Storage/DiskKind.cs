// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.System.Storage;

/// <summary>
/// What kind of controller a disk sits behind, as far as the primary-disk
/// rule cares. Declared in rank order: an AHCI disk ranks ahead of an NVMe
/// one, which ranks ahead of any other.
/// </summary>
internal enum DiskKind
{
    /// <summary>A SATA disk behind an AHCI controller (PCI class 01h, subclass 06h).</summary>
    Ahci,

    /// <summary>An NVMe namespace (PCI class 01h, subclass 08h).</summary>
    Nvme,

    /// <summary>Anything else, including a disk registered through the public API whose controller the manager cannot see.</summary>
    Other
}
