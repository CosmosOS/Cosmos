using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Common.Extensions;

namespace Cosmos.HAL.BlockDevice
{
    // Its not a BlockDevice, but its related to "fixed" devices
    // and necessary to create partition block devices
    // Im not comfortable with MBR and Partition being in Hardware ring and would prefer
    // them in the system ring, but there are issues relating to moving it there.
    public class MBR
    {
        // TODO Lock this so other code cannot add/remove/modify the list
        // Can make a locked list class which wraps a list<>

        /// <summary>
        /// List of Partitions
        /// </summary>
        public List<PartInfo> Partitions = new List<PartInfo>();

        /// <summary>
        /// EBR location. Set to 0 if no EBR detected
        /// </summary>
        public UInt32 EBRLocation = 0;
        /// <summary>
        /// Host device that has all of the partitions
        /// </summary>
        public BlockDevice Host;
        /// <summary>
        /// Partiton Info
        /// </summary>
        public class PartInfo
        {
            /// <summary>
            /// System ID
            /// </summary>
            public readonly byte SystemID;
            /// <summary>
            /// Start sector
            /// </summary>
            public readonly UInt32 StartSector;
            /// <summary>
            /// Sector count
            /// </summary>
            public readonly UInt32 SectorCount;
            /// <summary>
            /// Partition info constructor.
            /// </summary>
            /// <param name="aSystemID">System ID</param>
            /// <param name="aStartSector">Start sector</param>
            /// <param name="aSectorCount">Sector count</param>
            public PartInfo(byte aSystemID, UInt32 aStartSector, UInt32 aSectorCount)
            {
                SystemID = aSystemID;
                StartSector = aStartSector;
                SectorCount = aSectorCount;
            }
        }
        /// <summary>
        /// MBR contructor
        /// </summary>
        /// <param name="Host">Blockdevice that has all of the partitions</param>
        public MBR(BlockDevice Host)
        {
            this.Host = Host;
            Init();
        }
        /// <summary>
        /// Parses a partition
        /// </summary>
        /// <param name="aMBR">The MBR</param>
        /// <param name="aLoc">Location of the partition in the MBR</param>
        protected void ParsePartition(byte[] aMBR, UInt32 aLoc)
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
                UInt32 xStartSector = BitConverter.ToUInt32(aMBR, (int)aLoc + 8);
                UInt32 xSectorCount = BitConverter.ToUInt32(aMBR, (int)aLoc + 12);

                var xPartInfo = new PartInfo(xSystemID, xStartSector, xSectorCount);
                Partitions.Add(xPartInfo);
            }
        }
        /// <summary>
        /// Removes a partiton
        /// </summary>
        /// <param name="partIndex">The partition index. 0 = first partition, 1 = second partition, 2 = third partition, 3 = forth partition</param>
        /// <exception cref="NotImplementedException">Occurs when an Invaild partIndex is specified</exception>
        public void RemovePartition(int partIndex)
        {
            //get location from partition number
            int location;
            if (partIndex == 0)
            {
                location = 446;
            }
            else if (partIndex == 1)
            {
                location = 462;
            }
            else if (partIndex == 2)
            {
                location = 478;
            }
            else if (partIndex == 3)
            {
                location = 494;
            }
            else
            {
                throw new NotImplementedException();
            }

            //remove that partition
            byte[] mbr = Host.NewBlockArray(1);
            Host.ReadBlock(0, 1, ref mbr);
            for (int i = location; i < location + 16; i++)
            {
                mbr[i] = 0;
            }
            Host.WriteBlock(0, 1, ref mbr);
            Init();
        }
        /// <summary>
        /// Creates a partiton
        /// </summary>
        /// <param name="sizeInMB">Size of the partition in Megabytes</param>
        /// <exception cref="NotImplementedException">Occurs when an there are too many partitions</exception>
        public void CreatePartition(int sizeInMB)
        {
            int location;
            int startingSector = 63;
            uint amountOfSectors = (uint)(sizeInMB * 1024 * 1024 / 512);
            //TODO: Check if partition is too big

            if (Partitions.Count == 0)
            {
                location = 446;
                startingSector = 63;
            }
            else if (Partitions.Count == 1)
            {
                location = 462;
                startingSector = (int)(Partitions[0].StartSector + Partitions[0].SectorCount);
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
            mbrData[location + 4] = 83;//normal partition
            mbrData[location + 5] = 0xFE; //ending head
            mbrData[location + 6] = 0x3F; //Ending Sector
            mbrData[location + 7] = 0x40;//Ending Cylinder

            //Starting Sector
            byte[] startingSectorBytes = BitConverter.GetBytes(startingSector);
            mbrData[location + 8] = startingSectorBytes[0];
            mbrData[location + 9] = startingSectorBytes[1];
            mbrData[location + 10] = startingSectorBytes[2];
            mbrData[location + 11] = startingSectorBytes[3];

            //Total Sectors in partition
            byte[] total = BitConverter.GetBytes(amountOfSectors);
            mbrData[location + 12] = total[0];
            mbrData[location + 13] = total[1];
            mbrData[location + 14] = total[2];
            mbrData[location + 15] = total[3];

            //Boot flag
            byte[] boot = BitConverter.GetBytes((ushort)0xAA55);
            mbrData[510] = boot[0];
            mbrData[511] = boot[1];

            //Save the data
            Host.WriteBlock(0, 1, ref mbrData);
            Init();
        }
        /// <summary>
        /// Removes all partitions.
        /// </summary>
        public void RemoveAllPartitions()
        {
            for (int i = 0; i < 4; i++)
            {
                RemovePartition(i);
            }
        }
        /// <summary>
        /// Reads partition info from disk, and adds it to the Partitions list
        /// </summary>
        private void Init()
        {
            Partitions.Clear();

            byte[] mbr = Host.NewBlockArray(1);
            Host.ReadBlock(0, 1, ref mbr);

            ParsePartition(mbr, 446);
            ParsePartition(mbr, 462);
            ParsePartition(mbr, 478);
            ParsePartition(mbr, 494);
        }
    }
}
