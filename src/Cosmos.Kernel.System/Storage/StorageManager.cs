// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Core;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.HAL.Devices.Storage;
using Cosmos.Kernel.HAL.Interfaces.Devices;
using Cosmos.Kernel.System.Filesystems.Fat;
using Cosmos.Kernel.System.Vfs;
using SchedSpinLock = Cosmos.Kernel.Core.Scheduler.SpinLock;

namespace Cosmos.Kernel.System.Storage;

/// <summary>
/// Manages block storage devices. The tables change after boot too, when a
/// USB disk is plugged in or pulled out, from the USB hot-plug thread: each
/// is replaced whole on every change, so a list read from
/// <see cref="Devices"/>, <see cref="Partitions"/> or
/// <see cref="GetPartitions"/> never changes under its reader. Read it once
/// and index that copy: a second read may be a newer table.
/// </summary>
public static class StorageManager
{
    /// <summary>Maximum number of block devices the manager can register.</summary>
    private const int MaxDevices = 8;

    private static IBlockDevice? s_primaryDevice;
    private static IBlockDevice[]? s_devices;
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
    /// Gets the primary block device (first one registered), or
    /// <see langword="null"/> when storage is compiled out or no device
    /// registered at boot.
    /// </summary>
    public static IBlockDevice? PrimaryDevice => s_primaryDevice;

    /// <summary>
    /// Gets the number of registered block devices.
    /// </summary>
    public static int DeviceCount => s_devices?.Length ?? 0;

    /// <summary>
    /// Every registered block device, in registration order. Empty before
    /// initialization and when storage support is compiled out.
    /// </summary>
    public static IReadOnlyList<IBlockDevice> Devices => (IReadOnlyList<IBlockDevice>?)s_devices ?? Array.Empty<IBlockDevice>();

    /// <summary>
    /// Partitions discovered across every registered device. Each entry is
    /// itself an <see cref="IBlockDevice"/> rooted at the partition's
    /// starting LBA, so filesystem drivers consume them without knowing
    /// whether the host disk is GPT-, MBR-, or unpartitioned.
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
    /// HAL block devices are registered.
    /// </summary>
    internal static void Initialize()
    {
        ThrowIfDisabled();

        if (s_devices is not null)
        {
            return;
        }

        s_partitions = [];
        s_devices = [];
    }

    /// <summary>
    /// Registers every block device produced by the HAL storage drivers
    /// (AHCI ports, NVMe namespaces, then USB mass storage units, so an
    /// internal disk stays the primary one), and follows the USB disks
    /// plugged in or pulled out from then on. Called once during boot after
    /// the HAL has initialized the controllers.
    /// </summary>
    internal static void RegisterHalDevices()
    {
        if (!IsEnabled)
        {
            return;
        }

        // Before the boot disks are read, so none can slip between the two;
        // one reported twice is registered once.
        UsbMassStorageDriver.DiskAttached = RegisterDevice;
        UsbMassStorageDriver.DiskDetached = UnregisterDevice;

        IReadOnlyList<BlockDevice> ports = Ahci.Ports;
        for (int i = 0; i < ports.Count; i++)
        {
            RegisterDevice(ports[i]);
        }

        IReadOnlyList<NvmeNamespace> nvmeNamespaces = Nvme.Namespaces;
        for (int i = 0; i < nvmeNamespaces.Count; i++)
        {
            RegisterDevice(nvmeNamespaces[i]);
        }

        IReadOnlyList<UsbMassStorage> usbDisks = UsbMassStorageDriver.Disks;
        for (int i = 0; i < usbDisks.Count; i++)
        {
            RegisterDevice(usbDisks[i]);
        }
    }

    /// <summary>
    /// Registers a block device with the manager and scans it for a GPT or
    /// MBR partition table. Discovered partitions are appended to
    /// <see cref="Partitions"/>.
    /// </summary>
    /// <param name="device">The block device to register.</param>
    /// <exception cref="InvalidOperationException">Storage support is disabled.</exception>
    public static void RegisterDevice(IBlockDevice device)
    {
        ThrowIfDisabled();

        if (device is null || s_devices is null || IsRegistered(device) || s_devices.Length >= MaxDevices)
        {
            return;
        }

        // The scan reads the disk, so it runs before the lock, and only its
        // result is published under it. Re-registering a known device is a
        // no-op: this is public, so a second RegisterHalDevices call would
        // otherwise double-count the device and duplicate every partition
        // under identical names.
        List<Partition> partitions = ScanPartitions(device);

        s_mutationLock.Acquire();
        try
        {
            IBlockDevice[] devices = s_devices;
            if (IsRegistered(device) || devices.Length >= MaxDevices)
            {
                return;
            }

            s_devices = [.. devices, device];
            s_partitions = [.. Partitions, .. partitions];
            s_primaryDevice ??= device;
        }
        finally
        {
            s_mutationLock.Release();
        }
    }

    /// <summary>
    /// Forgets a block device that is gone (a USB disk pulled out): it
    /// leaves <see cref="Devices"/>, its partitions leave
    /// <see cref="Partitions"/>, and the filesystems mounted from them are
    /// detached from the VFS without a flush, since the device can take no
    /// more writes. When it was the primary device, the first one left
    /// takes its place.
    /// </summary>
    /// <param name="device">The device that is gone.</param>
    internal static void UnregisterDevice(IBlockDevice device)
    {
        if (s_devices is null)
        {
            return;
        }

        s_mutationLock.Acquire();
        try
        {
            if (!IsRegistered(device))
            {
                return;
            }

            List<IBlockDevice> devices = [];
            foreach (IBlockDevice other in s_devices)
            {
                if (!ReferenceEquals(other, device))
                {
                    devices.Add(other);
                }
            }

            s_devices = devices.ToArray();
            s_partitions = WithoutPartitionsOf(device);
            if (ReferenceEquals(s_primaryDevice, device))
            {
                s_primaryDevice = devices.Count > 0 ? devices[0] : null;
            }
        }
        finally
        {
            s_mutationLock.Release();
        }

        Serial.WriteString("[StorageManager] ");
        Serial.WriteString(device.Name);
        Serial.WriteString(" unregistered\n");
        VfsManager.DetachMounts(device);
    }

    /// <summary>
    /// Re-scan a previously-registered device for a partition table.
    /// Existing partitions belonging to that host are dropped first, so
    /// callers that just wrote a new layout (tests, formatting tools) get
    /// a clean partition list.
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
            if (IsRegistered(device))
            {
                s_partitions = [.. WithoutPartitionsOf(device), .. partitions];
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
