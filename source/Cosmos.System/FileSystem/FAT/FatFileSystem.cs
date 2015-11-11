using System;
using System.Collections.Generic;
using System.IO;
using Cosmos.Common.Extensions;
using Cosmos.HAL.BlockDevice;
using Cosmos.System.FileSystem.FAT.Listing;
using Cosmos.System.FileSystem.Listing;

namespace Cosmos.System.FileSystem.FAT
{
    public class FatFileSystem : FileSystem
    {
        public readonly uint BytesPerSector;
        public readonly uint SectorsPerCluster;
        public readonly uint BytesPerCluster;

        public readonly uint ReservedSectorCount;
        public readonly uint TotalSectorCount;
        public readonly uint ClusterCount;

        public readonly uint NumberOfFATs;
        public readonly uint FatSectorCount;

        public readonly uint RootSector;      // FAT12/16
        public readonly uint RootSectorCount; // FAT12/16, FAT32 remains 0
        public readonly uint RootCluster;     // FAT32
        public readonly uint RootEntryCount;

        public readonly uint DataSector;      // First Data Sector
        public readonly uint DataSectorCount;

        private enum FatTypeEnum
        {
            Unknown,
            Fat12,
            Fat16,
            Fat32
        }

        private readonly FatTypeEnum mFatType;

        private readonly BlockDevice mDevice;

        public FatFileSystem(BlockDevice aDevice)
        {
            mDevice = aDevice;
            byte[] xBPB = mDevice.NewBlockArray(1);

            mDevice.ReadBlock(0UL, 1U, xBPB);

            ushort xSig = xBPB.ToUInt16(510);
            if (xSig != 0xAA55)
            {
                throw new Exception("FAT signature not found.");
            }

            BytesPerSector = xBPB.ToUInt16(11);
            SectorsPerCluster = xBPB[13];
            BytesPerCluster = BytesPerSector * SectorsPerCluster;
            ReservedSectorCount = xBPB.ToUInt16(14);
            NumberOfFATs = xBPB[16];
            RootEntryCount = xBPB.ToUInt16(17);

            TotalSectorCount = xBPB.ToUInt16(19);
            if (TotalSectorCount == 0)
            {
                TotalSectorCount = xBPB.ToUInt32(32);
            }

            // FATSz
            FatSectorCount = xBPB.ToUInt16(22);
            if (FatSectorCount == 0)
            {
                FatSectorCount = xBPB.ToUInt32(36);
            }
            //Global.Dbg.Send("FAT Sector Count: " + FatSectorCount);

            DataSectorCount = TotalSectorCount -
                              (ReservedSectorCount + (NumberOfFATs * FatSectorCount) + ReservedSectorCount);

            // Computation rounds down.
            ClusterCount = DataSectorCount / SectorsPerCluster;
            // Determine the FAT type. Do not use another method - this IS the official and
            // proper way to determine FAT type.
            // Comparisons are purposefully < and not <=
            // FAT16 starts at 4085, FAT32 starts at 65525
            if (ClusterCount < 4085)
            {
                mFatType = FatTypeEnum.Fat12;
            }
            else if (ClusterCount < 65525)
            {
                mFatType = FatTypeEnum.Fat16;
            }
            else
            {
                mFatType = FatTypeEnum.Fat32;
            }

            if (mFatType == FatTypeEnum.Fat32)
            {
                RootCluster = xBPB.ToUInt32(44);
            }
            else
            {
                RootSector = ReservedSectorCount + (NumberOfFATs * FatSectorCount);
                RootSectorCount = (RootEntryCount * 32 + (BytesPerSector - 1)) / BytesPerSector;
            }
            DataSector = ReservedSectorCount + (NumberOfFATs * FatSectorCount) + RootSectorCount;
        }

        public void ReadFatTableSector(ulong xSectorNum, byte[] aData)
        {
            mDevice.ReadBlock(ReservedSectorCount + xSectorNum, 1, aData);
        }

        public bool FatEntryIsEOF(ulong aValue)
        {
            switch (mFatType)
            {
                case FatTypeEnum.Fat12:
                    return aValue >= 0x0FF8;
                case FatTypeEnum.Fat16:
                    return aValue >= 0xFFF8;
                default:
                    return aValue >= 0x0FFFFFF8;
            }
        }

