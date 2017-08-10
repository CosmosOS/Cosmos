using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Sys.FileSystem;

namespace Cosmos.Sys.FileSystem.FAT32
{
    public abstract class BootSector
    {
        protected Partition p;
        protected byte[] Sector;

        public BootSector(Partition p)
        {
            this.p = p;
            Sector = new byte[p.BlockSize];
        }

        public byte jmpBoot0
        {
            get
            {
                return Sector[0];
            }
            set
            {
                Sector[0] = value;
            }
        }
        public byte jmpBoot1
        {
            get
            {
                return Sector[1];
            }
            set
            {
                Sector[1] = value;
            }
        }
        public byte jmpBoot2
        {
            get
            {
                return Sector[2];
            }
            set
            {
                Sector[2] = value;
            }
        }
        public byte[] OEMName
        {
            get
            {
                return GetBytes(3, 8);
            }
            set
            {
                SetBytes(value, 3, 8);
            }

        }
        public ushort BytesPerSec
        {
            get
            {
                return BitConverter.ToUInt16(Sector, 11);
            }
            set
            {
                SetBytes(BitConverter.GetBytes(value), 11);
            }
        }
        public byte SectorsPerCluster
        {
            get
            {
                return Sector[13];
            }
            set
            {
                Sector[13] = value;
            }
        }
        public ushort ReservedSectorCount
        {
            get
            {
                return BitConverter.ToUInt16(Sector, 14);
            }
            set
            {
                SetBytes(BitConverter.GetBytes(value), 14);
            }
        }
        public byte NumberFATs
        {
            get
            {
                return Sector[16];
            }
            set
            {
                Sector[16] = value;
            }
        }
        public ushort RootEntryCount
        {
            get
            {
                return BitConverter.ToUInt16(Sector, 17);
            }
            set
            {
                SetBytes(BitConverter.GetBytes(value), 27);
            }
        }
        public ushort TotalSectors16
        {
            get
            {
                return BitConverter.ToUInt16(Sector, 19);
            }
            set
            {
                SetBytes(BitConverter.GetBytes(value), 19);
            }
        }
        public byte Media
        {
            get
            {
                return Sector[21];
            }
            set { Sector[21] = value; }
        }
        public ushort FATsz16
        {
            get
            {
                return BitConverter.ToUInt16(Sector, 22);
            }
            set
            {
                SetBytes(BitConverter.GetBytes(value), 22);
            }
        }
        public ushort SectorsPerTrack
        {
            get
            {
                return BitConverter.ToUInt16(Sector, 24);
            }
            set
            {
                SetBytes(BitConverter.GetBytes(value), 24);
            }
        }
        public ushort NumberOfHeads
        {
            get
            {
                return BitConverter.ToUInt16(Sector, 26);
            }
            set
            {
                SetBytes(BitConverter.GetBytes(value), 26);
            }
        }
        public uint HiddenSectors
        {
            get
            {
                return BitConverter.ToUInt32(Sector, 28);
            }
            set
            {
                SetBytes(BitConverter.GetBytes(value), 28);
            }
        }
        public uint TotalSectors32
        {
            get
            {
                return BitConverter.ToUInt32(Sector, 32);
            }
            set
            {
                SetBytes(BitConverter.GetBytes(value), 32);
            }
        }
        public ushort Signature
        {
            get
            {
                return BitConverter.ToUInt16(Sector, Sector.Length-2);
            }
            set
            {
                SetBytes(BitConverter.GetBytes(value), (uint)Sector.Length - 2);
            }
        }

        public abstract byte DriveNumber
        {
            get;
            set;
        }
        public abstract byte Reserved1
        {
            get;
            set;
        }
        public abstract byte BootSig
        {
            get;
            set;
        }
        public abstract byte[] VolId
        {
            get;
            set;
        }
        public abstract byte[] VolLab
        {
            get;
            set;
        }
        public abstract byte[] filSysType
        {
            get;
            set;
        }

