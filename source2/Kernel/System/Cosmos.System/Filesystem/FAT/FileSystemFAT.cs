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
      Global.Dbg.Send("FAT Sector Count: " + FatSectorCount);

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

    public byte[] NewClusterArray() {
      return new byte[SectorsPerCluster * BytesPerSector];
    }

    public void ReadCluster(UInt32 aCluster, byte[] aData) {
      //TODO:UInt64
      UInt32 xSector = DataSector + ((aCluster - 2) * SectorsPerCluster);
      mDevice.ReadBlock(xSector, SectorsPerCluster, aData);
    }

    // For FAT we should cache FAT tables a bit, however on average
    // FAT tables use 1 MB per 1 GB (160GB disk has 160MB of FAT) of disk space so its not practical to 
    // cache the entire FAT table.
    // One option would be to cache at least the last FAT sector and also cache the FAT entries for a file
    // as its used.
    public UInt32 GetNextCluster(UInt32 aCluster) {
      UInt32 xOffset = 0;
      if (FatType == FatTypeEnum.Fat12) {
        // Multiply by 1.5 without using floating point, the divide by 2 rounds DOWN
        xOffset = aCluster + (aCluster / 2);
      } else if (FatType == FatTypeEnum.Fat16) {
        xOffset = aCluster * 2;
      } else if (FatType == FatTypeEnum.Fat32) {
        xOffset = aCluster * 4;
      }
      UInt32 xFatSecNum = ReservedSectorCount + (xOffset / BytesPerSector);
      UInt32 xFatEntOffset = xOffset % BytesPerSector;

      byte[] xFatTable = mDevice.NewBlockArray(1);
      mDevice.ReadBlock(xFatSecNum, 1, xFatTable);

      UInt32 xResult = 0;
      if (FatType == FatTypeEnum.Fat12) {
        if (xFatEntOffset == (BytesPerSector - 1)) {
          throw new Exception("TODO: Sector Span");
          /* This cluster access spans a sector boundary in the FAT */
          /* There are a number of strategies to handling this. The */
          /* easiest is to always load FAT sectors into memory */
          /* in pairs if the volume is FAT12 (if you want to load */
          /* FAT sector N, you also load FAT sector N+1 immediately */
          /* following it in memory unless sector N is the last FAT */
          /* sector). It is assumed that this is the strategy used here */
          /* which makes this if test for a sector boundary span */
          /* unnecessary. */
        }
        // We now access the FAT entry as a WORD just as we do for FAT16, but if the cluster number i
        // EVEN, we only want the low 12-bits of the 16-bits we fetch. If the cluster number is ODD
        // we want the high 12-bits of the 16-bits we fetch. 
        xResult = xFatTable.ToUInt16(xFatEntOffset);
        if ((aCluster & 0x01) == 0) { // Even
          xResult = xResult & 0x0FFF;
        } else { // Odd
          xResult = xResult >> 4; 
        }
      } else if (FatType == FatTypeEnum.Fat16) {
        xResult = xFatTable.ToUInt16(xFatEntOffset);
      } else if (FatType == FatTypeEnum.Fat32) {
        xResult = xFatTable.ToUInt32(xFatEntOffset) & 0x0FFFFFFF;
      } 

      return xResult;
    }

    public UInt32 FindClusterForPosition(UInt32 aFirstCluster, UInt32 aPosition) {
      return 0;
    }

    public List<Cosmos.System.Filesystem.Listing.Base> GetRoot() {
      var xResult = new List<Cosmos.System.Filesystem.Listing.Base>();

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
      for (UInt32 i = 0; i < xData.Length; i = i + 32) {
        byte xAttrib = xData[i + 11];
        if (xAttrib == Attribs.LongName) {
          byte xType = xData[i + 12];
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
          byte xStatus = xData[i];
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

            UInt32 xFirstCluster = (UInt32)(xData.ToUInt16(i + 20) << 16 | xData.ToUInt16(i + 26));

            var xTest = xAttrib & (Attribs.Directory | Attribs.VolumeID);
            if (xTest == 0) {
              UInt32 xSize = xData.ToUInt32(i + 28);
              xResult.Add(new Listing.File(this, xName, xSize, xFirstCluster));
            } else if (xTest == Attribs.VolumeID) {
              //
            } else if (xTest == Attribs.Directory) {
              xResult.Add(new Listing.Directory(this, xName));
            }
            xLongName = "";
          }
        }
      }

      return xResult;
    }

  }
}
