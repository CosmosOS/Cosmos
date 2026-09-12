using System;
using Cosmos.Kernel.Boot.Limine;
using Cosmos.Kernel.Core;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.HAL;
using Cosmos.Kernel.HAL.Devices.Storage;
using Cosmos.Kernel.HAL.Interfaces.Devices;
using Cosmos.Kernel.HAL.Pci;
using Cosmos.Kernel.HAL.Pci.Enums;
using Cosmos.Kernel.HAL.Vfs;
using Cosmos.Kernel.System.Filesystems.Fat;
using Cosmos.Kernel.System.Storage;
using Cosmos.Kernel.System.Vfs;
using Cosmos.TestRunner.Framework;
using Sys = Cosmos.Kernel.System;
using TR = Cosmos.TestRunner.Framework.TestRunner;

namespace Cosmos.Kernel.Tests.Storage;

public class Kernel : Sys.Kernel
{
    // The single block device the active QEMU profile attached, captured
    // once at BeforeRun. The profile name (set by the engine — ahci,
    // ahci+gicv2, nvme, nvme+gicv3, nvme+acpi-off, ...) is what the report's
    // [profile] prefix shows; this field just holds whatever device bound.
    private static IBlockDevice? s_dev;

    // Reason surfaced through TR.RunIf when a test depends on a device
    // having actually bound. A profile whose driver did not enumerate
    // (e.g. nvme+acpi-off on arm64, where no-ACPI removes PCIe discovery)
    // lands here and the device tests skip.
    private const string SkipNoDevice = "no block device bound for this profile";

    // Same gating reason for the partition-table tests, which also need
    // a backing host device.
    private const string SkipNoHost = "no block device bound for partition-table tests";

    /// <summary>Total tests this suite reports per profile; the breakdown is at the TR.Start call site.</summary>
    private const ushort ExpectedTestCount = 68;

    /// <summary>Block devices the engine attaches per QEMU profile; any other count is a bind or double-registration regression.</summary>
    private const int AttachedDisksPerProfile = 1;

    /// <summary>Logical block size every disk in these profiles exposes (bytes).</summary>
    private const ulong SectorSizeBytes = 512;

    /// <summary>1 MiB-aligned start LBA used for the first GPT data partition, safely past the header (LBA 1) and entry array.</summary>
    private const ulong GptAlignedStartLba = 2048;

    /// <summary>Sector count of the partition stamped by the destructive reboot cell for boot 1's scan to find.</summary>
    private const ulong RebootPartitionSectorCount = 4096;

    /// <summary>LBA of the first sector of the primary GPT partition-entry array (UEFI spec 5.3: header at LBA 1, entries from LBA 2).</summary>
    private const ulong GptEntryArrayLba = 2;

    /// <summary>Size of a GUID field in a GPT partition entry (bytes, UEFI spec 5.3.3).</summary>
    private const int GptGuidSizeBytes = 16;

    /// <summary>Byte offset of the partition-type GUID within a GPT partition entry (UEFI spec 5.3.3).</summary>
    private const int GptEntryTypeGuidOffset = 0;

    /// <summary>Byte offset of the unique-partition GUID within a GPT partition entry (UEFI spec 5.3.3).</summary>
    private const int GptEntryUniqueGuidOffset = 16;

    /// <summary>Byte offset of the 64-bit starting-LBA field within a GPT partition entry (UEFI spec 5.3.3).</summary>
    private const int GptEntryStartLbaOffset = 32;

    /// <summary>Byte offset of the 64-bit ending-LBA field within a GPT partition entry (UEFI spec 5.3.3).</summary>
    private const int GptEntryEndLbaOffset = 40;

    /// <summary>Size of an LBA field in a GPT partition entry (bytes, 64-bit).</summary>
    private const int GptLbaFieldBytes = 8;

    /// <summary>Byte offset of the UTF-16 partition name within a GPT entry.</summary>
    private const int GptEntryNameOffset = 56;

    /// <summary>Size in bytes of one GPT partition entry.</summary>
    private const int GptEntrySizeBytes = 128;

    /// <summary>Size of one UTF-16 code unit in the GPT entry name field (bytes).</summary>
    private const int Utf16BytesPerChar = 2;

    /// <summary>Number of UTF-16 characters stamped into the GPT entry name field by the remove-clears cell.</summary>
    private const int GptNameStampChars = 8;

    /// <summary>Base UTF-16 code unit stamped into the GPT entry name field ('A'); successive characters add the loop index.</summary>
    private const char GptNameStampBaseChar = 'A';

    /// <summary>Non-zero byte planted in the unique-GUID field so the raw-crafted GPT entry is not all-zero.</summary>
    private const byte BogusUniqueGuidByte = 0x42;

    /// <summary>Raw-crafted start LBA that lands inside the GPT entry array (LBA 2..33), which Parse must reject.</summary>
    private const ulong GptOverlapStartLba = 10;

    /// <summary>Ending LBA paired with <see cref="GptOverlapStartLba"/> for the raw-crafted overlapping entry.</summary>
    private const ulong GptOverlapEndLba = 100;

    /// <summary>Sector count of the past-end GPT entry that AddPartition must reject.</summary>
    private const ulong GptBogusPastEndSectorCount = 16;

    /// <summary>Start-LBA delta from the first to the second GPT partition in the mutate-skips cell (sectors).</summary>
    private const ulong GptSecondPartitionDeltaSectors = 4096;

    /// <summary>Length of each partition in the GPT writer overlap test.</summary>
    private const ulong GptOverlapPartitionSectorCount = 1000;

    /// <summary>Distance from the first partition to the second in the GPT writer overlap test.</summary>
    private const ulong GptOverlapNeighbourOffsetSectors = 8000;

    /// <summary>Sectors past the device end where the raw-corrupted GPT entry's start LBA lands.</summary>
    private const ulong GptCorruptStartOvershootSectors = 5;

    /// <summary>Sectors past the device end where the raw-corrupted GPT entry's inclusive ending LBA lands.</summary>
    private const ulong GptCorruptEndOvershootSectors = 14;

    /// <summary>LBA of the MBR / boot sector (the disk's first sector in the classic PC layout).</summary>
    private const ulong MbrLba = 0;

    /// <summary>Byte offset of MBR partition entry 0 in the boot sector.</summary>
    private const int MbrEntry0Offset = 446;

    /// <summary>Byte offset of MBR partition entry 1 in the boot sector (446 + one 16-byte entry).</summary>
    private const int MbrEntry1Offset = 462;

    /// <summary>Byte offset of the system-ID (type) byte within an MBR partition entry.</summary>
    private const int MbrEntryTypeOffset = 4;

    /// <summary>Byte offset of the 32-bit start-LBA field within an MBR partition entry.</summary>
    private const int MbrEntryStartLbaOffset = 8;

    /// <summary>Byte offset of the 32-bit sector-count field within an MBR partition entry.</summary>
    private const int MbrEntrySectorCountOffset = 12;

    /// <summary>Size of an LBA/count field in an MBR partition entry (bytes, 32-bit).</summary>
    private const int MbrLbaFieldBytes = 4;

    /// <summary>Bit width of the on-disk MBR/EBR sector-count field; 1UL shifted by this is the first unrepresentable count.</summary>
    private const int MbrSectorCountFieldBits = 32;

    /// <summary>MBR system ID for a native Linux partition.</summary>
    private const byte MbrLinuxSystemId = 0x83;

    /// <summary>MBR system ID for a W95 FAT32 (LBA-unaware) partition.</summary>
    private const byte MbrFat32SystemId = 0x0B;

    /// <summary>Start sector of the first MBR round-trip partition window.</summary>
    private const uint MbrPartAStartSector = 100;

    /// <summary>Sector count of the first MBR round-trip partition window.</summary>
    private const uint MbrPartASectorCount = 200;

    /// <summary>Start sector of the second MBR round-trip partition window.</summary>
    private const uint MbrPartBStartSector = 1000;

    /// <summary>Sector count of the second MBR round-trip partition window.</summary>
    private const uint MbrPartBSectorCount = 500;

    /// <summary>Start LBA of 0, aliasing the owning table sector itself (MBR or EBR); writers and parsers must reject it.</summary>
    private const uint SelfAliasingStartLba = 0;

    /// <summary>Sector count of the bogus start-0 MBR entry (its start aliases the MBR sector itself).</summary>
    private const uint MbrBogusStartZeroSectorCount = 200;

    /// <summary>Sector count of the bogus past-end MBR entry.</summary>
    private const uint MbrBogusPastEndSectorCount = 100;

    /// <summary>How far before the device end the bogus past-end entry starts, so start + count overruns the disk.</summary>
    private const ulong PastEndBacktrackSectors = 10;

    /// <summary>Sectors past the device end where the corrupt EBR next pointer lands, so the chain walk must stop rather than read it.</summary>
    private const ulong WildNextOvershootSectors = 10;

    /// <summary>NSID of the controller's single namespace (NVMe namespace IDs are 1-based).</summary>
    private const uint NvmeNamespaceId = 1;

    /// <summary>NVMe 0's-based Number of Logical Blocks value for a one-block transfer (NVMe spec: NLB is zero-based).</summary>
    private const ushort NvmeSingleBlockNlb = 0;

    /// <summary>Scratch LBA the NVMe short-span cell writes, clear of every other test window.</summary>
    private const ulong ShortSpanLba = 4242;

    /// <summary>Fill byte of the full-block write whose residue must not leak into the short-span tail.</summary>
    private const byte ShortSpanResidueFill = 0xA5;

    /// <summary>Length of the deliberately short span handed to NvmeController.Write (bytes, less than one sector).</summary>
    private const int ShortSpanLengthBytes = 100;

    /// <summary>Fill byte of the short-span payload.</summary>
    private const byte ShortSpanFill = 0x5B;

    /// <summary>XOR seed decorrelating the single-block round-trip pattern from a plain index ramp.</summary>
    private const byte SingleBlockXorSeed = 0xA5;

    /// <summary>Per-byte multiplier of the multi-block round-trip pattern.</summary>
    private const int MultiBlockByteStep = 7;

    /// <summary>XOR seed of the multi-block round-trip pattern.</summary>
    private const byte MultiBlockXorSeed = 0x3C;

    /// <summary>Fill byte of the first (overwritten) idempotency write.</summary>
    private const byte IdempotentFirstFill = 0x11;

    /// <summary>Fill byte of the second idempotency write, which is what must read back.</summary>
    private const byte IdempotentSecondFill = 0x22;

    /// <summary>Shift folding the high index byte into the large-transfer pattern so 256-byte periods differ.</summary>
    private const int LargeTransferCarryShift = 8;

    /// <summary>XOR seed of the large-transfer pattern.</summary>
    private const byte LargeTransferXorSeed = 0x5A;

    /// <summary>XOR seed of the boundary-LBA round-trip pattern.</summary>
    private const byte BoundaryXorSeed = 0xF0;

    /// <summary>XOR seed of the LBA-0 round-trip pattern.</summary>
    private const byte LbaZeroXorSeed = 0x96;

    /// <summary>Marker base ORed with the block index for the cross-block isolation fill.</summary>
    private const byte CrossBlockMarkerBase = 0xC0;

    /// <summary>Per-slot multiplier of the stride-sweep tag.</summary>
    private const int StrideTagStep = 17;

    /// <summary>XOR seed of the stride-sweep tag.</summary>
    private const byte StrideTagXorSeed = 0x5A;

    /// <summary>Fill byte of the first random-order block.</summary>
    private const byte RandomOrderFillA = 0xAA;

    /// <summary>Fill byte of the second random-order block.</summary>
    private const byte RandomOrderFillB = 0xBB;

    /// <summary>Fill byte of the third random-order block.</summary>
    private const byte RandomOrderFillC = 0xCC;

    /// <summary>Per-byte multiplier of the tail-boundary pattern.</summary>
    private const int TailBoundaryByteStep = 13;

    /// <summary>XOR seed of the tail-boundary pattern.</summary>
    private const byte TailBoundaryXorSeed = 0x6E;

    /// <summary>XOR seed of the partition LBA-translation pattern.</summary>
    private const byte TranslateXorSeed = 0x42;

    /// <summary>Start sector of the small partition used by the out-of-bounds probe.</summary>
    private const ulong TinyPartStartSector = 5000;

    /// <summary>Sector count of the small partition; reading block <see cref="TinyPartSectorCount"/> is the first out-of-range access.</summary>
    private const ulong TinyPartSectorCount = 8;

    /// <summary>Sector count of the in-memory partition used by the bounds-overflow probe.</summary>
    private const ulong OverflowProbeSectorCount = 4;

    /// <summary>Smallest possible partition (one sector), requested from the degenerate 1-block device that cannot host it.</summary>
    private const ulong MinimalPartitionSectorCount = 1;

    /// <summary>Offset into the existing partition where the refused nested create starts (sectors).</summary>
    private const uint NestedCreateOffsetSectors = 10;

    /// <summary>Sector count of the refused nested create inside an existing partition.</summary>
    private const ulong NestedCreateSectorCount = 10;

    /// <summary>Sector count of the refused create at LBA 0.</summary>
    private const ulong LbaZeroCreateSectorCount = 100;

    /// <summary>MBR system ID of a CHS-addressed extended (EBR container) partition.</summary>
    private const byte MbrExtendedSystemId = 0x05;

    /// <summary>System ID 0xEE - GPT protective MBR entry.</summary>
    private const byte MbrGptProtectiveSystemId = 0xEE;

    /// <summary>Sector count stamped into the crafted 0xEE protective entry.</summary>
    private const uint MbrProtectiveSectorCount = 1000;

    /// <summary>New sector count of the refused resize aimed at the protective entry.</summary>
    private const uint ProtectiveResizeSectorCount = 128;

    /// <summary>Byte offset of the 0xAA55 boot signature within an MBR/EBR sector.</summary>
    private const int MbrBootSigOffset = 510;

    /// <summary>Size in bytes of the boot signature field.</summary>
    private const int MbrBootSigSizeBytes = 2;

    /// <summary>Boot signature value stamped at the end of hand-crafted EBR sectors.</summary>
    private const ushort MbrBootSignature = 0xAA55;

    /// <summary>First (low) byte of the little-endian 0xAA55 boot signature.</summary>
    private const byte MbrBootSigLowByte = 0x55;

    /// <summary>Second (high) byte of the little-endian 0xAA55 boot signature.</summary>
    private const byte MbrBootSigHighByte = 0xAA;

    /// <summary>Absolute start LBA of the extended partition the EBR / PartitionManager lifecycle cells stamp.</summary>
    private const uint ExtPartStartSector = 4000;

    /// <summary>Sector count of the extended partition the EBR / PartitionManager lifecycle cells stamp.</summary>
    private const uint ExtPartSectorCount = 12000;

    /// <summary>Gap between an EBR sector and its logical's first data sector: Ebr.TryAddLogical places the data in the very next sector.</summary>
    private const uint EbrLogicalDataOffsetSectors = 1;

    /// <summary>Sector delta of the refused attempt to move the extended container.</summary>
    private const uint ExtendedMoveProbeDeltaSectors = 64;

    /// <summary>Length of each of the two adjacent GPT partitions the resize-overlap cell stamps.</summary>
    private const ulong ResizeOverlapSectorCount = 4096;

    /// <summary>Free sectors left between the extended container and the destination of the refused container move.</summary>
    private const uint ContainerMoveGapSectors = 100;

    /// <summary>Fill seed of the witness sector that proves a refused move copied nothing.</summary>
    private const uint ContainerMoveWitnessSeed = 0xB0A70000;

    /// <summary>Length of the partition the refused table-sector move cells relocate.</summary>
    private const ulong TableMoveSectorCount = 64;

    /// <summary>A destination inside the GPT entry array (below Gpt.FirstUsableLba), which no partition may occupy.</summary>
    private const ulong GptReservedDestinationLba = 2;

    /// <summary>Sectors wiped ahead of the superfloppy format: the MBR sector (LBA 0) and the primary GPT header (LBA 1), the two signatures the partition scanner probes.</summary>
    private const ulong SuperfloppyWipeHeadSectors = 2;

    /// <summary>VFS driver name the superfloppy cell registers its FAT driver under.</summary>
    private const string SuperfloppyDriverName = "fat-superfloppy";

