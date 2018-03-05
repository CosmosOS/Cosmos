using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Sys.FileSystem;

namespace Cosmos.Sys.FileSystem.FAT32
{
    public class FileSystemInfo32
    {
        protected Partition p;
        protected byte[] Sector;
        protected uint Offset;

        public FileSystemInfo32(Partition p, uint Offset)
        {
            this.p = p;
            this.Offset = Offset;
            Sector = new byte[p.BlockSize];
        }

        public void Load()
        {
            p.ReadBlock(Offset, Sector);
        }
        public void Load(uint Offset)
        {
            this.Offset = Offset;
            Load();
        }
        public void Save(uint Offset)
        {
            this.Offset = Offset;
            Save();
        }

        public void Save()
        {
            p.WriteBlock(Offset, Sector);
        }

        public uint LeadSig
        {
            get
            {
                return BitConverter.ToUInt32(Sector, 0);
            }
            set
            {
                SetBytes(BitConverter.GetBytes(value), 0);
            }
        }
        public byte[] Reserved1
        {
            get
            {
                return GetBytes(4, 480);
            }
            set
            {
                SetBytes(value, 4, 480);
            }

        }
        public uint StrucSig
        {
            get
            {
                return BitConverter.ToUInt16(Sector, 484);
            }
            set
            {
                SetBytes(BitConverter.GetBytes(value), 484);
            }
        }
        public uint Free_Count
        {
            get
            {
                return BitConverter.ToUInt16(Sector, 488);
            }
            set
            {
                SetBytes(BitConverter.GetBytes(value), 488);
            }
        }
        public uint Nxt_Free
        {
            get
            {
                return BitConverter.ToUInt16(Sector, 492);
            }
            set
            {
                SetBytes(BitConverter.GetBytes(value), 492);
            }
        }
        public byte[] Reserved2
        {
            get
            {
                return GetBytes(496, 12);
            }
            set
            {
                SetBytes(value, 496, 12);
            }
        }
        public uint TrailSig
        {
            get
            {
                return BitConverter.ToUInt16(Sector, 508);
            }
            set
            {
                SetBytes(BitConverter.GetBytes(value), 508);
            }
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

    }
}
