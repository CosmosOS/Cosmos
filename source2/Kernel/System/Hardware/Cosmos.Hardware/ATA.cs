using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Cosmos.Hardware {
  public class ATA {
    protected Core.IOGroup.ATA IO;

    public ATA(Core.IOGroup.ATA aIO) {
      IO = aIO;
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
    public enum DriveSelect { Master = 0x00, Slave = 0x01 }

    // Directions:
    //#define      ATA_READ      0x00
    //#define      ATA_WRITE     0x01

    // ATA often requires a wait of 400 nanoseconds.
    // We cant wait right now, but our code is slow enough that we probably wait that
    // long anyways. But we have this call as a placeholder for when we can
    // actually wait.
    protected void Wait() {
      // Wait 400 ns
    }

    public void SelectDrive(DriveSelect aDrive) {
      IO.DeviceSelect.Byte = (byte)(DvcSelVal.Default | DvcSelVal.LBA | (aDrive == DriveSelect.Slave ? DvcSelVal.Slave : 0));
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

    public void Test() {
      var xType = SpecLevel.ATA;

      // Disable IRQs:
      IO.Control.Byte = 0x02;

      int xCount = 0;
      for (int xDrive = 0; xDrive <= 1; xDrive++) {
        SelectDrive((DriveSelect)xDrive);
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
            // Send a new command which will create a new buffer read
            SendCmd(Cmd.IdentifyPacket);
          } else {
            // Unknown type. Might not be a device.
            continue;
          }
        } else if ((xIdentifyStatus & Status.DRQ) == 0) {
          // Error
          continue;
        }

        //IDENTIFY command
        // Not sure if all this is needed, its different than documented elsewhere but might not be bad
        // to add code to do all listed here:
        //To use the IDENTIFY command, select a target drive by sending 0xA0 for the master drive, or 0xB0 for the slave, to the "drive select" IO port. On the Primary bus, this would be port 0x1F6. Then set the Sectorcount, LBAlo, LBAmid, and LBAhi IO ports to 0 (port 0x1F2 to 0x1F5). Then send the IDENTIFY command (0xEC) to the Command IO port (0x1F7). Then read the Status port (0x1F7) again. If the value read is 0, the drive does not exist. For any other value: poll the Status port (0x1F7) until bit 7 (BSY, value = 0x80) clears. Because of some ATAPI drives that do not follow spec, at this point you need to check the LBAmid and LBAhi ports (0x1F4 and 0x1F5) to see if they are non-zero. If so, the drive is not ATA, and you should stop polling. Otherwise, continue polling one of the Status ports until bit 3 (DRQ, value = 8) sets, or until bit 0 (ERR, value = 1) sets.
        //At that point, if ERR is clear, the data is ready to read from the Data port (0x1F0). Read 256 words, and store them. 

        // Read Identification Space of the Device
        var xBuff = new UInt16[256];
        IO.Data.Read(xBuff);
        string xSerialNo = GetString(xBuff, 10, 20);
        string xFirmwareRev = GetString(xBuff, 23, 8);
        string xModelNo = GetString(xBuff, 27, 40);

        //// (VII) Get Size:
        //if (ide_devices[count].CommandSets & (1 << 26))
        //   // Device uses 48-Bit Addressing:
        //   ide_devices[count].Size   = ((unsigned int *)(ide_buf + ATA_IDENT_MAX_LBA_EXT));
        //else
        //   // Device uses CHS or 28-bit Addressing:
        //   ide_devices[count].Size   = ((unsigned int *)(ide_buf + ATA_IDENT_MAX_LBA));
 
        //// (VIII) String indicates model of device (like Western Digital HDD and SONY DVD-RW...):
        //for(k = 0; k < 40; k += 2) {
        //   ide_devices[count].Model[k] = ide_buf[ATA_IDENT_MODEL + k + 1];
        //   ide_devices[count].Model[k + 1] = ide_buf[ATA_IDENT_MODEL + k];}
        //ide_devices[count].Model[40] = 0; // Terminate String.

        // Read Device Parameters:
        Global.Dbg.Send("--------------------------");
        Global.Dbg.Send("Drive #: " + xDrive + ", " + (xType == SpecLevel.ATA ? "ATA" : "ATAPI"));
        Global.Dbg.Send("Serial No: " + xSerialNo);
        Global.Dbg.Send("Firmware Rev: " + xFirmwareRev);
        Global.Dbg.Send("Model No: " + xModelNo);

        xCount++;
      }
    }

  }
}
