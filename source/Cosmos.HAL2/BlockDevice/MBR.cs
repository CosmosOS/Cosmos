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

        public MBR(byte[] aMBR)
        {
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
    }
}
