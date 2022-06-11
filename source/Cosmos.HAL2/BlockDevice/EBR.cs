using System;
using System.Collections.Generic;

namespace Cosmos.HAL.BlockDevice;

public class EBR
{
    public List<PartInfo> Partitions = new();

    public EBR(byte[] aEBR)
    {
        ParsePartition(aEBR, 446);
        ParsePartition(aEBR, 462);
    }

    protected void ParsePartition(byte[] aEBR, uint aLoc)
    {
        var xSystemID = aEBR[aLoc + 4];
        // SystemID = 0 means no partition
        //TODO: Extended Partition Table
        if (xSystemID == 0x5 || xSystemID == 0xF || xSystemID == 0x85)
        {
            //Another EBR Detected
        }
        else if (xSystemID != 0)
        {
            var xStartSector = BitConverter.ToUInt32(aEBR, (int)aLoc + 8);
            var xSectorCount = BitConverter.ToUInt32(aEBR, (int)aLoc + 12);

            var xPartInfo = new PartInfo(xSystemID, xStartSector, xSectorCount);
            Partitions.Add(xPartInfo);
        }
    }

    public class PartInfo
    {
        public readonly uint SectorCount;
        public readonly uint StartSector;
        public readonly byte SystemID;

        public PartInfo(byte aSystemID, uint aStartSector, uint aSectorCount)
        {
            SystemID = aSystemID;
            StartSector = aStartSector;
            SectorCount = aSectorCount;
        }
    }
}