    /// <summary>Mount point of the superfloppy volume.</summary>
    private const string SuperfloppyMountPoint = "/superfloppy";

    /// <summary>Name of the superfloppy round-trip file (8.3 so the cell does not also depend on LFN handling).</summary>
    private const string SuperfloppyFileName = "SFD410.TXT";

    /// <summary>VFS path of the superfloppy round-trip file.</summary>
    private const string SuperfloppyFilePath = SuperfloppyMountPoint + "/" + SuperfloppyFileName;

    /// <summary>Payload bytes of the superfloppy round-trip file (two sectors).</summary>
    private const int SuperfloppyPayloadBytes = 1024;

    /// <summary>XOR seed of the superfloppy round-trip payload.</summary>
    private const byte SuperfloppyXorSeed = 0x77;

    /// <summary>Forward delta of the deliberately legal MovePartition calls (post-adjacency and signature-restamp cells).</summary>
    private const uint LegalMoveDeltaSectors = 100;

    /// <summary>Per-byte multiplier of the FillPattern move-payload pattern.</summary>
    private const uint FillPatternByteStep = 31;

    /// <summary>Low-byte mask folding the 32-bit pattern accumulator into a byte.</summary>
    private const uint ByteMask = 0xFF;

    protected override void BeforeRun()
    {
        Serial.WriteString("[Storage] BeforeRun() reached!\n");

        // 3 manager + 1 boot-scan + 2 profile + 13 device + 7 partition
        // + 37 partition-lifecycle (MBR mutation, EBR chain, PartitionManager,
        // superfloppy) + 2 bounds probes + 2 mmio/pci + 1 boot-reboot
        // = 68 tests per profile.
        TR.Start("Storage Block Device Tests", expectedTests: ExpectedTestCount);

        bool hasDevice = StorageManager.DeviceCount > 0;
        s_dev = hasDevice ? StorageManager.GetDevice(0) : null;

        // ==================== Manager ====================
        TR.Run("Manager_StorageInitialized", TestManager_StorageInitialized);
        // A cell that attached a disk must SEE a disk: on x64 PCI enumerates
        // with or without ACPI, so zero devices is always a bind regression
        // and must fail, not skip; on arm64, acpi-off removes PCIe discovery,
        // so only those cells may legitimately come up empty.
#if ARCH_X64
        bool deviceExpected = true;
#else
        bool deviceExpected = !TR.ProfileContains("acpi-off");
#endif
        TR.RunIf(deviceExpected, "Manager_ExactlyOneDevice", TestManager_ExactlyOneDevice, SkipNoDevice);
        TR.RunIf(hasDevice, "Manager_DuplicateRegistrationIgnored", TestManager_DuplicateRegistrationIgnored, SkipNoDevice);

        // ==================== Boot persistence (works with Boot_RebootAfterGptWrite) ====================
        // Runs before anything mutates partition state: on boot 0 the engine's
        // fresh image must scan clean; on boot 1 the boot-time (phase-3) scan
        // must have found the GPT written before the reboot. Pins the init-window
        // partition scan, which regressed once without CI noticing (interpolated
        // partition names triple-faulted phase 3 on any populated disk).
        TR.RunIf(hasDevice, "Boot_PartitionScanMatchesBootState", TestBoot_PartitionScanMatchesBootState, SkipNoDevice);

        // ==================== Profile (assert the cell's hardware path) ====================
        // Prove the cell exercised the hardware it names, not just that block
        // I/O happened to work — the gap that let a silent MSI-X->polled
        // regression pass before.
        TR.RunIf(hasDevice, "Profile_DeviceKindMatches", TestProfile_DeviceKindMatches, SkipNoDevice);

        if (!TR.ProfileHasPrefix("nvme"))
        {
            TR.Skip("Profile_NvmeInterruptModeMatches", "not an NVMe profile");
        }
        else if (Nvme.Controllers.Count == 0)
        {
            TR.Skip("Profile_NvmeInterruptModeMatches", SkipNoDevice);
        }
        else if (TR.ProfileContains("gicv2") || TR.ProfileContains("gicv3"))
        {
            // Only the GIC-version cells pin a determinate NVMe interrupt path:
            // gicv3 brings up the ITS so MSI-X can route; gicv2 has no ITS so
            // the driver must fall back to polled. The expectation flag is
            // "expect MSI-X" == this is the gicv3 cell.
            TR.RunWithExpectation(TR.ProfileContains("gicv3"), "Profile_NvmeInterruptModeMatches", TestProfile_NvmeInterruptMode);
        }
        else
        {
#if ARCH_X64
            if (!TR.ProfileContains("acpi-off"))
            {
                // Plain x64 nvme: ACPI is on and the LAPIC MSI binder is
                // always registered (the Interrupts suite asserts
                // MsiRouting.IsAvailable unconditionally on x64), so the
                // driver landing in MSI-X mode IS determinate — a silent
                // MSI-X→polled regression here is exactly the failure this
                // cell exists to catch. expect-MSI-X = true.
                TR.RunWithExpectation(true, "Profile_NvmeInterruptModeMatches", TestProfile_NvmeInterruptMode);
            }
            else
            {
                // acpi-off x64: no MADT → no LAPIC MSI routing to pin.
                TR.Skip("Profile_NvmeInterruptModeMatches", "acpi-off has no MSI routing to pin");
            }
#else
            // arm64 bare nvme: the interrupt mode depends on the machine's
            // default gic-version, so it is not pinned here — the
            // gicv2/gicv3 cells assert both paths explicitly.
            TR.Skip("Profile_NvmeInterruptModeMatches", "interrupt mode not pinned by this cell");
#endif
        }

        // ==================== Device (single-disk round-trip) ====================
        bool dev = s_dev != null;
        TR.RunIf(dev, "Device_BlockGeometry_Sane",         TestDevice_BlockGeometrySane,        SkipNoDevice);
        TR.RunIf(dev, "Device_WriteRead_SingleBlock",      TestDevice_WriteReadSingleBlock,     SkipNoDevice);
        TR.RunIf(dev, "Device_WriteRead_MultiBlock",       TestDevice_WriteReadMultiBlock,      SkipNoDevice);
        TR.RunIf(dev, "Device_WriteRead_Idempotent",       TestDevice_WriteReadIdempotent,      SkipNoDevice);
        TR.RunIf(dev, "Device_ReReadStable",               TestDevice_ReReadStable,             SkipNoDevice);
        TR.RunIf(dev, "Device_LargeTransfer",              TestDevice_LargeTransfer,            SkipNoDevice);
        TR.RunIf(dev, "Device_BoundaryLBA",                TestDevice_BoundaryLBA,              SkipNoDevice);
        TR.RunIf(dev, "Device_LBA_Zero_RoundTrip",         TestDevice_LBAZeroRoundTrip,         SkipNoDevice);
        TR.RunIf(dev, "Device_CrossBlock_Isolation",       TestDevice_CrossBlockIsolation,      SkipNoDevice);
        TR.RunIf(dev, "Device_LBA_Stride_Sweep",           TestDevice_LBAStrideSweep,           SkipNoDevice);
        TR.RunIf(dev, "Device_RandomOrder_ReadAfterWrite", TestDevice_RandomOrderReadAfterWrite, SkipNoDevice);
        TR.RunIf(dev, "Device_Multiblock_TailBoundary",    TestDevice_MultiblockTailBoundary,   SkipNoDevice);
        TR.RunIf(dev && TR.ProfileHasPrefix("nvme"), "Nvme_ShortSpanWritesDeterministicTail", TestNvme_ShortSpanTail, "NVMe controller API is nvme-profile only");

        // ==================== Partition (MBR/GPT, partition translation) ====================
        // These run last because they overwrite LBA 0..33, which the device
        // round-trip tests above also touch — ordering them last keeps the
        // earlier results from being affected by the partition-table writes.
        TR.RunIf(dev, "Partition_MBR_RoundTrip",          TestPartition_MBRRoundTrip,          SkipNoHost);
        TR.RunIf(dev, "Partition_GPT_RoundTrip",          TestPartition_GPTRoundTrip,          SkipNoHost);
        TR.RunIf(dev, "Partition_RescanPartitions",       TestPartition_RescanPartitions,      SkipNoHost);
        TR.RunIf(dev, "Partition_ReadWrite_TranslatesLba", TestPartition_ReadWriteTranslatesLba, SkipNoHost);
        TR.RunIf(dev, "Partition_OutOfBounds_Throws",      TestPartition_OutOfBoundsThrows,     SkipNoHost);
        TR.RunIf(dev, "Partition_MbrParseRejectsBogus",    TestPartition_MbrParseRejectsBogus,  SkipNoHost);
        TR.RunIf(dev, "Partition_GptAddRejectsBogus",      TestPartition_GptAddRejectsBogus,    SkipNoHost);

        // ==================== Partition lifecycle (MBR mutation, EBR chain, PartitionManager) ====================
        // Each cell wipes and re-stamps its own layout (LBA 0..~16000), so
        // like the tests above they must stay behind the device round-trip
        // section. The final reboot cell stamps a fresh GPT afterwards, so
        // whatever layout the last of these leaves behind is irrelevant.
        TR.RunIf(dev, "Partition_MBR_Resize",              TestPartition_MbrResize,             SkipNoHost);
        TR.RunIf(dev, "Partition_MBR_Delete",              TestPartition_MbrDelete,             SkipNoHost);
        TR.RunIf(dev, "Partition_EBR_ChainDiscoversLogicals", TestEbr_ChainDiscoversLogicals,   SkipNoHost);
        TR.RunIf(dev, "Partition_RescanPartitions_WithEBR", TestPartition_RescanWithEbr,        SkipNoHost);
        TR.RunIf(dev, "PartitionManager_Create_OnMBR",     TestPartitionManager_CreateOnMbr,    SkipNoHost);
        TR.RunIf(dev, "PartitionManager_Resize_OnMBR",     TestPartitionManager_ResizeOnMbr,    SkipNoHost);
        TR.RunIf(dev, "PartitionManager_Delete_OnMBR",     TestPartitionManager_DeleteOnMbr,    SkipNoHost);
        TR.RunIf(dev, "PartitionManager_MoveWithData_NonOverlapping", TestPartitionManager_MoveNonOverlapping, SkipNoHost);
        TR.RunIf(dev, "PartitionManager_MoveWithData_OverlappingForward", TestPartitionManager_MoveOverlappingForward, SkipNoHost);
        TR.RunIf(dev, "PartitionManager_Resize_OnGPT",     TestPartitionManager_ResizeOnGpt,    SkipNoHost);
        TR.RunIf(dev, "PartitionManager_Delete_OnGPT",     TestPartitionManager_DeleteOnGpt,    SkipNoHost);
        TR.RunIf(dev, "PartitionManager_MoveWithData_OnGPT", TestPartitionManager_MoveOnGpt,    SkipNoHost);
        TR.RunIf(dev, "EBR_AddLogical_FirstAndSecond",     TestEbr_AddLogicalFirstAndSecond,    SkipNoHost);
        TR.RunIf(dev, "EBR_RemoveLogical_Last",            TestEbr_RemoveLogicalLast,           SkipNoHost);
        TR.RunIf(dev, "EBR_RemoveLogical_First_PromotesSuccessor", TestEbr_RemoveLogicalFirstPromotesSuccessor, SkipNoHost);
        TR.RunIf(dev, "EBR_RemoveLogical_Middle_CollapsesChain", TestEbr_RemoveLogicalMiddleCollapsesChain, SkipNoHost);
        TR.RunIf(dev, "EBR_RemoveLogical_OnlyOne_ClearsChain", TestEbr_RemoveLogicalOnlyOneClearsChain, SkipNoHost);
        TR.RunIf(dev, "EBR_ResizeLogical",                 TestEbr_ResizeLogical,               SkipNoHost);
        TR.RunIf(dev, "EBR_MoveLogical_TableLevel",        TestEbr_MoveLogicalTableLevel,       SkipNoHost);
        TR.RunIf(dev, "EBR_Parse_SkipsCorruptEntries",     TestEbr_ParseSkipsCorruptEntries,    SkipNoHost);
        TR.RunIf(dev, "EBR_Parse_StopsOnWildNextPointer",  TestEbr_ParseStopsOnWildNextPointer, SkipNoHost);
        TR.RunIf(dev, "EBR_AddLogical_RejectsBogusGeometry", TestEbr_AddLogicalRejectsBogusGeometry, SkipNoHost);
        TR.RunIf(dev, "EBR_ResizeLogical_RespectsEnvelopeBounds", TestEbr_ResizeLogicalRespectsEnvelopeBounds, SkipNoHost);
        TR.RunIf(dev, "MBR_TryGetExtended_RejectsBogusGeometry", TestMbr_TryGetExtendedRejectsBogusGeometry, SkipNoHost);
        TR.RunIf(dev, "MBR_ResizeMove_RejectsExtendedSlot", TestMbr_ResizeMoveRejectsExtendedSlot, SkipNoHost);
        TR.RunIf(dev, "MBR_ResizeMove_RejectsOverlap",     TestMbr_ResizeMoveRejectsOverlap,     SkipNoHost);
        TR.RunIf(dev, "GPT_ResizeMove_RejectsOverlap",     TestGpt_ResizeMoveRejectsOverlap,     SkipNoHost);
        TR.RunIf(dev, "MBR_ResizeMove_RestampsSignature",  TestMbr_ResizeMoveRestampsSignature,  SkipNoHost);
        TR.RunIf(dev, "GPT_Mutate_SkipsEntriesParseRejects", TestGpt_MutateSkipsEntriesParseRejects, SkipNoHost);
        TR.RunIf(dev, "GPT_Remove_ClearsWholeEntry",       TestGpt_RemoveClearsWholeEntry,       SkipNoHost);
        TR.RunIf(dev, "PartitionManager_MoveFailure_IsNonDestructive", TestPartitionManager_MoveFailureIsNonDestructive, SkipNoHost);
        TR.RunIf(dev, "PartitionManager_RejectsOccupiedRanges", TestPartitionManager_RejectsOccupiedRanges, SkipNoHost);
        TR.RunIf(dev, "PartitionManager_GuardsDoNotWrap",  TestPartitionManager_GuardsDoNotWrap, SkipNoHost);
        TR.RunIf(dev, "PartitionManager_Resize_RefusesOverlap", TestPartitionManager_ResizeRefusesOverlap, SkipNoHost);
        TR.RunIf(dev, "PartitionManager_Move_RefusesExtendedContainer", TestPartitionManager_MoveRefusesExtendedContainer, SkipNoHost);
        TR.RunIf(dev, "PartitionManager_Move_RefusesTableSectors", TestPartitionManager_MoveRefusesTableSectors, SkipNoHost);
        TR.RunIf(dev, "PartitionManager_CreateLogical",    TestPartitionManager_CreateLogical,  SkipNoHost);
        TR.RunIf(dev, "PartitionManager_Resize_OnLogical", TestPartitionManager_ResizeOnLogical, SkipNoHost);
        TR.RunIf(dev, "PartitionManager_Delete_OnLogical", TestPartitionManager_DeleteOnLogical, SkipNoHost);
        TR.RunIf(dev, "PartitionManager_MoveWithData_OnLogical", TestPartitionManager_MoveOnLogical, SkipNoHost);
        TR.RunIf(dev, "Partition_Superfloppy_MountsByIndex",   TestPartition_SuperfloppyMountsByIndex, SkipNoHost);

        // Overflow-safety of the bounds check is hardware-independent (in-memory
        // probe host), so it runs unconditionally, even on cells where no disk bound.
        TR.Run("Partition_BoundsOverflow_Throws", TestPartition_BoundsOverflowThrows);
        TR.Run("Partition_GptTinyDeviceSafe", TestPartition_GptTinyDeviceSafe);

        // ==================== MMIO mapping (64-bit BAR above 4 GiB) ====================
        // Runs after every cell that does block I/O: it relocates the live
        // NVMe controller's BAR while no transfer is in flight and restores
        // it before returning, so ordering it here keeps a mapper regression
        // (crash mid-cell) from also taking down the I/O results above.
#if ARCH_X64
        TR.RunIf(dev && TR.ProfileHasPrefix("nvme"), "Mmio_HighBar_RemappedOnDemand", TestMmio_HighBarRemapped,
            "64-bit BAR relocation probe is nvme-profile only");
        TR.RunIf(dev && TR.ProfileHasPrefix("nvme"), "Pci_GetBar64_ReadsLiveConfig", TestPciGetBar64ReadsLiveConfig,
            "64-bit BAR relocation probe is nvme-profile only");
#else
        TR.Skip("Mmio_HighBar_RemappedOnDemand", "x64 mapper cell; arm64 installs Device mappings via DeviceMapper");
        TR.Skip("Pci_GetBar64_ReadsLiveConfig", "BAR relocation probe is x64-only (same harness as the mapper cell)");
#endif

        // ==================== Boot persistence (destructive: reboots QEMU) ====================
        // Boot 0 stamps a fresh GPT with one partition and reboots; boot 1's
        // Boot_PartitionScanMatchesBootState (above) then proves the phase-3
        // boot-time scan survives a populated disk and names the partition.
        // Last on purpose: everything before it must have reported already.
        int skip = TR.GetSkipCount();
        if (!dev)
        {
            TR.Skip("Boot_RebootAfterGptWrite", SkipNoDevice);
        }
        else if (skip == 0)
        {
            TR.RunDestructive(
                "Boot_RebootAfterGptWrite",
                () =>
                {
                    Gpt.Create(s_dev!);
                    Assert.True(Gpt.AddPartition(s_dev!, GptAlignedStartLba, RebootPartitionSectorCount, Gpt.BasicDataPartitionType));
                    s_dev!.Flush();
                    Sys.Power.Reboot();
                },
                "Power.Reboot() returned without rebooting");
        }
        else
        {
            // Already fired in boot 0; replay as passed like the Power suite.
            TR.Run("Boot_RebootAfterGptWrite", () => { });
        }

        TR.Finish();

        Serial.WriteString("\n[Tests Complete - System Halting]\n");
    }

