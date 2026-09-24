using System;
using System.Collections.Generic;
using System.IO;
using Cosmos.Kernel.HAL.Vfs;
using Cosmos.Kernel.System.Filesystems.Fat;
using Cosmos.Kernel.System.Storage;
using Cosmos.Kernel.System.Vfs;
using DevKernel.Shell;
using DevKernel.Storage;

namespace DevKernel.Commands;

/// <summary>
/// Putting a filesystem on a partition and attaching it to the VFS tree.
/// </summary>
internal static class MountCommands
{
    /// <summary>Help section these commands are listed under.</summary>
    private const string Category = "Filesystem";

    /// <summary>Filesystem type assumed when <c>format</c> is given no explicit one.</summary>
    private const string DefaultFsType = "fat";

    public static void Register(CommandShell shell)
    {
        shell.Register(
            Category,
            new ShellCommand
            {
                Name = "format",
                Usage = "format <disk> <part> [fs_type]",
                Description = "Format a partition (fs: fat | fat12 | fat16 | fat32, default fat)",
                MinArgs = 2,
                MaxArgs = 3,
                Execute = static (context, args) =>
                {
                    if (!args.TryGetInt(0, out int diskNumber) || !args.TryGetInt(1, out int partitionNumber))
                    {
                        args.PrintUsage();
                        return;
                    }

                    FormatPartition(diskNumber, partitionNumber, args.Count >= 3 ? args.GetLower(2) : DefaultFsType);
                },
            },
            new ShellCommand
            {
                Name = "mount",
                Usage = "mount <disk> <part> <mountpoint>",
                Description = "Mount a partition at <mountpoint> (e.g. mount 0 0 /mnt)",
                MinArgs = 3,
                MaxArgs = 3,
                Execute = static (context, args) =>
                {
                    if (!args.TryGetInt(0, out int diskNumber) || !args.TryGetInt(1, out int partitionNumber))
                    {
                        args.PrintUsage();
                        return;
                    }

                    MountPartition(diskNumber, partitionNumber, args[2]);
                },
            },
            new ShellCommand
            {
                Name = "umount",
                Usage = "umount <mountpoint>",
                Description = "Flush and detach the filesystem at <mountpoint> (do it before pulling out a USB disk)",
                MinArgs = 1,
                MaxArgs = 1,
                Execute = static (context, args) => Unmount(context, context.ResolveNormalized(args[0])),
            },
            new ShellCommand
            {
                Name = "mounts",
                Usage = "mounts",
                Description = "Show mounted filesystems",
                Execute = static (context, args) => ShowMountPoints(),
            });
    }

    /// <summary>Maps a <c>fat*</c> type name onto the format hint the FAT driver expects.</summary>
    private static bool TryResolveFsType(string fsType, out IVfsFormatOptions? options)
    {
        options = null;
        switch (fsType)
        {
            case DefaultFsType:
                return true;
            case "fat12":
                options = new FatFormatOptions { Type = FatType.Fat12 };
                return true;
            case "fat16":
                options = new FatFormatOptions { Type = FatType.Fat16 };
                return true;
            case "fat32":
                options = new FatFormatOptions { Type = FatType.Fat32 };
                return true;
            default:
                Terminal.Error("Unknown filesystem: " + fsType + ". Supported: fat, fat12, fat16, fat32.");
                return false;
        }
    }

    private static void FormatPartition(int diskNumber, int partitionNumber, string fsType)
    {
        if (!StorageView.TryResolvePartition(diskNumber, partitionNumber, out Partition? target))
        {
            Terminal.Error("Invalid disk/partition. Use 'lspart' to list.");
            return;
        }

        if (!TryResolveFsType(fsType, out IVfsFormatOptions? options))
        {
            return;
        }

        // Refuse formatting a mounted partition: the stale superblock would
        // flush cached FAT and directory state with the old geometry over the
        // fresh volume. VfsManager.TryFormat refuses this too; the loop is here
        // for the message that names the mount point.
        IReadOnlyList<VfsManager.VfsMount> mounts = VfsManager.Mounts;
        for (int i = 0; i < mounts.Count; i++)
        {
            VfsManager.VfsMount mount = mounts[i];
            if (ReferenceEquals(mount.Partition, target))
            {
                Terminal.Error($"Partition is mounted at {mount.MountPoint}. Run 'umount {mount.MountPoint}' first.");
                return;
            }
        }

        if (!VfsManager.TryFormat(FatBootstrap.DriverName, target, options))
        {
            ulong sizeMiB = Units.ToMiB(target.BlockCount * target.BlockSize);
            Terminal.Error("Format failed: partition is likely too small for " + fsType.ToUpper() +
                " (" + sizeMiB + " MiB). Try 'format " + diskNumber + " " + partitionNumber + " fat' to auto-pick a variant.");
            return;
        }

        Terminal.Success("Disk " + diskNumber + " partition " + partitionNumber + " formatted as " + fsType.ToUpper() + ".");
    }

