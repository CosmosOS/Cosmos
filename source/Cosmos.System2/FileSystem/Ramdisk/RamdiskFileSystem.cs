using System;
using System.Collections.Generic;
using System.Linq;
using Cosmos.Core;
using Cosmos.HAL.BlockDevice;
using Cosmos.System.FileSystem.VFS;

namespace Cosmos.System.FileSystem.Ramdisk
{
    public class RamdiskFileSystem : FileSystem
    {
        public override long AvailableFreeSpace => (long)GCImplementation.GetAvailableRAM();
        public override long TotalFreeSpace => (long)GCImplementation.GetAvailableRAM();
        public override string Type => "Ramdisk";
        public override string Label { get; set; }
        public static List<RamdiskFileSystem> AllRamdisks = new();
        public RamdiskDirectoryEntry mEntry;
        public ulong mFileCount;
        public ulong mDirCount;
        public RamdiskFileSystem(Partition aDevice, string aRootPath, long aSize) : base(null, null, 0)
        {
            throw new ArgumentException("Ramdisk file system cannot be linked to partition. Use CreateRamdisk method instead");
        }
        private RamdiskFileSystem(string aRootPath) : base(null, aRootPath, CPU.GetAmountOfRAM() * 1024 * 1024)
        {
            mEntry = new RamdiskDirectoryEntry(this, null, RootPath, RootPath, Size, DirectoryEntryTypeEnum.Directory);
        }
        public static RamdiskFileSystem CreateRamdisk()
        {
            var rfs = new RamdiskFileSystem(VFSManager.GetNextFilesystemLetter());
            AllRamdisks.Add(rfs);
            return rfs;
        }
        public override DirectoryEntry CreateDirectory(DirectoryEntry aParentDirectory, string aNewDirectory)
        {
            if (aParentDirectory.mEntryType != DirectoryEntryTypeEnum.File)
            {
                RamdiskDirectoryEntry entry = new(this, aParentDirectory, aParentDirectory.mFullPath + $"/{aNewDirectory}", aNewDirectory, 0, DirectoryEntryTypeEnum.File);
                mDirCount++;
                return entry;
            }
            return null;
        }
        public override DirectoryEntry CreateFile(DirectoryEntry aParentDirectory, string aNewFile)
        {
            if (aParentDirectory.mEntryType != DirectoryEntryTypeEnum.File)
            {
                RamdiskDirectoryEntry entry = new(this, aParentDirectory, aParentDirectory.mFullPath + $"/{aNewFile}", aNewFile, 0, DirectoryEntryTypeEnum.File);
                mFileCount++;
                return entry;
            }
            return null;
        }
        public override void DeleteDirectory(DirectoryEntry aPath)
        {
            if (aPath != mEntry && aPath.mEntryType == DirectoryEntryTypeEnum.Directory)
            {
                ((RamdiskDirectoryEntry)aPath)._parent.mEntries.Remove((RamdiskDirectoryEntry)aPath);
                GCImplementation.Free(aPath);
            }
        }
        public override void DeleteFile(DirectoryEntry aPath)
        {
            if (aPath.mEntryType == DirectoryEntryTypeEnum.File)
            {
                ((RamdiskDirectoryEntry)aPath)._parent.mEntries.Remove((RamdiskDirectoryEntry)aPath);
                GCImplementation.Free(aPath);
            }
        }
        public override void DisplayFileSystemInfo()
        {
            global::System.Console.WriteLine($"=== Ramdisk Information \"{RootPath}\" ===");
            global::System.Console.WriteLine($"Created files       : {mFileCount}");
            global::System.Console.WriteLine($"Created directories : {mDirCount}");
        }
        public override void Format(string aDriveFormat, bool aQuick) => throw new InvalidOperationException("Ramdisk cannot be formatted");
        public override List<DirectoryEntry> GetDirectoryListing(DirectoryEntry baseDirectory)
        {
            var entries = mEntry.mEntries.ToList();
            var newlist = new List<DirectoryEntry>();
            foreach (var entry in entries)
            {
                newlist.Add(entry);
            }
            return newlist;
        }
        public override DirectoryEntry GetRootDirectory() => mEntry;
    }
}