    protected override void Run() => Stop();

    protected override void AfterRun()
    {
        TR.Complete();
        Cosmos.Kernel.System.Power.Halt();
    }

    // Re-registering an already-known device must be a no-op: RegisterDevice
    // is public and unguarded (unlike Initialize), so a second
    // RegisterHalDevices call would otherwise double-count the device and
    // duplicate every partition under identical names.
    private static void TestManager_DuplicateRegistrationIgnored()
    {
        int before = StorageManager.DeviceCount;
        int partitionsBefore = StorageManager.Partitions.Count;
        StorageManager.RegisterDevice(s_dev!);
        Assert.Equal(before, StorageManager.DeviceCount, "duplicate registration must not add a device");
        Assert.Equal(partitionsBefore, StorageManager.Partitions.Count, "duplicate registration must not duplicate partitions");
    }

    // ==================== Boot persistence ====================

    // Boot 0 boots the engine's fresh blank image; boot 1 boots the GPT
    // written by Boot_RebootAfterGptWrite. Asserting against the boot-time
    // scan result (before any test mutates partition state) pins the
    // phase-3 partition scan path end to end, including partition naming.
    private static void TestBoot_PartitionScanMatchesBootState()
    {
        int partitionsOnDevice = 0;
        Partition? first = null;
        for (int i = 0; i < StorageManager.Partitions.Count; i++)
        {
            Partition p = StorageManager.Partitions[i];
            if (HasOrdinalPrefix(p.Name, s_dev!.Name))
            {
                first ??= p;
                partitionsOnDevice++;
            }
        }

        if (TR.GetSkipCount() == 0)
        {
            Assert.Equal(0, partitionsOnDevice, "blank disk must yield no boot-time partitions");
        }
        else
        {
            Assert.Equal(1, partitionsOnDevice, "boot-time scan must find the GPT partition written before reboot");
            Assert.Equal(s_dev!.Name + "p0", first!.Name, "boot-time partition name");
        }
    }

    // Regression guard: Mbr.Parse must not turn corrupt on-disk entries into
    // live partitions — start 0 aliases the MBR sector itself (formatting
    // that "partition" destroys the table) and past-end ranges authorize
    // wild host I/O. The GPT parser got this hardening; MBR must match.
    private static void TestPartition_MbrParseRejectsBogus()
    {
        Mbr.Create(s_dev!);

        // The writer must reject bogus ranges up front (same rules as the
        // parser): start 0 aliases the MBR, past-end authorizes wild I/O.
        Assert.True(MbrAddPartitionRejects(0, startSector: SelfAliasingStartLba, sectorCount: MbrBogusStartZeroSectorCount),
            "AddPartition must reject startSector 0");
        Assert.True(MbrAddPartitionRejects(1, startSector: (uint)(s_dev!.BlockCount - PastEndBacktrackSectors), sectorCount: MbrBogusPastEndSectorCount),
            "AddPartition must reject past-end ranges");

        // The parser is the trust boundary for on-disk corruption, so craft
        // the same bogus entries raw (bypassing the writer's validation).
        int sector = (int)s_dev!.BlockSize;
        byte[] mbr = new byte[sector];
        s_dev!.ReadBlock(MbrLba, 1, mbr);
        Span<byte> m = mbr;
        m[MbrEntry0Offset + MbrEntryTypeOffset] = MbrLinuxSystemId;
        BitConverter.TryWriteBytes(m.Slice(MbrEntry0Offset + MbrEntryStartLbaOffset, MbrLbaFieldBytes), SelfAliasingStartLba);
        BitConverter.TryWriteBytes(m.Slice(MbrEntry0Offset + MbrEntrySectorCountOffset, MbrLbaFieldBytes), MbrBogusStartZeroSectorCount);
        m[MbrEntry1Offset + MbrEntryTypeOffset] = MbrLinuxSystemId;
        BitConverter.TryWriteBytes(m.Slice(MbrEntry1Offset + MbrEntryStartLbaOffset, MbrLbaFieldBytes), (uint)(s_dev!.BlockCount - PastEndBacktrackSectors));
        BitConverter.TryWriteBytes(m.Slice(MbrEntry1Offset + MbrEntrySectorCountOffset, MbrLbaFieldBytes), MbrBogusPastEndSectorCount);
        s_dev!.WriteBlock(MbrLba, 1, mbr);

        List<MbrPartitionEntry> parts = Mbr.Parse(s_dev!);
        Assert.Equal(0, parts.Count, "corrupt MBR entries must be rejected by Parse");
    }

    // The writer reports refusal by return value now, so the try/catch this
    // used to need is gone with it — and so is the arm64 EH dispatch failure
    // that two inlined catch blocks alongside span locals once triggered.
    private static bool MbrAddPartitionRejects(int index, ulong startSector, ulong sectorCount)
    {
        return !Mbr.AddPartition(s_dev!, index, systemId: MbrLinuxSystemId, startSector: startSector, sectorCount: sectorCount);
    }

    // Same distrust for the GPT writer: AddPartition must reject entries its
    // own Parse would silently drop (zero-length) or that point past the disk,
    // instead of returning true for a partition that never materializes.
    private static void TestPartition_GptAddRejectsBogus()
    {
        Gpt.Create(s_dev!);
        Assert.False(Gpt.AddPartition(s_dev!, GptAlignedStartLba, 0, Gpt.BasicDataPartitionType), "zero-length entry must be rejected");
        Assert.False(Gpt.AddPartition(s_dev!, s_dev!.BlockCount, GptBogusPastEndSectorCount, Gpt.BasicDataPartitionType), "past-end entry must be rejected");
        Assert.Equal(0, Gpt.Parse(s_dev!).Count, "rejected entries must not appear on disk");

        // Raw-craft an entry starting INSIDE the GPT entry array (LBA 10):
        // a write through such a partition would corrupt the table itself,
        // so Parse must drop it (CRCs are 0 — corruption is undetectable).
        int sector = (int)s_dev!.BlockSize;
        byte[] entries = new byte[sector];
        Span<byte> e = entries;
        Gpt.BasicDataPartitionType.TryWriteBytes(e.Slice(GptEntryTypeGuidOffset, GptGuidSizeBytes));
        e[GptEntryUniqueGuidOffset] = BogusUniqueGuidByte; // non-zero unique GUID
        BitConverter.TryWriteBytes(e.Slice(GptEntryStartLbaOffset, GptLbaFieldBytes), GptOverlapStartLba);  // startLba inside the array
        BitConverter.TryWriteBytes(e.Slice(GptEntryEndLbaOffset, GptLbaFieldBytes), GptOverlapEndLba); // endLba
        s_dev!.WriteBlock(GptEntryArrayLba, 1, entries);
        Assert.Equal(0, Gpt.Parse(s_dev!).Count, "entry overlapping the GPT structures must be rejected");
    }

    // The NvmeController.Read/Write public API accepts spans shorter than
    // the device transfer; the bounce tail must then be deterministic
    // (zeroed), not the previous command's residue leaking to disk.
    private static void TestNvme_ShortSpanTail()
    {
        NvmeController controller = Nvme.Controllers[0];
        uint nsid = NvmeNamespaceId;
        ulong lba = ShortSpanLba;
        int sector = (int)s_dev!.BlockSize;

        byte[] full = new byte[sector];
        for (int i = 0; i < sector; i++)
        {
            full[i] = ShortSpanResidueFill;
        }
        controller.Write(nsid, lba, full, NvmeSingleBlockNlb);

        byte[] shortSpan = new byte[ShortSpanLengthBytes];
        for (int i = 0; i < shortSpan.Length; i++)
        {
            shortSpan[i] = ShortSpanFill;
        }
        controller.Write(nsid, lba, shortSpan, NvmeSingleBlockNlb);

        byte[] readBack = new byte[sector];
        controller.Read(nsid, lba, readBack, NvmeSingleBlockNlb);
        for (int i = 0; i < shortSpan.Length; i++)
        {
            Assert.Equal(ShortSpanFill, readBack[i], "short-span payload");
        }
        for (int i = shortSpan.Length; i < sector; i++)
        {
            Assert.Equal((byte)0, readBack[i], "tail must be zeroed, not stale bounce residue");
        }
    }

    // ==================== Manager ====================

    private static void TestManager_StorageInitialized()
    {
        Assert.True(StorageManager.IsEnabled);
        Assert.True(StorageManager.IsInitialized);
    }

    // The engine attaches exactly one disk per profile, so anything other
    // than 1 means either the profile is misconfigured or the driver
    // double-registered. Bound devices stay enumerated for the run.
    private static void TestManager_ExactlyOneDevice()
    {
        Assert.Equal(AttachedDisksPerProfile, StorageManager.DeviceCount);
    }

    // ==================== Profile ====================

    // The cell name encodes the controller it attached: ahci => sata*,
    // nvme-* => nvme*. Proves the driver that bound matches the cell's
    // intent. Device names are unique per instance ("sata0", "nvme0n1"),
    // so only the driver prefix is pinned here.
    private static void TestProfile_DeviceKindMatches()
    {
        string expected = TR.ProfileHasPrefix("ahci") ? "sata" : "nvme";
        Assert.True(HasOrdinalPrefix(s_dev!.Name, expected),
            "device name does not match the cell's controller kind");
    }

    // Hand-rolled ordinal prefix check, mirroring TR.ProfileHasPrefix: the
    // kernel runtime does not plug the culture-sensitive string.StartsWith.
    private static bool HasOrdinalPrefix(string value, string prefix)
    {
        if (value.Length < prefix.Length)
        {
            return false;
        }

        for (int i = 0; i < prefix.Length; i++)
        {
            if (value[i] != prefix[i])
            {
                return false;
            }
        }

        return true;
    }

    // A gicv3 cell must come up MSI-X (arm64 GICv3 ITS routes it); a gicv2 cell
    // must fall back to polled (no ITS). The bool is the cell's expected mode
    // (true = MSI-X), supplied by the adaptive RunIf overload.
    private static void TestProfile_NvmeInterruptMode(bool expectMsix)
    {
        bool actual = Nvme.Controllers[0].IsMsiXEnabled;
        if (expectMsix)
        {
            Assert.True(actual, "expected NVMe MSI-X interrupts but the controller is polled");
        }
        else
        {
            Assert.False(actual, "expected NVMe polled fallback but the controller enabled MSI-X");
        }
    }

    // ==================== Device ====================

    private static void TestDevice_BlockGeometrySane()
    {
        Assert.Equal<ulong>(SectorSizeBytes, s_dev!.BlockSize);
        Assert.True(s_dev.BlockCount > 0);
    }

    private static void TestDevice_WriteReadSingleBlock()
    {
        const ulong lba = 100;
        ulong size = s_dev!.BlockSize;

        Span<byte> writeBuf = new byte[size];
        for (int i = 0; i < (int)size; i++)
        {
            writeBuf[i] = (byte)(i ^ SingleBlockXorSeed);
        }
        s_dev.WriteBlock(lba, 1, writeBuf);

        Span<byte> readBuf = new byte[size];
        s_dev.ReadBlock(lba, 1, readBuf);

        for (int i = 0; i < (int)size; i++)
        {
            Assert.Equal(writeBuf[i], readBuf[i]);
        }
    }

    private static void TestDevice_WriteReadMultiBlock()
    {
        const ulong lba = 200;
        const ulong blocks = 4;
        ulong total = blocks * s_dev!.BlockSize;

        Span<byte> writeBuf = new byte[total];
        for (int i = 0; i < (int)total; i++)
        {
            writeBuf[i] = (byte)((i * MultiBlockByteStep) ^ MultiBlockXorSeed);
        }
        s_dev.WriteBlock(lba, blocks, writeBuf);

        Span<byte> readBuf = new byte[total];
        s_dev.ReadBlock(lba, blocks, readBuf);

        for (int i = 0; i < (int)total; i++)
        {
            Assert.Equal(writeBuf[i], readBuf[i]);
        }
    }

    private static void TestDevice_WriteReadIdempotent()
    {
        const ulong lba = 250;
        ulong size = s_dev!.BlockSize;

        Span<byte> first = new byte[size];
        first.Fill(IdempotentFirstFill);
        s_dev.WriteBlock(lba, 1, first);

        Span<byte> second = new byte[size];
        second.Fill(IdempotentSecondFill);
        s_dev.WriteBlock(lba, 1, second);

        Span<byte> readBuf = new byte[size];
        s_dev.ReadBlock(lba, 1, readBuf);
        for (int i = 0; i < (int)size; i++)
        {
            Assert.Equal(IdempotentSecondFill, readBuf[i]);
        }
    }

    private static void TestDevice_ReReadStable()
    {
        const ulong lba = 300;
        ulong size = s_dev!.BlockSize;

        Span<byte> first = new byte[size];
        s_dev.ReadBlock(lba, 1, first);

        Span<byte> second = new byte[size];
        s_dev.ReadBlock(lba, 1, second);

        for (int i = 0; i < (int)size; i++)
        {
            Assert.Equal(first[i], second[i]);
        }
    }

    private static void TestDevice_LargeTransfer()
    {
        const ulong lba = 1000;
        const ulong blocks = 32;
        ulong total = blocks * s_dev!.BlockSize;

        Span<byte> writeBuf = new byte[total];
        for (int i = 0; i < (int)total; i++)
        {
            writeBuf[i] = (byte)((i + (i >> LargeTransferCarryShift)) ^ LargeTransferXorSeed);
        }
        s_dev.WriteBlock(lba, blocks, writeBuf);

        Span<byte> readBuf = new byte[total];
        s_dev.ReadBlock(lba, blocks, readBuf);

        for (int i = 0; i < (int)total; i++)
        {
            Assert.Equal(writeBuf[i], readBuf[i]);
        }
    }

    private static void TestDevice_BoundaryLBA()
    {
        ulong lba = s_dev!.BlockCount - 1;
        ulong size = s_dev.BlockSize;

        Span<byte> writeBuf = new byte[size];
        for (int i = 0; i < (int)size; i++)
        {
            writeBuf[i] = (byte)(i ^ BoundaryXorSeed);
        }
        s_dev.WriteBlock(lba, 1, writeBuf);

        Span<byte> readBuf = new byte[size];
        s_dev.ReadBlock(lba, 1, readBuf);

        for (int i = 0; i < (int)size; i++)
        {
            Assert.Equal(writeBuf[i], readBuf[i]);
        }
    }

