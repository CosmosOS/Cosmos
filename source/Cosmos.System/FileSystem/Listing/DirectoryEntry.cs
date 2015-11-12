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
        public readonly ulong Size;
        public readonly string Name;
        public readonly FileSystem FileSystem;
        public readonly DirectoryEntry Parent;
        public readonly DirectoryEntryTypeEnum EntryType;

        protected DirectoryEntry(FileSystem aFileSystem, DirectoryEntry aParent, string aName, ulong aSize, DirectoryEntryTypeEnum aEntryType)
        {
            FileSystem = aFileSystem;
            Parent = aParent;
            EntryType = aEntryType;
            Name = aName;
            Size = aSize;
        }

        public abstract void SetName(string aName);

        public abstract void SetSize(Int64 aSize);
    }
}
