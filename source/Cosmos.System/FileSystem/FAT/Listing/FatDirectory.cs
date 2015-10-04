using System;
using System.Linq;
using System.Threading.Tasks;

namespace Cosmos.System.FileSystem.FAT.Listing
{
    public class FatDirectory : System.FileSystem.Listing.Directory
    {
        public FatDirectory(FileSystem aFileSystem, string aName, uint firstCluster, string baseDirectory)
            : base(aFileSystem, aName, baseDirectory)
        {
            FirstClusterNr = firstCluster;
        }

        public uint FirstClusterNr;
    }
}
