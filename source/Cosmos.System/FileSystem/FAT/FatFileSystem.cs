using global::System;
using global::System.Collections.Generic;

using Cosmos.Common.Extensions;
using Cosmos.Debug.Kernel;
using Cosmos.HAL.BlockDevice;
using Cosmos.System.FileSystem.FAT.Listing;
using Cosmos.System.FileSystem.Listing;

namespace Cosmos.System.FileSystem.FAT
{
    internal class FatFileSystem : FileSystem
    {
        internal class Fat
        {
            private readonly FatFileSystem mFileSystem;

            private readonly ulong mFirstSector;

            public Fat(FatFileSystem aFileSystem, ulong aFirstSector)
            {
                mFileSystem = aFileSystem;
                mFirstSector = aFirstSector;
            }

            public ulong[] GetFatChain(ulong aFirstCluster, uint aDataSize = 0)
            {
                Global.mFileSystemDebugger.SendInternal($"Fat.GetFatChain : aFirstCluster = {aFirstCluster}, aDataSize = {aDataSize}");
                var xReturn = new ulong[0];
                ulong xCurrentCluster = aFirstCluster;
                ulong xValue;

                uint xClustersRequired = aDataSize / mFileSystem.BytesPerCluster;
                if (aDataSize % mFileSystem.BytesPerCluster != 0)
                {
                    xClustersRequired++;
                }

                GetFatEntry(xCurrentCluster, out xValue);
                Array.Resize(ref xReturn, xReturn.Length + 1);
                xReturn[xReturn.Length - 1] = xCurrentCluster;
                Global.mFileSystemDebugger.SendInternal($"Fat.GetFatChain : xCurrentCluster = {xCurrentCluster}, xValue = {xValue}");
                while (!FatEntryIsEof(xValue))
                {
                    xCurrentCluster = xValue;
                    GetFatEntry(xCurrentCluster, out xValue);
                    Array.Resize(ref xReturn, xReturn.Length + 1);
                    if (!FatEntryIsEof(xValue))
                    {
                        xReturn[xReturn.Length - 1] = xValue;
                    }
                    else
                    {
                        xReturn[xReturn.Length - 1] = xCurrentCluster;
                    }
                    Global.mFileSystemDebugger.SendInternal($"Fat.GetFatChain : xCurrentCluster = {xCurrentCluster},  xValue = {xValue}");
                }

                if (xClustersRequired > xReturn.Length)
                {
                    ulong xNewClusters = (uint)xReturn.Length - xClustersRequired;
                    Global.mFileSystemDebugger.SendInternal($"Fat.GetFatChain : Allocating {xNewClusters} new clusters.");
                    for (ulong i = 0; i < xNewClusters; i++)
                    {
                        xCurrentCluster = GetNextUnallocatedFatEntry();
                        ulong xLastFatEntry = xReturn[xReturn.Length - 1];
                        SetFatEntry(xLastFatEntry, xCurrentCluster);
                        SetFatEntry(xCurrentCluster, FatEntryEofValue());
                        Array.Resize(ref xReturn, xReturn.Length + 1);
                        xReturn[xReturn.Length - 1] = xCurrentCluster;
                        Global.mFileSystemDebugger.SendInternal($"Fat.GetFatChain : xCurrentCluster = {xCurrentCluster}");
                    }
                }

                return xReturn;
            }

            public uint GetNextUnallocatedFatEntry()
            {
                var xSector = new byte[mFileSystem.BytesPerSector];
                uint xEntryNumber = 0;

                for (uint i = 0; i < mFileSystem.FatSectorCount; i++)
                {
                    ReadFatTableSector(i, xSector);
                    for (uint j = 0; j < xSector.Length / 4; j += 4)
                    {
                        uint xEntryValue = xSector.ToUInt32(j);
                        xEntryNumber++;
                        if (xEntryValue == 0)
                        {
                            Global.mFileSystemDebugger.SendInternal($"Fat.GetNextUnallocatedFatEntry : xEntryNumber = {xEntryNumber}, xEntryValue = {xEntryValue}, Offset = {xEntryNumber * 4}");
                            return xEntryNumber;
                        }
                    }
                }

                // TODO: What should we return if no available entry is found.
                throw new Exception("Failed to find an unallocated FAT entry.");
            }

