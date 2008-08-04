using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Indy.IL2CPU.Plugs;
using Cosmos.FileSystem;

namespace Cosmos.Sys.Plugs {
    [Plug(Target=typeof(Directory))]
    public static class DirectoryImpl
    {
        public static bool Exists(string aDir) {
            return VFSManager.DirectoryExists(aDir);
        }

        public static string GetCurrentDirectory()
        {
            return @"0:/";
        }

        public static string[] GetDirectories(string aDir)
        {
            List<string> xDirectoryNames = new List<string>();
            foreach (FilesystemEntry xDirectory in VFSManager.GetDirectories(aDir))
                xDirectoryNames.Add(xDirectory.Name);

            return xDirectoryNames.ToArray();
            //return (from xDirectoryName in VFSManager.GetDirectories(aDir) select xDirectoryName.Name).ToArray();
        }

        public static string[] GetFiles(string aDir)
        {
            List<string> xFileNames = new List<string>();
            foreach (FilesystemEntry xFile in VFSManager.GetFiles(aDir))
            {
                xFileNames.Add(xFile.Name);
            }
            return xFileNames.ToArray();

            //return (from xFileName in VFSManager.GetFiles(aDir) select xFileName.Name).ToArray();
        }

        public static string[] GetLogicalDrives()
        {
            return VFSManager.GetLogicalDrives();
        }
    }
}