//#define COSMOSDEBUG

using System;
using System.IO;
using System.Collections.Generic;

using Cosmos.Common.Extensions;
using Cosmos.HAL;
using Cosmos.System.FileSystem.Listing;

namespace Cosmos.System.FileSystem.FAT.Listing
{
    internal class FatDirectoryEntry : DirectoryEntry
    {
        private readonly uint mEntryHeaderDataOffset;

        private readonly uint mFirstClusterNum;

        // Size is UInt32 because FAT doesn't support bigger.
        // Don't change to UInt64
        public FatDirectoryEntry(
            FatFileSystem aFileSystem,
            FatDirectoryEntry aParent,
            string aFullPath,
            string aName,
            long aSize,
            uint aFirstCluster,
            uint aEntryHeaderDataOffset,
            DirectoryEntryTypeEnum aEntryType)
            : base(aFileSystem, aParent, aFullPath, aName, aSize, aEntryType)
        {
            if (aFirstCluster < aFileSystem.RootCluster)
            {
                throw new ArgumentOutOfRangeException(nameof(aFirstCluster));
            }

            mFirstClusterNum = aFirstCluster;
            mEntryHeaderDataOffset = aEntryHeaderDataOffset;
        }

        public FatDirectoryEntry(
            FatFileSystem aFileSystem,
            FatDirectoryEntry aParent,
            string aFullPath,
            long aSize,
            string aName,
            uint aFirstCluster)
            : base(aFileSystem, aParent, aFullPath, aName, aSize, DirectoryEntryTypeEnum.Directory)
        {
            if (aFirstCluster < aFileSystem.RootCluster)
            {
                throw new ArgumentOutOfRangeException(nameof(aFirstCluster));
            }

            mFirstClusterNum = aFirstCluster;
            mEntryHeaderDataOffset = 0;
        }

        public uint[] GetFatTable()
        {
            Global.mFileSystemDebugger.SendInternal("-- FatDirectoryEntry.GetFatTable --");

            var xFat = ((FatFileSystem)mFileSystem).GetFat(0);
            return xFat?.GetFatChain(mFirstClusterNum, mSize);
        }

        public FatFileSystem GetFileSystem()
        {
            Global.mFileSystemDebugger.SendInternal("-- FatDirectoryEntry.GetFileSystem --");

            return ((FatFileSystem)mFileSystem);
        }

        public override Stream GetFileStream()
        {
            Global.mFileSystemDebugger.SendInternal("-- FatDirectoryEntry.GetFileStream --");

            if (mEntryType == DirectoryEntryTypeEnum.File)
            {
                return new FatStream(this);
            }

            return null;
        }

        public override void SetName(string aName)
        {
            Global.mFileSystemDebugger.SendInternal("-- FatDirectoryEntry.SetName --");
            Global.mFileSystemDebugger.SendInternal("aName =");
            Global.mFileSystemDebugger.SendInternal(aName);

            if (string.IsNullOrEmpty(aName))
            {
                throw new ArgumentException("Argument is null or empty", nameof(aName));
            }

            SetDirectoryEntryMetadataValue(FatDirectoryEntryMetadata.ShortName, aName);
            mName = aName;
        }

        public override void SetSize(long aSize)
        {
            Global.mFileSystemDebugger.SendInternal("FatDirectoryEntry.SetSize:");
            Global.mFileSystemDebugger.SendInternal("aSize =");
            Global.mFileSystemDebugger.SendInternal(aSize);

            if (aSize < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(aSize));
            }

            SetDirectoryEntryMetadataValue(FatDirectoryEntryMetadata.Size, aSize);
            mSize = aSize;
        }

