using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Kernel.FileSystems.Windows
{
    /// <summary>
    /// Implementation of operations on the Windows file system.
    /// </summary>
    public class File : IFile
    {
        public bool Exists(string path)
        {
            return true;
        }

    }
}
