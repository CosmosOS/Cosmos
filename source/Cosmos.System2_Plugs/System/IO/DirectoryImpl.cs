//#define COSMOSDEBUG
using System;
using System.IO;
using System.Collections.Generic;
using Cosmos.IL2CPU.API.Attribs;
using Cosmos.System;
using Cosmos.System.FileSystem.Listing;
using Cosmos.System.FileSystem.VFS;

namespace Cosmos.System_Plugs.System.IO
{
    [Plug(Target = typeof(Directory))]
    public static class DirectoryImpl
    {
        /* The real implementation uses IEnumerable and do a conversion ToArray that crashes IL2CPU */
        public static string[] GetDirectories(string aPath)
        {
            Global.mFileSystemDebugger.SendInternal("Directory.GetDirectories");
            if (aPath == null)
            {
                throw new ArgumentNullException(aPath);
            }

            var xDirectories = new List<string>();
            var xEntries = VFSManager.GetDirectoryListing(aPath);
            for (int i = 0; i < xEntries.Count; i++)
            {
                if (xEntries[i].mEntryType == DirectoryEntryTypeEnum.Directory)
                {
                    xDirectories.Add(xEntries[i].mName);
                }
            }

            return xDirectories.ToArray();
        }

        /* The real implementation uses IEnumerable and do a conversion ToArray that crashes IL2CPU */
        public static string[] GetFiles(string aPath)
        {
            Global.mFileSystemDebugger.SendInternal("Directory.GetFiles");
            if (aPath == null)
            {
                throw new ArgumentNullException(aPath);
            }

            var xFiles = new List<string>();
            var xEntries = VFSManager.GetDirectoryListing(aPath);
            for (int i = 0; i < xEntries.Count; i++)
            {
                if (xEntries[i].mEntryType == DirectoryEntryTypeEnum.File)
                {
                    xFiles.Add(xEntries[i].mName);
                }
            }

            return xFiles.ToArray();
        }
    }
}