    // LBA 0 is often special (boot sector / protective MBR). Round-trip
    // must work there too.
    private static void TestDevice_LBAZeroRoundTrip()
    {
        ulong size = s_dev!.BlockSize;

        Span<byte> writeBuf = new byte[size];
        for (int i = 0; i < (int)size; i++)
        {
            writeBuf[i] = (byte)(i ^ LbaZeroXorSeed);
        }
        s_dev.WriteBlock(0, 1, writeBuf);

        Span<byte> readBuf = new byte[size];
        s_dev.ReadBlock(0, 1, readBuf);

        for (int i = 0; i < (int)size; i++)
        {
            Assert.Equal(writeBuf[i], readBuf[i]);
        }
    }

    // Catches bounce-buffer reuse / wrong-LBA-cached bugs: write 8 contiguous
    // blocks each filled with its own marker byte, then read each block back
    // individually and confirm the markers haven't bled across blocks.
    private static void TestDevice_CrossBlockIsolation()
    {
        const ulong baseLba = 4000;
        const int blocks = 8;
        ulong size = s_dev!.BlockSize;

        for (int b = 0; b < blocks; b++)
        {
            Span<byte> wbuf = new byte[size];
            wbuf.Fill((byte)(CrossBlockMarkerBase | b));
            s_dev.WriteBlock(baseLba + (ulong)b, 1, wbuf);
        }

        for (int b = 0; b < blocks; b++)
        {
            Span<byte> rbuf = new byte[size];
            s_dev.ReadBlock(baseLba + (ulong)b, 1, rbuf);
            byte expected = (byte)(CrossBlockMarkerBase | b);
            for (int i = 0; i < (int)size; i++)
            {
                Assert.Equal(expected, rbuf[i]);
            }
        }
    }

    // Catches LBA off-by-one / stride bugs: write to LBAs that are far apart
    // (n * 1024) so any wrap or shift error lands on a different block; the
    // pattern depends on n so we can detect a mis-routed write.
    private static void TestDevice_LBAStrideSweep()
    {
        const int slots = 8;
        const ulong stride = 1024;
        ulong size = s_dev!.BlockSize;

        for (int n = 0; n < slots; n++)
        {
            Span<byte> wbuf = new byte[size];
            byte tag = (byte)((n * StrideTagStep) ^ StrideTagXorSeed);
            wbuf.Fill(tag);
            s_dev.WriteBlock((ulong)n * stride, 1, wbuf);
        }

        for (int n = 0; n < slots; n++)
        {
            Span<byte> rbuf = new byte[size];
            s_dev.ReadBlock((ulong)n * stride, 1, rbuf);
            byte tag = (byte)((n * StrideTagStep) ^ StrideTagXorSeed);
            for (int i = 0; i < (int)size; i++)
            {
                Assert.Equal(tag, rbuf[i]);
            }
        }
    }

    // Reads in non-sequential order should still return the right data —
    // catches code that assumes the last-touched LBA is "current".
    private static void TestDevice_RandomOrderReadAfterWrite()
    {
        const ulong baseLba = 6000;
        ulong size = s_dev!.BlockSize;

        Span<byte> a = new byte[size]; a.Fill(RandomOrderFillA);
        Span<byte> b = new byte[size]; b.Fill(RandomOrderFillB);
        Span<byte> c = new byte[size]; c.Fill(RandomOrderFillC);
        s_dev.WriteBlock(baseLba + 0, 1, a);
        s_dev.WriteBlock(baseLba + 1, 1, b);
        s_dev.WriteBlock(baseLba + 2, 1, c);

        Span<byte> r = new byte[size];

        s_dev.ReadBlock(baseLba + 2, 1, r);
        for (int i = 0; i < (int)size; i++) { Assert.Equal(RandomOrderFillC, r[i]); }

        s_dev.ReadBlock(baseLba + 0, 1, r);
        for (int i = 0; i < (int)size; i++) { Assert.Equal(RandomOrderFillA, r[i]); }

        s_dev.ReadBlock(baseLba + 1, 1, r);
        for (int i = 0; i < (int)size; i++) { Assert.Equal(RandomOrderFillB, r[i]); }
    }

    // Multi-block transfer that ends exactly at the device tail. Catches
    // truncation / wrap bugs at the upper edge of LBA space.
    private static void TestDevice_MultiblockTailBoundary()
    {
        const ulong blocks = 4;
        ulong size = s_dev!.BlockSize;
        ulong lba = s_dev.BlockCount - blocks;
        ulong total = blocks * size;

        Span<byte> writeBuf = new byte[total];
        for (int i = 0; i < (int)total; i++)
        {
            writeBuf[i] = (byte)((i * TailBoundaryByteStep) ^ TailBoundaryXorSeed);
        }
        s_dev.WriteBlock(lba, blocks, writeBuf);

        Span<byte> readBuf = new byte[total];
        s_dev.ReadBlock(lba, blocks, readBuf);

        for (int i = 0; i < (int)total; i++)
        {
            Assert.Equal(writeBuf[i], readBuf[i]);
        }
    }

    // ==================== Partition ====================

    private static void TestPartition_MBRRoundTrip()
    {
        // Wipe LBA 0 first so a leftover GPT signature from a prior
        // sub-test doesn't taint the IsMbr check.
        Span<byte> wipe = new byte[s_dev!.BlockSize];
        s_dev.WriteBlock(MbrLba, 1, wipe);

        Mbr.Create(s_dev);
        Assert.True(Mbr.IsMbr(s_dev));

        // Two primary entries at distinct LBA windows.
        Assert.True(Mbr.AddPartition(s_dev, 0, systemId: MbrLinuxSystemId, startSector: MbrPartAStartSector, sectorCount: MbrPartASectorCount));
        Assert.True(Mbr.AddPartition(s_dev, 1, systemId: MbrFat32SystemId, startSector: MbrPartBStartSector, sectorCount: MbrPartBSectorCount));

        List<MbrPartitionEntry> parts = Mbr.Parse(s_dev);
        Assert.Equal(2, parts.Count);
        Assert.Equal<byte>(MbrLinuxSystemId, parts[0].SystemId);
        Assert.Equal<ulong>(MbrPartAStartSector, parts[0].StartSector);
        Assert.Equal<ulong>(MbrPartASectorCount, parts[0].SectorCount);
        Assert.Equal<byte>(MbrFat32SystemId, parts[1].SystemId);
        Assert.Equal<ulong>(MbrPartBStartSector, parts[1].StartSector);
        Assert.Equal<ulong>(MbrPartBSectorCount, parts[1].SectorCount);
    }

    private static void TestPartition_GPTRoundTrip()
    {
        Gpt.Create(s_dev!);
        Assert.True(Gpt.IsGpt(s_dev));

        const ulong countA = 4096;
        const ulong startB = GptAlignedStartLba + countA;
        const ulong countB = 8192;

        Assert.True(Gpt.AddPartition(s_dev, GptAlignedStartLba, countA, Gpt.BasicDataPartitionType));
        Assert.True(Gpt.AddPartition(s_dev, startB, countB, Gpt.BasicDataPartitionType));

        List<GptPartitionEntry> parts = Gpt.Parse(s_dev);
        Assert.Equal(2, parts.Count);
        Assert.Equal(Gpt.BasicDataPartitionType, parts[0].PartitionType);
        Assert.Equal<ulong>(GptAlignedStartLba, parts[0].StartSector);
        Assert.Equal<ulong>(countA, parts[0].SectorCount);
        Assert.Equal<ulong>(startB, parts[1].StartSector);
        Assert.Equal<ulong>(countB, parts[1].SectorCount);
    }

    // Layout left by Partition_GPTRoundTrip: GPT with two partitions on s_dev.
    // Run order matters — depends on the previous test having succeeded.
    private static void TestPartition_RescanPartitions()
    {
        StorageManager.RescanPartitions(s_dev!);

        int matches = 0;
        for (int i = 0; i < StorageManager.Partitions.Count; i++)
        {
            Partition p = StorageManager.Partitions[i];
            if (ReferenceEquals(p.Host, s_dev))
            {
                matches++;
            }
        }
        Assert.Equal(2, matches);
    }

    // Issue #410: a FAT volume laid straight onto an unpartitioned disk
    // (a "superfloppy" — what mkfs.vfat / Windows format produce on a raw
    // image attached via --disk) carries the MBR's 0xAA55 boot signature in
    // its BPB sector, so the scanner claimed the disk as an MBR with zero
    // partitions and surfaced nothing. VfsManager.TryMount/TryFormat by
    // partition index could then never reach the volume, despite the
    // Partitions contract covering "GPT-, MBR-, or unpartitioned" hosts.
    private static void TestPartition_SuperfloppyMountsByIndex()
    {
        // Kill the signatures earlier cells left behind (MBR at LBA 0, GPT
        // primary header at LBA 1), then format the raw device the way host
        // tools format an image file: filesystem at LBA 0, no partition table.
        Span<byte> zero = new byte[(int)s_dev!.BlockSize];
        for (ulong lba = 0; lba < SuperfloppyWipeHeadSectors; lba++)
        {
            s_dev.WriteBlock(lba, 1, zero);
        }
        FatFilesystemType rawDriver = new(s_dev);
        Assert.True(rawDriver.TryFormat(string.Empty, null), "FAT format of the raw device must succeed");

        StorageManager.RescanPartitions(s_dev);

        int index = -1;
        int matches = 0;
        for (int i = 0; i < StorageManager.Partitions.Count; i++)
        {
            if (ReferenceEquals(StorageManager.Partitions[i].Host, s_dev))
            {
                index = i;
                matches++;
            }
        }
        Assert.Equal(1, matches, "superfloppy disk must surface exactly one whole-disk partition");
        if (matches != 1)
        {
            // Asserts record failure without throwing; without the partition
            // every later step would dereference a missing entry and take
            // the whole suite down instead of failing this one cell.
            return;
        }
        Partition whole = StorageManager.Partitions[index];
        Assert.Equal<ulong>(0, whole.StartSector, "whole-disk partition must start at LBA 0");
        Assert.Equal<ulong>(s_dev.BlockCount, whole.BlockCount, "whole-disk partition must span the device");

        // The issue's exact call shape: mount by StorageManager partition index.
        Assert.True(VfsManager.RegisterFilesystem(SuperfloppyDriverName, new FatFilesystemType()));
        Assert.True(VfsManager.TryMount(SuperfloppyDriverName, index.ToString(), MountFlags.None, SuperfloppyMountPoint, out VfsManager.VfsMount? mount),
            "mount by partition index must succeed on a superfloppy volume");
        Assert.NotNull(mount);
        Assert.Equal<long>((long)SectorSizeBytes, mount!.Superblock.BlockSize);

        // Prove the mount is usable end to end: file round-trip through the VFS.
        Assert.True(VfsManager.TryOpenDirectory(SuperfloppyMountPoint, out IVfsDirectoryHandle? root));
        Assert.True(root!.TryCreateFile(SuperfloppyFileName, VfsMode.RegularFile, out _));

        byte[] payload = new byte[SuperfloppyPayloadBytes];
        for (int i = 0; i < payload.Length; i++)
        {
            payload[i] = (byte)(i ^ SuperfloppyXorSeed);
        }
        using (IVfsFileHandle? writer = OpenVfsFile(SuperfloppyFilePath))
        {
            Assert.NotNull(writer);
            Assert.Equal<long>(payload.Length, writer!.Write(payload));
            Assert.True(writer.TryFlush());
        }
        using (IVfsFileHandle? reader = OpenVfsFile(SuperfloppyFilePath))
        {
            Assert.NotNull(reader);
            byte[] readBack = new byte[payload.Length];
            Assert.Equal<long>(payload.Length, reader!.Read(readBack));
            for (int i = 0; i < payload.Length; i++)
            {
                Assert.Equal(payload[i], readBack[i]);
            }
        }

        // Drop the superblock before later cells (and the reboot cell's GPT
        // stamp) rewrite the disk underneath it.
        Assert.True(VfsManager.TryUnmount(SuperfloppyMountPoint));
    }

    private static IVfsFileHandle? OpenVfsFile(string path)
    {
        return VfsManager.TryOpenFile(path, out IVfsFileHandle? file) ? file : null;
    }

    // Attach a partition starting at an arbitrary LBA, write to its LBA 0,
    // and verify the bytes show up at the host's StartSector — proves
    // the translation isn't off by one.
    private static void TestPartition_ReadWriteTranslatesLba()
    {
        const ulong startSector = 3000;
        const ulong sectorCount = 4;
        Partition partition = new(s_dev!, startSector, sectorCount, "test-part");

        Span<byte> writeBuf = new byte[s_dev!.BlockSize];
        for (int i = 0; i < writeBuf.Length; i++)
        {
            writeBuf[i] = (byte)(i ^ TranslateXorSeed);
        }
        partition.WriteBlock(0, 1, writeBuf);

        Span<byte> hostBuf = new byte[s_dev.BlockSize];
        s_dev.ReadBlock(startSector, 1, hostBuf);
        for (int i = 0; i < hostBuf.Length; i++)
        {
            Assert.Equal(writeBuf[i], hostBuf[i]);
        }
    }

    private static void TestPartition_OutOfBoundsThrows()
    {
        Partition partition = new(s_dev!, startSector: TinyPartStartSector, sectorCount: TinyPartSectorCount, name: "tiny-part");
        Span<byte> buf = new byte[s_dev!.BlockSize];
        try
        {
            partition.ReadBlock(blockNo: TinyPartSectorCount, blockCount: 1, buf);
            Assert.Fail("Expected ArgumentOutOfRangeException for partition over-read.");
        }
        catch (ArgumentOutOfRangeException)
        {
            // Expected.
        }
    }

    // Regression guard: a naive `blockNo + blockCount > BlockCount` check wraps for
    // a blockNo near ulong.MaxValue and lets an out-of-bounds request through to the
    // host. Partition.CheckBounds ships the overflow-safe form
    // (`blockNo > BlockCount || blockCount > BlockCount - blockNo`); this test pins it.
    private static void TestPartition_BoundsOverflowThrows()
    {
        BoundsProbeDevice probe = new();
        Partition partition = new(probe, startSector: 0, sectorCount: OverflowProbeSectorCount, name: "overflow-probe");
        Span<byte> buf = new byte[probe.BlockSize];
        try
        {
            // ulong.MaxValue + 1 wraps to 0, which is <= BlockCount (4): the current
            // check passes and the read reaches the host. It must throw instead.
            partition.ReadBlock(blockNo: ulong.MaxValue, blockCount: 1, buf);
            Assert.Fail("CheckBounds let an overflowing out-of-bounds read through (expected ArgumentOutOfRangeException).");
        }
        catch (ArgumentOutOfRangeException)
        {
            // Correct behavior once the overflow-safe check lands.
        }
    }

    // In-memory host for the bounds-overflow test: performs no I/O. If Partition's
    // bounds check wrongly lets a request through, the call lands here as a no-op
    // rather than issuing a wild DMA against a real disk.
    // Gpt.IsGpt/Parse/AddPartition read LBA 1; per the IBlockDevice contract
    // an out-of-range read throws, so on a degenerate 1-block device these
    // public APIs must return false/empty instead of leaking the throw.
    private static void TestPartition_GptTinyDeviceSafe()
    {
        TinyDevice tiny = new();
        Assert.False(Gpt.IsGpt(tiny), "1-block device cannot carry a GPT");
        Assert.Equal(0, Gpt.Parse(tiny).Count, "1-block device must parse empty");
        Assert.False(Gpt.AddPartition(tiny, Gpt.FirstUsableLba, MinimalPartitionSectorCount, Gpt.BasicDataPartitionType), "AddPartition must reject a 1-block device");
    }

    // ==================== Partition lifecycle (MBR mutation, EBR chain, PartitionManager) ====================

    // Resize slot 0 in place through the dedicated table mutator (systemId
    // and start untouched) and verify Parse reports the new geometry.
    private static void TestPartition_MbrResize()
    {
        IBlockDevice host = s_dev!;
        ResetHostMbr(host);
        Assert.True(Mbr.AddPartition(host, 0, MbrLinuxSystemId, MbrPartAStartSector, MbrPartASectorCount));
        Assert.True(Mbr.AddPartition(host, 1, MbrFat32SystemId, MbrPartBStartSector, MbrPartBSectorCount));

        const uint resizedSectorCount = 350;
        Assert.True(Mbr.ResizePartition(host, 0, resizedSectorCount));

        List<MbrPartitionEntry> parts = Mbr.Parse(host);
        Assert.Equal(2, parts.Count);
        Assert.Equal<ulong>(MbrPartAStartSector, parts[0].StartSector);
        Assert.Equal<ulong>(resizedSectorCount, parts[0].SectorCount);
        Assert.Equal<ulong>(MbrPartBStartSector, parts[1].StartSector);
        Assert.Equal<ulong>(MbrPartBSectorCount, parts[1].SectorCount);
    }

