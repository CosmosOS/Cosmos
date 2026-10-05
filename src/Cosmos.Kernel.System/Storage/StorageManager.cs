// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Core;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.HAL.DriverKit.Devices;
using Cosmos.Kernel.HAL.Interfaces.Devices;
using Cosmos.Kernel.System.Filesystems.Fat;
using Cosmos.Kernel.System.Vfs;
using SchedSpinLock = Cosmos.Kernel.Core.Scheduler.SpinLock;

namespace Cosmos.Kernel.System.Storage;

/// <summary>
/// Manages block storage devices. A disk reaches the manager one of two
/// ways: a driver kit driver publishes it and the manager's
/// <see cref="KitBlockConsumer"/> registers it (the AHCI, NVMe, virtio-blk
/// and USB mass storage drivers), or it is handed to
/// <see cref="RegisterDevice"/> directly by the kernel. Every registration
/// scans the disk for partitions. The tables change after boot too, when a
/// USB disk, or a virtio-blk disk behind a PCI Express hot-plug slot, is
/// plugged in or pulled out, from the kit worker in a probe or a teardown: each is
/// replaced whole on every change, so a list read from <see cref="Devices"/>,
/// <see cref="Partitions"/> or <see cref="GetPartitions"/> never changes
/// under its reader. Read it once and index that copy: a second read may be
/// a newer table.
/// <para>
/// The tables are kept in one order, which decides the primary device: a
/// kit disk before a hand-registered one; among kit disks the smaller node
/// path by ordinal comparison (<c>pci:</c> before <c>usb:</c> before
/// <c>virtio:</c>), and among equal paths the earlier registered; among
/// hand-registered disks the earlier registered. So an internal disk on a
/// PCI controller comes before a USB stick present at boot whichever
/// registered first, and a kit disk arriving after a USB stick moves ahead
/// of it and renumbers <see cref="Partitions"/>, which is why a mount by
/// <see cref="Partition"/> keeps its partition where a mount by index
/// string keeps its index.
/// </para>
/// </summary>
public static class StorageManager
{
    /// <summary>Maximum number of block devices the manager can register.</summary>
    private const int MaxDevices = 8;

    /// <summary>
    /// One registered block device: the device, where it came from, and the
    /// partitions its last scan found. The node path and the driver name are
    /// null for a hand-registered device.
    /// </summary>
    private readonly struct BlockDeviceEntry
    {
        /// <summary>Builds an entry.</summary>
        /// <param name="device">The block device.</param>
        /// <param name="nodePath">The path of the kit node whose driver published it, or null when hand-registered.</param>
        /// <param name="driverName">The name of the kit driver that published it, or null when hand-registered.</param>
        /// <param name="partitions">The partitions found on it, in on-disk order.</param>
        public BlockDeviceEntry(IBlockDevice device, string? nodePath, string? driverName, Partition[] partitions)
        {
            Device = device;
            NodePath = nodePath;
            DriverName = driverName;
            Partitions = partitions;
        }

        /// <summary>The block device.</summary>
        public IBlockDevice Device { get; }

        /// <summary>The path of the kit node whose driver published the device; null for a hand-registered one.</summary>
        public string? NodePath { get; }

        /// <summary>The name of the kit driver that published the device; null for a hand-registered one.</summary>
        public string? DriverName { get; }

        /// <summary>The partitions found on the device, in on-disk order; replaced whole by a rescan.</summary>
        public Partition[] Partitions { get; }

        /// <summary>The same entry with another partition list.</summary>
        /// <param name="partitions">The new partitions.</param>
        public BlockDeviceEntry WithPartitions(Partition[] partitions) => new(Device, NodePath, DriverName, partitions);
    }

    /// <summary>The device table, in the order of <see cref="Compare"/>; replaced whole on every change.</summary>
    private static BlockDeviceEntry[]? s_entries;

    /// <summary>The entries' devices in entry order; rebuilt with <see cref="s_entries"/>.</summary>
    private static IBlockDevice[]? s_devices;

    /// <summary>The entries' partitions concatenated in entry order; rebuilt with <see cref="s_entries"/>.</summary>
    private static Partition[]? s_partitions;

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
    /// Gets the primary block device: the first entry of <see cref="Devices"/>
    /// in the manager's order (a kit disk before a hand-registered one, the
    /// lowest node path among kit disks, then registration order), or
    /// <see langword="null"/> when storage is compiled out or no device is
    /// registered.
    /// </summary>
    public static IBlockDevice? PrimaryDevice
    {
        get
        {
            IBlockDevice[]? devices = s_devices;
            return devices is null || devices.Length == 0 ? null : devices[0];
        }
    }

