using System;
using System.Linq;
using System.Threading.Tasks;
using Cosmos.System.FileSystem.Listing;

namespace Cosmos.System.FileSystem.FAT.Listing
{
    public class FatDirectory : System.FileSystem.Listing.Directory
    {
        public new readonly FatFileSystem FileSystem;
        public readonly UInt64 FirstClusterNum;

        public FatDirectory(FatFileSystem aFileSystem, string aName, ulong firstCluster, Directory baseDirectory, ulong size)
            : base(aFileSystem, aName, baseDirectory)
        {
            FileSystem = aFileSystem;
            FirstClusterNum = firstCluster;
            Size = size;
        }
    }
}
