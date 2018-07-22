using System.Collections.Generic;
using Cosmos.System.FileSystem.Listing;
using System.IO;

namespace Cosmos.System.FileSystem.VFS
{
    public abstract class VFSBase
    {
        public abstract void Initialize();

        public abstract void RegisterFileSystem(FileSystemFactory aFileSystemFactory);

        public abstract DirectoryEntry CreateFile(string aPath);

        public abstract DirectoryEntry CreateDirectory(string aPath);

        public abstract bool DeleteFile(DirectoryEntry aPath);

        public abstract bool DeleteDirectory(DirectoryEntry aPath);

        public abstract DirectoryEntry GetDirectory(string aPath);

        public abstract DirectoryEntry GetFile(string aPath);

        public abstract List<DirectoryEntry> GetDirectoryListing(string aPath);

        public abstract List<DirectoryEntry> GetDirectoryListing(DirectoryEntry aEntry);

        public abstract DirectoryEntry GetVolume(string aVolume);

        public abstract List<DirectoryEntry> GetVolumes();

        public abstract FileAttributes GetFileAttributes(string aPath);

        public abstract void SetFileAttributes(string aPath, FileAttributes fileAttributes);

        public static char DirectorySeparatorChar { get { return '\\'; } }

        public static char AltDirectorySeparatorChar { get { return '/'; } }

        public static char VolumeSeparatorChar { get { return ':'; } }

        public abstract bool IsValidDriveId(string driveId);

        public abstract long GetTotalSize(string aDriveId);

        public abstract long GetAvailableFreeSpace(string aDriveId);

        public abstract long GetTotalFreeSpace(string aDriveId);

        public abstract string GetFileSystemType(string aDriveId);

        public abstract string GetFileSystemLabel(string aDriveId);

        public abstract void SetFileSystemLabel(string aDriveId, string aLabel);

        public abstract void Format(string aDriveId, string aDriveFormat, bool aQuick);
    }
}
