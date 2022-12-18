//#define COSMOSDEBUG
using System.IO;
using IL2CPU.API.Attribs;
using Cosmos.System;
using Cosmos.System.FileSystem.Listing;
using Cosmos.System.FileSystem.VFS;
using System.Collections.Generic;

namespace Cosmos.System_Plugs.System.IO
{
    /*
     * A lot (all?) of these plugs are not really needed they do exist here for bugs on IL2CPU when
     * interfaces are used
     */
    [Plug(Target = typeof(DirectoryInfo))]
    public static class DirectoryInfoImpl
    {
        /* The real implementation uses IEnumerable and do a conversion ToArray that crashes IL2CPU */
        public static DirectoryInfo[] GetDirectories(DirectoryInfo aThis)
        {
            Global.mFileSystemDebugger.SendInternal($"DirectoryInfo.GetDirectories() on path {aThis.FullName}");
            var xEntries = VFSManager.GetDirectoryListing(aThis.FullName);

            //var result = new DirectoryInfo[xEntries.Count];
            var result = new List<DirectoryInfo>();

            for (int i = 0; i < xEntries.Count; i++)
            {
                if (xEntries[i].mEntryType == DirectoryEntryTypeEnum.Directory)
                {
                    result.Add(new DirectoryInfo(xEntries[i].mFullPath));
                }
            }

            return result.ToArray();
        }

        /* The real implementation uses IEnumerable and do a conversion ToArray that crashes IL2CPU */
        public static FileInfo[] GetFiles(DirectoryInfo aThis)
        {
            Global.mFileSystemDebugger.SendInternal($"DirectoryInfo.GetFiles() on path {aThis.FullName}");
            var xEntries = VFSManager.GetDirectoryListing(aThis.FullName);

            var result = new List<FileInfo>();

            for (int i = 0; i < xEntries.Count; i++)
            {
                Global.mFileSystemDebugger.SendInternal($"Found entry of type {(int)xEntries[i].mEntryType} and name {xEntries[i].mFullPath}");
                if (xEntries[i].mEntryType == DirectoryEntryTypeEnum.File)
                {
                    //result[i] = new FileInfo(xEntries[i].mFullPath);
                    result.Add(new FileInfo(xEntries[i].mFullPath));
                }
            }

            return result.ToArray();
        }

        public static FileSystemInfo[] GetFileSystemInfos(DirectoryInfo aThis)
        {
            Global.mFileSystemDebugger.SendInternal($"DirectoryInfo.GetFiles() on path {aThis.FullName}");
            var xEntries = VFSManager.GetDirectoryListing(aThis.FullName);

            var result = new List<FileSystemInfo>();

            for (int i = 0; i < xEntries.Count; i++)
            {
                Global.mFileSystemDebugger.SendInternal($"Found entry of type {(int)xEntries[i].mEntryType} and name {xEntries[i].mFullPath}");
                if (xEntries[i].mEntryType == DirectoryEntryTypeEnum.Directory)
                {
                    result.Add(new DirectoryInfo(xEntries[i].mFullPath));
                }
                if (xEntries[i].mEntryType == DirectoryEntryTypeEnum.File)
                {
                    result.Add(new FileInfo(xEntries[i].mFullPath));
                }
            }

            return result.ToArray();
        }
    }
}
