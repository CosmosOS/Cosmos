using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Debug.Kernel;
using Cosmos.HAL.BlockDevice;
using Cosmos.System.FileSystem.FAT;
using Cosmos.System.FileSystem.VFS;

namespace Cosmos.System.FileSystem
{
    public class ManagedPartition
    {
        internal static Debugger PartitonDebugger = new Debugger("System", "Partiton");
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
            {
                return;
            }
            string xRootPath = String.Concat(VFSManager.GetNextFilesystemLetter(), VFSBase.VolumeSeparatorChar, VFSBase.DirectorySeparatorChar);
            var xSize = (long)(Host.BlockCount * Host.BlockSize / 1024 / 1024);

            foreach (var item in Disk.RegisteredFileSystemsTypes)
            {
                if (item.IsType(Host))
                {
                    MountedFS = item.Create(Host, xRootPath, xSize);
                    RootPath = xRootPath;
                    return;
                }
            }

            PartitonDebugger.Send("Cannot find file system for partiton.");
        }
        /// <summary>
        /// Mounts using a FileSystem factory.
        /// </summary>
        public void Mount(FileSystemFactory fact)
        {
            //Don't remount
            if (MountedFS != null)
            {
                return;
            }
            string xRootPath = String.Concat(VFSManager.GetNextFilesystemLetter(), VFSBase.VolumeSeparatorChar.ToString(), VFSBase.DirectorySeparatorChar.ToString());
            var xSize = (long)(Host.BlockCount * Host.BlockSize / 1024 / 1024);

            if (fact.IsType(Host))
            {
                MountedFS = fact.Create(Host, xRootPath, xSize);
                RootPath = xRootPath;
            }
            else
            {
                throw new Exception("The disk filesystem does not match with the FileSystemFactory.");
            }
        }
        /// <summary>
        /// Zeros out the entire partition
        /// </summary>
        public void Clear()
        {
            for (ulong i = 0; i < Host.BlockCount; i++)
            {
                byte[] data = new byte[512];
                Host.WriteBlock(i, 1, ref data);
            }
        }

        /// <summary>
        /// Format drive. (delete all)
        /// </summary>
        /// <param name="aDriveFormat">A drive format.</param>
        /// <param name="aQuick">Quick format.</param>
        /// <exception cref="NotImplementedException">
        /// <list type="bullet">
        /// <item>Thrown when formating to different filesystem format then current format.</item>
        /// <item>aQuick is false.</item>
        /// <item>Thrown when FAT type is unknown.</item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <list type = "bullet" >
        /// <item>Thrown when the data length is 0 or greater then Int32.MaxValue.</item>
        /// <item>Entrys matadata offset value is invalid.</item>
        /// <item>Fatal error (contact support).</item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentNullException">Thrown when filesystem is null / memory error.</exception>
        /// <exception cref="ArgumentException">
        /// <list type="bullet">
        /// <item>Thrown when drive name is null or empty.</item>
        /// <item>Data length is 0.</item>
        /// <item>Root path is null or empty.</item>
        /// <item>Memory error.</item>
        /// </list>
        /// </exception>
        /// <exception cref="Exception">
        /// <list type="bullet">
        /// <item>Unable to determine filesystem for path:  + drive name.</item>
        /// <item>Thrown when data size invalid.</item>
        /// <item>Thrown on unknown file system type.</item>
        /// <item>Thrown on fatal error (contact support).</item>
        /// </list>
        /// </exception>
        /// <exception cref="OverflowException">Thrown when data lenght is greater then Int32.MaxValue.</exception>
        /// <exception cref="DecoderFallbackException">Thrown on memory error.</exception>
        /// <exception cref="RankException">Thrown on fatal error (contact support).</exception>
        /// <exception cref="ArrayTypeMismatchException">Thrown on fatal error (contact support).</exception>
        /// <exception cref="InvalidCastException">Thrown when the data in aData is corrupted.</exception>
        /// <exception cref="NotSupportedException">Thrown when FAT type is unknown.</exception>
        public void Format(string aDriveFormat, bool aQuick = true)
        {
            var xSize = (long)(Host.BlockCount * Host.BlockSize / 1024 / 1024);

            if (aDriveFormat.StartsWith("FAT"))
            {
                FatFileSystem.CreateFatFileSystem(Host, VFSManager.GetNextFilesystemLetter() + ":\\", xSize, aDriveFormat);
                Mount();
            }
            else
            {
                throw new NotImplementedException(aDriveFormat + " formatting not supported.");
            }
        }

        /// <summary>
        /// Change drive letter.
        /// </summary>
        /// <param name="aNewName">A new name to be set.</param>
        /// <exception cref="ArgumentNullException">Thrown when aNewName is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if aNewName length is smaller then 2, or greater than Int32.MaxValue.</exception>
        /// <exception cref="ArgumentException">Thrown if aNewName is invalid drive identifier / not a root dir.</exception>
        /// <exception cref="NotImplementedException">Thrown always.</exception>
        public void ChangeDriveLetter(string aNewName)
        {
            if (aNewName == null)
            {
                throw new ArgumentNullException(nameof(aNewName));
            }

            if (!VFSManager.IsValidDriveId(aNewName))
            {
                throw new ArgumentException("Argument must be drive identifier or root dir");
            }

            /*
             * 1. Add new method in VFSManager to change identifier of a drive
             * 2. Update 'Name' to be 'aNewName'
             */
            throw new NotImplementedException("ChangeDriveLetter");
        }
    }
}
