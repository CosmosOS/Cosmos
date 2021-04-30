//#define COSMOSDEBUG

using System.Collections.Generic;
using Cosmos.HAL.BlockDevice;

namespace Cosmos.System.FileSystem
{
    public class Disk
    {
        /// <summary>
        /// The size of the disk in MB.
        /// </summary>
        public int Size { get; }
        /// <summary>
        /// List of partitions
        /// </summary>
        private List<ManagedPartition> parts = new List<ManagedPartition>();
        public List<ManagedPartition> Partitions { get { return parts; } }

        private static List<FileSystemFactory> registeredFileSystems = new List<FileSystemFactory>();
        /// <summary>
        /// List of file systems.
        /// </summary>
        public static List<FileSystemFactory> RegisteredFileSystemsTypes { get { return registeredFileSystems; } }
        /// <summary>
        /// Main blockdevice that has all of the partitions.
        /// </summary>
        public BlockDevice Host;
        public Disk(BlockDevice b)
        {
            Host = b;
            if (registeredFileSystems.Count == 0)
            {
                registeredFileSystems.Add(new FatFileSystemFactory());
            }
            for (int i = 0; i < BlockDevice.Devices.Count; i++)
            {
                if (BlockDevice.Devices[i] is Partition b2)
                {
                    if (b2.Host == b)
                    {
                        parts.Add(new ManagedPartition(b2));
                    }
                }
            }
            Size = (int)(b.BlockCount * b.BlockSize / 1024 / 1024);

            if (Partitions.Count > 0)
            {
                for (int i = 0; i < Partitions.Count; i++)
                {
                    Global.mFileSystemDebugger.SendInternal("Partition #: ");
                    Global.mFileSystemDebugger.SendInternal(i + 1);
                    global::System.Console.WriteLine("Partition #: " + (i + 1));
                    Global.mFileSystemDebugger.SendInternal("Block Size:");
                    Global.mFileSystemDebugger.SendInternal(Partitions[i].Host.BlockSize);
                    global::System.Console.WriteLine("Block Size: " + Partitions[i].Host.BlockSize + " bytes");
                    Global.mFileSystemDebugger.SendInternal("Block Count:");
                    Global.mFileSystemDebugger.SendInternal(Partitions[i].Host.BlockCount);
                    global::System.Console.WriteLine("Block Partitions: " + Partitions[i].Host.BlockCount);
                    Global.mFileSystemDebugger.SendInternal("Size:");
                    Global.mFileSystemDebugger.SendInternal(Partitions[i].Host.BlockCount * Partitions[i].Host.BlockSize / 1024 / 1024);
                    global::System.Console.WriteLine("Size: " + Partitions[i].Host.BlockCount * Partitions[i].Host.BlockSize / 1024 / 1024 + " MB");
                }
            }
            else
            {
                global::System.Console.WriteLine("No partitions found!");
            }
        }
        internal static int CurrentDriveLetter = 0;
        /// <summary>
        /// Mounts the disk.
        /// </summary>
        public void Mount()
        {
            foreach (var part in Partitions)
            {
                if (part != null)
                {
                    part.Mount();
                    CurrentDriveLetter++;
                }
            }
        }
        /// <summary>
        /// Initializes this disk with MBR. This may destroy data.
        /// </summary>
        public void Format()
        {
            var xMBRData = new byte[512];
            xMBRData[446 + 0] = 0x0; //bootable
            xMBRData[446 + 1] = 0x1; //starting head
            xMBRData[446 + 2] = 0x1; //Starting sector
            xMBRData[446 + 3] = 0x0; //Starting Cylinder
            xMBRData[446 + 4] = 83;//normal partition
            xMBRData[446 + 5] = 0xFE; //ending head
            xMBRData[446 + 6] = 0x3F; //Ending Sector
            xMBRData[446 + 7] = 0x40;//Ending Cylinder

            //Relative Sector
            xMBRData[446 + 8] = 0x3F;
            xMBRData[446 + 9] = 0;
            xMBRData[446 + 10] = 0;
            xMBRData[446 + 11] = 0;
            //Total Sectors in partition
            xMBRData[446 + 12] = 0xC2;
            xMBRData[446 + 13] = 0xEE;
            xMBRData[446 + 14] = 0x0F;
            xMBRData[446 + 15] = 00;

            Host.WriteBlock(0UL, 1U, ref xMBRData);
        }
    }
}
