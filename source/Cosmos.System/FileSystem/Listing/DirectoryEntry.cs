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
            FatHelpers.Debug("-- DirectoryEntry.ctor : aaPrent.Name = " + aParent?.Name + ", aName = " + aName + ", aSize = " + aSize + " --");
            FileSystem = aFileSystem;
            Parent = aParent;
            EntryType = aEntryType;
            Name = aName;
            Size = aSize;
        }

        public abstract void SetName(string aName);

        public abstract void SetSize(long aSize);
    }
}
