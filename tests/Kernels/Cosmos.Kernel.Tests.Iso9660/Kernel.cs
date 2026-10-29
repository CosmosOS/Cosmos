// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;
using System.Collections.Generic;
using System.IO;
using Cosmos.Kernel.System.Diagnostics;
using Cosmos.Kernel.System.FileSystem;
using Cosmos.Kernel.System.FileSystem.Iso9660;
using Cosmos.TestRunner.Framework;
using Sys = Cosmos.Kernel.System;
using TR = Cosmos.TestRunner.Framework.TestRunner;

namespace Cosmos.Kernel.Tests.Iso9660;

/// <summary>
/// The ISO 9660 driver over two images xorriso built from the fixture trees
/// at compile time (see the project file): one with Rock Ridge, read as a
/// CD (2048-byte sectors) and as a disk an image was written to (512-byte
/// sectors), and one with plain level 1 identifiers and a Joliet descriptor
/// the driver skips. The cells reach the driver through <c>System.IO</c>,
/// as a kernel does, and through <see cref="VfsManager"/> where the contract
/// is the VFS's own: the read-only mount, the refusals, StatFs.
/// </summary>
public class Kernel : Sys.Kernel
{
    /// <summary>Exact TR.Run cell count — the harness synthesizes failures
    /// for missing tests, so a mid-suite hang can't report ALL TESTS PASSED.</summary>
    private const ushort ExpectedTestCount = 47;

    private const string CdDriverName = "iso-cd";
    private const string DiskDriverName = "iso-disk";
    private const string Level1DriverName = "iso-level1";
    private const string CdMount = "/cd";
    private const string DiskMount = "/stick";
    private const string Level1Mount = "/plain";

    /// <summary>2026-10-28 12:00:00 UTC, the stamp every fixture entry carries.</summary>
    private const long FixtureEpoch = 1_793_188_800;

    /// <summary>The <see cref="VfsStatFs.Type"/> an ISO 9660 superblock reports.</summary>
    private const ulong Iso9660Magic = 0x9660;

    /// <summary>Entries in the Rock Ridge root: docs, empty, emptydir, hello.txt, Mixed Case.TXT, the 140-character name, link-to-hello, link-to-docs, dangling. rr_moved is hidden.</summary>
    private const int RockRidgeRootEntries = 9;

    /// <summary>Files in /cd/docs: forty numbered entries, numbered-lines.txt, ten-kilobytes.bin, pattern-200k.bin.</summary>
    private const int DocsFileCount = 43;

    private const int TenKilobytes = 10_000;
    private const int PatternBytes = 204_800;

    /// <summary>A chunk that is not a multiple of any sector size, so every chunked read runs the head, body and tail paths.</summary>
    private const int UnalignedChunk = 4_097;

    /// <summary>Position inside the last, partial logical block of the 10 000-byte file.</summary>
    private const int SeekInsideLastBlock = 8_200;

    private const int LongNameLength = 140;

    private const int NumberedLineCount = 2_047;

    /// <summary>The sector the primary volume descriptor sits in; its standard identifier CD001 starts at byte 1.</summary>
    private const ulong PrimaryDescriptorSector = 16;

