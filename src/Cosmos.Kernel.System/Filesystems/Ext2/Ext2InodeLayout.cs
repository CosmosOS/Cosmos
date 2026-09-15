// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.System.Filesystems.Ext2;

/// <summary>
/// On-disk layout constants for ext2 inodes and directory entries.
/// </summary>
internal static class Ext2InodeLayout
{
    // Inode field offsets within the inode structure.

    /// <summary>Byte offset of i_mode: file type plus permission bits (16-bit).</summary>
    public const int ModeOffset = 0;

    /// <summary>Byte offset of i_uid: owner user id, low 16 bits (16-bit).</summary>
    public const int UidOffset = 2;

    /// <summary>Byte offset of i_size: file size in bytes, low 32 bits (32-bit).</summary>
    public const int SizeOffset = 4;

    /// <summary>Byte offset of i_atime: last access time, Unix seconds (32-bit).</summary>
    public const int AtimeOffset = 8;

    /// <summary>Byte offset of i_ctime: inode change time, Unix seconds (32-bit).</summary>
    public const int CtimeOffset = 12;

    /// <summary>Byte offset of i_mtime: last modification time, Unix seconds (32-bit).</summary>
    public const int MtimeOffset = 16;

    /// <summary>Byte offset of i_dtime: deletion time, Unix seconds (32-bit).</summary>
    public const int DtimeOffset = 20;

    /// <summary>Byte offset of i_gid: owner group id, low 16 bits (16-bit).</summary>
    public const int GidOffset = 24;

    /// <summary>Byte offset of i_links_count: hard link count (16-bit).</summary>
    public const int LinksCountOffset = 26;

    /// <summary>Byte offset of i_blocks: 512-byte sectors allocated to the inode (32-bit).</summary>
    public const int BlocksOffset = 28;

    /// <summary>Byte offset of i_flags (32-bit).</summary>
    public const int FlagsOffset = 32;

    /// <summary>Byte offset of osd1: OS-dependent field (4 bytes).</summary>
    public const int Osd1Offset = 36;

    /// <summary>Byte offset of i_block: 15 block pointers (direct, indirect, double, triple).</summary>
    public const int BlockOffset = 40;

    /// <summary>Byte offset of i_generation: file version for NFS (32-bit).</summary>
    public const int GenerationOffset = 100;

    /// <summary>Byte offset of i_file_acl: file access control list block (32-bit).</summary>
    public const int FileAclOffset = 104;

    /// <summary>Byte offset of i_dir_acl: directory ACL block, or the high 32 bits of the size for regular files (32-bit).</summary>
    public const int DirAclOrSizeHighOffset = 108;

    /// <summary>Byte offset of i_faddr: fragment address (1 byte).</summary>
    public const int FaddrOffset = 112;

    /// <summary>Byte offset of osd2: OS-dependent field (12 bytes).</summary>
    public const int Osd2Offset = 115;

    /// <summary>Block pointers per inode (12 direct + single + double + triple indirect).</summary>
    public const int BlockCount = 15;

    /// <summary>Direct block pointers per inode.</summary>
    public const int DirectBlockCount = 12;

    /// <summary>Index into i_block of the single-indirect pointer.</summary>
    public const int SingleIndirectIndex = 12;

    /// <summary>Index into i_block of the double-indirect pointer.</summary>
    public const int DoubleIndirectIndex = 13;

    /// <summary>Index into i_block of the triple-indirect pointer (not resolved by this driver).</summary>
    public const int TripleIndirectIndex = 14;

    /// <summary>Longest symlink target stored inline in i_block (60 bytes).</summary>
    public const int InlineSymlinkMax = 60;

    // Mode file-type bits (mirrors Unix S_IFMT).

    /// <summary>Mask isolating the file-type nibble of i_mode.</summary>
    public const ushort IFMT = 0xF000;

    /// <summary>Socket file type.</summary>
    public const ushort IFSOCK = 0xC000;

    /// <summary>Symbolic link file type.</summary>
    public const ushort IFLNK = 0xA000;

    /// <summary>Regular file type.</summary>
    public const ushort IFREG = 0x8000;

    /// <summary>Block device type.</summary>
    public const ushort IFBLK = 0x6000;

    /// <summary>Directory type.</summary>
    public const ushort IFDIR = 0x4000;

    /// <summary>Character device type.</summary>
    public const ushort IFCHR = 0x2000;

    /// <summary>Named pipe type.</summary>
    public const ushort IFIFO = 0x1000;

    // Directory entry layout.

    /// <summary>Byte offset of the inode number within a directory entry (32-bit).</summary>
    public const int DirEntryInodeOffset = 0;

    /// <summary>Byte offset of rec_len within a directory entry (16-bit).</summary>
    public const int DirEntryRecLenOffset = 4;

    /// <summary>Byte offset of name_len within a directory entry.</summary>
    public const int DirEntryNameLenOffset = 6;

    /// <summary>Byte offset of file_type within a directory entry.</summary>
    public const int DirEntryFileTypeOffset = 7;

    /// <summary>Byte offset of the name bytes within a directory entry.</summary>
    public const int DirEntryNameOffset = 8;

    /// <summary>Smallest legal rec_len: the fixed 8-byte entry header.</summary>
    public const int DirEntryMinRecLen = 8;

    /// <summary>Directory entries are padded to a multiple of this many bytes.</summary>
    public const int DirEntryAlignment = 4;

    // File type values for dir entries (when feature supports it).

    /// <summary>Unknown file type.</summary>
    public const byte FileTypeUnknown = 0;

    /// <summary>Regular file entry.</summary>
    public const byte FileTypeRegFile = 1;

    /// <summary>Directory entry.</summary>
    public const byte FileTypeDir = 2;

    /// <summary>Character device entry.</summary>
    public const byte FileTypeChrDev = 3;

    /// <summary>Block device entry.</summary>
    public const byte FileTypeBlkDev = 4;

    /// <summary>Named pipe entry.</summary>
    public const byte FileTypeFifo = 5;

    /// <summary>Socket entry.</summary>
    public const byte FileTypeSock = 6;

    /// <summary>Symbolic link entry.</summary>
    public const byte FileTypeSymlink = 7;
}
