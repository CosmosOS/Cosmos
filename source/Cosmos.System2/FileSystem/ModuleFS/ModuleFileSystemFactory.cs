using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos.HAL.BlockDevice;

namespace Cosmos.System.FileSystem.ModuleFS
{
    public class ModuleFileSystemFactory : FileSystemFactory
    {
        private static ModuleFileSystem mfs_instance = null;
        public override string Name => "ModuleFS";

        public override FileSystem Create(Partition aDevice, string aRootPath, long aSize) => new ModuleFileSystem(aSize, aRootPath);
        public override bool IsType(Partition aDevice)
        {
            return false;
        }
    }
}
