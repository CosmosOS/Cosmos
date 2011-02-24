using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.System.Filesystem {
  public class FAT : Filesystem {
    readonly public int BytesPerSector;
    readonly public int SectorsPerCluster;
    readonly public int ReservedSectorCount;
    readonly public int NumberOfFATs;
    readonly public int FatSectorCount;
    readonly public int TotalSectorCount;
    readonly public int DataSectorCount;
    readonly public int ClusterCount;
    readonly public int RootSector; // FAT12/16
    readonly public int RootCluster; // FAT32
    readonly public int RootEntryCount;

    public enum FatTypeEnum {Unknown, Fat12, Fat16, Fat32}
    readonly public FatTypeEnum FatType = FatTypeEnum.Unknown;

    Cosmos.Hardware.BlockDevice.BlockDevice mDevice;

    public FAT(Cosmos.Hardware.BlockDevice.BlockDevice aDevice) {
      mDevice = aDevice;

      // 0xAA55
      // [510] == 0x55
      // [511] == 0xAA
      // This is the FAT signature. We need to check it in the future.

      var xBPB = aDevice.GetDataArray(1);
      mDevice.ReadBlock(0, 1, xBPB);
      BytesPerSector = xBPB[12] << 8 | xBPB[11];
      SectorsPerCluster = xBPB[13];
      ReservedSectorCount = xBPB[15] << 8 | xBPB[14];
      NumberOfFATs = xBPB[16];
      RootEntryCount = xBPB[18] << 8 | xBPB[17];

      TotalSectorCount = xBPB[20] << 8 | xBPB[19];
      if (TotalSectorCount == 0) {
        TotalSectorCount = xBPB[35] << 24 | xBPB[34] << 16 | xBPB[33] << 8 | xBPB[32];
      }

      FatSectorCount = xBPB[23] << 8 | xBPB[22];
      if (FatSectorCount == 0) {
        FatSectorCount = xBPB[39] << 24 | xBPB[38] << 16 | xBPB[37] << 8 | xBPB[36];
      }

      DataSectorCount = TotalSectorCount - (ReservedSectorCount + (NumberOfFATs * FatSectorCount) + ReservedSectorCount);
 
      // Computation rounds down. 
      ClusterCount = DataSectorCount / SectorsPerCluster;
      // Determine the FAT type. Do not use another method - this IS the official and
      // proper way to determine FAT type.
      // Comparisons are purposefully < and not <=
      // FAT16 starts at 4085, FAT32 starts at 65525 
      if (ClusterCount < 4085) {
        FatType = FatTypeEnum.Fat12;
      } else if(ClusterCount < 65525) {
        FatType = FatTypeEnum.Fat16;
      } else {
        FatType = FatTypeEnum.Fat32;
      }

      if (FatType == FatTypeEnum.Fat32) {
        RootCluster = xBPB[47] << 24 | xBPB[46] << 16 | xBPB[45] << 8 | xBPB[44];
      } else {
        RootSector = ReservedSectorCount + (NumberOfFATs * FatSectorCount);
      }
    }

  }
}