    private static void MountPartition(int diskNumber, int partitionNumber, string mountPoint)
    {
        if (!StorageView.TryResolvePartition(diskNumber, partitionNumber, out Partition? target))
        {
            Terminal.Error("Invalid disk/partition. Use 'lspart' to list.");
            return;
        }

        if (string.IsNullOrEmpty(mountPoint) || mountPoint[0] != VfsPath.Separator)
        {
            Terminal.Error("Mount point must be an absolute path (e.g. /mnt).");
            return;
        }

        if (!VfsManager.TryMount(FatBootstrap.DriverName, target, MountFlags.None, mountPoint, out _))
        {
            Terminal.Error("Mount failed (not FAT or unreadable).");
            return;
        }

        Terminal.Success("Disk " + diskNumber + " partition " + partitionNumber + " mounted at " + mountPoint);
    }

    /// <summary>
    /// Detaches the filesystem at <paramref name="mountPoint"/> after flushing
    /// it, and leaves it when the shell was inside it.
    /// </summary>
    private static void Unmount(ShellContext context, string mountPoint)
    {
        bool unmounted;
        try
        {
            unmounted = VfsManager.TryUnmount(mountPoint);
        }
        catch (IOException ex)
        {
            // The mount is gone already; only its last writes may be.
            LeaveMountPoint(context, mountPoint);
            Terminal.Error($"Unmounted {mountPoint}, but its last writes may be lost: {ex.Message}");
            return;
        }

        if (!unmounted)
        {
            Terminal.Error($"Nothing is mounted at {mountPoint}. Use 'mounts' to list.");
            return;
        }

        LeaveMountPoint(context, mountPoint);
        Terminal.Success($"Unmounted {mountPoint}");
    }

    /// <summary>Moves the shell to the root when its directory was under <paramref name="mountPoint"/>.</summary>
    private static void LeaveMountPoint(ShellContext context, string mountPoint)
    {
        if (context.Cwd == mountPoint || context.Cwd.StartsWith($"{mountPoint}{VfsPath.Separator}", StringComparison.Ordinal))
        {
            context.Cwd = VfsPath.Root;
        }
    }

    private static void ShowMountPoints()
    {
        Terminal.Header("Mounted Filesystems:");

        IReadOnlyList<VfsManager.VfsMount> mounts = VfsManager.Mounts;
        if (mounts.Count == 0)
        {
            Terminal.Warning("No filesystems mounted.");
            return;
        }

        for (int i = 0; i < mounts.Count; i++)
        {
            VfsManager.VfsMount mount = mounts[i];
            Console.Write("  ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(mount.MountPoint);
            Console.ResetColor();
            Console.Write(" -> ");

            PrintMountSource(mount);

            if (VfsManager.TryStatFs(mount.MountPoint, out VfsStatFs stats))
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write(" (" + Units.ToMiB(stats.Blocks * stats.BlockSize) + " MiB)");
                Console.ResetColor();
            }

            Console.WriteLine();
        }
    }

    /// <summary>
    /// Names the backing store of a mount. <c>mount.Partition</c> is the
    /// partition itself, so it keeps naming the same range across the rescans
    /// mkpart/rmpart/format trigger; only the disk and slot it is reported
    /// under are looked up fresh.
    /// </summary>
    private static void PrintMountSource(VfsManager.VfsMount mount)
    {
        Partition? partition = mount.Partition;
        if (partition is null || !StorageView.TryDescribePartition(partition, out int diskNumber, out int partitionNumber))
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(mount.Name);
            Console.ResetColor();
            return;
        }

        Console.ForegroundColor = ConsoleColor.White;
        Console.Write("disk " + diskNumber + " part " + partitionNumber);
        Console.ResetColor();
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.Write("  " + partition.Name);
        Console.ResetColor();
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.Write("  " + StorageView.DetectFilesystem(partition));
        Console.ResetColor();
    }
}
