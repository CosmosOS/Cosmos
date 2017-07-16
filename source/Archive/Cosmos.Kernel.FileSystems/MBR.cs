using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Hardware2;

namespace Cosmos.Sys.FileSystem
{
    public class MBR : PartitionManager
    {
        protected Disk blockdevice;

        public static void Initialise()
        {
            for (int i = 0; i < Cosmos.Hardware2.Device.Devices.Count; i++)
            {
                Device d = Cosmos.Hardware2.Device.Devices[i];
                if (d is Disk)
                {
                    MBR mbr = new MBR(d as Disk);
                    if (mbr.IsValid())
                        Cosmos.Hardware2.Device.Devices.Add(mbr);
                }
            }
        }

        public MBR(Disk bd)
        {
            this.blockdevice = bd;
            Partition = new IBMPartitionInformation[4];
            Partition[0] = new IBMPartitionInformation(this, 0);
            Partition[1] = new IBMPartitionInformation(this, 1);
            Partition[2] = new IBMPartitionInformation(this, 2);
            Partition[3] = new IBMPartitionInformation(this, 3);
            Refresh();
        }

        private byte[] Sector;

        public byte[] Code
        {

            get
            {
                byte[] b = new byte[440];
                Array.Copy(Sector, b, 440);
                return b;
            }
            set
            {
                byte[] b;
                if (value.Length != 440)
                {
                    b = new byte[440];
                    Array.Copy(value, b, 0);
                }
                else
                {
                    b = value;
                }
                Array.Copy(value, Sector, 0);
            }

        }

        public uint DiskSignature
        {
            get
            {
                return BitConverter.ToUInt32(Sector, 440);
            }
            set
            {
                byte[] b = BitConverter.GetBytes(value);
                Sector[440] = b[0];
                Sector[441] = b[1];
                Sector[442] = b[2];
                Sector[443] = b[3];
            }
        }
        public ushort Null
        {
            get
            {
                return BitConverter.ToUInt16(Sector, 444);
            }
            set
            {
                byte[] b = BitConverter.GetBytes(value);
                Sector[444] = b[0];
                Sector[445] = b[1];
            }
        }

        public IBMPartitionInformation[] Partition;

        public ushort MBRSignature
        {
            get
            {
                return BitConverter.ToUInt16(Sector,510);
            }
        }

        public void Refresh()
        {
            // disable and remove any partitions we already added
            blockdevice.ReadBlock(0, Sector);
            //add partitons back to device list
        }
        public void Save()
        {
            Sector[510] = 0x55;
            Sector[511] = 0xaa;
            blockdevice.WriteBlock(0, Sector);
        }

        public bool IsValid()
        {
            return MBRSignature == 0x55aa;
        }

        public class IBMPartitionInformation
        {
            private MBR mbr;
            private int offset;
            public IBMPartitionInformation(MBR mbr, int index)
            {
                this.mbr=mbr;
                this.offset=446+16*index;
            }

            public byte Status
            {
                get
                {
                    return mbr.Sector[offset];
                }
                set
                {
                    mbr.Sector[offset] = value;
                }
            }

            public bool Bootable
            {
                get
                {
                    return (Status & 0x80) == 0x80;
                }
                set
                {
                    Status = (byte)((Status & 0x7f) | (value ? 0x80 : 0));
                }
            }

            public byte PartitionType
            {
                get
                {
                    return mbr.Sector[offset + 4];
                }
                set
                {
                    mbr.Sector[offset + 4] = value;
                }
            }
            public uint StartLBA
            {
                get
                {
                    return BitConverter.ToUInt32(mbr.Sector,offset+8);
                }
                set
                {
                    byte[] b = BitConverter.GetBytes(value);
                    mbr.Sector[offset + 8] = b[0];
                    mbr.Sector[offset + 9] = b[1];
                    mbr.Sector[offset + 10] = b[2];
                    mbr.Sector[offset + 11] = b[3];
                }
            }
            public uint LengthLBA
            {
                get
                {
                    return BitConverter.ToUInt32(mbr.Sector, offset + 12);
                }
                set
                {
                    byte[] b = BitConverter.GetBytes(value);
                    mbr.Sector[offset + 12] = b[0];
                    mbr.Sector[offset + 13] = b[1];
                    mbr.Sector[offset + 14] = b[2];
                    mbr.Sector[offset + 15] = b[3];
                }
            }

            public bool ValidPartition()
            {
                return (PartitionType > 0 && (Status == 0x00 || Status == 0x80));
            }

            public Partition GetPartitionDevice()
            {
                if (ValidPartition())
                    return new MBRPartition(this, mbr);
                return null;
            }
        }

        public class MBRPartition : Partition
        {
            private MBR Mbr;
            private uint Start, Length;
            private BlockDevice blockDev;

            public MBRPartition(IBMPartitionInformation info, MBR mbr)
            {
                Mbr = mbr;
                Start = info.StartLBA;
                Length = info.LengthLBA;
                Identifier = info.PartitionType;
                blockDev = mbr.blockdevice;
            }

            public override uint BlockSize
            {
                get { return blockDev.BlockSize; }
            }

            public override ulong BlockCount
            {
                get { return Length; }
            }

            public override void ReadBlock(ulong aBlock, byte[] aBuffer)
            {
                blockDev.ReadBlock(aBlock + Start, aBuffer);
            }

            public override void WriteBlock(ulong aBlock, byte[] aContents)
            {
                blockDev.WriteBlock(aBlock + Start, aContents);
            }

            public override string Name
            {
                get { return "MBR Partition [Type="+Identifier + "] in MBR " + blockDev.Name; }
            }
        }

        public override string Name
        {
            get { throw new NotImplementedException(); }
        }
    }

    
    public abstract class Disk : BlockDevice
    {
    }
    public abstract class PartitionManager : Device
    {
    }

    public abstract class Partition : BlockDevice
    {
        public object Identifier;
    }
}
