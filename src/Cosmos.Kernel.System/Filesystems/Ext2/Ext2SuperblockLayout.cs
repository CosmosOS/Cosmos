// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.System.Filesystems.Ext2;

/// <summary>
/// On-disk layout constants for the ext2 superblock and block group
/// descriptors. Field offsets match the ext2 specification (all
/// multi-byte fields are little-endian on disk).
/// </summary>
internal static class Ext2SuperblockLayout
{
    /// <summary>Byte offset of the superblock within the volume (always 1024, regardless of block size).</summary>
    public const int SuperblockOffset = 1024;

    /// <summary>Superblock size in bytes.</summary>
    public const int SuperblockSize = 1024;

    /// <summary>Superblock magic value identifying an ext2 volume (0xEF53).</summary>
    public const ushort Magic = 0xEF53;

    // Superblock field offsets (little-endian).

    /// <summary>Byte offset of s_inodes_count: total inodes in the volume (32-bit).</summary>
    public const int InodesCountOffset = 0;

    /// <summary>Byte offset of s_blocks_count: total blocks in the volume (32-bit).</summary>
    public const int BlocksCountOffset = 4;

    /// <summary>Byte offset of s_r_blocks_count: blocks reserved for the superuser (32-bit).</summary>
    public const int RBlocksCountOffset = 8;

    /// <summary>Byte offset of s_free_blocks_count (32-bit).</summary>
    public const int FreeBlocksCountOffset = 12;

    /// <summary>Byte offset of s_free_inodes_count (32-bit).</summary>
    public const int FreeInodesCountOffset = 16;

    /// <summary>Byte offset of s_first_data_block: first block usable for data (32-bit; 1 for 1 KiB blocks, 0 otherwise).</summary>
    public const int FirstDataBlockOffset = 20;

    /// <summary>Byte offset of s_log_block_size: block size is 1024 shifted left by this value (32-bit).</summary>
    public const int LogBlockSizeOffset = 24;

    /// <summary>Byte offset of s_log_frag_size: fragment size shift, same encoding as the block size (32-bit).</summary>
    public const int LogFragSizeOffset = 28;

    /// <summary>Byte offset of s_blocks_per_group (32-bit).</summary>
    public const int BlocksPerGroupOffset = 32;

    /// <summary>Byte offset of s_frags_per_group (32-bit).</summary>
    public const int FragsPerGroupOffset = 36;

    /// <summary>Byte offset of s_inodes_per_group (32-bit).</summary>
    public const int InodesPerGroupOffset = 40;

    /// <summary>Byte offset of s_mtime: last mount time, Unix seconds (32-bit).</summary>
    public const int MtimeOffset = 44;

    /// <summary>Byte offset of s_wtime: last write time, Unix seconds (32-bit).</summary>
    public const int WtimeOffset = 48;

    /// <summary>Byte offset of s_mnt_count: mounts since the last check (16-bit).</summary>
    public const int MntCountOffset = 52;

    /// <summary>Byte offset of s_max_mnt_count: mounts allowed before a check is forced (16-bit).</summary>
    public const int MaxMntCountOffset = 54;

    /// <summary>Byte offset of s_magic (16-bit).</summary>
    public const int MagicOffset = 56;

    /// <summary>Byte offset of s_state: filesystem state (16-bit).</summary>
    public const int StateOffset = 58;

    /// <summary>Byte offset of s_errors: behavior on error detection (16-bit).</summary>
    public const int ErrorsOffset = 60;

    /// <summary>Byte offset of s_minor_rev_level (16-bit).</summary>
    public const int MinorRevLevelOffset = 62;

    /// <summary>Byte offset of s_lastcheck: last consistency check, Unix seconds (32-bit).</summary>
    public const int LastCheckOffset = 64;

    /// <summary>Byte offset of s_checkinterval: seconds allowed between checks (32-bit).</summary>
    public const int CheckIntervalOffset = 68;

    /// <summary>Byte offset of s_creator_os: ID of the OS that created the volume (32-bit).</summary>
    public const int CreatorOsOffset = 72;