        public ulong GetFatEntry(byte[] aSector, ulong aClusterNum, ulong aOffset)
        {
            switch (mFatType)
            {
                case FatTypeEnum.Fat12:
                    if (aOffset == (BytesPerSector - 1))
                    {
                        throw new Exception("TODO: Sector Span");
                        /* This cluster access spans a sector boundary in the FAT */
                        /* There are a number of strategies to handling this. The */
                        /* easiest is to always load FAT sectors into memory */
                        /* in pairs if the volume is FAT12 (if you want to load */
                        /* FAT sector N, you also load FAT sector N+1 immediately */
                        /* following it in memory unless sector N is the last FAT */
                        /* sector). It is assumed that this is the strategy used here */
                        /* which makes this if test for a sector boundary span */
                        /* unnecessary. */
                    }
                    // We now access the FAT entry as a WORD just as we do for FAT16, but if the cluster number is
                    // EVEN, we only want the low 12-bits of the 16-bits we fetch. If the cluster number is ODD
                    // we want the high 12-bits of the 16-bits we fetch.
                    uint xResult = aSector.ToUInt16(aOffset);
                    if ((aClusterNum & 0x01) == 0)
                    {
                        return xResult & 0x0FFF; // Even
                    }
                    return xResult >> 4; // Odd
                case FatTypeEnum.Fat16:
                    return aSector.ToUInt16(aOffset);
                default:
                    return aSector.ToUInt32(aOffset) & 0x0FFFFFFF;
            }
        }

        public byte[] NewClusterArray()
        {
            return new byte[BytesPerCluster];
        }

        public void ReadCluster(ulong aCluster, byte[] aData)
        {
            ulong xSector = DataSector + ((aCluster - 2) * SectorsPerCluster);
            mDevice.ReadBlock(xSector, SectorsPerCluster, aData);
        }

        public void WriteCluster(ulong aCluster, byte[] aData)
        {
            ulong xSector = DataSector + ((aCluster - 2) * SectorsPerCluster);

            FatHelpers.Debug("WriteCluster: xSector = " + xSector);
            FatHelpers.Debug("WriteCluster: SectorsPerCluster = " + SectorsPerCluster);
            FatHelpers.Debug("WriteCluster: aData.Length = " + aData.Length);

            mDevice.WriteBlock(xSector, SectorsPerCluster, aData);
            
        }

        public void GetFatTableSector(ulong aClusterNum, out uint oSector, out uint oOffset)
        {
            ulong xOffset = 0;
            if (mFatType == FatTypeEnum.Fat12)
            {
                // Multiply by 1.5 without using floating point, the divide by 2 rounds DOWN
                xOffset = aClusterNum + (aClusterNum / 2);
            }
            else if (mFatType == FatTypeEnum.Fat16)
            {
                xOffset = aClusterNum * 2;
            }
            else if (mFatType == FatTypeEnum.Fat32)
            {
                xOffset = aClusterNum * 4;
            }
            oSector = (uint)(xOffset / BytesPerSector);
            oOffset = (uint)(xOffset % BytesPerSector);
        }

        private List<FatDirectoryEntry> GetRoot()
        {
            byte[] xData;
            if (mFatType == FatTypeEnum.Fat32)
            {
                xData = NewClusterArray();
                ReadCluster(RootCluster, xData);
            }
            else
            {
                xData = mDevice.NewBlockArray(RootSectorCount);
                mDevice.ReadBlock(RootSector, RootSectorCount, xData);
            }
            return ReadDirectoryContents(xData, null);
        }

        private byte[] GetDirectoryEntryData(FatDirectoryEntry aDirectoryEntry)
        {
            byte[] xData;
            if (mFatType == FatTypeEnum.Fat32)
            {
                if (aDirectoryEntry.EntryType != DirectoryEntryTypeEnum.Unknown)
                {
                    xData = NewClusterArray();
                    ReadCluster(aDirectoryEntry.FirstClusterNum, xData);
                }
                else
                {
                    throw new Exception("Invalid directory entry type");
                }
            }
            else
            {
                if (aDirectoryEntry.EntryType != DirectoryEntryTypeEnum.Unknown)
                {
                    xData = mDevice.NewBlockArray(1);
                    mDevice.ReadBlock(aDirectoryEntry.FirstClusterNum, RootSectorCount, xData);
                }
                else
                {
                    throw new Exception("Invalid directory entry type");
                }
            }
            return xData;
        }

