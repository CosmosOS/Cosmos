using System;
using System.Collections.Generic;

namespace Cosmos.HAL.BlockDevice;

public class GPT
{
    // Signature: "EFI PART"
    private const ulong EFIParitionSignature = 0x5452415020494645;

    public List<GPartInfo> Partitions = new();

    public GPT(BlockDevice aBlockDevice)
    {
        var GPTHeader = new byte[512];
        aBlockDevice.ReadBlock(1, 1, ref GPTHeader);

        // Start of parition entries
        var partEntryStart = BitConverter.ToUInt64(GPTHeader, 72);
        var numParitions = BitConverter.ToUInt32(GPTHeader, 80);
        var partSize = BitConverter.ToUInt32(GPTHeader, 84);

        var paritionsPerSector = 512 / partSize;

        for (ulong i = 0; i < numParitions / paritionsPerSector; i++)
        {
            var partData = new byte[512];
            aBlockDevice.ReadBlock(partEntryStart + i, 1, ref partData);

            for (uint j = 0; j < paritionsPerSector; j++)
            {
                ParseParition(partData, j * partSize);
            }
        }
    }

    private void ParseParition(byte[] partData, uint off)
    {
        var guidArray = new byte[16];

        Array.Copy(partData, off, guidArray, 0, 16);
        var partType = new Guid(guidArray);

        Array.Copy(partData, off + 16, guidArray, 0, 16);
        var partGuid = new Guid(guidArray);

        var startLBA = BitConverter.ToUInt64(partData, (int)(off + 32));
        var endLBA = BitConverter.ToUInt64(partData, (int)(off + 40));

        // endLBA + 1 because endLBA is inclusive
        var count = endLBA + 1 - startLBA;

        if (partType != Guid.Empty && partGuid != Guid.Empty)
        {
            Partitions.Add(new GPartInfo(partType, partGuid, startLBA, count));
        }
    }

    public static bool IsGPTPartition(BlockDevice aBlockDevice)
    {
        var GPTHeader = new byte[512];
        aBlockDevice.ReadBlock(1, 1, ref GPTHeader);

        var signature = BitConverter.ToUInt64(GPTHeader, 0);

        return signature == EFIParitionSignature;
    }

    public class GPartInfo
    {
        public readonly Guid ParitionGuid;
        public readonly Guid ParitionType;
        public readonly ulong SectorCount;
        public readonly ulong StartSector;

        public GPartInfo(Guid aParitionType, Guid aParitionGuid, ulong aStartSector, ulong aSectorCount)
        {
            ParitionType = aParitionType;
            ParitionGuid = aParitionGuid;
            StartSector = aStartSector;
            SectorCount = aSectorCount;
        }
    }
}
