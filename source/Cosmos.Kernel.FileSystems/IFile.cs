using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Kernel.FileSystems
{
    public interface IFile
    {
        bool Exists(string path);
    }
}