    /// <summary>
    /// Gets the number of registered block devices.
    /// </summary>
    public static int DeviceCount => s_devices?.Length ?? 0;

    /// <summary>
    /// Every registered block device, in the manager's order: kit disks by
    /// node path, then hand-registered disks in registration order. Empty
    /// before initialization and when storage support is compiled out.
    /// </summary>
    public static IReadOnlyList<IBlockDevice> Devices => (IReadOnlyList<IBlockDevice>?)s_devices ?? Array.Empty<IBlockDevice>();

    /// <summary>
    /// Partitions discovered across every registered device, grouped by
    /// device in the order of <see cref="Devices"/>. Each entry is itself an
    /// <see cref="IBlockDevice"/> rooted at the partition's starting LBA, so
    /// filesystem drivers consume them without knowing whether the host disk
    /// is GPT-, MBR-, or unpartitioned.
    /// </summary>
    public static IReadOnlyList<Partition> Partitions => (IReadOnlyList<Partition>?)s_partitions ?? Array.Empty<Partition>();

    /// <summary>
    /// The partitions discovered on one device, in on-disk order, so a kernel
    /// can number them per disk the way a user does. A partition's position in
    /// this list is its index on <paramref name="device"/>. Empty when the
    /// device has no partition table or is not registered. Allocation-free:
    /// the list is the entry's own array, replaced whole by a rescan, so a
    /// caller's copy never changes under it.
    /// </summary>
    /// <param name="device">The device to list the partitions of.</param>
    public static IReadOnlyList<Partition> GetPartitions(IBlockDevice device)
    {
        BlockDeviceEntry[]? entries = s_entries;
        if (entries is null || device is null)
        {
            return Array.Empty<Partition>();
        }

        int index = IndexOf(entries, device);
        return index < 0 ? Array.Empty<Partition>() : entries[index].Partitions;
    }

    /// <summary>
    /// Initializes the storage manager: creates the empty tables and installs
    /// the manager's consumer of the kit's block devices. Called once during
    /// boot, before the HAL block devices are registered and before the
    /// driver stage runs, so every disk a kit driver publishes is consumed.
    /// </summary>
    internal static void Initialize()
    {
        ThrowIfDisabled();

        if (s_devices is not null)
        {
            return;
        }

        s_entries = [];
        s_partitions = [];
        s_devices = [];
        DeviceRegistry.SetConsumer(DeviceKind.Block, new KitBlockConsumer());
    }

    /// <summary>
    /// Registers a block device with the manager and scans it for a GPT or
    /// MBR partition table. Discovered partitions are appended to
    /// <see cref="Partitions"/>. A device already registered, or one more
    /// than the manager holds, is silently ignored. Thread context: the boot
    /// thread or a kernel's own thread; the scan reads the disk before the
    /// lock.
    /// </summary>
    /// <param name="device">The block device to register.</param>
    /// <exception cref="InvalidOperationException">Storage support is disabled.</exception>
    public static void RegisterDevice(IBlockDevice device)
    {
        ThrowIfDisabled();
        TryRegister(device, null, null);
    }

