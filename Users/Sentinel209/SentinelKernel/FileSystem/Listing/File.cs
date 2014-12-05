using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SentinelKernel.System.FileSystem.Listing
{
    public class File : Base
    {
        //TODO:UInt64? Size until ulong works.. nullable types have a problem right now
        public File(FileSystem aFileSystem, string aName, UInt64 aSize)
            : base(aFileSystem, aName)
        {
            mSize = aSize;
        }
    }
}