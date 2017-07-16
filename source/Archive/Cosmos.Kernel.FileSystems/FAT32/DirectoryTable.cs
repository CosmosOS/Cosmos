using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Sys.FileSystem.FAT32
{
    public class DirectoryTable
    {
        FAT fat;
        uint cluster;
        public DirectoryTable(FAT fat, uint cluster)
        {
            this.fat = fat;
            this.cluster = cluster;
        }

    }
}
