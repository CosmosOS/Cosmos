namespace Cosmos.Kernel.Tests.File;

// Using directives sit after the namespace declaration on purpose: the
// namespace's own last segment is "File", which would otherwise shadow
// System.IO.File for every simple-name lookup in this file. The global::
// qualifier is required too — from inside this namespace a plain "System"
// binds to Cosmos.Kernel.System.
using global::System;
using global::System.IO;
using Cosmos.Kernel.System.Diagnostics;
using Cosmos.Kernel.HAL.Vfs;
using Cosmos.Kernel.System.Filesystems.Fat;
using Cosmos.Kernel.System.Vfs;
using Cosmos.TestRunner.Framework;
using Sys = Cosmos.Kernel.System;
using TR = Cosmos.TestRunner.Framework.TestRunner;

/// <summary>
/// Exercises .NET's own System.IO surface (File, Directory, FileStream,
/// StreamReader/Writer, FileInfo/DirectoryInfo, Path, current directory) on
/// top of the VFS: the BCL code runs unmodified and reaches the FAT driver
/// through the Interop.Sys plugs in Cosmos.Kernel.Plugs.
/// </summary>
public class Kernel : Sys.Kernel
{
    /// <summary>Exact TR.Run cell count — the harness synthesizes failures
    /// for missing tests, so a mid-suite hang can't report ALL TESTS PASSED.</summary>
    private const ushort ExpectedTestCount = 37;

    private const string DriverName = "fat-file-test";
    private const string MountPoint = "/mnt";

    /// <summary>Payload sizes: small buffers stay inside one 4 KiB cluster,
    /// the large one spans 24 clusters to exercise chain walking.</summary>
    private const int SmallBinaryBytes = 8 * 1024;
    private const int MultiClusterBytes = 96 * 1024;
    private const int StreamPayloadBytes = 256;

    /// <summary>Offsets for the sparse-write cells: the far write position and
    /// a probe inside the zero-filled gap it must leave behind.</summary>
    private const int HoleWriteOffset = 9000;
    private const int HoleProbeOffset = 5000;
    private const int GrownLength = 5000;
    private const int GrowProbeOffset = 4000;
    private const int ShrunkLength = 100;

