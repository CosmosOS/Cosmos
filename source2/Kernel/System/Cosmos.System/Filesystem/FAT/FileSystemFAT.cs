using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Common.Extensions;

namespace Cosmos.System.Filesystem.FAT {
  public class FileSystemFAT : FileSystem {
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

    public static class Attribs {
      public const int Test = 0x01;
      public const int Hidden = 0x02;
      public const int System = 0x04;
      public const int VolumeID = 0x08;
      public const int Directory = 0x10;
      public const int Archive = 0x20;
      // LongName was created after and is a combination of other attribs. Its "special".
      public const int LongName = 0x0F;
    }

    public enum FatTypeEnum {Unknown, Fat12, Fat16, Fat32}
    readonly public FatTypeEnum FatType = FatTypeEnum.Unknown;

    Cosmos.Hardware.BlockDevice.BlockDevice mDevice;

    public FileSystemFAT(Cosmos.Hardware.BlockDevice.BlockDevice aDevice) {
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

    public List<Listing.Base> GetDir() {
      var xResult = new List<Listing.Base>();

      byte[] xData;
      if (FatType == FatTypeEnum.Fat32) {
        xData = NewClusterArray();
        ReadCluster(RootCluster, xData);
      } else {
        xData = mDevice.NewBlockArray(RootSectorCount);
        mDevice.ReadBlock(RootSector, RootSectorCount, xData);
      }

      //TODO: Change xLongName to StringBuilder
      string xLongName = "";
      for (int i = 0; i < xData.Length; i = i + 32) {
        byte xAttrib = xData[i + 11];
        byte xType = xData[i + 12];
        byte xStatus = xData[i];
        if (xAttrib == Attribs.LongName) {
          if (xType == 0) {
            byte xOrd = xData[i];
            if ((xOrd & 0x40) > 0) {
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
            if (xData.ToUInt16(i + 14) != 0xFFFF || xLongPart.Length == 5) {
              xLongPart = xLongPart + xData.GetUtf16String(i + 14, 6);
              if (xData.ToUInt16(i + 28) != 0xFFFF || xLongPart.Length == 11) {
                xLongPart = xLongPart + xData.GetUtf16String(i + 28, 2);
              }
            }
            xLongName = xLongPart + xLongName;
            //TODO: LDIR_Chksum 
          }
        } else {
          if (xStatus == 0x00) {
            // Empty slot, and no more entries after this
            break;
          } else if (xStatus == 0x05) {
            // Japanese characters - We dont handle these
          } else if (xStatus == 0xE5) {
            // Empty slot, skip it
          } else if (xStatus >= 0x20) {
            string xName;
            if (xLongName.Length > 0) {
              // Leading and trailing spaces are to be ignored according to spec.
              // Many programs (including Windows) pad trailing spaces although it 
              // it is not required for long names.
              // TODO: As per spec, ignore trailing periods
              xName = xLongName.Trim();
            } else {
              string xEntry = xData.GetAsciiString(i, 11);
              xName = xEntry.Substring(0, 8).TrimEnd();
              string xExt = xEntry.Substring(8, 3).TrimEnd();
              if (xExt.Length > 0) {
                xName = xName + "." + xExt;
              }
            }
            var xTest = xAttrib & (Attribs.Directory | Attribs.VolumeID);
            if (xTest == 0) {
              xResult.Add(new Listing.File(xName));
            } else if (xTest == Attribs.VolumeID) {
              //
            } else if (xTest == Attribs.Directory) {
              xResult.Add(new Listing.Directory(xName));
            }
            xLongName = "";
          }
        }
      }

      return xResult;
    }

  }
}
