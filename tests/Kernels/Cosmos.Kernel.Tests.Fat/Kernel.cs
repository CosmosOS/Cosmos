using System;
using Cosmos.Kernel.System.Diagnostics;
using Cosmos.Kernel.HAL.Vfs;
using Cosmos.Kernel.System.Filesystems.Fat;
using Cosmos.Kernel.System.Vfs;
using Cosmos.TestRunner.Framework;
using Sys = Cosmos.Kernel.System;
using TR = Cosmos.TestRunner.Framework.TestRunner;

namespace Cosmos.Kernel.Tests.Fat;

public class Kernel : Sys.Kernel
{
    /// <summary>Exact TR.Run cell count — the harness synthesizes failures
    /// for missing tests, so a mid-suite hang can't report ALL TESTS PASSED.</summary>
    private const ushort ExpectedTestCount = 42;

    private const string Fat16Mount = "/fat";
    private const string Fat32Mount = "/fat32";

    /// <summary><see cref="FatTestVolume.BlockSize"/> as an int, for sector buffers, byte-offset math and BlockSize asserts (bytes).</summary>
    private const int SectorSizeBytes = (int)FatTestVolume.BlockSize;

    /// <summary>Sectors in the ~3 MiB disk the explicit-FAT12 format cell uses — ~6000 data sectors, well inside the FAT12 band.</summary>
    private const ulong Fat12DiskBlockCount = 3UL * 1024 * 1024 / FatTestVolume.BlockSize;

    /// <summary>Sectors in the 4 MiB disk whose auto-detected format is the FAT12 baseline of the FAT-size cell.</summary>
    private const ulong Fat12AutoDetectBlockCount = 4UL * 1024 * 1024 / FatTestVolume.BlockSize;

    /// <summary>Sectors in the 8 MiB scratch disks the BPB-corruption, ClusterToLba-bounds and FAT-bounds cells auto-format (SPC auto-picks 8, landing in the FAT12 band).</summary>
    private const uint HardeningDiskBlockCount = 8u * 1024 * 1024 / SectorSizeBytes;

    /// <summary>Sectors in the 16 MiB scratch volumes the FAT16-specific cells format — inside the FAT16 band and under the 65536-sector TotSec16 threshold.</summary>
    private const uint Fat16ScratchBlockCount = 16u * 1024 * 1024 / SectorSizeBytes;

    /// <summary>Sectors in the 4 MiB device of the out-of-range I/O cell — far below the 2^32-wrapping block number.</summary>
    private const ulong RangeCheckDiskBlockCount = 8192;

    /// <summary>Block number whose byte offset (× 512-byte blocks) is exactly 2^32, so a truncated 32-bit cast would alias sector 0.</summary>
    private const ulong Uint32WrapBlockNumber = 8_388_608;

    /// <summary>Blocks in the read that starts on the device's last sector and must straddle the end.</summary>
    private const ulong StraddlingReadBlocks = 2;

    /// <summary>Sector size below FAT's 512-byte minimum, which the formatter must refuse (bytes).</summary>
    private const int UndersizedSectorBytes = 256;

    /// <summary>Block count paired with the undersized sector — a plausible 4 MiB device.</summary>
    private const ulong UndersizedSectorBlockCount = 16384;

    /// <summary>64 KiB sector size past FAT's 4096-byte maximum — it would truncate the 16-bit BPB_BytsPerSec field (bytes).</summary>
    private const int OversizedSectorBytes = 65536;

    /// <summary>Block count paired with the oversized sector — an 8 MiB device.</summary>
    private const ulong OversizedSectorBlockCount = 128;

    /// <summary>LBA of the BPB/boot sector — also the sector a 2^32-wrapped byte offset would alias.</summary>
    private const ulong BootSectorLba = 0;

    /// <summary>LBA of the FAT32 FSInfo sector on the volumes formatted here (BPB_FSInfo, fatgen103).</summary>
    private const ulong FsInfoSectorLba = 1;

    /// <summary>LBA of the FAT32 backup boot sector on the volumes formatted here (BPB_BkBootSec, fatgen103).</summary>
    private const ulong BackupBootSectorLba = 6;

    /// <summary>Byte offset of BPB_BytsPerSec in the boot sector (fatgen103).</summary>
    private const int BpbBytsPerSecOffset = 11;

    /// <summary>Byte offset of BPB_SecPerClus in the boot sector (fatgen103).</summary>
    private const int BpbSecPerClusOffset = 13;

    /// <summary>Byte offset of BPB_RsvdSecCnt in the boot sector (fatgen103).</summary>
    private const int BpbRsvdSecCntOffset = 14;

    /// <summary>Byte offset of BPB_NumFATs in the boot sector (fatgen103).</summary>
    private const int BpbNumFatsOffset = 16;

    /// <summary>Byte offset of BPB_RootEntCnt in the boot sector (fatgen103).</summary>
    private const int BpbRootEntCntOffset = 17;

    /// <summary>Byte offset of BPB_TotSec16 in the boot sector (fatgen103).</summary>
    private const int BpbTotSec16Offset = 19;

    /// <summary>Byte offset of BPB_FATSz16 in the boot sector (fatgen103).</summary>
    private const int BpbFatSz16Offset = 22;

    /// <summary>Byte offset of BPB_TotSec32 in the boot sector (fatgen103).</summary>
    private const int BpbTotSec32Offset = 32;

    /// <summary>Byte offset of BPB_FATSz32 in the FAT32 boot sector (fatgen103).</summary>
    private const int BpbFatSz32Offset = 36;

    /// <summary>Byte offset of BPB_RootClus in the FAT32 boot sector (fatgen103).</summary>
    private const int BpbRootClusOffset = 44;

    /// <summary>Size of a 16-bit BPB field (bytes).</summary>
    private const int BpbWordBytes = 2;

    /// <summary>Size of a 32-bit BPB field (bytes).</summary>
    private const int BpbDwordBytes = 4;

    /// <summary>A zeroed 16-bit BPB size/count field tells parsers to use its 32-bit sibling instead (fatgen103).</summary>
    private const ushort BpbNarrowFieldUnused = 0;

    /// <summary>BPB_TotSec32 when the sector count fits BPB_TotSec16: the wide field must hold 0 (fatgen103).</summary>
    private const uint BpbWideFieldUnused = 0;

    /// <summary>Cluster count at which a volume stops being FAT12 (fatgen103 §3.5).</summary>
    private const uint Fat16MinClusterCount = 4085;

    /// <summary>Cluster count at which a volume becomes FAT32 (fatgen103 §3.5).</summary>
    private const uint Fat32MinClusterCount = 65525;

    /// <summary>Reserved FAT[0]/FAT[1] entries every FAT copy carries ahead of the data clusters (fatgen103 §6.2).</summary>
    private const uint ReservedFatEntries = 2;

    /// <summary>Bytes per FAT16 entry (fatgen103 §4).</summary>
    private const uint Fat16EntrySize = 2;

    /// <summary>Bytes per FAT32 entry (fatgen103 §4).</summary>
    private const uint Fat32EntrySize = 4;

    /// <summary>FAT12 packs this many entries into every <see cref="Fat12PairBytes"/> bytes — 1.5 bytes per entry (fatgen103 §4).</summary>
    private const uint Fat12EntriesPerPair = 2;

    /// <summary>Bytes holding one FAT12 entry pair (fatgen103 §4).</summary>
    private const uint Fat12PairBytes = 3;

    /// <summary>Length of the raw 11-byte 8.3 name field in a directory entry (fatgen103 §6).</summary>
    private const int ShortNameLength = 11;

    /// <summary>Stem length that makes stem + ".txt" exactly the 255-char LFN cap.</summary>
    private const int MaxLfnStemLength = 251;

    /// <summary>Name length past the 255-char LFN cap, which create/rename must refuse.</summary>
    private const int OverlongNameLength = 300;

    /// <summary>Files that pack the fresh directory's single 512-byte cluster: 16 dirent slots minus '.' and '..'.</summary>
    private const int PackFillEntries = 14;

    /// <summary>Hex digits in the generated Fxxxx.TXT names — fixed width keeps them 8.3-valid and unique.</summary>
    private const int HexNameDigits = 4;

    /// <summary>Distinct files the root-growth cell creates; at 16 dirents per 512-byte cluster (SPC=1) this pushes the FAT32 root across several cluster boundaries.</summary>
    private const int RootGrowthFileCount = 64;

    /// <summary>First-cluster value fatgen103 mandates in '..' when the parent directory is the FAT32 root.</summary>
    private const uint RootDotDotClusterValue = 0;

    /// <summary>SPC=1: the smallest cluster (one 512-byte sector) — keeps the 33 MiB FAT32 volume above 65525 clusters and makes data span clusters quickly.</summary>
    private const byte OneSectorPerCluster = 1;

    /// <summary>SPC of the explicit FAT12 volume (1 KiB clusters keep its cluster count in the FAT12 band).</summary>
    private const byte Fat12SectorsPerCluster = 2;

    /// <summary>SPC the 16 MiB FAT16 scratch volumes format with (2 KiB clusters).</summary>
    private const byte Fat16ScratchSectorsPerCluster = 4;

    /// <summary>Reserved sectors ahead of the first FAT on the fresh FAT32 volume (fatgen103's customary 32 for FAT32).</summary>
    private const ushort Fat32ReservedSectors = 32;

    /// <summary>Reserved sectors ahead of the first FAT on the FAT12 volume — just the boot sector, customary for FAT12/16.</summary>
    private const ushort Fat12ReservedSectors = 1;

    /// <summary>Fixed root-directory entry count of the FAT12 volume (the classic 512-entry root).</summary>
    private const ushort Fat12RootEntryCount = 512;

    /// <summary>FAT copy size, in sectors, requested for the fresh 33 MiB FAT32 volume.</summary>
    private const uint Fat32FreshFatSectors = 512;

    /// <summary>FAT copy size, in sectors, requested for the ~3 MiB FAT12 volume — comfortably covers its clusters at 1.5 bytes each.</summary>
    private const uint Fat12FatSectors = 12;

    /// <summary>FAT32 root cluster below <see cref="FatTable.FirstDataCluster"/>, which format must refuse.</summary>
    private const uint RootClusterBelowDataArea = FatTable.FirstDataCluster - 1;

    /// <summary>Cluster number 0 — below <see cref="FatTable.FirstDataCluster"/> (2); ClusterToLba's cluster−2 underflows on it, so mount/format validation must reject it (fatgen103).</summary>
    private const uint ReservedClusterZero = 0;

    /// <summary>FAT32 root cluster far past the 33 MiB volume's ~66k clusters, which format must refuse.</summary>
    private const uint RootClusterPastClusterCount = 1_000_000;

    /// <summary>Non-power-of-two BPB_SecPerClus that mount-time validation must reject.</summary>
    private const byte NonPowerOfTwoSpc = 3;

    /// <summary>Corrupt BPB_FATSz32: with two FATs the uint fat-region product wraps to 0 (2 × 0x80000000 == 2^32).</summary>
    private const uint WrappingFatSectorCount = 0x80000000u;

    /// <summary>BPB_FATSz32 zeroed to forge a zero-length FAT, which parse/mount validation must reject (fatgen103 §3).</summary>
    private const uint ZeroedFatSize = 0;

    /// <summary>Factor by which the corrupt BPB_TotSec32 overclaims the backing device's sector count.</summary>
    private const uint PastDeviceSectorMultiplier = 4;

    /// <summary>Clusters in the chain the FAT-bounds cell allocates before corrupting its middle link.</summary>
    private const int ProbeChainClusters = 3;