    protected override void BeforeRun()
    {
        Log.WriteString("[FileTests] BeforeRun() reached!\n");

        TR.Start("System.IO File Tests", expectedTests: ExpectedTestCount);

        MemoryBlockDevice disk = FileTestVolume.Create("MEMFILE16");
        FatFilesystemType driver = new(disk);

        // ---------- before any mount ----------

        TR.Run("Test_NoMount_Graceful", () =>
        {
            // With nothing mounted, System.IO must degrade cleanly: the
            // virtual root exists and enumerates empty, everything else is
            // reported missing, and writes fail with an exception instead of
            // faulting the kernel.
            Assert.True(Directory.Exists("/"), "virtual root should exist with no mounts");
            Assert.Equal(0, Directory.GetDirectories("/").Length);
            Assert.False(File.Exists("/nofs/file.txt"));
            Assert.False(Directory.Exists("/nofs"));
            Assert.True(ReadMissingThrowsFileNotFound("/missing.txt"),
                "read of a missing file in the virtual root should raise FileNotFoundException");
            Assert.True(ReadInMissingDirectoryThrows("/nofs/file.txt"),
                "read under a missing directory should raise DirectoryNotFoundException");
            Assert.True(WriteInMissingDirectoryThrows("/nofs/file.txt"),
                "write with no mount should raise DirectoryNotFoundException");
            Assert.True(WriteThrowsIOException("/direct.txt"),
                "write into the virtual root should raise IOException");
        });

        // ---------- mount ----------

        TR.Run("Test_Mount_Volume", () =>
        {
            Assert.True(VfsManager.RegisterFilesystem(DriverName, driver));
            Assert.True(VfsManager.TryMount(DriverName, "", MountFlags.None, MountPoint, out VfsManager.VfsMount? mount));
            Assert.NotNull(mount);
        });

        TR.Run("Test_Directory_Exists_Roots", () =>
        {
            Assert.True(Directory.Exists("/"), "virtual root should exist");
            Assert.True(Directory.Exists(MountPoint), "mount point should exist");
            Assert.False(Directory.Exists(MountPoint + "/absent"), "missing directory reported as existing");
        });

        TR.Run("Test_Mount_Boundary", () =>
        {
            // "/mntx" must not resolve inside the "/mnt" mount even when
            // "/mnt/x" exists (VfsManager.MountCovers segment-boundary check).
            Directory.CreateDirectory("/mnt/x");
            Assert.False(Directory.Exists("/mntx"), "path outside the mount resolved into it");
            Assert.False(File.Exists("/mntx"));
        });

        // ---------- File basics ----------

        TR.Run("Test_File_WriteAllText_ReadAllText", () =>
        {
            File.WriteAllText("/mnt/hello.txt", "Hello from System.IO\nsecond line");
            Assert.Equal("Hello from System.IO\nsecond line", File.ReadAllText("/mnt/hello.txt"));
        });

        TR.Run("Test_File_Exists", () =>
        {
            Assert.True(File.Exists("/mnt/hello.txt"));
            Assert.False(File.Exists("/mnt/nothing-here.txt"));
            Assert.False(File.Exists(MountPoint), "a directory must not count as a file");
        });

        TR.Run("Test_File_WriteAllBytes_ReadAllBytes", () =>
        {
            byte[] payload = MakePattern(SmallBinaryBytes, 7);
            File.WriteAllBytes("/mnt/data.bin", payload);
            Assert.Equal(payload, File.ReadAllBytes("/mnt/data.bin"));
        });

        TR.Run("Test_File_AppendAllText", () =>
        {
            File.WriteAllText("/mnt/append.txt", "abc");
            File.AppendAllText("/mnt/append.txt", "def");
            Assert.Equal("abcdef", File.ReadAllText("/mnt/append.txt"));
        });

        TR.Run("Test_File_WriteAllLines_ReadAllLines", () =>
        {
            string[] lines = ["alpha", "beta", "gamma"];
            File.WriteAllLines("/mnt/lines.txt", lines);
            string[] readBack = File.ReadAllLines("/mnt/lines.txt");
            Assert.Equal(3, readBack.Length);
            Assert.Equal("alpha", readBack[0]);
            Assert.Equal("beta", readBack[1]);
            Assert.Equal("gamma", readBack[2]);
        });

        // ---------- File copy/move/delete ----------

        TR.Run("Test_File_Copy", () =>
        {
            Directory.CreateDirectory("/mnt/copy");
            File.WriteAllText("/mnt/copy/src.txt", "copy me");
            File.Copy("/mnt/copy/src.txt", "/mnt/copy/dst.txt");
            Assert.True(File.Exists("/mnt/copy/src.txt"), "source must survive a copy");
            Assert.Equal("copy me", File.ReadAllText("/mnt/copy/dst.txt"));
        });

        TR.Run("Test_File_Copy_Overwrite", () =>
        {
            File.WriteAllText("/mnt/copy/replaced.txt", "old content that is longer");
            File.Copy("/mnt/copy/src.txt", "/mnt/copy/replaced.txt", overwrite: true);
            Assert.Equal("copy me", File.ReadAllText("/mnt/copy/replaced.txt"));
        });

        TR.Run("Test_File_Copy_OntoExisting_Throws", () =>
        {
            Assert.True(
                CopyOntoExistingThrows("/mnt/copy/src.txt", "/mnt/copy/dst.txt"),
                "File.Copy without overwrite must throw IOException on an existing destination");
        });

        TR.Run("Test_File_Move", () =>
        {
            Directory.CreateDirectory("/mnt/move");
            File.WriteAllText("/mnt/move/src.txt", "move me");
            File.Move("/mnt/move/src.txt", "/mnt/move/moved.txt");
            Assert.False(File.Exists("/mnt/move/src.txt"));
            Assert.Equal("move me", File.ReadAllText("/mnt/move/moved.txt"));
        });

        TR.Run("Test_File_Move_OntoExisting_Throws", () =>
        {
            File.WriteAllText("/mnt/move/a.txt", "content a");
            File.WriteAllText("/mnt/move/b.txt", "content b");
            Assert.True(
                MoveOntoExistingThrows("/mnt/move/a.txt", "/mnt/move/b.txt"),
                "File.Move must throw IOException when the destination exists");
            Assert.Equal("content b", File.ReadAllText("/mnt/move/b.txt"));
        });

        TR.Run("Test_File_Delete", () =>
        {
            File.WriteAllText("/mnt/delete-me.txt", "x");
            File.Delete("/mnt/delete-me.txt");
            Assert.False(File.Exists("/mnt/delete-me.txt"));
            Assert.True(
                DeleteMissingFileDoesNotThrow("/mnt/delete-me.txt"),
                "File.Delete on a missing file must be a silent no-op");
        });

        TR.Run("Test_File_Open_Missing_Throws", () =>
        {
            Assert.True(
                OpenMissingThrowsFileNotFound("/mnt/no-such-file.txt"),
                "File.Open(FileMode.Open) on a missing file must throw FileNotFoundException");
        });

        TR.Run("Test_File_Write_MissingDirectory_Throws", () =>
        {
            Assert.True(
                WriteInMissingDirectoryThrows("/mnt/absentdir/file.txt"),
                "writing under a missing directory must throw DirectoryNotFoundException");
        });

        // ---------- FileStream ----------

        TR.Run("Test_FileStream_Write_Read_Seek", () =>
        {
            Directory.CreateDirectory("/mnt/fs");
            byte[] payload = MakePattern(StreamPayloadBytes, 3);
            using (FileStream stream = new("/mnt/fs/stream.bin", FileMode.Create, FileAccess.ReadWrite))
            {
                stream.Write(payload, 0, payload.Length);
                Assert.Equal((long)StreamPayloadBytes, stream.Length);
                Assert.Equal((long)StreamPayloadBytes, stream.Position);

                stream.Seek(0, SeekOrigin.Begin);
                Assert.Equal(payload[0], (byte)stream.ReadByte());

                stream.Seek(99, SeekOrigin.Begin);
                Assert.Equal(payload[99], (byte)stream.ReadByte());

                stream.Seek(-1, SeekOrigin.End);
                Assert.Equal(payload[StreamPayloadBytes - 1], (byte)stream.ReadByte());
            }
        });

        TR.Run("Test_FileStream_CreateNew_Existing_Throws", () =>
        {
            Assert.True(
                CreateNewOnExistingThrows("/mnt/fs/stream.bin"),
                "FileMode.CreateNew on an existing file must throw IOException");
        });

        TR.Run("Test_FileStream_Truncate", () =>
        {
            File.WriteAllBytes("/mnt/fs/trunc.bin", MakePattern(100, 5));
            using (FileStream stream = new("/mnt/fs/trunc.bin", FileMode.Truncate, FileAccess.Write))
            {
                Assert.Equal(0L, stream.Length);
                stream.Write([1, 2, 3], 0, 3);
            }

            byte[] after = File.ReadAllBytes("/mnt/fs/trunc.bin");
            Assert.Equal(3, after.Length);
            Assert.Equal((byte)1, after[0]);
            Assert.Equal((byte)3, after[2]);
        });

        TR.Run("Test_FileStream_Append", () =>
        {
            File.WriteAllBytes("/mnt/fs/append.bin", [0x41, 0x42]);
            using (FileStream stream = new("/mnt/fs/append.bin", FileMode.Append, FileAccess.Write))
            {
                stream.Write([0x43, 0x44], 0, 2);
            }

            byte[] combined = File.ReadAllBytes("/mnt/fs/append.bin");
            Assert.Equal(new byte[] { 0x41, 0x42, 0x43, 0x44 }, combined);
        });

        TR.Run("Test_FileStream_SetLength", () =>
        {
            using (FileStream stream = new("/mnt/fs/setlen.bin", FileMode.Create, FileAccess.ReadWrite))
            {
                stream.Write(MakePattern(16, 0xAA), 0, 16);

                stream.SetLength(GrownLength);
                Assert.Equal((long)GrownLength, stream.Length);
                stream.Seek(GrowProbeOffset, SeekOrigin.Begin);
                Assert.Equal(0, stream.ReadByte());

                stream.SetLength(ShrunkLength);
                Assert.Equal((long)ShrunkLength, stream.Length);
            }

            Assert.Equal(ShrunkLength, File.ReadAllBytes("/mnt/fs/setlen.bin").Length);
        });

        TR.Run("Test_FileStream_Seek_Past_End_Zero_Gap", () =>
        {
            using (FileStream stream = new("/mnt/fs/hole.bin", FileMode.Create, FileAccess.ReadWrite))
            {
                stream.Write([9, 9, 9, 9], 0, 4);
                stream.Seek(HoleWriteOffset, SeekOrigin.Begin);
                stream.Write([1, 2, 3, 4], 0, 4);
                Assert.Equal((long)(HoleWriteOffset + 4), stream.Length);

                stream.Seek(HoleProbeOffset, SeekOrigin.Begin);
                Assert.Equal(0, stream.ReadByte());
            }
        });

        TR.Run("Test_FileStream_MultiCluster_File", () =>
        {
            byte[] payload = MakePattern(MultiClusterBytes, 11);
            File.WriteAllBytes("/mnt/fs/big.bin", payload);
            Assert.Equal(payload, File.ReadAllBytes("/mnt/fs/big.bin"));
        });

        // ---------- StreamWriter / StreamReader ----------

        TR.Run("Test_StreamWriter_StreamReader", () =>
        {
            Directory.CreateDirectory("/mnt/stream");
            using (StreamWriter writer = new("/mnt/stream/log.txt"))
            {
                writer.WriteLine("first");
                writer.WriteLine("second");
                writer.Write("third");
            }

            using (StreamReader reader = new("/mnt/stream/log.txt"))
            {
                Assert.True(string.Equals("first", reader.ReadLine(), StringComparison.Ordinal));
                Assert.True(string.Equals("second", reader.ReadLine(), StringComparison.Ordinal));
                Assert.True(string.Equals("third", reader.ReadLine(), StringComparison.Ordinal));
                Assert.Null(reader.ReadLine());
            }
        });

        // ---------- Directory ----------

        TR.Run("Test_Directory_Create_Nested", () =>
        {
            DirectoryInfo created = Directory.CreateDirectory("/mnt/tree/a/b/c");
            Assert.True(created.Exists);
            Assert.True(Directory.Exists("/mnt/tree/a"));
            Assert.True(Directory.Exists("/mnt/tree/a/b"));
            Assert.True(Directory.Exists("/mnt/tree/a/b/c"));
        });

        TR.Run("Test_Directory_GetFiles", () =>
        {
            Directory.CreateDirectory("/mnt/list/sub");
            File.WriteAllText("/mnt/list/a.txt", "a");
            File.WriteAllText("/mnt/list/b.txt", "b");
            File.WriteAllText("/mnt/list/c.txt", "c");

            string[] files = Directory.GetFiles("/mnt/list");
            Assert.Equal(3, files.Length);
            Assert.True(ContainsString(files, "/mnt/list/a.txt"), "GetFiles must return full paths");
            Assert.True(ContainsString(files, "/mnt/list/b.txt"));
            Assert.True(ContainsString(files, "/mnt/list/c.txt"));
        });

        TR.Run("Test_Directory_GetDirectories", () =>
        {
            string[] directories = Directory.GetDirectories("/mnt/list");
            Assert.Equal(1, directories.Length);
            Assert.True(ContainsString(directories, "/mnt/list/sub"));
        });

        TR.Run("Test_Directory_Enumerate_Pattern", () =>
        {
            Directory.CreateDirectory("/mnt/pat");
            File.WriteAllText("/mnt/pat/x.txt", "x");
            File.WriteAllText("/mnt/pat/y.txt", "y");
            File.WriteAllText("/mnt/pat/z.bin", "z");

            string[] matches = Directory.GetFiles("/mnt/pat", "*.txt");
            Assert.Equal(2, matches.Length);
            Assert.True(ContainsString(matches, "/mnt/pat/x.txt"));
            Assert.True(ContainsString(matches, "/mnt/pat/y.txt"));
        });

        TR.Run("Test_Directory_Delete_NonEmpty_Throws", () =>
        {
            Assert.True(
                DeleteNonEmptyDirectoryThrows("/mnt/list"),
                "non-recursive Directory.Delete on a non-empty directory must throw IOException");
            Assert.True(Directory.Exists("/mnt/list"));
        });

        TR.Run("Test_Directory_Delete_Recursive", () =>
        {
            Directory.Delete("/mnt/tree", recursive: true);
            Assert.False(Directory.Exists("/mnt/tree"));
        });

        TR.Run("Test_Directory_Move", () =>
        {
            Directory.CreateDirectory("/mnt/mv/src");
            File.WriteAllText("/mnt/mv/src/inner.txt", "inside");
            Directory.Move("/mnt/mv/src", "/mnt/mv/dst");
            Assert.False(Directory.Exists("/mnt/mv/src"));
            Assert.True(Directory.Exists("/mnt/mv/dst"));
            Assert.Equal("inside", File.ReadAllText("/mnt/mv/dst/inner.txt"));
        });

        TR.Run("Test_Directory_Root_Listing", () =>
        {
            string[] roots = Directory.GetDirectories("/");
            Assert.True(roots.Length >= 1, "root listing should surface the mount points");
            Assert.True(ContainsString(roots, MountPoint));
        });

        // ---------- FileInfo / DirectoryInfo ----------

        TR.Run("Test_FileInfo_Properties", () =>
        {
            Directory.CreateDirectory("/mnt/info");
            File.WriteAllBytes("/mnt/info/data.bin", MakePattern(1234, 9));

            FileInfo info = new("/mnt/info/data.bin");
            Assert.True(info.Exists);
            Assert.Equal(1234L, info.Length);
            Assert.True(string.Equals("data.bin", info.Name, StringComparison.Ordinal));
            Assert.True(string.Equals(".bin", info.Extension, StringComparison.Ordinal));

            info.Delete();
            Assert.False(File.Exists("/mnt/info/data.bin"));
        });

        TR.Run("Test_DirectoryInfo_Enumerate", () =>
        {
            DirectoryInfo info = Directory.CreateDirectory("/mnt/info2");
            File.WriteAllText("/mnt/info2/f1.txt", "1");
            File.WriteAllText("/mnt/info2/f2.txt", "2");
            Directory.CreateDirectory("/mnt/info2/nested");

            FileInfo[] files = info.GetFiles();
            DirectoryInfo[] directories = info.GetDirectories();
            Assert.Equal(2, files.Length);
            Assert.Equal(1, directories.Length);
            Assert.True(string.Equals("nested", directories[0].Name, StringComparison.Ordinal));
        });

        // ---------- attributes ----------

        TR.Run("Test_File_Attributes", () =>
        {
            Directory.CreateDirectory("/mnt/attr");
            File.WriteAllText("/mnt/attr/ro.txt", "x");

            Assert.True((File.GetAttributes(MountPoint) & FileAttributes.Directory) != 0);
            Assert.True((File.GetAttributes("/mnt/attr/ro.txt") & FileAttributes.Directory) == 0);

            File.SetAttributes("/mnt/attr/ro.txt", FileAttributes.ReadOnly);
            Assert.True((File.GetAttributes("/mnt/attr/ro.txt") & FileAttributes.ReadOnly) != 0,
                "ReadOnly must map onto the FAT read-only attribute");

            File.SetAttributes("/mnt/attr/ro.txt", FileAttributes.Normal);
            Assert.True((File.GetAttributes("/mnt/attr/ro.txt") & FileAttributes.ReadOnly) == 0);
        });

        // ---------- current directory / relative paths ----------

        TR.Run("Test_CurrentDirectory_Relative_Paths", () =>
        {
            Assert.True(string.Equals("/", Directory.GetCurrentDirectory(), StringComparison.Ordinal));

            Directory.CreateDirectory("/mnt/cwd");
            Directory.SetCurrentDirectory("/mnt/cwd");
            File.WriteAllText("rel.txt", "relative");
            Assert.True(File.Exists("/mnt/cwd/rel.txt"), "relative paths must resolve against the current directory");
            Assert.True(string.Equals("/mnt/cwd/x.txt", Path.GetFullPath("x.txt"), StringComparison.Ordinal));
            Assert.True(string.Equals("/mnt/cwd/y.txt", Path.GetFullPath("sub/../y.txt"), StringComparison.Ordinal));

            Directory.SetCurrentDirectory("/");
        });

        TR.Finish();

        Log.WriteString("\n[Tests Complete - System Halting]\n");
    }

