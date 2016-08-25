//#define COSMOSDEBUG

using Cosmos.Common.Extensions;
using Cosmos.System.FileSystem.Listing;

using global::System;
using global::System.Collections.Generic;

namespace Cosmos.System.FileSystem.FAT.Listing
{
    using global::System.IO;

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
            string aName,
            uint aFirstCluster)
            : base(aFileSystem, aParent, aFullPath, aName, 0, DirectoryEntryTypeEnum.Directory)
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

        private void AllocateDirectoryEntry()
        {
            // TODO: Deal with short and long name.
            Global.mFileSystemDebugger.SendInternal("-- FatDirectoryEntry.AllocateDirectoryEntry --");

            char[] xName =
                {
                    (char)0x20, (char)0x20, (char)0x20, (char)0x20, (char)0x20, (char)0x20, (char)0x20,
                    (char)0x20, (char)0x20, (char)0x20, (char)0x20
                };

            int j = 0;
            for (int i = 0; i < mName.Length; i++)
            {
                if (mName[i] == '.')
                {
                    i++;
                    j = 8;
                }
                if (i > xName.Length)
                {
                    break;
                }
                xName[j] = mName[i];
                j++;
            }

            string xNameString = new string(xName);
            SetDirectoryEntryMetadataValue(FatDirectoryEntryMetadata.ShortName, xNameString);
            if (mEntryType == DirectoryEntryTypeEnum.Directory)
            {
                SetDirectoryEntryMetadataValue(FatDirectoryEntryMetadata.Attributes, FatDirectoryEntryAttributeConsts.Directory);
            }
            SetDirectoryEntryMetadataValue(FatDirectoryEntryMetadata.FirstClusterHigh, (uint)(mFirstClusterNum >> 16));
            SetDirectoryEntryMetadataValue(FatDirectoryEntryMetadata.FirstClusterLow, (uint)(mFirstClusterNum & 0xFFFF));
            byte[] xData = GetDirectoryEntryData();
            SetDirectoryEntryData(xData);
        }

        public FatDirectoryEntry AddDirectoryEntry(string aName, DirectoryEntryTypeEnum aType)
        {
            Global.mFileSystemDebugger.SendInternal("-- FatDirectoryEntry.AddDirectoryEntry --");
            Global.mFileSystemDebugger.SendInternal("aName =");
            Global.mFileSystemDebugger.SendInternal(aName);
            Global.mFileSystemDebugger.SendInternal("aType =");
            Global.mFileSystemDebugger.SendInternal((uint)aType);

            if ((aType == DirectoryEntryTypeEnum.Directory) || (aType == DirectoryEntryTypeEnum.File))
            {
                string xFullPath = Path.Combine(mFullPath, aName);
                uint xFirstCluster = ((FatFileSystem)mFileSystem).GetFat(0).GetNextUnallocatedFatEntry();
                uint xEntryHeaderDataOffset = GetNextUnallocatedDirectoryEntry();
                Global.mFileSystemDebugger.SendInternal("xFullPath =");
                Global.mFileSystemDebugger.SendInternal(xFullPath);
                Global.mFileSystemDebugger.SendInternal("xFirstCluster =");
                Global.mFileSystemDebugger.SendInternal(xFirstCluster);
                Global.mFileSystemDebugger.SendInternal("xEntryHeaderDataOffset =");
                Global.mFileSystemDebugger.SendInternal(xEntryHeaderDataOffset);

                var xNewEntry = new FatDirectoryEntry((FatFileSystem)mFileSystem, this, xFullPath, aName, 0, xFirstCluster, xEntryHeaderDataOffset, aType);
                xNewEntry.AllocateDirectoryEntry();
                return xNewEntry;
            }
            throw new ArgumentOutOfRangeException(nameof(aType), "Unknown directory entry type.");
        }

        public void DeleteDirectoryEntry()
        {
            if (mEntryType == DirectoryEntryTypeEnum.Unknown)
                throw new NotImplementedException();

            SetDirectoryEntryMetadataValue(FatDirectoryEntryMetadata.FirstByte, FatDirectoryEntryAttributeConsts.UnusedOrDeletedEntry);
        }

        public List<FatDirectoryEntry> ReadDirectoryContents()
        {
            Global.mFileSystemDebugger.SendInternal("-- FatDirectoryEntry.ReadDirectoryContents --");

            var xData = GetDirectoryEntryData();
            var xResult = new List<FatDirectoryEntry>();
            FatDirectoryEntry xParent = (FatDirectoryEntry)(mParent ?? mFileSystem.GetRootDirectory());

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
                        case FatDirectoryEntryAttributeConsts.UnusedOrDeletedEntry:
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

                int xTest = xAttrib & (FatDirectoryEntryAttributeConsts.Directory | FatDirectoryEntryAttributeConsts.VolumeID);
                if (xStatus == FatDirectoryEntryAttributeConsts.UnusedOrDeletedEntry)
                {
                    // deleted file
                }
                else if (xAttrib == FatDirectoryEntryAttributeConsts.LongName)
                {
                    // skip adding, as it's a LongFileName entry, meaning the next normal entry is the item with the name.
                    //Global.mFileSystemDebugger.SendInternal($"Entry was a Long FileName entry. Current LongName = '{xLongName}'");
                }
                else if (xTest == 0)
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
            }

            return xResult;
        }

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

            // TODO: What should we return if no available entry is found.
            throw new Exception("Failed to find an unallocated directory entry.");
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
            Global.mFileSystemDebugger.SendInternal("-- FatDirectoryEntry.SetDirectoryEntryMetadataValue(ulong) --");
            Global.mFileSystemDebugger.SendInternal("aValue =");
            Global.mFileSystemDebugger.SendInternal(aValue);

            if (mParent != null)
            {
                var xData = ((FatDirectoryEntry)mParent).GetDirectoryEntryData();
                if (xData.Length > 0)
                {
                    var xValue = new byte[aEntryMetadata.DataLength];
                    xValue.SetUInt32(0, (uint) aValue);
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
    }
}
