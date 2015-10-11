using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cosmos.System.FileSystem.FAT.Listing
{
    public class FatFile : System.FileSystem.Listing.File
    {
        public new readonly FatFileSystem FileSystem;
        public readonly UInt64 FirstClusterNum;

        // Size is UInt32 because FAT doesn't support bigger.
        // Dont change to UInt64
        public FatFile(FatFileSystem aFileSystem, string aName, UInt32 aSize, UInt64 aFirstCluster, FatDirectory baseDirectory)
            : base(aFileSystem, aName, aSize, baseDirectory)
        {
            FileSystem = aFileSystem;
            FirstClusterNum = aFirstCluster;
        }
    }
}
