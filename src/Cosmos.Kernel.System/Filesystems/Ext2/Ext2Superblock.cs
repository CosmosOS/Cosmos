// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.Interfaces.Devices;
using Cosmos.Kernel.HAL.Vfs;
using global::System.Diagnostics.CodeAnalysis;

namespace Cosmos.Kernel.System.Filesystems.Ext2;

/// <summary>
/// Per-mount ext2 filesystem instance: parsed superblock, group descriptors,
/// block and inode allocators, and the logical-to-physical block mapper.
/// Block numbers coming from on-disk metadata are untrusted: mapping treats
/// anything outside the volume as a hole and never lets it drive I/O.
/// </summary>
internal sealed class Ext2Superblock : IVfsSuperblock
{
    private readonly IBlockDevice _device;

    /// <summary>Live inodes by number; keeps open handles coherent with metadata writes.</summary>
    private readonly Dictionary<uint, Ext2Inode> _inodeCache = [];

    /// <summary>Reusable buffer for indirect block reads (single and double first level).</summary>
    private byte[]? _indirBuf1;

    /// <summary>Reusable second-level buffer for the double-indirect path.</summary>
    private byte[]? _indirBuf2;

    /// <summary>Block number held by <see cref="_indirCacheData"/>; uint.MaxValue = invalid.</summary>
    private uint _indirCacheBlk = uint.MaxValue;

    /// <summary>Last-read indirect block contents; sequential reads through the same indirect block skip the disk.</summary>
    private byte[]? _indirCacheData;

    /// <summary>Reusable buffer for inode table reads.</summary>
    private byte[]? _inodeBuf;

    /// <summary>Total inodes in the volume (s_inodes_count).</summary>
    public uint InodesCount { get; }

    /// <summary>Total blocks in the volume (s_blocks_count).</summary>
    public uint BlocksCount { get; }

    /// <summary>Free blocks (s_free_blocks_count).</summary>
    public uint FreeBlocksCount { get; internal set; }

    /// <summary>Free inodes (s_free_inodes_count).</summary>
    public uint FreeInodesCount { get; internal set; }

    /// <summary>First block usable for data (s_first_data_block).</summary>
    public uint FirstDataBlock { get; }

    /// <summary>Block size shift (s_log_block_size).</summary>
    public uint LogBlockSize { get; }

    /// <summary>Fragment size shift (s_log_frag_size).</summary>
    public uint LogFragSize { get; }

    /// <summary>Blocks per group (s_blocks_per_group).</summary>
    public uint BlocksPerGroup { get; }

    /// <summary>Fragments per group (s_frags_per_group).</summary>
    public uint FragsPerGroup { get; }

    /// <summary>Inodes per group (s_inodes_per_group).</summary>
    public uint InodesPerGroup { get; }

    /// <summary>First non-reserved inode (s_first_ino).</summary>
    public uint FirstIno { get; }

    /// <summary>Inode structure size in bytes (s_inode_size).</summary>
    public ushort InodeSize { get; }

    /// <summary>Filesystem revision (s_rev_level).</summary>
    public uint RevLevel { get; }

    /// <summary>Compatible feature flags (s_feature_compat).</summary>
    public uint FeatureCompat { get; }

    /// <summary>Incompatible feature flags (s_feature_incompat).</summary>
    public uint FeatureIncompat { get; }

    /// <summary>Read-only compatible feature flags (s_feature_ro_compat).</summary>
    public uint FeatureRoCompat { get; }

    /// <summary>Block size in bytes, derived from the log shift.</summary>
    public uint BlockSize { get; }

    /// <summary>Fragment size in bytes, derived from the log shift.</summary>
    public uint FragsSize { get; }

    /// <summary>Number of block groups in the volume.</summary>
    public uint GroupsCount { get; }

    /// <summary>Reserved; mirrors the on-disk blocks-per-group geometry.</summary>
    public uint BlocksPerGroupActual { get; }

    /// <summary>Group descriptor table (flat array).</summary>
    private readonly Ext2GroupDesc[] _groups;

    /// <summary>Directory and metadata operations for this volume.</summary>
    public Ext2InodeOperations InodeOps { get; }

    /// <summary>Open-file byte I/O for this volume.</summary>
    public Ext2FileOperations FileOps { get; }

    /// <summary>Superblock-level callbacks for this volume.</summary>
    public Ext2SuperblockOperations SuperOps { get; }

    /// <summary>Root directory inode of this mount.</summary>
    public IVfsInode Root { get; }

    public ISuperblockOperations SuperOperations => SuperOps;

    long IVfsSuperblock.BlockSize => BlockSize;
    public ulong MaxNameLength => 255;

    /// <summary>The block device holding the volume.</summary>
    public IBlockDevice Device => _device;

    private Ext2Superblock(
        IBlockDevice device,
        uint inodesCount,
        uint blocksCount,
        uint freeBlocksCount,
        uint freeInodesCount,
        uint firstDataBlock,
        uint logBlockSize,
        uint logFragSize,
        uint blocksPerGroup,
        uint fragsPerGroup,
        uint inodesPerGroup,
        uint firstIno,
        ushort inodeSize,
        uint revLevel,
        uint featureCompat,
        uint featureIncompat,
        uint featureRoCompat,
        Ext2GroupDesc[] groups)
    {
        _device = device;
        InodesCount = inodesCount;
        BlocksCount = blocksCount;
        FreeBlocksCount = freeBlocksCount;
        FreeInodesCount = freeInodesCount;
        FirstDataBlock = firstDataBlock;
        LogBlockSize = logBlockSize;
        LogFragSize = logFragSize;
        BlocksPerGroup = blocksPerGroup;
        FragsPerGroup = fragsPerGroup;
        InodesPerGroup = inodesPerGroup;
        FirstIno = firstIno;
        InodeSize = inodeSize;
        RevLevel = revLevel;
        FeatureCompat = featureCompat;
        FeatureIncompat = featureIncompat;
        FeatureRoCompat = featureRoCompat;
        BlockSize = (uint)Ext2SuperblockLayout.BaseBlockSize << (int)logBlockSize;
        // A negative log shift halves the base size per step instead of doubling it.
        if ((int)logFragSize >= 0)
        {
            FragsSize = (uint)Ext2SuperblockLayout.BaseBlockSize << (int)logFragSize;
        }
        else
        {
            FragsSize = (uint)Ext2SuperblockLayout.BaseBlockSize >> (int)(-(int)logFragSize);
        }
        _groups = groups;
        GroupsCount = (uint)groups.Length;

        InodeOps = new Ext2InodeOperations(this);
        FileOps = new Ext2FileOperations(this);
        SuperOps = new Ext2SuperblockOperations();

        Ext2Inode root = ReadInode(Ext2SuperblockLayout.RootInodeNumber, "/");
        Root = root;
    }