    /// <summary>
    /// Registers a block device and says what happened, so the kit's block
    /// consumer can refuse a device the manager could not take. The scan
    /// reads the disk and runs before the lock; only its result is published
    /// under it. A kit device (a non-null <paramref name="driverName"/>)
    /// gets its registered line written here, with the primary suffix when
    /// the order rule put it first at that moment; a hand-registered device
    /// logs nothing. Thread context: the publishing probe on the kit worker,
    /// or the boot thread.
    /// </summary>
    /// <param name="device">The block device to register.</param>
    /// <param name="nodePath">The path of the kit node whose driver published it, or null when hand-registered.</param>
    /// <param name="driverName">The name of the kit driver that published it, or null when hand-registered.</param>
    /// <returns>
    /// <see cref="BlockRegistration.Unavailable"/> when storage is compiled
    /// out, the manager is not initialized or the device is null;
    /// <see cref="BlockRegistration.Duplicate"/> when the device is already
    /// registered by reference; <see cref="BlockRegistration.Full"/> when the
    /// table holds <see cref="MaxDevices"/>; else <see cref="BlockRegistration.Added"/>.
    /// </returns>
    internal static BlockRegistration TryRegister(IBlockDevice device, string? nodePath, string? driverName)
    {
        if (!IsEnabled || device is null || s_entries is null)
        {
            return BlockRegistration.Unavailable;
        }

        // Re-registering a known device is a no-op: RegisterDevice is
        // public, so a kernel handing the same device over twice would
        // otherwise double-count it and duplicate every partition under
        // identical names. Checked before the scan, which reads the disk,
        // and again under the lock.
        if (IsRegistered(device))
        {
            return BlockRegistration.Duplicate;
        }

        if (s_entries.Length >= MaxDevices)
        {
            return BlockRegistration.Full;
        }

        List<Partition> partitions = ScanPartitions(device);

        bool primary;
        s_mutationLock.Acquire();
        try
        {
            BlockDeviceEntry[] entries = s_entries;
            if (IsRegistered(device))
            {
                return BlockRegistration.Duplicate;
            }

            if (entries.Length >= MaxDevices)
            {
                return BlockRegistration.Full;
            }

            BlockDeviceEntry[] next = new BlockDeviceEntry[entries.Length + 1];
            Array.Copy(entries, next, entries.Length);
            next[entries.Length] = new BlockDeviceEntry(device, nodePath, driverName, partitions.ToArray());
            Sort(next);
            IBlockDevice[] devices = Replace(next);
            primary = ReferenceEquals(devices[0], device);
        }
        finally
        {
            s_mutationLock.Release();
        }

        // String fragments only: a kernel may register a device before
        // CoreLib number formatting is safe. The suffix says the rule chose
        // the device at that moment; a later line carrying it supersedes
        // this one.
        if (driverName is not null)
        {
            Serial.WriteString("[StorageManager] ");
            Serial.WriteString(device.Name);
            Serial.WriteString(" registered by ");
            Serial.WriteString(driverName);
            if (primary)
            {
                Serial.WriteString(" (primary)");
            }

            Serial.WriteString("\n");
        }

        return BlockRegistration.Added;
    }

    /// <summary>
    /// Forgets a block device that is gone (a USB disk pulled out, a kit disk
    /// withdrawn): it leaves <see cref="Devices"/>, its partitions leave
    /// <see cref="Partitions"/>, and the filesystems mounted from them are
    /// detached from the VFS without a flush, since the device can take no
    /// more writes. When it was the primary device, the first one left
    /// takes its place: the order of the rest does not change. Thread
    /// context: the kit worker in a teardown.
    /// </summary>
    /// <param name="device">The device that is gone.</param>
    internal static void UnregisterDevice(IBlockDevice device)
    {
        if (s_entries is null)
        {
            return;
        }

        bool wasPrimary;
        IBlockDevice? next;
        s_mutationLock.Acquire();
        try
        {
            BlockDeviceEntry[] entries = s_entries;
            int index = IndexOf(entries, device);
            if (index < 0)
            {
                return;
            }

            IBlockDevice[]? current = s_devices;
            wasPrimary = current is not null && current.Length > 0 && ReferenceEquals(current[0], device);

            BlockDeviceEntry[] kept = new BlockDeviceEntry[entries.Length - 1];
            Array.Copy(entries, kept, index);
            Array.Copy(entries, index + 1, kept, index, entries.Length - index - 1);
            IBlockDevice[] devices = Replace(kept);
            next = devices.Length > 0 ? devices[0] : null;
        }
        finally
        {
            s_mutationLock.Release();
        }

        Serial.WriteString("[StorageManager] ");
        Serial.WriteString(device.Name);
        Serial.WriteString(" unregistered");
        if (wasPrimary)
        {
            if (next is not null)
            {
                Serial.WriteString(" (primary now ");
                Serial.WriteString(next.Name);
                Serial.WriteString(")");
            }
            else
            {
                Serial.WriteString(" (no primary)");
            }
        }

        Serial.WriteString("\n");
        VfsManager.DetachMounts(device);
    }