            private void ReadFatTableSector(ulong xSectorNum, byte[] aData)
            {
                Global.mFileSystemDebugger.SendInternal($"Fat.ReadFatTableSector : xSectorNum = {xSectorNum},  aData.Length = {aData.Length}");
                ulong xSectorToRead = mFirstSector + xSectorNum;
                mFileSystem.mDevice.ReadBlock(xSectorToRead, 1, aData);
            }

            private void WriteFatTableSector(ulong xSectorNum, byte[] aData)
            {
                Global.mFileSystemDebugger.SendInternal($"Fat.WriteFatTableSector : xSectorNum = {xSectorNum},  aData.Length = {aData.Length}");
                ulong xSectorToRead = mFirstSector + xSectorNum;
                mFileSystem.mDevice.WriteBlock(xSectorToRead, 1, aData);
            }

            private void GetFatTableSector(ulong aClusterNum, out ulong aSector, out ulong aOffset)
            {
                ulong xOffset = 0;
                if (mFileSystem.mFatType == FatTypeEnum.Fat12)
                {
                    // Multiply by 1.5 without using floating point, the divide by 2 rounds DOWN
                    xOffset = aClusterNum + aClusterNum / 2;
                }
                else if (mFileSystem.mFatType == FatTypeEnum.Fat16)
                {
                    xOffset = aClusterNum * 2;
                }
                else if (mFileSystem.mFatType == FatTypeEnum.Fat32)
                {
                    xOffset = aClusterNum * 4;
                }
                aSector = (xOffset / mFileSystem.BytesPerSector);
                aOffset = (xOffset % mFileSystem.BytesPerSector);
            }

            private void GetFatEntry(ulong aClusterNum, out ulong aValue)
            {
                ulong xOffset = aClusterNum * 8;
                ulong xSectorNumber = xOffset / mFileSystem.BytesPerSector;
                ulong xSectorOffset = xSectorNumber * mFileSystem.BytesPerSector + xOffset;
                var xSector = new byte[mFileSystem.BytesPerSector];

                Global.mFileSystemDebugger.SendInternal($"Fat.GetFatEntry : aClusterNum = {aClusterNum},  xOffset = {xOffset},  xSectorNumber = {xSectorNumber},  xSectorOffset = {xSectorOffset}");

                ReadFatTableSector(xSectorNumber, xSector);
                switch (mFileSystem.mFatType)
                {
                    case FatTypeEnum.Fat12:
                        // We now access the FAT entry as a WORD just as we do for FAT16, but if the cluster number is
                        // EVEN, we only want the low 12-bits of the 16-bits we fetch. If the cluster number is ODD
                        // we want the high 12-bits of the 16-bits we fetch.
                        uint xResult = xSector.ToUInt16(xSectorOffset);
                        if ((aClusterNum & 0x01) == 0)
                        {
                            aValue = xResult & 0x0FFF; // Even
                        }
                        else
                        {
                            aValue = xResult >> 4; // Odd
                        }
                        break;
                    case FatTypeEnum.Fat16:
                        aValue = xSector.ToUInt16(xSectorOffset);
                        break;
                    case FatTypeEnum.Fat32:
                        aValue = xSector.ToUInt32(xSectorOffset) & 0x0FFFFFFF;
                        break;
                    default:
                        throw new Exception("Unknown file system type.");
                }

                Global.mFileSystemDebugger.SendInternal($"Fat.GetFatEntry : aValue = {aValue}");
            }

            private void SetFatEntry(ulong aClusterNum, ulong aValue)
            {
                ulong xOffset = aClusterNum * 8;
                ulong xSectorNumber = xOffset / mFileSystem.BytesPerSector;
                ulong xSectorOffset = xSectorNumber * mFileSystem.BytesPerSector - xOffset;
                var xSector = new byte[mFileSystem.BytesPerSector];

                ReadFatTableSector(xSectorNumber, xSector);
                switch (mFileSystem.mFatType)
                {
                    case FatTypeEnum.Fat12:
                        if (xOffset == mFileSystem.BytesPerSector - 1)
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
                        xSector.SetUInt16(xSectorOffset, (ushort)aValue);
                        break;
                    case FatTypeEnum.Fat16:
                        xSector.SetUInt16(xSectorOffset, (ushort)aValue);
                        break;
                    default:
                        xSector.SetUInt32(xSectorOffset, (uint)aValue);
                        break;
                }
                Global.mFileSystemDebugger.SendInternal($"Fat.SetFatEntry : aClusterNum = {aClusterNum},  aValue = {aValue}");
                WriteFatTableSector(xSectorNumber, xSector);
            }

