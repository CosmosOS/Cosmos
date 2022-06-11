//#define COSMOSDEBUG

using System;
using System.Collections.Generic;
using Cosmos.HAL.BlockDevice;
using Cosmos.System.FileSystem.FAT;
using Cosmos.System.FileSystem.ISO9660;
using Cosmos.System.FileSystem.VFS;

namespace Cosmos.System.FileSystem;

public class Disk
{
    private readonly FileSystem[] MountedPartitions = new FileSystem[4];
    private readonly List<ManagedPartition> parts = new();

    /// <summary>
    ///     Main blockdevice that has all of the partitions.
    /// </summary>
    public BlockDevice Host;

    public Disk(BlockDevice mainBlockDevice)
    {
        Host = mainBlockDevice;
        foreach (var part in Partition.Partitions)
        {
            if (part.Host == mainBlockDevice)
            {
                parts.Add(new ManagedPartition(part));
            }
        }

        Size = (int)(mainBlockDevice.BlockCount * mainBlockDevice.BlockSize);
    }

    public bool IsMBR => !GPT.IsGPTPartition(Host);

    /// <summary>
    ///     The size of the disk in bytes.
    /// </summary>
    public int Size { get; }

    /// <summary>
    ///     List of partitions
    /// </summary>
    public List<ManagedPartition> Partitions
    {
        get
        {
            var converted = new List<ManagedPartition>();
            if (GPT.IsGPTPartition(Host))
            {
                var gpt = new GPT(Host);
                var i = 0;
                foreach (var item in gpt.Partitions)
                {
                    var part = new ManagedPartition(new Partition(Host, item.StartSector, item.SectorCount));
                    if (MountedPartitions[i] != null)
                    {
                        var data = MountedPartitions[i];
                        part.RootPath = data.RootPath;
                        part.MountedFS = data;
                    }

                    converted.Add(part);
                    i++;
                }
            }
            else
            {
                var mbr = new MBR(Host);
                var i = 0;
                foreach (var item in mbr.Partitions)
                {
                    var part = new ManagedPartition(new Partition(Host, item.StartSector, item.SectorCount));
                    if (MountedPartitions[i] != null)
                    {
                        var data = MountedPartitions[i];
                        part.RootPath = data.RootPath;
                        part.MountedFS = data;
                    }

                    converted.Add(part);
                    i++;
                }
            }

            return converted;
        }
    }

    /// <summary>
    ///     List of file systems.
    /// </summary>
    public static List<FileSystemFactory> RegisteredFileSystemsTypes { get; } =
        new() { new FatFileSystemFactory(), new ISO9660FileSystemFactory() };

    public BlockDeviceType Type => Host.Type;

    /// <summary>
    ///     Mounts all of the partitions in the disk
    /// </summary>
    public void Mount()
    {
        for (var i = 0; i < Partitions.Count; i++)
        {
            MountPartition(i);
        }
    }