    /// <summary>
    /// Re-scan a previously-registered device for a partition table.
    /// Existing partitions belonging to that host are dropped first, so
    /// callers that just wrote a new layout (tests, formatting tools) get
    /// a clean partition list. The device keeps its place in
    /// <see cref="Devices"/>. Thread context: a kernel's own thread or a
    /// test; the scan reads the disk before the lock.
    /// </summary>
    /// <param name="device">The registered device to re-scan.</param>
    /// <exception cref="InvalidOperationException">Storage support is disabled.</exception>
    public static void RescanPartitions(IBlockDevice device)
    {
        ThrowIfDisabled();

        if (s_entries is null || device is null)
        {
            return;
        }

        List<Partition> partitions = ScanPartitions(device);

        s_mutationLock.Acquire();
        try
        {
            // Unplugged while it was scanned: nothing left to rescan.
            BlockDeviceEntry[] entries = s_entries;
            int index = IndexOf(entries, device);
            if (index >= 0)
            {
                BlockDeviceEntry[] next = new BlockDeviceEntry[entries.Length];
                Array.Copy(entries, next, entries.Length);
                next[index] = entries[index].WithPartitions(partitions.ToArray());
                Replace(next);
            }
        }
        finally
        {
            s_mutationLock.Release();
        }
    }

    /// <summary>
    /// Installs a new entry table and rebuilds the two flat arrays from it,
    /// so the three are replaced together. Under the mutation lock only.
    /// </summary>
    /// <param name="entries">The new table, already in order.</param>
    /// <returns>The new device array, for the caller's primary checks.</returns>
    private static IBlockDevice[] Replace(BlockDeviceEntry[] entries)
    {
        IBlockDevice[] devices = new IBlockDevice[entries.Length];
        int partitionCount = 0;
        for (int i = 0; i < entries.Length; i++)
        {
            devices[i] = entries[i].Device;
            partitionCount += entries[i].Partitions.Length;
        }

        Partition[] partitions = new Partition[partitionCount];
        int filled = 0;
        for (int i = 0; i < entries.Length; i++)
        {
            Partition[] onDevice = entries[i].Partitions;
            Array.Copy(onDevice, 0, partitions, filled, onDevice.Length);
            filled += onDevice.Length;
        }

        s_entries = entries;
        s_devices = devices;
        s_partitions = partitions;
        return devices;
    }

    /// <summary>
    /// A stable insertion sort by <see cref="Compare"/>: equal entries keep
    /// their registration order, which is the rule among the devices of one
    /// node and among hand-registered devices.
    /// </summary>
    /// <param name="entries">The table to order in place.</param>
    private static void Sort(BlockDeviceEntry[] entries)
    {
        for (int i = 1; i < entries.Length; i++)
        {
            BlockDeviceEntry entry = entries[i];
            int j = i - 1;
            while (j >= 0 && Compare(entries[j], entry) > 0)
            {
                entries[j + 1] = entries[j];
                j--;
            }

            entries[j + 1] = entry;
        }
    }

    /// <summary>
    /// The order rule: a kit device (one with a node path) before a
    /// hand-registered one; among kit devices the smaller node path by
    /// ordinal comparison; equal paths and hand-registered devices compare
    /// equal, so their registration order holds.
    /// </summary>
    /// <param name="left">One entry.</param>
    /// <param name="right">The other.</param>
    private static int Compare(BlockDeviceEntry left, BlockDeviceEntry right)
    {
        bool leftKit = left.NodePath is not null;
        bool rightKit = right.NodePath is not null;
        if (leftKit != rightKit)
        {
            return leftKit ? -1 : 1;
        }

        if (!leftKit)
        {
            return 0;
        }

        return string.CompareOrdinal(left.NodePath, right.NodePath);
    }

    private static bool IsRegistered(IBlockDevice device)
    {
        BlockDeviceEntry[]? entries = s_entries;
        return entries is not null && IndexOf(entries, device) >= 0;
    }

    /// <summary>The position of <paramref name="device"/> in <paramref name="entries"/> by reference, or -1.</summary>
    private static int IndexOf(BlockDeviceEntry[] entries, IBlockDevice device)
    {
        for (int i = 0; i < entries.Length; i++)
        {
            if (ReferenceEquals(entries[i].Device, device))
            {
                return i;
            }
        }

        return -1;
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
            // Best-effort scan: a flaky device shouldn't block storage init,
            // but say so, or a real device fault (NVMe timeout throw per the
            // IBlockDevice error contract) is indistinguishable from "no
            // partition table". String-only output: this can run in the
            // phase-3 window where int formatting is off-limits.
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
