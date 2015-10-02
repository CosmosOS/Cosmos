using System;
using System.Linq;
using System.Threading.Tasks;

namespace Cosmos.System.FileSystem.Listing
{
    public class Directory : Base
    {
        public Directory(FileSystem aFileSystem, string aName)
            : base(aFileSystem, aName)
        {
        }
    }
}