        private void AllocateDirectoryEntry(string aShortName)
        {
            Global.mFileSystemDebugger.SendInternal("-- FatDirectoryEntry.AllocateDirectoryEntry --");

            string xNameString = GetShortName(aShortName);

            SetDirectoryEntryMetadataValue(FatDirectoryEntryMetadata.ShortName, xNameString);

            if (mEntryType == DirectoryEntryTypeEnum.Directory)
            {
                SetDirectoryEntryMetadataValue(FatDirectoryEntryMetadata.Attributes, FatDirectoryEntryAttributeConsts.Directory);
            }

            // Date and Time
            uint xDate = ((((uint)RTC.Century * 100 + (uint)RTC.Year) - 1980) << 9) | (uint)RTC.Month << 5 | (uint)RTC.DayOfTheMonth;
            uint xTime = (uint)RTC.Hour << 11 | (uint)RTC.Minute << 5 | ((uint)RTC.Second / 2);

            SetDirectoryEntryMetadataValue(FatDirectoryEntryMetadata.CreationDate, xDate);
            SetDirectoryEntryMetadataValue(FatDirectoryEntryMetadata.ModifiedDate, xDate);
            SetDirectoryEntryMetadataValue(FatDirectoryEntryMetadata.CreationTime, xTime);
            SetDirectoryEntryMetadataValue(FatDirectoryEntryMetadata.ModifiedTime, xTime);

            //First cluster
            SetDirectoryEntryMetadataValue(FatDirectoryEntryMetadata.FirstClusterHigh, (uint)(mFirstClusterNum >> 16));
            SetDirectoryEntryMetadataValue(FatDirectoryEntryMetadata.FirstClusterLow, (uint)(mFirstClusterNum & 0xFFFF));

            // GetFatTable calls GetFatChain, which "refreshes" the FAT table and clusters
            GetFatTable();
        }

