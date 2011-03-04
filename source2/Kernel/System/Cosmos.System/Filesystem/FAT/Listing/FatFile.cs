using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.System.Filesystem.FAT.Listing {
  public class FatFile : Cosmos.System.Filesystem.Listing.File {
    public readonly FatFileSystem FileSystem;
    public readonly UInt32 FirstClusterNum;

    // Size is UInt32 because FAT doesn't support bigger.
    // Dont change to UInt64
    public FatFile(FatFileSystem aFileSystem, string aName, UInt32 aSize, UInt32 aFirstCluster)
      : base(aFileSystem, aName, aSize) {
      FileSystem = aFileSystem;
      FirstClusterNum = aFirstCluster;
    }

    //TODO: Seperate out the file mechanics from the Listing class
    // so a file can exist without a listing instance
    public List<UInt32> GetFatTable() {
      var xResult = new List<UInt32>((int)(Size / (FileSystem.SectorsPerCluster * FileSystem.BytesPerSector)));
      UInt32 xClusterNum = FirstClusterNum;

      byte[] xSector = new byte[FileSystem.BytesPerSector];
      UInt32? xSectorNum = null;

      UInt32 xNextSectorNum;
      UInt32 xNextSectorOffset;
      do {
        FileSystem.GetFatTableSector(xClusterNum, out xNextSectorNum, out xNextSectorOffset);
        if (xSectorNum.HasValue == false || xSectorNum != xNextSectorNum) {
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
