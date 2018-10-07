//#define COSMOSDEBUG

using System;
using System.Collections.Generic;

using Cosmos.Common.Extensions;
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

            private readonly ulong mFatSector;

            /// <summary>
            /// Initializes a new instance of the <see cref="Fat"/> class.
            /// </summary>
            /// <param name="aFileSystem">The file system.</param>
            /// <param name="aFatSector">The first sector of the FAT table.</param>
            public Fat(FatFileSystem aFileSystem, ulong aFatSector)
            {
                if (aFileSystem == null)
                {
                    throw new ArgumentNullException(nameof(aFileSystem));
                }

                mFileSystem = aFileSystem;
                mFatSector = aFatSector;
            }

            /// <summary>
            /// Gets the size of a FAT entry in bytes.
            /// </summary>
            /// <returns>The size of a FAT entry in bytes.</returns>
            /// <exception cref="NotSupportedException">Can not get the FAT entry size for an unknown FAT type.</exception>
            private uint GetFatEntrySizeInBytes()
            {
                switch (mFileSystem.mFatType)
                {
                    case FatTypeEnum.Fat32:
                        return 4;

                    case FatTypeEnum.Fat16:
                        return 2;

                    case FatTypeEnum.Fat12:
                        // TODO:
                        break;
                }

                throw new NotSupportedException("Can not get the FAT entry size for an unknown FAT type.");
            }

            /// <summary>
            /// Gets the FAT chain.
            /// </summary>
            /// <param name="aFirstEntry">The first entry.</param>
            /// <param name="aDataSize">Size of a data to be stored in bytes.</param>
            /// <returns>An array of cluster numbers for the FAT chain.</returns>
            /// <exception cref="ArgumentOutOfRangeException"></exception>
            public uint[] GetFatChain(uint aFirstEntry, long aDataSize = 0)
            {
                Global.mFileSystemDebugger.SendInternal("-- Fat.GetFatChain --");
                Global.mFileSystemDebugger.SendInternal("aFirstEntry =");
                Global.mFileSystemDebugger.SendInternal(aFirstEntry);
                Global.mFileSystemDebugger.SendInternal("aDataSize =");
                Global.mFileSystemDebugger.SendInternal(aDataSize);

                var xReturn = new uint[0];
                uint xCurrentEntry = aFirstEntry;
                uint xValue;

                long xEntriesRequired = aDataSize / mFileSystem.BytesPerCluster;
                if (aDataSize % mFileSystem.BytesPerCluster != 0)
                {
                    xEntriesRequired++;
                }

                GetFatEntry(xCurrentEntry, out xValue);
                Array.Resize(ref xReturn, xReturn.Length + 1);
                xReturn[xReturn.Length - 1] = xCurrentEntry;

                Global.mFileSystemDebugger.SendInternal("xEntriesRequired =");
                Global.mFileSystemDebugger.SendInternal(xEntriesRequired);
                Global.mFileSystemDebugger.SendInternal("xCurrentEntry =");
                Global.mFileSystemDebugger.SendInternal(xCurrentEntry);
                Global.mFileSystemDebugger.SendInternal("xReturn.Length =");
                Global.mFileSystemDebugger.SendInternal(xReturn.Length);

                if (xEntriesRequired > 0)
                {
                    while (!FatEntryIsEof(xValue))
                    {
                        xCurrentEntry = xValue;
                        GetFatEntry(xCurrentEntry, out xValue);
                        Array.Resize(ref xReturn, xReturn.Length + 1);
                        if (!FatEntryIsEof(xValue))
                        {
                            xReturn[xReturn.Length - 1] = xValue;
                        }
                        else
                        {
                            xReturn[xReturn.Length - 1] = xCurrentEntry;
                        }
                        Global.mFileSystemDebugger.SendInternal("xCurrentEntry =");
                        Global.mFileSystemDebugger.SendInternal(xCurrentEntry);
                        Global.mFileSystemDebugger.SendInternal("xReturn.Length =");
                        Global.mFileSystemDebugger.SendInternal(xReturn.Length);
                    }

                    if (xEntriesRequired > xReturn.Length)
                    {
                        long xNewClusters = xReturn.Length - xEntriesRequired;
                        for (int i = 0; i < xNewClusters; i++)
                        {
                            xCurrentEntry = GetNextUnallocatedFatEntry();
                            uint xLastFatEntry = xReturn[xReturn.Length - 1];
                            SetFatEntry(xLastFatEntry, xCurrentEntry);
                            Array.Resize(ref xReturn, xReturn.Length + 1);
                            xReturn[xReturn.Length - 1] = xCurrentEntry;
                        }
                    }
                }

                SetFatEntry(xCurrentEntry, FatEntryEofValue());

                return xReturn;
            }

            /// <summary>
            /// Gets the next unallocated FAT entry.
            /// </summary>
            /// <returns>The index of the next unallocated FAT entry.</returns>
            /// <exception cref="Exception">Failed to find an unallocated FAT entry.</exception>
            public uint GetNextUnallocatedFatEntry()
            {
                Global.mFileSystemDebugger.SendInternal("-- Fat.GetNextUnallocatedFatEntry --");

                uint xTotalEntries = mFileSystem.FatSectorCount * mFileSystem.BytesPerSector / GetFatEntrySizeInBytes();
                for (uint i = mFileSystem.RootCluster; i < xTotalEntries; i++)
                {
                    uint xEntryValue;
                    GetFatEntry(i, out xEntryValue);
                    if (!FatEntryIsEof(xEntryValue))
                    {
                        Global.mFileSystemDebugger.SendInternal("i =");
                        Global.mFileSystemDebugger.SendInternal(i);
                        return i;
                    }
                }

                throw new Exception("Failed to find an unallocated FAT entry.");
            }

            /// <summary>
            /// Clears a FAT entry.
            /// </summary>
            /// <param name="aEntryNumber">The entry number.</param>
            public void ClearFatEntry(ulong aEntryNumber)
            {
                SetFatEntry(aEntryNumber, 0);
            }

            private void SetFatEntry(byte[] aData, ulong aEntryNumber, ulong aValue)
            {
                uint xEntrySize = GetFatEntrySizeInBytes();
                ulong xEntryOffset = aEntryNumber * xEntrySize;



                switch (mFileSystem.mFatType)
                {
                    case FatTypeEnum.Fat12:
                        aData.SetUInt16(xEntryOffset, (ushort)aValue);
                        break;
                    case FatTypeEnum.Fat16:
                        aData.SetUInt16(xEntryOffset, (ushort)aValue);
                        break;
                    case FatTypeEnum.Fat32:
                        aData.SetUInt32(xEntryOffset, (uint)aValue);
                        break;
                    default:
                        throw new NotSupportedException("Unknown FAT type.");
                }
            }

            private void GetFatEnty(byte[] aData, uint aEntryNumber, out uint aValue)
            {
                uint xEntrySize = GetFatEntrySizeInBytes();
                ulong xEntryOffset = aEntryNumber * xEntrySize;

                switch (mFileSystem.mFatType)
                {
                    case FatTypeEnum.Fat12:
                        // We now access the FAT entry as a WORD just as we do for FAT16, but if the cluster number is
                        // EVEN, we only want the low 12-bits of the 16-bits we fetch. If the cluster number is ODD
                        // we want the high 12-bits of the 16-bits we fetch.
                        uint xResult = BitConverter.ToUInt16(aData, (int)xEntryOffset);
                        if ((aEntryNumber & 0x01) == 0)
                        {
                            aValue = xResult & 0x0FFF; // Even
                        }
                        else
                        {
                            aValue = xResult >> 4; // Odd
                        }
                        break;
                    case FatTypeEnum.Fat16:
                        aValue = BitConverter.ToUInt16(aData, (int)xEntryOffset);
                        break;
                    case FatTypeEnum.Fat32:
                        aValue = BitConverter.ToUInt32(aData, (int)xEntryOffset) & 0x0FFFFFFF;
                        break;
                    default:
                        throw new NotSupportedException("Unknown FAT type.");
                }
            }

            public void ClearAllFat()
            {
                //byte[] xFatTable = new byte[4096]; // TODO find where '4096' is defined
                byte[] xFatTable = mFileSystem.NewBlockArray();
                //var xFatTableSize = mFileSystem.FatSectorCount * mFileSystem.BytesPerSector / GetFatEntrySizeInBytes();

                Global.mFileSystemDebugger.SendInternal($"FatSector is {mFatSector}");
                Global.mFileSystemDebugger.SendInternal($"RootCluster is {mFileSystem.RootCluster}");
                Global.mFileSystemDebugger.SendInternal("Clearing all Fat Table");

                byte[] xFatTableFistSector;
                ReadFatSector(0, out xFatTableFistSector);

                /* Change 3rd entry (RootDirectory) to be EOC */
                SetFatEntry(xFatTableFistSector, 2, FatEntryEofValue());

                /* Copy first three elements on xFatTable */
                Array.Copy(xFatTableFistSector, xFatTable, 12);

                Global.mFileSystemDebugger.SendInternal($"Clearing First sector...");
                /* The rest of 'xFatTable' should be all 0s as new does this internally */
                WriteFatSector(0, xFatTable);
                Global.mFileSystemDebugger.SendInternal($"First sector cleared");

                /* Restore the Array will all 0s as it is this we have to write in the other sectors */
                //Array.Clear(xFatTable, 0, 12);

                /* Array.Clear() not work: stack overflow! */
                for (int i = 0; i < 11; i++)
                {
                    xFatTable[i] = 0;
                }

                for (ulong sector = 1; sector < mFileSystem.FatSectorCount; sector++)
                {
                    if (sector % 100 == 0)
                    {
                        Global.mFileSystemDebugger.SendInternal($"Clearing sector {sector}");
                    }
                    WriteFatSector(sector, xFatTable);
                }
            }

            private void ReadFatSector(ulong aSector, out byte[] aData)
            {
                aData = mFileSystem.NewBlockArray();
                var xSector = mFatSector + aSector;
                Global.mFileSystemDebugger.SendInternal("xSector  =");
                Global.mFileSystemDebugger.SendInternal(xSector);
                mFileSystem.Device.ReadBlock(xSector, mFileSystem.SectorsPerCluster, aData);
            }

            private void WriteFatSector(ulong aSector, byte[] aData)
            {
                if (aData == null)
                {
                    throw new ArgumentNullException(nameof(aData));
                }

                var xSector = mFatSector + aSector;
                mFileSystem.Device.WriteBlock(xSector, mFileSystem.SectorsPerCluster, aData);
            }

            /// <summary>
            /// Gets a FAT entry.
            /// </summary>
            /// <param name="aEntryNumber">The entry number.</param>
            /// <param name="aValue">The entry value.</param>
            private void GetFatEntry(uint aEntryNumber, out uint aValue)
            {
                Global.mFileSystemDebugger.SendInternal("-- Fat.GetFatEntry --");
                Global.mFileSystemDebugger.SendInternal("aEntryNumber =");
                Global.mFileSystemDebugger.SendInternal(aEntryNumber);

                uint xEntrySize = GetFatEntrySizeInBytes();
                ulong xEntryOffset = aEntryNumber * xEntrySize;
                Global.mFileSystemDebugger.SendInternal("xEntrySize =");
                Global.mFileSystemDebugger.SendInternal(xEntrySize);
                Global.mFileSystemDebugger.SendInternal("xEntryOffset =");
                Global.mFileSystemDebugger.SendInternal(xEntryOffset);

                ulong xSector = xEntryOffset / mFileSystem.BytesPerSector;
                ulong xSectorOffset = (xSector * mFileSystem.BytesPerSector) - xEntryOffset;
                Global.mFileSystemDebugger.SendInternal("xSector =");
                Global.mFileSystemDebugger.SendInternal(xSector);

                byte[] xData;

                ReadFatSector(xSector, out xData);

                switch (mFileSystem.mFatType)
                {
                    case FatTypeEnum.Fat12:
                        // We now access the FAT entry as a WORD just as we do for FAT16, but if the cluster number is
                        // EVEN, we only want the low 12-bits of the 16-bits we fetch. If the cluster number is ODD
                        // we want the high 12-bits of the 16-bits we fetch.
                        uint xResult = BitConverter.ToUInt16(xData, (int)xEntryOffset);
                        if ((aEntryNumber & 0x01) == 0)
                        {
                            aValue = xResult & 0x0FFF; // Even
                        }
                        else
                        {
                            aValue = xResult >> 4; // Odd
                        }
                        break;

                    case FatTypeEnum.Fat16:
                        aValue = BitConverter.ToUInt16(xData, (int)xEntryOffset);
                        break;

                    case FatTypeEnum.Fat32:
                        aValue = BitConverter.ToUInt32(xData, (int)xEntryOffset) & 0x0FFFFFFF;
                        break;

                    default:
                        throw new NotSupportedException("Unknown FAT type.");
                }
                Global.mFileSystemDebugger.SendInternal("aValue =");
                Global.mFileSystemDebugger.SendInternal(aValue);
            }

            /// <summary>
            /// Sets a FAT entry.
            /// </summary>
            /// <param name="aEntryNumber">The entry number.</param>
            /// <param name="aValue">The value.</param>
            private void SetFatEntry(ulong aEntryNumber, ulong aValue)
            {
                Global.mFileSystemDebugger.SendInternal("--- Fat.SetFatEntry ---");
                Global.mFileSystemDebugger.SendInternal("aEntryNumber =");
                Global.mFileSystemDebugger.SendInternal(aEntryNumber);

                uint xEntrySize = GetFatEntrySizeInBytes();
                ulong xEntryOffset = aEntryNumber * xEntrySize;

                ulong xSector = xEntryOffset / mFileSystem.BytesPerSector;
                ulong xSectorOffset = (xSector * mFileSystem.BytesPerSector) - xEntryOffset;

                byte[] xData;
                ReadFatSector(xSector, out xData);

                switch (mFileSystem.mFatType)
                {
                    case FatTypeEnum.Fat12:
                        xData.SetUInt16(xEntryOffset, (ushort)aValue);
                        break;

                    case FatTypeEnum.Fat16:
                        xData.SetUInt16(xEntryOffset, (ushort)aValue);
                        break;

                    case FatTypeEnum.Fat32:
                        xData.SetUInt32(xEntryOffset, (uint)aValue);
                        break;

                    default:
                        throw new NotSupportedException("Unknown FAT type.");
                }

                WriteFatSector(xSector, xData);
            }

            /// <summary>
            /// Is the FAT entry EOF?
            /// </summary>
            /// <param name="aValue">The entry index.</param>
            /// <returns></returns>
            /// <exception cref="Exception">Unknown file system type.</exception>
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

            /// <summary>
            /// The the EOF value for a specific FAT type.
            /// </summary>
            /// <returns>The EOF value.</returns>
            /// <exception cref="Exception">Unknown file system type.</exception>
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

        public override string Type
        {
            get
            {
                switch (mFatType)
                {
                    case FatTypeEnum.Fat12:
                        return "FAT12";

                    case FatTypeEnum.Fat16:
                        return "FAT16";

                    case FatTypeEnum.Fat32:
                        return "FAT32";

                    default:
                        throw new Exception("Unknown FAT file system type.");
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FatFileSystem"/> class.
        /// </summary>
        /// <param name="aDevice">The partition.</param>
        /// <param name="aRootPath">The root path.</param>
        /// <exception cref="Exception">FAT signature not found.</exception>
        public FatFileSystem(Partition aDevice, string aRootPath, long aSize)
            : base(aDevice, aRootPath, aSize)
        {
            if (aDevice == null)
            {
                throw new ArgumentNullException(nameof(aDevice));
            }

            if (String.IsNullOrEmpty(aRootPath))
            {
                throw new ArgumentException("Argument is null or empty", nameof(aRootPath));
            }

            var xBPB = Device.NewBlockArray(1);

            Device.ReadBlock(0UL, 1U, xBPB);

            ushort xSig = BitConverter.ToUInt16(xBPB, 510);
            if (xSig != 0xAA55)
            {
                throw new Exception("FAT signature not found.");
            }

            BytesPerSector = BitConverter.ToUInt16(xBPB, 11);
            SectorsPerCluster = xBPB[13];
            BytesPerCluster = BytesPerSector * SectorsPerCluster;
            ReservedSectorCount = BitConverter.ToUInt16(xBPB, 14);
            NumberOfFATs = xBPB[16];
            RootEntryCount = BitConverter.ToUInt16(xBPB, 17);

            TotalSectorCount = BitConverter.ToUInt16(xBPB, 19);
            if (TotalSectorCount == 0)
            {
                TotalSectorCount = BitConverter.ToUInt32(xBPB, 32);
            }

            // FATSz
            FatSectorCount = BitConverter.ToUInt16(xBPB, 22);
            if (FatSectorCount == 0)
            {
                FatSectorCount = BitConverter.ToUInt32(xBPB, 36);
            }

            DataSectorCount = TotalSectorCount -
                              (ReservedSectorCount + NumberOfFATs * FatSectorCount + ReservedSectorCount);

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
                RootCluster = BitConverter.ToUInt32(xBPB, 44);
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

            throw new Exception("The fat table number doesn't exist.");
        }

        internal byte[] NewBlockArray()
        {
            return new byte[BytesPerCluster];
        }

        internal void Read(long aCluster, out byte[] aData, long aSize = 0, long aOffset = 0)
        {
            Global.mFileSystemDebugger.SendInternal("-- FatFileSystem.Read --");

            if (aSize == 0)
            {
                aSize = BytesPerCluster;
            }

            if (mFatType == FatTypeEnum.Fat32)
            {
                aData = NewBlockArray();
                long xSector = DataSector + (aCluster - RootCluster) * SectorsPerCluster;
                Device.ReadBlock((ulong)xSector, SectorsPerCluster, aData);
            }
            else
            {
                aData = Device.NewBlockArray(1);
                Device.ReadBlock((ulong)aCluster, RootSectorCount, aData);
            }
        }

        internal void Write(long aCluster, byte[] aData, long aSize = 0, long aOffset = 0)
        {
            Global.mFileSystemDebugger.SendInternal("-- FatFileSystem.Write --");

            if (aData == null)
            {
                throw new ArgumentNullException(nameof(aData));
            }

            if (aSize == 0)
            {
                aSize = BytesPerCluster;
            }

            byte[] xData;

            Read(aCluster, out xData);
            Array.Copy(aData, 0, xData, aOffset, aData.Length);

            if (mFatType == FatTypeEnum.Fat32)
            {
                long xSector = DataSector + (aCluster - RootCluster) * SectorsPerCluster;
                Device.WriteBlock((ulong)xSector, SectorsPerCluster, xData);
            }
            else
            {
                Device.WriteBlock((ulong)aCluster, RootSectorCount, xData);
            }
        }

        public override void DisplayFileSystemInfo()
        {
            global::System.Console.WriteLine("-------File System--------");
            global::System.Console.WriteLine("Bytes per Cluster     = " + BytesPerCluster);
            global::System.Console.WriteLine("Bytes per Sector      = " + BytesPerSector);
            global::System.Console.WriteLine("Cluster Count         = " + ClusterCount);
            global::System.Console.WriteLine("Data Sector           = " + DataSector);
            global::System.Console.WriteLine("Data Sector Count     = " + DataSectorCount);
            global::System.Console.WriteLine("FAT Sector Count      = " + FatSectorCount);
            global::System.Console.WriteLine("FAT Type              = " + (uint)mFatType);
            global::System.Console.WriteLine("Number of FATS        = " + NumberOfFATs);
            global::System.Console.WriteLine("Reserved Sector Count = " + ReservedSectorCount);
            global::System.Console.WriteLine("Root Cluster          = " + RootCluster);
            global::System.Console.WriteLine("Root Entry Count      = " + RootEntryCount);
            global::System.Console.WriteLine("Root Sector           = " + RootSector);
            global::System.Console.WriteLine("Root Sector Count     = " + RootSectorCount);
            global::System.Console.WriteLine("Sectors per Cluster   = " + SectorsPerCluster);
            global::System.Console.WriteLine("Total Sector Count    = " + TotalSectorCount);

            Global.mFileSystemDebugger.SendInternal("Bytes per Cluster =");
            Global.mFileSystemDebugger.SendInternal(BytesPerCluster);
            Global.mFileSystemDebugger.SendInternal("Bytes per Sector =");
            Global.mFileSystemDebugger.SendInternal(BytesPerSector);
            Global.mFileSystemDebugger.SendInternal("Cluster Count =");
            Global.mFileSystemDebugger.SendInternal(ClusterCount);
            Global.mFileSystemDebugger.SendInternal("Data Sector =");
            Global.mFileSystemDebugger.SendInternal(DataSector);
            Global.mFileSystemDebugger.SendInternal("Data Sector Count =");
            Global.mFileSystemDebugger.SendInternal(DataSectorCount);
            Global.mFileSystemDebugger.SendInternal("FAT Sector Count =");
            Global.mFileSystemDebugger.SendInternal(FatSectorCount);
            Global.mFileSystemDebugger.SendInternal("FAT Type =");
            Global.mFileSystemDebugger.SendInternal((uint)mFatType);
            Global.mFileSystemDebugger.SendInternal("Number of FATS =");
            Global.mFileSystemDebugger.SendInternal(NumberOfFATs);
            Global.mFileSystemDebugger.SendInternal("Reserved Sector Count =");
            Global.mFileSystemDebugger.SendInternal(ReservedSectorCount);
            Global.mFileSystemDebugger.SendInternal("Root Cluster =");
            Global.mFileSystemDebugger.SendInternal(RootCluster);
            Global.mFileSystemDebugger.SendInternal("Root Entry Count =");
            Global.mFileSystemDebugger.SendInternal(RootEntryCount);
            Global.mFileSystemDebugger.SendInternal("Root Sector =");
            Global.mFileSystemDebugger.SendInternal(RootSector);
            Global.mFileSystemDebugger.SendInternal("Root Sector Count =");
            Global.mFileSystemDebugger.SendInternal(RootSectorCount);
            Global.mFileSystemDebugger.SendInternal("Sectors per Cluster =");
            Global.mFileSystemDebugger.SendInternal(SectorsPerCluster);
            Global.mFileSystemDebugger.SendInternal("Total Sector Count =");
            Global.mFileSystemDebugger.SendInternal(TotalSectorCount);
        }

        public override List<DirectoryEntry> GetDirectoryListing(DirectoryEntry baseDirectory)
        {
            Global.mFileSystemDebugger.SendInternal("-- FatFileSystem.GetDirectoryListing --");

            if (baseDirectory == null)
            {
                throw new ArgumentNullException(nameof(baseDirectory));
            }

            var result = new List<DirectoryEntry>();
            var xEntry = (FatDirectoryEntry)baseDirectory;
            var fatListing = xEntry.ReadDirectoryContents();

            for (int i = 0; i < fatListing.Count; i++)
            {
                result.Add(fatListing[i]);
            }
            return result;
        }

        public override DirectoryEntry GetRootDirectory()
        {
            Global.mFileSystemDebugger.SendInternal("-- FatFileSystem.GetRootDirectory --");

            var xRootEntry = new FatDirectoryEntry(this, null, RootPath, Size, RootPath, RootCluster);
            return xRootEntry;
        }

        public override DirectoryEntry CreateDirectory(DirectoryEntry aParentDirectory, string aNewDirectory)
        {
            Global.mFileSystemDebugger.SendInternal("-- FatFileSystem.CreateDirectory --");
            Global.mFileSystemDebugger.SendInternal("aParentDirectory.Name = " + aParentDirectory?.mName);
            Global.mFileSystemDebugger.SendInternal("aNewDirectory = " + aNewDirectory);

            if (aParentDirectory == null)
            {
                throw new ArgumentNullException(nameof(aParentDirectory));
            }

            if (string.IsNullOrEmpty(aNewDirectory))
            {
                throw new ArgumentNullException(nameof(aNewDirectory));
            }

            var xParentDirectory = (FatDirectoryEntry)aParentDirectory;
            var xDirectoryEntryToAdd = xParentDirectory.AddDirectoryEntry(aNewDirectory, DirectoryEntryTypeEnum.Directory);
            return xDirectoryEntryToAdd;
        }

        public override DirectoryEntry CreateFile(DirectoryEntry aParentDirectory, string aNewFile)
        {
            Global.mFileSystemDebugger.SendInternal("-- FatFileSystem.CreateFile --");
            Global.mFileSystemDebugger.SendInternal("aParentDirectory.Name = " + aParentDirectory?.mName);
            Global.mFileSystemDebugger.SendInternal("aNewFile =" + aNewFile);

            if (aParentDirectory == null)
            {
                throw new ArgumentNullException(nameof(aParentDirectory));
            }

            if (string.IsNullOrEmpty(aNewFile))
            {
                throw new ArgumentNullException(nameof(aNewFile));
            }

            var xParentDirectory = (FatDirectoryEntry)aParentDirectory;

            var xDirectoryEntryToAdd = xParentDirectory.AddDirectoryEntry(aNewFile, DirectoryEntryTypeEnum.File);
            return xDirectoryEntryToAdd;
        }

        public override void DeleteDirectory(DirectoryEntry aDirectoryEntry)
        {
            if (aDirectoryEntry == null)
            {
                throw new ArgumentNullException(nameof(aDirectoryEntry));
            }

            var xDirectoryEntry = (FatDirectoryEntry)aDirectoryEntry;

            xDirectoryEntry.DeleteDirectoryEntry();
        }

        public override void DeleteFile(DirectoryEntry aDirectoryEntry)
        {
            if (aDirectoryEntry == null)
            {
                throw new ArgumentNullException(nameof(aDirectoryEntry));
            }

            var xDirectoryEntry = (FatDirectoryEntry)aDirectoryEntry;

            var entries = xDirectoryEntry.GetFatTable();

            foreach (var entry in entries)
            {
                GetFat(0).ClearFatEntry(entry);
            }

            xDirectoryEntry.DeleteDirectoryEntry();
        }

        public override string Label
        {
            /*
             * In the FAT filesystem the name field of RootDirectory is - in reality - the Volume Label
             */
            get
            {
                Global.mFileSystemDebugger.SendInternal("-- FatFileSystem.mLabel --");
                var RootDirectory = (FatDirectoryEntry)GetRootDirectory();

                var VolumeId = RootDirectory.FindVolumeId();
                if (VolumeId == null)
                {
                    Global.mFileSystemDebugger.SendInternal("No VolumeID, returning drive name");
                    return RootDirectory.mName;
                }

                Global.mFileSystemDebugger.SendInternal($"Volume label is |{VolumeId.mName.TrimEnd()}|");
                return VolumeId.mName.TrimEnd();
            }
            set
            {
                Global.mFileSystemDebugger.SendInternal($"FatFileSystem - Setting Volume label to |{value}|");

                var RootDirectory = (FatDirectoryEntry)GetRootDirectory();

                var VolumeId = RootDirectory.FindVolumeId();
                if (VolumeId != null)
                {
                    VolumeId.SetName(value);
                    return;
                }

                Global.mFileSystemDebugger.SendInternal("No VolumeID found, let's create it!");

                VolumeId = RootDirectory.CreateVolumeId(value);
            }
        }

        public override long AvailableFreeSpace
        {
            get
            {
                var RootDirectory = (FatDirectoryEntry)GetRootDirectory();
                // We do not support "user quotas" for now so this is effectively the same then mTotalFreeSpace

                /* mSize is expressed in MegaByte */
                var TotalSizeInBytes = Size * 1024 * 1024;
                var UsedSpace = RootDirectory.GetUsedSpace();

                Global.mFileSystemDebugger.SendInternal($"TotalSizeInBytes {TotalSizeInBytes} UsedSpace {UsedSpace}");

                return TotalSizeInBytes - UsedSpace;
                //return (mSize * 1024 * 1024) - RootDirectory.GetUsedSpace();
            }
        }

        public override long TotalFreeSpace
        {
            get
            {
                var RootDirectory = (FatDirectoryEntry)GetRootDirectory();

                /* mSize is expressed in MegaByte */
                var TotalSizeInBytes = Size * 1024 * 1024;
                var UsedSpace = RootDirectory.GetUsedSpace();

                Global.mFileSystemDebugger.SendInternal($"TotalSizeInBytes {TotalSizeInBytes} UsedSpace {UsedSpace}");

                return TotalSizeInBytes - UsedSpace;
                //return (mSize * 1024 * 1024) - RootDirectory.GetUsedSpace();
            }
        }

        private enum FatTypeEnum
        {
            Unknown,

            Fat12,

            Fat16,

            Fat32
        }

        public override void Format(string aDriveFormat, bool aQuick)
        {
            var xRootDirectory = (FatDirectoryEntry)GetRootDirectory();

            var Fat = GetFat(0);

            var x = xRootDirectory.ReadDirectoryContents();

            foreach (var el in x)
            {
                Global.mFileSystemDebugger.SendInternal($"Found '{el.mName}' of type {(int)el.mEntryType}");
                // Delete yourself!
                el.DeleteDirectoryEntry();
            }

            Fat.ClearAllFat();
        }
    }
}
