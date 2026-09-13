using System;
using System.Collections.Generic;
using Cosmos.Kernel.HAL.Interfaces.Devices;
using Cosmos.Kernel.System.Storage;
using DevKernel.Shell;
using DevKernel.Storage;

namespace DevKernel.Commands;

/// <summary>
/// Partition table authoring: create MBR/GPT tables and the entries inside them.
/// </summary>
internal static class PartitionCommands
{
    /// <summary>Help section these commands are listed under.</summary>
    private const string Category = "Partitions";

    /// <summary>Conventional first partition LBA on an MBR disk (1 MiB alignment).</summary>
    private const ulong MbrFirstPartitionLba = 2048;

    /// <summary>MBR system ID byte (FAT32 LBA) stamped on partitions created by mkpart.</summary>
    private const byte MbrFat32LbaSystemId = 0x0B;

    /// <summary>Indent of the per-partition lines nested under a disk.</summary>
    private const string PartitionIndent = "        ";

    public static void Register(CommandShell shell)
    {
        shell.Register(
            Category,
            new ShellCommand
            {
                Name = "lspart",
                Usage = "lspart",
                Description = "List partitions, grouped under each disk",
                Execute = static (context, args) => ListPartitions(),
            },
            new ShellCommand
            {
                Name = "mkmbr",
                Usage = "mkmbr <disk>",
                Description = "Write a fresh empty MBR to a disk",
                MinArgs = 1,
                MaxArgs = 1,
                Execute = static (context, args) =>
                {
                    if (!args.TryGetInt(0, out int diskNumber))
                    {
                        args.PrintUsage();
                        return;
                    }

                    CreateTable(diskNumber, gpt: false);
                },
            },
            new ShellCommand
            {
                Name = "mkgpt",
                Usage = "mkgpt <disk>",
                Description = "Write a fresh empty GPT to a disk",
                MinArgs = 1,
                MaxArgs = 1,
                Execute = static (context, args) =>
                {
                    if (!args.TryGetInt(0, out int diskNumber))
                    {
                        args.PrintUsage();
                        return;
                    }

                    CreateTable(diskNumber, gpt: true);
                },
            },
            new ShellCommand
            {
                Name = "mkpart",
                Usage = "mkpart <disk> [start_lba] <size_mb>",
                Description = "Create a partition (start LBA optional, appended when omitted)",
                MinArgs = 2,
                MaxArgs = 3,
                Execute = static (context, args) => CreatePartitionEntry(args),
            },
            new ShellCommand
            {
                Name = "rmpart",
                Usage = "rmpart <disk> <part>",
                Description = "Delete a partition",
                MinArgs = 2,
                MaxArgs = 2,
                Execute = static (context, args) =>
                {
                    if (!args.TryGetInt(0, out int diskNumber) || !args.TryGetInt(1, out int partitionNumber))
                    {
                        args.PrintUsage();
                        return;
                    }

                    DeletePartitionEntry(diskNumber, partitionNumber);
                },
            });
    }

    private static void ListPartitions()
    {
        if (!StorageManager.IsEnabled)
        {
            Terminal.Error("Storage support is disabled.");
            return;
        }

        Terminal.Header("Partitions:");

        if (StorageManager.DeviceCount == 0)
        {
            Terminal.Warning("No storage devices discovered.");
            return;
        }

        IReadOnlyList<IBlockDevice> devices = StorageManager.Devices;
        for (int i = 0; i < devices.Count; i++)
        {
            Console.WriteLine();
            StorageView.PrintDeviceBlock(i, devices[i], detailed: false);
            StorageView.PrintPrimaryMarker(devices[i]);

            IReadOnlyList<Partition> onDisk = StorageManager.GetPartitions(devices[i]);
            for (int p = 0; p < onDisk.Count; p++)
            {
                PrintPartitionLine(p, onDisk[p]);
            }

            if (onDisk.Count == 0)
            {
                Terminal.Muted(PartitionIndent + "(no partitions)");
            }
        }
    }

    private static void PrintPartitionLine(int localIndex, Partition partition)
    {
        ulong sizeBytes = partition.BlockCount * partition.BlockSize;

        Console.Write(PartitionIndent);
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write("[" + localIndex + "] ");
        Console.ResetColor();
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write(partition.Name);
        Console.ResetColor();
        Console.Write("  Start=" + partition.StartSector);
        Console.Write("  Sectors=" + partition.BlockCount);
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("  " + Units.ToMiB(sizeBytes) + " MiB");
        Console.ResetColor();
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.Write("  " + StorageView.DetectFilesystem(partition));
        Console.ResetColor();
        Console.WriteLine();
    }

    private static void CreateTable(int diskNumber, bool gpt)
    {
        IBlockDevice? device = StorageManager.GetDevice(diskNumber);
        if (device == null)
        {
            Terminal.Error("Invalid disk number.");
            return;
        }

        if (gpt)
        {
            Gpt.Create(device);
        }
        else
        {
            Mbr.Create(device);
        }

        StorageManager.RescanPartitions(device);
        Terminal.Success((gpt ? "GPT" : "MBR") + " table written to disk " + diskNumber + ".");
    }