    /// <summary>
    ///     Display information about the disk.
    /// </summary>
    public void DisplayInformation()
    {
        if (Partitions.Count > 0)
        {
            for (var i = 0; i < Partitions.Count; i++)
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
                Global.mFileSystemDebugger.SendInternal(Partitions[i].Host.BlockCount * Partitions[i].Host.BlockSize /
                                                        1024 / 1024);
                global::System.Console.WriteLine("Size: " +
                                                 Partitions[i].Host.BlockCount * Partitions[i].Host.BlockSize / 1024 /
                                                 1024 + " MB");
            }
        }
        else
        {
            global::System.Console.WriteLine("No partitions found!");
        }
    }

    /// <summary>
    ///     Create Partition.
    /// </summary>
    /// <param name="size">Size in MB.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if start / end is smaller then 0.</exception>
    /// <exception cref="ArgumentException">Thrown if end is smaller or equal to start.</exception>
    /// <exception cref="NotImplementedException">Thrown if partition type is GPT.</exception>
    public void CreatePartition(int size)
    {
        if ((size == 0) | (size < 0))
        {
            throw new ArgumentException("size");
        }

        if (GPT.IsGPTPartition(Host))
        {
            throw new Exception("Creating partitions with GPT style not yet supported!");
        }

        int location;
        var startingSector = 63;
        var amountOfSectors = (uint)(size * 1024 * 1024 / 512);
        //TODO: Check if partition is too big

        if (Partitions.Count == 0)
        {
            location = 446;
            startingSector = 63;
        }
        else if (Partitions.Count == 1)
        {
            location = 462;
            startingSector = (int)(Partitions[0].Host.BlockSize + Partitions[0].Host.BlockCount);
        }
        else if (Partitions.Count == 2)
        {
            location = 478;
        }
        else if (Partitions.Count == 3)
        {
            location = 494;
        }
        else
        {
            throw new NotImplementedException("Extended partitons not yet supported.");
        }

        //Create MBR
        var mbrData = new byte[512];
        Host.ReadBlock(0, 1, ref mbrData);
        mbrData[location + 0] = 0x80; //bootable
        mbrData[location + 1] = 0x1; //starting head
        mbrData[location + 2] = 0; //Starting sector
        mbrData[location + 3] = 0x0; //Starting Cylinder
        mbrData[location + 4] = 83; //normal partition
        mbrData[location + 5] = 0xFE; //ending head
        mbrData[location + 6] = 0x3F; //Ending Sector
        mbrData[location + 7] = 0x40; //Ending Cylinder

        //Starting Sector
        var startingSectorBytes = BitConverter.GetBytes(startingSector);
        mbrData[location + 8] = startingSectorBytes[0];
        mbrData[location + 9] = startingSectorBytes[1];
        mbrData[location + 10] = startingSectorBytes[2];
        mbrData[location + 11] = startingSectorBytes[3];

        //Total Sectors in partition
        var total = BitConverter.GetBytes(amountOfSectors);
        mbrData[location + 12] = total[0];
        mbrData[location + 13] = total[1];
        mbrData[location + 14] = total[2];
        mbrData[location + 15] = total[3];

        //Boot flag
        var boot = BitConverter.GetBytes((ushort)0xAA55);
        mbrData[510] = boot[0];
        mbrData[511] = boot[1];

        //Save the data
        Host.WriteBlock(0, 1, ref mbrData);
    }

    /// <summary>
    ///     Deletes a partition
    /// </summary>
    /// <param name="index">Partition index starting from 0</param>
    public void DeletePartition(int index)
    {
        if (GPT.IsGPTPartition(Host))
        {
            throw new Exception("Deleting partitions with GPT style not yet supported!");
        }

        var location = 446 + 16 * index;

        var mbr = Host.NewBlockArray(1);
        Host.ReadBlock(0, 1, ref mbr);
        for (var i = location; i < location + 16; i++)
        {
            mbr[i] = 0;
        }

        Host.WriteBlock(0, 1, ref mbr);
    }

    /// <summary>
    ///     Deletes all partitions on the disk.
    /// </summary>
    public void Clear()
    {
        if (GPT.IsGPTPartition(Host))
        {
            throw new Exception("Removing all partitions with GPT style not yet supported!");
        }

        for (var i = 0; i < Partitions.Count; i++)
        {
            DeletePartition(i);
        }
    }

    public void FormatPartition(int index, string format, bool quick = true)
    {
        var part = Partitions[index];

        var xSize = (long)(Host.BlockCount * Host.BlockSize / 1024 / 1024);

        if (format.StartsWith("FAT"))
        {
            FatFileSystem.CreateFatFileSystem(part.Host, VFSManager.GetNextFilesystemLetter() + ":\\", xSize, format);
            Mount();
        }
        else
        {
            throw new NotImplementedException(format + " formatting not supported.");
        }
    }

    /// <summary>
    ///     Mounts a partition
    /// </summary>
    /// <param name="index">Partiton index</param>
    public void MountPartition(int index)
    {
        var part = Partitions[index];
        //Don't remount
        if (MountedPartitions[index] != null)
        {
            //We already mounted this partiton
            return;
        }

        var xRootPath = String.Concat(VFSManager.GetNextFilesystemLetter(), VFSBase.VolumeSeparatorChar,
            VFSBase.DirectorySeparatorChar);
        var xSize = (long)(Host.BlockCount * Host.BlockSize / 1024 / 1024);

        foreach (var item in RegisteredFileSystemsTypes)
        {
            if (item.IsType(part.Host))
            {
                Kernel.PrintDebug("Mounted partition.");

                //We would have done Partitions[i].MountedFS = item.Create(...), but since the array is not cached, we need to store the mounted partitions in a list
                MountedPartitions[index] = item.Create(part.Host, xRootPath, xSize);
                return;
            }
        }

        Kernel.PrintDebug("Cannot find file system for partiton.");
    }
}