        private void SetDirectoryEntryData(FatDirectoryEntry aDirectoryEntry, byte[] aData)
        {
            if (aDirectoryEntry == null)
            {
                throw new ArgumentNullException("aDirectoryEntry");
            }

            if (aData == null)
            {
                throw new ArgumentNullException("aData");
            }

            if (aData.Length == 0)
            {
                FatHelpers.Debug("SetDirectoryEntryData: No data to write.");
                return;
            }

            FatHelpers.Debug("SetDirectoryEntryData: Name = " + aDirectoryEntry.Name);
            FatHelpers.Debug("SetDirectoryEntryData: Size = " + aDirectoryEntry.Size);
            FatHelpers.Debug("SetDirectoryEntryData: FirstClusterNum = " + aDirectoryEntry.FirstClusterNum);
            FatHelpers.Debug("SetDirectoryEntryData: aData.Length = " + aData.Length);

            if (aDirectoryEntry.EntryType != DirectoryEntryTypeEnum.Unknown)
            {
                if (mFatType == FatTypeEnum.Fat32)
                {
                    WriteCluster(aDirectoryEntry.FirstClusterNum, aData);
                }
                else
                {
                    mDevice.WriteBlock(aDirectoryEntry.FirstClusterNum, RootSectorCount, aData);
                }
            }
            else
            {
                throw new Exception("Invalid directory entry type");
            }
        }

        private List<FatDirectoryEntry> GetDirectoryContents(FatDirectoryEntry directory)
        {
            if (directory == null)
            {
                throw new ArgumentNullException("directory");
            }

            byte[] xData = GetDirectoryEntryData(directory);
            // todo: what about larger directories?

            return ReadDirectoryContents(xData, directory);
        }

        private List<FatDirectoryEntry> ReadDirectoryContents(byte[] xData, FatDirectoryEntry aDirectory)
        {
            var xResult = new List<FatDirectoryEntry>();
            //TODO: Change xLongName to StringBuilder
            string xLongName = "";
            string xName = "";
            for (uint i = 0; i < xData.Length; i = i + 32)
            {
                FatHelpers.Debug("-------------------------------------------------");
                byte xAttrib = xData[i + 11];
                byte xStatus = xData[i];

                FatHelpers.Debug("Attrib = " + xAttrib + ", Status = " + xStatus);
                if (xAttrib == FatDirectoryEntryAttributeConsts.LongName)
                {
                    byte xType = xData[i + 12];
                    byte xOrd = xData[i];
                    FatHelpers.Debug("Reading LFN with Seqnr " + xOrd + ", Type = " + xType);
                    if (xOrd == 0xE5)
                    {
                        FatHelpers.Debug("Skipping deleted entry");
                        continue;
                    }
                    if (xType == 0)
                    {
                        if ((xOrd & 0x40) > 0)
                        {
                            xLongName = "";
                        }
                        //TODO: Check LDIR_Ord for ordering and throw exception
                        // if entries are found out of order.
                        // Also save buffer and only copy name if a end Ord marker is found.
                        string xLongPart = xData.GetUtf16String(i + 1, 5);
                        // We have to check the length because 0xFFFF is a valid Unicode codepoint.
                        // So we only want to stop if the 0xFFFF is AFTER a 0x0000. We can determin
                        // this by also looking at the length. Since we short circuit the or, the length
                        // is rarely evaluated.
                        if (xData.ToUInt16(i + 14) != 0xFFFF || xLongPart.Length == 5)
                        {
                            xLongPart = xLongPart + xData.GetUtf16String(i + 14, 6);
                            if (xData.ToUInt16(i + 28) != 0xFFFF || xLongPart.Length == 11)
                            {
                                xLongPart = xLongPart + xData.GetUtf16String(i + 28, 2);
                            }
                        }
                        xLongName = xLongPart + xLongName;
                        xLongPart = null;
                        //TODO: LDIR_Chksum
                    }
                }
                else
                {
                    xName = xLongName;
                    if (xStatus == 0x00)
                    {
                        FatHelpers.Debug("End of directory");
                        break;
                    }
                    switch (xStatus)
                    {
                        case 0x05:
                            // Japanese characters - We dont handle these
                            break;
                        case 0xE5:
                            // Empty slot, skip it
                            break;
                        default:
                            if (xStatus >= 0x20)
                            {
                                if (xLongName.Length > 0)
                                {
                                    // Leading and trailing spaces are to be ignored according to spec.
                                    // Many programs (including Windows) pad trailing spaces although it
                                    // it is not required for long names.
                                    // As per spec, ignore trailing periods
                                    xName = xLongName.Trim();

                                    //If there are trailing periods
                                    int nameIndex = xName.Length - 1;
                                    if (xName[nameIndex] == '.')
                                    {
                                        //Search backwards till we find the first non-period character
                                        for (; nameIndex > 0; nameIndex--)
                                        {
                                            if (xName[nameIndex] != '.')
                                            {
                                                break;
                                            }
                                        }
                                        //Substring to remove the periods
                                        xName = xName.Substring(0, nameIndex + 1);
                                    }
                                    xLongName = "";
                                }
                                else
                                {
                                    string xEntry = xData.GetAsciiString(i, 11);
                                    xName = xEntry.Substring(0, 8).TrimEnd();
                                    string xExt = xEntry.Substring(8, 3).TrimEnd();
                                    if (xExt.Length > 0)
                                    {
                                        xName = xName + "." + xExt;
                                    }
                                }
                            }
                            break;
                    }
                }
                uint xFirstCluster = (uint)(xData.ToUInt16(i + 20) << 16 | xData.ToUInt16(i + 26));

                var xTest = xAttrib &
                            (FatDirectoryEntryAttributeConsts.Directory | FatDirectoryEntryAttributeConsts.VolumeID);
                if (xAttrib == FatDirectoryEntryAttributeConsts.LongName)
                {
                    // skip adding, as it's a LongFileName entry, meaning the next normal entry is the item with the name.
                    FatHelpers.Debug("Entry was a Long FileName entry. Current LongName = '" + xLongName + "'");
                }
                else if (xTest == 0)
                {
                    uint xSize = xData.ToUInt32(i + 28);
                    if (xSize == 0 && xName.Length == 0)
                    {
                        continue;
                    }
                    var xEntry = new FatDirectoryEntry(this, xName, xSize, xFirstCluster, aDirectory, i, DirectoryEntryTypeEnum.File);
                    xResult.Add(xEntry);
                    FatHelpers.Debug("Returning file '" + xEntry.Name + "', FirstCluster = " + xEntry.FirstClusterNum +
                                     ", Size = " + xEntry.Size);
                }
                else if (xTest == FatDirectoryEntryAttributeConsts.Directory)
                {
                    uint xSize = xData.ToUInt32(i + 28);
                    var xEntry = new FatDirectoryEntry(this, xName, xSize, xFirstCluster, aDirectory, i, DirectoryEntryTypeEnum.Directory);
                    FatHelpers.Debug("Returning directory '" + xEntry.Name + "', FirstCluster = " +
                                     xEntry.FirstClusterNum + ", Size = " + xEntry.Size);
                    xResult.Add(xEntry);
                }
                else if (xTest == FatDirectoryEntryAttributeConsts.VolumeID)
                {
                    FatHelpers.Debug("Directory entry is VolumeID");
                }
                else
                {
                    FatHelpers.Debug("Not sure what to do!");
                }
            }

            return xResult;
        }