    /// <summary>
    /// Two accepted shapes: <c>mkpart &lt;disk&gt; &lt;size_mb&gt;</c> appends after
    /// the last partition, <c>mkpart &lt;disk&gt; &lt;start&gt; &lt;size_mb&gt;</c> places it explicitly.
    /// </summary>
    private static void CreatePartitionEntry(CommandArgs args)
    {
        if (!args.TryGetInt(0, out int diskNumber))
        {
            args.PrintUsage();
            return;
        }

        ulong? startSector = null;
        int sizeMb;
        if (args.Count == 2)
        {
            if (!args.TryGetInt(1, out sizeMb))
            {
                args.PrintUsage();
                return;
            }
        }
        else
        {
            if (!args.TryGetULong(1, out ulong explicitStart) || !args.TryGetInt(2, out sizeMb))
            {
                args.PrintUsage();
                return;
            }

            startSector = explicitStart;
        }

        CreatePartitionEntry(diskNumber, startSector, sizeMb);
    }

    private static void CreatePartitionEntry(int diskNumber, ulong? startSectorOrAuto, int sizeMb)
    {
        if (sizeMb <= 0)
        {
            Terminal.Error("Partition size must be greater than 0 MB.");
            return;
        }

        IBlockDevice? device = StorageManager.GetDevice(diskNumber);
        if (device == null)
        {
            Terminal.Error("Invalid disk number.");
            return;
        }

        bool isGpt = Gpt.IsGpt(device);
        bool isMbr = !isGpt && Mbr.IsMbr(device);
        if (!isGpt && !isMbr)
        {
            Terminal.Error("Disk has no partition table. Run 'mkmbr " + diskNumber + "' or 'mkgpt " + diskNumber + "' first.");
            return;
        }

        ulong firstUsable = isGpt ? Gpt.FirstUsableLba : MbrFirstPartitionLba;
        ulong sectorsPerMb = Units.BytesPerMiB / device.BlockSize;
        ulong sectorCount = (ulong)sizeMb * sectorsPerMb;

        ulong startSector;
        if (startSectorOrAuto.HasValue)
        {
            startSector = startSectorOrAuto.Value;
            if (startSector < firstUsable)
            {
                Terminal.Error("start_lba must be >= " + firstUsable + " on " + (isGpt ? "GPT" : "MBR") + " disks.");
                return;
            }

            if (!IsFreeRange(device, startSector, sectorCount))
            {
                return;
            }
        }
        else
        {
            startSector = FindAppendPoint(device, firstUsable);
        }

        if (startSector + sectorCount > device.BlockCount)
        {
            Terminal.Error("Partition does not fit on disk.");
            return;
        }

        if (!PartitionManager.Create(device, startSector, sectorCount, mbrSystemId: MbrFat32LbaSystemId, gptType: Gpt.BasicDataPartitionType))
        {
            Terminal.Error("Failed to create partition (no free slot or bad geometry).");
            return;
        }

        StorageManager.RescanPartitions(device);
        Terminal.Success("Partition created at LBA " + startSector + " (" + sectorCount + " sectors).");
    }

    /// <summary>Rejects, with a message, a range overlapping an existing partition on the same disk.</summary>
    private static bool IsFreeRange(IBlockDevice device, ulong startSector, ulong sectorCount)
    {
        for (int i = 0; i < StorageManager.Partitions.Count; i++)
        {
            Partition partition = StorageManager.Partitions[i];
            if (!ReferenceEquals(partition.Host, device))
            {
                continue;
            }

            ulong end = partition.StartSector + partition.BlockCount;
            if (startSector < end && startSector + sectorCount > partition.StartSector)
            {
                Terminal.Error("Range overlaps partition [" + partition.StartSector + ".." + end + ").");
                return false;
            }
        }

        return true;
    }

    /// <summary>The first sector past every existing partition on <paramref name="device"/>.</summary>
    private static ulong FindAppendPoint(IBlockDevice device, ulong firstUsable)
    {
        ulong startSector = firstUsable;
        for (int i = 0; i < StorageManager.Partitions.Count; i++)
        {
            Partition partition = StorageManager.Partitions[i];
            if (!ReferenceEquals(partition.Host, device))
            {
                continue;
            }

            ulong end = partition.StartSector + partition.BlockCount;
            if (end > startSector)
            {
                startSector = end;
            }
        }

        return startSector;
    }

    private static void DeletePartitionEntry(int diskNumber, int partitionNumber)
    {
        if (!StorageView.TryResolvePartition(diskNumber, partitionNumber, out Partition? partition))
        {
            Terminal.Error("Invalid disk/partition. Use 'lspart' to list.");
            return;
        }

        IBlockDevice host = partition.Host;
        ulong start = partition.StartSector;
        ulong count = partition.BlockCount;

        if (!PartitionManager.Delete(host, new PartitionManager.PartitionLocation(start, count)))
        {
            Terminal.Error("Failed to delete partition.");
            return;
        }

        StorageManager.RescanPartitions(host);
        Terminal.Success("Disk " + diskNumber + " partition " + partitionNumber + " deleted (LBA " + start + ", " + count + " sectors).");
    }
}
