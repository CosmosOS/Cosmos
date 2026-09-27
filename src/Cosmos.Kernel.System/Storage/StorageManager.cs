// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Core;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.HAL.Drivers.Engine;
using Cosmos.Kernel.HAL.Drivers.Pci;
using Cosmos.Kernel.HAL.Interfaces.Devices;
using Cosmos.Kernel.HAL.Pci;
using Cosmos.Kernel.HAL.Pci.Enums;
using Cosmos.Kernel.System.Filesystems.Fat;
using Cosmos.Kernel.System.Vfs;
using SchedSpinLock = Cosmos.Kernel.Core.Scheduler.SpinLock;

namespace Cosmos.Kernel.System.Storage;

/// <summary>
/// Manages block storage devices. The tables change after boot too: when a
/// USB disk is plugged in or pulled out, from the USB hot-plug thread, and
/// when a driver publishes a disk from one of its work items, from the
/// driver-work thread. Each is replaced whole on every change, so a list
/// read from <see cref="Devices"/>, <see cref="Partitions"/> or
/// <see cref="GetPartitions"/> never changes under its reader. Read it once
/// and index that copy: a second read may be a newer table.
/// </summary>
/// <remarks>
/// The disks are kept in the order of the primary-disk rule, whatever order
/// they register in: a disk that cannot leave the machine before one that
/// can (a USB disk), then AHCI disks, then NVMe namespaces, then any other
/// disk, then by the PCI function behind them in bus, device and function
/// order, then in registration order. The first is
/// <see cref="PrimaryDevice"/>, and <see cref="Partitions"/> follows the
/// same order, so the primary disk's partitions, when it has any, come
/// first. The drivers publish their disks from the driver pass in
/// Global.StartKernel in the order they bind, and a USB stick plugged in
/// later registers after them, so the rule, not that order, is what keeps
/// an internal disk primary. It orders the disks the manager holds: at
/// most 8, taken first come, so once 8 are registered a later disk is
/// refused whatever its rank.
/// </remarks>
public static class StorageManager
{
    /// <summary>Maximum number of block devices the manager can register.</summary>
    private const int MaxDevices = 8;

    private static IBlockDevice[]? s_devices;
    private static Partition[]? s_partitions;

    /// <summary>Each device's rank, in the order of <see cref="s_devices"/>; replaced with it, under the lock.</summary>
    private static DiskRank[]? s_ranks;

    /// <summary>Counts registrations, for the last key of the rule. Changed under the lock.</summary>
    private static ulong s_nextSequence;

    /// <summary>Serializes the replacement of the device and partition tables.</summary>
    private static SchedSpinLock s_mutationLock;

    /// <summary>
    /// Whether storage support is enabled. Uses centralized feature flag.
    /// </summary>
    public static bool IsEnabled => CosmosFeatures.StorageEnabled;

    /// <summary>
    /// Throws when storage support is compiled out. Guards actions, not reads:
    /// a read answers honestly (0, null, false, empty) so a kernel can branch
    /// on it, and an action names the switch to set instead of failing
    /// silently.
    /// </summary>
    private static void ThrowIfDisabled()
    {
        if (!IsEnabled)
        {
            throw new InvalidOperationException("Storage support is disabled. Set CosmosEnableStorage=true in your csproj to enable it.");
        }
    }

    /// <summary>
    /// Gets whether the storage manager is initialized, which is what makes
    /// the device table exist.
    /// </summary>
    public static bool IsInitialized => s_devices is not null;

    /// <summary>
    /// Gets the primary block device: the first of <see cref="Devices"/>,
    /// which is the one the primary-disk rule ranks first (see the class
    /// remarks), or <see langword="null"/> when storage is compiled out or
    /// no device is registered. It can change as disks come and go: a disk
    /// that ranks ahead of it takes its place when it registers, and when
    /// it is unregistered the next one takes it.
    /// </summary>
    public static IBlockDevice? PrimaryDevice => s_devices is { Length: > 0 } devices ? devices[0] : null;

    /// <summary>
    /// Gets the number of registered block devices.
    /// </summary>
    public static int DeviceCount => s_devices?.Length ?? 0;

    /// <summary>
    /// Every registered block device, in the order of the primary-disk rule
    /// (see the class remarks): the first is <see cref="PrimaryDevice"/>.
    /// Empty before initialization and when storage support is compiled out.
    /// </summary>
    public static IReadOnlyList<IBlockDevice> Devices => (IReadOnlyList<IBlockDevice>?)s_devices ?? Array.Empty<IBlockDevice>();

