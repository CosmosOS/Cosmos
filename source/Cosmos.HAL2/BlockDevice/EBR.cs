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
            public readonly ulong StartSector;
            public readonly ulong SectorCount;

            public PartInfo(byte aSystemID, ulong aStartSector, ulong aSectorCount)
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

        protected void ParsePartition(byte[] aEBR, int aLoc)
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
                ulong xStartSector = BitConverter.ToUInt64(aEBR, aLoc + 8);
                ulong xSectorCount = BitConverter.ToUInt64(aEBR, aLoc + 12);

                var xPartInfo = new PartInfo(xSystemID, xStartSector, xSectorCount);
                Partitions.Add(xPartInfo);
            }
        }
    }
}
