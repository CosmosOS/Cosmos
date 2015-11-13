using System;
using System.IO;

using Cosmos.IL2CPU.Plugs;
using Cosmos.System.FileSystem.VFS;

namespace Cosmos.System.Plugs.System.IO
{
    using Cosmos.System.FileSystem;

    [Plug(Target = typeof(Directory))]
    public static class DirectoryImpl
    {
        private static string mCurrentDirectory = string.Empty;

        public static string GetCurrentDirectory()
        {
            FatHelpers.Debug("-- Directory.GetCurrentDirectory --");

            return mCurrentDirectory;
        }

        public static void SetCurrentDirectory(string aPath)
        {
            FatHelpers.Debug("-- Directory.SetCurrentDirectory --");

            mCurrentDirectory = aPath;
        }

        public static bool Exists(string aPath)
        {
            FatHelpers.Debug("-- Directory.Exists --");

            return VFSManager.DirectoryExists(aPath);
        }

        public static DirectoryInfo GetParent(string aPath)
        {
            FatHelpers.Debug("-- Directory.GetParent --");

            if (aPath == null)
            {
                throw new ArgumentNullException("aPath");
            }

            if (aPath.Length == 0)
            {
                throw new ArgumentException("Path must not be empty.", "aPath");
            }

            string xFullPath = Path.GetFullPath(aPath);
            string xName = Path.GetDirectoryName(xFullPath);
            if (xName == null)
            {
                return null;
            }

            return new DirectoryInfo(xName);
        }
    }
}