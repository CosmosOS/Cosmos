using System;
using System.Collections.Generic;
using Cosmos.Kernel.HAL.Vfs;
using Cosmos.Kernel.System.Vfs;
using DevKernel.Shell;

namespace DevKernel.Commands;

/// <summary>
/// The POSIX-flavored file and directory commands, operating on whatever the
/// VFS has mounted.
/// </summary>
internal static class FileCommands
{
    /// <summary>Help section these commands are listed under.</summary>
    private const string Category = "Files";

    /// <summary>Column width (chars) used to pad entry names in the ls listing.</summary>
    private const int DirEntryNameColumnWidth = 24;

    /// <summary>Column width (chars) used to right-align file sizes in the ls listing.</summary>
    private const int FileSizeColumnWidth = 10;

    /// <summary>Size (bytes) of each read chunk used by the cat command.</summary>
    private const int CatChunkSizeBytes = 512;

    /// <summary>
    /// Mode bits of a file the shell creates: a regular file, owner readable and writable.
    /// Not const: a const field typed as an enum from another assembly forces Mono.Cecil to
    /// resolve that assembly when the patcher rewrites this one, which it cannot do.
    /// </summary>
    private static readonly VfsMode s_newFileMode = VfsMode.RegularFile | VfsMode.OwnerRead | VfsMode.OwnerWrite;

    /// <summary>Mode bits of a directory the shell creates: owner readable, writable and traversable.</summary>
    private static readonly VfsMode s_newDirectoryMode = VfsMode.Directory | VfsMode.OwnerRead | VfsMode.OwnerWrite | VfsMode.OwnerExecute;

    /// <summary>Substitute drawn for a byte that would not render on the console.</summary>
    private const char UnprintableGlyph = '.';

    /// <summary>Size a file is truncated to before <c>write</c> replaces its contents.</summary>
    private const ulong TruncatedSize = 0;

    public static void Register(CommandShell shell)
    {
        shell.Register(
            Category,
            new ShellCommand
            {
                Name = "cd",
                Usage = "cd <path>",
                Description = "Change current directory",
                MaxArgs = 1,
                Execute = static (context, args) => ChangeDirectory(context, args.GetOrDefault(0, VfsPath.Root)),
            },
            new ShellCommand
            {
                Name = "pwd",
                Usage = "pwd",
                Description = "Print current directory",
                Execute = static (context, args) => Terminal.Info(context.Cwd),
            },
            new ShellCommand
            {
                Name = "ls",
                Aliases = ["dir"],
                Usage = "ls [path]",
                Description = "List directory contents (defaults to cwd)",
                MaxArgs = 1,
                Execute = static (context, args) => ListDirectory(context, args.GetOrDefault(0, context.Cwd)),
            },
            new ShellCommand
            {
                Name = "cat",
                Aliases = ["type"],
                Usage = "cat <path>",
                Description = "Display file contents",
                MinArgs = 1,
                MaxArgs = 1,
                Execute = static (context, args) => DisplayFileContents(context, args[0]),
            },
            new ShellCommand
            {
                Name = "mkdir",
                Usage = "mkdir <path>",
                Description = "Create directory",
                MinArgs = 1,
                MaxArgs = 1,
                Execute = static (context, args) => CreateDirectory(context, args[0]),
            },
            new ShellCommand
            {
                Name = "touch",
                Usage = "touch <path>",
                Description = "Create empty file",
                MinArgs = 1,
                MaxArgs = 1,
                Execute = static (context, args) => CreateEmptyFile(context, args[0]),
            },
            new ShellCommand
            {
                Name = "rm",
                Aliases = ["del"],
                Usage = "rm <path>",
                Description = "Delete file or empty directory",
                MinArgs = 1,
                MaxArgs = 1,
                Execute = static (context, args) => DeleteEntry(context, args[0]),
            },
            new ShellCommand
            {
                Name = "write",
                Usage = "write <path> <text>",
                Description = "Write text to file (overwrite)",
                MinArgs = 2,
                MaxArgs = ShellCommand.UnlimitedArgs,
                Execute = static (context, args) => WriteToFile(context, args[0], args.Join(1)),
            });
    }

