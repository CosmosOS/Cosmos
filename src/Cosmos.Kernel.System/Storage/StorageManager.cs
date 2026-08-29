// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Core;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.HAL.Devices.Storage;
using Cosmos.Kernel.HAL.Interfaces.Devices;
using Cosmos.Kernel.System.Filesystems.Fat;
using SchedSpinLock = Cosmos.Kernel.Core.Scheduler.SpinLock;

namespace Cosmos.Kernel.System.Storage;

/// <summary>
/// Manages block storage devices.
/// </summary>
public static class StorageManager
{
    /// <summary>Maximum number of block devices the manager can register.</summary>
    private const int MaxDevices = 8;

    private static IBlockDevice? s_primaryDevice;
    private static List<IBlockDevice>? s_devices;
    private static List<Partition>? s_partitions;

    /// <summary>Guards every mutation of the device and partition tables.</summary>
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
    public static bool IsInitialized => s_devices != null;

    /// <summary>
    /// Gets the primary block device (first one registered), or
    /// <see langword="null"/> when storage is compiled out or no device
    /// registered at boot.
    /// </summary>
    public static IBlockDevice? PrimaryDevice => s_primaryDevice;

    /// <summary>
    /// Gets the number of registered block devices.
    /// </summary>
    public static int DeviceCount => s_devices?.Count ?? 0;

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
        if (s_partitions == null || device == null)
        {
            return Array.Empty<Partition>();
        }

        List<Partition> onDevice = new();
        for (int i = 0; i < s_partitions.Count; i++)
        {
            if (ReferenceEquals(s_partitions[i].Host, device))
            {
                onDevice.Add(s_partitions[i]);
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

        if (s_devices != null)
        {
            return;
        }

        s_partitions = new List<Partition>();
        s_devices = new List<IBlockDevice>(MaxDevices);
    }

    /// <summary>
    /// Registers every block device produced by the HAL storage drivers
    /// (AHCI ports, NVMe namespaces). Called once during boot after the HAL
    /// has initialized the controllers.
    /// </summary>
    internal static void RegisterHalDevices()
    {
        if (!IsEnabled)
        {
            return;
        }

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

        if (device == null || s_devices == null || s_devices.Count >= MaxDevices)
        {
            return;
        }

        // Serializes s_devices/s_partitions mutation for post-boot callers
        // (device hotplug paths, tests); reads are still unsynchronized —
        // enumerating Partitions while another thread rescans remains the
        // caller's problem. Re-registering a known device is a no-op: this is
        // public, so a second RegisterHalDevices call would otherwise
        // double-count the device and duplicate every partition under
        // identical names.
        s_mutationLock.Acquire();
        try
        {
            for (int i = 0; i < s_devices.Count; i++)
            {
                if (ReferenceEquals(s_devices[i], device))
                {
                    return;
                }
            }

            s_devices.Add(device);

            // First device becomes primary
            if (s_primaryDevice == null)
            {
                s_primaryDevice = device;
            }

            ScanPartitions(device);
        }
        finally
        {
            s_mutationLock.Release();
        }
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

        if (s_partitions == null || device == null)
        {
            return;
        }

        s_mutationLock.Acquire();
        try
        {
            for (int i = s_partitions.Count - 1; i >= 0; i--)
            {
                if (ReferenceEquals(s_partitions[i].Host, device))
                {
                    s_partitions.RemoveAt(i);
                }
            }

            ScanPartitions(device);
        }
        finally
        {
            s_mutationLock.Release();
        }
    }

    private static void ScanPartitions(IBlockDevice device)
    {
        if (s_partitions == null)
        {
            return;
        }

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
                    s_partitions.Add(new Partition(device, e.StartSector, e.SectorCount, (uint)i));
                }
                return;
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
                    s_partitions.Add(new Partition(device, e.StartSector, e.SectorCount, slot));
                    slot++;
                }

                if (Mbr.TryGetExtendedPartition(device, out ulong extendedStart))
                {
                    Serial.WriteString("[StorageManager] Extended partition found, walking EBR chain\n");
                    List<MbrPartitionEntry> logicals = Ebr.Parse(device, extendedStart);
                    for (int i = 0; i < logicals.Count; i++)
                    {
                        MbrPartitionEntry e = logicals[i];
                        s_partitions.Add(new Partition(device, e.StartSector, e.SectorCount, slot));
                        slot++;
                    }
                }

                if (slot > 0)
                {
                    return;
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
                s_partitions.Add(new Partition(device, 0, device.BlockCount, 0u));
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
    }

    /// <summary>
    /// Gets a block device by index.
    /// </summary>
    /// <param name="index">The device index.</param>
    /// <returns>The block device, or null when the index names none and when
    /// storage support is disabled.</returns>
    public static IBlockDevice? GetDevice(int index)
    {
        if (s_devices == null || index < 0 || index >= s_devices.Count)
        {
            return null;
        }

        return s_devices[index];
    }
}
