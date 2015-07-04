using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SentinelKernel.System.FileSystem.FAT.Listing
{
    public class FatDirectory : System.FileSystem.Listing.Directory
    {
        public FatDirectory(FileSystem aFileSystem, string aName, uint firstCluster)
            : base(aFileSystem, aName)
        {
            FirstClusterNr = firstCluster;
        }

        public uint FirstClusterNr;
    }
}