        public FatDirectoryEntry AddDirectoryEntry(string aName, DirectoryEntryTypeEnum aEntryType)
        {
            Global.mFileSystemDebugger.SendInternal("-- FatDirectoryEntry.AddDirectoryEntry --");
            Global.mFileSystemDebugger.SendInternal("aName =");
            Global.mFileSystemDebugger.SendInternal(aName);
            Global.mFileSystemDebugger.SendInternal("aEntryType =");
            Global.mFileSystemDebugger.SendInternal((uint)aEntryType);

            if ((aEntryType == DirectoryEntryTypeEnum.Directory) || (aEntryType == DirectoryEntryTypeEnum.File))
            {
                string xShortName = aName;
                uint[] xDirectoryEntriesToAllocate = null;

                //Stack corruption, just delete everything from this until commented if when it's fixed
                var x1 = aEntryType == DirectoryEntryTypeEnum.File;
                var x2 = aName.Contains(".");
                var x3 = x2 ? aName.Substring(0, aName.LastIndexOf('.')).Contains(".") : false;
                var x4 = x2 ? aName.Substring(0, aName.IndexOf('.')).Length > 8 : false;
                var x5 = x2 ? aName.Substring(aName.IndexOf('.') + 1).Length > 3 : false;
                var x6 = aEntryType == DirectoryEntryTypeEnum.Directory;
                var x7 = aName.Length > 11;

                var x8 = (x3 || (x4 || x5));
                var x9 = (x2 && x8);

                var x10 = (x1 && x9) || (x6 && x7);

                //if ((aEntryType == DirectoryEntryTypeEnum.File && (aName.Contains(".") && (aName.Substring(0, aName.LastIndexOf('.')).Contains(".") || (aName.Substring(0, aName.IndexOf('.')).Length > 8 || aName.Substring(aName.IndexOf('.') + 1).Length > 3)))) ||
                //    (aEntryType == DirectoryEntryTypeEnum.Directory && aName.Length > 11))
                if (x10)
                {
                    string xLongName = aName;

                    char[] xInvalidShortNameChars = new char[] { '"', '*', '+', ',', '.', '/', ':', ';', '<', '=', '>', '?', '[', '\\', ']', '|' };

                    int xLastPeriodPosition = aName.LastIndexOf('.');

                    string xExt = "";

                    if (xLastPeriodPosition + 1 > 0 && xLastPeriodPosition + 1 < aName.Length)
                    {
                        xExt = xShortName.Substring(xLastPeriodPosition + 1);
                    }

                    for (int i = xShortName.Length - 1; i > 0; i--)
                    {
                        char xChar = xShortName[i];

                        if (char.IsWhiteSpace(xChar) || (xChar == '.' && i != xLastPeriodPosition))
                        {
                            xShortName.Remove(i, 1);
                        }
                    }

                    foreach (char xInvalidChar in xInvalidShortNameChars)
                    {
                        xShortName.Replace(xInvalidChar, '_');
                    }

                    int n = 1;
                    List<FatDirectoryEntry> xDirectoryEntries = ReadDirectoryContents(true);
                    string[] xShortFilenames = new string[xDirectoryEntries.Count];

                    for (int i = 0; i < xDirectoryEntries.Count; i++)
                    {
                        xShortFilenames[i] = xDirectoryEntries[i].mName;
                    }

                    string xNameTry = "";

                    bool xTest = false;

                    do
                    {
                        xNameTry = (xShortName.Substring(0, 7 - n.ToString().Length) + "~" + n).ToUpperInvariant();

                        if (!string.IsNullOrEmpty(xExt))
                        {
                            xNameTry += '.' + xExt.ToUpperInvariant();
                        }

                        n++;

                        xTest = false;

                        foreach (string name in xShortFilenames)
                        {
                            if (name == xNameTry)
                            {
                                xTest = true;
                                break;
                            }
                        }
                    }
                    //TODO: Array.TrySZIndexOf plug is not being recognized; to use the generic version of IndexOf, just remove the cast to Array
                    //while (Array.IndexOf((Array)xShortFilenames, xNameTry) != -1);
                    while (xTest);

                    xShortName = xNameTry;

                    uint xChecksum = CalculateChecksum(GetShortName(xShortName));

                    int xNumEntries = (int)Math.Ceiling((double)xLongName.Length / (double)13);

                    char[] xLongNameWithPad = new char[xNumEntries * 13];

                    xLongNameWithPad[xLongNameWithPad.Length - 1] = (char)0xFFFF;
                    Array.Copy(xLongName.ToCharArray(), xLongNameWithPad, xLongName.Length);

                    xDirectoryEntriesToAllocate = GetNextUnallocatedDirectoryEntries(xNumEntries + 1);

                    for (int i = xNumEntries - 1; i >= 0; i--)
                    {
                        uint xEntry = xDirectoryEntriesToAllocate[xNumEntries - i - 1];

                        SetLongFilenameEntryMetadataValue(xEntry, FatDirectoryEntryMetadata.LongFilenameEntryMetadata.SequenceNumberAndAllocationStatus, (i + 1) | (i == xNumEntries - 1 ? (1 << 6) : 0));
                        SetLongFilenameEntryMetadataValue(xEntry, FatDirectoryEntryMetadata.LongFilenameEntryMetadata.Attributes, FatDirectoryEntryAttributeConsts.LongName);
                        SetLongFilenameEntryMetadataValue(xEntry, FatDirectoryEntryMetadata.LongFilenameEntryMetadata.Checksum, xChecksum);

                        SetLongFilenameEntryMetadataValue(xEntry, FatDirectoryEntryMetadata.LongFilenameEntryMetadata.LongName1, new string(xLongNameWithPad, i * 13, 5));
                        SetLongFilenameEntryMetadataValue(xEntry, FatDirectoryEntryMetadata.LongFilenameEntryMetadata.LongName2, new string(xLongNameWithPad, i * 13 + 5, 6));
                        SetLongFilenameEntryMetadataValue(xEntry, FatDirectoryEntryMetadata.LongFilenameEntryMetadata.LongName3, new string(xLongNameWithPad, i * 13 + 11, 2));
                    }
                }

                string xFullPath = Path.Combine(mFullPath, aName);
                uint xFirstCluster = ((FatFileSystem)mFileSystem).GetFat(0).GetNextUnallocatedFatEntry();
                uint xEntryHeaderDataOffset = xDirectoryEntriesToAllocate == null ? GetNextUnallocatedDirectoryEntry() : xDirectoryEntriesToAllocate[xDirectoryEntriesToAllocate.Length - 1];

                Global.mFileSystemDebugger.SendInternal("xFullPath =");
                Global.mFileSystemDebugger.SendInternal(xFullPath);
                Global.mFileSystemDebugger.SendInternal("xFirstCluster =");
                Global.mFileSystemDebugger.SendInternal(xFirstCluster);
                Global.mFileSystemDebugger.SendInternal("xEntryHeaderDataOffset =");
                Global.mFileSystemDebugger.SendInternal(xEntryHeaderDataOffset);

                var xNewEntry = new FatDirectoryEntry((FatFileSystem)mFileSystem, this, xFullPath, aName, 0, xFirstCluster, xEntryHeaderDataOffset, aEntryType);

                xNewEntry.AllocateDirectoryEntry(xShortName);

                return xNewEntry;
            }

            throw new ArgumentOutOfRangeException(nameof(aEntryType), "Unknown directory entry type.");
        }

