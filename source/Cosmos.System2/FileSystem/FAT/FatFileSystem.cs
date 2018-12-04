//#define COSMOSDEBUG

using System;
using System.Collections.Generic;

using Cosmos.Common.Extensions;
using Cosmos.HAL.BlockDevice;
using Cosmos.System.FileSystem.FAT.Listing;
using Cosmos.System.FileSystem.Listing;
using Fields = Cosmos.System.FileSystem.FAT.BiosParameterBlock;
using static Cosmos.System.FileSystem.FAT.BiosParameterBlock;

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
                mFileSystem = aFileSystem ?? throw new ArgumentNullException(nameof(aFileSystem));
                mFatSector = aFatSector;
            }

            /// <summary>
            /// Gets the size of a FAT entry in bytes.
            /// </summary>
            /// <returns>The size of a FAT entry in bytes.</returns>
            /// <exception cref="NotSupportedException">Can not get the FAT entry size for an unknown FAT type.</exception>
            private uint GetFatEntrySizeInBytes()
            {
                switch (mFileSystem.FatKind)
                {
                    case FatKind.Fat32:
                        return 4;

                    case FatKind.Fat16:
                        return 2;

                    case FatKind.Fat12:
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

                var xReturn = Array.Empty<uint>();
                uint xCurrentEntry = aFirstEntry;

                long xEntriesRequired = aDataSize / mFileSystem.BytesPerCluster;
                if (aDataSize % mFileSystem.BytesPerCluster != 0)
                {
                    xEntriesRequired++;
                }

                GetFatEntry(xCurrentEntry, out var xValue);
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
                    GetFatEntry(i, out var xEntryValue);
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

                switch (mFileSystem.FatKind)
                {
                    case FatKind.Fat12:
                        aData.SetUInt16(xEntryOffset, (ushort)aValue);
                        break;
                    case FatKind.Fat16:
                        aData.SetUInt16(xEntryOffset, (ushort)aValue);
                        break;
                    case FatKind.Fat32:
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

                switch (mFileSystem.FatKind)
                {
                    case FatKind.Fat12:
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
                    case FatKind.Fat16:
                        aValue = BitConverter.ToUInt16(aData, (int)xEntryOffset);
                        break;
                    case FatKind.Fat32:
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

                ReadFatSector(0, out var xFatTableFistSector);

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

                switch (mFileSystem.FatKind)
                {
                    case FatKind.Fat12:
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

                    case FatKind.Fat16:
                        aValue = BitConverter.ToUInt16(xData, (int)xEntryOffset);
                        break;

                    case FatKind.Fat32:
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

                switch (mFileSystem.FatKind)
                {
                    case FatKind.Fat12:
                        xData.SetUInt16(xEntryOffset, (ushort)aValue);
                        break;
                    case FatKind.Fat16:
                        xData.SetUInt16(xEntryOffset, (ushort)aValue);
                        break;
                    case FatKind.Fat32:
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
                switch (mFileSystem.FatKind)
                {
                    case FatKind.Fat12:
                        return aValue >= 0xFF8;
                    case FatKind.Fat16:
                        return aValue >= 0xFFF8;
                    case FatKind.Fat32:
                        return aValue >= 0xFFFFFF8;
                    default:
                        throw new Exception("Unknown file system type.");
                }
            }

            /// <summary>
            /// The EOF value for a specific FAT type.
            /// </summary>
            /// <returns>The EOF value.</returns>
            /// <exception cref="Exception">Unknown file system type.</exception>
            private ulong FatEntryEofValue()
            {
                switch (mFileSystem.FatKind)
                {
                    case FatKind.Fat12:
                        return 0x0FFF;
                    case FatKind.Fat16:
                        return 0xFFFF;
                    case FatKind.Fat32:
                        return 0x0FFFFFFF;
                    default:
                        throw new Exception("Unknown file system type.");
                }
            }
        }

        private readonly Fat[] mFats;

        /// <summary>
        /// Initializes a new instance of the <see cref="FatFileSystem"/> class.
        /// </summary>
        /// <param name="aDevice">The partition.</param>
        /// <param name="aRootPath">The root path.</param>
        /// <exception cref="Exception">FAT signature not found.</exception>
        public FatFileSystem(BiosParameterBlock bpb, Partition aDevice, string aRootPath, long aSize)
            : base(aDevice, aRootPath, aSize)
        {
            BiosParameterBlock = bpb;

            BytesPerSector = BiosParameterBlock.GetValue(Fields.BytesPerSector);
            SectorsPerCluster = BiosParameterBlock.GetValue(Fields.SectorsPerCluster);

            BytesPerCluster = (uint)(BytesPerSector * SectorsPerCluster);

            ReservedSectorCount = BiosParameterBlock.GetValue(Fields.ReservedSectorCount);

            FatCount = BiosParameterBlock.GetValue(Fields.FatCount);

            var rootEntryCount = BiosParameterBlock.GetValue(Fields.RootEntryCount);

            // 1.
            RootDirectorySectorCount = (uint)(((rootEntryCount * 32) + (BytesPerSector - 1)) / BytesPerSector);

            // 2.
            FatSectorCount = BiosParameterBlock.GetValue(Fields.FatSectorCount16);

            if (FatSectorCount == 0)
            {
                FatSectorCount = BiosParameterBlock.GetValue(Fat32.FatSectorCount32);
            }

            TotalSectorCount = BiosParameterBlock.GetValue(Fields.TotalSectorCount16);

            if (TotalSectorCount == 0)
            {
                TotalSectorCount = BiosParameterBlock.GetValue(Fields.TotalSectorCount32);
            }

            FirstDataSector = ReservedSectorCount + (FatCount * FatSectorCount) + RootDirectorySectorCount;
            DataSectorCount = TotalSectorCount - FirstDataSector;

            // 3.
            ClusterCount = DataSectorCount / SectorsPerCluster;

            if (ClusterCount < 4085)
            {
                FatKind = FatKind.Fat12;
            }
            else if (ClusterCount < 65525)
            {
                FatKind = FatKind.Fat16;
            }
            else
            {
                FatKind = FatKind.Fat32;
            }

            if (FatKind == FatKind.Fat32)
            {
                RootCluster = BiosParameterBlock.GetValue(Fat32.RootCluster);
            }
            else
            {
                RootSector = ReservedSectorCount + FatCount * FatSectorCount;
            }

            mFats = new Fat[FatCount];
            for (ulong i = 0; i < FatCount; i++)
            {
                mFats[i] = new Fat(this, (ReservedSectorCount + i * FatSectorCount));
            }
        }

        public FatKind FatKind { get; }

        protected BiosParameterBlock BiosParameterBlock { get; }

        protected ushort BytesPerSector { get; }

        protected byte SectorsPerCluster { get; }

        public uint BytesPerCluster { get; }

        protected ushort ReservedSectorCount { get; }

        protected byte FatCount { get; }

        protected uint RootDirectorySectorCount { get; }

        protected uint FatSectorCount { get; }

        protected uint TotalSectorCount { get; }

        protected uint FirstDataSector { get; }

        protected uint DataSectorCount { get; }

        protected uint ClusterCount { get; }

        // FAT12/16
        public uint RootSector { get; }

        // FAT32
        public uint RootCluster { get; }

        public override string Type
        {
            get
            {
                switch (FatKind)
                {
                    case FatKind.Fat12:
                        return "FAT12";

                    case FatKind.Fat16:
                        return "FAT16";

                    case FatKind.Fat32:
                        return "FAT32";

                    default:
                        throw new Exception("Unknown FAT file system type.");
                }
            }
        }

        internal Fat GetFat(int aTableNumber)
        {
            if (mFats.Length > aTableNumber)
            {
                return mFats[aTableNumber];
            }

            throw new Exception("The FAT table number doesn't exist.");
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

            if (FatKind == FatKind.Fat32)
            {
                aData = NewBlockArray();
                long xSector = FirstDataSector + (aCluster - RootCluster) * SectorsPerCluster;
                Device.ReadBlock((ulong)xSector, SectorsPerCluster, aData);
            }
            else
            {
                aData = Device.NewBlockArray(1);
                Device.ReadBlock((ulong)aCluster, RootDirectorySectorCount, aData);
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

            Read(aCluster, out var xData);
            Array.Copy(aData, 0, xData, aOffset, aData.Length);



            if (FatKind == FatKind.Fat32)
            {
                long xSector = FirstDataSector + (aCluster - RootCluster) * SectorsPerCluster;
                Device.WriteBlock((ulong)xSector, SectorsPerCluster, xData);
            }
            else
            {
                Device.WriteBlock((ulong)aCluster, RootSector, xData);
            }
        }

        public override void DisplayFileSystemInfo()
        {
            global::System.Console.WriteLine("-------File System--------");
            global::System.Console.WriteLine("Bytes per Cluster     = " + BytesPerCluster);
            global::System.Console.WriteLine("Bytes per Sector      = " + BytesPerSector);
            global::System.Console.WriteLine("Cluster Count         = " + ClusterCount);
            global::System.Console.WriteLine("First Data Sector     = " + FirstDataSector);
            global::System.Console.WriteLine("Data Sector Count     = " + DataSectorCount);
            global::System.Console.WriteLine("FAT Sector Count      = " + FatSectorCount);
            global::System.Console.WriteLine("FAT Kind              = " + (uint)FatKind);
            global::System.Console.WriteLine("Number of FATs        = " + FatCount);
            global::System.Console.WriteLine("Reserved Sector Count = " + ReservedSectorCount);
            global::System.Console.WriteLine("Root Cluster          = " + RootCluster);
            global::System.Console.WriteLine("Root Entry Count      = " + BiosParameterBlock.GetValue(Fields.RootEntryCount));
            global::System.Console.WriteLine("Root Sector Count     = " + RootDirectorySectorCount);
            global::System.Console.WriteLine("Sectors per Cluster   = " + SectorsPerCluster);
            global::System.Console.WriteLine("Total Sector Count    = " + TotalSectorCount);

            Global.mFileSystemDebugger.SendInternal("Bytes per Cluster =");
            Global.mFileSystemDebugger.SendInternal(BytesPerCluster);
            Global.mFileSystemDebugger.SendInternal("Bytes per Sector =");
            Global.mFileSystemDebugger.SendInternal(BytesPerSector);
            Global.mFileSystemDebugger.SendInternal("Cluster Count =");
            Global.mFileSystemDebugger.SendInternal(ClusterCount);
            Global.mFileSystemDebugger.SendInternal("First Data Sector =");
            Global.mFileSystemDebugger.SendInternal(FirstDataSector);
            Global.mFileSystemDebugger.SendInternal("Data Sector Count =");
            Global.mFileSystemDebugger.SendInternal(DataSectorCount);
            Global.mFileSystemDebugger.SendInternal("FAT Sector Count =");
            Global.mFileSystemDebugger.SendInternal(FatSectorCount);
            Global.mFileSystemDebugger.SendInternal("FAT Type =");
            Global.mFileSystemDebugger.SendInternal((uint)FatKind);
            Global.mFileSystemDebugger.SendInternal("Number of FATs =");
            Global.mFileSystemDebugger.SendInternal(FatCount);
            Global.mFileSystemDebugger.SendInternal("Reserved Sector Count =");
            Global.mFileSystemDebugger.SendInternal(ReservedSectorCount);
            Global.mFileSystemDebugger.SendInternal("Root Cluster =");
            Global.mFileSystemDebugger.SendInternal(RootCluster);
            Global.mFileSystemDebugger.SendInternal("Root Entry Count =");
            Global.mFileSystemDebugger.SendInternal(BiosParameterBlock.GetValue(Fields.RootEntryCount));
            Global.mFileSystemDebugger.SendInternal("Root Sector Count =");
            Global.mFileSystemDebugger.SendInternal(RootDirectorySectorCount);
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
