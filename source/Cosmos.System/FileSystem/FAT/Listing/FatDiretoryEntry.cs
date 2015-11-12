using System.Collections.Generic;

using Cosmos.System.FileSystem.Listing;

namespace Cosmos.System.FileSystem.FAT.Listing
{
    public class FatDirectoryEntry : DirectoryEntry
    {
        public readonly ulong FirstClusterNum;
        public readonly uint EntryHeaderDataOffset;
        public new readonly FatDirectoryEntry Parent;
        public new readonly FatFileSystem FileSystem;

        // Size is UInt32 because FAT doesn't support bigger.
        // Don't change to UInt64
        public FatDirectoryEntry(FatFileSystem aFileSystem, FatDirectoryEntry aParent, string aName, uint aSize, ulong aFirstClusterNum, uint aEntryHeaderDataOffset, DirectoryEntryTypeEnum aEntryType)
            : base(aFileSystem, aParent, aName, aSize, aEntryType)
        {
            FileSystem = aFileSystem;
            Parent = aParent;
            FirstClusterNum = aFirstClusterNum;
            EntryHeaderDataOffset = aEntryHeaderDataOffset;
        }

        public override void SetName(string aName)
        {
            FileSystem.SetDirectoryEntryMetadataValue(this, FatDirectoryEntryMetadata.ShortName, aName);
        }

        public override void SetSize(long aSize)
        {
            FileSystem.SetDirectoryEntryMetadataValue(this, FatDirectoryEntryMetadata.Size, (uint)aSize);
        }

        //TODO: Seperate out the file mechanics from the Listing class
        // so a file can exist without a listing instance
        internal List<ulong> GetFatTable()
        {
            var xResult = new List<ulong>((int)(Size / (FileSystem.SectorsPerCluster * FileSystem.BytesPerSector)));
            ulong xClusterNum = FirstClusterNum;

            byte[] xSector = new byte[FileSystem.BytesPerSector];
            uint? xSectorNum = null;

            do
            {
                uint xNextSectorNum;
                uint xNextSectorOffset;
                FileSystem.GetFatTableSector(xClusterNum, out xNextSectorNum, out xNextSectorOffset);
                if (xSectorNum.HasValue == false || xSectorNum != xNextSectorNum)
                {
                    FileSystem.ReadFatTableSector(xNextSectorNum, xSector);
                    xSectorNum = xNextSectorNum;
                }

                xResult.Add(xClusterNum);
                xClusterNum = FileSystem.GetFatEntry(xSector, xClusterNum, xNextSectorOffset);
            } while (!FileSystem.FatEntryIsEOF(xClusterNum));

            return xResult;
        }
    }
}
