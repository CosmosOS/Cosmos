using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Cosmos.Common.Extensions;

namespace Cosmos.Hardware {
  public class ATA {
    // Rename this class to AtaPio later - we need this class as a fallback for debugging/boot/debugstub in the future even when
    // we create DMA and other method support
    protected Core.IOGroup.ATA IO;

    public ATA(Core.IOGroup.ATA aIO) {
      IO = aIO;
      // Disable IRQs, we use polling currently
      IO.Control.Byte = 0x02;
    }

    [Flags] public enum Status : byte {None = 0x00, Busy = 0x80, ATA_SR_DRD = 0x40, ATA_SR_DF = 0x20, ATA_SR_DSC = 0x10, DRQ = 0x08, ATA_SR_COR = 0x04, ATA_SR_IDX = 0x02, Error = 0x01 };
    [Flags] enum Error : byte { ATA_ER_BBK = 0x80, ATA_ER_UNC = 0x40, ATA_ER_MC = 0x20, ATA_ER_IDNF = 0x10, ATA_ER_MCR = 0x08, ATA_ER_ABRT = 0x04, ATA_ER_TK0NF = 0x02, ATA_ER_AMNF = 0x01 };
    [Flags]
    enum DvcSelVal : byte {
      // Bits 0-3: Head Number for CHS.
      // Bit 4: Slave Bit. (0: Selecting Master Drive, 1: Selecting Slave Drive).
      Slave = 0x10,
      //* Bit 6: LBA (0: CHS, 1: LBA).
      LBA = 0x40,
      //* Bit 5: Obsolete and isn't used, but should be set.
      //* Bit 7: Obsolete and isn't used, but should be set. 
      Default = 0xA0
    };
    public enum Cmd : byte { ReadPio = 0x20, ReadPioExt = 0x24, ReadDma = 0xC8, ReadDmaExt = 0x25, WritePio = 0x30, WritePioExt = 0x34, WriteDma = 0xCA, WriteDmaExt = 0x35, CacheFlush = 0xE7,
      CacheFlushExt = 0xEA, Packet = 0xA0, IdentifyPacket = 0xA1, Identify = 0xEC, Read = 0xA8, Eject = 0x1B }
    public enum Ident : byte { DEVICETYPE = 0, CYLINDERS = 2, HEADS = 6, SECTORS = 12, SERIAL = 20, MODEL = 54, CAPABILITIES = 98, FIELDVALID = 106, MAX_LBA = 120, COMMANDSETS = 164, MAX_LBA_EXT = 200 }
    public enum SpecLevel { ATA = 0, ATAPI = 1 }

    // Directions:
    //#define      ATA_READ      0x00
    //#define      ATA_WRITE     0x01

    // ATA requires a wait of 400 nanoseconds.
    // Read the Status register FIVE TIMES, and only pay attention to the value 
    // returned by the last one -- after selecting a new master or slave device. The point being that 
    // you can assume an IO port read takes approximately 100ns, so doing the first four creates a 400ns 
    // delay -- which allows the drive time to push the correct voltages onto the bus. 
    // Since we read status again later, we wait by reading it 4 times.
    protected void Wait() {
      // Wait 400 ns
      byte xVoid;
      xVoid = IO.Status.Byte;
      xVoid = IO.Status.Byte;
      xVoid = IO.Status.Byte;
      xVoid = IO.Status.Byte;
    }

    public void SelectDrive(bool aSlave) {
      SelectDrive(aSlave, 0);
    }
    public void SelectDrive(bool aSlave, byte aLbaHigh4) {
      IO.DeviceSelect.Byte = (byte)((byte)(DvcSelVal.Default | DvcSelVal.LBA | (aSlave ? DvcSelVal.Slave : 0)) | aLbaHigh4);
      Wait();
    }

    public Status SendCmd(Cmd aCmd) {
      IO.Command.Byte = (byte)aCmd;
      Status xStatus;
      do {
        Wait();
        xStatus = (Status)IO.Status.Byte;
      } while ((xStatus & Status.Busy) != 0);
      return xStatus;
    }

    protected string GetString(UInt16[] aBuffer, int aIndexStart, int aStringLength) {
      // Would be nice to convert to byte[] and use 
      // new string(ASCIIEncoding.ASCII.GetChars(xBytes));
      // But it requires some code Cosmos doesnt support yet
      var xChars = new char[aStringLength];
      for (int i = 0; i < aStringLength / 2; i++) {
        UInt16 xChar = aBuffer[aIndexStart + i];
        xChars[i * 2] = (char)(xChar >> 8);
        xChars[i * 2 + 1] = (char)xChar;
      }
      return new string(xChars);
    }