    // Delete slot 1 through the dedicated table mutator; Parse skips the
    // now-empty entry.
    private static void TestPartition_MbrDelete()
    {
        IBlockDevice host = s_dev!;
        ResetHostMbr(host);
        Assert.True(Mbr.AddPartition(host, 0, MbrLinuxSystemId, MbrPartAStartSector, MbrPartASectorCount));
        Assert.True(Mbr.AddPartition(host, 1, MbrFat32SystemId, MbrPartBStartSector, MbrPartBSectorCount));

        Assert.True(Mbr.RemovePartition(host, 1));

        List<MbrPartitionEntry> parts = Mbr.Parse(host);
        Assert.Equal(1, parts.Count);
        Assert.Equal<byte>(MbrLinuxSystemId, parts[0].SystemId);
        Assert.Equal<ulong>(MbrPartAStartSector, parts[0].StartSector);
        Assert.Equal<ulong>(MbrPartASectorCount, parts[0].SectorCount);
    }

    // Hand-crafts a two-link EBR chain and asserts Ebr.Parse walks it,
    // resolving each logical's absolute LBA from the EBR-relative fields.
    private static void TestEbr_ChainDiscoversLogicals()
    {
        IBlockDevice host = s_dev!;
        const ulong extendedStart = 4000;
        const uint extendedCount = 8000;
        const uint logicalRelStart = 32;
        const uint logicalSectorCount = 1000;
        const uint nextRelativeLba = 2000;

        // Wipe LBAs 0..1 to clear any GPT signature a previous cell left, so
        // the StorageManager rescan (next cell) takes the MBR code path.
        Span<byte> wipe = new byte[host.BlockSize];
        host.WriteBlock(MbrLba, 1, wipe);
        host.WriteBlock(Gpt.PrimaryHeaderLba, 1, wipe);

        Mbr.Create(host);
        Assert.True(Mbr.AddPartition(host, 0, MbrLinuxSystemId, MbrPartAStartSector, MbrPartASectorCount));
        Assert.True(Mbr.AddPartition(host, 1, MbrExtendedSystemId, (uint)extendedStart, extendedCount));

        WriteEbrSector(host, extendedStart, logicalRelStart, logicalSectorCount, hasNext: true, nextRelativeLba: nextRelativeLba);
        WriteEbrSector(host, extendedStart + nextRelativeLba, logicalRelStart, logicalSectorCount, hasNext: false, nextRelativeLba: 0);

        List<MbrPartitionEntry> logicals = Ebr.Parse(host, extendedStart);
        Assert.Equal(2, logicals.Count);
        Assert.Equal<ulong>(extendedStart + logicalRelStart, logicals[0].StartSector);
        Assert.Equal<ulong>(logicalSectorCount, logicals[0].SectorCount);
        Assert.Equal<ulong>(extendedStart + nextRelativeLba + logicalRelStart, logicals[1].StartSector);
        Assert.Equal<ulong>(logicalSectorCount, logicals[1].SectorCount);
    }

    // Layout left by TestEbr_ChainDiscoversLogicals: one primary plus one
    // extended entry pointing at a two-link chain. Rescan must register
    // 1 primary + 2 logicals = 3 partitions on the host.
    private static void TestPartition_RescanWithEbr()
    {
        StorageManager.RescanPartitions(s_dev!);

        int matches = 0;
        for (int i = 0; i < StorageManager.Partitions.Count; i++)
        {
            Partition p = StorageManager.Partitions[i];
            if (ReferenceEquals(p.Host, s_dev))
            {
                matches++;
            }
        }
        Assert.Equal(3, matches);
    }

    private static void TestPartitionManager_CreateOnMbr()
    {
        IBlockDevice host = s_dev!;
        ResetHostMbr(host);
        const uint start = 200;
        const uint count = 100;
        Assert.True(PartitionManager.Create(host, start, count, MbrLinuxSystemId, Gpt.BasicDataPartitionType));

        List<MbrPartitionEntry> parts = Mbr.Parse(host);
        Assert.Equal(1, parts.Count);
        Assert.Equal<byte>(MbrLinuxSystemId, parts[0].SystemId);
        Assert.Equal<ulong>(start, parts[0].StartSector);
        Assert.Equal<ulong>(count, parts[0].SectorCount);
    }

    private static void TestPartitionManager_ResizeOnMbr()
    {
        IBlockDevice host = s_dev!;
        ResetHostMbr(host);
        const uint start = 200;
        const uint count = 100;
        const ulong resized = 250;
        PartitionManager.Create(host, start, count, MbrLinuxSystemId, Gpt.BasicDataPartitionType);

        Assert.True(PartitionManager.Resize(host, new PartitionManager.PartitionLocation(start, count), resized));

        List<MbrPartitionEntry> parts = Mbr.Parse(host);
        Assert.Equal(1, parts.Count);
        Assert.Equal<ulong>(start, parts[0].StartSector);
        Assert.Equal<ulong>(resized, parts[0].SectorCount);
    }

    private static void TestPartitionManager_DeleteOnMbr()
    {
        IBlockDevice host = s_dev!;
        ResetHostMbr(host);
        const uint startA = 200;
        const uint countA = 100;
        const uint startB = 500;
        const uint countB = 200;
        PartitionManager.Create(host, startA, countA, MbrLinuxSystemId, Gpt.BasicDataPartitionType);
        PartitionManager.Create(host, startB, countB, MbrFat32SystemId, Gpt.BasicDataPartitionType);

        Assert.True(PartitionManager.Delete(host, new PartitionManager.PartitionLocation(startA, countA)));

        List<MbrPartitionEntry> parts = Mbr.Parse(host);
        Assert.Equal(1, parts.Count);
        Assert.Equal<ulong>(startB, parts[0].StartSector);
        Assert.Equal<ulong>(countB, parts[0].SectorCount);
    }

    private static void TestPartitionManager_MoveNonOverlapping()
    {
        IBlockDevice host = s_dev!;
        ResetHostMbr(host);
        const uint oldStart = 1000;
        const uint count = 64;
        const uint newStart = 5000;
        const uint seedBase = 0xC0DE0000;
        PartitionManager.Create(host, oldStart, count, MbrLinuxSystemId, Gpt.BasicDataPartitionType);

        Span<byte> patternSector = new byte[host.BlockSize];
        for (ulong lba = 0; lba < count; lba++)
        {
            FillPattern(patternSector, (uint)(seedBase + lba));
            host.WriteBlock(oldStart + lba, 1, patternSector);
        }

        Assert.True(PartitionManager.MoveWithData(host, new PartitionManager.PartitionLocation(oldStart, count), newStart));

        List<MbrPartitionEntry> parts = Mbr.Parse(host);
        Assert.Equal(1, parts.Count);
        Assert.Equal<ulong>(newStart, parts[0].StartSector);
        Assert.Equal<ulong>(count, parts[0].SectorCount);

        AssertMovedPattern(host, newStart, count, seedBase);
    }

    // Destination starts inside the source range, so the copy must walk
    // backwards or it would overwrite source sectors before reading them.
    private static void TestPartitionManager_MoveOverlappingForward()
    {
        IBlockDevice host = s_dev!;
        ResetHostMbr(host);
        const uint oldStart = 2000;
        const uint count = 200;
        const uint newStart = 2050; // overlaps oldStart..oldStart+count
        const uint seedBase = 0xBABE0000;
        PartitionManager.Create(host, oldStart, count, MbrLinuxSystemId, Gpt.BasicDataPartitionType);

        Span<byte> patternSector = new byte[host.BlockSize];
        for (ulong lba = 0; lba < count; lba++)
        {
            FillPattern(patternSector, (uint)(seedBase + lba));
            host.WriteBlock(oldStart + lba, 1, patternSector);
        }

        Assert.True(PartitionManager.MoveWithData(host, new PartitionManager.PartitionLocation(oldStart, count), newStart));

        // Same table assertions as the non-overlapping variant: an
        // overlap-specific slot-resolution regression would keep the data
        // pattern intact while rewriting the wrong entry.
        List<MbrPartitionEntry> parts = Mbr.Parse(host);
        Assert.Equal(1, parts.Count);
        Assert.Equal<ulong>(newStart, parts[0].StartSector);
        Assert.Equal<ulong>(count, parts[0].SectorCount);

        AssertMovedPattern(host, newStart, count, seedBase);
    }

    private static void TestPartitionManager_ResizeOnGpt()
    {
        IBlockDevice host = s_dev!;
        ResetHostGpt(host);
        const ulong count = 4096;
        const ulong resized = 8192;
        Assert.True(Gpt.AddPartition(host, GptAlignedStartLba, count, Gpt.BasicDataPartitionType));

        Assert.True(PartitionManager.Resize(host, new PartitionManager.PartitionLocation(GptAlignedStartLba, count), resized));

        List<GptPartitionEntry> parts = Gpt.Parse(host);
        Assert.Equal(1, parts.Count);
        Assert.Equal<ulong>(GptAlignedStartLba, parts[0].StartSector);
        Assert.Equal<ulong>(resized, parts[0].SectorCount);
    }

    private static void TestPartitionManager_DeleteOnGpt()
    {
        IBlockDevice host = s_dev!;
        ResetHostGpt(host);
        const ulong count = 4096;
        Assert.True(Gpt.AddPartition(host, GptAlignedStartLba, count, Gpt.BasicDataPartitionType));
        Assert.True(Gpt.AddPartition(host, GptAlignedStartLba + count, count, Gpt.BasicDataPartitionType));

        Assert.True(PartitionManager.Delete(host, new PartitionManager.PartitionLocation(GptAlignedStartLba, count)));

        List<GptPartitionEntry> parts = Gpt.Parse(host);
        Assert.Equal(1, parts.Count);
        Assert.Equal<ulong>(GptAlignedStartLba + count, parts[0].StartSector);
    }

    private static void TestPartitionManager_MoveOnGpt()
    {
        IBlockDevice host = s_dev!;
        ResetHostGpt(host);
        const ulong count = 64;
        const ulong newStart = 4096;
        const uint seedBase = 0xF00D0000;
        Assert.True(Gpt.AddPartition(host, GptAlignedStartLba, count, Gpt.BasicDataPartitionType));

        Span<byte> patternSector = new byte[host.BlockSize];
        for (ulong lba = 0; lba < count; lba++)
        {
            FillPattern(patternSector, (uint)(seedBase + lba));
            host.WriteBlock(GptAlignedStartLba + lba, 1, patternSector);
        }

        Assert.True(PartitionManager.MoveWithData(host, new PartitionManager.PartitionLocation(GptAlignedStartLba, count), newStart));

        List<GptPartitionEntry> parts = Gpt.Parse(host);
        Assert.Equal(1, parts.Count);
        Assert.Equal<ulong>(newStart, parts[0].StartSector);
        Assert.Equal<ulong>(count, parts[0].SectorCount);

        AssertMovedPattern(host, newStart, count, seedBase);
    }

    // --- EBR (logical partition) lifecycle -----------------------------
    // Each cell starts from a fresh MBR + extended-partition layout.

    private static void TestEbr_AddLogicalFirstAndSecond()
    {
        IBlockDevice host = s_dev!;
        ResetHostExtendedMbr(host, ExtPartStartSector, ExtPartSectorCount);
        const uint countA = 100;
        const uint countB = 200;

        Ebr.TryAddLogical(host, ExtPartStartSector, ExtPartSectorCount, MbrLinuxSystemId, countA, out ulong logical0Start);
        Assert.True(logical0Start != 0);

        Ebr.TryAddLogical(host, ExtPartStartSector, ExtPartSectorCount, MbrLinuxSystemId, countB, out ulong logical1Start);
        Assert.True(logical1Start != 0);

        List<MbrPartitionEntry> logicals = Ebr.Parse(host, ExtPartStartSector);
        Assert.Equal(2, logicals.Count);
        Assert.Equal<ulong>(logical0Start, logicals[0].StartSector);
        Assert.Equal<ulong>(countA, logicals[0].SectorCount);
        Assert.Equal<ulong>(logical1Start, logicals[1].StartSector);
        Assert.Equal<ulong>(countB, logicals[1].SectorCount);
    }

    private static void TestEbr_RemoveLogicalLast()
    {
        IBlockDevice host = s_dev!;
        ResetHostExtendedMbr(host, ExtPartStartSector, ExtPartSectorCount);
        const uint countA = 100;
        const uint countB = 200;
        Ebr.TryAddLogical(host, ExtPartStartSector, ExtPartSectorCount, MbrLinuxSystemId, countA, out ulong logical0Start);
        Ebr.TryAddLogical(host, ExtPartStartSector, ExtPartSectorCount, MbrLinuxSystemId, countB, out _);

        Assert.True(Ebr.RemoveLogical(host, ExtPartStartSector, 1));
        List<MbrPartitionEntry> logicals = Ebr.Parse(host, ExtPartStartSector);
        Assert.Equal(1, logicals.Count);
        Assert.Equal<ulong>(logical0Start, logicals[0].StartSector);
        Assert.Equal<ulong>(countA, logicals[0].SectorCount);
    }

    private static void TestEbr_RemoveLogicalFirstPromotesSuccessor()
    {
        IBlockDevice host = s_dev!;
        ResetHostExtendedMbr(host, ExtPartStartSector, ExtPartSectorCount);
        const uint countA = 100;
        const uint countB = 200;
        Ebr.TryAddLogical(host, ExtPartStartSector, ExtPartSectorCount, MbrLinuxSystemId, countA, out _);
        Ebr.TryAddLogical(host, ExtPartStartSector, ExtPartSectorCount, MbrLinuxSystemId, countB, out ulong logical1Start);

        Assert.True(Ebr.RemoveLogical(host, ExtPartStartSector, 0));
        List<MbrPartitionEntry> logicals = Ebr.Parse(host, ExtPartStartSector);
        Assert.Equal(1, logicals.Count);
        Assert.Equal<ulong>(logical1Start, logicals[0].StartSector);
        Assert.Equal<ulong>(countB, logicals[0].SectorCount);
    }

    private static void TestEbr_RemoveLogicalMiddleCollapsesChain()
    {
        IBlockDevice host = s_dev!;
        ResetHostExtendedMbr(host, ExtPartStartSector, ExtPartSectorCount);
        const uint countA = 100;
        const uint countB = 200;
        const uint countC = 300;
        Ebr.TryAddLogical(host, ExtPartStartSector, ExtPartSectorCount, MbrLinuxSystemId, countA, out ulong logical0Start);
        Ebr.TryAddLogical(host, ExtPartStartSector, ExtPartSectorCount, MbrLinuxSystemId, countB, out _);
        Ebr.TryAddLogical(host, ExtPartStartSector, ExtPartSectorCount, MbrLinuxSystemId, countC, out ulong logical2Start);

        Assert.True(Ebr.RemoveLogical(host, ExtPartStartSector, 1));
        List<MbrPartitionEntry> logicals = Ebr.Parse(host, ExtPartStartSector);
        Assert.Equal(2, logicals.Count);
        Assert.Equal<ulong>(logical0Start, logicals[0].StartSector);
        Assert.Equal<ulong>(countA, logicals[0].SectorCount);
        Assert.Equal<ulong>(logical2Start, logicals[1].StartSector);
        Assert.Equal<ulong>(countC, logicals[1].SectorCount);
    }

    private static void TestEbr_RemoveLogicalOnlyOneClearsChain()
    {
        IBlockDevice host = s_dev!;
        ResetHostExtendedMbr(host, ExtPartStartSector, ExtPartSectorCount);
        const uint count = 100;
        Ebr.TryAddLogical(host, ExtPartStartSector, ExtPartSectorCount, MbrLinuxSystemId, count, out _);

        Assert.True(Ebr.RemoveLogical(host, ExtPartStartSector, 0));
        List<MbrPartitionEntry> logicals = Ebr.Parse(host, ExtPartStartSector);
        Assert.Equal(0, logicals.Count);
    }

