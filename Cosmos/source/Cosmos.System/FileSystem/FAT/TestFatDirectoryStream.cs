using System;
using System.Linq;
using System.Threading.Tasks;
using Cosmos.System.FileSystem.FAT.Listing;

namespace Cosmos.System.FileSystem.FAT
{
    public class TestFatDirectoryStream: BaseFatStream
    {
        public TestFatDirectoryStream(FatDirectory directory) : base(directory, directory.FileSystem, directory.FirstClusterNum)
        {

        }
    }
}