    protected void InitDrive(SpecLevel aType) {
      if (aType == SpecLevel.ATA) {
        SendCmd(Cmd.Identify);
      } else {
        SendCmd(Cmd.IdentifyPacket);
      }
      //IDENTIFY command
      // Not sure if all this is needed, its different than documented elsewhere but might not be bad
      // to add code to do all listed here:
      //To use the IDENTIFY command, select a target drive by sending 0xA0 for the master drive, or 0xB0 for the slave, to the "drive select" IO port. On the Primary bus, this would be port 0x1F6. Then set the Sectorcount, LBAlo, LBAmid, and LBAhi IO ports to 0 (port 0x1F2 to 0x1F5). Then send the IDENTIFY command (0xEC) to the Command IO port (0x1F7). Then read the Status port (0x1F7) again. If the value read is 0, the drive does not exist. For any other value: poll the Status port (0x1F7) until bit 7 (BSY, value = 0x80) clears. Because of some ATAPI drives that do not follow spec, at this point you need to check the LBAmid and LBAhi ports (0x1F4 and 0x1F5) to see if they are non-zero. If so, the drive is not ATA, and you should stop polling. Otherwise, continue polling one of the Status ports until bit 3 (DRQ, value = 8) sets, or until bit 0 (ERR, value = 1) sets.
      //At that point, if ERR is clear, the data is ready to read from the Data port (0x1F0). Read 256 words, and store them. 

      // Read Identification Space of the Device
      var xBuff = new UInt16[256];
      IO.Data.Read16(xBuff);
      string xSerialNo = GetString(xBuff, 10, 20);
      string xFirmwareRev = GetString(xBuff, 23, 8);
      string xModelNo = GetString(xBuff, 27, 40);

      //Words (61:60) shall contain the value one greater than the total number of user-addressable
      //sectors in 28-bit addressing and shall not exceed 0FFFFFFFh.  The content of words (61:60) shall
      //be greater than or equal to one and less than or equal to 268,435,455.
      // Sectors are 512 bytes
      UInt32 xSectors28 = (UInt32)(xBuff[61] << 16 | xBuff[60]);

      //Words (103:100) shall contain the value one greater than the total number of user-addressable
      //sectors in 48-bit addressing and shall not exceed 0000FFFFFFFFFFFFh.
      //The contents of words (61:60) and (103:100) shall not be used to determine if 48-bit addressing is
      //supported. IDENTIFY DEVICE bit 10 word 83 indicates support for 48-bit addressing.
      UInt32 xSectors48 = 0;
      bool xLba48Capable = (xBuff[83] & 0x400) != 0;
      if (xLba48Capable) {
        xSectors48 = (UInt32)(xBuff[102] << 32 | xBuff[101] << 16 | xBuff[100]);
      }

      Global.Dbg.Send("--------------------------");
      Global.Dbg.Send("Type: " + (aType == SpecLevel.ATA ? "ATA" : "ATAPI"));
      Global.Dbg.Send("Serial No: " + xSerialNo);
      Global.Dbg.Send("Firmware Rev: " + xFirmwareRev);
      Global.Dbg.Send("Model No: " + xModelNo);
      Global.Dbg.Send("Disk Size 28 (MB): " + xSectors28 * 512 / 1024 / 1024);
      if (xLba48Capable) {
        Global.Dbg.Send("48 bit LBA): yes");
        Global.Dbg.Send("Disk Size 48 (MB): " + xSectors48 * 512 / 1024 / 1024);
      }
    }

    public void ReadSector(bool aSlave, UInt64 aSectorNo, byte[] aData) {
      SelectDrive(aSlave, (byte)(aSectorNo >> 24));
      // Number of sectors to read
      IO.SectorCount.Byte = 1;
      IO.LBA0.Byte = (byte)(aSectorNo & 0xFF);
      IO.LBA1.Byte = (byte)((aSectorNo & 0xFF00) >> 8);
      IO.LBA2.Byte = (byte)((aSectorNo & 0xFF0000) >> 16);
      SendCmd(Cmd.ReadPio);
      //TODO: Update SendCmd to look for error bit
      IO.Data.Read16(aData);
    }

    public void Test() {
      var xType = SpecLevel.ATA;

      int xCount = 0;
      for (int xDrive = 0; xDrive <= 1; xDrive++) {
        SelectDrive(xDrive == 1);
        var xIdentifyStatus = SendCmd(Cmd.Identify);
        // No drive found, go to next
        if (xIdentifyStatus == Status.None) {
          continue;
        } else if ((xIdentifyStatus & Status.Error) != 0) {
          // Can look in Error port for more info
          // Device is not ATA
          // This is also triggered by ATAPI devices
          int xTypeId = IO.LBA2.Byte << 8 | IO.LBA1.Byte;
          if (xTypeId == 0xEB14 || xTypeId == 0x9669) {
            xType = SpecLevel.ATAPI;
          } else {
            // Unknown type. Might not be a device.
            continue;
          }
        } else if ((xIdentifyStatus & Status.DRQ) == 0) {
          // Error
          continue;
        }

        InitDrive(xType);
        //var xData = new byte[512];
        //ReadSector(xDrive == 1, 0, xData);
        //var xSB = new StringBuilder();
        //for (int i = 0; i < 256; i++) {
        //  xSB.Append(xData[i].ToHex());
        // }
        //Console.WriteLine(xSB.ToString());
        xCount++;
      }
    }

  }
}
