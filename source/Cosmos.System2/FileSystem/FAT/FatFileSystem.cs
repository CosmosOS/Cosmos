//#define COSMOSDEBUG

using System;
using System.Collections.Generic;

using Cosmos.Common.Extensions;
using Cosmos.HAL.BlockDevice;
using Cosmos.System.FileSystem.FAT.Listing;
using Cosmos.System.FileSystem.Listing;

namespace Cosmos.System.FileSystem.FAT
{
    /// <summary>
    /// FatFileSystem class.
    /// </summary>
    internal class FatFileSystem : FileSystem
    {
        /// <summary>
        /// FAT class. Used to manage individual FAT entry. 
        /// </summary>
        internal class Fat
        {
            private readonly FatFileSystem mFileSystem;

            private readonly ulong mFatSector;

            /// <summary>
            /// Initializes a new instance of the <see cref="Fat"/> class.
            /// </summary>
            /// <param name="aFileSystem">The file system.</param>
            /// <param name="aFatSector">The first sector of the FAT table.</param>
            /// <exception cref="ArgumentNullException">Thrown when aFileSystem is null.</exception>
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
            /// <exception cref="NotSupportedException">Thrown when FAT type is unknown.</exception>
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
            /// <exception cref="ArgumentOutOfRangeException">Thrown when the size of the chain is less then zero. (Never thrown)</exception>
            /// <exception cref="OverflowException">Thrown when data lenght is greater then Int32.MaxValue.</exception>
            /// <exception cref="Exception">
            /// <list type="bullet">
            /// <item>Thrown on out of memory.</item>
            /// <item>data size invalid.</item>
            /// <item>unknown file system type</item>
            /// <item>memory error.</item>
            /// </list>
            /// </exception>
            /// <exception cref="ArgumentException">Thrown when bad aFirstEntry passed.</exception>
            /// <exception cref="ArgumentNullException">Thrown on fatal error (contact support).</exception>
            /// <exception cref="NotSupportedException">Thrown when FAT type is unknown.</exception>
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
                        xReturn[xReturn.Length - 1] = xCurrentEntry;
                        Global.mFileSystemDebugger.SendInternal("xCurrentEntry =");
                        Global.mFileSystemDebugger.SendInternal(xCurrentEntry);
                        Global.mFileSystemDebugger.SendInternal("xReturn.Length =");
                        Global.mFileSystemDebugger.SendInternal(xReturn.Length);
                    }

                    if (xEntriesRequired > xReturn.Length)
                    {
                        long xNewClusters = xEntriesRequired - xReturn.Length;
                        for (int i = 0; i < xNewClusters; i++)
                        {
                            xCurrentEntry = GetNextUnallocatedFatEntry();
                            mFileSystem.Write(xCurrentEntry, new byte[mFileSystem.BytesPerCluster]);
                            uint xLastFatEntry = xReturn[xReturn.Length - 1];
                            SetFatEntry(xLastFatEntry, xCurrentEntry);
                            SetFatEntry(xCurrentEntry, FatEntryEofValue());
                            Array.Resize(ref xReturn, xReturn.Length + 1);
                            xReturn[xReturn.Length - 1] = xCurrentEntry;
                        }
                    }
                }

                string xChain = "";
                for (int i = 0; i < xReturn.Length; i++)
                {
                    xChain += xReturn[i];
                    if (i > 0 || i < xReturn.Length - 1)
                    {
                        xChain += "->";
                    }
                }
                Global.mFileSystemDebugger.SendInternal("Fat xChain:");
                Global.mFileSystemDebugger.SendInternal(xChain);

                SetFatEntry(xCurrentEntry, FatEntryEofValue());

