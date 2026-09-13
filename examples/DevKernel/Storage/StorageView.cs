using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.HAL.Interfaces.Devices;
using Cosmos.Kernel.System.Filesystems.Fat;
using Cosmos.Kernel.System.Storage;
using DevKernel.Shell;

namespace DevKernel.Storage;

/// <summary>
/// Rendering and lookup helpers shared by the disk, partition and filesystem
/// commands, so the labels they print and the numbering they accept cannot
/// drift apart.
/// </summary>
internal static class StorageView
{
    /// <summary>Column width (chars) used to pad the labels of the disk listings.</summary>
    public const int DiskLabelColumnWidth = 17;

    /// <summary>Block count passed to <c>ReadBlock</c> when probing a single sector.</summary>
    private const ulong SingleBlock = 1;

    /// <summary>Reports that storage is off or empty; returns false when there is nothing to list.</summary>
    public static bool RequireDevices()
    {
        if (!StorageManager.IsEnabled)
        {
            Terminal.Error("Storage support is disabled (CosmosEnableStorage=false).");
            return false;
        }

        if (StorageManager.DeviceCount == 0)
        {
            Terminal.Warning("No storage devices discovered. Attach a SATA disk to QEMU and reboot.");
            return false;
        }

        return true;
    }

    /// <summary>Prints the geometry block of every attached device, marking the primary one.</summary>
    public static void PrintDevices(bool detailed)
    {
        IReadOnlyList<IBlockDevice> devices = StorageManager.Devices;
        Terminal.InfoLine("Device Count", devices.Count.ToString());

        for (int i = 0; i < devices.Count; i++)
        {
            Console.WriteLine();
            PrintDeviceBlock(i, devices[i], detailed);
            PrintPrimaryMarker(devices[i]);
        }
    }

    /// <summary>Prints one device's name, geometry and partition table type.</summary>
    public static void PrintDeviceBlock(int index, IBlockDevice device, bool detailed)
    {
        ulong totalBytes = device.BlockCount * device.BlockSize;
        Terminal.InfoLine($"[{index}] Name", device.Name, DiskLabelColumnWidth);
        if (detailed)
        {
            Terminal.InfoLine("    Block Size", device.BlockSize.ToString() + " B", DiskLabelColumnWidth);
        }

        Terminal.InfoLine("    Sectors", device.BlockCount.ToString(), DiskLabelColumnWidth);
        Terminal.InfoLine("    Capacity", Units.ToMiB(totalBytes).ToString() + " MiB", DiskLabelColumnWidth);
        Terminal.InfoLine("    Table", DescribePartitionTable(device), DiskLabelColumnWidth);
    }

    /// <summary>Adds the <c>Primary yes</c> line when <paramref name="device"/> is the primary device.</summary>
    public static void PrintPrimaryMarker(IBlockDevice device)
    {
        if (ReferenceEquals(device, StorageManager.PrimaryDevice))
        {
            Terminal.InfoLine("    Primary", "yes", DiskLabelColumnWidth);
        }
    }

    /// <summary>Names the partition table written on <paramref name="device"/>.</summary>
    public static string DescribePartitionTable(IBlockDevice device)
    {
        if (Gpt.IsGpt(device))
        {
            return "GPT";
        }

        if (Mbr.IsMbr(device))
        {
            return "MBR";
        }

        return "None";
    }

    /// <summary>Names the filesystem on <paramref name="partition"/> by parsing its boot sector.</summary>
    public static string DetectFilesystem(Partition partition)
    {
        Span<byte> boot = new byte[partition.BlockSize];
        try
        {
            partition.ReadBlock(FatBootSector.BootSectorLba, SingleBlock, boot);
        }
        catch
        {
            return "unreadable";
        }

        if (FatBootSector.TryParse(boot, out FatBootSector? bootSector))
        {
            return bootSector.Type switch
            {
                FatType.Fat12 => "FAT12",
                FatType.Fat16 => "FAT16",
                FatType.Fat32 => "FAT32",
                _ => "FAT"
            };
        }

        return "unknown";
    }

    /// <summary>
    /// Resolves the (disk, per-disk partition) pair the user types to the
    /// partition itself. The ring numbers partitions per device, which is the
    /// numbering the listings print.
    /// </summary>
    public static bool TryResolvePartition(int diskNumber, int partitionNumber, [NotNullWhen(true)] out Partition? partition)
    {
        partition = null;

        IReadOnlyList<IBlockDevice> devices = StorageManager.Devices;
        if (diskNumber < 0 || diskNumber >= devices.Count)
        {
            return false;
        }

        IReadOnlyList<Partition> onDisk = StorageManager.GetPartitions(devices[diskNumber]);
        if (partitionNumber < 0 || partitionNumber >= onDisk.Count)
        {
            return false;
        }

        partition = onDisk[partitionNumber];
        return true;
    }

    /// <summary>Inverse of <see cref="TryResolvePartition"/>: names the disk and per-disk slot a partition sits in.</summary>
    public static bool TryDescribePartition(Partition partition, out int diskNumber, out int partitionNumber)
    {
        diskNumber = -1;
        partitionNumber = 0;

        IReadOnlyList<IBlockDevice> devices = StorageManager.Devices;
        for (int d = 0; d < devices.Count; d++)
        {
            if (!ReferenceEquals(devices[d], partition.Host))
            {
                continue;
            }

            IReadOnlyList<Partition> onDisk = StorageManager.GetPartitions(devices[d]);
            for (int i = 0; i < onDisk.Count; i++)
            {
                if (ReferenceEquals(onDisk[i], partition))
                {
                    diskNumber = d;
                    partitionNumber = i;
                    return true;
                }
            }
        }

        return false;
    }
}
