using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SentinelKernel.System.FileSystem.Listing
{
    public class Directory : Base
    {
        public Directory(FileSystem aFileSystem, string aName)
            : base(aFileSystem, aName)
        {
        }
    }
}