                return xReturn;
            }

            /// <summary>
            /// Gets the next unallocated FAT entry.
            /// </summary>
            /// <returns>The index of the next unallocated FAT entry.</returns>
            /// <exception cref="OverflowException">Thrown when data lenght is greater then Int32.MaxValue.</exception>
            /// <exception cref="Exception">Thrown when data size invalid / Failed to find an unallocated FAT entry.</exception>
            /// <exception cref="ArgumentException">Thrown on fatal error (contact support).</exception>
            /// <exception cref="ArgumentNullException">Thrown on fatal error (contact support).</exception>
            /// <exception cref="ArgumentOutOfRangeException">Thrown on fatal error (contact support).</exception>
            /// <exception cref="NotSupportedException">Thrown when FAT type is unknown.</exception>
            public uint GetNextUnallocatedFatEntry()
            {
                Global.mFileSystemDebugger.SendInternal("-- Fat.GetNextUnallocatedFatEntry --");

                uint xTotalEntries = mFileSystem.FatSectorCount * mFileSystem.BytesPerSector / GetFatEntrySizeInBytes();
                for (uint i = mFileSystem.RootCluster + 1; i < xTotalEntries; i++)
                {
                    GetFatEntry(i, out uint xEntryValue);
                    if (FatEntryIsFree(xEntryValue))
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
            /// <exception cref="NotSupportedException">Thrown when FAT type is unknown.</exception>
            /// <exception cref="OverflowException">Thrown when data lenght is greater then Int32.MaxValue.</exception>
            /// <exception cref="Exception">Thrown when data size invalid.</exception>
            /// <exception cref="ArgumentNullException">Thrown when FAT sector data is null.</exception>
            public void ClearFatEntry(ulong aEntryNumber)
            {
                SetFatEntry(aEntryNumber, 0);
            }

            /// <summary>
            /// Set a value in aData corresponding to the type of Fat Filesystem currently in use
            /// </summary>
            /// <param name="aEntryNumber">A entry number to set.</param>
            /// <param name="aValue">A value to set.</param>
            /// <param name="aData">A data array in which the value should be set</param>
            /// <exception cref="NotSupportedException">Thrown when FAT type is unknown.</exception>
            private void SetValueInFat(ulong aEntryNumber, ulong aValue, byte[] aData)
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

            /// <summary>
            /// Clears all the FAT sectors.
            /// </summary>
            /// <exception cref="OverflowException">Thrown when data lenght is greater then Int32.MaxValue.</exception>
            /// <exception cref="Exception">Thrown when data size invalid / Unknown file system type.</exception>
            /// <exception cref="NotSupportedException">Thrown when FAT type is unknown.</exception>
            /// <exception cref="ArgumentNullException">
            /// <list type="bullet">
            /// <item>Thrown when entrys aData is null.</item>
            /// <item>Out of memory.</item>
            /// </list>
            /// </exception>
            /// <exception cref="RankException">Thrown on fatal error (contact support).</exception>
            /// <exception cref="ArrayTypeMismatchException">Thrown on fatal error (contact support).</exception>
            /// <exception cref="InvalidCastException">Thrown when the data in aData is corrupted.</exception>
            /// <exception cref="ArgumentOutOfRangeException">
            /// <list type = "bullet" >
            /// <item>Thrown when the data length is 0 or greater then Int32.MaxValue.</item>
            /// <item>Entrys matadata offset value is invalid.</item>
            /// </list>
            /// </exception>
            /// <exception cref="ArgumentException">
            /// <list type="bullet">
            /// <item>Data length is 0.</item>
            /// </list>
            /// </exception>
            public void ClearAllFat()
            {
                //byte[] xFatTable = new byte[4096]; // TODO find where '4096' is defined
                byte[] xFatTable = mFileSystem.NewBlockArray();
                //var xFatTableSize = mFileSystem.FatSectorCount * mFileSystem.BytesPerSector / GetFatEntrySizeInBytes();

                Global.mFileSystemDebugger.SendInternal($"FatSector is {mFatSector}");
                Global.mFileSystemDebugger.SendInternal($"RootCluster is {mFileSystem.RootCluster}");
                Global.mFileSystemDebugger.SendInternal("Clearing all Fat Table");

                byte[] xFatTableFirstSector;
                ReadFatSector(0, out xFatTableFirstSector);

                /* Change 3rd entry (RootDirectory) to be EOC */
                SetValueInFat(2, FatEntryEofValue(), xFatTableFirstSector);

                /* Copy first three elements on xFatTable */
                Array.Copy(xFatTableFirstSector, xFatTable, 12);

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

            /// <summary>
            /// Read FAT sector.
            /// </summary>
            /// <param name="aSector">A sector to read from.</param>
            /// <param name="aData">Output data byte.</param>
            /// <exception cref="OverflowException">Thrown when data lenght is greater then Int32.MaxValue.</exception>
            /// <exception cref="Exception">Thrown when data size invalid.</exception>
            private void ReadFatSector(ulong aSector, out byte[] aData)
            {
                aData = mFileSystem.NewBlockArray();
                ulong xSector = mFatSector + aSector;
                Global.mFileSystemDebugger.SendInternal("xSector  =");
                Global.mFileSystemDebugger.SendInternal(xSector);
                mFileSystem.Device.ReadBlock(xSector, mFileSystem.SectorsPerCluster, ref aData);
            }

            /// <summary>
            /// Write FAT sector.
            /// </summary>
            /// <param name="aSector">A sector to write to.</param>
            /// <param name="aData">A data to write.</param>
            /// <exception cref="OverflowException">Thrown when data lenght is greater then Int32.MaxValue.</exception>
            /// <exception cref="Exception">Thrown when data size invalid.</exception>
            /// <exception cref="ArgumentNullException">Thrown when aData is null.</exception>
            private void WriteFatSector(ulong aSector, byte[] aData)
            {
                if (aData == null)
                {
                    throw new ArgumentNullException(nameof(aData));
                }

                var xSector = mFatSector + aSector;
                mFileSystem.Device.WriteBlock(xSector, mFileSystem.SectorsPerCluster, ref aData);
            }

            /// <summary>
            /// Gets a FAT entry.
            /// </summary>
            /// <param name="aEntryNumber">The entry number.</param>
            /// <param name="aValue">The entry value.</param>
            /// <exception cref="OverflowException">Thrown when data lenght is greater then Int32.MaxValue.</exception>
            /// <exception cref="Exception">Thrown when data size invalid.</exception>
            /// <exception cref="ArgumentException">Thrown when bad aEntryNumber passed.</exception>
            /// <exception cref="ArgumentNullException">Thrown on fatal error (contact support).</exception>
            /// <exception cref="ArgumentOutOfRangeException">Thrown when bad aEntryNumber passed.</exception>
            /// <exception cref="NotSupportedException">Thrown when FAT type is unknown.</exception>
            internal void GetFatEntry(uint aEntryNumber, out uint aValue)
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
                Global.mFileSystemDebugger.SendInternal("xSector =");
                Global.mFileSystemDebugger.SendInternal(xSector);

                ReadFatSector(xSector, out byte[] xData);

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
            /// Sets value in a FAT entry.
            /// </summary>
            /// <param name="aEntryNumber">The entry number.</param>
            /// <param name="aValue">The value.</param>
            /// <exception cref="NotSupportedException">Thrown when FAT type is unknown.</exception>
            /// <exception cref="OverflowException">Thrown when data lenght is greater then Int32.MaxValue.</exception>
            /// <exception cref="Exception">Thrown when data size invalid.</exception>
            /// <exception cref="ArgumentNullException">Thrown when FAT sector data is null.</exception>
            internal void SetFatEntry(ulong aEntryNumber, ulong aValue)
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
            /// Sets an array of values in a FAT entry.
            /// </summary>
            /// <param name="aEntryNumber">The entry number.</param>
            /// <param name="aData">The value.</param>
            /// <param name="aOffset">The offset in the sector to write the value to</param>
            /// <param name="aLength">The length of data to write</param>
            /// <exception cref="NotSupportedException">Thrown when FAT type is unknown.</exception>
            /// <exception cref="OverflowException">Thrown when data lenght is greater then Int32.MaxValue.</exception>
            /// <exception cref="Exception">Thrown when data size invalid.</exception>
            /// <exception cref="ArgumentNullException">Thrown when FAT sector data is null.</exception>
            internal void SetFatEntry(ulong aEntryNumber, byte[] aData, uint aOffset, uint aLength)
            {
                Global.mFileSystemDebugger.SendInternal("--- Fat.SetFatEntry ---");
                Global.mFileSystemDebugger.SendInternal("aEntryNumber =");
                Global.mFileSystemDebugger.SendInternal(aEntryNumber);

                uint xEntrySize = GetFatEntrySizeInBytes();
                ulong xEntryOffset = aEntryNumber * xEntrySize;

                ulong xSector = xEntryOffset / mFileSystem.BytesPerSector;
                ulong xSectorOffset = (xSector * mFileSystem.BytesPerSector) - xEntryOffset;

                byte[] xData;
                ReadFatSector(xSectorOffset, out xData);

                switch (mFileSystem.mFatType)
                {
                    case FatTypeEnum.Fat12:
                    case FatTypeEnum.Fat16:
                    case FatTypeEnum.Fat32:
                        Array.Copy(aData, 0, xData, aOffset, aLength);
                        break;

                    default:
                        throw new NotSupportedException("Unknown FAT type.");
                }

                WriteFatSector(xSectorOffset, xData);
            }

            /// <summary>
            /// Check if FAT entry is free.
            /// </summary>
            /// <param name="aValue">A entry to check.</param>
            /// <returns>bool value.</returns>
            internal bool FatEntryIsFree(uint aValue)
            {
                return aValue == 0;
            }

            /// <summary>
            /// Check if EOF to FAT entry.
            /// </summary>
            /// <param name="aValue">A value to check if is EOF.</param>
            /// <returns>bool value.</returns>
            /// <exception cref="ArgumentException">Thrown when FAT type is unknown.</exception>
            internal bool FatEntryIsEof(uint aValue)
            {
                switch (mFileSystem.mFatType)
                {
                    case FatTypeEnum.Fat12:
                        return (aValue & 0x0FFF) >= 0x0FF8;
                    case FatTypeEnum.Fat16:
                        return (aValue & 0xFFFF) >= 0xFFF8;
                    case FatTypeEnum.Fat32:
                        return (aValue & 0x0FFFFFF8) >= 0x0FFFFFF8;
                    default:
                        throw new ArgumentException("Unknown FAT type");
                }
            }

            /// <summary>
            /// Check if FAT entry is bad.
            /// </summary>
            /// <param name="aValue">A value to check0</param>
            /// <returns>bool value.</returns>
            /// <exception cref="ArgumentException">Thrown when FAT type is unknown.</exception>
            internal bool FatEntryIsBad(uint aValue)
            {
                switch (mFileSystem.mFatType)
                {
                    case FatTypeEnum.Fat12:
                        return (aValue & 0x0FFF) == 0x0FF7;
                    case FatTypeEnum.Fat16:
                        return (aValue & 0xFFFF) == 0xFFF7;
                    case FatTypeEnum.Fat32:
                        return (aValue & 0x0FFFFFF8) == 0x0FFFFFF7;
                    default:
                        throw new ArgumentException("Unknown FAT type");
                }
            }

            /// <summary>
            /// The the EOF value for a specific FAT type.
            /// </summary>
            /// <returns>The EOF value.</returns>
            /// <exception cref="Exception">Unknown file system type.</exception>
            internal ulong FatEntryEofValue()
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

        /// <summary>
        /// Number of bytes per cluster.
        /// </summary>
        public readonly uint BytesPerCluster;

        /// <summary>
        /// Number of bytes per sector.
        /// </summary>
        public readonly uint BytesPerSector;

        /// <summary>
        /// Number of clusters.
        /// </summary>
        public readonly uint ClusterCount;

        /// <summary>
        /// First data sector.
        /// </summary>
        public readonly uint DataSector; // First Data Sector

        /// <summary>
        /// Number of data sectors.
        /// </summary>
        public readonly uint DataSectorCount;

        /// <summary>
        /// Number of FAT sectors.
        /// </summary>
        public readonly uint FatSectorCount;

        /// <summary>
        /// FAT type.
        /// <para>
        /// possible types:
        /// <list type="bullet">
        /// <item>Unknown.</item>
        /// <item>Fat12.</item>
        /// <item>Fat16.</item>
        /// <item>Fat32.</item>
        /// </list>
        /// </para>
        /// </summary>
        private readonly FatTypeEnum mFatType;

        /// <summary>
        /// Nuber of FATs in the filesystem.
        /// </summary>
        public readonly uint NumberOfFATs;

        /// <summary>
        /// Number of reserved sectors.
        /// </summary>
        public readonly uint ReservedSectorCount;

        /// <summary>
        /// FAT32 root cluster.
        /// </summary>
        public readonly uint RootCluster; // FAT32

        /// <summary>
        /// Number of root entrys.
        /// </summary>
        public readonly uint RootEntryCount;

        /// <summary>
        /// FAT12/16 root sector.
        /// </summary>
        public readonly uint RootSector; // FAT12/16

        /// <summary>
        /// Number of root sectors.
        /// <para>
        /// For FAT12/16. In FAT32 this field remains 0.
        /// </para>
        /// </summary>
        public readonly uint RootSectorCount; // FAT12/16, FAT32 remains 0

        /// <summary>
        /// Number of sectors per cluster.
        /// </summary>
        public readonly uint SectorsPerCluster;

        /// <summary>
        /// Total number of sectors.
        /// </summary>
        public readonly uint TotalSectorCount;

        /// <summary>
        /// FATs array.
        /// </summary>
        private readonly Fat[] mFats;

        /// <summary>
        /// Get FAT type.
        /// <para>
        /// possible types:
        /// <list type="bullet">
        /// <item>Unknown.</item>
        /// <item>Fat12.</item>
        /// <item>Fat16.</item>
        /// <item>Fat32.</item>
        /// </list>
        /// </para>
        /// </summary>
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
        /// <param name="aSize">The partition size.</param>
        /// <exception cref="ArgumentNullException">
        /// <list type="bullet">
        /// <item>Thrown when aDevice is null.</item>
        /// <item>Thrown when FatFileSystem is null.</item>
        /// <item>Thrown on fatal error (contact support).</item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <list type="bullet">
        /// <item>Thrown when aRootPath is null.</item>
        /// <item>Thrown on fatal error (contact support).</item>
        /// </list>
        /// </exception>
        /// <exception cref="OverflowException">Thrown on fatal error (contact support).</exception>
        /// <exception cref="Exception">
        /// <list type="bullet">
        /// <item>Thrown on fatal error (contact support).</item>
        /// <item>>FAT signature not found.</item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on fatal error (contact support).</exception>
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

            Device.ReadBlock(0UL, 1U, ref xBPB);

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

        /// <summary>
        /// Get FAT.
        /// </summary>
        /// <param name="aTableNumber">A table number to get.</param>
        /// <returns>FAT type.</returns>
        /// <exception cref="Exception">Thrown when FAT table number doesn't exist.</exception>
        internal Fat GetFat(int aTableNumber)
        {
            if (mFats.Length > aTableNumber)
            {
                return mFats[aTableNumber];
            }

            throw new Exception("The fat table number doesn't exist.");
        }

        /// <summary>
        /// Create new block array.
        /// </summary>
        /// <returns>Byte array.</returns>
        internal byte[] NewBlockArray()
        {
            return new byte[BytesPerCluster];
        }

        /// <summary>
        /// Read data from cluster.
        /// </summary>
        /// <param name="aCluster">A cluster to read from.</param>
        /// <param name="aData">A data array to write the output to.</param>
        /// <param name="aSize">prob. unused.</param>
        /// <param name="aOffset">unused.</param>
        /// <exception cref="OverflowException">Thrown when data lenght is greater then Int32.MaxValue.</exception>
        /// <exception cref="Exception">Thrown when data size invalid.</exception>
        internal void Read(long aCluster, out byte[] aData)
        {
            Global.mFileSystemDebugger.SendInternal("-- FatFileSystem.Read --");

            if (mFatType == FatTypeEnum.Fat32)
            {
                aData = NewBlockArray();
                long xSector = DataSector + (aCluster - RootCluster) * SectorsPerCluster;
                Global.mFileSystemDebugger.SendInternal("xSector: " + aCluster);
                Device.ReadBlock((ulong)xSector, SectorsPerCluster, ref aData);
            }
            else
            {
                Global.mFileSystemDebugger.SendInternal("aCluster: " + aCluster);
                aData = Device.NewBlockArray(1);
                Device.ReadBlock((ulong)aCluster, RootSectorCount, ref aData);
            }
        }

        /// <summary>
        /// Write data to cluster.
        /// </summary>
        /// <param name="aCluster">A cluster to write to.</param>
        /// <param name="aData">A data to write.</param>
        /// <param name="aSize">prob. unused.</param>
        /// <param name="aOffset">The offset to write from.</param>
        /// <exception cref="OverflowException">Thrown when data lenght is greater then Int32.MaxValue.</exception>
        /// <exception cref="Exception">Thrown when data size invalid.</exception>
        /// <exception cref="ArgumentNullException">
        /// <list type="bullet">
        /// <item>Thrown when entrys aData is null.</item>
        /// <item>Out of memory.</item>
        /// </list>
        /// </exception>
        /// <exception cref="RankException">Thrown on fatal error (contact support).</exception>
        /// <exception cref="ArrayTypeMismatchException">Thrown on fatal error (contact support).</exception>
        /// <exception cref="InvalidCastException">Thrown when the data in aData is corrupted.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <list type = "bullet" >
        /// <item>Thrown when the data length is 0 or greater then Int32.MaxValue.</item>
        /// <item>Entrys matadata offset value is invalid.</item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <list type="bullet">
        /// <item>aData length is 0.</item>
        /// </list>
        /// </exception>
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


            Array.Copy(aData, 0, xData, aOffset, aSize);

            if (mFatType == FatTypeEnum.Fat32)
            {
                long xSector = DataSector + (aCluster - RootCluster) * SectorsPerCluster;
                Device.WriteBlock((ulong)xSector, SectorsPerCluster, ref xData);
            }
            else
            {
                Device.WriteBlock((ulong)aCluster, RootSectorCount, ref xData);
            }
        }

        /// <summary>
        /// Print filesystem info.
        /// </summary>
        /// <exception cref="IOException">Thrown on I/O error.</exception>
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

        /// <summary>
        /// Get list of entries of a directory.
        /// </summary>
        /// <param name="baseDirectory">A base directory.</param>
        /// <returns>DirectoryEntry list.</returns>
        /// <exception cref="ArgumentNullException">Thrown when baseDirectory is null / memory error.</exception>
        /// <exception cref="OverflowException">Thrown when data lenght is greater then Int32.MaxValue.</exception>
        /// <exception cref="Exception">Thrown when data size invalid / invalid directory entry type.</exception>
        /// <exception cref="ArgumentException">Thrown on memory error.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on memory error.</exception>
        /// <exception cref="DecoderFallbackException">Thrown on memory error.</exception>
        public override List<DirectoryEntry> GetDirectoryListing(DirectoryEntry baseDirectory)
        {
            Global.mFileSystemDebugger.SendInternal("-- FatFileSystem.GetDirectoryListing --");
            Global.mFileSystemDebugger.SendInternal("baseDirectory: " + baseDirectory.mFullPath);

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

        /// <summary>
        /// Get root directory.
        /// </summary>
        /// <returns>DirectoryEntry value.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when root directory address is smaller then root directory address.</exception>
        /// <exception cref="ArgumentNullException">Thrown when filesystem is null.</exception>
        /// <exception cref="ArgumentException">Thrown when root path is null or empty.</exception>
        public override DirectoryEntry GetRootDirectory()
        {
            Global.mFileSystemDebugger.SendInternal("-- FatFileSystem.GetRootDirectory --");

            var xRootEntry = new FatDirectoryEntry(this, null, RootPath, RootPath, Size, RootCluster);
            return xRootEntry;
        }

        /// <summary>
        /// Create directory.
        /// </summary>
        /// <param name="aParentDirectory">A parent directory.</param>
        /// <param name="aNewDirectory">A new directory name.</param>
        /// <returns>DirectoryEntry value.</returns>
        /// <exception cref="ArgumentNullException">
        /// <list type="bullet">
        /// <item>Thrown when aParentDirectory is null.</item>
        /// <item>aNewDirectory is null or empty.</item>
        /// <item>memory error.</item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on memory error / unknown directory entry type.</exception>
        /// <exception cref="OverflowException">Thrown when data lenght is greater then Int32.MaxValue.</exception>
        /// <exception cref="Exception">Thrown when data size invalid / invalid directory entry type / memory error.</exception>
        /// <exception cref="ArgumentException">Thrown on memory error.</exception>
        /// <exception cref="DecoderFallbackException">Thrown on memory error.</exception>
        /// <exception cref="RankException">Thrown on fatal error (contact support).</exception>
        /// <exception cref="ArrayTypeMismatchException">Thrown on fatal error (contact support).</exception>
        /// <exception cref="InvalidCastException">Thrown on memory error.</exception>
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

        /// <summary>
        /// Create file.
        /// </summary>
        /// <param name="aParentDirectory">A parent directory.</param>
        /// <param name="aNewFile">A new file name.</param>
        /// <returns>DirectoryEntry value.</returns>
        /// <exception cref="ArgumentNullException">
        /// <list type="bullet">
        /// <item>Thrown when aParentDirectory is null.</item>
        /// <item>aNewFile is null or empty.</item>
        /// <item>memory error.</item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on memory error / unknown directory entry type.</exception>
        /// <exception cref="OverflowException">Thrown when data lenght is greater then Int32.MaxValue.</exception>
        /// <exception cref="Exception">Thrown when data size invalid / invalid directory entry type / memory error.</exception>
        /// <exception cref="ArgumentException">Thrown on memory error.</exception>
        /// <exception cref="DecoderFallbackException">Thrown on memory error.</exception>
        /// <exception cref="RankException">Thrown on fatal error (contact support).</exception>
        /// <exception cref="ArrayTypeMismatchException">Thrown on fatal error (contact support).</exception>
        /// <exception cref="InvalidCastException">Thrown on memory error.</exception>
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

        /// <summary>
        /// Delete directory.
        /// </summary>
        /// <param name="aDirectoryEntry">A directory entry to delete.</param>
        /// <exception cref="NotImplementedException">Thrown when given entry type is unknown.</exception>
        /// <exception cref="Exception">
        /// <list type="bullet">
        /// <item>Thrown when tring to delete root directory.</item>
        /// <item>directory entry type is invalid.</item>
        /// <item>data size invalid.</item>
        /// <item>FAT table not found.</item>
        /// <item>out of memory.</item>
        /// </list>
        /// </exception>
        /// <exception cref="OverflowException">Thrown when data lenght is greater then Int32.MaxValue.</exception>
        /// <exception cref="ArgumentNullException">
        /// <list type="bullet">
        /// <item>Thrown when aDirectoryEntry is null.</item>
        /// <item>aData is null.</item>
        /// <item>Out of memory.</item>
        /// </list>
        /// </exception>
        /// <exception cref="RankException">Thrown on fatal error (contact support).</exception>
        /// <exception cref="ArrayTypeMismatchException">Thrown on fatal error (contact support).</exception>
        /// <exception cref="InvalidCastException">Thrown when the data in aData is corrupted.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <list type = "bullet" >
        /// <item>Thrown when the data length is 0 or greater then Int32.MaxValue.</item>
        /// <item>Entrys matadata offset value is invalid.</item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <list type="bullet">
        /// <item>aData length is 0.</item>
        /// </list>
        /// </exception>
        /// <exception cref="NotSupportedException">Thrown when FAT type is unknown.</exception>
        public override void DeleteDirectory(DirectoryEntry aDirectoryEntry)
        {
            if (aDirectoryEntry == null)
            {
                throw new ArgumentNullException(nameof(aDirectoryEntry));
            }

            var xDirectoryEntry = (FatDirectoryEntry)aDirectoryEntry;

            xDirectoryEntry.DeleteDirectoryEntry();
        }

        /// <summary>
        /// Delete file.
        /// </summary>
        /// <param name="aDirectoryEntry">A directory entry to delete.</param>
        /// <exception cref="NotImplementedException">Thrown when given entry type is unknown.</exception>
        /// <exception cref="Exception">
        /// <list type="bullet">
        /// <item>Thrown when tring to delete root directory.</item>
        /// <item>directory entry type is invalid.</item>
        /// <item>data size invalid.</item>
        /// <item>FAT table not found.</item>
        /// <item>out of memory.</item>
        /// </list>
        /// </exception>
        /// <exception cref="OverflowException">
        /// <list type="bullet">
        /// <item>Thrown when data lenght is greater then Int32.MaxValue.</item>
        /// <item>The number of clusters in the FAT entry is greater than Int32.MaxValue.</item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <list type="bullet">
        /// <item>Thrown when aDirectoryEntry is null.</item>
        /// <item>aData is null.</item>
        /// <item>Out of memory.</item>
        /// </list>
        /// </exception>
        /// <exception cref="RankException">Thrown on fatal error (contact support).</exception>
        /// <exception cref="ArrayTypeMismatchException">Thrown on fatal error (contact support).</exception>
        /// <exception cref="InvalidCastException">Thrown when the data in aData is corrupted.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <list type = "bullet" >
        /// <item>Thrown when the data length is 0 or greater then Int32.MaxValue.</item>
        /// <item>The size of the chain is less then zero.</item>
        /// <item>Entrys matadata offset value is invalid.</item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <list type="bullet">
        /// <item>Thrown when FAT type is unknown.</item>
        /// <item>aData length is 0.</item>
        /// </list>
        /// </exception>
        /// <exception cref="NotSupportedException">Thrown when FAT type is unknown.</exception>
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

        /// <summary>
        /// Get and set the root directory label.
        /// </summary>
        /// <exception cref="OverflowException">Thrown when data lenght is greater then Int32.MaxValue.</exception>
        /// <exception cref="ArgumentException">
        /// <list type="bullet">
        /// <item>(get) Thrown when root path is null or empty.</item>
        /// <item>(set) Thrown when label is null or empty string.</item>
        /// <item>(set) aData length is 0.</item>
        /// <item>(get / set) memory error.</item>
        /// </list>
        /// </exception>
        /// <exception cref="Exception">
        /// <list type="bullet">
        /// <item>(get) Thrown when trying to access VolumeId out of Root Directory.</item>
        /// <item>(set) Thrown when entry metadata could not be changed.</item>
        /// <item>(get / set) Invalid entry type.</item>
        /// <item>(get / set) Invalid entry data size.</item>
        /// <item>(get / set) Invalid directory entry type.</item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <list type="bullet">
        /// <item>(get) Thrown when filesystem is null.</item>
        /// <item>(set) Thrown when entrys aValue is null.</item>
        /// <item>(set) Thrown when entrys aData is null.</item>
        /// <item>(get / set) Out of memory.</item>
        /// </list>
        /// </exception>
        /// <exception cref="EncoderFallbackException">Thrown when encoder fallback operation on aValue fails / memory error.</exception>
        /// <exception cref="RankException">Thrown on fatal error (contact support).</exception>
        /// <exception cref="ArrayTypeMismatchException">Thrown on fatal error (contact support).</exception>
        /// <exception cref="InvalidCastException">Thrown when the data in aValue is corrupted.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <list type = "bullet" >
        /// <item>(get) Thrown when root directory address is smaller then root directory address.</item>
        /// <item>(get) memory error.</item>
        /// <item>(set) Thrown when the data length is 0 or greater then Int32.MaxValue.</item>
        /// <item>(set) Entrys matadata offset value is invalid.</item>
        /// </list>
        /// </exception>
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

        /// <summary>
        /// Get size of free space available.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on fatal error (contact support).</exception>
        /// <exception cref="ArgumentNullException">Thrown when filesystem is null.</exception>
        /// <exception cref="ArgumentException">
        /// <list type="bullet">
        /// <item>Thrown when root path is null or empty.</item>
        /// <item>root directory entry data corrupted.</item>
        /// </list>
        /// </exception>
        /// <exception cref="OverflowException">Thrown when data lenght is greater then Int32.MaxValue.</exception>
        /// <exception cref="Exception">Thrown when data size invalid.</exception>
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

        /// <summary>
        /// Get total free space.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on fatal error (contact support).</exception>
        /// <exception cref="ArgumentNullException">Thrown when filesystem is null.</exception>
        /// <exception cref="ArgumentException">
        /// <list type="bullet">
        /// <item>Thrown when root path is null or empty.</item>
        /// <item>root directory entry data corrupted.</item>
        /// </list>
        /// </exception>
        /// <exception cref="OverflowException">Thrown when data lenght is greater then Int32.MaxValue.</exception>
        /// <exception cref="Exception">Thrown when data size invalid.</exception>
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

        /// <summary>
        /// FAT types.
        /// </summary>
        internal enum FatTypeEnum
        {
            Unknown,

            Fat12,

            Fat16,

            Fat32
        }

        /// <summary>
        /// Format drive. (delete all)
        /// </summary>
        /// <param name="aDriveFormat">unused.</param>
        /// <param name="aQuick">unused.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <list type = "bullet" >
        /// <item>Thrown when the data length is 0 or greater then Int32.MaxValue.</item>
        /// <item>Entrys matadata offset value is invalid.</item>
        /// <item>Fatal error (contact support).</item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentNullException">Thrown when filesystem is null / memory error.</exception>
        /// <exception cref="ArgumentException">
        /// <list type="bullet">
        /// <item>Data length is 0.</item>
        /// <item>Root path is null or empty.</item>
        /// <item>Memory error.</item>
        /// </list>
        /// </exception>
        /// <exception cref="Exception">
        /// <list type="bullet">
        /// <item>Thrown when data size invalid.</item>
        /// <item>Thrown on unknown file system type.</item>
        /// <item>Thrown on fatal error (contact support).</item>
        /// </list>
        /// </exception>
        /// <exception cref="OverflowException">Thrown when data lenght is greater then Int32.MaxValue.</exception>
        /// <exception cref="DecoderFallbackException">Thrown on memory error.</exception>
        /// <exception cref="NotImplementedException">Thrown when FAT type is unknown.</exception>
        /// <exception cref="RankException">Thrown on fatal error (contact support).</exception>
        /// <exception cref="ArrayTypeMismatchException">Thrown on fatal error (contact support).</exception>
        /// <exception cref="InvalidCastException">Thrown when the data in aData is corrupted.</exception>
        /// <exception cref="NotSupportedException">Thrown when FAT type is unknown.</exception>
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