    /// <summary>Length GetChain returns once the probe chain's middle link (index 1) is corrupted — the first two clusters, then the out-of-range entry halts the walk.</summary>
    private const int TruncatedChainLength = 2;

    /// <summary>Clusters past the volume's count for the corrupt FAT link — still decodable, landing inside the second FAT copy.</summary>
    private const uint WildLinkOvershoot = 500;

    /// <summary>Clusters past the volume's count where the out-of-range ClusterToLba probe lands.</summary>
    private const uint ClusterCountOvershoot = 5;

    /// <summary>Wild FAT32 cluster in the huge gap between real cluster counts and the EOC band.</summary>
    private const uint Fat32WildCluster = 0x00FF0000u;

    /// <summary>SetAttr size one past FAT's 4 GiB − 1 file-size cap; a 32-bit cast would wrap it to 0.</summary>
    private const ulong SizeBeyondFatCap = 0x1_0000_0000UL;

    /// <summary>Nonzero FstClusHI stamp playing the OS/2 EA handle real FAT12/16 volumes may carry in that reserved word.</summary>
    private const ushort EaHandleStamp = 0x1234;

    /// <summary>First cluster stamped into the planted ghost entry — plausibly inside the data area.</summary>
    private const ushort GhostFirstCluster = 3;

    /// <summary>On-disk file size stamped into the planted ghost entry so it parses as plausible (bytes).</summary>
    private const uint GhostFileSizeBytes = 512;

    /// <summary>Payload bytes of the FAT16 inode round-trip file.</summary>
    private const int RoundTripPayloadBytes = 1024;

    /// <summary>Pattern salt of the FAT16 inode round-trip payload.</summary>
    private const byte RoundTripSalt = 0xA5;

    /// <summary>Payload bytes of the nested-directory inner file.</summary>
    private const int NestedInnerPayloadBytes = 64;

    /// <summary>Pattern salt of the nested-directory inner payload.</summary>
    private const byte NestedInnerSalt = 0x10;

    /// <summary>Payload bytes of the FAT16 large write — 32 KiB spans eight 4 KiB clusters at SPC=8.</summary>
    private const int CrossClusterPayloadBytes = 32 * 1024;

    /// <summary>Pattern salt of the cluster-crossing payload.</summary>
    private const byte CrossClusterSalt = 0x07;

    /// <summary>Payload bytes of the FAT32 VFS-handle round-trip file.</summary>
    private const int VfsRoundTripPayloadBytes = 2048;

    /// <summary>Pattern salt of the FAT32 VFS-handle round-trip payload.</summary>
    private const byte VfsRoundTripSalt = 0x33;

    /// <summary>Payload bytes of the seek-test file.</summary>
    private const int SeekPayloadBytes = 4096;

    /// <summary>Pattern salt of the seek-test payload.</summary>
    private const byte SeekSalt = 0x42;

    /// <summary>Bytes read back from SeekWhence.End — the payload tail the seek must land on.</summary>
    private const int SeekTailBytes = 128;

    /// <summary>Offset of the SeekWhence.Cur hop from position 0.</summary>
    private const int SeekCurOffset = 1024;

    /// <summary>Bytes of the mid-file window read after the Set + Cur seeks.</summary>
    private const int SeekMiddleBytes = 16;

    /// <summary>Payload bytes of the read-at-EOF file.</summary>
    private const int EofPayloadBytes = 64;

    /// <summary>Pattern salt of the read-at-EOF payload.</summary>
    private const byte EofSalt = 0x77;

    /// <summary>Buffer bytes offered to the read at EOF, which must return 0.</summary>
    private const int EofProbeBytes = 8;

    /// <summary>Payload bytes of the FAT32 256 KB many-cluster file — 512 clusters at SPC=1 (512 B/cluster).</summary>
    private const int Big32PayloadBytes = 256 * 1024;

    /// <summary>Pattern salt of the 256 KB many-cluster payload.</summary>
    private const byte Big32Salt = 0xC9;

    /// <summary>Payload bytes of the FAT32 long-file-name file.</summary>
    private const int LfnPayloadBytes = 256;

    /// <summary>Pattern salt of the long-file-name payload.</summary>
    private const byte LfnSalt = 0x5A;

    /// <summary>Payload bytes of the A/B/C nested-directories leaf file.</summary>
    private const int NestedDirsPayloadBytes = 128;

    /// <summary>Pattern salt of the nested-directories leaf payload.</summary>
    private const byte NestedDirsSalt = 0xEE;

    /// <summary>Bytes SetAttr grows the empty file to; the FS must allocate and zero-fill them.</summary>
    private const int GrowTargetBytes = 4096;

    /// <summary>Payload bytes of the cluster-hogging file whose unlink must free them.</summary>
    private const int HogPayloadBytes = 64 * 1024;

    /// <summary>Pattern salt of the cluster-hog payload.</summary>
    private const byte HogSalt = 0xDE;

    /// <summary>Allowed Bfree drift (clusters) across the hog's create + unlink — tolerance for root-directory growth.</summary>
    private const ulong BfreeToleranceClusters = 8;

    /// <summary>Payload bytes of the file that must survive unmount + remount.</summary>
    private const int SurvivePayloadBytes = 800;

    /// <summary>Pattern salt of the persistence payload.</summary>
    private const byte SurviveSalt = 0x9F;

    /// <summary>Payload bytes written through the freshly formatted FAT32 — 8 KiB = 16 clusters at SPC=1.</summary>
    private const int FreshFormatPayloadBytes = 8 * 1024;

    /// <summary>Pattern salt of the fresh-format payload.</summary>
    private const byte FreshFormatSalt = 0x6C;

    /// <summary>Payload bytes written through the freshly formatted FAT12.</summary>
    private const int Fat12PayloadBytes = 2048;

    /// <summary>Pattern salt of the FAT12 payload.</summary>
    private const byte Fat12Salt = 0x12;

    /// <summary>Canary stamped into sector 0 so a silent 2^32 alias would be detectable.</summary>
    private const byte AliasCanaryFill = 0xA5;

    /// <summary>Payload bytes of the first stale-handle file (A), whose slot is freed and reused.</summary>
    private const int StaleAPayloadBytes = 1024;

    /// <summary>Pattern salt of stale file A.</summary>
    private const byte StaleASalt = 0x51;

    /// <summary>Payload bytes of the reusing file (B) — smaller than A so a clobbered size would show.</summary>
    private const int StaleBPayloadBytes = 700;

    /// <summary>Pattern salt of reusing file B.</summary>
    private const byte StaleBSalt = 0x52;

    /// <summary>Payload bytes of each tail-collision file.</summary>
    private const int TailCollisionPayloadBytes = 64;

    /// <summary>Pattern salt of tail-collision file A.</summary>
    private const byte TailCollisionASalt = 0x0A;

    /// <summary>Pattern salt of tail-collision file B — distinct from A so the ~2 lookup proves which file answered.</summary>
    private const byte TailCollisionBSalt = 0x0B;

    /// <summary>Payload bytes of the file whose FstClusHI gets the EA-handle stamp.</summary>
    private const int EaFilePayloadBytes = 1024;

    /// <summary>Pattern salt of the EA-handle file's payload.</summary>
    private const byte EaFileSalt = 0x33;

    /// <summary>Payload bytes of the file whose 4 GiB SetAttr must be refused without truncation.</summary>
    private const int SizeCapPayloadBytes = 512;

    /// <summary>Pattern salt of the size-cap payload.</summary>
    private const byte SizeCapSalt = 0x44;

    /// <summary>Size a directory SetAttr tries to grow to; size changes on directories must be refused (bytes).</summary>
    private const int DirGrowAttemptBytes = 4096;

    /// <summary>Bytes of the chunks written before and after the hole.</summary>
    private const int HoleChunkBytes = 16;

    /// <summary>Pattern salt of the pre-hole head chunk.</summary>
    private const byte HoleHeadSalt = 0x77;

    /// <summary>Pattern salt of the past-EOF tail chunk.</summary>
    private const byte HoleTailSalt = 0x78;

    /// <summary>Past-EOF write position — several 2 KiB clusters beyond the 16-byte head.</summary>
    private const int HoleFarOffset = 9000;

    /// <summary>Offset inside the zero gap where the read-back probes.</summary>
    private const int HoleGapProbeOffset = 4096;

    /// <summary>Bytes probed inside the gap, all of which must read as zero.</summary>
    private const int HoleGapProbeBytes = 64;

    /// <summary>Shift bringing the next-higher byte of the index into the payload pattern (bits per byte).</summary>
    private const int BitsPerByte = 8;

    /// <summary>Ten decimal digit glyphs: the digit/letter split of hex rendering and the base of the two-digit packed-directory names.</summary>
    private const int DecimalBase = 10;

    /// <summary>Mask isolating one hex digit (low nibble).</summary>
    private const int NibbleMask = 0xF;

    /// <summary>Bits per hex digit (nibble).</summary>
    private const int BitsPerNibble = 4;