        protected void SetBytes(byte[] b, uint offset)
        {
            Array.Copy(b, 0, Sector, offset, b.Length);
        }
        protected void SetBytes(byte[] b, uint offset, uint length)
        {
            if (b.Length != length)
            {
                byte[] t = new byte[length];
                Array.Copy(b, t, 0);
                b = t;
            }
            Array.Copy(b, 0, Sector, offset, length);
        }
        protected byte[] GetBytes(uint offset, uint length)
        {
            byte[] t = new byte[length];
            Array.Copy(Sector, offset, t, 0, length);
            return t;
        }

        public void Refresh()
        {
            p.ReadBlock(0, Sector);
        }
        public void Save()
        {
            p.WriteBlock(0, Sector);
        }
    }
    public class BootSectorFAT12 : BootSector
    {
        public BootSectorFAT12(Partition p) : base(p) { }
        public override byte DriveNumber
        {
            get
            {
                return Sector[36];
            }
            set
            {
                Sector[36] = value;
            }
        }
        public override byte Reserved1
        {
            get
            {
                return Sector[37];
            }
            set
            {
                Sector[37] = value;
            }
        }
        public override byte BootSig
        {
            get
            {
                return Sector[38];
            }
            set
            {
                Sector[38] = value;
            }
        }
        public override byte[] VolId
        {
            get
            {
                return GetBytes(39, 4);
            }
            set
            {
                SetBytes(value, 39, 4);
            }
        }
        public override byte[] VolLab
        {
            get
            {
                return GetBytes(43, 11);
            }
            set
            {
                SetBytes(value, 43, 11);
            }
        }
        public override byte[] filSysType
        {
            get
            {
                return GetBytes(54, 8);
            }
            set
            {
                SetBytes(value, 54, 8);
            }
        }
    }
    public class BootSectorFAT16 : BootSectorFAT12
    {
        public BootSectorFAT16(Partition p) : base(p) { }
    }
    public class BootSectorFAT32 : BootSector
    {
        public BootSectorFAT32(Partition p) : base(p) { }
        public uint BPB_FATSz32
        {
            get
            {
                return BitConverter.ToUInt32(Sector, 36);
            }
            set
            {
                SetBytes(BitConverter.GetBytes(value), 36);
            }
        }
        public ushort BPB_ExtFlags
        {
            get
            {
                return BitConverter.ToUInt16(Sector, 40);
            }
            set
            {
                SetBytes(BitConverter.GetBytes(value), 40);
            }
        }
        public ushort BPB_FSVer
        {
            get
            {
                return BitConverter.ToUInt16(Sector, 42);
            }
            set
            {
                SetBytes(BitConverter.GetBytes(value), 42);
            }
        }
        public uint BPB_RootClus
        {
            get
            {
                return BitConverter.ToUInt32(Sector, 44);
            }
            set
            {
                SetBytes(BitConverter.GetBytes(value), 44);
            }
        }
        public ushort BPB_FSInfo
        {
            get
            {
                return BitConverter.ToUInt16(Sector, 48);
            }
            set
            {
                SetBytes(BitConverter.GetBytes(value), 48);
            }
        }
        public ushort BPB_BkBootSec
        {
            get
            {
                return BitConverter.ToUInt16(Sector, 50);
            }
            set
            {
                SetBytes(BitConverter.GetBytes(value), 50);
            }
        }
        public byte[] BPB_Reserved
        {
            get
            {
                return GetBytes(52, 12);

            }
            set
            {
                SetBytes(value, 52, 12);
            }
        }
        public override byte DriveNumber
        {
            get
            {
                return Sector[64];
            }
            set
            {
                Sector[64] = value;
            }
        }
        public override byte Reserved1
        {
            get
            {
                return Sector[65];
            }
            set
            {
                Sector[65] = value;
            }
        }
        public override byte BootSig
        {
            get
            {
                return Sector[66];
            }
            set
            {
                Sector[66] = value;
            }
        }
        public override byte[] VolId
        {
            get
            {
                return GetBytes(67, 4);
            }
            set
            {
                SetBytes(value, 67, 4);
            }
        }
        public override byte[] VolLab
        {
            get
            {
                return GetBytes(71, 11);
            }
            set
            {
                SetBytes(value, 71, 11);
            }
        }
        public override byte[] filSysType
        {
            get
            {
                return GetBytes(82, 8);
            }
            set
            {
                SetBytes(value, 82, 8);
            }
        }

    }
}
