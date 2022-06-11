using System;
using System.IO;
using Cosmos.System.FileSystem.Listing;

namespace Cosmos.System.FileSystem.ISO9660;

internal class ISO9660DirectoryEntry : DirectoryEntry
{
    private readonly ISO9660FileSystem fs;
    internal ISO9660Directory internalEntry;

    public ISO9660DirectoryEntry(ISO9660Directory internalEntry, ISO9660FileSystem aFileSystem,
        ISO9660DirectoryEntry aParent, string aFullPath, string aName, long aSize, DirectoryEntryTypeEnum aEntryType) :
        base(aFileSystem, aParent, aFullPath, aName, aSize, aEntryType)
    {
        this.internalEntry = internalEntry;
        fs = aFileSystem;
    }

    public override Stream GetFileStream() => new MemoryStream(fs.ReadFile(internalEntry));

    public override long GetUsedSpace() => internalEntry.FileSize;

    public override void SetName(string aName) => throw new NotImplementedException("Read only file system");

    public override void SetSize(long aSize) => throw new NotImplementedException("Read only file system");
}

internal class ISO9660Directory
{
    public byte ExtLength;
    public byte FileFlags;
    public string FileID;

    public byte FileIdLen;
    public uint FileSize;
    public uint LBA;
    public byte Length;
}