        public static bool IsDeviceFAT(Partition aDevice)
        {
            byte[] xBPB = aDevice.NewBlockArray(1);
            aDevice.ReadBlock(0UL, 1U, xBPB);
            ushort xSig = xBPB.ToUInt16(510);
            if (xSig != 0xAA55)
            {
                return false;
            }
            return true;
        }

        public override void DisplayFileSystemInfo()
        {
            global::System.Console.WriteLine("-------File System--------");
            global::System.Console.WriteLine("Bytes per Cluster: " + BytesPerCluster);
            global::System.Console.WriteLine("Bytes per Sector: " + BytesPerSector);
            global::System.Console.WriteLine("Cluster Count: " + ClusterCount);
            global::System.Console.WriteLine("Data Sector: " + DataSector);
            global::System.Console.WriteLine("Data Sector Count: " + DataSectorCount);
            global::System.Console.WriteLine("FAT Sector Count: " + FatSectorCount);
            global::System.Console.WriteLine("FAT Type: " + mFatType);
            global::System.Console.WriteLine("Number of FATS: " + NumberOfFATs);
            global::System.Console.WriteLine("Reserved Sector Count: " + ReservedSectorCount);
            global::System.Console.WriteLine("Root Cluster: " + RootCluster);
            global::System.Console.WriteLine("Root Entry Count: " + RootEntryCount);
            global::System.Console.WriteLine("Root Sector: " + RootSector);
            global::System.Console.WriteLine("Root Sector Count: " + RootSectorCount);
            global::System.Console.WriteLine("Sectors per Cluster: " + SectorsPerCluster);
            global::System.Console.WriteLine("Total Sector Count: " + TotalSectorCount);
        }