    private static void TestEbr_ResizeLogical()
    {
        IBlockDevice host = s_dev!;
        ResetHostExtendedMbr(host, ExtPartStartSector, ExtPartSectorCount);
        const uint count = 100;
        const ulong resized = 250;
        Ebr.TryAddLogical(host, ExtPartStartSector, ExtPartSectorCount, MbrLinuxSystemId, count, out _);

        Assert.True(Ebr.ResizeLogical(host, ExtPartStartSector, 0, resized));
        List<MbrPartitionEntry> logicals = Ebr.Parse(host, ExtPartStartSector);
        Assert.Equal(1, logicals.Count);
        Assert.Equal<ulong>(resized, logicals[0].SectorCount);
    }

    private static void TestEbr_MoveLogicalTableLevel()
    {
        IBlockDevice host = s_dev!;
        ResetHostExtendedMbr(host, ExtPartStartSector, ExtPartSectorCount);
        const uint count = 50;
        const ulong moveDelta = 200;
        Ebr.TryAddLogical(host, ExtPartStartSector, ExtPartSectorCount, MbrLinuxSystemId, count, out ulong logicalStart);
        // The first logical lands right after its EBR sector.
        Assert.Equal<ulong>(ExtPartStartSector + EbrLogicalDataOffsetSectors, logicalStart);

        // Move the logical's data range forward inside its EBR's frame.
        ulong newStart = logicalStart + moveDelta;
        Assert.True(Ebr.MoveLogical(host, ExtPartStartSector, 0, newStart));

        List<MbrPartitionEntry> logicals = Ebr.Parse(host, ExtPartStartSector);
        Assert.Equal(1, logicals.Count);
        Assert.Equal<ulong>(newStart, logicals[0].StartSector);
        Assert.Equal<ulong>(count, logicals[0].SectorCount);
    }

    // Regression guard: Ebr.Parse is the trust boundary for on-disk EBR
    // corruption, same rule as Mbr.Parse for primaries — a logical whose
    // range leaves the extended envelope authorizes wild host I/O, and a
    // relative start of 0 aliases the EBR sector itself (writing through
    // that "partition" destroys the chain).
    private static void TestEbr_ParseSkipsCorruptEntries()
    {
        IBlockDevice host = s_dev!;
        ResetHostExtendedMbr(host, ExtPartStartSector, ExtPartSectorCount);
        const uint count = 100;
        Assert.True(Ebr.TryAddLogical(host, ExtPartStartSector, ExtPartSectorCount, MbrLinuxSystemId, count, out _));

        // Oversize the logical's sector count in place so its range runs
        // past the extended envelope and the device end.
        int sector = (int)host.BlockSize;
        byte[] ebr = new byte[sector];
        host.ReadBlock(ExtPartStartSector, 1, ebr);
        Span<byte> e = ebr;
        BitConverter.TryWriteBytes(e.Slice(MbrEntry0Offset + MbrEntrySectorCountOffset, MbrLbaFieldBytes), uint.MaxValue);
        host.WriteBlock(ExtPartStartSector, 1, ebr);
        Assert.Equal(0, EbrParseCountSafe(host), "logical running past the extended envelope must be dropped");

        // Relative start 0: the logical's first sector IS its EBR sector.
        host.ReadBlock(ExtPartStartSector, 1, ebr);
        BitConverter.TryWriteBytes(e.Slice(MbrEntry0Offset + MbrEntryStartLbaOffset, MbrLbaFieldBytes), SelfAliasingStartLba);
        BitConverter.TryWriteBytes(e.Slice(MbrEntry0Offset + MbrEntrySectorCountOffset, MbrLbaFieldBytes), count);
        host.WriteBlock(ExtPartStartSector, 1, ebr);
        Assert.Equal(0, EbrParseCountSafe(host), "logical aliasing its own EBR sector must be dropped");
    }

    // A corrupt next pointer must stop the walk, not crash it: past the
    // device end the ReadBlock would throw per the IBlockDevice contract,
    // and inside the disk but outside the extended envelope any
    // 0x55AA-terminated sector (e.g. a FAT VBR) parses as garbage logicals.
    private static void TestEbr_ParseStopsOnWildNextPointer()
    {
        IBlockDevice host = s_dev!;
        ResetHostExtendedMbr(host, ExtPartStartSector, ExtPartSectorCount);
        const uint count = 50;
        Assert.True(Ebr.TryAddLogical(host, ExtPartStartSector, ExtPartSectorCount, MbrLinuxSystemId, count, out _));

        // Next pointer that stays on-disk but escapes the extended
        // envelope, landing on a crafted 0x55AA sector with a plausible
        // logical entry: the walk must stop at the envelope edge.
        const uint escapeRelative = ExtPartSectorCount + 1000;
        int sector = (int)host.BlockSize;
        byte[] fake = new byte[sector];
        Span<byte> f = fake;
        f[MbrEntry0Offset + MbrEntryTypeOffset] = MbrLinuxSystemId;
        BitConverter.TryWriteBytes(f.Slice(MbrEntry0Offset + MbrEntryStartLbaOffset, MbrLbaFieldBytes), EbrLogicalDataOffsetSectors);
        BitConverter.TryWriteBytes(f.Slice(MbrEntry0Offset + MbrEntrySectorCountOffset, MbrLbaFieldBytes), count);
        f[sector - MbrBootSigSizeBytes] = MbrBootSigLowByte;
        f[sector - 1] = MbrBootSigHighByte;
        host.WriteBlock(ExtPartStartSector + escapeRelative, 1, fake);

        byte[] ebr = new byte[sector];
        host.ReadBlock(ExtPartStartSector, 1, ebr);
        Span<byte> e = ebr;
        e[MbrEntry1Offset + MbrEntryTypeOffset] = MbrExtendedSystemId;
        BitConverter.TryWriteBytes(e.Slice(MbrEntry1Offset + MbrEntryStartLbaOffset, MbrLbaFieldBytes), escapeRelative);
        host.WriteBlock(ExtPartStartSector, 1, ebr);
        Assert.Equal(1, EbrParseCountSafe(host), "walk must stop when the next pointer leaves the extended envelope");

        // Next pointer past the device end: the walk must stop instead of
        // letting the out-of-range ReadBlock throw out of Parse.
        host.ReadBlock(ExtPartStartSector, 1, ebr);
        BitConverter.TryWriteBytes(
            e.Slice(MbrEntry1Offset + MbrEntryStartLbaOffset, MbrLbaFieldBytes),
            (uint)(host.BlockCount - ExtPartStartSector + WildNextOvershootSectors));
        host.WriteBlock(ExtPartStartSector, 1, ebr);
        Assert.Equal(1, EbrParseCountSafe(host), "walk must stop when the next pointer leaves the device");
    }

    // Ebr.TryAddLogical must reject geometry its own parser would drop: the
    // caller-supplied envelope is on-disk metadata (the MBR's extended
    // entry), so it cannot authorize I/O past the device end — and a
    // sector count that does not fit the 32-bit on-disk field would be
    // silently truncated (2^32 stamps a zero-length entry).
    private static void TestEbr_AddLogicalRejectsBogusGeometry()
    {
        IBlockDevice host = s_dev!;
        ResetHostExtendedMbr(host, ExtPartStartSector, ExtPartSectorCount);

        // Oversized caller envelope + range past the device end: the room
        // check passes against the fake envelope, so an unclamped
        // TryAddLogical stamps a logical extending past the disk.
        ulong fakeEnvelope = ulong.MaxValue - ExtPartStartSector;
        Assert.False(
            Ebr.TryAddLogical(host, ExtPartStartSector, fakeEnvelope, MbrLinuxSystemId, host.BlockCount, out _),
            "an envelope past the device end must not authorize a past-end logical");

        Assert.False(
            Ebr.TryAddLogical(host, ExtPartStartSector, fakeEnvelope, MbrLinuxSystemId, 1UL << MbrSectorCountFieldBits, out _),
            "a sector count exceeding the 32-bit on-disk field must be rejected");

        Assert.Equal(0, EbrParseCountSafe(host), "rejected TryAddLogical calls must leave no live entries");
    }

    // ResolveExtendedCount hands ResizeLogical/MoveLogical their upper
    // bound. A corrupt extended count must clamp to the device end, and a
    // missing MBR extended entry must grant nothing — the whole-disk
    // fallback let a resize grow the last logical into whatever follows
    // the extended partition.
    private static void TestEbr_ResizeLogicalRespectsEnvelopeBounds()
    {
        IBlockDevice host = s_dev!;
        ResetHostExtendedMbr(host, ExtPartStartSector, ExtPartSectorCount);
        const uint count = 100;
        Assert.True(Ebr.TryAddLogical(host, ExtPartStartSector, ExtPartSectorCount, MbrLinuxSystemId, count, out _));

        // Corrupt the MBR extended entry's count (raw, bypassing the
        // writer's validation) so it runs past the device end.
        int sector = (int)host.BlockSize;
        byte[] mbr = new byte[sector];
        host.ReadBlock(MbrLba, 1, mbr);
        Span<byte> m = mbr;
        BitConverter.TryWriteBytes(m.Slice(MbrEntry0Offset + MbrEntrySectorCountOffset, MbrLbaFieldBytes), uint.MaxValue);
        host.WriteBlock(MbrLba, 1, mbr);
        Assert.False(Ebr.ResizeLogical(host, ExtPartStartSector, 0, host.BlockCount),
            "a corrupt extended count must not authorize a resize past the device end");

        // Remove the extended entry entirely: with no confirmable envelope
        // the resize must be refused outright.
        ResetHostExtendedMbr(host, ExtPartStartSector, ExtPartSectorCount);
        Assert.True(Ebr.TryAddLogical(host, ExtPartStartSector, ExtPartSectorCount, MbrLinuxSystemId, count, out _));
        Assert.True(Mbr.RemovePartition(host, 0));
        Assert.False(Ebr.ResizeLogical(host, ExtPartStartSector, 0, ExtPartSectorCount * 2),
            "a resize without a confirmable extended envelope must be refused");
    }

    // Mbr.TryGetExtendedPartition is the root every EBR walk starts from —
    // it must apply the same on-disk distrust as Parse instead of handing
    // Ebr whatever geometry the extended slot claims, and a corrupt slot
    // must not hide a valid one behind it.
    private static void TestMbr_TryGetExtendedRejectsBogusGeometry()
    {
        IBlockDevice host = s_dev!;
        ResetHostExtendedMbr(host, ExtPartStartSector, ExtPartSectorCount);

        // Corrupt the count (raw) so the envelope runs past the device end.
        int sector = (int)host.BlockSize;
        byte[] mbr = new byte[sector];
        host.ReadBlock(MbrLba, 1, mbr);
        Span<byte> m = mbr;
        BitConverter.TryWriteBytes(m.Slice(MbrEntry0Offset + MbrEntrySectorCountOffset, MbrLbaFieldBytes), uint.MaxValue);
        host.WriteBlock(MbrLba, 1, mbr);
        Assert.False(Mbr.TryGetExtendedPartition(host, out _, out _),
            "an extended entry running past the device end must be rejected");

        // Slot 0 corrupt (start 0), slot 1 valid: the valid entry must win
        // instead of the corrupt slot short-circuiting the scan.
        ResetHostExtendedMbr(host, ExtPartStartSector, ExtPartSectorCount);
        host.ReadBlock(MbrLba, 1, mbr);
        BitConverter.TryWriteBytes(m.Slice(MbrEntry0Offset + MbrEntryStartLbaOffset, MbrLbaFieldBytes), SelfAliasingStartLba);
        m[MbrEntry1Offset + MbrEntryTypeOffset] = MbrExtendedSystemId;
        BitConverter.TryWriteBytes(m.Slice(MbrEntry1Offset + MbrEntryStartLbaOffset, MbrLbaFieldBytes), ExtPartStartSector);
        BitConverter.TryWriteBytes(m.Slice(MbrEntry1Offset + MbrEntrySectorCountOffset, MbrLbaFieldBytes), ExtPartSectorCount);
        host.WriteBlock(MbrLba, 1, mbr);
        Assert.True(Mbr.TryGetExtendedPartition(host, out ulong start, out ulong count),
            "a corrupt extended slot must not hide a valid one");
        Assert.Equal<ulong>(ExtPartStartSector, start);
        Assert.Equal<ulong>(ExtPartSectorCount, count);
    }

    // Resize/Move must refuse slots Parse never surfaces: moving an
    // extended container orphans its EBR chain (the first EBR physically
    // stays at the old start), shrinking it can cut off tail logicals,
    // and the 0xEE protective entry guards the GPT structures.
    private static void TestMbr_ResizeMoveRejectsExtendedSlot()
    {
        IBlockDevice host = s_dev!;
        ResetHostExtendedMbr(host, ExtPartStartSector, ExtPartSectorCount);
        const uint count = 100;
        Assert.True(Ebr.TryAddLogical(host, ExtPartStartSector, ExtPartSectorCount, MbrLinuxSystemId, count, out _));

        Assert.True(MbrResizeRefused(0, ExtPartSectorCount / 2),
            "resizing the extended container must be refused");
        Assert.True(MbrMoveRefused(0, ExtPartStartSector + ExtendedMoveProbeDeltaSectors),
            "moving the extended container must be refused");
        Assert.Equal(1, EbrParseCountSafe(host));

        // GPT protective entry (0xEE) in slot 1 — also never surfaced.
        int sector = (int)host.BlockSize;
        byte[] mbr = new byte[sector];
        host.ReadBlock(MbrLba, 1, mbr);
        Span<byte> m = mbr;
        m[MbrEntry1Offset + MbrEntryTypeOffset] = MbrGptProtectiveSystemId;
        BitConverter.TryWriteBytes(m.Slice(MbrEntry1Offset + MbrEntryStartLbaOffset, MbrLbaFieldBytes), Gpt.ProtectiveMbrStartLba);
        BitConverter.TryWriteBytes(m.Slice(MbrEntry1Offset + MbrEntrySectorCountOffset, MbrLbaFieldBytes), MbrProtectiveSectorCount);
        host.WriteBlock(MbrLba, 1, mbr);
        Assert.True(MbrResizeRefused(1, ProtectiveResizeSectorCount),
            "resizing the GPT protective entry must be refused");
    }

    // Growing slot 0 into slot 1 or moving slot 1 onto slot 0 yields
    // overlapping partitions Parse returns both of; once registered as
    // block devices, writes through one corrupt the other. The write-time
    // rejection must catch the intersection while the table is in hand.
    private static void TestMbr_ResizeMoveRejectsOverlap()
    {
        IBlockDevice host = s_dev!;
        ResetHostMbr(host);
        const uint startA = 4000;
        const uint countA = 1000;
        const uint startB = 8000;
        const uint countB = 1000;
        Assert.True(Mbr.AddPartition(host, 0, MbrLinuxSystemId, startA, countA));
        Assert.True(Mbr.AddPartition(host, 1, MbrLinuxSystemId, startB, countB));

        Assert.True(MbrResizeRefused(0, startB - startA + 1),
            "growing a primary into its neighbour must be refused");
        Assert.True(MbrMoveRefused(1, startA + countA - 1),
            "moving a primary onto its neighbour must be refused");

        // Adjacency (half-open ranges) stays legal.
        Assert.True(Mbr.ResizePartition(host, 0, startB - startA));
        Assert.True(Mbr.MovePartition(host, 1, startB + LegalMoveDeltaSectors));
        Assert.Equal(2, Mbr.Parse(host).Count);
    }