            private bool FatEntryIsEof(ulong aValue)
            {
                switch (mFileSystem.mFatType)
                {
                    case FatTypeEnum.Fat12:
                        return aValue >= 0xFF8;
                    case FatTypeEnum.Fat16:
                        return aValue >= 0xFFF8;
                    case FatTypeEnum.Fat32:
                        return aValue >= 0xFFFFFF8;
                    default:
                        throw new Exception("Unknown file system type.");
                }
            }

            private ulong FatEntryEofValue()
            {
                switch (mFileSystem.mFatType)
                {
                    case FatTypeEnum.Fat12:
                        return 0x0FFF;
                    case FatTypeEnum.Fat16:
                        return 0xFFFF;
                    case FatTypeEnum.Fat32:
                        return 0x0FFFFFFF;
                    default:
                        throw new Exception("Unknown file system type.");
                }
            }
        }

        public readonly uint BytesPerCluster;

        public readonly uint BytesPerSector;

        public readonly uint ClusterCount;

        public readonly uint DataSector; // First Data Sector

        public readonly uint DataSectorCount;

        public readonly uint FatSectorCount;

        private readonly FatTypeEnum mFatType;

        public readonly uint NumberOfFATs;

        public readonly uint ReservedSectorCount;

        public readonly uint RootCluster; // FAT32

        public readonly uint RootEntryCount;

        public readonly uint RootSector; // FAT12/16

        public readonly uint RootSectorCount; // FAT12/16, FAT32 remains 0

        public readonly uint SectorsPerCluster;

        public readonly uint TotalSectorCount;

        private readonly Fat[] mFats;

        public FatFileSystem(Partition aDevice, string aRootPath)
            : base(aDevice, aRootPath)
        {
            var xBPB = mDevice.NewBlockArray(1);

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

            DataSectorCount = TotalSectorCount - (ReservedSectorCount + NumberOfFATs * FatSectorCount + ReservedSectorCount);

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
                RootSector = ReservedSectorCount + NumberOfFATs * FatSectorCount;
                RootSectorCount = (RootEntryCount * 32 + (BytesPerSector - 1)) / BytesPerSector;
            }
            DataSector = ReservedSectorCount + NumberOfFATs * FatSectorCount + RootSectorCount;

            mFats = new Fat[NumberOfFATs];
            for (ulong i = 0; i < NumberOfFATs; i++)
            {
                mFats[i] = new Fat(this, (ReservedSectorCount + i * FatSectorCount));
            }
        }

        internal Fat GetFat(int aTableNumber)
        {
            if (mFats.Length > aTableNumber)
            {
                return mFats[aTableNumber];
            }

            throw new IndexOutOfRangeException("The fat table number doesn't exist.");
        }

        internal byte[] NewClusterArray()
        {
            return new byte[BytesPerCluster];
        }

        private void ReadInternal(ulong aFirstCluster, out byte[] aData)
        {
            if (mFatType == FatTypeEnum.Fat32)
            {
                aData = NewClusterArray();
                ulong xSector = DataSector + (aFirstCluster - 2) * SectorsPerCluster;
                mDevice.ReadBlock(xSector, SectorsPerCluster, aData);
            }
            else
            {
                aData = mDevice.NewBlockArray(1);
                mDevice.ReadBlock(aFirstCluster, RootSectorCount, aData);
            }

            Global.mFileSystemDebugger.SendInternal($"FatFileSystem.ReadInternal : aFirstCluster = {aFirstCluster},  aData.Length = {aData.Length}");
        }

        private void WriteInternal(ulong aFirstCluster, byte[] aData)
        {
            if (mFatType == FatTypeEnum.Fat32)
            {
                ulong xSector = DataSector + (aFirstCluster - 2) * SectorsPerCluster;
                mDevice.WriteBlock(xSector, SectorsPerCluster, aData);
            }
            else
            {
                mDevice.WriteBlock(aFirstCluster, RootSectorCount, aData);
            }

            Global.mFileSystemDebugger.SendInternal($"FatFileSystem.WriteInternal : aFirstCluster = {aFirstCluster},  aData.Length = {aData.Length}");
        }

