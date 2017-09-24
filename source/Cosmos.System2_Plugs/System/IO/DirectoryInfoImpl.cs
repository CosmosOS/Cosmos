#define COSMOSDEBUG
using System;
using System.IO;
using System.Collections.Generic;
using Cosmos.Debug.Kernel;
using Cosmos.IL2CPU.API;
using Cosmos.IL2CPU.API.Attribs;
using Cosmos.System;
using Cosmos.System.FileSystem;
using Cosmos.System.FileSystem.Listing;
using Cosmos.System.FileSystem.VFS;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Cosmos.System_Plugs.System.IO
{
    /*
     * A lot (all?) of these plugs are not really needed they do exist here for bugs on IL2CPU when
     * interfaces are used
     */
    [Plug(Target = typeof(DirectoryInfo))]
    public static class DirectoryInfoImpl
    {

        public static bool get_Exists(DirectoryInfo aThis)
        {
            string aPath = aThis.FullName;
            Global.mFileSystemDebugger.SendInternal($"DirectoryInfo.Exists : aPath = {aPath}");
            return VFSManager.DirectoryExists(aPath);
        }

        /* The real implementation uses IEnumerable and do a conversion ToArray that crashes IL2CPU */
        public static DirectoryInfo[] GetDirectories(DirectoryInfo aThis)
        {
            Global.mFileSystemDebugger.SendInternal($"DirectoryInfo.GetDirectories() on path {aThis.FullName}");
            var xEntries = VFSManager.GetDirectoryListing(aThis.FullName);
            /*
             * GetDirectoryListing() returns Directories and Files so could allocate a little more than needed but
             * but I think this is better than to use a List and then convert it to an array
             */
            var result = new DirectoryInfo[xEntries.Count];

            for (int i = 0; i < xEntries.Count; i++)
            {
                if (xEntries[i].mEntryType == DirectoryEntryTypeEnum.Directory)
                {
                    result[i] = new DirectoryInfo(xEntries[i].mFullPath);
                }
            }

            return result;
        }

        /* The real implementation uses IEnumerable and do a conversion ToArray that crashes IL2CPU */
        public static FileInfo[] GetFiles(DirectoryInfo aThis)
        {
            Global.mFileSystemDebugger.SendInternal($"DirectoryInfo.GetFiles() on path {aThis.FullName}");
            var xEntries = VFSManager.GetDirectoryListing(aThis.FullName);
            /*
             * GetDirectoryListing() returns Directories and Files so could allocate a little more than needed but
             * but I think this is better than to use a List and then convert it to an array
             */
            var result = new FileInfo[xEntries.Count];

            for (int i = 0; i < xEntries.Count; i++)
            {
                Global.mFileSystemDebugger.SendInternal($"Found entry of type {(int)xEntries[i].mEntryType} and name {xEntries[i].mFullPath}");
                if (xEntries[i].mEntryType == DirectoryEntryTypeEnum.File)
                {
                    result[i] = new FileInfo(xEntries[i].mFullPath);
                }
            }

            return result;
        }
    }
}