    protected override void BeforeRun()
    {
        Log.WriteString("[FatTests] BeforeRun() reached!\n");

        TR.Start("FAT Driver Tests", expectedTests: ExpectedTestCount);

        // Two independent disks + drivers, mounted at distinct points so the
        // FAT16 and FAT32 suites can't perturb one another.
        MemoryBlockDevice fat16Disk = FatTestVolume.CreateFat16("MEMFAT16");
        FatFilesystemType fat16Driver = new(fat16Disk);

        MemoryBlockDevice fat32Disk = FatTestVolume.CreateFat32("MEMFAT32");
        FatFilesystemType fat32Driver = new(fat32Disk);

        // Every cell that needs its own disk recycles this one buffer via
        // Reconfigure. Per-cell devices looked collectable but are not:
        // the kernel GC's first collection under pressure is what hangs,
        // so the run must fit the heap without one — ~420 MiB of per-cell
        // arrays blew the ARM64 CI heap (~286 MiB) at cell 31 while x64
        // (~504 MiB) finished with ~5 MiB to spare.
        MemoryBlockDevice scratchDisk = new("SCRATCH", FatTestVolume.BlockSize, FatTestVolume.Fat32BlockCount);

        TR.Run("Test_RegisterFilesystem_Fat16", () =>
        {
            Assert.True(VfsManager.RegisterFilesystem("fat16-test", fat16Driver));
        });

        TR.Run("Test_RegisterFilesystem_Fat32", () =>
        {
            Assert.True(VfsManager.RegisterFilesystem("fat32-test", fat32Driver));
        });

        TR.Run("Test_Mount_FAT16", () =>
        {
            Assert.True(VfsManager.TryMount("fat16-test", "", MountFlags.None, Fat16Mount, out VfsManager.VfsMount? mount));
            Assert.NotNull(mount);
            Assert.Equal<long>(SectorSizeBytes, mount!.Superblock.BlockSize);
        });

        TR.Run("Test_Mount_FAT32", () =>
        {
            Assert.True(VfsManager.TryMount("fat32-test", "", MountFlags.None, Fat32Mount, out VfsManager.VfsMount? mount));
            Assert.NotNull(mount);
            Assert.Equal<long>(SectorSizeBytes, mount!.Superblock.BlockSize);
            Assert.True(mount.Superblock.MaxNameLength >= FatDirectory.MaxLfnNameLength);

            // Cluster count >= 65525 is what makes the volume FAT32 (lower
            // counts trigger the FAT16/FAT12 paths); the StatFs report below
            // pulls that number directly from the parsed BPB.
            Assert.True(mount.Superblock.SuperOperations.StatFs(mount.Superblock, out VfsStatFs stat));
            Assert.True(stat.Blocks >= Fat32MinClusterCount);
        });

        TR.Run("Test_StatFs_FAT16", () =>
        {
            Assert.True(VfsManager.TryGetMount(Fat16Mount, out VfsManager.VfsMount? mount));
            Assert.True(mount!.Superblock.SuperOperations.StatFs(mount.Superblock, out VfsStatFs stat));
            Assert.True(stat.Blocks > 0);
            Assert.True(stat.Bfree > 0);
            Assert.True(stat.NameMax >= FatDirectory.MaxLfnNameLength);
        });

        TR.Run("Test_StatFs_FAT32", () =>
        {
            Assert.True(VfsManager.TryGetMount(Fat32Mount, out VfsManager.VfsMount? mount));
            Assert.True(mount!.Superblock.SuperOperations.StatFs(mount.Superblock, out VfsStatFs stat));
            Assert.True(stat.Blocks >= Fat32MinClusterCount);
            Assert.True(stat.Bfree > 0);
            Assert.True(stat.NameMax >= FatDirectory.MaxLfnNameLength);
        });

        // ---------- FAT16 inode-level coverage (kept from prior pass) ----------

        TR.Run("Test_Fat16_Create_Write_Read_RoundTrip", () =>
        {
            IVfsInode root = ResolveRoot(Fat16Mount);
            Assert.True(root.InodeOperations.Create(root, "HELLO.TXT", VfsMode.RegularFile, out IVfsInode? created));
            Assert.NotNull(created);

            byte[] payload = MakePayload(RoundTripPayloadBytes, RoundTripSalt);
            WriteAll(created!, payload);

            Assert.True(root.InodeOperations.Lookup(root, "HELLO.TXT", out IVfsInode? lookup));
            byte[] readBack = ReadAll(lookup!, payload.Length);
            AssertBytesEqual(payload, readBack);
        });

        TR.Run("Test_Fat16_LongFileName_LookupBothNames", () =>
        {
            IVfsInode root = ResolveRoot(Fat16Mount);
            const string longName = "longfilename.txt";
            Assert.True(root.InodeOperations.Create(root, longName, VfsMode.RegularFile, out _));
            Assert.True(root.InodeOperations.Lookup(root, longName, out IVfsInode? byLong));
            Assert.NotNull(byLong);
            Assert.True(root.InodeOperations.Lookup(root, "LONGFI~1.TXT", out IVfsInode? byShort));
            Assert.NotNull(byShort);
        });

        TR.Run("Test_Fat16_Mkdir_Nested_Lookup", () =>
        {
            IVfsInode root = ResolveRoot(Fat16Mount);
            Assert.True(root.InodeOperations.Mkdir(root, "DIR", VfsMode.Directory, out IVfsInode? dir));
            Assert.True(dir!.InodeOperations.Create(dir, "INNER.TXT", VfsMode.RegularFile, out IVfsInode? inner));

            byte[] data = MakePayload(NestedInnerPayloadBytes, NestedInnerSalt);
            WriteAll(inner!, data);

            Assert.True(root.InodeOperations.Lookup(root, "DIR", out IVfsInode? lookupDir));
            Assert.True(lookupDir!.InodeOperations.Lookup(lookupDir, "INNER.TXT", out IVfsInode? lookupInner));
            byte[] readBack = ReadAll(lookupInner!, data.Length);
            AssertBytesEqual(data, readBack);
        });

        TR.Run("Test_Fat16_Unlink_RemovesFile", () =>
        {
            IVfsInode root = ResolveRoot(Fat16Mount);
            Assert.True(root.InodeOperations.Create(root, "GONE.TXT", VfsMode.RegularFile, out _));
            Assert.True(root.InodeOperations.Unlink(root, "GONE.TXT"));
            Assert.False(root.InodeOperations.Lookup(root, "GONE.TXT", out _));
        });

        TR.Run("Test_Fat16_Rmdir_RemovesEmptyDir", () =>
        {
            IVfsInode root = ResolveRoot(Fat16Mount);
            Assert.True(root.InodeOperations.Mkdir(root, "EMPTY", VfsMode.Directory, out _));
            Assert.True(root.InodeOperations.Rmdir(root, "EMPTY"));
            Assert.False(root.InodeOperations.Lookup(root, "EMPTY", out _));
        });

        TR.Run("Test_Fat16_LargeWrite_CrossesClusters", () =>
        {
            IVfsInode root = ResolveRoot(Fat16Mount);
            Assert.True(root.InodeOperations.Create(root, "BIG.BIN", VfsMode.RegularFile, out IVfsInode? created));
            byte[] payload = MakePayload(CrossClusterPayloadBytes, CrossClusterSalt);
            WriteAll(created!, payload);
            byte[] readBack = ReadAll(created!, payload.Length);
            AssertBytesEqual(payload, readBack);
        });

        // ---------- FAT32 + VFS-handle coverage ----------

        TR.Run("Test_Fat32_OpenDirectory_Root", () =>
        {
            Assert.True(VfsManager.TryOpenDirectory(Fat32Mount, out IVfsDirectoryHandle? root));
            Assert.NotNull(root);
            Assert.True(root!.TryStat(out VfsStat stat));
            Assert.True(stat.IsDirectory);
        });

        TR.Run("Test_Fat32_OpenDirectory_RejectsRegularFile", () =>
        {
            Assert.True(VfsManager.TryOpenDirectory(Fat32Mount, out IVfsDirectoryHandle? root));
            Assert.True(root!.TryCreateFile("NOTDIR.TXT", VfsMode.RegularFile, out _));
            Assert.False(VfsManager.TryOpenDirectory(Fat32Mount + "/NOTDIR.TXT", out IVfsDirectoryHandle? bad));
            Assert.Null(bad);
        });

        TR.Run("Test_Fat32_Vfs_CreateFile_OpenByPath_RoundTrip", () =>
        {
            Assert.True(VfsManager.TryOpenDirectory(Fat32Mount, out IVfsDirectoryHandle? root));
            Assert.True(root!.TryCreateFile("HELLO32.TXT", VfsMode.RegularFile, out _));

            using IVfsFileHandle? file = OpenFile(Fat32Mount + "/HELLO32.TXT");
            Assert.NotNull(file);

            byte[] payload = MakePayload(VfsRoundTripPayloadBytes, VfsRoundTripSalt);
            long written = file!.Write(payload);
            Assert.Equal<long>(payload.Length, written);
            Assert.Equal<long>(payload.Length, file.Position);
            Assert.True(file.TryFlush());

            using IVfsFileHandle? reader = OpenFile(Fat32Mount + "/HELLO32.TXT");
            Assert.NotNull(reader);
            byte[] readBack = new byte[payload.Length];
            long readBytes = reader!.Read(readBack);
            Assert.Equal<long>(payload.Length, readBytes);
            AssertBytesEqual(payload, readBack);
        });

        TR.Run("Test_Fat32_Vfs_Seek_Set_Cur_End", () =>
        {
            Assert.True(VfsManager.TryOpenDirectory(Fat32Mount, out IVfsDirectoryHandle? root));
            Assert.True(root!.TryCreateFile("SEEK.BIN", VfsMode.RegularFile, out _));

            byte[] payload = MakePayload(SeekPayloadBytes, SeekSalt);
            using (IVfsFileHandle? w = OpenFile(Fat32Mount + "/SEEK.BIN"))
            {
                Assert.NotNull(w);
                w!.Write(payload);
            }

            using IVfsFileHandle? r = OpenFile(Fat32Mount + "/SEEK.BIN");
            Assert.NotNull(r);

            // Seek End-128 and read 128 bytes; must match payload tail.
            Assert.True(r!.TrySeek(-SeekTailBytes, SeekWhence.End));
            byte[] tail = new byte[SeekTailBytes];
            long got = r.Read(tail);
            Assert.Equal<long>(SeekTailBytes, got);
            for (int i = 0; i < SeekTailBytes; i++)
            {
                Assert.Equal<byte>(payload[payload.Length - SeekTailBytes + i], tail[i]);
            }

            // Seek Set + Cur navigates correctly.
            Assert.True(r.TrySeek(0, SeekWhence.Set));
            Assert.Equal<long>(0, r.Position);
            Assert.True(r.TrySeek(SeekCurOffset, SeekWhence.Cur));
            Assert.Equal<long>(SeekCurOffset, r.Position);
            byte[] middle = new byte[SeekMiddleBytes];
            r.Read(middle);
            for (int i = 0; i < SeekMiddleBytes; i++)
            {
                Assert.Equal<byte>(payload[SeekCurOffset + i], middle[i]);
            }
        });

        TR.Run("Test_Fat32_Vfs_Read_AtEof_ReturnsZero", () =>
        {
            Assert.True(VfsManager.TryOpenDirectory(Fat32Mount, out IVfsDirectoryHandle? root));
            Assert.True(root!.TryCreateFile("EOF.BIN", VfsMode.RegularFile, out _));

            byte[] payload = MakePayload(EofPayloadBytes, EofSalt);
            using (IVfsFileHandle? w = OpenFile(Fat32Mount + "/EOF.BIN"))
            {
                w!.Write(payload);
            }

            using IVfsFileHandle? r = OpenFile(Fat32Mount + "/EOF.BIN");
            Assert.True(r!.TrySeek(0, SeekWhence.End));
            byte[] tail = new byte[EofProbeBytes];
            long n = r.Read(tail);
            Assert.Equal<long>(0, n);
        });

        TR.Run("Test_Fat32_Vfs_LargeFile_AcrossManyClusters", () =>
        {
            Assert.True(VfsManager.TryOpenDirectory(Fat32Mount, out IVfsDirectoryHandle? root));
            Assert.True(root!.TryCreateFile("BIG32.BIN", VfsMode.RegularFile, out _));

            // 256 KB file at 512 B/cluster (FAT32 SPC=1) = 512 clusters.
            const int totalBytes = Big32PayloadBytes;
            byte[] payload = MakePayload(totalBytes, Big32Salt);
            using (IVfsFileHandle? w = OpenFile(Fat32Mount + "/BIG32.BIN"))
            {
                long written = w!.Write(payload);
                Assert.Equal<long>(totalBytes, written);
            }

            using IVfsFileHandle? r = OpenFile(Fat32Mount + "/BIG32.BIN");
            byte[] readBack = new byte[totalBytes];
            long readBytes = r!.Read(readBack);
            Assert.Equal<long>(totalBytes, readBytes);
            AssertBytesEqual(payload, readBack);
        });

        TR.Run("Test_Fat32_Vfs_LongFileName", () =>
        {
            Assert.True(VfsManager.TryOpenDirectory(Fat32Mount, out IVfsDirectoryHandle? root));
            const string lfn = "very_long_file_name_test.bin";
            Assert.True(root!.TryCreateFile(lfn, VfsMode.RegularFile, out _));

            using IVfsFileHandle? f = OpenFile(Fat32Mount + "/" + lfn);
            Assert.NotNull(f);
            byte[] payload = MakePayload(LfnPayloadBytes, LfnSalt);
            f!.Write(payload);
            f.TryFlush();

            using IVfsFileHandle? rByLong = OpenFile(Fat32Mount + "/" + lfn);
            Assert.NotNull(rByLong);
            byte[] tmp = new byte[payload.Length];
            Assert.Equal<long>(payload.Length, rByLong!.Read(tmp));
            AssertBytesEqual(payload, tmp);

            using IVfsFileHandle? rByShort = OpenFile(Fat32Mount + "/VERY_L~1.BIN");
            Assert.NotNull(rByShort);
        });

        TR.Run("Test_Fat32_Vfs_ManyFiles_RootGrowsAcrossClusters", () =>
        {
            // FAT32 root is cluster-chained; at SPC=1 (512 B/cluster) every
            // 16 entries pushes root past a cluster boundary. Create 64
            // distinct files and confirm each one is reachable afterwards.
            Assert.True(VfsManager.TryOpenDirectory(Fat32Mount, out IVfsDirectoryHandle? root));

            const int count = RootGrowthFileCount;
            for (int i = 0; i < count; i++)
            {
                string name = "F" + IntToHex(i, HexNameDigits) + ".TXT";
                Assert.True(root!.TryCreateFile(name, VfsMode.RegularFile, out _));
            }

            for (int i = 0; i < count; i++)
            {
                string name = "F" + IntToHex(i, HexNameDigits) + ".TXT";
                Assert.True(root!.TryLookup(name, out IVfsNodeHandle? child));
                using (child)
                {
                    Assert.NotNull(child);
                }
            }
        });

        TR.Run("Test_Fat32_Vfs_NestedDirectories", () =>
        {
            Assert.True(VfsManager.TryOpenDirectory(Fat32Mount, out IVfsDirectoryHandle? root));
            Assert.True(root!.TryCreateDirectory("A", VfsMode.Directory, out IVfsDirectoryHandle? a));
            Assert.True(a!.TryCreateDirectory("B", VfsMode.Directory, out IVfsDirectoryHandle? b));
            Assert.True(b!.TryCreateDirectory("C", VfsMode.Directory, out IVfsDirectoryHandle? c));
            Assert.True(c!.TryCreateFile("LEAF.TXT", VfsMode.RegularFile, out _));

            byte[] payload = MakePayload(NestedDirsPayloadBytes, NestedDirsSalt);
            using (IVfsFileHandle? leaf = OpenFile(Fat32Mount + "/A/B/C/LEAF.TXT"))
            {
                Assert.NotNull(leaf);
                leaf!.Write(payload);
            }

            using IVfsFileHandle? readback = OpenFile(Fat32Mount + "/A/B/C/LEAF.TXT");
            Assert.NotNull(readback);
            byte[] got = new byte[payload.Length];
            Assert.Equal<long>(payload.Length, readback!.Read(got));
            AssertBytesEqual(payload, got);
        });

        TR.Run("Test_Fat32_Vfs_Truncate_GrowsViaSetAttr", () =>
        {
            Assert.True(VfsManager.TryOpenDirectory(Fat32Mount, out IVfsDirectoryHandle? root));
            Assert.True(root!.TryCreateFile("GROW.BIN", VfsMode.RegularFile, out IVfsNodeHandle? created));
            Assert.NotNull(created);

            using (created)
            {
                // SetAttr to enlarge the file from 0 to 4096 bytes; the FS must
                // allocate clusters and zero-fill them.
                VfsStat want = default;
                want.Size = GrowTargetBytes;
                Assert.True(created!.Inode.InodeOperations.SetAttr(created.Inode, SetAttrFlags.Size, want));
            }

            using IVfsFileHandle? r = OpenFile(Fat32Mount + "/GROW.BIN");
            Assert.NotNull(r);
            byte[] readBack = new byte[GrowTargetBytes];
            long readBytes = r!.Read(readBack);
            Assert.Equal<long>(GrowTargetBytes, readBytes);
            for (int i = 0; i < readBack.Length; i++)
            {
                Assert.Equal<byte>(0, readBack[i]);
            }
        });

        TR.Run("Test_Fat32_Vfs_Unlink_FreesClusters", () =>
        {
            Assert.True(VfsManager.TryGetMount(Fat32Mount, out VfsManager.VfsMount? mount));
            Assert.True(mount!.Superblock.SuperOperations.StatFs(mount.Superblock, out VfsStatFs before));

            Assert.True(VfsManager.TryOpenDirectory(Fat32Mount, out IVfsDirectoryHandle? root));
            Assert.True(root!.TryCreateFile("HOG.BIN", VfsMode.RegularFile, out _));
            using (IVfsFileHandle? w = OpenFile(Fat32Mount + "/HOG.BIN"))
            {
                w!.Write(MakePayload(HogPayloadBytes, HogSalt));
            }

            Assert.True(root.TryUnlink("HOG.BIN"));
            Assert.False(root.TryLookup("HOG.BIN", out _));

            Assert.True(mount.Superblock.SuperOperations.StatFs(mount.Superblock, out VfsStatFs after));
            // Unlink must return Bfree to within +/-8 clusters of its
            // pre-test value: net-neutrality catches any of the 128
            // freed clusters (64 KiB at SPC=1) leaking, with tolerance
            // for root-directory growth.
            Assert.True(after.Bfree >= before.Bfree - BfreeToleranceClusters && after.Bfree + BfreeToleranceClusters >= before.Bfree);
        });

        TR.Run("Test_Fat32_Persistence_AcrossUnmount", () =>
        {
            Assert.True(VfsManager.TryOpenDirectory(Fat32Mount, out IVfsDirectoryHandle? root));
            Assert.True(root!.TryCreateFile("SURVIVE.TXT", VfsMode.RegularFile, out _));
            byte[] payload = MakePayload(SurvivePayloadBytes, SurviveSalt);
            using (IVfsFileHandle? w = OpenFile(Fat32Mount + "/SURVIVE.TXT"))
            {
                w!.Write(payload);
                w.TryFlush();
            }

            // Drop the superblock and remount on the same backing buffer.
            Assert.True(VfsManager.TryGetMount(Fat32Mount, out VfsManager.VfsMount? oldMount));
            oldMount!.Superblock.SuperOperations.Drop(oldMount.Superblock);

            FatFilesystemType freshDriver = new(fat32Disk);
            Assert.True(VfsManager.RegisterFilesystem("fat32-remount", freshDriver));
            Assert.True(VfsManager.TryMount("fat32-remount", "", MountFlags.None, "/fat32b", out _));

            using IVfsFileHandle? r = OpenFile("/fat32b/SURVIVE.TXT");
            Assert.NotNull(r);
            byte[] readBack = new byte[payload.Length];
            Assert.Equal<long>(payload.Length, r!.Read(readBack));
            AssertBytesEqual(payload, readBack);
        });

        // ---------- VfsManager.TryFormat / TryDestroy round-trip ----------

        TR.Run("Test_Vfs_Format_Fat32_RoundTrip", () =>
        {
            // Fresh empty disk -> format via VfsManager -> mount -> create +
            // write across cluster boundaries -> read back. Proves the formatter
            // produced a real FAT32, not just one that statfs's correctly.
            MemoryBlockDevice freshDisk = scratchDisk.Reconfigure("MEMFAT32B", FatTestVolume.BlockSize, FatTestVolume.Fat32BlockCount);
            FatFilesystemType driver = new(freshDisk);
            Assert.True(VfsManager.RegisterFilesystem("fat32-fresh", driver));

            FatFormatOptions opts = new()
            {
                Type = FatType.Fat32,
                SectorsPerCluster = OneSectorPerCluster,
                ReservedSectorCount = Fat32ReservedSectors,
                NumberOfFats = FatTestVolume.FatCopyCount,
                FatSectorCount = Fat32FreshFatSectors,
                RootCluster = FatTable.FirstDataCluster,
                VolumeLabel = "FRESHFAT32 ",
            };
            Assert.True(VfsManager.TryFormat("fat32-fresh", "", opts));

            Assert.True(VfsManager.TryMount("fat32-fresh", "", MountFlags.None, "/freshfat32", out VfsManager.VfsMount? mount));
            Assert.True(mount!.Superblock.SuperOperations.StatFs(mount.Superblock, out VfsStatFs stat));
            Assert.True(stat.Blocks >= Fat32MinClusterCount);

            // Write spans many clusters at SPC=1 (8 KiB = 16 clusters).
            byte[] payload = MakePayload(FreshFormatPayloadBytes, FreshFormatSalt);
            Assert.True(VfsManager.TryOpenDirectory("/freshfat32", out IVfsDirectoryHandle? rootDir));
            Assert.True(rootDir!.TryCreateFile("FORMAT.BIN", VfsMode.RegularFile, out _));
            using (IVfsFileHandle? w = OpenFile("/freshfat32/FORMAT.BIN"))
            {
                Assert.NotNull(w);
                Assert.Equal<long>(payload.Length, w!.Write(payload));
                w.TryFlush();
            }

            using IVfsFileHandle? r = OpenFile("/freshfat32/FORMAT.BIN");
            Assert.NotNull(r);
            byte[] readBack = new byte[payload.Length];
            Assert.Equal<long>(payload.Length, r!.Read(readBack));
            AssertBytesEqual(payload, readBack);

            // Persist across remount on the same backing buffer.
            Assert.True(VfsManager.TryGetMount("/freshfat32", out VfsManager.VfsMount? m2));
            m2!.Superblock.SuperOperations.Drop(m2.Superblock);

            FatFilesystemType remountDriver = new(freshDisk);
            Assert.True(VfsManager.RegisterFilesystem("fat32-fresh-remount", remountDriver));
            Assert.True(VfsManager.TryMount("fat32-fresh-remount", "", MountFlags.None, "/freshfat32b", out _));

            using IVfsFileHandle? r2 = OpenFile("/freshfat32b/FORMAT.BIN");
            Assert.NotNull(r2);
            byte[] readBack2 = new byte[payload.Length];
            Assert.Equal<long>(payload.Length, r2!.Read(readBack2));
            AssertBytesEqual(payload, readBack2);
        });

        TR.Run("Test_Vfs_Destroy_PreventsRemount", () =>
        {
            MemoryBlockDevice disk = FatTestVolume.FormatFat16(
                scratchDisk.Reconfigure("MEMFAT16D", FatTestVolume.BlockSize, FatTestVolume.Fat16BlockCount));
            FatFilesystemType driver = new(disk);
            Assert.True(VfsManager.RegisterFilesystem("fat16-destroy", driver));

            // Pre-destroy: the BPB is valid, mount works.
            Assert.True(VfsManager.TryMount("fat16-destroy", "", MountFlags.None, "/destroy-pre", out _));

            // Destroy wipes the label head; unmount first (a mounted
            // source is refused) so no live mount sits over wiped storage.
            Assert.True(VfsManager.TryUnmount("/destroy-pre"));
            Assert.True(VfsManager.TryDestroy("fat16-destroy", ""));

            FatFilesystemType post = new(disk);
            Assert.True(VfsManager.RegisterFilesystem("fat16-destroyed", post));
            Assert.False(VfsManager.TryMount("fat16-destroyed", "", MountFlags.None, "/destroy-post", out _));
        });

        TR.Run("Test_Vfs_Format_Fat12_Mountable", () =>
        {
            // ~3 MiB / SPC=1 / 1-sector reserved -> ~6000 sectors of data, well inside FAT12 band.
            MemoryBlockDevice disk = scratchDisk.Reconfigure("MEMFAT12", FatTestVolume.BlockSize, Fat12DiskBlockCount);
            FatFilesystemType driver = new(disk);
            Assert.True(VfsManager.RegisterFilesystem("fat12-fresh", driver));

            FatFormatOptions opts = new()
            {
                Type = FatType.Fat12,
                SectorsPerCluster = Fat12SectorsPerCluster,
                ReservedSectorCount = Fat12ReservedSectors,
                NumberOfFats = FatTestVolume.FatCopyCount,
                RootEntryCount = Fat12RootEntryCount,
                FatSectorCount = Fat12FatSectors,
                VolumeLabel = "FAT12VOL   ",
            };
            Assert.True(VfsManager.TryFormat("fat12-fresh", "", opts));

            Assert.True(VfsManager.TryMount("fat12-fresh", "", MountFlags.None, "/fat12", out VfsManager.VfsMount? mount));
            Assert.True(mount!.Superblock.SuperOperations.StatFs(mount.Superblock, out VfsStatFs stat));
            Assert.True(stat.Blocks > 0);
            Assert.True(stat.Blocks < Fat16MinClusterCount);

            // Real write through the freshly formatted FAT12.
            byte[] payload = MakePayload(Fat12PayloadBytes, Fat12Salt);
            Assert.True(VfsManager.TryOpenDirectory("/fat12", out IVfsDirectoryHandle? rootDir));
            Assert.True(rootDir!.TryCreateFile("HELLO.TXT", VfsMode.RegularFile, out _));
            using (IVfsFileHandle? w = OpenFile("/fat12/HELLO.TXT"))
            {
                Assert.NotNull(w);
                Assert.Equal<long>(payload.Length, w!.Write(payload));
                w.TryFlush();
            }
            using IVfsFileHandle? r = OpenFile("/fat12/HELLO.TXT");
            Assert.NotNull(r);
            byte[] back = new byte[payload.Length];
            Assert.Equal<long>(payload.Length, r!.Read(back));
            AssertBytesEqual(payload, back);
        });

        // fatgen103 §6.2: one FAT copy must hold an entry for every data
        // cluster plus the two reserved entries. An undersized FAT makes
        // FatTable index past the FAT region into the second copy or the
        // data area — silent corruption on any moderately full volume.
        TR.Run("Test_Format_FatSize_CoversClusterCount", () =>
        {
            // Explicit FAT32 (33 MiB, SPC auto-picks 1 → ~66k clusters):
            // entries are 4 bytes — 128 per 512-byte sector, half the
            // FAT16 density.
            AssertFormattedFatCovers(
                scratchDisk.Reconfigure("FMT32", FatTestVolume.BlockSize, FatTestVolume.Fat32BlockCount),
                new FatFormatOptions { Type = FatType.Fat32 });
            // Auto-detected FAT16 (32 MiB, SPC auto-picks 8 → ~8k
            // clusters): the FAT must be sized for the resolved type, not
            // the FAT12 first guess.
            AssertFormattedFatCovers(
                scratchDisk.Reconfigure("FMTAUTO16", FatTestVolume.BlockSize, FatTestVolume.Fat16BlockCount),
                new FatFormatOptions());
            // Auto-detected FAT12 baseline.
            AssertFormattedFatCovers(
                scratchDisk.Reconfigure("FMTAUTO12", FatTestVolume.BlockSize, Fat12AutoDetectBlockCount),
                new FatFormatOptions());
        });

        // Writers reject what the parser would drop, and cleanly: a
        // sub-512-byte sector crashed the fixed-offset BPB writer
        // mid-format, an oversized one truncated the 16-bit BPB field
        // into an unmountable volume, and an out-of-band FAT32 root
        // cluster underflowed ZeroRootArea into a wild write after the
        // BPB and FATs were already on disk.
        TR.Run("Test_Format_RejectsBogusGeometry", () =>
        {
            Assert.True(FormatRefusedCleanly(
                scratchDisk.Reconfigure("FMTSS256", UndersizedSectorBytes, UndersizedSectorBlockCount), new FatFormatOptions()),
                "a 256-byte-sector device must be refused, not crash the BPB writer");
            Assert.True(FormatRefusedCleanly(
                scratchDisk.Reconfigure("FMTSS64K", OversizedSectorBytes, OversizedSectorBlockCount), new FatFormatOptions()),
                "a 64 KiB-sector device must be refused, not truncated into an unmountable BPB");
            // One FAT32-band reconfiguration shared by both root-cluster cases.
            MemoryBlockDevice fat32Band = scratchDisk.Reconfigure("FMTRC", FatTestVolume.BlockSize, FatTestVolume.Fat32BlockCount);
            Assert.True(FormatRefusedCleanly(
                fat32Band,
                new FatFormatOptions { Type = FatType.Fat32, RootCluster = RootClusterBelowDataArea }),
                "a FAT32 root cluster below 2 must be refused");
            Assert.True(FormatRefusedCleanly(
                fat32Band,
                new FatFormatOptions { Type = FatType.Fat32, RootCluster = RootClusterPastClusterCount }),
                "a FAT32 root cluster beyond the cluster count must be refused");
        });

        // The RAM test double must honor the throw-on-failure IBlockDevice
        // contract: (int)(blockNo * BlockSize) truncates, and a product
        // that is a multiple of 2^32 aliases sector 0 — a driver bug
        // computing a wild sector would read plausible data and pass.
        TR.Run("Test_MemoryBlockDevice_ThrowsOutOfRange", () =>
        {
            MemoryBlockDevice disk = scratchDisk.Reconfigure("MBDRANGE", FatTestVolume.BlockSize, RangeCheckDiskBlockCount);
            byte[] sector = new byte[SectorSizeBytes];
            // Stamp sector 0 so a silent alias would be detectable.
            sector[0] = AliasCanaryFill;
            disk.WriteBlock(BootSectorLba, 1, sector);

            Assert.True(ReadThrowsOutOfRange(disk, disk.BlockCount, 1),
                "a read past the device end must throw");
            // 8388608 * 512 == 2^32: the truncated cast yields offset 0.
            Assert.True(ReadThrowsOutOfRange(disk, Uint32WrapBlockNumber, 1),
                "a read whose byte offset wraps 2^32 must throw, not alias sector 0");
            Assert.True(ReadThrowsOutOfRange(disk, disk.BlockCount - 1, StraddlingReadBlocks),
                "a read straddling the device end must throw");
        });

        // The BPB is on-disk metadata: parse/mount-time validation must
        // reject what would otherwise wrap the uint geometry math into a
        // self-consistent-looking volume or wild cluster LBAs.
        TR.Run("Test_Mount_RejectsCorruptBpb", () =>
        {
            MemoryBlockDevice disk = scratchDisk.Reconfigure("BPBHARD", FatTestVolume.BlockSize, HardeningDiskBlockCount);

            Assert.True(MountRefusedCleanly(CorruptBpb(disk, PatchBpbFatRegionWraps)),
                "a FAT region that wraps uint must be rejected");
            Assert.True(MountRefusedCleanly(CorruptBpb(disk, PatchBpbTotalPastDevice)),
                "a volume extending past the device must be rejected");
            Assert.True(MountRefusedCleanly(CorruptBpb(disk, PatchBpbZeroFat)),
                "a zero-length FAT must be rejected");
            Assert.True(MountRefusedCleanly(CorruptBpb(disk, PatchBpbBadSpc)),
                "a non-power-of-two SectorsPerCluster must be rejected");

            // FAT32 root cluster 0 underflows ClusterToLba's cluster - 2.
            MemoryBlockDevice disk32 = FatTestVolume.FormatFat32(
                scratchDisk.Reconfigure("BPBHARD32", FatTestVolume.BlockSize, FatTestVolume.Fat32BlockCount));
            byte[] sector = new byte[SectorSizeBytes];
            disk32.ReadBlock(BootSectorLba, 1, sector);
            BitConverter.TryWriteBytes(sector.AsSpan(BpbRootClusOffset, BpbDwordBytes), ReservedClusterZero);
            disk32.WriteBlock(BootSectorLba, 1, sector);
            Assert.True(MountRefusedCleanly(disk32),
                "a FAT32 root cluster below 2 must be rejected at mount");

            // ClusterToLba bounds on a healthy volume.
            MemoryBlockDevice valid = scratchDisk.Reconfigure("BPBRANGE", FatTestVolume.BlockSize, HardeningDiskBlockCount);
            FatFilesystemType fmt = new(valid);
            Assert.True(fmt.TryFormat(default, new FatFormatOptions()));
            valid.ReadBlock(BootSectorLba, 1, sector);
            Assert.True(FatBootSector.TryParse(sector, out FatBootSector? bs) && bs != null);
            Assert.True(ClusterToLbaThrows(bs!, ReservedClusterZero),
                "ClusterToLba(0) must throw, not underflow into an exabyte LBA");
            Assert.True(ClusterToLbaThrows(bs!, bs!.ClusterCount + ClusterCountOvershoot),
                "ClusterToLba beyond the cluster count must throw");
        });

        // A corrupt FAT entry or dirent FirstCluster with a huge cluster
        // number passes the EOC/bad checks: GetChain follows it, Get reads
        // sectors past the first FAT copy as entries, and Free()'s
        // read-modify-write lands outside the FAT region (mirrored
        // NumberOfFats times) — silent corruption of other FAT copies or
        // file data.
        TR.Run("Test_FatTable_BoundsClusterNumbers", () =>
        {
            MemoryBlockDevice disk = scratchDisk.Reconfigure("FATBOUND", FatTestVolume.BlockSize, HardeningDiskBlockCount);
            FatFilesystemType driver = new(disk);
            Assert.True(driver.TryFormat(default, new FatFormatOptions()));
            byte[] sector = new byte[SectorSizeBytes];
            disk.ReadBlock(BootSectorLba, 1, sector);
            Assert.True(FatBootSector.TryParse(sector, out FatBootSector? bs) && bs != null);
            FatTable table = new(disk, bs!);

            uint first = table.AllocateChain(ProbeChainClusters);
            Assert.True(first != 0);
            Assert.Equal(ProbeChainClusters, table.GetChain(first).Count);

            // Corrupt the middle link to a cluster far past the volume's
            // count (still decodable — it lands inside the second FAT copy).
            List<uint> chain = table.GetChain(first);
            uint wild = bs!.ClusterCount + WildLinkOvershoot;
            table.Set(chain[1], wild);

            Assert.Equal(TruncatedChainLength, table.GetChain(first).Count,
                "the chain walk must stop at an out-of-range link");
            Assert.True(table.IsEndOfChain(table.Get(wild)),
                "Get outside the volume's clusters must return EOC, not decode other sectors");

            // Set on an out-of-range cluster must not touch the device:
            // snapshot the two sectors the unbounded math would RMW (one
            // in the second FAT copy, its mirror in the region beyond).
            uint wildOffset = wild + wild / Fat12EntriesPerPair;
            ulong wildLba = bs!.FatStartLba + wildOffset / SectorSizeBytes;
            ulong mirrorLba = wildLba + bs!.FatSectorCount;
            byte[] beforeA = new byte[SectorSizeBytes];
            byte[] beforeB = new byte[SectorSizeBytes];
            disk.ReadBlock(wildLba, 1, beforeA);
            disk.ReadBlock(mirrorLba, 1, beforeB);
            table.Set(wild, FatTable.FreeCluster);
            byte[] afterA = new byte[SectorSizeBytes];
            byte[] afterB = new byte[SectorSizeBytes];
            disk.ReadBlock(wildLba, 1, afterA);
            disk.ReadBlock(mirrorLba, 1, afterB);
            AssertBytesEqual(beforeA, afterA);
            AssertBytesEqual(beforeB, afterB);

            // FAT32's gap below the EOC band is huge; a wild link must
            // resolve to EOC without out-of-range device I/O.
            MemoryBlockDevice disk32 = FatTestVolume.FormatFat32(
                scratchDisk.Reconfigure("FATBOUND32", FatTestVolume.BlockSize, FatTestVolume.Fat32BlockCount));
            disk32.ReadBlock(BootSectorLba, 1, sector);
            Assert.True(FatBootSector.TryParse(sector, out FatBootSector? bs32) && bs32 != null);
            FatTable table32 = new(disk32, bs32!);
            Assert.True(GetReturnsEocSafely(table32, Fat32WildCluster),
                "a wild FAT32 cluster must resolve to EOC, not out-of-range I/O");
        });

        // Unlink invalidates nothing on a still-referenced inode, so a
        // later Fsync through a stale handle patches whatever directory
        // entry reused the slot — cross-linking the new file onto the old
        // file's freed clusters.
        TR.Run("Test_Fat16_StaleHandleFsync_DoesNotClobberReusedSlot", () =>
        {
            IVfsInode root = ResolveRoot(Fat16Mount);
            Assert.True(root.InodeOperations.Create(root, "STALEA.TXT", VfsMode.RegularFile, out IVfsInode? fileA));
            byte[] payloadA = MakePayload(StaleAPayloadBytes, StaleASalt);
            WriteAll(fileA!, payloadA);

            Assert.True(root.InodeOperations.Unlink(root, "STALEA.TXT"));

            // Same slot count (plain 8.3 name): reuses A's freed slot.
            Assert.True(root.InodeOperations.Create(root, "STALEB.TXT", VfsMode.RegularFile, out IVfsInode? fileB));
            byte[] payloadB = MakePayload(StaleBPayloadBytes, StaleBSalt);
            WriteAll(fileB!, payloadB);

            // Fsync through the stale handle must not patch B's entry.
            IFileOperations opsA = fileA!.FileOperations!;
            opsA.Fsync(new TestOpenFile(fileA!, opsA));

            Assert.True(root.InodeOperations.Lookup(root, "STALEB.TXT", out IVfsInode? lookupB));
            Assert.True(lookupB!.InodeOperations.GetAttr(lookupB, out VfsStat statB));
            Assert.Equal<ulong>((ulong)payloadB.Length, statB.Size);
            byte[] readBack = ReadAll(lookupB!, payloadB.Length);
            AssertBytesEqual(payloadB, readBack);
        });

        // SetAttr(Mode) updates the in-memory attributes and relies on
        // UpdateInodeEntry to persist them; the attribute byte must reach
        // disk or the change evaporates on the next Lookup.
        TR.Run("Test_Fat16_SetAttrReadOnly_Persists", () =>
        {
            IVfsInode root = ResolveRoot(Fat16Mount);
            // Create writable (mode with a write bit maps to no ReadOnly
            // attribute), then chmod it read-only via SetAttr.
            Assert.True(root.InodeOperations.Create(
                root, "ROFLAG.TXT", VfsMode.RegularFile | VfsMode.OwnerRead | VfsMode.OwnerWrite, out IVfsInode? created));
            Assert.True(root.InodeOperations.Lookup(root, "ROFLAG.TXT", out IVfsInode? sanity));
            Assert.True(sanity!.InodeOperations.GetAttr(sanity, out VfsStat sanityStat));
            Assert.True((sanityStat.Mode & VfsMode.OwnerWrite) != 0, "baseline file must be writable on disk");

            VfsStat wanted = default;
            wanted.Mode = VfsMode.RegularFile | VfsMode.OwnerRead | VfsMode.GroupRead | VfsMode.OtherRead;
            Assert.True(created!.InodeOperations.SetAttr(created, SetAttrFlags.Mode, wanted));

            // A fresh Lookup re-parses the on-disk entry.
            Assert.True(root.InodeOperations.Lookup(root, "ROFLAG.TXT", out IVfsInode? lookup));
            Assert.True(lookup!.InodeOperations.GetAttr(lookup, out VfsStat stat));
            Assert.True((stat.Mode & VfsMode.OwnerWrite) == 0,
                "the ReadOnly attribute must survive a re-parse of the on-disk entry");
        });

        // A maximal LFN needs 21 slots (672 bytes) but a 512-byte cluster
        // holds 16: growing by exactly one cluster can never satisfy the
        // run, so creates in a packed directory fail despite free space.
        TR.Run("Test_GrowDirectory_FitsMaxLfnRun", () =>
        {
            MemoryBlockDevice disk = scratchDisk.Reconfigure("GROWDIR", FatTestVolume.BlockSize, Fat16ScratchBlockCount);
            FatFilesystemType driver = new(disk);
            Assert.True(driver.TryFormat(default, new FatFormatOptions { Type = FatType.Fat16, SectorsPerCluster = OneSectorPerCluster }));
            Assert.True(driver.TryMount(default, MountFlags.None, out IVfsSuperblock? sb));
            IVfsInode root = sb!.Root;

            Assert.True(root.InodeOperations.Mkdir(root, "PACK", VfsMode.Directory, out IVfsInode? dir));
            // '.' and '..' occupy 2 of the 16 slots; fill the other 14.
            for (int i = 0; i < PackFillEntries; i++)
            {
                string name = "F" + (i / DecimalBase) + (i % DecimalBase) + ".TXT";
                Assert.True(dir!.InodeOperations.Create(dir, name, VfsMode.RegularFile, out _));
            }

            string maxLfn = new string('x', MaxLfnStemLength) + ".txt";
            Assert.True(dir!.InodeOperations.Create(dir, maxLfn, VfsMode.RegularFile, out _),
                "a packed directory must grow enough clusters to fit a maximal LFN run");
            Assert.True(dir.InodeOperations.Lookup(dir, maxLfn, out _));
        });

        // VFS sync and unmount are durability points: they must flush the
        // device's volatile write cache.
        TR.Run("Test_Superblock_SyncAndDrop_Flush", () =>
        {
            MemoryBlockDevice disk = scratchDisk.Reconfigure("SYNCFLUSH", FatTestVolume.BlockSize, Fat16ScratchBlockCount);
            FatFilesystemType driver = new(disk);
            Assert.True(driver.TryFormat(default, new FatFormatOptions { Type = FatType.Fat16, SectorsPerCluster = Fat16ScratchSectorsPerCluster }));
            Assert.True(driver.TryMount(default, MountFlags.None, out IVfsSuperblock? sb));

            int before = disk.FlushCount;
            Assert.True(sb!.SuperOperations.Sync(sb));
            Assert.True(disk.FlushCount > before, "Sync must flush the device write cache");

            before = disk.FlushCount;
            sb.SuperOperations.Drop(sb);
            Assert.True(disk.FlushCount > before, "Drop (unmount) must flush the device write cache");
        });

        // Short-name generation must never silently lose the requested
        // spelling: mangled names take the LFN path, and generated 8.3
        // tails must be unique within the directory.
        TR.Run("Test_FatDirectory_ShortNameAndLfnHardening", () =>
        {
            IVfsInode root = ResolveRoot(Fat16Mount);

            Assert.True(root.InodeOperations.Create(root, "A+B.TXT", VfsMode.RegularFile, out _));
            Assert.True(root.InodeOperations.Lookup(root, "A+B.TXT", out _),
                "a name the 8.3 encoder mangles must take the LFN path");

            Assert.True(root.InodeOperations.Create(root, "TailCollisionA.txt", VfsMode.RegularFile, out IVfsInode? lfA));
            Assert.True(root.InodeOperations.Create(root, "TailCollisionB.txt", VfsMode.RegularFile, out IVfsInode? lfB));
            WriteAll(lfA!, MakePayload(TailCollisionPayloadBytes, TailCollisionASalt));
            WriteAll(lfB!, MakePayload(TailCollisionPayloadBytes, TailCollisionBSalt));
            Assert.True(root.InodeOperations.Lookup(root, "TAILCO~2.TXT", out IVfsInode? byTail),
                "the second colliding long name must get a ~2 tail");
            if (byTail != null)
            {
                AssertBytesEqual(MakePayload(TailCollisionPayloadBytes, TailCollisionBSalt), ReadAll(byTail, TailCollisionPayloadBytes));
            }

            Assert.False(root.InodeOperations.Create(root, new string('y', OverlongNameLength), VfsMode.RegularFile, out _),
                "names longer than the 255-char LFN cap must be refused");
        });

        // LFN chains and reserved dirent fields are on-disk metadata:
        // orphaned LFN entries must not attach to the next short entry,
        // FstClusHI is reserved on FAT12/16 (EA handle may be nonzero),
        // and consuming the 0x00 terminator must re-terminate or stale
        // bytes past it get parsed back to life.
        TR.Run("Test_FatDirectory_DistrustsOnDiskMetadata", () =>
        {
            MemoryBlockDevice disk = scratchDisk.Reconfigure("DIRHARD", FatTestVolume.BlockSize, Fat16ScratchBlockCount);
            FatFilesystemType driver = new(disk);
            Assert.True(driver.TryFormat(default, new FatFormatOptions { Type = FatType.Fat16, SectorsPerCluster = Fat16ScratchSectorsPerCluster }));
            Assert.True(driver.TryMount(default, MountFlags.None, out IVfsSuperblock? sb));
            IVfsInode root = sb!.Root;
            byte[] bpbSector = new byte[SectorSizeBytes];
            disk.ReadBlock(BootSectorLba, 1, bpbSector);
            Assert.True(FatBootSector.TryParse(bpbSector, out FatBootSector? bs) && bs != null);

            // FstClusHI carries an EA handle on real FAT16 volumes.
            Assert.True(root.InodeOperations.Create(root, "EAFILE.TXT", VfsMode.RegularFile, out IVfsInode? ea));
            byte[] payload = MakePayload(EaFilePayloadBytes, EaFileSalt);
            WriteAll(ea!, payload);
            PatchRootEntry(disk, bs!, "EAFILE  TXT", static (region, off) =>
            {
                BitConverter.TryWriteBytes(region.AsSpan(off + FatDirectory.FirstClusterHighOffset, FatDirectory.ClusterWordBytes), EaHandleStamp);
            });
            Assert.True(root.InodeOperations.Lookup(root, "EAFILE.TXT", out IVfsInode? eaBack));
            if (eaBack != null)
            {
                AssertBytesEqual(payload, ReadAll(eaBack, payload.Length));
            }

            // A non-LFN-aware tool renaming the 8.3 entry in place leaves
            // the preceding LFN chain orphaned; its checksum no longer
            // matches and it must not lend its name to the renamed entry.
            Assert.True(root.InodeOperations.Create(root, "orphantest.txt", VfsMode.RegularFile, out _));
            Assert.True(root.InodeOperations.Create(root, "PLAIN.TXT", VfsMode.RegularFile, out _));
            PatchRootEntry(disk, bs!, "ORPHAN~1TXT", static (region, off) =>
            {
                ReadOnlySpan<char> renamed = "RENAMED TXT";
                for (int i = 0; i < ShortNameLength; i++)
                {
                    region[off + i] = (byte)renamed[i];
                }
            });
            Assert.False(root.InodeOperations.Lookup(root, "orphantest.txt", out _),
                "an orphaned LFN chain must not attach to a rewritten short entry");
            Assert.True(root.InodeOperations.Lookup(root, "RENAMED.TXT", out _));
            Assert.True(root.InodeOperations.Lookup(root, "PLAIN.TXT", out _));

            // Plant a plausible stale entry right after the terminator,
            // then create a file over the terminator slot.
            PlantAfterTerminator(disk, bs!);
            Assert.True(root.InodeOperations.Create(root, "REALFILE.TXT", VfsMode.RegularFile, out _));
            Assert.False(root.InodeOperations.Lookup(root, "GHOST.TXT", out _),
                "stale bytes past the consumed terminator must not be parsed back to life");
        });

        // Namespace-operation ordering and integer-width guards.
        TR.Run("Test_FatInodeOps_OrderingAndGuards", () =>
        {
            MemoryBlockDevice disk = scratchDisk.Reconfigure("INODEOPS", FatTestVolume.BlockSize, Fat16ScratchBlockCount);
            FatFilesystemType driver = new(disk);
            Assert.True(driver.TryFormat(default, new FatFormatOptions { Type = FatType.Fat16, SectorsPerCluster = Fat16ScratchSectorsPerCluster }));
            Assert.True(driver.TryMount(default, MountFlags.None, out IVfsSuperblock? sb));
            IVfsInode root = sb!.Root;

            // Duplicate names produce an invalid volume with the second
            // entry unreachable — Create/Mkdir/Rename must refuse them.
            Assert.True(root.InodeOperations.Create(root, "DUP.TXT", VfsMode.RegularFile, out _));
            Assert.False(root.InodeOperations.Create(root, "DUP.TXT", VfsMode.RegularFile, out _),
                "creating an existing name must fail");
            Assert.True(root.InodeOperations.Mkdir(root, "DUPDIR", VfsMode.Directory, out _));
            Assert.False(root.InodeOperations.Mkdir(root, "DUPDIR", VfsMode.Directory, out _),
                "mkdir over an existing name must fail");
            Assert.True(root.InodeOperations.Create(root, "RSRC.TXT", VfsMode.RegularFile, out _));
            Assert.False(root.InodeOperations.Rename(root, "RSRC.TXT", root, "DUP.TXT"),
                "renaming onto an existing name must fail");

            // A failed rename must leave the source intact (destination
            // allocation refused here by the 255-char LFN cap).
            Assert.True(root.InodeOperations.Create(root, "KEEP.TXT", VfsMode.RegularFile, out _));
            Assert.False(root.InodeOperations.Rename(root, "KEEP.TXT", root, new string('z', OverlongNameLength)));
            Assert.True(root.InodeOperations.Lookup(root, "KEEP.TXT", out _),
                "a failed rename must leave the source entry intact");

            // SetAttr size: 4 GiB wraps the uint cast to 0 and truncates
            // the file — silent data loss instead of a refusal.
            Assert.True(root.InodeOperations.Create(root, "BIGSZ.TXT", VfsMode.RegularFile, out IVfsInode? bigsz));
            WriteAll(bigsz!, MakePayload(SizeCapPayloadBytes, SizeCapSalt));
            VfsStat huge = default;
            huge.Mode = VfsMode.RegularFile;
            huge.Size = SizeBeyondFatCap;
            Assert.False(bigsz!.InodeOperations.SetAttr(bigsz, SetAttrFlags.Size, huge),
                "a size beyond FAT's 4 GiB cap must be refused");
            Assert.True(bigsz.InodeOperations.GetAttr(bigsz, out VfsStat afterStat));
            Assert.Equal<ulong>(SizeCapPayloadBytes, afterStat.Size);

            // SetAttr size on a directory would wipe live directory
            // clusters via the grow path and stamp a nonzero size.
            Assert.True(root.InodeOperations.Lookup(root, "DUPDIR", out IVfsInode? dupdir));
            VfsStat dirGrow = default;
            dirGrow.Mode = VfsMode.Directory;
            dirGrow.Size = DirGrowAttemptBytes;
            Assert.False(dupdir!.InodeOperations.SetAttr(dupdir, SetAttrFlags.Size, dirGrow),
                "size changes on directories must be refused");

            // Lookup('..') must not hijack the cached parent inode.
            Assert.True(dupdir.InodeOperations.Lookup(dupdir, "..", out IVfsInode? dotdot));
            Assert.True(dotdot != null && dotdot.Name != "..",
                "Lookup('..') must resolve structurally, not mutate the cached parent");

            // A directory moved across parents must get its '..' rewritten.
            byte[] bpbSector = new byte[SectorSizeBytes];
            disk.ReadBlock(BootSectorLba, 1, bpbSector);
            Assert.True(FatBootSector.TryParse(bpbSector, out FatBootSector? bs) && bs != null);
            Assert.True(root.InodeOperations.Mkdir(root, "MOVEME", VfsMode.Directory, out _));
            Assert.True(root.InodeOperations.Mkdir(root, "DEST", VfsMode.Directory, out IVfsInode? dest));
            Assert.True(root.InodeOperations.Rename(root, "MOVEME", dest!, "MOVEME"));
            Assert.True(dest!.InodeOperations.Lookup(dest, "MOVEME", out IVfsInode? moved));
            Assert.True(moved!.InodeOperations.GetAttr(moved, out VfsStat movedStat));
            Assert.True(dest.InodeOperations.GetAttr(dest, out VfsStat destStat));
            Assert.Equal<uint>((uint)destStat.Ino & FatDirectory.ClusterWordMask, ReadDotDotClusterLow(disk, bs!, (uint)movedStat.Ino),
                "a moved directory's '..' must point at its new parent");

            // Writing past EOF must zero the gap (holes read as zero, and
            // unzeroed clusters leak other files' freed data).
            Assert.True(root.InodeOperations.Create(root, "HOLE.BIN", VfsMode.RegularFile, out IVfsInode? hole));
            WriteAll(hole!, MakePayload(HoleChunkBytes, HoleHeadSalt));
            IFileOperations holeOps = hole!.FileOperations!;
            TestOpenFile holeHandle = new(hole!, holeOps);
            Assert.True(holeOps.Seek(holeHandle, HoleFarOffset, SeekWhence.Set, out _));
            Assert.Equal<long>(HoleChunkBytes, holeOps.Write(holeHandle, MakePayload(HoleChunkBytes, HoleTailSalt)));
            TestOpenFile readBack = new(hole!, holeOps);
            Assert.True(holeOps.Seek(readBack, HoleGapProbeOffset, SeekWhence.Set, out _));
            byte[] gap = new byte[HoleGapProbeBytes];
            Assert.Equal<long>(HoleGapProbeBytes, holeOps.Read(readBack, gap));
            AssertAllZero(gap, "the gap between old EOF and a past-EOF write must read as zeros");

            // Fsync is a durability point: it must flush the device.
            int flushesBefore = disk.FlushCount;
            Assert.True(holeOps.Fsync(holeHandle));
            Assert.True(disk.FlushCount > flushesBefore, "Fsync must flush the device write cache");
        });

        // Mkdir under the FAT32 root must write 0 into '..' (fatgen103),
        // not the root's cluster number.
        TR.Run("Test_Fat32_Mkdir_DotDotIsZeroForRoot", () =>
        {
            IVfsInode root = ResolveRoot(Fat32Mount);
            Assert.True(root.InodeOperations.Mkdir(root, "DOTZERO", VfsMode.Directory, out IVfsInode? dir));
            Assert.True(dir!.InodeOperations.GetAttr(dir, out VfsStat dirStat));
            byte[] bpbSector = new byte[SectorSizeBytes];
            fat32Disk.ReadBlock(BootSectorLba, 1, bpbSector);
            Assert.True(FatBootSector.TryParse(bpbSector, out FatBootSector? bs32) && bs32 != null);
            Assert.Equal<uint>(RootDotDotClusterValue, ReadDotDotClusterLow(fat32Disk, bs32!, (uint)dirStat.Ino),
                "'..' of a root child must store cluster 0 per the FAT spec");
        });

        // Formatting or destroying a mounted source rewrites the volume
        // underneath a live superblock (stale geometry + FAT cache), so
        // the manager must refuse while a matching mount exists.
        TR.Run("Test_Vfs_FormatMounted_Refused", () =>
        {
            MemoryBlockDevice disk = FatTestVolume.FormatFat16(
                scratchDisk.Reconfigure("MNTGUARD", FatTestVolume.BlockSize, FatTestVolume.Fat16BlockCount));
            FatFilesystemType driver = new(disk);
            Assert.True(VfsManager.RegisterFilesystem("fat16-guard", driver));
            Assert.True(VfsManager.TryMount("fat16-guard", "", MountFlags.None, "/guard", out _));

            Assert.False(VfsManager.TryFormat("fat16-guard", "", null),
                "formatting a mounted source must be refused");
            Assert.False(VfsManager.TryDestroy("fat16-guard", ""),
                "destroying a mounted source must be refused");

            // After an unmount the same operations must proceed.
            Assert.True(VfsManager.TryUnmount("/guard"));
            Assert.True(VfsManager.TryDestroy("fat16-guard", ""),
                "destroy must succeed once the source is unmounted");
        });

        // Durability and spec conformance of the written volume.
        TR.Run("Test_Format_DurabilityAndSpecFields", () =>
        {
            // TotSec16: a FAT12/16 volume under 65536 sectors must store
            // its count in the 16-bit field (strict drivers and fsck.fat
            // read only TotSec16 there), with TotSec32 zero.
            MemoryBlockDevice small16 = scratchDisk.Reconfigure("FMT16SMALL", FatTestVolume.BlockSize, Fat16ScratchBlockCount);
            FatFilesystemType driver16 = new(small16);
            Assert.True(driver16.TryFormat(default, new FatFormatOptions { Type = FatType.Fat16, SectorsPerCluster = Fat16ScratchSectorsPerCluster }));
            Assert.True(small16.FlushCount > 0, "Format must flush the device before reporting success");
            byte[] bpb = new byte[SectorSizeBytes];
            small16.ReadBlock(BootSectorLba, 1, bpb);
            Assert.Equal<uint>(Fat16ScratchBlockCount, BitConverter.ToUInt16(bpb.AsSpan(BpbTotSec16Offset, BpbWordBytes)));
            Assert.Equal<uint>(BpbWideFieldUnused, BitConverter.ToUInt32(bpb.AsSpan(BpbTotSec32Offset, BpbDwordBytes)));

            // Destroy must clear the FAT32 FSInfo and backup boot sector
            // too, or BPB_BkBootSec-honoring tools can still reconstruct
            // and mount the volume.
            MemoryBlockDevice disk32 = FatTestVolume.FormatFat32(
                scratchDisk.Reconfigure("FMTDESTROY", FatTestVolume.BlockSize, FatTestVolume.Fat32BlockCount));
            FatFilesystemType driver32 = new(disk32);
            Assert.True(driver32.TryDestroy(default));
            byte[] sector = new byte[SectorSizeBytes];
            disk32.ReadBlock(FsInfoSectorLba, 1, sector);
            AssertAllZero(sector, "FSInfo sector must be wiped by Destroy");
            disk32.ReadBlock(BackupBootSectorLba, 1, sector);
            AssertAllZero(sector, "backup boot sector must be wiped by Destroy");
        });

        TR.Finish();

        Log.WriteString("\n[Tests Complete - System Halting]\n");
    }

