using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Cosmos.IL2CPU.Plugs;
using SentinelKernel.System.FileSystem.VFS;

namespace SentinelKernel.System.Plugs.System.IO
{
    [Plug(Target = typeof(Directory))]
    public static class DirectoryImpl
    {
        private static string mCurrentDirectory = @"/0";

        public static string GetCurrentDirectory()
        {
            return mCurrentDirectory;
        }

        public static void SetCurrentDirectory(string aPath)
        {
            mCurrentDirectory = aPath;
        }

        public static bool Exists(string aPath)
        {
            return FileSystem.VFS.VFSManager.DirectoryExists(aPath);
        }
    }
}