    /// <summary>
    /// Partitions discovered across every registered device, device by
    /// device in the order of <see cref="Devices"/>, and each device's in
    /// on-disk order, so the primary disk's partitions, when it has any,
    /// come first. Each entry is itself an <see cref="IBlockDevice"/> rooted
    /// at the partition's starting LBA, so filesystem drivers consume them
    /// without knowing whether the host disk is GPT-, MBR-, or unpartitioned.
    /// </summary>
    public static IReadOnlyList<Partition> Partitions => (IReadOnlyList<Partition>?)s_partitions ?? Array.Empty<Partition>();

    /// <summary>
    /// The partitions discovered on one device, in on-disk order, so a kernel
    /// can number them per disk the way a user does. A partition's position in
    /// this list is its index on <paramref name="device"/>. Empty when the
    /// device has no partition table or is not registered.
    /// </summary>
    /// <param name="device">The device to list the partitions of.</param>
    public static IReadOnlyList<Partition> GetPartitions(IBlockDevice device)
    {
        Partition[]? partitions = s_partitions;
        if (partitions is null || device is null)
        {
            return Array.Empty<Partition>();
        }

        List<Partition> onDevice = [];
        for (int i = 0; i < partitions.Length; i++)
        {
            if (ReferenceEquals(partitions[i].Host, device))
            {
                onDevice.Add(partitions[i]);
            }
        }

        return onDevice;
    }

    /// <summary>
    /// Initializes the storage manager. Called once during boot, before the
    /// driver pass delivers the disks the drivers publish.
    /// </summary>
    internal static void Initialize()
    {
        ThrowIfDisabled();

        if (s_devices is not null)
        {
            return;
        }

        s_partitions = [];
        s_ranks = [];
        s_devices = [];
    }

    /// <summary>
    /// Registers a block device with the manager and scans it for a GPT or
    /// MBR partition table. The device takes its place in
    /// <see cref="Devices"/> by the primary-disk rule (see the class
    /// remarks), and its partitions take theirs in <see cref="Partitions"/>.
    /// A device the manager cannot see the controller of ranks as a
    /// non-removable disk of no known kind: ahead of the USB disks, behind
    /// the AHCI and NVMe ones. Does nothing for a device already registered,
    /// or once 8 devices are.
    /// </summary>
    /// <param name="device">The block device to register.</param>
    /// <exception cref="InvalidOperationException">Storage support is disabled.</exception>
    public static void RegisterDevice(IBlockDevice device)
    {
        ThrowIfDisabled();
        _ = TryRegisterDevice(device);
    }

    /// <summary>
    /// What <see cref="RegisterDevice"/> does, answering whether the device
    /// is now registered, which the driver kit logs for the disks drivers
    /// publish. Thread context: it reads the disk.
    /// </summary>
    /// <returns>
    /// False when the device was null or already registered, the manager is
    /// not initialized, or it holds 8 devices already.
    /// </returns>
    internal static bool TryRegisterDevice(IBlockDevice device)
    {
        if (device is null || s_devices is null || IsRegistered(device) || s_devices.Length >= MaxDevices)
        {
            return false;
        }

        // The scan reads the disk, so it runs before the lock, and only its
        // result is published under it. Re-registering a known device is a
        // no-op: this is public, so a second RegisterDevice call would
        // otherwise double-count the device and duplicate every partition
        // under identical names.
        List<Partition> partitions = ScanPartitions(device);

        s_mutationLock.Acquire();
        try
        {
            IBlockDevice[] devices = s_devices;
            DiskRank[] ranks = s_ranks ?? [];
            if (IsRegistered(device) || devices.Length >= MaxDevices)
            {
                return false;
            }

            DiskRank rank = RankOf(device, s_nextSequence);
            s_nextSequence++;
            int index = DiskRank.InsertionIndex(ranks, rank);
            IBlockDevice[] ordered = WithInserted(devices, index, device);
            s_partitions = InDeviceOrder(ordered, device, partitions);
            s_ranks = WithInserted(ranks, index, rank);
            s_devices = ordered;
        }
        finally
        {
            s_mutationLock.Release();
        }

        return true;
    }

