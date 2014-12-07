using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SentinelKernel.System.FileSystem.FAT.Listing
{
    public class FatFile : System.FileSystem.Listing.File
    {
        public new readonly FatFileSystem FileSystem;
        public readonly UInt64 FirstClusterNum;

        // Size is UInt32 because FAT doesn't support bigger.
        // Dont change to UInt64
        public FatFile(FatFileSystem aFileSystem, string aName, UInt32 aSize, UInt64 aFirstCluster)
            : base(aFileSystem, aName, aSize)
        {
            FileSystem = aFileSystem;
            FirstClusterNum = aFirstCluster;
        }

        //TODO: Seperate out the file mechanics from the Listing class
        // so a file can exist without a listing instance
        public List<UInt64> GetFatTable()
        {
            var xResult = new List<UInt64>((int)(Size / (FileSystem.SectorsPerCluster * FileSystem.BytesPerSector)));
            UInt64 xClusterNum = FirstClusterNum;

            byte[] xSector = new byte[FileSystem.BytesPerSector];
            UInt32? xSectorNum = null;

            UInt32 xNextSectorNum;
            UInt32 xNextSectorOffset;
            do
            {
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