    protected override void BeforeRun()
    {
        Log.WriteString("[Iso9660Tests] BeforeRun() reached!\n");

        TR.Start("ISO 9660 Driver Tests", expectedTests: ExpectedTestCount);

        MemoryBlockDevice cd = FixtureImages.RockRidge("CDROM", FixtureImages.CdBlockSize);
        MemoryBlockDevice stick = FixtureImages.RockRidge("STICK", FixtureImages.DiskBlockSize);
        MemoryBlockDevice plain = FixtureImages.Level1("PLAIN", FixtureImages.CdBlockSize);
        Iso9660FileSystemType cdDriver = new(cd);
        Iso9660FileSystemType stickDriver = new(stick);
        Iso9660FileSystemType plainDriver = new(plain);

        // ---------- mount ----------

        TR.Run("Test_Mount_RockRidge_CdGeometry", () =>
        {
            Assert.True(VfsManager.RegisterFileSystem(CdDriverName, cdDriver));
            Assert.True(VfsManager.TryMount(CdDriverName, "", MountFlags.None, CdMount, out VfsMount? mount));
            Assert.NotNull(mount);
            Assert.True(Directory.Exists(CdMount), "the mount point should be a directory");
        });

        TR.Run("Test_Mount_RockRidge_DiskGeometry", () =>
        {
            Assert.True(VfsManager.RegisterFileSystem(DiskDriverName, stickDriver));
            Assert.True(VfsManager.TryMount(DiskDriverName, "", MountFlags.None, DiskMount, out _));
            Assert.True(Directory.Exists(DiskMount));
        });

        TR.Run("Test_Mount_Level1_SkipsJoliet", () =>
        {
            Assert.True(VfsManager.RegisterFileSystem(Level1DriverName, plainDriver));
            Assert.True(VfsManager.TryMount(Level1DriverName, "", MountFlags.None, Level1Mount, out _));
            Assert.True(Directory.Exists(Level1Mount));
        });

        TR.Run("Test_Mount_IsReadOnly", () =>
        {
            Assert.True(VfsManager.TryGetMount(CdMount, out VfsMount? mount));
            Assert.True(mount is not null && mount.IsReadOnly, "an ISO 9660 mount must be read-only");
            Assert.True(mount is not null && (mount.Flags & MountFlags.ReadOnly) != 0, "the mount's flags must carry ReadOnly");
            Assert.True(mount is not null && mount.Superblock.IsReadOnly, "the superblock must report read-only");
        });

        TR.Run("Test_Mount_RejectsZeroedDevice", () =>
        {
            MemoryBlockDevice zeroed = new("ZERO", FixtureImages.CdBlockSize, new byte[64 * 2048]);
            Iso9660FileSystemType driver = new(zeroed);
            Assert.False(driver.TryMount("", MountFlags.None, out _), "a device without a descriptor must not mount");
        });

        TR.Run("Test_Mount_RejectsCorruptIdentifier", () =>
        {
            MemoryBlockDevice corrupt = FixtureImages.RockRidge("CORRUPT", FixtureImages.CdBlockSize);
            byte[] sector = new byte[FixtureImages.CdBlockSize];
            corrupt.ReadBlock(PrimaryDescriptorSector, 1, sector);
            sector[1] = (byte)'X';
            corrupt.WriteBlock(PrimaryDescriptorSector, 1, sector);
            Iso9660FileSystemType driver = new(corrupt);
            Assert.False(driver.TryMount("", MountFlags.None, out _), "a descriptor without CD001 must not mount");
        });

        TR.Run("Test_Mount_RejectsDeviceShorterThanVolume", () =>
        {
            MemoryBlockDevice shortDevice = FixtureImages.RockRidge("SHORT", FixtureImages.CdBlockSize);
            shortDevice.Truncate(shortDevice.BlockCount / 2);
            Iso9660FileSystemType driver = new(shortDevice);
            Assert.False(driver.TryMount("", MountFlags.None, out _), "a volume larger than its device must not mount");
        });

        TR.Run("Test_FormatAndDestroy_Refused", () =>
        {
            Assert.False(cdDriver.TryFormat("", null), "an ISO 9660 driver formats nothing");
            Assert.False(cdDriver.TryDestroy(""), "an ISO 9660 driver destroys nothing");
            Assert.False(VfsManager.TryFormat(CdDriverName, "", null));
        });

        TR.Run("Test_StatFs", () =>
        {
            Assert.True(VfsManager.TryStatFs(CdMount, out VfsStatFs stats));
            Assert.Equal(Iso9660Magic, stats.Type);
            Assert.Equal(FixtureImages.CdBlockSize, stats.BlockSize);
            Assert.True(stats.Blocks > 0, "the volume space must be reported");
            Assert.Equal(0UL, stats.FreeBlocks);
            Assert.Equal(0UL, stats.AvailableBlocks);
            Assert.True(VfsManager.TryStatFs(DiskMount, out VfsStatFs diskStats));
            Assert.Equal(stats.Blocks, diskStats.Blocks, "the volume's block count does not depend on the device's sector size");
        });

        // ---------- listing ----------

        TR.Run("Test_RootListing_RockRidgeNames", () =>
        {
            string[] entries = Directory.GetFileSystemEntries(CdMount);
            Assert.Equal(RockRidgeRootEntries, entries.Length);
            Assert.True(ContainsString(entries, "/cd/hello.txt"), "POSIX names come from NM");
            Assert.True(ContainsString(entries, "/cd/Mixed Case.TXT"), "case and spaces survive");
            Assert.True(ContainsString(entries, "/cd/docs"));
            Assert.True(ContainsString(entries, "/cd/emptydir"));
            Assert.True(ContainsString(entries, "/cd/link-to-hello"));
            Assert.False(ContainsString(entries, "/cd/rr_moved"), "the relocation directory is hidden");
            Assert.False(ContainsString(entries, "/cd/HELLO.TXT"), "the ISO identifier is not presented beside the NM name");
        });

        TR.Run("Test_RootListing_Level1Names", () =>
        {
            string[] files = Directory.GetFiles(Level1Mount);
            string[] directories = Directory.GetDirectories(Level1Mount);
            Assert.Equal(2, files.Length);
            Assert.Equal(1, directories.Length);
            Assert.True(ContainsString(files, "/plain/README"), "the dot of an empty extension and the version are dropped");
            Assert.True(ContainsString(files, "/plain/FILE.TXT"));
            Assert.True(ContainsString(directories, "/plain/SUB"));
        });

        TR.Run("Test_Listing_DirectorySpanningBlocks", () =>
        {
            string[] files = Directory.GetFiles("/cd/docs");
            Assert.Equal(DocsFileCount, files.Length);
            Assert.True(ContainsString(files, "/cd/docs/entry-number-01-padded-to-make-it-long.txt"));
            Assert.True(ContainsString(files, "/cd/docs/entry-number-40-padded-to-make-it-long.txt"));
            Assert.Equal(1, Directory.GetDirectories("/cd/docs").Length);
        });

        TR.Run("Test_Listing_Pattern", () =>
        {
            string[] matches = Directory.GetFiles("/cd/docs", "*.bin");
            Assert.Equal(2, matches.Length);
            Assert.True(ContainsString(matches, "/cd/docs/ten-kilobytes.bin"));
            Assert.True(ContainsString(matches, "/cd/docs/pattern-200k.bin"));
        });

        TR.Run("Test_Listing_EmptyDirectory", () =>
        {
            Assert.Equal(0, Directory.GetFileSystemEntries("/cd/emptydir").Length);
        });

        TR.Run("Test_Listing_SameOnBothGeometries", () =>
        {
            string[] cdEntries = Directory.GetFileSystemEntries(CdMount);
            string[] stickEntries = Directory.GetFileSystemEntries(DiskMount);
            Assert.Equal(cdEntries.Length, stickEntries.Length);
            Assert.Equal(Directory.GetFiles("/cd/docs").Length, Directory.GetFiles("/stick/docs").Length);
        });

        // ---------- lookup ----------

        TR.Run("Test_Lookup_RockRidge_IsCaseSensitive", () =>
        {
            Assert.True(File.Exists("/cd/hello.txt"));
            Assert.False(File.Exists("/cd/HELLO.TXT"), "a POSIX name matches exactly");
            Assert.True(File.Exists("/cd/Mixed Case.TXT"));
            Assert.False(File.Exists("/cd/mixed case.txt"));
        });

        TR.Run("Test_Lookup_Level1_IgnoresCase", () =>
        {
            Assert.True(File.Exists("/plain/README"));
            Assert.True(File.Exists("/plain/readme"), "a plain identifier matches without regard to case");
            Assert.True(File.Exists("/plain/file.txt"));
            Assert.True(Directory.Exists("/plain/sub"));
            Assert.Equal("sub\n", File.ReadAllText("/plain/SUB/INNER.TXT"));
        });

        TR.Run("Test_Lookup_LongName_ThroughContinuation", () =>
        {
            string name = new('n', LongNameLength);
            string path = $"/cd/{name}";
            Assert.True(File.Exists(path), "a name whose NM spills into a CE continuation must be found");
            Assert.Equal("long\n", File.ReadAllText(path));
            FileInfo info = new(path);
            Assert.Equal(LongNameLength, info.Name.Length);
        });

        TR.Run("Test_Lookup_DeepPath_AtLimit", () =>
        {
            Assert.Equal("bottom\n", File.ReadAllText("/cd/docs/deep1/deep2/deep3/deep4/deep5/deep6/bottom.txt"));
        });

        TR.Run("Test_Lookup_RelocatedDirectory", () =>
        {
            // deep7 is past the depth xorriso relocates at: its record under
            // deep6 is a CL placeholder pointing into rr_moved.
            Assert.True(Directory.Exists("/cd/docs/deep1/deep2/deep3/deep4/deep5/deep6/deep7"), "a relocated directory resolves through its placeholder");
            Assert.Equal("relocated\n", File.ReadAllText("/cd/docs/deep1/deep2/deep3/deep4/deep5/deep6/deep7/relocated.txt"));
            string[] entries = Directory.GetFileSystemEntries("/cd/docs/deep1/deep2/deep3/deep4/deep5/deep6");
            Assert.Equal(2, entries.Length);
        });

        TR.Run("Test_Lookup_DotSegments", () =>
        {
            Assert.True(string.Equals("/cd/docs/ten-kilobytes.bin", Path.GetFullPath("/cd/docs/deep1/../ten-kilobytes.bin"), StringComparison.Ordinal));
            Assert.Equal(TenKilobytes, File.ReadAllBytes("/cd/docs/deep1/../ten-kilobytes.bin").Length);
            Assert.Equal("hello from the cd\n", File.ReadAllText("/cd/./hello.txt"));
        });

        TR.Run("Test_Lookup_Missing", () =>
        {
            Assert.False(File.Exists("/cd/absent.txt"));
            Assert.False(Directory.Exists("/cd/absent"));
            Assert.True(ReadThrows<FileNotFoundException>("/cd/absent.txt"), "a missing file reads as FileNotFoundException");
            Assert.True(ReadThrows<DirectoryNotFoundException>("/cd/absent/file.txt"), "a missing directory reads as DirectoryNotFoundException");
        });

        // ---------- reading ----------

        TR.Run("Test_Read_SmallFile", () =>
        {
            Assert.Equal("hello from the cd\n", File.ReadAllText("/cd/hello.txt"));
            Assert.Equal("hello from the cd\n", File.ReadAllText("/stick/hello.txt"));
            Assert.Equal("readme\n", File.ReadAllText("/plain/README"));
        });

        TR.Run("Test_Read_EmptyFile", () =>
        {
            Assert.Equal(0, File.ReadAllBytes("/cd/empty").Length);
            Assert.Equal(0L, new FileInfo("/cd/empty").Length);
        });

        TR.Run("Test_Read_PartialLastBlock", () =>
        {
            byte[] expected = MakePattern(TenKilobytes);
            Assert.Equal(expected, File.ReadAllBytes("/cd/docs/ten-kilobytes.bin"));
            Assert.Equal(expected, File.ReadAllBytes("/stick/docs/ten-kilobytes.bin"));
        });

        TR.Run("Test_Read_Pattern200K_Whole", () =>
        {
            byte[] expected = MakePattern(PatternBytes);
            Assert.Equal(expected, File.ReadAllBytes("/cd/docs/pattern-200k.bin"));
            Assert.Equal(expected, File.ReadAllBytes("/stick/docs/pattern-200k.bin"));
        });

        TR.Run("Test_Read_Pattern200K_UnalignedChunks", () =>
        {
            byte[] expected = MakePattern(PatternBytes);
            Assert.True(ReadsBackInChunks("/cd/docs/pattern-200k.bin", expected, UnalignedChunk), "chunked reads on the CD geometry");
            Assert.True(ReadsBackInChunks("/stick/docs/pattern-200k.bin", expected, UnalignedChunk), "chunked reads on the disk geometry");
        });

        TR.Run("Test_Read_WholeBlocksInOneDeviceRead", () =>
        {
            // The body of a read goes to the device as one command: a 200 KiB
            // file on 512-byte sectors is one ReadBlock plus at most a head
            // and a tail, not four hundred.
            int before = stick.ReadCount;
            File.ReadAllBytes("/stick/docs/pattern-200k.bin");
            int reads = stick.ReadCount - before;
            Assert.True(reads <= 4, $"expected a handful of device reads, got {reads}");
        });

        TR.Run("Test_Read_Seek", () =>
        {
            byte[] expected = MakePattern(TenKilobytes);
            using FileStream stream = new("/cd/docs/ten-kilobytes.bin", FileMode.Open, FileAccess.Read);
            Assert.Equal((long)TenKilobytes, stream.Length);
            stream.Seek(SeekInsideLastBlock, SeekOrigin.Begin);
            Assert.Equal(expected[SeekInsideLastBlock], (byte)stream.ReadByte());
            stream.Seek(-1, SeekOrigin.End);
            Assert.Equal(expected[TenKilobytes - 1], (byte)stream.ReadByte());
            Assert.Equal(-1, stream.ReadByte());
            stream.Seek(0, SeekOrigin.Begin);
            Assert.Equal(expected[0], (byte)stream.ReadByte());
        });

        TR.Run("Test_Read_StreamReader_Lines", () =>
        {
            using StreamReader reader = new("/cd/docs/numbered-lines.txt");
            int count = 0;
            string? line = reader.ReadLine();
            while (line is not null)
            {
                count++;
                line = reader.ReadLine();
            }

            Assert.Equal(NumberedLineCount, count);
        });

        TR.Run("Test_Read_NoFlushEver", () =>
        {
            Assert.Equal(0, cd.FlushCount, "a read-only volume never asks the device to flush");
            Assert.True(VfsManager.TryGetMount(CdMount, out VfsMount? mount));
            Assert.True(mount is not null && mount.Superblock.SuperOperations.Sync(mount.Superblock));
            Assert.Equal(0, cd.FlushCount);
        });

        // ---------- attributes ----------

        TR.Run("Test_Attributes_ReadOnly", () =>
        {
            FileInfo info = new("/cd/hello.txt");
            Assert.True(info.IsReadOnly, "a file on a CD is read-only");
            Assert.True((File.GetAttributes("/cd/hello.txt") & FileAttributes.ReadOnly) != 0);
            Assert.True((File.GetAttributes("/cd/docs") & FileAttributes.Directory) != 0);
            Assert.True((File.GetAttributes("/plain/README") & FileAttributes.ReadOnly) != 0, "a plain volume's files are read-only too");
        });

        TR.Run("Test_Attributes_TimeStamps_RockRidge", () =>
        {
            DateTime expected = DateTime.UnixEpoch.AddSeconds(FixtureEpoch);
            Assert.True(File.GetLastWriteTimeUtc("/cd/hello.txt") == expected, "the TF modify stamp is the pinned fixture date");
            Assert.True(File.GetLastAccessTimeUtc("/cd/hello.txt") == expected);
            Assert.True(Directory.GetLastWriteTimeUtc("/cd/docs") == expected);
        });

        TR.Run("Test_Attributes_TimeStamps_Level1", () =>
        {
            DateTime expected = DateTime.UnixEpoch.AddSeconds(FixtureEpoch);
            Assert.True(File.GetLastWriteTimeUtc("/plain/README") == expected, "without Rock Ridge the recording date is the stamp");
        });

        TR.Run("Test_Attributes_Kinds", () =>
        {
            Assert.True(Directory.Exists("/cd/docs"));
            Assert.False(File.Exists("/cd/docs"), "a directory is not a file");
            Assert.True(File.Exists("/cd/hello.txt"));
            Assert.False(Directory.Exists("/cd/hello.txt"), "a file is not a directory");
            Assert.Equal(18L, new FileInfo("/cd/hello.txt").Length);
        });

        TR.Run("Test_Attributes_DistinctInodes", () =>
        {
            // Every empty file and every symbolic link shares one extent on
            // a xorriso image: the inode number must come from the record,
            // not the extent, or File.Move would call them the same file.
            // The entries are read through the directory handle, since a
            // path stat resolves a link to its target.
            Assert.True(VfsManager.TryOpenDirectory(CdMount, out IVfsDirectoryHandle? root));
            IReadOnlyList<IVfsInode> entries = [];
            Assert.True(root is not null && root.TryReadDir(out entries));
            Assert.Equal(RockRidgeRootEntries, entries.Count);
            ulong[] numbers = new ulong[entries.Count];
            for (int i = 0; i < entries.Count; i++)
            {
                Assert.True(entries[i].InodeOperations.GetAttr(entries[i], out VfsStat stat), $"stat of {entries[i].Name}");
                Assert.True(stat.InodeNumber != 0, $"inode number of {entries[i].Name} is reported");
                numbers[i] = stat.InodeNumber;
            }

            Assert.True(AllDistinct(numbers), "every root entry has its own inode number");
            Assert.True(VfsManager.TryStat("/cd/empty", out VfsStat first));
            Assert.True(VfsManager.TryStat("/cd/empty", out VfsStat second) && second.InodeNumber == first.InodeNumber, "inode numbers are stable");
        });

        // ---------- symbolic links ----------

        TR.Run("Test_Symlink_ToFile", () =>
        {
            Assert.True(File.Exists("/cd/link-to-hello"), "a link to a file is a file");
            Assert.Equal("hello from the cd\n", File.ReadAllText("/cd/link-to-hello"));
        });

        TR.Run("Test_Symlink_ToDirectory", () =>
        {
            Assert.True(Directory.Exists("/cd/link-to-docs"), "a link to a directory is a directory");
            string[] entries = Directory.GetDirectories("/cd/link-to-docs");
            Assert.Equal(1, entries.Length);
            Assert.True(ContainsString(entries, "/cd/link-to-docs/deep2"));
        });

        TR.Run("Test_Symlink_Dangling", () =>
        {
            Assert.False(File.Exists("/cd/dangling"), "a dangling link resolves to nothing");
            Assert.True(ReadThrows<FileNotFoundException>("/cd/dangling"));
        });

        TR.Run("Test_Symlink_ListedAsTargetKind", () =>
        {
            string[] files = Directory.GetFiles(CdMount);
            string[] directories = Directory.GetDirectories(CdMount);
            Assert.True(ContainsString(files, "/cd/link-to-hello"), "a link to a file lists among the files");
            Assert.True(ContainsString(directories, "/cd/link-to-docs"), "a link to a directory lists among the directories");
        });

        // ---------- refusals ----------

        TR.Run("Test_ReadOnly_Create_Throws", () =>
        {
            Assert.True(ThrowsReadOnly(() => File.WriteAllText("/cd/new.txt", "x")), "File.WriteAllText on a new name");
            Assert.True(ThrowsReadOnly(() => File.Create("/cd/new2.txt").Dispose()), "File.Create");
            Assert.True(ThrowsReadOnly(() => Directory.CreateDirectory("/cd/newdir")), "Directory.CreateDirectory");
            Assert.False(File.Exists("/cd/new.txt"));
        });

        TR.Run("Test_ReadOnly_Write_Throws", () =>
        {
            Assert.True(ThrowsReadOnly(() => File.WriteAllText("/cd/hello.txt", "x")), "File.WriteAllText on an existing file");
            Assert.True(ThrowsReadOnly(() => File.AppendAllText("/cd/hello.txt", "x")), "File.AppendAllText");
            Assert.True(ThrowsReadOnly(() => new FileStream("/cd/hello.txt", FileMode.Open, FileAccess.Write).Dispose()), "FileStream with write access");
            Assert.True(ThrowsReadOnly(() => new FileStream("/cd/hello.txt", FileMode.Truncate, FileAccess.Write).Dispose()), "FileMode.Truncate");
            Assert.Equal("hello from the cd\n", File.ReadAllText("/cd/hello.txt"));
        });

        TR.Run("Test_ReadOnly_DeleteMoveCopy_Throws", () =>
        {
            Assert.True(ThrowsReadOnly(() => File.Delete("/cd/hello.txt")), "File.Delete");
            Assert.True(ThrowsReadOnly(() => File.Move("/cd/hello.txt", "/cd/moved.txt")), "File.Move");
            Assert.True(ThrowsReadOnly(() => File.Copy("/plain/README", "/cd/copied.txt")), "File.Copy onto the mount");
            Assert.True(ThrowsReadOnly(() => Directory.Delete("/cd/emptydir")), "Directory.Delete");
            Assert.True(ThrowsReadOnly(() => Directory.Move("/cd/emptydir", "/cd/renamed")), "Directory.Move");
            Assert.True(File.Exists("/cd/hello.txt"));
            Assert.True(Directory.Exists("/cd/emptydir"));
        });

        TR.Run("Test_ReadOnly_SetAttributes_Throws", () =>
        {
            Assert.True(ThrowsReadOnly(() => File.SetAttributes("/cd/hello.txt", FileAttributes.Normal)), "File.SetAttributes");
            Assert.True(new FileInfo("/cd/hello.txt").IsReadOnly);
        });

        TR.Run("Test_ReadOnly_VfsManager_Refuses", () =>
        {
            Assert.False(VfsManager.TryCreateFile("/cd/new.txt", VfsMode.OwnerRead), "TryCreateFile");
            Assert.False(VfsManager.TryCreateDirectory("/cd/newdir", VfsMode.OwnerRead), "TryCreateDirectory");
            Assert.False(VfsManager.TryUnlink("/cd/hello.txt"), "TryUnlink");
            Assert.False(VfsManager.TryRemoveDirectory("/cd/emptydir"), "TryRemoveDirectory");
            Assert.False(VfsManager.TryRename("/cd/hello.txt", "/cd/moved.txt"), "TryRename");
            Assert.True(File.Exists("/cd/hello.txt"));
        });

        // ---------- unmount ----------

        TR.Run("Test_Unmount_And_Remount", () =>
        {
            Assert.True(VfsManager.TryUnmount(Level1Mount));
            Assert.False(Directory.Exists("/plain/SUB"), "an unmounted volume's paths are gone");
            Assert.True(ReadThrows<DirectoryNotFoundException>("/plain/README"));
            Assert.True(VfsManager.TryMount(Level1DriverName, "", MountFlags.None, Level1Mount, out _));
            Assert.Equal("readme\n", File.ReadAllText("/plain/README"));
        });

        TR.Run("Test_TwoMounts_SameDevice", () =>
        {
            Assert.True(VfsManager.TryMount(CdDriverName, "", MountFlags.None, "/cd2", out _));
            Assert.Equal("hello from the cd\n", File.ReadAllText("/cd2/hello.txt"));
            Assert.Equal("hello from the cd\n", File.ReadAllText("/cd/hello.txt"));
            Assert.True(VfsManager.TryUnmount("/cd2"));
            Assert.True(File.Exists("/cd/hello.txt"), "the first mount survives the second's unmount");
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

    /// <summary>The byte pattern the fixture files carry: (i * 7 + 3) &amp; 0xFF.</summary>
    private static byte[] MakePattern(int length)
    {
        byte[] data = new byte[length];
        for (int i = 0; i < length; i++)
        {
            data[i] = (byte)(((i * 7) + 3) & 0xFF);
        }

        return data;
    }

    private static bool AllDistinct(ulong[] values)
    {
        for (int i = 0; i < values.Length; i++)
        {
            for (int j = i + 1; j < values.Length; j++)
            {
                if (values[i] == values[j])
                {
                    return false;
                }
            }
        }

        return true;
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

    private static bool ReadsBackInChunks(string path, byte[] expected, int chunk)
    {
        byte[] buffer = new byte[chunk];
        int position = 0;
        using FileStream stream = new(path, FileMode.Open, FileAccess.Read);
        int read = stream.Read(buffer, 0, chunk);
        while (read > 0)
        {
            for (int i = 0; i < read; i++)
            {
                if (position + i >= expected.Length || buffer[i] != expected[position + i])
                {
                    return false;
                }
            }

            position += read;
            read = stream.Read(buffer, 0, chunk);
        }

        return position == expected.Length;
    }

    // Expected-exception probes live in their own methods with a single
    // try/catch each (multiple EH regions per method have misbehaved on
    // arm64; see the Storage suite).

    private static bool ReadThrows<TException>(string path) where TException : Exception
    {
        try
        {
            File.ReadAllText(path);
            return false;
        }
        catch (Exception e)
        {
            return e is TException;
        }
    }

    /// <summary>True when <paramref name="action"/> throws the plain IOException
    /// the BCL raises for EROFS ("Read-only file system"), and not one of its
    /// not-found subclasses nor UnauthorizedAccessException.</summary>
    private static bool ThrowsReadOnly(Action action)
    {
        try
        {
            action();
            return false;
        }
        catch (Exception e)
        {
            return e is IOException && e is not FileNotFoundException && e is not DirectoryNotFoundException;
        }
    }
}
