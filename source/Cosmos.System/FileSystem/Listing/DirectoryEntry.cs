namespace Cosmos.System.FileSystem.Listing
{
    using global::System.IO;

    public enum DirectoryEntryTypeEnum
    {
        Directory,
        File,
        Unknown
    }

    public abstract class DirectoryEntry
    {
        public readonly uint mSize;
        public readonly string mName;
        protected readonly FileSystem mFileSystem;
        public readonly DirectoryEntry mParent;
        public readonly DirectoryEntryTypeEnum mEntryType;

        protected DirectoryEntry(FileSystem aFileSystem, DirectoryEntry aParent, string aName, uint aSize, DirectoryEntryTypeEnum aEntryType)
        {
            mFileSystem = aFileSystem;
            mParent = aParent;
            mEntryType = aEntryType;
            mName = aName;
            mSize = aSize;
        }

        public abstract void SetName(string aName);

        public abstract void SetSize(long aSize);

        public abstract Stream GetFileStream();
    }
}