    protected override void Run()
    {
        Stop();
    }

    protected override void AfterRun()
    {
        TR.Complete();
    }

    private static IVfsInode ResolveRoot(string mountPoint)
    {
        Assert.True(VfsManager.TryGetMount(mountPoint, out VfsManager.VfsMount? mount));
        Assert.NotNull(mount);
        return mount!.Superblock.Root;
    }

    private static IVfsFileHandle? OpenFile(string path)
    {
        VfsManager.TryOpenFile(path, out IVfsFileHandle? handle);
        return handle;
    }

    // Formats the disk fresh, applies the patch to sector 0, writes it
    // back. Keeps the corruption cells to one line per scenario.
    private static MemoryBlockDevice CorruptBpb(MemoryBlockDevice disk, Action<byte[]> patch)
    {
        FatFilesystemType driver = new(disk);
        Assert.True(driver.TryFormat(default, new FatFormatOptions()), "baseline format for a corruption cell failed");
        byte[] sector = new byte[SectorSizeBytes];
        disk.ReadBlock(BootSectorLba, 1, sector);
        patch(sector);
        disk.WriteBlock(BootSectorLba, 1, sector);
        return disk;
    }

    // FATSz16 = 0, FATSz32 = 0x80000000: with 2 FATs the uint fatRegion
    // product wraps to 0 and the geometry collapses onto the reserved area.
    private static void PatchBpbFatRegionWraps(byte[] bpb)
    {
        BitConverter.TryWriteBytes(bpb.AsSpan(BpbFatSz16Offset, BpbWordBytes), BpbNarrowFieldUnused);
        BitConverter.TryWriteBytes(bpb.AsSpan(BpbFatSz32Offset, BpbDwordBytes), WrappingFatSectorCount);
        BitConverter.TryWriteBytes(bpb.AsSpan(BpbRootClusOffset, BpbDwordBytes), FatTable.FirstDataCluster);
    }