    // Resize/Move are read-modify-write on the MBR sector; the sibling
    // writers (AddPartition/RemovePartition) repair a corrupt signature
    // on the way out, so these must too.
    private static void TestMbr_ResizeMoveRestampsSignature()
    {
        IBlockDevice host = s_dev!;
        ResetHostMbr(host);
        const uint start = 4000;
        const uint count = 1000;
        Assert.True(Mbr.AddPartition(host, 0, MbrLinuxSystemId, start, count));

        int sector = (int)host.BlockSize;
        byte[] mbr = new byte[sector];
        host.ReadBlock(MbrLba, 1, mbr);
        mbr[sector - MbrBootSigSizeBytes] = 0;
        mbr[sector - 1] = 0;
        host.WriteBlock(MbrLba, 1, mbr);
        Assert.True(Mbr.ResizePartition(host, 0, count * 2));
        Assert.True(Mbr.IsMbr(host), "ResizePartition must restamp the boot signature");

        host.ReadBlock(MbrLba, 1, mbr);
        mbr[sector - MbrBootSigSizeBytes] = 0;
        mbr[sector - 1] = 0;
        host.WriteBlock(MbrLba, 1, mbr);
        Assert.True(Mbr.MovePartition(host, 0, start + LegalMoveDeltaSectors));
        Assert.True(Mbr.IsMbr(host), "MovePartition must restamp the boot signature");
    }

    // One corrupt entry ahead of the target must not shift MutateEntry's
    // index space away from Parse's: Delete/Resize would then hit a
    // different, healthy partition (CRCs are 0 — nothing on disk flags the
    // damage), and mutators could see unvalidated LBAs (the
    // BlockCount - startLba underflow in ResizePartition).
    private static void TestGpt_MutateSkipsEntriesParseRejects()
    {
        IBlockDevice host = s_dev!;
        ResetHostGpt(host);
        const uint countA = 512;
        const uint countB = 512;
        const uint resized = 256;
        Assert.True(Gpt.AddPartition(host, GptAlignedStartLba, countA, Gpt.BasicDataPartitionType));
        Assert.True(Gpt.AddPartition(host, GptAlignedStartLba + GptSecondPartitionDeltaSectors, countB, Gpt.BasicDataPartitionType));

        // Raw-corrupt slot 0 (the first partition): start past the disk
        // end, which Parse drops but a naive used-slot count still sees.
        int sector = (int)host.BlockSize;
        byte[] entries = new byte[sector];
        host.ReadBlock(GptEntryArrayLba, 1, entries);
        Span<byte> e = entries;
        BitConverter.TryWriteBytes(e.Slice(GptEntryStartLbaOffset, GptLbaFieldBytes), host.BlockCount + GptCorruptStartOvershootSectors);
        BitConverter.TryWriteBytes(e.Slice(GptEntryEndLbaOffset, GptLbaFieldBytes), host.BlockCount + GptCorruptEndOvershootSectors);
        host.WriteBlock(GptEntryArrayLba, 1, entries);
        Assert.Equal(1, Gpt.Parse(host).Count);

        // Index 0 in Parse's space is the surviving partition — resize and
        // delete must land on it, not on the corrupt slot ahead of it.
        Assert.True(Gpt.ResizePartition(host, 0, resized));
        List<GptPartitionEntry> parts = Gpt.Parse(host);
        Assert.Equal(1, parts.Count);
        Assert.Equal<ulong>(resized, parts[0].SectorCount);
        Assert.True(Gpt.RemovePartition(host, 0));
        Assert.Equal(0, Gpt.Parse(host).Count, "delete must land on the partition Parse reports at index 0");
    }

    // UEFI expects unused entries fully zeroed, and AddPartition's slot
    // reuse never rewrites the name field — a deleted partition's UTF-16
    // name would resurface on the next partition created in that slot.
    private static void TestGpt_RemoveClearsWholeEntry()
    {
        IBlockDevice host = s_dev!;
        ResetHostGpt(host);
        const uint count = 512;
        Assert.True(Gpt.AddPartition(host, GptAlignedStartLba, count, Gpt.BasicDataPartitionType));

        // Stamp a UTF-16 name into slot 0 the way external tooling would.
        int sector = (int)host.BlockSize;
        byte[] entries = new byte[sector];
        host.ReadBlock(GptEntryArrayLba, 1, entries);
        for (int i = 0; i < GptNameStampChars; i++)
        {
            entries[GptEntryNameOffset + i * Utf16BytesPerChar] = (byte)(GptNameStampBaseChar + i);
        }
        host.WriteBlock(GptEntryArrayLba, 1, entries);

        Assert.True(Gpt.RemovePartition(host, 0));
        host.ReadBlock(GptEntryArrayLba, 1, entries);
        bool allZero = true;
        for (int i = 0; i < GptEntrySizeBytes; i++)
        {
            if (entries[i] != 0)
            {
                allZero = false;
                break;
            }
        }
        Assert.True(allZero, "a removed entry must be fully zeroed, including the UTF-16 name");
    }

    // A false return from MoveWithData must leave the disk unmodified:
    // the data copy may only happen after the table entry is resolved and
    // every table-specific constraint has passed.
    private static void TestPartitionManager_MoveFailureIsNonDestructive()
    {
        IBlockDevice host = s_dev!;
        ResetHostMbr(host);
        const uint start = 4000;
        const uint count = 64;
        const ulong freeDest = 20000;
        const uint sentinelSeed = 0xDEAD0000;
        Assert.True(PartitionManager.Create(host, start, count, MbrLinuxSystemId, Gpt.BasicDataPartitionType));

        Span<byte> patternSector = new byte[host.BlockSize];
        for (ulong lba = 0; lba < count; lba++)
        {
            FillPattern(patternSector, (uint)(sentinelSeed + lba));
            host.WriteBlock(freeDest + lba, 1, patternSector);
        }

        // Location matches no table entry (wrong count): must fail without
        // having copied a single sector to the destination.
        Assert.False(PartitionManager.MoveWithData(host, new PartitionManager.PartitionLocation(start, count - 1), freeDest));
        AssertMovedPattern(host, freeDest, count, sentinelSeed);
    }

    // The destination of a data-copying move must be free space —
    // copying first physically clobbers the neighbour before the table
    // edit can refuse — and Create must not stamp a range intersecting
    // an existing partition.
    private static void TestPartitionManager_RejectsOccupiedRanges()
    {
        IBlockDevice host = s_dev!;
        ResetHostMbr(host);
        const uint startA = 4000;
        const uint countA = 100;
        const uint startB = 8000;
        const uint countB = 100;
        const uint seedB = 0xBEEF0000;
        Assert.True(PartitionManager.Create(host, startA, countA, MbrLinuxSystemId, Gpt.BasicDataPartitionType));
        Assert.True(PartitionManager.Create(host, startB, countB, MbrLinuxSystemId, Gpt.BasicDataPartitionType));

        Span<byte> patternSector = new byte[host.BlockSize];
        for (ulong lba = 0; lba < countB; lba++)
        {
            FillPattern(patternSector, (uint)(seedB + lba));
            host.WriteBlock(startB + lba, 1, patternSector);
        }

        Assert.True(PmMoveRefusedCleanly(new PartitionManager.PartitionLocation(startA, countA), startB - countA / 2),
            "a move onto an occupied destination must be refused");
        AssertMovedPattern(host, startB, countB, seedB);

        Assert.True(PmCreateRefusedCleanly(startA + NestedCreateOffsetSectors, NestedCreateSectorCount),
            "creating inside an existing partition must be refused");
    }

    // The facade's bounds guards used wrapping ulong addition: a
    // destination near 2^64 wrapped the sum, slipped past the guard and
    // reached raw sector I/O; Create at LBA 0 threw from Mbr.AddPartition
    // instead of returning the documented false.
    private static void TestPartitionManager_GuardsDoNotWrap()
    {
        IBlockDevice host = s_dev!;
        ResetHostMbr(host);
        const uint start = 4000;
        const uint count = 64;
        Assert.True(PartitionManager.Create(host, start, count, MbrLinuxSystemId, Gpt.BasicDataPartitionType));

        Assert.True(PmCreateRefusedCleanly(SelfAliasingStartLba, LbaZeroCreateSectorCount),
            "create at LBA 0 must be refused, not thrown");
        // Last on purpose: pre-fix this issued a wild write near 2^64.
        Assert.True(PmMoveRefusedCleanly(new PartitionManager.PartitionLocation(start, count), ulong.MaxValue - 1),
            "a move destination near 2^64 must be refused, not wrapped past the guard");
    }

    // Resize must apply the free-space invariant Create already applies. The
    // low-level writers only validate device bounds, and each format broke
    // differently without this: Gpt.ResizePartition checks only the device
    // end, so growing over a neighbour stamped two entries aliasing the same
    // sectors and returned true; Mbr.ResizePartition threw out of a
    // bool-returning method. Only Ebr.ResizeLogical bounded itself.
    private static void TestGpt_ResizeMoveRejectsOverlap()
    {
        IBlockDevice host = s_dev!;
        ResetHostGpt(host);
        const ulong startA = GptAlignedStartLba;
        const ulong startB = GptAlignedStartLba + GptOverlapNeighbourOffsetSectors;
        Assert.True(Gpt.AddPartition(host, startA, GptOverlapPartitionSectorCount, Gpt.BasicDataPartitionType));
        Assert.True(Gpt.AddPartition(host, startB, GptOverlapPartitionSectorCount, Gpt.BasicDataPartitionType));

        // The writers refuse the overlap themselves; nothing above them
        // has to.
        Assert.False(Gpt.ResizePartition(host, 0, startB - startA + 1),
            "growing a GPT entry into its neighbour must be refused");
        Assert.False(Gpt.MovePartition(host, 1, startA + GptOverlapPartitionSectorCount - 1),
            "moving a GPT entry onto its neighbour must be refused");
        Assert.False(Gpt.AddPartition(host, startA + GptOverlapPartitionSectorCount / 2, GptOverlapPartitionSectorCount, Gpt.BasicDataPartitionType),
            "adding a GPT entry on top of another must be refused");
        List<GptPartitionEntry> parts = Gpt.Parse(host);
        Assert.Equal(2, parts.Count, "a refused write leaves the table unchanged");
        Assert.Equal<ulong>(GptOverlapPartitionSectorCount, parts[0].SectorCount, "the refused resize left the first entry alone");
        Assert.Equal<ulong>(startB, parts[1].StartSector, "the refused move left the second entry alone");

        // Adjacency stays legal: the end LBA is inclusive, so an entry may
        // end on the sector just before its neighbour starts.
        Assert.True(Gpt.ResizePartition(host, 0, startB - startA), "growing up to the neighbour is allowed");
        Assert.True(Gpt.MovePartition(host, 1, startB + LegalMoveDeltaSectors), "moving into free space is allowed");
        Assert.Equal(2, Gpt.Parse(host).Count);
    }

    private static void TestPartitionManager_ResizeRefusesOverlap()
    {
        IBlockDevice host = s_dev!;

        ResetHostGpt(host);
        Assert.True(Gpt.AddPartition(host, GptAlignedStartLba, ResizeOverlapSectorCount, Gpt.BasicDataPartitionType));
        Assert.True(Gpt.AddPartition(host, GptAlignedStartLba + ResizeOverlapSectorCount, ResizeOverlapSectorCount, Gpt.BasicDataPartitionType));
        Assert.True(
            PmResizeRefusedCleanly(
                new PartitionManager.PartitionLocation(GptAlignedStartLba, ResizeOverlapSectorCount),
                ResizeOverlapSectorCount * 2),
            "growing a GPT partition into its neighbour must be refused");
        List<GptPartitionEntry> gptParts = Gpt.Parse(host);
        Assert.Equal(2, gptParts.Count);
        Assert.Equal<ulong>(ResizeOverlapSectorCount, gptParts[0].SectorCount);

        ResetHostMbr(host);
        Assert.True(Mbr.AddPartition(host, 0, MbrLinuxSystemId, MbrPartAStartSector, MbrPartASectorCount));
        Assert.True(Mbr.AddPartition(host, 1, MbrFat32SystemId, MbrPartBStartSector, MbrPartBSectorCount));
        Assert.True(
            PmResizeRefusedCleanly(
                new PartitionManager.PartitionLocation(MbrPartAStartSector, MbrPartASectorCount),
                MbrPartBStartSector + MbrPartBSectorCount - MbrPartAStartSector),
            "growing an MBR primary into its neighbour must be refused, not thrown");
        List<MbrPartitionEntry> mbrParts = Mbr.Parse(host);
        Assert.Equal(2, mbrParts.Count);
        Assert.Equal<ulong>(MbrPartASectorCount, mbrParts[0].SectorCount);
    }

    // MoveWithData resolves the table entry and its constraints before
    // copying, so a false return is side-effect free. The extended container
    // is matched by the raw-table slot lookup but refused by the writer, so
    // asking afterwards copied the sectors first and then threw.
    private static void TestPartitionManager_MoveRefusesExtendedContainer()
    {
        IBlockDevice host = s_dev!;
        ResetHostExtendedMbr(host, ExtPartStartSector, ExtPartSectorCount);

        ulong destination = ExtPartStartSector + ExtPartSectorCount + ContainerMoveGapSectors;
        Span<byte> witness = new byte[host.BlockSize];
        FillPattern(witness, ContainerMoveWitnessSeed);
        host.WriteBlock(destination, 1, witness);

        Assert.True(
            PmMoveRefusedCleanly(
                new PartitionManager.PartitionLocation(ExtPartStartSector, ExtPartSectorCount),
                destination),
            "moving the extended container must be refused, not thrown");

        // The witness survives only if nothing was copied over it.
        AssertMovedPattern(host, destination, 1, ContainerMoveWitnessSeed);
    }

    // Both writers refuse a destination inside the table's own metadata, but
    // they run after CopySectors: the copy overwrote the table the writer was
    // about to re-read, and the call reported false having destroyed it.
    private static void TestPartitionManager_MoveRefusesTableSectors()
    {
        IBlockDevice host = s_dev!;

        ResetHostMbr(host);
        Assert.True(Mbr.AddPartition(host, 0, MbrLinuxSystemId, ExtPartStartSector, TableMoveSectorCount));
        Assert.True(
            PmMoveRefusedCleanly(new PartitionManager.PartitionLocation(ExtPartStartSector, TableMoveSectorCount), MbrLba),
            "a move onto the MBR sector must be refused");
        Assert.True(Mbr.IsMbr(host), "a refused move must leave the partition table intact");
        Assert.Equal(1, Mbr.Parse(host).Count);

        ResetHostGpt(host);
        Assert.True(Gpt.AddPartition(host, GptAlignedStartLba, TableMoveSectorCount, Gpt.BasicDataPartitionType));
        Assert.True(
            PmMoveRefusedCleanly(new PartitionManager.PartitionLocation(GptAlignedStartLba, TableMoveSectorCount), GptReservedDestinationLba),
            "a move into the GPT entry array must be refused");
        Assert.True(Gpt.IsGpt(host), "a refused move must leave the GPT intact");
        Assert.Equal(1, Gpt.Parse(host).Count);
    }

    // One try/catch per method on purpose (cf. MbrAddPartitionRejects).
    private static bool PmResizeRefusedCleanly(PartitionManager.PartitionLocation location, ulong newSectorCount)
    {
        try
        {
            return !PartitionManager.Resize(s_dev!, location, newSectorCount);
        }
        catch (Exception)
        {
            return false;
        }
    }

    // One try/catch per method on purpose (cf. MbrAddPartitionRejects):
    // true = the facade refused cleanly (false return, no throw, no side
    // effects claimed).
    private static bool PmMoveRefusedCleanly(PartitionManager.PartitionLocation location, ulong newStart)
    {
        try
        {
            return !PartitionManager.MoveWithData(s_dev!, location, newStart);
        }
        catch (Exception)
        {
            return false;
        }
    }

    // One try/catch per method on purpose (cf. MbrAddPartitionRejects).
    private static bool PmCreateRefusedCleanly(ulong start, ulong count)
    {
        try
        {
            return !PartitionManager.Create(s_dev!, start, count, MbrLinuxSystemId, Gpt.BasicDataPartitionType);
        }
        catch (Exception)
        {
            return false;
        }
    }

    private static bool MbrResizeRefused(int index, ulong newCount)
    {
        return !Mbr.ResizePartition(s_dev!, index, newCount);
    }

    private static bool MbrMoveRefused(int index, ulong newStart)
    {
        return !Mbr.MovePartition(s_dev!, index, newStart);
    }

    // One try/catch per method on purpose (cf. MbrAddPartitionRejects):
    // -1 marks "Parse threw", which every caller asserts against. Exception
    // (not ArgumentOutOfRangeException) because a past-end read surfaces
    // driver-specific errors — AHCI raises "SATA Fatal error: Command
    // aborted" and leaves the port wedged for every later test.
    private static int EbrParseCountSafe(IBlockDevice host)
    {
        try
        {
            return Ebr.Parse(host, ExtPartStartSector).Count;
        }
        catch (Exception)
        {
            return -1;
        }
    }