        internal void Read(ulong aFirstCluster, out byte[] aData, ulong aSize = 0, ulong aOffset = 0)
        {
            if (aSize == 0)
            {
                aSize = BytesPerCluster;
            }

            if (aSize > BytesPerCluster - aOffset)
            {
                throw new NotImplementedException("TODO: Add cluster spanning read.");
            }

            aData = new byte[aSize];
            byte[] xTempData;
            ReadInternal(aFirstCluster, out xTempData);
            Array.Copy(xTempData, (long)aOffset, aData, 0, (long)aSize);
        }

        internal void Write(ulong aFirstCluster, byte[] aData, ulong aSize = 0, ulong aOffset = 0)
        {
            if (aSize == 0)
            {
                aSize = BytesPerCluster;
            }

            if (aSize > BytesPerCluster - aOffset)
            {
                throw new NotImplementedException("TODO: Add cluster spanning write.");
            }

            byte[] xTempData;
            ReadInternal(aFirstCluster, out xTempData);
            Array.Copy(aData, (long)aOffset, xTempData, 0, (long)aSize);
            WriteInternal(aFirstCluster, aData);
        }

        public static bool IsDeviceFAT(Partition aDevice)
        {
            var xBPB = aDevice.NewBlockArray(1);
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
            Global.mFileSystemDebugger.SendInternal($"FatFileSystem.GetDirectoryListing : baseDirectory.Name = {baseDirectory?.mName}");

            var result = new List<DirectoryEntry>();
            List<FatDirectoryEntry> fatListing;
            if (baseDirectory == null)
            {
                // get root folder
                var xEntry = (FatDirectoryEntry)GetRootDirectory();
                fatListing = xEntry.ReadDirectoryContents();
            }
            else
            {
                var xEntry = (FatDirectoryEntry)baseDirectory;
                fatListing = xEntry.ReadDirectoryContents();
            }

            for (int i = 0; i < fatListing.Count; i++)
            {
                result.Add(fatListing[i]);
            }
            return result;
        }

        public override DirectoryEntry GetRootDirectory()
        {
            Global.mFileSystemDebugger.SendInternal($"FatFileSystem.GetRootDirectory : RootCluster = {RootCluster}");
            var xRootEntry = new FatDirectoryEntry(this, null, mRootPath, RootCluster);
            return xRootEntry;
        }

        public override DirectoryEntry CreateDirectory(DirectoryEntry aParentDirectory, string aNewDirectory)
        {
            if (aParentDirectory == null)
            {
                throw new ArgumentNullException("aParentDirectory");
            }

            if (aNewDirectory == null)
            {
                throw new ArgumentNullException("aNewDirectory");
            }

            if (string.IsNullOrWhiteSpace(aNewDirectory))
            {
                throw new ArgumentException("The new directory must be specified.", "aNewDirectory");
            }

            Global.mFileSystemDebugger.SendInternal($"FatFileSystem.CreateDirectory : aParentDirectory.Name = {aParentDirectory?.mName},  aNewDirectory = {aNewDirectory}");
            var xParentDirectory = (FatDirectoryEntry)aParentDirectory;
            var xDirectoryEntryToAdd = xParentDirectory.AddDirectoryEntry(aNewDirectory, DirectoryEntryTypeEnum.Directory);
            return xDirectoryEntryToAdd;
        }

        public override DirectoryEntry CreateFile(DirectoryEntry aParentDirectory, string aNewFile)
        {
            if (aParentDirectory == null)
            {
                throw new ArgumentNullException("aParentDirectory");
            }

            if (aNewFile == null)
            {
                throw new ArgumentNullException("aNewFile");
            }

            if (string.IsNullOrWhiteSpace(aNewFile))
            {
                throw new ArgumentException("The new file must be specified.", "aNewFile");
            }

            Global.mFileSystemDebugger.SendInternal($"FatFileSystem.CreateFile : aParentDirectory.Name = {aParentDirectory?.mName},  aNewFile = {aNewFile}");
            var xParentDirectory = (FatDirectoryEntry)aParentDirectory;
            var xDirectoryEntryToAdd = xParentDirectory.AddDirectoryEntry(aNewFile, DirectoryEntryTypeEnum.File);
            return xDirectoryEntryToAdd;
        }

        private enum FatTypeEnum
        {
            Unknown,

            Fat12,

            Fat16,

            Fat32
        }
    }
}