    /// <summary>
    /// Forgets a block device that is gone, such as a USB disk pulled out:
    /// a USB driver's Remove calls it for the disk it registered with
    /// <see cref="RegisterDevice"/>. The device leaves <see cref="Devices"/>,
    /// its partitions leave <see cref="Partitions"/>, and the filesystems
    /// mounted from them are detached from the VFS without a flush, since
    /// the device can take no more writes. When it was the primary device,
    /// the next one in the order takes its place. Thread context: it
    /// allocates, and takes the manager's and the VFS's locks.
    /// </summary>
    /// <param name="device">The device that is gone.</param>
    /// <returns>
    /// True when the device was registered and is now forgotten. False when
    /// it was not registered, which includes every device of a kernel built
    /// without storage support, so a cleanup path needs no check of its own.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="device"/> is null.</exception>
    public static bool UnregisterDevice(IBlockDevice device)
    {
        ArgumentNullException.ThrowIfNull(device);

        // The switch alone, so ILC folds it and a kernel without storage
        // trims the rest: nothing could have registered the device.
        if (!CosmosFeatures.StorageEnabled)
        {
            return false;
        }

        if (s_devices is null)
        {
            return false;
        }

        s_mutationLock.Acquire();
        try
        {
            if (!IsRegistered(device))
            {
                return false;
            }

            IBlockDevice[] registered = s_devices;
            DiskRank[] ranks = s_ranks ?? [];
            List<IBlockDevice> devices = [];
            List<DiskRank> keptRanks = [];
            for (int i = 0; i < registered.Length; i++)
            {
                if (!ReferenceEquals(registered[i], device))
                {
                    devices.Add(registered[i]);
                    keptRanks.Add(ranks[i]);
                }
            }

            s_partitions = WithoutPartitionsOf(device);
            s_ranks = keptRanks.ToArray();
            s_devices = devices.ToArray();
        }
        finally
        {
            s_mutationLock.Release();
        }

        Serial.WriteString("[StorageManager] ");
        Serial.WriteString(device.Name);
        Serial.WriteString(" unregistered\n");
        VfsManager.DetachMounts(device);
        return true;
    }

    /// <summary>
    /// Re-scan a previously-registered device for a partition table.
    /// Existing partitions belonging to that host are dropped first, so
    /// callers that just wrote a new layout (tests, formatting tools) get
    /// a clean partition list; the new ones take the device's place in
    /// <see cref="Partitions"/>.
    /// </summary>
    /// <param name="device">The registered device to re-scan.</param>
    /// <exception cref="InvalidOperationException">Storage support is disabled.</exception>
    public static void RescanPartitions(IBlockDevice device)
    {
        ThrowIfDisabled();

        if (s_partitions is null || device is null)
        {
            return;
        }

        List<Partition> partitions = ScanPartitions(device);

        s_mutationLock.Acquire();
        try
        {
            // Unplugged while it was scanned: nothing left to rescan.
            if (s_devices is { } devices && IsRegistered(device))
            {
                s_partitions = InDeviceOrder(devices, device, partitions);
            }
        }
        finally
        {
            s_mutationLock.Release();
        }
    }

