using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Common.Extensions;

namespace Cosmos.System.Filesystem {
  public class FAT : Filesystem {
    readonly public UInt32 BytesPerSector;
    readonly public UInt32 SectorsPerCluster;
    
    readonly public UInt32 ReservedSectorCount;
    readonly public UInt32 TotalSectorCount;
    readonly public UInt32 ClusterCount;
    
    readonly public UInt32 NumberOfFATs;
    readonly public UInt32 FatSectorCount;

    readonly public UInt32 RootSector = 0; // FAT12/16
    readonly public UInt32 RootSectorCount = 0; // FAT12/16, FAT32 remains 0
    readonly public UInt32 RootCluster; // FAT32
    readonly public UInt32 RootEntryCount;

    readonly public UInt32 DataSector; // First Data Sector
    readonly public UInt32 DataSectorCount;

    public enum FatTypeEnum {Unknown, Fat12, Fat16, Fat32}
    readonly public FatTypeEnum FatType = FatTypeEnum.Unknown;

    Cosmos.Hardware.BlockDevice.BlockDevice mDevice;

    public FAT(Cosmos.Hardware.BlockDevice.BlockDevice aDevice) {
      mDevice = aDevice;

      var xBPB = aDevice.NewBlockArray(1);
      mDevice.ReadBlock(0, 1, xBPB);

      UInt16 xSig = xBPB.ToUInt16(510);
      if (xSig != 0xAA55) {
        throw new Exception("FAT signature not found.");
      }

      BytesPerSector = xBPB.ToUInt16(11);
      SectorsPerCluster = xBPB[13];
      ReservedSectorCount = xBPB.ToUInt16(14);
      NumberOfFATs = xBPB[16];
      RootEntryCount = xBPB.ToUInt16(17);

      TotalSectorCount = xBPB.ToUInt16(19);
      if (TotalSectorCount == 0) {
        TotalSectorCount = xBPB.ToUInt32(32);
      }

      // FATSz
      FatSectorCount = xBPB.ToUInt16(22);
      if (FatSectorCount == 0) {
        FatSectorCount = xBPB.ToUInt32(36);
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
        RootCluster = xBPB.ToUInt32(44);
      } else {
        RootSector = ReservedSectorCount + (NumberOfFATs * FatSectorCount);
        RootSectorCount = (RootEntryCount * 32 + (BytesPerSector - 1)) / BytesPerSector;
      }
      DataSector = ReservedSectorCount + (NumberOfFATs * FatSectorCount) + RootSectorCount;
    }

    protected byte[] NewClusterArray() {
      return new byte[SectorsPerCluster * BytesPerSector];
    }

    protected void ReadCluster(UInt32 aCluster, byte[] aData) {
      //TODO:UInt64
      UInt32 xSector = DataSector + ((aCluster - 2) * SectorsPerCluster);
      mDevice.ReadBlock(xSector, SectorsPerCluster, aData);
    }

    public List<string> GetDir() {
      var xResult = new List<string>();

      byte[] xData;
      if (FatType == FatTypeEnum.Fat32) {
        xData = NewClusterArray();
        ReadCluster(RootCluster, xData);
      } else {
        xData = mDevice.NewBlockArray(RootSectorCount);
        mDevice.ReadBlock(RootSector, RootSectorCount, xData);
      }

      for (int i = 0; i < xData.Length; i = i + 32) {
        byte xStatus = xData[i];
        byte xAttrib = xData[i + 11];
        byte xType = xData[i + 12];
        if (xStatus == 0xE5) {
          // 0xE5 = Empty slot
        } else if (xStatus == 0x00) {
          // Empty slot, and no more entries after this
          break;
        } else if (xStatus == 0x05) {
          // Japanese characters - We dont handle this
        } else {
          string xName = xData.GetAsciiString(i, 11);
          int x = 4;
        }
      }

      return xResult;
    }

  }
}
