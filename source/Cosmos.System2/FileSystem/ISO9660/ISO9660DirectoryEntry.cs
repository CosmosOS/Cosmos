using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Cosmos.System.FileSystem.ISO9660
{
    internal class ISO9660DirectoryEntry : DirectoryEntry
    {
        ISO9660FileSystem fs;
        internal ISO9660Directory internalEntry;

        public ISO9660DirectoryEntry(ISO9660Directory internalEntry, ISO9660FileSystem aFileSystem, ISO9660DirectoryEntry aParent, string aFullPath, string aName, long aSize, DirectoryEntryTypeEnum aEntryType) : base(aFileSystem, aParent, aFullPath, aName, aSize, aEntryType)
        {
            this.internalEntry = internalEntry;
            fs = aFileSystem;
        }
        public override Stream GetFileStream()
        {
            return new MemoryStream(fs.ReadFile(internalEntry));
        }

        public override long GetUsedSpace()
        {
            return internalEntry.FileSize;
        }

        public override void SetName(string aName)
        {
            throw new NotImplementedException("Read only file system");
        }

        public override void SetSize(long aSize)
        {
            throw new NotImplementedException("Read only file system");
        }
    }
    internal class ISO9660Directory
    {
        public byte Length;
        public byte ExtLength;
        public uint LBA;
        public uint FileSize;
        public byte FileFlags;

        public byte FileIdLen;
        public string FileID;
    }
}
