using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmos.System.FileSystem.Listing
{
    public enum DirectoryEntryTypeEnum
    {
        Directory,
        File,
        Unknown
    }

    public abstract class DirectoryEntry
    {
        public readonly UInt64 Size;
        public readonly string Name;
        public readonly FileSystem FileSystem;
        public readonly DirectoryEntryTypeEnum EntryType;

        protected DirectoryEntry(FileSystem aFileSystem, string aName, UInt64 aSize, DirectoryEntryTypeEnum aEntryType)
        {
            FileSystem = aFileSystem;
            EntryType = aEntryType;
            Name = aName;
            Size = aSize;
        }

        public abstract void SetName(string aName);

        public abstract void SetSize(Int64 aSize);
    }
}
