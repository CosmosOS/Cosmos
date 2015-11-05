using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Cosmos.Debug.Kernel;

namespace Cosmos.System.FileSystem.FAT
{
    public class FatFileStream : BaseFatStream
    {
        public FatFileStream(Listing.FatFile aFile):base(aFile, aFile.FileSystem, aFile.FirstClusterNum)
        {
        }
    }
}