    private static bool IsRegistered(IBlockDevice device)
    {
        IBlockDevice[]? devices = s_devices;
        if (devices is null)
        {
            return false;
        }

        for (int i = 0; i < devices.Length; i++)
        {
            if (ReferenceEquals(devices[i], device))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Every partition of <paramref name="devices"/>, device by device, each
    /// device's in on-disk order: <paramref name="changedPartitions"/> for
    /// <paramref name="changed"/>, the ones already known for the others.
    /// </summary>
    private static Partition[] InDeviceOrder(IBlockDevice[] devices, IBlockDevice changed, List<Partition> changedPartitions)
    {
        IReadOnlyList<Partition> known = Partitions;
        List<Partition> ordered = [];
        for (int i = 0; i < devices.Length; i++)
        {
            IBlockDevice device = devices[i];
            if (ReferenceEquals(device, changed))
            {
                ordered.AddRange(changedPartitions);
                continue;
            }

            for (int j = 0; j < known.Count; j++)
            {
                if (ReferenceEquals(known[j].Host, device))
                {
                    ordered.Add(known[j]);
                }
            }
        }

        return ordered.ToArray();
    }

    /// <summary>A copy of <paramref name="items"/> with <paramref name="item"/> at <paramref name="index"/>.</summary>
    private static T[] WithInserted<T>(T[] items, int index, T item)
    {
        T[] inserted = new T[items.Length + 1];
        for (int i = 0; i < index; i++)
        {
            inserted[i] = items[i];
        }

        inserted[index] = item;
        for (int i = index; i < items.Length; i++)
        {
            inserted[i + 1] = items[i];
        }

        return inserted;
    }

    /// <summary>
    /// Where <paramref name="device"/> stands in the primary-disk rule. A
    /// disk a driver published through the kit carries its binding, which
    /// says whether it can leave the bus, as every USB driver's can, and
    /// which PCI function it sits on; anything else is a fixed disk of no
    /// kind the manager knows.
    /// </summary>
    private static DiskRank RankOf(IBlockDevice device, ulong sequence)
    {
        // The switch alone, so ILC folds it: a kernel without PCI keeps no
        // kit adapter here.
        if (CosmosFeatures.PCIEnabled)
        {
            if (device is PublishedBlockDevice published)
            {
                if (published.Function is not { } function)
                {
                    return new DiskRank(published.IsRemovable, DiskKind.Other, DiskRank.NoPciAddress, sequence);
                }

                return new DiskRank(published.IsRemovable, KindOf(function), AddressOf(function.Device), sequence);
            }
        }

        return new DiskRank(false, DiskKind.Other, DiskRank.NoPciAddress, sequence);
    }

    /// <summary>The kind of controller <paramref name="function"/> is, by its PCI class and subclass.</summary>
    private static DiskKind KindOf(PciFunction function)
    {
        if ((ClassId)function.BaseClass != ClassId.MassStorageController)
        {
            return DiskKind.Other;
        }

        return (SubclassId)function.Subclass switch
        {
            SubclassId.SataController => DiskKind.Ahci,
            SubclassId.NvmController => DiskKind.Nvme,
            _ => DiskKind.Other
        };
    }

    private static uint AddressOf(PciDevice function) =>
        DiskRank.PackPciAddress(function.Bus, function.Slot, function.Function);

    private static Partition[] WithoutPartitionsOf(IBlockDevice device)
    {
        List<Partition> kept = [];
        foreach (Partition partition in Partitions)
        {
            if (!ReferenceEquals(partition.Host, device))
            {
                kept.Add(partition);
            }
        }

        return kept.ToArray();
    }

    /// <summary>Reads the partition table of <paramref name="device"/>; empty when there is none or it cannot be read.</summary>
    private static List<Partition> ScanPartitions(IBlockDevice device)
    {
        List<Partition> partitions = [];
        try
        {
            if (Gpt.IsGpt(device))
            {
                Serial.WriteString("[StorageManager] GPT detected on ");
                Serial.WriteString(device.Name);
                Serial.WriteString("\n");
                List<GptPartitionEntry> entries = Gpt.Parse(device);
                for (int i = 0; i < entries.Count; i++)
                {
                    GptPartitionEntry e = entries[i];
                    partitions.Add(new Partition(device, e.StartSector, e.SectorCount, (uint)i));
                }
                return partitions;
            }

            if (Mbr.IsMbr(device))
            {
                Serial.WriteString("[StorageManager] MBR detected on ");
                Serial.WriteString(device.Name);
                Serial.WriteString("\n");
                List<MbrPartitionEntry> entries = Mbr.Parse(device);
                uint slot = 0;
                for (int i = 0; i < entries.Count; i++)
                {
                    MbrPartitionEntry e = entries[i];
                    partitions.Add(new Partition(device, e.StartSector, e.SectorCount, slot));
                    slot++;
                }

                if (Mbr.TryGetExtendedPartition(device, out ulong extendedStart))
                {
                    Serial.WriteString("[StorageManager] Extended partition found, walking EBR chain\n");
                    List<MbrPartitionEntry> logicals = Ebr.Parse(device, extendedStart);
                    for (int i = 0; i < logicals.Count; i++)
                    {
                        MbrPartitionEntry e = logicals[i];
                        partitions.Add(new Partition(device, e.StartSector, e.SectorCount, slot));
                        slot++;
                    }
                }

                if (slot > 0)
                {
                    return partitions;
                }
            }

            // No partition table produced an entry. A filesystem formatted
            // straight onto the disk (a "superfloppy": mkfs.vfat / Windows
            // format of a raw image) carries the MBR's 0xAA55 boot signature
            // in its BPB sector, so it lands here rather than in a table
            // branch above. Probe the boot sector as a FAT BPB and, when its
            // claimed geometry fits the device, surface the whole disk as the
            // single partition the Partitions contract promises for
            // unpartitioned hosts. A blank disk fails the probe and stays
            // partitionless; GPT disks never reach here (empty GPTs return
            // above, keeping their on-disk structures out of partition I/O).
            Span<byte> boot = new byte[(int)device.BlockSize];
            device.ReadBlock(FatBootSector.BootSectorLba, 1, boot);
            if (FatBootSector.TryParse(boot, out FatBootSector? volume)
                && volume.BytesPerSector == device.BlockSize
                && volume.TotalSectorCount <= device.BlockCount)
            {
                Serial.WriteString("[StorageManager] Unpartitioned filesystem volume detected on ");
                Serial.WriteString(device.Name);
                Serial.WriteString("\n");
                partitions.Add(new Partition(device, 0, device.BlockCount, 0u));
            }
        }
        catch (Exception)
        {
            // Best-effort scan: a flaky device shouldn't block storage init —
            // but say so, or a real device fault (NVMe timeout throw per the
            // IBlockDevice error contract) is indistinguishable from "no
            // partition table".
            Serial.WriteString("[StorageManager] Partition scan failed on ");
            Serial.WriteString(device.Name);
            Serial.WriteString("\n");
        }

        return partitions;
    }

    /// <summary>
    /// Gets a block device by index.
    /// </summary>
    /// <param name="index">The device index.</param>
    /// <returns>The block device, or null when the index names none and when
    /// storage support is disabled.</returns>
    public static IBlockDevice? GetDevice(int index)
    {
        IBlockDevice[]? devices = s_devices;
        if (devices is null || index < 0 || index >= devices.Length)
        {
            return null;
        }

        return devices[index];
    }
}
