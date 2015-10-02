using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Cosmos.HAL.BlockDevice;
using Directory = Cosmos.System.FileSystem.Listing.Directory;
using File = Cosmos.System.FileSystem.Listing.File;

namespace Cosmos.System.FileSystem
{
    public enum FileSystemType
    {
        FAT,
        Unknown
    }

    public abstract class FileSystem
    {
        // Currently we map to the Windows scheme of single lettter: for drives. Cosmos will
        // NOT do this in the future, but it will be able to map paths to things that look like
        // drive letters for compatibility with Windows code.
        // For now we use Dictionary for simplicity, but in future this will change.
        //static protected Dictionary<string, FileSystem> mMappings = new Dictionary<string, FileSystem>();

        //static protected FileSystem mFS;

        //static public void AddMapping(string aPath, FileSystem aFileSystem)
        //{
        //    //mMappings.Add(aPath.ToUpper(), aFileSystem);
        //    // Dictionary<> doesnt work yet, so for now we just hack this and support only one FS
        //    mFS = aFileSystem;
        //}

        public static FileSystemType GetFileSystemType(Partition aDevice)
        {
            if (FAT.FatFileSystem.IsDeviceFAT(aDevice))
            {
                return FileSystemType.FAT;
            }

            return FileSystemType.Unknown;
        }

        public abstract List<Listing.Base> GetDirectoryListing(Directory baseDirectory);

        public abstract Directory GetRootDirectory(string name);
        public abstract Stream GetFileStream(File fileInfo);
    }
}
