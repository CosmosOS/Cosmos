using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Sys.FileSystem;

namespace Cosmos.Sys.FileSystem.FAT32
{
    public abstract class FileAllocationTable
    {
        public abstract uint GetFreeCluster();
        public abstract uint GetNextCluster(uint CurrentCluster);
        public abstract uint AddFreeCluster(uint LastSector);


        public uint ClusterSize;

        public abstract void Format();
    }
    public class FileAllocationTableFAT32 : FileAllocationTable
    {
        private FAT32 fat32;
        private uint FirstSector;
        private uint SecondSector;
        private uint TotalClusters;
        private Partition p;
        public FileAllocationTableFAT32(FAT32 fat32, Partition p, uint firstsector, uint secondsector)
        {
            this.fat32 = fat32;
            this.FirstSector = firstsector;
            this.SecondSector = secondsector;
            this.TotalClusters = (fat32.BootSector.TotalSectors32-fat32.BootSector.ReservedSectorCount)/ fat32.BootSector.SectorsPerCluster;
            this.p = p;
            this.ClusterSize = (uint)(fat32.BootSector.SectorsPerCluster * fat32.BootSector.BytesPerSec);

            switch (fat32.BootSector.BytesPerSec/4)
            {
                case 128:
                    Shift = 7;
                    Mask = 0x7f;
                    break;
                default:
                    throw new Exception("weird sector size");
            }
        }


        private int Shift;
        private uint Mask;
       
        public override void Format()
        {
            byte[] b = new byte[fat32.BootSector.BytesPerSec];
            for (int i = 0; i < fat32.BootSector.FATsz16; i++)
                p.WriteBlock((uint)(FirstSector + i), b);

            b[0] = fat32.BootSector.Media;
            b[1] = 0xff;
            b[2] = 0xff;
            b[3] = 0xff;

            Array.Copy(BitConverter.GetBytes((uint)0x0ffffff8), 0, b, 4, 4);

            p.WriteBlock(FirstSector, b); // first 2 sectors are reserved 
        }

        public uint ClusterFree = 0x0;
        public uint ClusterReserved = 0x0;
        public uint ClusterEOL = 0xffffff8;
        public uint ClusterBAD = 0xffffff6;


        public override uint AddFreeCluster(uint LastCluster)
        {
            uint newcluster = GetFreeCluster();
            WriteClusterAllocation(newcluster, ClusterEOL);
            WriteClusterAllocation(LastCluster, newcluster);
            return newcluster;
        }

        public override uint GetFreeCluster()
        {
            uint start = fat32.FileSystemInfo32.Nxt_Free;
            if (start == 0x0 || start == 0xffffffff)
                start = 0x2;

            for (uint i = 0; i < TotalClusters; i++)
            {
                uint actualcluster = (start + i - 2) % (TotalClusters) + 2;
                if (ReadClusterAllocation(actualcluster) == 0x0)
                {
                    fat32.FileSystemInfo32.Nxt_Free = actualcluster + 1;
                    return actualcluster;
                }
            }

            throw new Exception("Disk full");

        }

        private unsafe uint ReadClusterAllocation(uint actualcluster)
        {
            uint index = actualcluster & Mask;
            uint sector = FirstSector + actualcluster >> Shift;

            byte[] data= GetSector(sector);

            return BitConverter.ToUInt32(data,(int)(index<<2));
        }
        private unsafe void WriteClusterAllocation(uint actualcluster, uint value)
        {
            uint index = actualcluster & Mask;
            uint sector = FirstSector + actualcluster >> Shift;

            byte[] data = GetSector(sector);
            byte[] b = BitConverter.GetBytes(value);
            index <<= 2;
            data[index++] = b[0];
            data[index++] = b[1];
            data[index++] = b[2];
            data[index++] = b[3];

            SetSector(sector, data);
        }

        byte[] CachedSector;
        uint SectorNumber=0xffffffff;


        public override uint GetNextCluster(uint CurrentCluster)
        {
            return ReadClusterAllocation(CurrentCluster);
        }

        private void SetSector(uint sector, byte[] data)
        {
            if (sector == SectorNumber)
                CachedSector = data;
            p.WriteBlock(sector, data);
        }
        private  unsafe byte[] GetSector(uint Sector)
        {
            if (Sector != SectorNumber)
            {
                SectorNumber = Sector;
                p.ReadBlock(Sector, CachedSector);
            }
            return CachedSector;
        }


    }
}
