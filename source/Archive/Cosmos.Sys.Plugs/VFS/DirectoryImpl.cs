using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Cosmos.IL2CPU.Plugs;
using Cosmos.Sys.FileSystem;

namespace Cosmos.Sys.Plugs {
    [Plug(Target=typeof(Directory))]
    public static class DirectoryImpl
    {
        public static bool Exists(string aDir) {
            return VFSManager.DirectoryExists(aDir);
        }

        private static string mCurrentDirectory = @"/0";
        public static string GetCurrentDirectory()
        {
            return mCurrentDirectory;
        }

        public static void SetCurrentDirectory(string aDir)
        {
            mCurrentDirectory = aDir;
        }

        public static string[] GetDirectories(string aDir)
        {
            var xEntries = VFSManager.GetDirectoryListing(aDir);
            //var xResult = new string[xEntries.Length];
            var xResult = new List<string>();
            for (int i = 0; i < xEntries.Length; i++)
            {
            ////var i = 0;
                if (xEntries[i].IsDirectory)
                {
                    xResult.Add(xEntries[i].Name);
                    //xResult[i] = xEntries[i].Name;
                }
            }
            //var xResult = new string[1];
            //xResult[0] = "TempDir";
            return xResult.ToArray();
        //    List<string> xDirectoryNames = new List<string>();
        //    foreach (FilesystemEntry xDirectory in VFSManager.GetDirectories(aDir))
        //        xDirectoryNames.Add(xDirectory.Name);

        //    return xDirectoryNames.ToArray();
        //    //return (from xDirectoryName in VFSManager.GetDirectories(aDir) select xDirectoryName.Name).ToArray();
        }

        //Plugs Directory.InternalGetFileDirectoryNames which is used by 6 methods (in Directory and DirectoryInfo)
        public static string[] InternalGetFileDirectoryNames(string path, string userPathOriginal, string searchPattern, bool includeFiles, bool includeDirs, SearchOption searchOption)
        {
            return VFSManager.InternalGetFileDirectoryNames(path, userPathOriginal, searchPattern, includeFiles, includeDirs, searchOption);
        }

        public static string[] GetFiles(string aDir)
        {
            List<string> xFileNames = new List<string>();
            var xFileEntries = VFSManager.GetFiles(aDir);
            for (int i = 0; i < xFileEntries.Length; i++)
            {
                xFileNames.Add(xFileEntries[i].Name);
            }
            return xFileNames.ToArray();

            //return (from xFileName in VFSManager.GetFiles(aDir) select xFileName.Name).ToArray();
        }

        public static string[] GetLogicalDrives()
        {
            return VFSManager.GetLogicalDrives();
        }

        public static DirectoryInfo GetParent(string aDir)
        {
            if (aDir == null)
                throw new ArgumentNullException("aDir");

            if (aDir.Length == 0)
                throw new ArgumentException("aDir is empty");

            string xDirectoryName = Path.GetDirectoryName(aDir);
            if (xDirectoryName == null)
                return null;

            return new DirectoryInfo(xDirectoryName);
        }
    }
}