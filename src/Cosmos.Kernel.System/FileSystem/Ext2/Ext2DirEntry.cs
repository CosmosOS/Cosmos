// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.System.FileSystem.Ext2;

/// <summary>
/// Parsed ext2 directory entry. <see cref="Offset"/> is the byte position of
/// the entry within the buffer that produced it.
/// </summary>
internal sealed class Ext2DirEntry
{
    /// <summary>Inode number of the entry; 0 marks a free slot.</summary>
    public uint Inode { get; }

    /// <summary>Record length: bytes from this entry to the next one.</summary>
    public ushort RecLen { get; }

    /// <summary>Name length in bytes.</summary>
    public byte NameLen { get; }

    /// <summary>File type (one of the <c>FileType*</c> values).</summary>
    public byte FileType { get; }

    /// <summary>Decoded entry name.</summary>
    public string Name { get; }

    /// <summary>Byte offset of the entry within the parsed directory data.</summary>
    public int Offset { get; }

    /// <summary>
    /// Creates a parsed directory entry.
    /// </summary>
    /// <param name="inode">Inode number of the entry.</param>
    /// <param name="recLen">Record length in bytes.</param>
    /// <param name="nameLen">Name length in bytes.</param>
    /// <param name="fileType">File type value.</param>
    /// <param name="name">Decoded entry name.</param>
    /// <param name="offset">Byte offset within the parsed directory data.</param>
    public Ext2DirEntry(uint inode, ushort recLen, byte nameLen, byte fileType, string name, int offset)
    {
        Inode = inode;
        RecLen = recLen;
        NameLen = nameLen;
        FileType = fileType;
        Name = name;
        Offset = offset;
    }
}
