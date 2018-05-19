using Cosmos.HAL.BlockDevice;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.System.FileSystem
{
    public class FileSystemFactory
    {
        public virtual string Name { get; private set; }

        public virtual FileSystem Create(Partition aDevice, string aRootPath, long aSize) => null;

        public virtual bool IsType(Partition aDevice) => false;
    }
}