    protected override void Run()
    {
        // All tests ran in BeforeRun; stop the main loop after one iteration.
        Stop();
    }

    protected override void AfterRun()
    {
        // Flush coverage data and signal QEMU to terminate.
        TR.Complete();
        Sys.Power.Halt();
    }

    // ---------- helpers ----------

    private static byte[] MakePattern(int length, byte salt)
    {
        byte[] data = new byte[length];
        for (int i = 0; i < length; i++)
        {
            data[i] = (byte)((i * 31 + salt) & 0xFF);
        }

        return data;
    }

    private static bool ContainsString(string[] values, string expected)
    {
        for (int i = 0; i < values.Length; i++)
        {
            if (string.Equals(values[i], expected, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    // Expected-exception probes live in their own methods with a single
    // try/catch each (multiple EH regions per method have misbehaved on
    // arm64; see the Storage suite).

    private static bool ReadMissingThrowsFileNotFound(string path)
    {
        try
        {
            File.ReadAllText(path);
            return false;
        }
        catch (Exception e)
        {
            return e is FileNotFoundException;
        }
    }

    /// <summary>Creating a file in the virtual root: the parent "directory" exists but
    /// nothing is mounted there, so the plug reports EROFS and the BCL turns
    /// it into a plain IOException ("read-only file system").</summary>
    private static bool WriteThrowsIOException(string path)
    {
        try
        {
            File.WriteAllText(path, "x");
            return false;
        }
        catch (Exception e)
        {
            return e is IOException && e is not FileNotFoundException && e is not DirectoryNotFoundException;
        }
    }

    private static bool OpenMissingThrowsFileNotFound(string path)
    {
        try
        {
            using FileStream stream = File.Open(path, FileMode.Open);
            return false;
        }
        catch (Exception e)
        {
            return e is FileNotFoundException;
        }
    }

    private static bool ReadInMissingDirectoryThrows(string path)
    {
        try
        {
            File.ReadAllText(path);
            return false;
        }
        catch (Exception e)
        {
            return e is DirectoryNotFoundException;
        }
    }

    private static bool WriteInMissingDirectoryThrows(string path)
    {
        try
        {
            File.WriteAllText(path, "x");
            return false;
        }
        catch (Exception e)
        {
            return e is DirectoryNotFoundException;
        }
    }

    private static bool CopyOntoExistingThrows(string source, string destination)
    {
        try
        {
            File.Copy(source, destination, overwrite: false);
            return false;
        }
        catch (Exception e)
        {
            return e is IOException && e is not FileNotFoundException && e is not DirectoryNotFoundException;
        }
    }

    private static bool MoveOntoExistingThrows(string source, string destination)
    {
        try
        {
            File.Move(source, destination);
            return false;
        }
        catch (Exception e)
        {
            return e is IOException && e is not FileNotFoundException && e is not DirectoryNotFoundException;
        }
    }

    private static bool CreateNewOnExistingThrows(string path)
    {
        try
        {
            using FileStream stream = new(path, FileMode.CreateNew, FileAccess.Write);
            return false;
        }
        catch (Exception e)
        {
            return e is IOException && e is not FileNotFoundException && e is not DirectoryNotFoundException;
        }
    }

    private static bool DeleteNonEmptyDirectoryThrows(string path)
    {
        try
        {
            Directory.Delete(path, recursive: false);
            return false;
        }
        catch (Exception e)
        {
            return e is IOException && e is not DirectoryNotFoundException;
        }
    }

    private static bool DeleteMissingFileDoesNotThrow(string path)
    {
        try
        {
            File.Delete(path);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
