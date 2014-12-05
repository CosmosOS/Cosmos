using Cosmos.HAL.BlockDevice;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SentinelKernel.System.FileSystem
{
    public enum FileSystemType
    {
        FAT,
        Unknown
    }

    public class FileSystem
    {
        // Currently we map to the Windows scheme of single lettter: for drives. Cosmos will 
        // NOT do this in the future, but it will be able to map paths to things that look like
        // drive letters for compatibility with Windows code.
        // For now we use Dictionary for simplicity, but in future this will change.
        //static protected Dictionary<string, FileSystem> mMappings = new Dictionary<string, FileSystem>();

        static protected FileSystem mFS;

        static public void AddMapping(string aPath, FileSystem aFileSystem)
        {
            //mMappings.Add(aPath.ToUpper(), aFileSystem);
            // Dictionary<> doesnt work yet, so for now we just hack this and support only one FS
            mFS = aFileSystem;
        }

        public static FileSystemType GetFileSystemType(Partition aDevice)
        {
            if (FAT.FatFileSystem.IsDeviceFAT(aDevice))
            {
                return FileSystemType.FAT;
            }

            return FileSystemType.Unknown;
        }
    }
}