    private static void ChangeDirectory(ShellContext context, string path)
    {
        string target = context.ResolveNormalized(path);

        if (target != VfsPath.Root)
        {
            if (!VfsManager.TryOpenDirectory(target, out IVfsDirectoryHandle? dir))
            {
                Terminal.Error(VfsManager.TryStat(target, out _)
                    ? "Not a directory: " + target
                    : "No such directory: " + target);
                return;
            }
        }

        context.Cwd = target;
    }

    private static void ListDirectory(ShellContext context, string path)
    {
        if (!VfsGuards.RequireMount())
        {
            return;
        }

        string fullPath = context.ResolveNormalized(path);

        // No mount lives at "/" itself, so plain `ls` would fail even when
        // filesystems are mounted at sub-paths. List the available mount points
        // so the user can `cd` into one rather than see a cryptic
        // "Cannot open directory: /".
        if (fullPath == VfsPath.Root && !VfsManager.TryGetMount(VfsPath.Root, out _))
        {
            PrintAvailableMounts();
            return;
        }

        if (!VfsManager.TryOpenDirectory(fullPath, out IVfsDirectoryHandle? dir))
        {
            Terminal.Error("Cannot open directory: " + fullPath);
            return;
        }

        if (!dir.TryReadDir(out IReadOnlyList<IVfsInode> entries))
        {
            Terminal.Error("ReadDir failed.");
            return;
        }

        Terminal.Header("Contents of " + fullPath + ":");

        if (entries.Count == 0)
        {
            Terminal.Warning("  (empty)");
            return;
        }

        for (int i = 0; i < entries.Count; i++)
        {
            PrintDirectoryEntry(entries[i]);
        }
    }

    private static void PrintAvailableMounts()
    {
        Terminal.Hint("Available mount points (cd into one):");

        IReadOnlyList<VfsManager.VfsMount> mounts = VfsManager.Mounts;
        for (int i = 0; i < mounts.Count; i++)
        {
            VfsManager.VfsMount mount = mounts[i];
            Console.Write("  ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(mount.MountPoint);
            Console.ResetColor();
            Console.Write(" (");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(mount.Name);
            Console.ResetColor();
            Console.WriteLine(")");
        }
    }

    private static void PrintDirectoryEntry(IVfsInode entry)
    {
        VfsStat stat = default;
        bool haveStat = entry.InodeOperations.GetAttr(entry, out stat);
        bool isDirectory = haveStat && stat.IsDirectory;

        Console.Write("  ");
        if (isDirectory)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("[DIR] ");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("[FILE]");
        }

        Console.ResetColor();
        Console.Write(" ");
        Console.Write(entry.Name.PadRight(DirEntryNameColumnWidth));
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.Write(stat.Size.ToString().PadLeft(FileSizeColumnWidth));
        Console.Write(" bytes");
        Console.ResetColor();
        Console.WriteLine();
    }

    private static void DisplayFileContents(ShellContext context, string path)
    {
        if (!VfsGuards.RequireMount())
        {
            return;
        }

        string fullPath = context.Resolve(path);
        if (!VfsManager.TryOpenFile(fullPath, out IVfsFileHandle? file))
        {
            Terminal.Error("File not found: " + fullPath);
            return;
        }

        try
        {
            Span<byte> buffer = stackalloc byte[CatChunkSizeBytes];
            while (true)
            {
                long read = file.Read(buffer);
                if (read <= 0)
                {
                    break;
                }

                for (int i = 0; i < read; i++)
                {
                    char c = (char)buffer[i];
                    if (c == '\n' || c == '\r' || Ascii.IsPrintable(c))
                    {
                        Console.Write(c.ToString());
                    }
                    else
                    {
                        Console.Write(UnprintableGlyph);
                    }
                }
            }

            Console.WriteLine();
        }
        finally
        {
            file.Dispose();
        }
    }

