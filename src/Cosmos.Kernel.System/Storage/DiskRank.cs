// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.System.Storage;

/// <summary>
/// Where a registered disk stands in the storage manager's order, which
/// decides the primary disk: the first one. A disk that cannot leave the
/// machine ranks ahead of one that can, an AHCI disk ahead of an NVMe one
/// ahead of any other, then the disk whose PCI function comes first in bus,
/// device and function order, then the disk registered first. The order no
/// longer depends on which driver happened to register its disk first,
/// which it did while HAL registered every disk in a fixed order at boot:
/// a driver the kit binds registers its disks later, from
/// Global.StartKernel.
/// </summary>
internal readonly struct DiskRank : IComparable<DiskRank>, IEquatable<DiskRank>
{
    /// <summary>The address of a disk with no PCI function the manager can see, which sorts after every real one.</summary>
    internal const uint NoPciAddress = uint.MaxValue;

    /// <summary>True for a disk whose device can leave the bus, such as a USB stick.</summary>
    internal bool IsRemovable { get; }

    /// <summary>The kind of controller behind the disk.</summary>
    internal DiskKind Kind { get; }

    /// <summary>
    /// The PCI function behind the disk as bus, device and function packed
    /// into one number that sorts the same way, or <see cref="NoPciAddress"/>.
    /// </summary>
    internal uint PciAddress { get; }

    /// <summary>The manager's registration counter when the disk registered.</summary>
    internal ulong Sequence { get; }

    /// <summary>Creates the rank of one disk from the four keys the rule compares, in rule order.</summary>
    internal DiskRank(bool isRemovable, DiskKind kind, uint pciAddress, ulong sequence)
    {
        IsRemovable = isRemovable;
        Kind = kind;
        PciAddress = pciAddress;
        Sequence = sequence;
    }

    /// <summary>Packs a PCI function's bus, device and function into a <see cref="PciAddress"/>.</summary>
    internal static uint PackPciAddress(uint bus, uint device, uint function) =>
        (bus << 16) | (device << 8) | function;

    /// <summary>
    /// Where a disk of rank <paramref name="rank"/> goes in
    /// <paramref name="ranks"/>, which is sorted: behind every disk that
    /// ranks ahead of it or level with it.
    /// </summary>
    internal static int InsertionIndex(ReadOnlySpan<DiskRank> ranks, DiskRank rank)
    {
        int index = ranks.Length;
        while (index > 0 && ranks[index - 1].CompareTo(rank) > 0)
        {
            index--;
        }

        return index;
    }

    /// <summary>
    /// Negative when this disk ranks ahead of <paramref name="other"/>:
    /// non-removable first, then by kind, PCI address and sequence.
    /// </summary>
    public int CompareTo(DiskRank other)
    {
        if (IsRemovable != other.IsRemovable)
        {
            return IsRemovable ? 1 : -1;
        }

        if (Kind != other.Kind)
        {
            return Kind < other.Kind ? -1 : 1;
        }

        if (PciAddress != other.PciAddress)
        {
            return PciAddress < other.PciAddress ? -1 : 1;
        }

        return Sequence.CompareTo(other.Sequence);
    }

    /// <summary>True when both ranks carry the same four keys.</summary>
    public bool Equals(DiskRank other) =>
        IsRemovable == other.IsRemovable
        && Kind == other.Kind
        && PciAddress == other.PciAddress
        && Sequence == other.Sequence;

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is DiskRank other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(IsRemovable, Kind, PciAddress, Sequence);
}
