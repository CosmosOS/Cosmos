using System;
using System.Linq;
using System.Threading.Tasks;
using Cosmos.System.FileSystem.FAT.Listing;

namespace Cosmos.System.FileSystem.FAT
{
    public class FatDirectoryStream : BaseFatStream
    {
        public FatDirectoryStream(FatDirectory directory) : base(directory, directory.FileSystem, directory.FirstClusterNum)
        {

        }
    }
}