    private static void CreateDirectory(ShellContext context, string path)
    {
        if (!VfsGuards.RequireMount())
        {
            return;
        }

        string fullPath = context.Resolve(path);
        if (!VfsGuards.TryOpenParent(fullPath, out IVfsDirectoryHandle? parentDir, out string leaf))
        {
            return;
        }

        if (!parentDir.TryCreateDirectory(leaf, s_newDirectoryMode, out _))
        {
            Terminal.Error("Mkdir failed.");
            return;
        }

        Terminal.Success("Directory created: " + fullPath);
    }

    private static void CreateEmptyFile(ShellContext context, string path)
    {
        if (!VfsGuards.RequireMount())
        {
            return;
        }

        string fullPath = context.Resolve(path);
        if (!VfsGuards.TryOpenParent(fullPath, out IVfsDirectoryHandle? parentDir, out string leaf))
        {
            return;
        }

        if (!parentDir.TryCreateFile(leaf, s_newFileMode, out _))
        {
            Terminal.Error("Create file failed.");
            return;
        }

        Terminal.Success("File created: " + fullPath);
    }

    private static void DeleteEntry(ShellContext context, string path)
    {
        if (!VfsGuards.RequireMount())
        {
            return;
        }

        string fullPath = context.Resolve(path);
        if (!VfsGuards.TryOpenParent(fullPath, out IVfsDirectoryHandle? parentDir, out string leaf))
        {
            return;
        }

        if (parentDir.TryUnlink(leaf))
        {
            Terminal.Success("File deleted: " + fullPath);
            return;
        }

        if (parentDir.TryRemoveDirectory(leaf))
        {
            Terminal.Success("Directory deleted: " + fullPath);
            return;
        }

        Terminal.Error("Path not found or not empty: " + fullPath);
    }

    private static void WriteToFile(ShellContext context, string path, string text)
    {
        if (!VfsGuards.RequireMount())
        {
            return;
        }

        string fullPath = context.Resolve(path);

        if (!VfsManager.TryOpenFile(fullPath, out IVfsFileHandle? file))
        {
            (string parent, string leaf) = VfsPath.Split(fullPath);
            if (string.IsNullOrEmpty(leaf)
                || !VfsManager.TryOpenDirectory(parent, out IVfsDirectoryHandle? parentDir)
                || !parentDir.TryCreateFile(leaf, s_newFileMode, out _)
                || !VfsManager.TryOpenFile(fullPath, out file))
            {
                Terminal.Error("Cannot open or create: " + fullPath);
                return;
            }
        }
        else if (!Truncate(file))
        {
            Terminal.Error("Cannot truncate: " + fullPath);
            file.Dispose();
            return;
        }

        try
        {
            byte[] bytes = new byte[text.Length];
            for (int i = 0; i < text.Length; i++)
            {
                bytes[i] = (byte)text[i];
            }

            long written = file.Write(bytes);

            if (!file.TryFlush())
            {
                Terminal.Error("Wrote " + written + " bytes to " + fullPath + " but the flush failed");
                return;
            }

            Terminal.Success("Wrote " + written + " bytes to " + fullPath);
        }
        finally
        {
            file.Dispose();
        }
    }

    /// <summary>
    /// <c>write</c> is advertised as an overwrite: without this the old tail past
    /// the new text survives and the next <c>cat</c> looks corrupt.
    /// </summary>
    private static bool Truncate(IVfsFileHandle file)
    {
        VfsStat zeroSize = default;
        zeroSize.Mode = VfsMode.RegularFile;
        zeroSize.Size = TruncatedSize;
        return file.TrySetAttr(SetAttrFlags.Size, zeroSize);
    }
}
