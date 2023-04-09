using Cosmos.System.FileSystem.Listing;
using Cosmos.System.FileSystem.VFS;
using Cosmos.System;
using IL2CPU.API.Attribs;

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
        public static DirectoryInfo[] GetDirectories(DirectoryInfo directory)
        {
            Global.FileSystemDebugger.SendInternal($"DirectoryInfo.GetDirectories() on path '{directory.FullName}'.");
            var xEntries = VFSManager.GetDirectoryListing(directory.FullName);

            //var result = new DirectoryInfo[xEntries.Count];
            List<DirectoryInfo> result = new();

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
        public static FileInfo[] GetFiles(DirectoryInfo directory)
        {
            Global.FileSystemDebugger.SendInternal($"DirectoryInfo.GetFiles() on path '{directory.FullName}'.");
            var xEntries = VFSManager.GetDirectoryListing(directory.FullName);

            List<FileInfo> result = new();

            for (int i = 0; i < xEntries.Count; i++)
            {
                Global.FileSystemDebugger.SendInternal($"Found entry of type {(int)xEntries[i].mEntryType} and name {xEntries[i].mFullPath}");
                if (xEntries[i].mEntryType == DirectoryEntryTypeEnum.File)
                {
                    //result[i] = new FileInfo(xEntries[i].mFullPath);
                    result.Add(new FileInfo(xEntries[i].mFullPath));
                }
            }

            return result.ToArray();
        }

        public static FileSystemInfo[] GetFileSystemInfos(DirectoryInfo directory)
        {
            Global.FileSystemDebugger.SendInternal($"DirectoryInfo.GetFiles() on path '{directory.FullName}'.");
            var xEntries = VFSManager.GetDirectoryListing(directory.FullName);

            List<FileSystemInfo> result = new();

            for (int i = 0; i < xEntries.Count; i++)
            {
                Global.FileSystemDebugger.SendInternal($"Found entry of type {(int)xEntries[i].mEntryType} and name {xEntries[i].mFullPath}");
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