    /// <summary>Byte offset of s_rev_level: 0 for the original format, 1 for dynamic revisions (32-bit).</summary>
    public const int RevLevelOffset = 76;

    /// <summary>Byte offset of s_def_resuid: default uid for reserved blocks (16-bit).</summary>
    public const int DefResUidOffset = 80;

    /// <summary>Byte offset of s_def_resgid: default gid for reserved blocks (16-bit).</summary>
    public const int DefResGidOffset = 82;

    /// <summary>Byte offset of s_first_ino: first non-reserved inode (32-bit).</summary>
    public const int FirstInoOffset = 84;

    /// <summary>Byte offset of s_inode_size: inode structure size in bytes (16-bit).</summary>
    public const int InodeSizeOffset = 88;

    /// <summary>Byte offset of s_block_group_nr: block group holding this superblock copy (16-bit).</summary>
    public const int BlockGroupNrOffset = 90;

    /// <summary>Byte offset of s_feature_compat: compatible feature flags (32-bit).</summary>
    public const int FeatureCompatOffset = 92;

    /// <summary>Byte offset of s_feature_incompat: incompatible feature flags (32-bit).</summary>
    public const int FeatureIncompatOffset = 96;

    /// <summary>Byte offset of s_feature_ro_compat: read-only compatible feature flags (32-bit).</summary>
    public const int FeatureRoCompatOffset = 100;

    /// <summary>Byte offset of s_uuid: 128-bit volume identifier (16 bytes).</summary>
    public const int UuidOffset = 104;

    /// <summary>Byte offset of s_volume_name: null-padded label (16 bytes).</summary>
    public const int VolumeNameOffset = 120;

    /// <summary>Byte offset of s_last_mounted: null-padded last mount point (64 bytes).</summary>
    public const int LastMountedOffset = 136;

    /// <summary>Byte offset of s_algo_bitmap: compression algorithms in use (32-bit).</summary>
    public const int AlgoBitmapOffset = 200;

    /// <summary>Base block size before the log shift (1024 bytes).</summary>
    public const int BaseBlockSize = 1024;

    /// <summary>Minimum s_log_block_size value (0 selects 1024-byte blocks).</summary>
    public const uint MinLogBlockSize = 0;

    /// <summary>Maximum s_log_block_size this driver accepts (2 selects 4096-byte blocks).</summary>
    public const uint MaxLogBlockSize = 2;

    /// <summary>Inode number of the root directory.</summary>
    public const uint RootInodeNumber = 2;

    /// <summary>First non-reserved inode number the formatter stamps into a fresh volume.</summary>
    public const uint DefaultFirstIno = 11;

    /// <summary>Revision 0: original fixed 128-byte inodes.</summary>
    public const uint RevOriginal = 0;

    /// <summary>Revision 1: dynamic revision with a variable inode size.</summary>
    public const uint RevDynamic = 1;

    /// <summary>Default inode size for revision 0 volumes (128 bytes).</summary>
    public const ushort DefaultInodeSize = 128;

    /// <summary>Block group descriptor size in bytes.</summary>
    public const int GroupDescSize = 32;

    /// <summary>Byte offset of bg_block_bitmap within a group descriptor (32-bit).</summary>
    public const int GroupDescBlockBitmapOffset = 0;

    /// <summary>Byte offset of bg_inode_bitmap within a group descriptor (32-bit).</summary>
    public const int GroupDescInodeBitmapOffset = 4;

    /// <summary>Byte offset of bg_inode_table within a group descriptor (32-bit).</summary>
    public const int GroupDescInodeTableOffset = 8;

    /// <summary>Byte offset of bg_free_blocks_count within a group descriptor (16-bit).</summary>
    public const int GroupDescFreeBlocksCountOffset = 12;

    /// <summary>Byte offset of bg_free_inodes_count within a group descriptor (16-bit).</summary>
    public const int GroupDescFreeInodesCountOffset = 14;

    /// <summary>Byte offset of bg_used_dirs_count within a group descriptor (16-bit).</summary>
    public const int GroupDescUsedDirsCountOffset = 16;
}
