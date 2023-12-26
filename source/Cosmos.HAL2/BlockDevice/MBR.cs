using System;
using System.Collections.Generic;
using Cosmos.Core;

namespace Cosmos.HAL.BlockDevice
{
    // It's not a BlockDevice, but its related to "fixed" devices
    // and necessary to create partition block devices
    // I'm not comfortable with MBR and Partition being in Hardware ring and would prefer
    // them in the system ring, but there are issues relating to moving it there.
    public class MBR
    {
        // TODO Lock this so other code cannot add/remove/modify the list
        // Can make a locked list class which wraps a list<>
        public List<PartInfo> Partitions = new List<PartInfo>();

        public uint EBRLocation = 0;

        public class PartInfo
        {
            public readonly byte SystemID;
            public readonly uint StartSector;
            public readonly uint SectorCount;

            public PartInfo(byte aSystemID, uint aStartSector, uint aSectorCount)
            {
                SystemID = aSystemID;
                StartSector = aStartSector;
                SectorCount = aSectorCount;
            }
        }

        public MBR(BlockDevice device)
        {
            var aMBR = device.NewBlockArray(1);
            device.ReadBlock(0, 1, ref aMBR);

            ParsePartition(aMBR, 446);
            ParsePartition(aMBR, 462);
            ParsePartition(aMBR, 478);
            ParsePartition(aMBR, 494);
        }

        protected void ParsePartition(byte[] aMBR, uint aLoc)
        {
            byte xSystemID = aMBR[aLoc + 4];
            // SystemID = 0 means no partition

            if (xSystemID == 0x5 || xSystemID == 0xF || xSystemID == 0x85)
            {
                //Extended Partition Detected
                //DOS only knows about 05, Windows 95 introduced 0F, Linux introduced 85
                //Search for logical volumes
                //http://thestarman.pcministry.com/asm/mbr/PartTables2.htm
                EBRLocation = BitConverter.ToUInt32(aMBR, (int)aLoc + 8);
            }
            else if (xSystemID != 0)
            {
                uint xStartSector = BitConverter.ToUInt32(aMBR, (int)aLoc + 8);
                uint xSectorCount = BitConverter.ToUInt32(aMBR, (int)aLoc + 12);

                var xPartInfo = new PartInfo(xSystemID, xStartSector, xSectorCount);
                Partitions.Add(xPartInfo);
            }
        }

        /// <summary>
        /// Creates a MBR partition table on a disk
        /// </summary>
        /// <param name="aDevice">The device to be written a partition table.</param>
        /// <exception cref="ArgumentNullException">
        /// <list type="bullet">
        /// <item>Thrown when aDevice is null.</item>
        /// </list>
        public void CreateMBR(BlockDevice aDevice)
        {
            if (aDevice == null)
            {
                throw new ArgumentNullException();
            }

            ManagedMemoryBlock mb = new ManagedMemoryBlock(512);
            mb.Fill(0);
            //Boot code
            mb.Write32(0, 0x1000B8FA);
            mb.Write32(4, 0x00BCD08E);
            mb.Write32(8, 0x0000B8B0);
            mb.Write32(12, 0xC08ED88E);
            mb.Write32(16, 0x7C00BEFB);
            mb.Write32(20, 0xB90600BF);
            mb.Write32(24, 0xA4F30200);
            mb.Write32(28, 0x000621EA);
            mb.Write32(32, 0x07BEBE07);
            mb.Write32(36, 0x0B750438);
            mb.Write32(40, 0x8110C683);
            mb.Write32(44, 0x7507FEFE);
            mb.Write32(48, 0xB416EBF3);
            mb.Write32(52, 0xBB01B002);
            mb.Write32(56, 0x80B27C00);
            mb.Write32(60, 0x8B01748A);
            mb.Write32(64, 0x13CD024C);
            mb.Write32(68, 0x007C00EA);
            mb.Write32(72, 0x00FEEB00);
            //Unique disk ID, is used to seperate different drives
            mb.Write32(440, (uint)aDevice.GetHashCode() * 0x5A5A);
            //Signature
            mb.Write16(510, 0xAA55);
            aDevice.WriteBlock(0, 1, ref mb.memory);
        }

        /// <summary>
        /// Writes the selected partitions information on the MBR
        /// </summary>
        /// <param name="partition">The partition whose information will be written.</param>
        /// <param name="PartitionNo">The partition number.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <list type="bullet">
        /// <item>Thrown when the partition number is larger or smaller than allowed partition number count.</item>
        /// </list>
        public void WritePartitionInformation(Partition partition, byte PartitionNo)
        {
            if (PartitionNo < 0 || PartitionNo > 3)
            {
                throw new ArgumentOutOfRangeException();
            }

            ManagedMemoryBlock mb = new ManagedMemoryBlock(512);
            partition.Host.ReadBlock(0, 1, ref mb.memory);
            //TO DO: Implement the CHS starting / ending sector adresses and partition type
            mb.Write8((uint)(446 + (PartitionNo * 16) + 4), 0x0B);
            mb.Write32((uint)(446 + (PartitionNo * 16) + 8), (uint) partition.StartingSector);
            mb.Write32((uint)(446 + (PartitionNo * 16) + 12), (uint) partition.BlockCount);
            partition.Host.WriteBlock(0, 1, ref mb.memory);
            ParsePartition(mb.memory, 446 + (uint)(PartitionNo * 16));
            
        }
    }
}