    private static void TestPartitionManager_CreateLogical()
    {
        IBlockDevice host = s_dev!;
        ResetHostExtendedMbr(host, ExtPartStartSector, ExtPartSectorCount);
        const uint countA = 100;
        const uint countB = 200;

        PartitionManager.TryCreateLogical(host, MbrLinuxSystemId, countA, out ulong logical0);
        Assert.True(logical0 != 0);

        PartitionManager.TryCreateLogical(host, MbrFat32SystemId, countB, out ulong logical1);
        Assert.True(logical1 != 0);

        List<MbrPartitionEntry> logicals = Ebr.Parse(host, ExtPartStartSector);
        Assert.Equal(2, logicals.Count);
        Assert.Equal<byte>(MbrLinuxSystemId, logicals[0].SystemId);
        Assert.Equal<byte>(MbrFat32SystemId, logicals[1].SystemId);
    }

    private static void TestPartitionManager_ResizeOnLogical()
    {
        IBlockDevice host = s_dev!;
        ResetHostExtendedMbr(host, ExtPartStartSector, ExtPartSectorCount);
        const uint count = 100;
        const ulong resized = 250;
        PartitionManager.TryCreateLogical(host, MbrLinuxSystemId, count, out ulong logicalStart);

        Assert.True(PartitionManager.Resize(
            host,
            new PartitionManager.PartitionLocation(logicalStart, count),
            resized));

        List<MbrPartitionEntry> logicals = Ebr.Parse(host, ExtPartStartSector);
        Assert.Equal(1, logicals.Count);
        Assert.Equal<ulong>(resized, logicals[0].SectorCount);
    }

    private static void TestPartitionManager_DeleteOnLogical()
    {
        IBlockDevice host = s_dev!;
        ResetHostExtendedMbr(host, ExtPartStartSector, ExtPartSectorCount);
        const uint countA = 100;
        const uint countB = 200;
        PartitionManager.TryCreateLogical(host, MbrLinuxSystemId, countA, out ulong l0);
        PartitionManager.TryCreateLogical(host, MbrLinuxSystemId, countB, out _);

        Assert.True(PartitionManager.Delete(
            host,
            new PartitionManager.PartitionLocation(l0, countA)));

        List<MbrPartitionEntry> logicals = Ebr.Parse(host, ExtPartStartSector);
        Assert.Equal(1, logicals.Count);
        Assert.Equal<ulong>(countB, logicals[0].SectorCount);
    }

    private static void TestPartitionManager_MoveOnLogical()
    {
        IBlockDevice host = s_dev!;
        ResetHostExtendedMbr(host, ExtPartStartSector, ExtPartSectorCount);
        const uint logicalCount = 32;
        const ulong moveDelta = 500;
        const uint seedBase = 0xCAFE0000;
        PartitionManager.TryCreateLogical(host, MbrLinuxSystemId, logicalCount, out ulong logicalStart);
        Assert.True(logicalStart != 0);

        Span<byte> patternSector = new byte[host.BlockSize];
        for (ulong lba = 0; lba < logicalCount; lba++)
        {
            FillPattern(patternSector, (uint)(seedBase + lba));
            host.WriteBlock(logicalStart + lba, 1, patternSector);
        }

        ulong newStart = logicalStart + moveDelta;
        Assert.True(PartitionManager.MoveWithData(
            host,
            new PartitionManager.PartitionLocation(logicalStart, logicalCount),
            newStart));

        List<MbrPartitionEntry> logicals = Ebr.Parse(host, ExtPartStartSector);
        Assert.Equal(1, logicals.Count);
        Assert.Equal<ulong>(newStart, logicals[0].StartSector);
        Assert.Equal<ulong>(logicalCount, logicals[0].SectorCount);

        AssertMovedPattern(host, newStart, logicalCount, seedBase);
    }

    // Wipes the label sectors and stamps a fresh MBR (empty table).
    private static void ResetHostMbr(IBlockDevice host)
    {
        Span<byte> wipe = new byte[host.BlockSize];
        host.WriteBlock(MbrLba, 1, wipe);
        host.WriteBlock(Gpt.PrimaryHeaderLba, 1, wipe);
        Mbr.Create(host);
    }

    // Wipes the label sectors and stamps a fresh GPT.
    private static void ResetHostGpt(IBlockDevice host)
    {
        Span<byte> wipe = new byte[host.BlockSize];
        host.WriteBlock(MbrLba, 1, wipe);
        host.WriteBlock(Gpt.PrimaryHeaderLba, 1, wipe);
        Gpt.Create(host);
    }

    // Wipes the label sectors plus the first EBR sector, then stamps a fresh
    // MBR whose slot 0 is an extended partition covering
    // [extStart, extStart + extCount).
    private static void ResetHostExtendedMbr(IBlockDevice host, uint extStart, uint extCount)
    {
        Span<byte> wipe = new byte[host.BlockSize];
        host.WriteBlock(MbrLba, 1, wipe);
        host.WriteBlock(Gpt.PrimaryHeaderLba, 1, wipe);
        host.WriteBlock(extStart, 1, wipe);
        Mbr.Create(host);
        Assert.True(Mbr.AddPartition(host, 0, MbrExtendedSystemId, extStart, extCount));
    }

    // Deterministic per-seed sector pattern for the move-with-data cells.
    private static void FillPattern(Span<byte> buffer, uint seed)
    {
        for (int i = 0; i < buffer.Length; i++)
        {
            buffer[i] = (byte)((seed + (uint)i * FillPatternByteStep) & ByteMask);
        }
    }

    // Re-derives the per-LBA pattern at the destination and compares every
    // byte — shared tail of the three move-with-data cells.
    private static void AssertMovedPattern(IBlockDevice host, ulong newStart, ulong count, uint seedBase)
    {
        Span<byte> readBuf = new byte[host.BlockSize];
        Span<byte> expected = new byte[host.BlockSize];
        for (ulong lba = 0; lba < count; lba++)
        {
            host.ReadBlock(newStart + lba, 1, readBuf);
            FillPattern(expected, (uint)(seedBase + lba));
            for (int i = 0; i < (int)host.BlockSize; i++)
            {
                Assert.Equal(expected[i], readBuf[i]);
            }
        }
    }

    // Hand-writes one EBR sector: a logical entry in slot 0 (start/count
    // relative to this EBR sector) and, optionally, a chain link in slot 1
    // (relative to the extended partition start).
    private static void WriteEbrSector(
        IBlockDevice device,
        ulong ebrLba,
        uint relativeStart,
        uint sectorCount,
        bool hasNext,
        uint nextRelativeLba)
    {
        Span<byte> sector = new byte[device.BlockSize];

        sector[MbrEntry0Offset + MbrEntryTypeOffset] = MbrLinuxSystemId;
        BitConverter.TryWriteBytes(sector.Slice(MbrEntry0Offset + MbrEntryStartLbaOffset, MbrLbaFieldBytes), relativeStart);
        BitConverter.TryWriteBytes(sector.Slice(MbrEntry0Offset + MbrEntrySectorCountOffset, MbrLbaFieldBytes), sectorCount);

        if (hasNext)
        {
            sector[MbrEntry1Offset + MbrEntryTypeOffset] = MbrExtendedSystemId;
            BitConverter.TryWriteBytes(sector.Slice(MbrEntry1Offset + MbrEntryStartLbaOffset, MbrLbaFieldBytes), nextRelativeLba);
        }

        BitConverter.TryWriteBytes(sector.Slice(MbrBootSigOffset, MbrBootSigSizeBytes), MbrBootSignature);

        device.WriteBlock(ebrLba, 1, sector);
    }

#if ARCH_X64
    // Physical address for the relocation probe: above 4 GiB, where Limine's
    // base-revision-0 blanket map (identity + HHDM of the low 4 GiB plus
    // memory-map regions) no longer covers anything, and clear of RAM and of
    // every fixed q35 window (ECAM, LAPIC, IO-APIC all sit below 4 GiB).
    private const ulong HighBarPhys = 0x1_1000_0000;

    /// <summary>Index of the NVMe controller's 64-bit register BAR (BAR0).</summary>
    private const int NvmeRegisterBarIndex = 0;

    /// <summary>Mask of the type bits in a memory BAR's lower dword (bits 0-2), composed from the PciDevice BAR field layout.</summary>
    private const uint PciBarTypeMask = (PciDevice.BarTypeMask << PciDevice.BarTypeShift) | PciDevice.BarIoSpaceMask;

    /// <summary>Type-bit value marking a 64-bit memory BAR.</summary>
    private const uint PciBar64BitMemoryType = PciDevice.BarType64Bit << PciDevice.BarTypeShift;

    /// <summary>Mask selecting the flag bits of a memory BAR's lower dword.</summary>
    private const uint PciBarFlagsMask = ~PciDevice.BarMemoryAddressMask;

    /// <summary>All-ones 32-bit MMIO read value, meaning nothing decodes the address.</summary>
    private const uint MmioAllOnesValue = 0xFFFF_FFFF;

    private static unsafe ulong HhdmOffset()
        => Limine.HHDM.Response != null ? Limine.HHDM.Response->Offset : 0;

    // Proves 64-bit BAR MMIO stays reachable when the BAR sits above 4 GiB,
    // where firmware on real hardware may place it: the HHDM alias of such a
    // BAR is unmapped until EnsureMmioMapped installs a page-table entry for
    // it. The cell relocates the NVMe controller's own BAR0 up there, mirrors
    // the driver-init access pattern (EnsureMmioMapped + phys-plus-HHDM
    // arithmetic) against the new address, and asserts the VS register reads
    // back identical — then restores the original BAR before returning. With
    // a no-op x64 EnsureMmioMapped this cell dies on an unhandled page fault.
    private static void TestMmio_HighBarRemapped()
    {
        PciDevice? nvmePci = PciManager.GetDeviceClass(ClassId.MassStorageController, SubclassId.NvmController);
        Assert.True(nvmePci != null, "an NVMe PCI function must exist on an nvme profile");

        uint barLow = nvmePci!.ReadRegister32((byte)Config.Bar0);
        uint barHigh = nvmePci.ReadRegister32((byte)Config.Bar1);
        Assert.True((barLow & PciBarTypeMask) == PciBar64BitMemoryType, "NVMe BAR0 must be a 64-bit memory BAR");

        ulong origPhys = ((ulong)barHigh << PciDevice.BarUpperHalfShift) | (barLow & PciDevice.BarMemoryAddressMask);
        ulong hhdm = HhdmOffset();
        uint vsOrig = Native.MMIO.Read32(origPhys + hhdm + NvmeRegisters.VsOffset);
        Assert.True(vsOrig != 0 && vsOrig != MmioAllOnesValue, "NVMe VS must read sane at the original BAR");

        // Quiesce decode while the BAR moves, like firmware would. No block
        // I/O is in flight (every I/O cell ran earlier), so nothing touches
        // the controller through the stale driver mapping meanwhile.
        ushort command = nvmePci.ReadRegister16((byte)Config.Command);
        nvmePci.WriteRegister16((byte)Config.Command, (ushort)(command & ~(ushort)PciCommand.Memory));
        nvmePci.WriteRegister32((byte)Config.Bar0, (uint)(HighBarPhys & PciDevice.BarMemoryAddressMask) | (barLow & PciBarFlagsMask));
        nvmePci.WriteRegister32((byte)Config.Bar1, (uint)(HighBarPhys >> PciDevice.BarUpperHalfShift));
        nvmePci.WriteRegister16((byte)Config.Command, command);

        uint vsHigh;
        try
        {
            PlatformHAL.Initializer?.EnsureMmioMapped(HighBarPhys);
            vsHigh = Native.MMIO.Read32(HighBarPhys + hhdm + NvmeRegisters.VsOffset);
        }
        finally
        {
            // Put the BAR back exactly as found so the destructive reboot
            // cell (and boot 1's scan) still see a working controller.
            nvmePci.WriteRegister16((byte)Config.Command, (ushort)(command & ~(ushort)PciCommand.Memory));
            nvmePci.WriteRegister32((byte)Config.Bar0, barLow);
            nvmePci.WriteRegister32((byte)Config.Bar1, barHigh);
            nvmePci.WriteRegister16((byte)Config.Command, command);
        }

        Assert.True(vsHigh == vsOrig, "VS read through the remapped high BAR must match the original");
    }

    // GetBar64Address must read BOTH halves of a 64-bit BAR from live
    // config space: mixing the enumeration-time cached lower half with a
    // live upper half splices two different addresses together the moment
    // a BAR is reprogrammed (exactly what the remap cell above — or any
    // future PCI resource allocator — does). Decode stays disabled for the
    // whole probe window: only config space is touched.
    private static void TestPciGetBar64ReadsLiveConfig()
    {
        PciDevice? nvmePci = PciManager.GetDeviceClass(ClassId.MassStorageController, SubclassId.NvmController);
        Assert.True(nvmePci != null, "an NVMe PCI function must exist on an nvme profile");

        uint barLow = nvmePci!.ReadRegister32((byte)Config.Bar0);
        uint barHigh = nvmePci.ReadRegister32((byte)Config.Bar1);
        ulong origPhys = ((ulong)barHigh << PciDevice.BarUpperHalfShift) | (barLow & PciDevice.BarMemoryAddressMask);
        Assert.True(nvmePci.GetBar64Address(NvmeRegisterBarIndex) == origPhys, "baseline: GetBar64Address must match raw config space");

        ushort command = nvmePci.ReadRegister16((byte)Config.Command);
        nvmePci.WriteRegister16((byte)Config.Command, (ushort)(command & ~(ushort)PciCommand.Memory));
        nvmePci.WriteRegister32((byte)Config.Bar0, (uint)(HighBarPhys & PciDevice.BarMemoryAddressMask) | (barLow & PciBarFlagsMask));
        nvmePci.WriteRegister32((byte)Config.Bar1, (uint)(HighBarPhys >> PciDevice.BarUpperHalfShift));

        ulong reported = nvmePci.GetBar64Address(NvmeRegisterBarIndex);

        nvmePci.WriteRegister32((byte)Config.Bar0, barLow);
        nvmePci.WriteRegister32((byte)Config.Bar1, barHigh);
        nvmePci.WriteRegister16((byte)Config.Command, command);

        Assert.True(reported == HighBarPhys,
            "GetBar64Address must read both halves live after a BAR reprogram, not splice cached low with live high");
    }
#endif

    // Contract-faithful degenerate device: one 512-byte block, throws on any
    // out-of-range access like real drivers do.
    private sealed class TinyDevice : BlockDevice
    {
        /// <summary>Single-block capacity of the degenerate probe: too small for even the GPT header at LBA 1.</summary>
        private const ulong TinyBlockCount = 1;

        public TinyDevice()
        {
            BlockSize = SectorSizeBytes;
            BlockCount = TinyBlockCount;
        }

        public override string Name => "tiny-probe";

        public override void ReadBlock(ulong blockNo, ulong blockCount, Span<byte> data)
        {
            if (blockNo > BlockCount || blockCount > BlockCount - blockNo)
            {
                throw new ArgumentOutOfRangeException(nameof(blockNo));
            }
            data.Clear();
        }

        public override void WriteBlock(ulong blockNo, ulong blockCount, ReadOnlySpan<byte> data)
        {
            if (blockNo > BlockCount || blockCount > BlockCount - blockNo)
            {
                throw new ArgumentOutOfRangeException(nameof(blockNo));
            }
        }
    }

    private sealed class BoundsProbeDevice : BlockDevice
    {
        /// <summary>Backing block count of the in-memory probe device.</summary>
        private const ulong ProbeBlockCount = 1024;

        public BoundsProbeDevice()
        {
            BlockSize = SectorSizeBytes;
            BlockCount = ProbeBlockCount;
        }

        public override string Name => "bounds-probe";

        public override void ReadBlock(ulong blockNo, ulong blockCount, Span<byte> data)
        {
        }

        public override void WriteBlock(ulong blockNo, ulong blockCount, ReadOnlySpan<byte> data)
        {
        }
    }
}
