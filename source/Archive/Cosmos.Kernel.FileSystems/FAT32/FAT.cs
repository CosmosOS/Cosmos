using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Sys.FileSystem;

namespace Cosmos.Sys.FileSystem.FAT32
{
    public abstract class FAT
    {
        protected Partition p;

        protected FAT(Partition p)
        {
            this.p = p;
        }

        public BootSector BootSector;
        public FileAllocationTable FileAllocationTable;


        public abstract byte[] ReadCluster(uint Cluster);
        public abstract void WriteCluster(uint Cluster, byte[] bytes);


    }

    public class FAT32 : FAT
    {
        public FileSystemInfo32 FileSystemInfo32;
        public FAT32(Partition p) : base(p)
        {
            BootSector = new BootSectorFAT32(p);
            BootSector.Refresh();
            if ((BootSector as BootSectorFAT32).BPB_FSInfo != 0x0)
                FileSystemInfo32 = new FileSystemInfo32(p, (BootSector as BootSectorFAT32).BPB_FSInfo);
            else
                FileSystemInfo32 = null;

            FileAllocationTable = new FileAllocationTableFAT32(this, p,
                BootSector.ReservedSectorCount, 
                (uint)(BootSector.ReservedSectorCount + BootSector.FATsz16));
            CalcOffsets();
        }
        public void Format(string label, uint clustersize)
        {
            BootSectorFAT32 BootSector = new BootSectorFAT32(p);
            BootSector.TotalSectors32 = (uint)p.BlockCount;
            BootSector.BytesPerSec = (ushort)p.BlockSize;
            BootSector.SectorsPerCluster = (byte)(clustersize / p.BlockSize);
            BootSector.BPB_FATSz32 = (BootSector.TotalSectors32 * 512 * 4 / clustersize + p.BlockSize - 1) / p.BlockSize;
            BootSector.ReservedSectorCount = 32;
            BootSector.Media = 0xf8;
            BootSector.NumberFATs = 2;
            BootSector.Signature = 0xaa55;
            this.BootSector = BootSector;

            FileAllocationTable = new FileAllocationTableFAT32(this, p, BootSector.ReservedSectorCount, (uint)(BootSector.ReservedSectorCount + BootSector.FATsz16));

            FileSystemInfo32 = new FileSystemInfo32(p, 1);
            BootSector.BPB_FSInfo = 1;
            FileSystemInfo32.Free_Count = (BootSector.TotalSectors32 / BootSector.SectorsPerCluster -2);
            FileSystemInfo32.LeadSig = 0x41615252;
            FileSystemInfo32.Nxt_Free = 0x2;
            FileSystemInfo32.StrucSig = 0x61417272;
            FileSystemInfo32.TrailSig = 0xaa550000;
 
            BootSector.Save();
            FileSystemInfo32.Save();
            FileAllocationTable.Format();

            CalcOffsets();
        }

        public uint FirstDataSector;
        public uint SectorsPerCluster;
        public uint ClusterSize;
        public uint BytesPerSector;


        private void CalcOffsets()
        {
            FirstDataSector =(uint)( BootSector.ReservedSectorCount + BootSector.NumberFATs * BootSector.FATsz16);
            SectorsPerCluster = BootSector.SectorsPerCluster;
            BytesPerSector = BootSector.BytesPerSec;
            ClusterSize = SectorsPerCluster * BytesPerSector;
        }
        private uint FirstSectorOfCluster(uint cluster)
        {
            return (cluster - 2) * SectorsPerCluster + FirstDataSector;
        }

        public override byte[] ReadCluster(uint cluster)
        {
            byte[] data = new byte[ClusterSize];
            
            uint Sector = FirstSectorOfCluster(cluster);
            byte[] read = new byte[p.BlockSize];
            for (int i = 0; i < SectorsPerCluster; i++)
            {
                p.ReadBlock(Sector++, read);
                Array.Copy(read, 0, data, i * BytesPerSector, BytesPerSector);
            }
            return data;
        }

        public override void WriteCluster(uint cluster, byte[] bytes)
        {
            uint Sector = FirstSectorOfCluster(cluster);
            
            byte[] dest = new byte[BytesPerSector];

            for (int i = 0; i < SectorsPerCluster; i++)
            {
                Array.Copy(bytes, i * BytesPerSector, dest,0, BytesPerSector);
                p.WriteBlock(Sector++, dest);
            }

        }
    }
}
