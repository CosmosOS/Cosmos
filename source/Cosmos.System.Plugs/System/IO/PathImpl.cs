#define COSMOSDEBUG

using System;
using System.IO;

using Cosmos.Common;
using Cosmos.Debug.Kernel;
using Cosmos.IL2CPU.Plugs;
using Cosmos.System.FileSystem;
using Cosmos.System.FileSystem.VFS;

namespace Cosmos.System.Plugs.System.IO
{
    [Plug(Target = typeof(Path))]
    public static class PathImpl
    {
        public static string NormalizePath(string aPath, bool aFullCheck)
        {
            // For now let's return the Path not normalized
	        return aPath;
        }

        public static string NormalizePath(string path, bool fullCheck, int maxPathLength, bool expandShortPaths)
        {
            return path;
        }

        public static string GetTempPath()
        {
            
            throw new NotImplementedException("Path.GetTempPath()");
        }

        public static char[] GetInvalidFileNameChars()
        {
            // TODO implement System.Object.MemberwiseClone() should permit to unplug this method
            throw new NotImplementedException("Path.GetInvalidFileNameChars()");
        }

        public static char[] GetInvalidPathChars()
        {
            // TODO implement System.Object.MemberwiseClone() should permit to unplug this this method
            throw new NotImplementedException("Path.GetInvalidPathChars()");
        }

        public static string GetTempFileName()
        {
            throw new NotImplementedException("Path.GetTempFileName()");
        }

        public static string GetRandomFileName()
        {
            // TODO this method needs Random to work
            throw new NotImplementedException("Path.GetRandomFileName()");
        }

    }
}
