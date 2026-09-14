using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.HAL.Vfs;
using Cosmos.Kernel.System.Vfs;

namespace DevKernel.Shell;

/// <summary>
/// Preconditions the file and directory commands share: something must be
/// mounted, and a path's parent directory must exist.
/// </summary>
internal static class VfsGuards
{
    /// <summary>True when at least one filesystem is mounted; otherwise prints the bring-up recipe.</summary>
    public static bool RequireMount()
    {
        if (VfsManager.Mounts.Count > 0)
        {
            return true;
        }

        Terminal.Warning("No filesystem mounted.");
        Terminal.Hint("To mount a filesystem and use 'ls':");
        Terminal.Info("  1. lsdisk                 - show attached disks");
        Terminal.Info("  2. lspart                 - list partitions on each disk");
        Terminal.Info("  3. mkgpt <d>              - if disk has no partition table");
        Terminal.Info("  4. mkpart <d> <mb>        - create a partition of <mb> MiB (or 'mkpart <d> <start> <mb>')");
        Terminal.Info("  5. format <d> <p> [fs]    - format disk <d> partition <p> (fs: fat | fat12 | fat16 | fat32)");
        Terminal.Info("  6. mount <d> <p> <path>   - mount disk <d> partition <p> at any path (e.g. /mnt)");
        Terminal.Info("  7. cd <mountpoint>        - change into it, then 'ls'");
        return false;
    }

    /// <summary>Opens the parent directory of <paramref name="fullPath"/> and yields its last component.</summary>
    public static bool TryOpenParent(string fullPath, [NotNullWhen(true)] out IVfsDirectoryHandle? parentDir, out string leaf)
    {
        (string parent, string leafName) = VfsPath.Split(fullPath);
        leaf = leafName;
        parentDir = null;

        if (string.IsNullOrEmpty(leaf))
        {
            Terminal.Error("Invalid path.");
            return false;
        }

        if (!VfsManager.TryOpenDirectory(parent, out parentDir))
        {
            Terminal.Error("Parent directory not found: " + parent);
            return false;
        }

        return true;
    }
}
