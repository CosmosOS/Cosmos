using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Kernel.FileSystems
{
    /// <summary>
    /// Static utility class that wraps the underlaying filesystem.
    /// </summary>
    public static class File
    {
        private static IFile _file;

        public static void SetFileSystem(IFile fileSystem)
        {
            _file = fileSystem;   
        }

        public static bool Exists(string path)
        {
            return _file.Exists(path);
        }
    }
}