    // Claims 4x the device's sectors.
    private static void PatchBpbTotalPastDevice(byte[] bpb)
    {
        BitConverter.TryWriteBytes(bpb.AsSpan(BpbTotSec16Offset, BpbWordBytes), BpbNarrowFieldUnused);
        BitConverter.TryWriteBytes(bpb.AsSpan(BpbTotSec32Offset, BpbDwordBytes), PastDeviceSectorMultiplier * HardeningDiskBlockCount);
    }

    private static void PatchBpbZeroFat(byte[] bpb)
    {
        BitConverter.TryWriteBytes(bpb.AsSpan(BpbFatSz16Offset, BpbWordBytes), BpbNarrowFieldUnused);
        BitConverter.TryWriteBytes(bpb.AsSpan(BpbFatSz32Offset, BpbDwordBytes), ZeroedFatSize);
    }

    private static void PatchBpbBadSpc(byte[] bpb)
    {
        bpb[BpbSecPerClusOffset] = NonPowerOfTwoSpc;
    }

    // One try/catch per method on purpose: true = TryMount returned false
    // without throwing.
    private static bool MountRefusedCleanly(MemoryBlockDevice disk)
    {
        try
        {
            FatFilesystemType driver = new(disk);
            return !driver.TryMount(default, MountFlags.None, out _);
        }
        catch (Exception)
        {
            return false;
        }
    }