        public override List<DirectoryEntry> GetDirectoryListing(DirectoryEntry baseDirectory)
        {
            List<DirectoryEntry> result = new List<DirectoryEntry>();
            List<FatDirectoryEntry> fatListing = new List<FatDirectoryEntry>();
            if (baseDirectory == null)
            {
                // get root folder
                fatListing = GetRoot();
            }
            else
            {
                fatListing = GetDirectoryContents((FatDirectoryEntry)baseDirectory);
            }

            for (int i = 0; i < fatListing.Count; i++)
            {
                result.Add(fatListing[i]);
            }
            return result;
        }

        public override DirectoryEntry GetRootDirectory(string name)
        {
            //TODO: Get size.
            return new FatDirectoryEntry(this, name, 0, RootCluster, null, 0, DirectoryEntryTypeEnum.Directory);
        }

        public override Stream GetFileStream(DirectoryEntry fileInfo)
        {
            if (fileInfo.EntryType == DirectoryEntryTypeEnum.File)
            {
                return new FatStream((FatDirectoryEntry)fileInfo);
            }

            FatHelpers.Debug("GetFileStream only works for file entries.");
            return null;
        }

        internal void SetDirectoryEntryMetadataValue(FatDirectoryEntry aDirectoryEntry, FatDirectoryEntryMetadata aEntryMetadata, uint aValue)
        {
            var xData = GetDirectoryEntryData(aDirectoryEntry.Parent);
            if (xData.Length > 0)
            {
                byte[] xValue = new byte[aEntryMetadata.DataLength];
                xValue.SetUInt32(0, (uint)aValue);

                uint offset = aDirectoryEntry.EntryHeaderDataOffset + aEntryMetadata.DataOffset;

                Array.Copy(xValue, 0, xData, offset, aEntryMetadata.DataLength);

                FatHelpers.Debug("SetDirectoryEntryMetadataValue: DataLength = " + aEntryMetadata.DataLength);
                FatHelpers.Debug("SetDirectoryEntryMetadataValue: DataOffset = " + aEntryMetadata.DataOffset);
                FatHelpers.Debug("SetDirectoryEntryMetadataValue: EntryHeaderDataOffset = " + aDirectoryEntry.EntryHeaderDataOffset);
                FatHelpers.Debug("SetDirectoryEntryMetadataValue: TotalOffset = " + offset);
                FatHelpers.Debug("SetDirectoryEntryMetadataValue: aValue = " + aValue);

                for (int i = 0; i < xValue.Length; i++)
                {
                    FatHelpers.DebugNumber(xValue[i]);
                }
            }

            SetDirectoryEntryData(aDirectoryEntry.Parent, xData);
        }

        internal void SetDirectoryEntryMetadataValue(FatDirectoryEntry aDirectoryEntry, FatDirectoryEntryMetadata aEntryMetadata, string aValue)
        {
            var xData = GetDirectoryEntryData(aDirectoryEntry.Parent);
            if (xData.Length > 0)
            {
                byte[] xValue = new byte[aEntryMetadata.DataLength];
                xValue = aValue.ToString().GetUtf8Bytes(0, aEntryMetadata.DataLength);

                uint offset = aDirectoryEntry.EntryHeaderDataOffset + aEntryMetadata.DataOffset;

                Array.Copy(xValue, 0, xData, offset, aEntryMetadata.DataLength);

                FatHelpers.Debug("SetDirectoryEntryMetadataValue: DataLength = " + aEntryMetadata.DataLength);
                FatHelpers.Debug("SetDirectoryEntryMetadataValue: DataOffset = " + aEntryMetadata.DataOffset);
                FatHelpers.Debug("SetDirectoryEntryMetadataValue: EntryHeaderDataOffset = " + aDirectoryEntry.EntryHeaderDataOffset);
                FatHelpers.Debug("SetDirectoryEntryMetadataValue: TotalOffset = " + offset);
                FatHelpers.Debug("SetDirectoryEntryMetadataValue: aValue = " + aValue);

                for (int i = 0; i < xValue.Length; i++)
                {
                    FatHelpers.DebugNumber(xValue[i]);
                }

                SetDirectoryEntryData(aDirectoryEntry.Parent, xData);
            }
        }

        public override DirectoryEntry CreateDirectory(string aPath)
        {
            return null;
        }
    }
}