        public void DeleteDirectoryEntry()
        {
            if (mEntryType == DirectoryEntryTypeEnum.Unknown)
            {
                throw new NotImplementedException();
            }

            if (mParent != null)
            {
                var xData = ((FatDirectoryEntry)mParent).GetDirectoryEntryData();

                var xEntryOffset = mEntryHeaderDataOffset - 32;

                while (xData[xEntryOffset + 11] == FatDirectoryEntryAttributeConsts.LongName)
                {
                    xData[xEntryOffset] = FatDirectoryEntryAttributeConsts.UnusedOrDeletedEntry;
                    xEntryOffset -= 32;
                }

                ((FatDirectoryEntry)mParent).SetDirectoryEntryData(xData);
            }
            else
            {
                throw new Exception("Parent directory is null");
            }

            SetDirectoryEntryMetadataValue(FatDirectoryEntryMetadata.FirstByte, FatDirectoryEntryAttributeConsts.UnusedOrDeletedEntry);

            // GetFatTable calls GetFatChain, which "refreshes" the FAT table and clusters
            GetFatTable();
        }

        /// <summary>
        /// Retrieves a <see cref="List{T}"/> of <see cref="FatDirectoryEntry"/> objects that represent the Directory Entries inside this Directory
        /// </summary>
        /// <returns>Returns a <see cref="List{T}"/> of the Directory Entries inside this Directory</returns>
        public List<FatDirectoryEntry> ReadDirectoryContents(bool aReturnShortFilenames = false)
        {
            Global.mFileSystemDebugger.SendInternal("-- FatDirectoryEntry.ReadDirectoryContents --");

            var xData = GetDirectoryEntryData();
            var xResult = new List<FatDirectoryEntry>();
            FatDirectoryEntry xParent = this;

            //TODO: Change xLongName to StringBuilder
            string xLongName = "";
            string xName = "";

            for (uint i = 0; i < xData.Length; i = i + 32)
            {
                byte xAttrib = xData[i + 11];
                byte xStatus = xData[i];

                if (xAttrib == FatDirectoryEntryAttributeConsts.LongName)
                {
                    byte xType = xData[i + 12];

                    if (aReturnShortFilenames)
                    {
                        continue;
                    }

                    if (xStatus == FatDirectoryEntryAttributeConsts.UnusedOrDeletedEntry)
                    {
                        Global.mFileSystemDebugger.SendInternal("<DELETED> : Attrib = " + xAttrib + ", Status = " + xStatus);
                        continue;
                    }

                    if (xType == 0)
                    {
                        if ((xStatus & 0x40) > 0)
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
                        Global.mFileSystemDebugger.SendInternal("<EOF> : Attrib = " + xAttrib + ", Status = " + xStatus);
                        break;
                    }

                    switch (xStatus)
                    {
                        case 0x05:
                            // Japanese characters - We dont handle these
                            break;
                        case 0x2E:
                            // Dot entry
                            continue;
                        case FatDirectoryEntryAttributeConsts.UnusedOrDeletedEntry:
                            // Empty slot, skip it
                            continue;
                        default:
                            int xTest = xAttrib & (FatDirectoryEntryAttributeConsts.Directory | FatDirectoryEntryAttributeConsts.VolumeID);

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
                                    if (xTest == 0)
                                    {
                                        string xEntry = xData.GetAsciiString(i, 11);
                                        xName = xEntry.Substring(0, 8).TrimEnd();
                                        string xExt = xEntry.Substring(8, 3).TrimEnd();

                                        if (xExt.Length > 0)
                                        {
                                            xName = xName + "." + xExt;
                                        }
                                    }
                                    else
                                    {
                                        xName = xData.GetAsciiString(i, 11).TrimEnd();
                                    }
                                }
                            }

                            uint xFirstCluster = (uint)(xData.ToUInt16(i + 20) << 16 | xData.ToUInt16(i + 26));

                            if (xTest == 0)
                            {
                                uint xSize = xData.ToUInt32(i + 28);

                                if (xSize == 0 && xName.Length == 0)
                                {
                                    continue;
                                }

                                string xFullPath = Path.Combine(mFullPath, xName);
                                var xEntry = new FatDirectoryEntry(((FatFileSystem)mFileSystem), xParent, xFullPath, xName, xSize, xFirstCluster, i, DirectoryEntryTypeEnum.File);
                                xResult.Add(xEntry);
                                Global.mFileSystemDebugger.SendInternal(xEntry.mName + " - " + xEntry.mSize + " bytes");
                            }
                            else if (xTest == FatDirectoryEntryAttributeConsts.Directory)
                            {
                                string xFullPath = Path.Combine(mFullPath, xName);
                                uint xSize = xData.ToUInt32(i + 28);
                                var xEntry = new FatDirectoryEntry(((FatFileSystem)mFileSystem), xParent, xFullPath, xName, xSize, xFirstCluster, i, DirectoryEntryTypeEnum.Directory);
                                Global.mFileSystemDebugger.SendInternal(xEntry.mName + " <DIR> " + xEntry.mSize + " bytes : Attrib = " + xAttrib + ", Status = " + xStatus);
                                xResult.Add(xEntry);
                            }
                            else if (xTest == FatDirectoryEntryAttributeConsts.VolumeID)
                            {
                                Global.mFileSystemDebugger.SendInternal("<VOLUME ID> : Attrib = " + xAttrib + ", Status = " + xStatus);
                            }
                            else
                            {
                                Global.mFileSystemDebugger.SendInternal("<INVALID ENTRY> : Attrib = " + xAttrib + ", Status = " + xStatus);
                            }
                            break;
                    }
                }
            }

            return xResult;
        }

        /// <summary>
        /// Tries to find an empty space for a directory entry and returns the offset to that space if successful, otherwise throws an exception.
        /// </summary>
        /// <returns>Returns the offset to the next unallocated directory entry.</returns>
        private uint GetNextUnallocatedDirectoryEntry()
        {
            Global.mFileSystemDebugger.SendInternal("-- FatDirectoryEntry.GetNextUnallocatedDirectoryEntry --");

            var xData = GetDirectoryEntryData();
            for (uint i = 0; i < xData.Length; i += 32)
            {
                uint x1 = xData.ToUInt32(i);
                uint x2 = xData.ToUInt32(i + 8);
                uint x3 = xData.ToUInt32(i + 16);
                uint x4 = xData.ToUInt32(i + 24);
                if ((x1 == 0) && (x2 == 0) && (x3 == 0) && (x4 == 0))
                {
                    Global.mFileSystemDebugger.SendInternal("Returning i =");
                    Global.mFileSystemDebugger.SendInternal(i);
                    return i;
                }
            }

            // TODO: What should we return if no available entry is found. - Update Method description above.
            throw new Exception("Failed to find an unallocated directory entry.");
        }

        /// <summary>
        /// Tries to find an empty space for the specified number of directory entries and returns an array of offsets to those spaces if successful, otherwise throws an exception.
        /// </summary>
        /// <param name="aEntryCount">The number of entried to allocate.</param>
        /// <returns>Returns an array of offsets to the next unallocated directory entries.</returns>
        private uint[] GetNextUnallocatedDirectoryEntries(int aEntryCount)
        {
            Global.mFileSystemDebugger.SendInternal("-- FatDirectoryEntry.GetNextUnallocatedDirectoryEntry --");

            var xData = GetDirectoryEntryData();
            int xCount = 0;
            uint[] xEntries = new uint[aEntryCount];

            for (uint i = 0; i < xData.Length; i += 32)
            {
                uint x1 = xData.ToUInt32(i);
                uint x2 = xData.ToUInt32(i + 8);
                uint x3 = xData.ToUInt32(i + 16);
                uint x4 = xData.ToUInt32(i + 24);
                if ((x1 == 0) && (x2 == 0) && (x3 == 0) && (x4 == 0))
                {
                    xEntries[xCount] = i;
                    xCount++;

                    if (aEntryCount == xCount)
                    {
                        return xEntries;
                    }
                }
                else
                {
                    xCount = 0;
                }
            }

            // TODO: What should we return if no available entry is found. - Update Method description above.
            throw new Exception($"Failed to find {aEntryCount} unallocated directory entries.");
        }

        private byte[] GetDirectoryEntryData()
        {
            Global.mFileSystemDebugger.SendInternal("-- FatDirectoryEntry.GetDirectoryEntryData --");

            if (mEntryType != DirectoryEntryTypeEnum.Unknown)
            {
                byte[] xData;
                ((FatFileSystem)mFileSystem).Read(mFirstClusterNum, out xData);
                return xData;
            }

            throw new Exception("Invalid directory entry type");
        }

        private void SetDirectoryEntryData(byte[] aData)
        {
            Global.mFileSystemDebugger.SendInternal("-- FatDirectoryEntry.SetDirectoryEntryData(byte) --");
            Global.mFileSystemDebugger.SendInternal("aData.Length =");
            Global.mFileSystemDebugger.SendInternal(aData.Length);

            if (aData == null)
            {
                throw new ArgumentNullException(nameof(aData));
            }

            if (aData.Length == 0)
            {
                throw new ArgumentException("aData does not contain any data.", nameof(aData));
            }

            if (mEntryType != DirectoryEntryTypeEnum.Unknown)
            {
                ((FatFileSystem)mFileSystem).Write(mFirstClusterNum, aData);
            }
            else
            {
                throw new Exception("Invalid directory entry type");
            }
        }

        internal void SetDirectoryEntryMetadataValue(FatDirectoryEntryMetadata aEntryMetadata, uint aValue)
        {
            Global.mFileSystemDebugger.SendInternal(" -- FatDirectoryEntry.SetDirectoryEntryMetadataValue(uint) --");
            Global.mFileSystemDebugger.SendInternal("aValue =");
            Global.mFileSystemDebugger.SendInternal(aValue);

            if (mParent != null)
            {
                var xData = ((FatDirectoryEntry)mParent).GetDirectoryEntryData();

                if (xData.Length > 0)
                {
                    var xValue = new byte[aEntryMetadata.DataLength];
                    xValue.SetUInt32(0, aValue);
                    uint offset = mEntryHeaderDataOffset + aEntryMetadata.DataOffset;
                    Array.Copy(xValue, 0, xData, offset, aEntryMetadata.DataLength);
                    ((FatDirectoryEntry)mParent).SetDirectoryEntryData(xData);
                }
            }
            else
            {
                throw new Exception("Root directory metadata can not be changed using the file stream.");
            }
        }

        internal void SetDirectoryEntryMetadataValue(FatDirectoryEntryMetadata aEntryMetadata, long aValue)
        {
            Global.mFileSystemDebugger.SendInternal("-- FatDirectoryEntry.SetDirectoryEntryMetadataValue(long) --");
            Global.mFileSystemDebugger.SendInternal("aValue =");
            Global.mFileSystemDebugger.SendInternal(aValue);

            if (mParent != null)
            {
                var xData = ((FatDirectoryEntry)mParent).GetDirectoryEntryData();

                if (xData.Length > 0)
                {
                    var xValue = new byte[aEntryMetadata.DataLength];
                    xValue.SetUInt32(0, (uint)aValue);
                    uint offset = mEntryHeaderDataOffset + aEntryMetadata.DataOffset;
                    Global.mFileSystemDebugger.SendInternal("offset =");
                    Global.mFileSystemDebugger.SendInternal(offset);
                    Array.Copy(xValue, 0, xData, offset, aEntryMetadata.DataLength);
                    ((FatDirectoryEntry)mParent).SetDirectoryEntryData(xData);
                }
            }
            else
            {
                throw new Exception("Root directory metadata can not be changed using the file stream.");
            }
        }

        internal void SetDirectoryEntryMetadataValue(FatDirectoryEntryMetadata aEntryMetadata, string aValue)
        {
            Global.mFileSystemDebugger.SendInternal("-- FatDirectoryEntry.SetDirectoryEntryMetadataValue(string) --");
            Global.mFileSystemDebugger.SendInternal("aValue =");
            Global.mFileSystemDebugger.SendInternal(aValue);

            var xData = ((FatDirectoryEntry)mParent).GetDirectoryEntryData();

            if (xData.Length > 0)
            {
                var xValue = new byte[aEntryMetadata.DataLength];
                xValue = aValue.GetUtf8Bytes(0, aEntryMetadata.DataLength);

                uint offset = mEntryHeaderDataOffset + aEntryMetadata.DataOffset;
                Array.Copy(xValue, 0, xData, offset, aEntryMetadata.DataLength);

                ((FatDirectoryEntry)mParent).SetDirectoryEntryData(xData);
            }
        }

        internal void SetLongFilenameEntryMetadataValue(uint aEntryHeaderDataOffset, FatDirectoryEntryMetadata aEntryMetadata, uint aValue)
        {
            Global.mFileSystemDebugger.SendInternal(" -- FatDirectoryEntry.SetLongFilenameEntryMetadataValue(uint) --");
            Global.mFileSystemDebugger.SendInternal("aValue =");
            Global.mFileSystemDebugger.SendInternal(aValue);

            var xData = GetDirectoryEntryData();

            if (xData.Length > 0)
            {
                var xValue = new byte[aEntryMetadata.DataLength];
                xValue.SetUInt32(0, aValue);
                uint offset = aEntryHeaderDataOffset + aEntryMetadata.DataOffset;
                Array.Copy(xValue, 0, xData, (int)offset, (int)aEntryMetadata.DataLength);
                SetDirectoryEntryData(xData);
            }
        }

        internal void SetLongFilenameEntryMetadataValue(uint aEntryHeaderDataOffset, FatDirectoryEntryMetadata aEntryMetadata, long aValue)
        {
            Global.mFileSystemDebugger.SendInternal("-- FatDirectoryEntry.SetLongFilenameEntryMetadataValue(long) --");
            Global.mFileSystemDebugger.SendInternal("aValue =");
            Global.mFileSystemDebugger.SendInternal(aValue);

            var xData = GetDirectoryEntryData();

            if (xData.Length > 0)
            {
                var xValue = new byte[aEntryMetadata.DataLength];
                xValue.SetUInt32(0, (uint)aValue);
                uint offset = aEntryHeaderDataOffset + aEntryMetadata.DataOffset;
                Global.mFileSystemDebugger.SendInternal("offset =");
                Global.mFileSystemDebugger.SendInternal(offset);
                Array.Copy(xValue, 0, xData, (int)offset, (int)aEntryMetadata.DataLength);
                SetDirectoryEntryData(xData);
            }
        }

        internal void SetLongFilenameEntryMetadataValue(uint aEntryHeaderDataOffset, FatDirectoryEntryMetadata aEntryMetadata, string aValue)
        {
            Global.mFileSystemDebugger.SendInternal("-- FatDirectoryEntry.SetLongFilenameEntryMetadataValue(string) --");
            Global.mFileSystemDebugger.SendInternal("aValue =");
            Global.mFileSystemDebugger.SendInternal(aValue);

            var xData = GetDirectoryEntryData();

            if (xData.Length > 0)
            {
                var xValue = new byte[aEntryMetadata.DataLength];
                xValue = aValue.GetUtf16Bytes(0, aEntryMetadata.DataLength / 2);

                uint offset = aEntryHeaderDataOffset + aEntryMetadata.DataOffset;
                Array.Copy(xValue, 0, xData, (int)offset, (int)aEntryMetadata.DataLength);

                SetDirectoryEntryData(xData);
            }
        }

        /// <summary>
        /// Gets the short filename to be written to the FAT directory entry.
        /// </summary>
        /// <param name="aShortName">The short filename.</param>
        /// <returns>Returns the short filename to be written to the FAT directory entry.</returns>
        internal static string GetShortName(string aShortName)
        {
            char[] xName = new char[11];

            for (int i = 0; i < xName.Length; i++)
            {
                xName[i] = (char)0x20;
            }

            int j = 0;

            for (int i = 0; i < aShortName.Length; i++)
            {
                if (aShortName[i] == '.')
                {
                    i++;
                    j = 8;
                }

                if (i > xName.Length)
                {
                    break;
                }

                xName[j] = aShortName[i];

                j++;
            }

            return new string(xName);
        }

        /// <summary>
        /// Calculates the checksum for a given short filename.
        /// </summary>
        /// <param name="aShortName">The short filename without the extension period.</param>
        /// <returns>Returns the checksum for the given short filename.</returns>
        internal static uint CalculateChecksum(string aShortName)
        {
            uint xChecksum = 0;

            for (int i = 0; i < 11; i++)
            {
                xChecksum = (((xChecksum & 1) << 7) | ((xChecksum & 0xFE) >> 1)) + aShortName[i];
            }

            return xChecksum;
        }
    }
}