    // Reads the low first-cluster word of the '..' entry (slot 1) in the
    // directory whose first cluster is dirCluster.
    private static uint ReadDotDotClusterLow(MemoryBlockDevice disk, FatBootSector bs, uint dirCluster)
    {
        byte[] cluster = new byte[bs.BytesPerCluster];
        disk.ReadBlock(bs.ClusterToLba(dirCluster), bs.SectorsPerCluster, cluster);
        return BitConverter.ToUInt16(cluster.AsSpan(FatDirectory.EntrySize + FatDirectory.FirstClusterLowOffset, FatDirectory.ClusterWordBytes));
    }

    // Locates the 8.3 entry with the given raw 11-char name in the FAT16
    // fixed root region and lets the caller patch it in place — the way
    // a foreign, non-LFN-aware tool would.
    private static void PatchRootEntry(MemoryBlockDevice disk, FatBootSector bs, string raw11, Action<byte[], int> patch)
    {
        byte[] region = new byte[bs.RootSectorCount * bs.BytesPerSector];
        disk.ReadBlock(bs.RootStartLba, bs.RootSectorCount, region);
        for (int off = 0; off + FatDirectory.EntrySize <= region.Length; off += FatDirectory.EntrySize)
        {
            bool match = true;
            for (int i = 0; i < ShortNameLength && match; i++)
            {
                match = region[off + i] == (byte)raw11[i];
            }
            if (match)
            {
                patch(region, off);
                disk.WriteBlock(bs.RootStartLba, bs.RootSectorCount, region);
                return;
            }
        }
        Assert.True(false, "raw root entry not found: " + raw11);
    }

