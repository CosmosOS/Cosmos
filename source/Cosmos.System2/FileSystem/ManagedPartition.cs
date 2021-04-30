using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.HAL.BlockDevice;
using Cosmos.System.FileSystem.VFS;

namespace Cosmos.System.FileSystem
{
    public class ManagedPartition
    {
        public readonly Partition Host;
        /// <summary>
        /// The root path of the file system. Example: 0:\
        /// </summary>
        public string RootPath = "";
        /// <summary>
        /// The FileSystem object. Null if not mounted.
        /// </summary>
        public FileSystem MountedFS;
        /// <summary>
        /// Does the partition have a known file system?
        /// </summary>
        public bool HasFileSystem
        {
            get
            {
                return MountedFS != null;
            }
        }

        public ManagedPartition(Partition host)
        {
            Host = host;
        }
        /// <summary>
        /// Mounts the partition
        /// </summary>
        public void Mount()
        {
            //Don't remount
            if (MountedFS != null)
                return;
            string xRootPath = string.Concat(Disk.CurrentDriveLetter, VFSBase.VolumeSeparatorChar, VFSBase.DirectorySeparatorChar);
            var xSize = (long)(Host.BlockCount * Host.BlockSize / 1024 / 1024);

            foreach (var item in Disk.RegisteredFileSystemsTypes)
            {
                if (item.IsType(Host))
                {
                    MountedFS = item.Create(Host, xRootPath, xSize);
                    RootPath = xRootPath;
                }
            }
        }
        /// <summary>
        /// Mounts a FileSystem factory.
        /// </summary>
        public void Mount(FileSystemFactory fact)
        {
            //Don't remount
            if (MountedFS != null)
                return;
            string xRootPath = string.Concat(Disk.CurrentDriveLetter, VFSBase.VolumeSeparatorChar, VFSBase.DirectorySeparatorChar);
            var xSize = (long)(Host.BlockCount * Host.BlockSize / 1024 / 1024);

            foreach (var item in Disk.RegisteredFileSystemsTypes)
            {
                if (item.IsType(Host))
                {
                    MountedFS = item.Create(Host, xRootPath, xSize);
                    RootPath = xRootPath;
                }
            }
        }
        /// <summary>
        /// Zeros out the partition
        /// </summary>
        public void Clear()
        {
            for (ulong i = 0; i < Host.BlockCount; i++)
            {
                byte[] data = new byte[512];
                Host.WriteBlock(i, 1, ref data);
            }
        }
    }
}
