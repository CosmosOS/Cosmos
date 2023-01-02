using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Common.Extensions;
using Cosmos.Debug.Kernel;

namespace Cosmos.HAL.BlockDevice
{
    public class GPT
    {
        // Signature: "EFI PART"
        private const ulong EFIParitionSignature = 0x5452415020494645;

        public List<GPartInfo> Partitions = new List<GPartInfo>();

        public class GPartInfo
        {
            public readonly Guid ParitionType;
            public readonly Guid ParitionGuid;
            public readonly ulong StartSector;
            public readonly ulong SectorCount;

            public GPartInfo(Guid aParitionType, Guid aParitionGuid, ulong aStartSector, ulong aSectorCount)
            {
                ParitionType = aParitionType;
                ParitionGuid = aParitionGuid;
                StartSector = aStartSector;
                SectorCount = aSectorCount;
            }
        }

        public GPT(BlockDevice aBlockDevice)
        {
            byte[] GPTHeader = new byte[512];
            aBlockDevice.ReadBlock(1, 1, ref GPTHeader);

            // Start of parition entries
            ulong partEntryStart = BitConverter.ToUInt64(GPTHeader, 72);
            uint numParitions = BitConverter.ToUInt32(GPTHeader, 80);
            uint partSize = BitConverter.ToUInt32(GPTHeader, 84);

            uint paritionsPerSector = 512 / partSize;

            for (ulong i = 0; i < numParitions/paritionsPerSector; i++)
            {

                byte[] partData = new byte[512];
                aBlockDevice.ReadBlock(partEntryStart + i, 1, ref partData);

                for (uint j = 0; j < paritionsPerSector; j++)
                {
                    ParseParition(partData, j * partSize);
                }
            }
        }

        private void ParseParition(byte[] partData, uint off)
        {
            byte[] guidArray = new byte[16];

            Array.Copy(partData, off, guidArray, 0, 16);
            var partType = new Guid(guidArray);

            Array.Copy(partData, off + 16, guidArray, 0, 16);
            var partGuid = new Guid(guidArray);

            ulong startLBA = BitConverter.ToUInt64(partData, (int)(off + 32));
            ulong endLBA = BitConverter.ToUInt64(partData, (int)(off + 40));

            // endLBA + 1 because endLBA is inclusive
            ulong count = endLBA + 1 - startLBA;

            if (partType != Guid.Empty && partGuid != Guid.Empty)
            {
                Partitions.Add(new GPartInfo(partType, partGuid, startLBA, count));
            }
        }

        public static bool IsGPTPartition(BlockDevice aBlockDevice)
        {
            byte[] GPTHeader = new byte[512];
            aBlockDevice.ReadBlock(1, 1, ref GPTHeader);

            ulong signature = BitConverter.ToUInt64(GPTHeader, 0);

            return signature == EFIParitionSignature;
        }
    }
}