    // Writes a plausible stale 8.3 entry ("GHOST.TXT") into the slot right
    // after the root directory's 0x00 terminator — the on-disk state the
    // spec allows after a directory was truncated by a single terminator.
    private static void PlantAfterTerminator(MemoryBlockDevice disk, FatBootSector bs)
    {
        byte[] region = new byte[bs.RootSectorCount * bs.BytesPerSector];
        disk.ReadBlock(bs.RootStartLba, bs.RootSectorCount, region);
        for (int off = 0; off + FatDirectory.EntrySize * 2 <= region.Length; off += FatDirectory.EntrySize)
        {
            if (region[off] != FatDirectory.EndOfDirectoryMarker)
            {
                continue;
            }
            int ghost = off + FatDirectory.EntrySize;
            ReadOnlySpan<char> name = "GHOST   TXT";
            for (int i = 0; i < ShortNameLength; i++)
            {
                region[ghost + i] = (byte)name[i];
            }
            region[ghost + FatDirectory.AttributesOffset] = (byte)FatAttr.Archive;
            BitConverter.TryWriteBytes(region.AsSpan(ghost + FatDirectory.FirstClusterLowOffset, FatDirectory.ClusterWordBytes), GhostFirstCluster);
            BitConverter.TryWriteBytes(region.AsSpan(ghost + FatDirectory.SizeOffset, FatDirectory.SizeFieldBytes), GhostFileSizeBytes);
            disk.WriteBlock(bs.RootStartLba, bs.RootSectorCount, region);
            return;
        }
        Assert.True(false, "no root terminator found to plant behind");
    }

