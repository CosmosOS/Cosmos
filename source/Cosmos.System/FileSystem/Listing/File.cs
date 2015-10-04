using System;
using System.Linq;
using System.Threading.Tasks;

namespace Cosmos.System.FileSystem.Listing
{
    public class File : Base
    {
        //TODO:UInt64? Size until ulong works.. nullable types have a problem right now
        public File(FileSystem aFileSystem, string aName, UInt64 aSize, Directory baseDirectory)
            : base(aFileSystem, aName, baseDirectory)
        {
            mSize = aSize;
        }
    }
}
