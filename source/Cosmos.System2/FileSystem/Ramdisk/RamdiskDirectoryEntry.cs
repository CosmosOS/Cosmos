using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmos.System.FileSystem.Ramdisk
{
    public class RamdiskDirectoryEntry : DirectoryEntry
    {
        public FileSystem mFileSystem;
        public List<RamdiskDirectoryEntry> mEntries;
        public RamdiskDirectoryEntry _parent;
        public MemoryStream _stream;
        public DirectoryEntryTypeEnum _entryType;
        public RamdiskDirectoryEntry(FileSystem aFileSystem, DirectoryEntry aParent, string aFullPath, string aName, long aSize, DirectoryEntryTypeEnum aEntryType) : base(aFileSystem, aParent, aFullPath, aName, aSize, aEntryType)
        {
            mFileSystem = aFileSystem;
            _parent = (RamdiskDirectoryEntry)aParent;
            _parent.mEntries.Add(this);
            _entryType = aEntryType;
            _stream = new();
            mFullPath = aFullPath;
            mName = aName;
        }
        public override Stream GetFileStream() => _stream;
        public override long GetUsedSpace() => _stream.Capacity;
        public override void SetName(string aName)
        {
            mFullPath = mFullPath.Replace(mName, aName);
            mName = aName;
        }
        public override void SetSize(long aSize)
        {
            _stream.SetLength(aSize);
        }
    }
}