    // One try/catch per method on purpose: true = Get returned an EOC
    // marker without any out-of-range device I/O throwing.
    private static bool GetReturnsEocSafely(FatTable table, uint cluster)
    {
        try
        {
            return table.IsEndOfChain(table.Get(cluster));
        }
        catch (Exception)
        {
            return false;
        }
    }

    // One try/catch per method on purpose.
    private static bool ClusterToLbaThrows(FatBootSector bs, uint cluster)
    {
        try
        {
            bs.ClusterToLba(cluster);
            return false;
        }
        catch (ArgumentOutOfRangeException)
        {
            return true;
        }
    }

    // One try/catch per method on purpose: true = the read threw the
    // contract's ArgumentOutOfRangeException.
    private static bool ReadThrowsOutOfRange(MemoryBlockDevice disk, ulong blockNo, ulong blockCount)
    {
        try
        {
            byte[] buffer = new byte[(int)(disk.BlockSize * blockCount)];
            disk.ReadBlock(blockNo, blockCount, buffer);
            return false;
        }
        catch (ArgumentOutOfRangeException)
        {
            return true;
        }
    }

    // One try/catch per method on purpose: true = TryFormat returned
    // false without throwing and without claiming success.
    private static bool FormatRefusedCleanly(MemoryBlockDevice disk, FatFormatOptions opts)
    {
        try
        {
            FatFilesystemType driver = new(disk);
            return !driver.TryFormat(default, opts);
        }
        catch (Exception)
        {
            return false;
        }
    }

    private static void AssertAllZero(byte[] sector, string message)
    {
        for (int i = 0; i < sector.Length; i++)
        {
            if (sector[i] != 0)
            {
                Assert.True(false, message);
                return;
            }
        }
        Assert.True(true, message);
    }

    // Formats the disk, decodes the fresh BPB, and checks one FAT copy's
    // capacity against the volume's cluster count (fatgen103 §6.2: data
    // clusters + 2 reserved entries must fit).
    private static void AssertFormattedFatCovers(MemoryBlockDevice disk, FatFormatOptions opts)
    {
        FatFilesystemType driver = new(disk);
        Assert.True(driver.TryFormat(default, opts));

        byte[] bpb = new byte[SectorSizeBytes];
        disk.ReadBlock(BootSectorLba, 1, bpb);
        Span<byte> b = bpb;
        uint bytesPerSector = BitConverter.ToUInt16(b.Slice(BpbBytsPerSecOffset, BpbWordBytes));
        uint spc = b[BpbSecPerClusOffset];
        uint reserved = BitConverter.ToUInt16(b.Slice(BpbRsvdSecCntOffset, BpbWordBytes));
        uint numFats = b[BpbNumFatsOffset];
        uint rootEntries = BitConverter.ToUInt16(b.Slice(BpbRootEntCntOffset, BpbWordBytes));
        uint fatSize = BitConverter.ToUInt16(b.Slice(BpbFatSz16Offset, BpbWordBytes));
        if (fatSize == BpbNarrowFieldUnused)
        {
            fatSize = BitConverter.ToUInt32(b.Slice(BpbFatSz32Offset, BpbDwordBytes));
        }
        uint totalSectors = BitConverter.ToUInt16(b.Slice(BpbTotSec16Offset, BpbWordBytes));
        if (totalSectors == BpbNarrowFieldUnused)
        {
            totalSectors = BitConverter.ToUInt32(b.Slice(BpbTotSec32Offset, BpbDwordBytes));
        }

        uint rootDirSectors = (rootEntries * FatDirectory.EntrySize + bytesPerSector - 1) / bytesPerSector;
        uint dataStart = reserved + numFats * fatSize + rootDirSectors;
        uint clusterCount = (totalSectors - dataStart) / spc;

        // Entry density per family, derived from the cluster count the
        // way fatgen103 §3.5 resolves the type.
        uint entriesPerSector = clusterCount < Fat16MinClusterCount
            ? bytesPerSector * Fat12EntriesPerPair / Fat12PairBytes
            : (clusterCount < Fat32MinClusterCount ? bytesPerSector / Fat16EntrySize : bytesPerSector / Fat32EntrySize);
        Assert.True(fatSize * entriesPerSector >= clusterCount + ReservedFatEntries,
            "one FAT copy must cover every data cluster plus the two reserved entries");
    }

    private static byte[] MakePayload(int size, byte salt)
    {
        byte[] buffer = new byte[size];
        for (int i = 0; i < size; i++)
        {
            // Mix higher position bits in so the pattern does not repeat
            // with period 256: byte-identical clusters would hide chain
            // walks that reorder, repeat or swap clusters.
            buffer[i] = (byte)(i ^ (i >> BitsPerByte) ^ (i >> (BitsPerByte * 2)) ^ salt);
        }
        return buffer;
    }

    private static void AssertBytesEqual(byte[] expected, byte[] actual)
    {
        Assert.Equal<int>(expected.Length, actual.Length);
        for (int i = 0; i < expected.Length; i++)
        {
            Assert.Equal<byte>(expected[i], actual[i]);
        }
    }

    private static void WriteAll(IVfsInode inode, byte[] data)
    {
        IFileOperations ops = inode.FileOperations!;
        IVfsOpenFile h = new TestOpenFile(inode, ops);
        long written = ops.Write(h, data);
        Assert.Equal<long>(data.Length, written);
        ops.Release(h);
    }

    private static byte[] ReadAll(IVfsInode inode, int length)
    {
        IFileOperations ops = inode.FileOperations!;
        IVfsOpenFile h = new TestOpenFile(inode, ops);
        byte[] buf = new byte[length];
        long n = ops.Read(h, buf);
        Assert.Equal<long>(length, n);
        ops.Release(h);
        return buf;
    }

    private static string IntToHex(int value, int width)
    {
        Span<char> chars = stackalloc char[width];
        for (int i = width - 1; i >= 0; i--)
        {
            int nibble = value & NibbleMask;
            chars[i] = (char)(nibble < DecimalBase ? '0' + nibble : 'A' + (nibble - DecimalBase));
            value >>= BitsPerNibble;
        }
        return new string(chars);
    }

    private sealed class TestOpenFile : IVfsOpenFile
    {
        public TestOpenFile(IVfsInode inode, IFileOperations ops)
        {
            Inode = inode;
            Operations = ops;
        }

        public IVfsInode Inode { get; }
        public IFileOperations Operations { get; }
        public long Position { get; set; }
    }
}
