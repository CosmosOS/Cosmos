using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Common.Extensions;

namespace Cosmos.HAL.BlockDevice
{
    public class EBR
    {
        public List<PartInfo> Partitions = new List<PartInfo>();

        public class PartInfo
        {
            public readonly byte SystemID;
            public readonly UInt32 StartSector;
            public readonly UInt32 SectorCount;

            public PartInfo(byte aSystemID, UInt32 aStartSector, UInt32 aSectorCount)
            {
                SystemID = aSystemID;
                StartSector = aStartSector;
                SectorCount = aSectorCount;
            }
        }

        public EBR(byte[] aEBR)
        {
            ParsePartition(aEBR, 446);
            ParsePartition(aEBR, 462);
        }

        protected void ParsePartition(byte[] aEBR, UInt32 aLoc)
        {
            byte xSystemID = aEBR[aLoc + 4];
            // SystemID = 0 means no partition
            //TODO: Extended Partition Table
            if (xSystemID == 0x5 || xSystemID == 0xF || xSystemID == 0x85)
            {
                //Another EBR Detected
            }
            else if (xSystemID != 0)
            {
                UInt32 xStartSector = BitConverter.ToUInt32(aEBR, (int)aLoc + 8);
                UInt32 xSectorCount = BitConverter.ToUInt32(aEBR, (int)aLoc + 12);

                var xPartInfo = new PartInfo(xSystemID, xStartSector, xSectorCount);
                Partitions.Add(xPartInfo);
            }
        }
    }
}