    /// <summary>
    /// Probe <paramref name="device"/> for an ext2 volume and mount it.
    /// </summary>
    /// <param name="device">Block device holding the candidate volume.</param>
    /// <param name="superblock">Mounted volume on success; null on failure.</param>
    /// <returns>true when the device carries a valid, supported ext2 superblock.</returns>
    public static bool TryCreate(IBlockDevice device, [NotNullWhen(true)] out Ext2Superblock? superblock)
    {
        superblock = null;
        try
        {
            // The superblock sits at byte 1024, which may straddle device
            // blocks: round the read up to whole device blocks.
            ulong devBlockSize = device.BlockSize;
            ulong sbByteOffset = Ext2SuperblockLayout.SuperblockOffset;
            ulong sbLba = sbByteOffset / devBlockSize;
            ulong blocksToRead = (Ext2SuperblockLayout.SuperblockSize + devBlockSize - 1) / devBlockSize;
            ulong totalBytes = blocksToRead * devBlockSize;
            byte[] buf = new byte[totalBytes];
            device.ReadBlock(sbLba, blocksToRead, buf);
            int offsetInBuf = (int)(sbByteOffset % devBlockSize);
            ReadOnlySpan<byte> sb = buf.AsSpan(offsetInBuf, Ext2SuperblockLayout.SuperblockSize);

            ushort magic = BitConverter.ToUInt16(sb.Slice(Ext2SuperblockLayout.MagicOffset, 2));
            if (magic != Ext2SuperblockLayout.Magic)
            {
                return false;
            }

            uint inodesCount = BitConverter.ToUInt32(sb.Slice(Ext2SuperblockLayout.InodesCountOffset, 4));
            uint blocksCount = BitConverter.ToUInt32(sb.Slice(Ext2SuperblockLayout.BlocksCountOffset, 4));
            uint freeBlocks = BitConverter.ToUInt32(sb.Slice(Ext2SuperblockLayout.FreeBlocksCountOffset, 4));
            uint freeInodes = BitConverter.ToUInt32(sb.Slice(Ext2SuperblockLayout.FreeInodesCountOffset, 4));
            uint firstDataBlock = BitConverter.ToUInt32(sb.Slice(Ext2SuperblockLayout.FirstDataBlockOffset, 4));
            uint logBlockSize = BitConverter.ToUInt32(sb.Slice(Ext2SuperblockLayout.LogBlockSizeOffset, 4));
            uint logFragSize = BitConverter.ToUInt32(sb.Slice(Ext2SuperblockLayout.LogFragSizeOffset, 4));
            uint blocksPerGroup = BitConverter.ToUInt32(sb.Slice(Ext2SuperblockLayout.BlocksPerGroupOffset, 4));
            uint fragsPerGroup = BitConverter.ToUInt32(sb.Slice(Ext2SuperblockLayout.FragsPerGroupOffset, 4));
            uint inodesPerGroup = BitConverter.ToUInt32(sb.Slice(Ext2SuperblockLayout.InodesPerGroupOffset, 4));
            uint revLevel = BitConverter.ToUInt32(sb.Slice(Ext2SuperblockLayout.RevLevelOffset, 4));
            uint firstIno = BitConverter.ToUInt32(sb.Slice(Ext2SuperblockLayout.FirstInoOffset, 4));
            ushort inodeSize = BitConverter.ToUInt16(sb.Slice(Ext2SuperblockLayout.InodeSizeOffset, 2));
            uint featureCompat = BitConverter.ToUInt32(sb.Slice(Ext2SuperblockLayout.FeatureCompatOffset, 4));
            uint featureIncompat = BitConverter.ToUInt32(sb.Slice(Ext2SuperblockLayout.FeatureIncompatOffset, 4));
            uint featureRoCompat = BitConverter.ToUInt32(sb.Slice(Ext2SuperblockLayout.FeatureRoCompatOffset, 4));

            if (logBlockSize > Ext2SuperblockLayout.MaxLogBlockSize)
            {
                return false;
            }

            uint blockSize = (uint)Ext2SuperblockLayout.BaseBlockSize << (int)logBlockSize;
            // The parser never sees the device: reject block sizes the
            // device cannot address before any group-descriptor I/O.
            if (blockSize % devBlockSize != 0)
            {
                return false;
            }

            if (blocksPerGroup == 0 || inodesPerGroup == 0)
            {
                return false;
            }

            if (inodeSize == 0)
            {
                inodeSize = Ext2SuperblockLayout.DefaultInodeSize;
            }

            if (firstIno == 0)
            {
                firstIno = Ext2SuperblockLayout.DefaultFirstIno;
            }

            // Reject volumes whose claimed size leaves the device before
            // any group-descriptor I/O can hit the device's range guard.
            ulong devTotalBytes = device.BlockCount * devBlockSize;
            ulong fsBytes = (ulong)blocksCount * blockSize;
            if (fsBytes > devTotalBytes)
            {
                return false;
            }

            uint groupsCount = (blocksCount + blocksPerGroup - 1) / blocksPerGroup;
            uint groupsByInodes = (inodesCount + inodesPerGroup - 1) / inodesPerGroup;
            if (groupsByInodes > groupsCount)
            {
                groupsCount = groupsByInodes;
            }

            // The descriptor table starts in the block after the superblock:
            // block 2 for 1 KiB volumes (block 0 is boot, block 1 is the
            // superblock), block 1 otherwise.
            uint gdStartBlock = blockSize == 1024 ? 2u : 1u;
            uint gdBytes = groupsCount * (uint)Ext2SuperblockLayout.GroupDescSize;
            uint gdBlocks = (gdBytes + blockSize - 1) / blockSize;
            byte[] gdBuf = new byte[gdBlocks * blockSize];
            ReadBlocks(device, gdStartBlock, gdBlocks, blockSize, devBlockSize, gdBuf);

            Ext2GroupDesc[] groups = new Ext2GroupDesc[groupsCount];
            for (int i = 0; i < groupsCount; i++)
            {
                int off = i * Ext2SuperblockLayout.GroupDescSize;
                uint bBitmap = BitConverter.ToUInt32(gdBuf.AsSpan(off + Ext2SuperblockLayout.GroupDescBlockBitmapOffset, 4));
                uint iBitmap = BitConverter.ToUInt32(gdBuf.AsSpan(off + Ext2SuperblockLayout.GroupDescInodeBitmapOffset, 4));
                uint iTable = BitConverter.ToUInt32(gdBuf.AsSpan(off + Ext2SuperblockLayout.GroupDescInodeTableOffset, 4));
                ushort freeBlocksCount = BitConverter.ToUInt16(gdBuf.AsSpan(off + Ext2SuperblockLayout.GroupDescFreeBlocksCountOffset, 2));
                ushort freeInodesCount = BitConverter.ToUInt16(gdBuf.AsSpan(off + Ext2SuperblockLayout.GroupDescFreeInodesCountOffset, 2));
                ushort usedDirs = BitConverter.ToUInt16(gdBuf.AsSpan(off + Ext2SuperblockLayout.GroupDescUsedDirsCountOffset, 2));
                groups[i] = new Ext2GroupDesc(bBitmap, iBitmap, iTable, freeBlocksCount, freeInodesCount, usedDirs);
            }

            superblock = new Ext2Superblock(
                device,
                inodesCount, blocksCount, freeBlocks, freeInodes,
                firstDataBlock, logBlockSize, logFragSize,
                blocksPerGroup, fragsPerGroup, inodesPerGroup,
                firstIno, inodeSize, revLevel,
                featureCompat, featureIncompat, featureRoCompat,
                groups);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Read <paramref name="count"/> filesystem blocks into <paramref name="dest"/>.
    /// Out-of-range or boot-sector reads report zeros instead of throwing.
    /// </summary>
    /// <param name="device">Block device holding the volume.</param>
    /// <param name="ext2Block">First filesystem block number.</param>
    /// <param name="count">Blocks to read.</param>
    /// <param name="ext2BlockSize">Filesystem block size in bytes.</param>
    /// <param name="devBlockSize">Device block size in bytes.</param>
    /// <param name="dest">Destination buffer.</param>
    internal static void ReadBlocks(IBlockDevice device, uint ext2Block, uint count, uint ext2BlockSize, ulong devBlockSize, Span<byte> dest)
    {
        if (device is null || dest.Length == 0)
        {
            return;
        }

        if (ext2Block == 0 && count != 0)
        {
            // Block 0 is the boot sector; ext2 data never lives there.
            dest.Clear();
            return;
        }

        ulong byteOffset = (ulong)ext2Block * ext2BlockSize;
        ulong lba = byteOffset / devBlockSize;
        ulong devBlocks = (ulong)count * ext2BlockSize / devBlockSize;
        if (lba + devBlocks > device.BlockCount)
        {
            dest.Clear();
            return;
        }

        if ((ulong)dest.Length < (ulong)count * ext2BlockSize)
        {
            return;
        }

        device.ReadBlock(lba, devBlocks, dest);
    }

    /// <summary>
    /// Read <paramref name="count"/> blocks of this volume into <paramref name="dest"/>.
    /// </summary>
    /// <param name="ext2Block">First filesystem block number.</param>
    /// <param name="count">Blocks to read.</param>
    /// <param name="dest">Destination buffer.</param>
    internal void ReadBlocks(uint ext2Block, uint count, Span<byte> dest)
    {
        if (_device is null || dest.Length == 0)
        {
            dest.Clear();
            return;
        }

        if (ext2Block >= BlocksCount)
        {
            dest.Clear();
            return;
        }

        ReadBlocks(_device, ext2Block, count, BlockSize, _device.BlockSize, dest);
    }

    /// <summary>
    /// Write <paramref name="count"/> blocks of this volume. Out-of-range
    /// writes are dropped instead of throwing.
    /// </summary>
    /// <param name="ext2Block">First filesystem block number.</param>
    /// <param name="count">Blocks to write.</param>
    /// <param name="data">Source bytes.</param>
    internal void WriteBlocks(uint ext2Block, uint count, ReadOnlySpan<byte> data)
    {
        if (_device is null || data.Length == 0)
        {
            return;
        }

        if (ext2Block >= BlocksCount)
        {
            return;
        }

        ulong byteOffset = (ulong)ext2Block * BlockSize;
        ulong lba = byteOffset / _device.BlockSize;
        ulong devBlocks = (ulong)count * BlockSize / _device.BlockSize;
        if (lba + devBlocks > _device.BlockCount)
        {
            return;
        }

        _device.WriteBlock(lba, devBlocks, data);
    }

    /// <summary>
    /// Group descriptor for a block group index.
    /// </summary>
    /// <param name="groupIndex">Zero-based block group.</param>
    internal Ext2GroupDesc GetGroup(uint groupIndex) => _groups[groupIndex];

    /// <summary>
    /// Block group holding an inode number.
    /// </summary>
    /// <param name="inodeNumber">One-based inode number.</param>
    internal uint GroupOfInode(uint inodeNumber) => (inodeNumber - 1) / InodesPerGroup;

    /// <summary>
    /// Index of an inode number within its block group.
    /// </summary>
    /// <param name="inodeNumber">One-based inode number.</param>
    internal uint IndexInGroup(uint inodeNumber) => (inodeNumber - 1) % InodesPerGroup;

    /// <summary>
    /// Read an inode from its table, consulting the live-inode cache first.
    /// </summary>
    /// <param name="inodeNumber">One-based inode number.</param>
    /// <param name="name">Leaf name the caller resolved it under.</param>
    public Ext2Inode ReadInode(uint inodeNumber, string name)
    {
        if (inodeNumber == 0 || inodeNumber > InodesCount)
        {
            throw new ArgumentOutOfRangeException(nameof(inodeNumber));
        }

        if (_inodeCache.TryGetValue(inodeNumber, out Ext2Inode? cached))
        {
            // The cache is keyed by number, but a lookup may reach the same
            // inode under a new name (rename); refresh the leaf name.
            cached.Name = name;
            return cached;
        }

        uint group = GroupOfInode(inodeNumber);
        if (group >= GroupsCount)
        {
            throw new ArgumentOutOfRangeException(nameof(inodeNumber));
        }

        uint index = IndexInGroup(inodeNumber);
        Ext2GroupDesc gd = _groups[group];
        if (gd.InodeTable == 0 || gd.InodeTable >= BlocksCount)
        {
            throw new ArgumentOutOfRangeException(nameof(inodeNumber));
        }
        uint inodeTableBlock = gd.InodeTable;

        // Inode table is contiguous blocks.
        uint inodesPerBlock = BlockSize / InodeSize;
        uint blockOffset = index / inodesPerBlock;
        uint offsetInBlock = (index % inodesPerBlock) * InodeSize;

        if (_inodeBuf is null || _inodeBuf.Length != BlockSize)
        {
            _inodeBuf = new byte[BlockSize];
        }

        byte[] blockBuf = _inodeBuf;
        ReadBlocks(inodeTableBlock + blockOffset, 1, blockBuf);
        ReadOnlySpan<byte> raw = blockBuf.AsSpan((int)offsetInBlock, InodeSize);

        ushort mode = BitConverter.ToUInt16(raw.Slice(Ext2InodeLayout.ModeOffset, 2));
        ushort uid = BitConverter.ToUInt16(raw.Slice(Ext2InodeLayout.UidOffset, 2));
        uint size = BitConverter.ToUInt32(raw.Slice(Ext2InodeLayout.SizeOffset, 4));
        uint atime = BitConverter.ToUInt32(raw.Slice(Ext2InodeLayout.AtimeOffset, 4));
        uint ctime = BitConverter.ToUInt32(raw.Slice(Ext2InodeLayout.CtimeOffset, 4));
        uint mtime = BitConverter.ToUInt32(raw.Slice(Ext2InodeLayout.MtimeOffset, 4));
        uint dtime = BitConverter.ToUInt32(raw.Slice(Ext2InodeLayout.DtimeOffset, 4));
        ushort gid = BitConverter.ToUInt16(raw.Slice(Ext2InodeLayout.GidOffset, 2));
        ushort links = BitConverter.ToUInt16(raw.Slice(Ext2InodeLayout.LinksCountOffset, 2));
        uint blocks = BitConverter.ToUInt32(raw.Slice(Ext2InodeLayout.BlocksOffset, 4));
        uint flags = BitConverter.ToUInt32(raw.Slice(Ext2InodeLayout.FlagsOffset, 4));
        uint sizeHigh = 0;
        if (InodeSize >= 108 + 4)
        {
            sizeHigh = BitConverter.ToUInt32(raw.Slice(Ext2InodeLayout.DirAclOrSizeHighOffset, 4));
        }

        uint[] block = new uint[Ext2InodeLayout.BlockCount];
        for (int i = 0; i < Ext2InodeLayout.BlockCount; i++)
        {
            block[i] = BitConverter.ToUInt32(raw.Slice(Ext2InodeLayout.BlockOffset + i * 4, 4));
        }

        Ext2Inode inode = new(this, inodeNumber, name)
        {
            Mode = mode,
            Uid = uid,
            Gid = gid,
            Size = size,
            SizeHigh = sizeHigh,
            Atime = atime,
            Ctime = ctime,
            Mtime = mtime,
            Dtime = dtime,
            LinksCount = links,
            Blocks = blocks,
            Flags = flags,
            Block = block,
        };
        _inodeCache[inodeNumber] = inode;
        return inode;
    }

    /// <summary>
    /// Persist an inode's fields to its table slot (read-modify-write).
    /// </summary>
    /// <param name="inode">Inode to write back.</param>
    internal void WriteInode(Ext2Inode inode)
    {
        uint group = GroupOfInode(inode.InodeNumber);
        uint index = IndexInGroup(inode.InodeNumber);
        Ext2GroupDesc gd = _groups[group];
        uint inodeTableBlock = gd.InodeTable;
        uint inodesPerBlock = BlockSize / InodeSize;
        uint blockOffset = index / inodesPerBlock;
        uint offsetInBlock = (index % inodesPerBlock) * InodeSize;

        if (_inodeBuf is null || _inodeBuf.Length != BlockSize)
        {
            _inodeBuf = new byte[BlockSize];
        }

        byte[] blockBuf = _inodeBuf;
        ReadBlocks(inodeTableBlock + blockOffset, 1, blockBuf);
        Span<byte> raw = blockBuf.AsSpan((int)offsetInBlock, InodeSize);

        BitConverter.TryWriteBytes(raw.Slice(Ext2InodeLayout.ModeOffset, 2), inode.Mode);
        BitConverter.TryWriteBytes(raw.Slice(Ext2InodeLayout.UidOffset, 2), inode.Uid);
        BitConverter.TryWriteBytes(raw.Slice(Ext2InodeLayout.SizeOffset, 4), inode.Size);
        BitConverter.TryWriteBytes(raw.Slice(Ext2InodeLayout.AtimeOffset, 4), inode.Atime);
        BitConverter.TryWriteBytes(raw.Slice(Ext2InodeLayout.CtimeOffset, 4), inode.Ctime);
        BitConverter.TryWriteBytes(raw.Slice(Ext2InodeLayout.MtimeOffset, 4), inode.Mtime);
        BitConverter.TryWriteBytes(raw.Slice(Ext2InodeLayout.DtimeOffset, 4), inode.Dtime);
        BitConverter.TryWriteBytes(raw.Slice(Ext2InodeLayout.GidOffset, 2), inode.Gid);
        BitConverter.TryWriteBytes(raw.Slice(Ext2InodeLayout.LinksCountOffset, 2), inode.LinksCount);
        BitConverter.TryWriteBytes(raw.Slice(Ext2InodeLayout.BlocksOffset, 4), inode.Blocks);
        BitConverter.TryWriteBytes(raw.Slice(Ext2InodeLayout.FlagsOffset, 4), inode.Flags);
        if (InodeSize >= 108 + 4)
        {
            BitConverter.TryWriteBytes(raw.Slice(Ext2InodeLayout.DirAclOrSizeHighOffset, 4), inode.SizeHigh);
        }
        for (int i = 0; i < Ext2InodeLayout.BlockCount; i++)
        {
            BitConverter.TryWriteBytes(raw.Slice(Ext2InodeLayout.BlockOffset + i * 4, 4), inode.Block[i]);
        }

        WriteBlocks(inodeTableBlock + blockOffset, 1, blockBuf);
        _inodeCache[inode.InodeNumber] = inode;
    }

    /// <summary>
    /// Allocate a free inode, preferring <paramref name="groupHint"/>.
    /// </summary>
    /// <param name="groupHint">Block group to try first.</param>
    /// <param name="inodeNumber">Allocated one-based inode number on success.</param>
    /// <returns>true when an inode was allocated.</returns>
    internal bool TryAllocateInode(uint groupHint, out uint inodeNumber)
    {
        for (uint g = 0; g < GroupsCount; g++)
        {
            uint gi = (groupHint + g) % GroupsCount;
            Ext2GroupDesc gd = _groups[gi];
            if (gd.FreeInodesCount == 0)
            {
                continue;
            }

            byte[] bmp = new byte[BlockSize];
            ReadBlocks(gd.InodeBitmap, 1, bmp);
            uint max = InodesPerGroup;
            if (gi == GroupsCount - 1)
            {
                uint rem = InodesCount % InodesPerGroup;
                if (rem != 0)
                {
                    max = rem;
                }
            }

            for (uint i = 0; i < max; i++)
            {
                uint byteIdx = i / 8;
                uint bitIdx = i % 8;
                if ((bmp[byteIdx] & (1u << (int)bitIdx)) == 0)
                {
                    bmp[byteIdx] |= (byte)(1u << (int)bitIdx);
                    WriteBlocks(gd.InodeBitmap, 1, bmp);
                    gd.FreeInodesCount--;
                    UpdateGroupDesc(gi);
                    FreeInodesCount--;
                    inodeNumber = gi * InodesPerGroup + i + 1;
                    return true;
                }
            }
        }

        inodeNumber = 0;
        return false;
    }

    /// <summary>
    /// Release an inode back to its group's bitmap.
    /// </summary>
    /// <param name="inodeNumber">One-based inode number to free.</param>
    internal void FreeInode(uint inodeNumber)
    {
        uint group = GroupOfInode(inodeNumber);
        uint index = IndexInGroup(inodeNumber);
        Ext2GroupDesc gd = _groups[group];
        byte[] bmp = new byte[BlockSize];
        ReadBlocks(gd.InodeBitmap, 1, bmp);
        uint byteIdx = index / 8;
        uint bitIdx = index % 8;
        bmp[byteIdx] &= (byte)~(1u << (int)bitIdx);
        WriteBlocks(gd.InodeBitmap, 1, bmp);
        gd.FreeInodesCount++;
        UpdateGroupDesc(group);
        FreeInodesCount++;
        _inodeCache.Remove(inodeNumber);
    }

    /// <summary>
    /// Allocate a free block, preferring <paramref name="groupHint"/>. The
    /// block is zeroed before it is returned.
    /// </summary>
    /// <param name="groupHint">Block group to try first.</param>
    /// <param name="blockNumber">Allocated block number on success.</param>
    /// <returns>true when a block was allocated.</returns>
    internal bool TryAllocateBlock(uint groupHint, out uint blockNumber)
    {
        for (uint g = 0; g < GroupsCount; g++)
        {
            uint gi = (groupHint + g) % GroupsCount;
            Ext2GroupDesc gd = _groups[gi];
            if (gd.FreeBlocksCount == 0)
            {
                continue;
            }

            byte[] bmp = new byte[BlockSize];
            ReadBlocks(gd.BlockBitmap, 1, bmp);
            uint max = BlocksPerGroup;
            if (gi == GroupsCount - 1)
            {
                uint rem = BlocksCount % BlocksPerGroup;
                if (rem != 0)
                {
                    max = rem;
                }
            }

            for (uint i = 0; i < max; i++)
            {
                uint byteIdx = i / 8;
                uint bitIdx = i % 8;
                if ((bmp[byteIdx] & (1u << (int)bitIdx)) == 0)
                {
                    bmp[byteIdx] |= (byte)(1u << (int)bitIdx);
                    WriteBlocks(gd.BlockBitmap, 1, bmp);
                    gd.FreeBlocksCount--;
                    UpdateGroupDesc(gi);
                    FreeBlocksCount--;
                    blockNumber = gi * BlocksPerGroup + i;
                    byte[] zero = new byte[BlockSize];
                    WriteBlocks(blockNumber, 1, zero);
                    return true;
                }
            }
        }

        blockNumber = 0;
        return false;
    }

    /// <summary>
    /// Release a block back to its group's bitmap.
    /// </summary>
    /// <param name="blockNumber">Block number to free.</param>
    internal void FreeBlock(uint blockNumber)
    {
        uint group = blockNumber / BlocksPerGroup;
        uint index = blockNumber % BlocksPerGroup;
        if (group >= GroupsCount)
        {
            return;
        }

        Ext2GroupDesc gd = _groups[group];
        byte[] bmp = new byte[BlockSize];
        ReadBlocks(gd.BlockBitmap, 1, bmp);
        uint byteIdx = index / 8;
        uint bitIdx = index % 8;
        bmp[byteIdx] &= (byte)~(1u << (int)bitIdx);
        WriteBlocks(gd.BlockBitmap, 1, bmp);
        gd.FreeBlocksCount++;
        UpdateGroupDesc(group);
        FreeBlocksCount++;
    }

    /// <summary>
    /// Persist one group's descriptor and the volume's free counts.
    /// </summary>
    /// <param name="groupIndex">Zero-based block group.</param>
    private void UpdateGroupDesc(uint groupIndex)
    {
        uint gdStartBlock = BlockSize == 1024 ? 2u : 1u;
        uint gdBytes = GroupsCount * (uint)Ext2SuperblockLayout.GroupDescSize;
        uint gdBlocks = (gdBytes + BlockSize - 1) / BlockSize;
        byte[] gdBuf = new byte[gdBlocks * BlockSize];
        ReadBlocks(gdStartBlock, gdBlocks, gdBuf);
        int off = (int)groupIndex * Ext2SuperblockLayout.GroupDescSize;
        Ext2GroupDesc gd = _groups[groupIndex];
        BitConverter.TryWriteBytes(gdBuf.AsSpan(off + Ext2SuperblockLayout.GroupDescFreeBlocksCountOffset, 2), gd.FreeBlocksCount);
        BitConverter.TryWriteBytes(gdBuf.AsSpan(off + Ext2SuperblockLayout.GroupDescFreeInodesCountOffset, 2), gd.FreeInodesCount);
        BitConverter.TryWriteBytes(gdBuf.AsSpan(off + Ext2SuperblockLayout.GroupDescUsedDirsCountOffset, 2), gd.UsedDirsCount);
        BitConverter.TryWriteBytes(gdBuf.AsSpan(off + Ext2SuperblockLayout.GroupDescBlockBitmapOffset, 4), gd.BlockBitmap);
        BitConverter.TryWriteBytes(gdBuf.AsSpan(off + Ext2SuperblockLayout.GroupDescInodeBitmapOffset, 4), gd.InodeBitmap);
        BitConverter.TryWriteBytes(gdBuf.AsSpan(off + Ext2SuperblockLayout.GroupDescInodeTableOffset, 4), gd.InodeTable);
        WriteBlocks(gdStartBlock, gdBlocks, gdBuf);
        UpdateSuperblock();
    }

    /// <summary>
    /// Persist the volume's free block and inode counts to the superblock.
    /// </summary>
    internal void UpdateSuperblock()
    {
        ulong devBlockSize = _device.BlockSize;
        ulong sbByteOffset = Ext2SuperblockLayout.SuperblockOffset;
        ulong sbLba = sbByteOffset / devBlockSize;
        ulong blocksToRead = (Ext2SuperblockLayout.SuperblockSize + devBlockSize - 1) / devBlockSize;
        ulong totalBytes = blocksToRead * devBlockSize;
        byte[] buf = new byte[totalBytes];
        _device.ReadBlock(sbLba, blocksToRead, buf);
        int off = (int)(sbByteOffset % devBlockSize);
        Span<byte> sb = buf.AsSpan(off, Ext2SuperblockLayout.SuperblockSize);
        BitConverter.TryWriteBytes(sb.Slice(Ext2SuperblockLayout.FreeBlocksCountOffset, 4), FreeBlocksCount);
        BitConverter.TryWriteBytes(sb.Slice(Ext2SuperblockLayout.FreeInodesCountOffset, 4), FreeInodesCount);
        _device.WriteBlock(sbLba, blocksToRead, buf);
    }

    /// <summary>
    /// Tear down this mount: drop cached inodes and flush.
    /// </summary>
    public void Drop()
    {
        _inodeCache.Clear();
        InvalidateIndirCache();
        Flush();
    }

    /// <summary>
    /// Flush the device's volatile write cache — the durability point for sync and unmount.
    /// </summary>
    public void Flush()
    {
        _device.Flush();
        UpdateSuperblock();
    }

    /// <summary>
    /// Resolve a file-logical block to a physical block number, allocating
    /// through direct, single-indirect and double-indirect pointers when
    /// <paramref name="allocate"/> is set. Numbers outside the volume read
    /// as holes (0), never as device I/O.
    /// </summary>
    /// <param name="inode">File inode.</param>
    /// <param name="logicalBlock">Zero-based block index within the file.</param>
    /// <param name="allocate">Allocate a missing block instead of reporting a hole.</param>
    /// <param name="isNew">True when the call allocated the returned block.</param>
    /// <returns>Physical block number, or 0 for a hole or on failure.</returns>
    internal uint GetBlockPointer(Ext2Inode inode, uint logicalBlock, bool allocate, out bool isNew)
    {
        isNew = false;
        if (inode is null || inode.Block is null)
        {
            return 0;
        }

        if (BlockSize == 0)
        {
            return 0;
        }

        uint perBlock = BlockSize / 4;
        if (perBlock == 0)
        {
            return 0;
        }

        if (logicalBlock < Ext2InodeLayout.DirectBlockCount)
        {
            uint blk = inode.Block[logicalBlock];
            if (blk >= BlocksCount)
            {
                return 0;
            }

            if (blk == 0 && allocate)
            {
                if (!TryAllocateBlock(GroupOfInode(inode.InodeNumber), out uint nb))
                {
                    return 0;
                }

                inode.Block[logicalBlock] = nb;
                inode.Blocks += BlockSize / 512;
                isNew = true;
                return nb;
            }

            return blk;
        }

        logicalBlock -= Ext2InodeLayout.DirectBlockCount;

        if (logicalBlock < perBlock)
        {
            uint indir = inode.Block[Ext2InodeLayout.SingleIndirectIndex];
            if (indir == 0)
            {
                if (!allocate)
                {
                    return 0;
                }

                if (!TryAllocateBlock(GroupOfInode(inode.InodeNumber), out uint nb))
                {
                    return 0;
                }

                indir = nb;
                inode.Block[Ext2InodeLayout.SingleIndirectIndex] = nb;
                inode.Blocks += BlockSize / 512;
                byte[] zero = new byte[BlockSize];
                WriteBlocks(indir, 1, zero);
                isNew = true;
            }

            if (indir == 0 || indir >= BlocksCount)
            {
                return 0;
            }

            if (_indirBuf1 is null || _indirBuf1.Length != BlockSize)
            {
                _indirBuf1 = new byte[BlockSize];
            }

            byte[] blockBuf = _indirBuf1;
            if (_indirCacheData is not null && _indirCacheBlk == indir)
            {
                Buffer.BlockCopy(_indirCacheData, 0, blockBuf, 0, (int)BlockSize);
            }
            else
            {
                ReadBlocks(indir, 1, blockBuf);
                if (_indirCacheData is null || _indirCacheData.Length != BlockSize)
                {
                    _indirCacheData = new byte[BlockSize];
                }

                Buffer.BlockCopy(blockBuf, 0, _indirCacheData, 0, (int)BlockSize);
                _indirCacheBlk = indir;
            }

            uint entry = BitConverter.ToUInt32(blockBuf.AsSpan((int)logicalBlock * 4, 4));
            if (entry >= BlocksCount)
            {
                return 0;
            }

            if (entry == 0 && allocate)
            {
                if (!TryAllocateBlock(GroupOfInode(inode.InodeNumber), out uint nb))
                {
                    return 0;
                }

                BitConverter.TryWriteBytes(blockBuf.AsSpan((int)logicalBlock * 4, 4), nb);
                WriteBlocks(indir, 1, blockBuf);
                Buffer.BlockCopy(blockBuf, 0, _indirCacheData, 0, (int)BlockSize);
                _indirCacheBlk = indir;
                inode.Blocks += BlockSize / 512;
                byte[] zero = new byte[BlockSize];
                WriteBlocks(nb, 1, zero);
                return nb;
            }

            return entry;
        }

        logicalBlock -= perBlock;

        uint perBlockSq = perBlock * perBlock;
        if (logicalBlock < perBlockSq)
        {
            uint dindir = inode.Block[Ext2InodeLayout.DoubleIndirectIndex];
            if (dindir == 0)
            {
                if (!allocate)
                {
                    return 0;
                }

                if (!TryAllocateBlock(GroupOfInode(inode.InodeNumber), out uint nb))
                {
                    return 0;
                }

                dindir = nb;
                inode.Block[Ext2InodeLayout.DoubleIndirectIndex] = nb;
                inode.Blocks += BlockSize / 512;
                byte[] zero = new byte[BlockSize];
                WriteBlocks(dindir, 1, zero);
            }

            uint firstIdx = logicalBlock / perBlock;
            uint secondIdx = logicalBlock % perBlock;

            if (dindir == 0 || dindir >= BlocksCount)
            {
                return 0;
            }

            if (_indirBuf1 is null || _indirBuf1.Length != BlockSize)
            {
                _indirBuf1 = new byte[BlockSize];
            }

            byte[] firstBuf = _indirBuf1;
            if (_indirCacheData is not null && _indirCacheBlk == dindir)
            {
                Buffer.BlockCopy(_indirCacheData, 0, firstBuf, 0, (int)BlockSize);
            }
            else
            {
                ReadBlocks(dindir, 1, firstBuf);
                if (_indirCacheData is null || _indirCacheData.Length != BlockSize)
                {
                    _indirCacheData = new byte[BlockSize];
                }

                Buffer.BlockCopy(firstBuf, 0, _indirCacheData, 0, (int)BlockSize);
                _indirCacheBlk = dindir;
            }

            uint firstBlk = BitConverter.ToUInt32(firstBuf.AsSpan((int)firstIdx * 4, 4));
            if (firstBlk == 0)
            {
                if (!allocate)
                {
                    return 0;
                }

                if (!TryAllocateBlock(GroupOfInode(inode.InodeNumber), out uint nb))
                {
                    return 0;
                }

                firstBlk = nb;
                BitConverter.TryWriteBytes(firstBuf.AsSpan((int)firstIdx * 4, 4), nb);
                WriteBlocks(dindir, 1, firstBuf);
                Buffer.BlockCopy(firstBuf, 0, _indirCacheData, 0, (int)BlockSize);
                _indirCacheBlk = dindir;
                inode.Blocks += BlockSize / 512;
                byte[] zero = new byte[BlockSize];
                WriteBlocks(firstBlk, 1, zero);
            }

            if (_indirBuf2 is null || _indirBuf2.Length != BlockSize)
            {
                _indirBuf2 = new byte[BlockSize];
            }

            byte[] secondBuf = _indirBuf2;
            if (firstBlk == 0 || firstBlk >= BlocksCount)
            {
                return 0;
            }

            ReadBlocks(firstBlk, 1, secondBuf);
            uint entry = BitConverter.ToUInt32(secondBuf.AsSpan((int)secondIdx * 4, 4));
            if (entry >= BlocksCount)
            {
                return 0;
            }

            if (entry == 0 && allocate)
            {
                if (!TryAllocateBlock(GroupOfInode(inode.InodeNumber), out uint nb))
                {
                    return 0;
                }

                BitConverter.TryWriteBytes(secondBuf.AsSpan((int)secondIdx * 4, 4), nb);
                WriteBlocks(firstBlk, 1, secondBuf);
                inode.Blocks += BlockSize / 512;
                byte[] zero = new byte[BlockSize];
                WriteBlocks(nb, 1, zero);
                return nb;
            }

            return entry;
        }

        // Triple indirect would address gigabytes past the double range;
        // files that large are out of scope for this driver.
        return 0;
    }

    /// <summary>
    /// List the physical blocks of a file in logical order, stopping at the
    /// first hole.
    /// </summary>
    /// <param name="inode">File inode.</param>
    internal List<uint> CollectBlocks(Ext2Inode inode)
    {
        List<uint> result = [];
        uint blocksNeeded = (uint)((inode.FullSize + BlockSize - 1) / BlockSize);
        uint perBlock = BlockSize / 4;
        for (uint i = 0; i < blocksNeeded; i++)
        {
            uint blk = GetBlockPointer(inode, i, false, out _);
            if (blk == 0)
            {
                break;
            }

            result.Add(blk);
        }

        return result;
    }

    /// <summary>
    /// Shrink a file to <paramref name="newSize"/>, freeing blocks past the
    /// new end. Emptied indirect blocks leak (they are not reclaimed), which
    /// only costs space, not correctness.
    /// </summary>
    /// <param name="inode">File inode.</param>
    /// <param name="newSize">New size in bytes.</param>
    internal void Truncate(Ext2Inode inode, ulong newSize)
    {
        uint oldBlocks = (uint)((inode.FullSize + BlockSize - 1) / BlockSize);
        uint newBlocks = (uint)((newSize + BlockSize - 1) / BlockSize);
        if (newSize == 0)
        {
            newBlocks = 0;
        }

        if (newBlocks < oldBlocks)
        {
            for (uint i = newBlocks; i < oldBlocks; i++)
            {
                uint blk = GetBlockPointer(inode, i, false, out _);
                if (blk != 0)
                {
                    FreeBlock(blk);
                    SetBlockPointer(inode, i, 0);
                    inode.Blocks -= BlockSize / 512;
                }
            }
        }

        inode.Size = (uint)(newSize & 0xFFFFFFFF);
        inode.SizeHigh = (uint)(newSize >> 32);
        WriteInode(inode);
        InvalidateIndirCache();
    }

    /// <summary>
    /// Drops the indirect block cache. Called after any write that mutates
    /// indirect block contents outside <see cref="GetBlockPointer"/>.
    /// </summary>
    private void InvalidateIndirCache()
    {
        _indirCacheBlk = uint.MaxValue;
    }

    /// <summary>
    /// Clear a logical block pointer (used when truncating). Indirect slots
    /// are updated with a read-modify-write through the reusable buffers.
    /// </summary>
    /// <param name="inode">File inode.</param>
    /// <param name="logicalBlock">Zero-based block index within the file.</param>
    /// <param name="value">Pointer value to store.</param>
    private void SetBlockPointer(Ext2Inode inode, uint logicalBlock, uint value)
    {
        uint perBlock = BlockSize / 4;
        if (logicalBlock < Ext2InodeLayout.DirectBlockCount)
        {
            inode.Block[logicalBlock] = value;
            return;
        }

        logicalBlock -= Ext2InodeLayout.DirectBlockCount;
        if (logicalBlock < perBlock)
        {
            uint indir = inode.Block[Ext2InodeLayout.SingleIndirectIndex];
            if (indir == 0)
            {
                return;
            }

            if (_indirBuf1 is null || _indirBuf1.Length != BlockSize)
            {
                _indirBuf1 = new byte[BlockSize];
            }

            byte[] buf = _indirBuf1;
            ReadBlocks(indir, 1, buf);
            BitConverter.TryWriteBytes(buf.AsSpan((int)logicalBlock * 4, 4), value);
            WriteBlocks(indir, 1, buf);
            InvalidateIndirCache();
            return;
        }

        logicalBlock -= perBlock;
        uint perSq = perBlock * perBlock;
        if (logicalBlock < perSq)
        {
            uint dindir = inode.Block[Ext2InodeLayout.DoubleIndirectIndex];
            if (dindir == 0)
            {
                return;
            }

            uint firstIdx = logicalBlock / perBlock;
            uint secondIdx = logicalBlock % perBlock;
            if (_indirBuf1 is null || _indirBuf1.Length != BlockSize)
            {
                _indirBuf1 = new byte[BlockSize];
            }

            byte[] firstBuf = _indirBuf1;
            ReadBlocks(dindir, 1, firstBuf);
            uint firstBlk = BitConverter.ToUInt32(firstBuf.AsSpan((int)firstIdx * 4, 4));
            if (firstBlk == 0)
            {
                return;
            }

            if (_indirBuf2 is null || _indirBuf2.Length != BlockSize)
            {
                _indirBuf2 = new byte[BlockSize];
            }

            byte[] secondBuf = _indirBuf2;
            ReadBlocks(firstBlk, 1, secondBuf);
            BitConverter.TryWriteBytes(secondBuf.AsSpan((int)secondIdx * 4, 4), value);
            WriteBlocks(firstBlk, 1, secondBuf);
            InvalidateIndirCache();
        }
    }
}

/// <summary>
/// Parsed block group descriptor: bitmap and table locations plus the
/// group's free counters.
/// </summary>
internal sealed class Ext2GroupDesc
{
    /// <summary>Block holding the group's block bitmap.</summary>
    public uint BlockBitmap;

    /// <summary>Block holding the group's inode bitmap.</summary>
    public uint InodeBitmap;

    /// <summary>First block of the group's inode table.</summary>
    public uint InodeTable;

    /// <summary>Free blocks remaining in the group.</summary>
    public ushort FreeBlocksCount;

    /// <summary>Free inodes remaining in the group.</summary>
    public ushort FreeInodesCount;

    /// <summary>Directories in the group.</summary>
    public ushort UsedDirsCount;

    /// <summary>
    /// Creates a group descriptor.
    /// </summary>
    /// <param name="blockBitmap">Block holding the group's block bitmap.</param>
    /// <param name="inodeBitmap">Block holding the group's inode bitmap.</param>
    /// <param name="inodeTable">First block of the group's inode table.</param>
    /// <param name="freeBlocks">Free blocks remaining in the group.</param>
    /// <param name="freeInodes">Free inodes remaining in the group.</param>
    /// <param name="usedDirs">Directories in the group.</param>
    public Ext2GroupDesc(uint blockBitmap, uint inodeBitmap, uint inodeTable, ushort freeBlocks, ushort freeInodes, ushort usedDirs)
    {
        BlockBitmap = blockBitmap;
        InodeBitmap = inodeBitmap;
        InodeTable = inodeTable;
        FreeBlocksCount = freeBlocks;
        FreeInodesCount = freeInodes;
        UsedDirsCount = usedDirs;
    }
}
