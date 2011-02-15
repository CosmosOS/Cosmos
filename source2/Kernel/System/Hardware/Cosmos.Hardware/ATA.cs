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
    public enum Ident : byte { DEVICETYPE = 0, CYLINDERS = 2, HEADS = 6, SECTORS = 12, SERIAL = 20, MODEL = 54, CAPABILITIES = 98, 
      FIELDVALID = 106, MAX_LBA = 120, COMMANDSETS = 164, MAX_LBA_EXT = 200 }
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

        // Read Identification Space of the Device
        var xBuff = new uint[128];
        IO.Data.Read(xBuff);

        // Read Device Parameters:
        Global.Dbg.Send("--------------------------");
        Global.Dbg.Send("Drive #: " + xDrive + ", " + (xType == SpecLevel.ATA ? "ATA" : "ATAPI"));
        //      ide_devices[count].Signature    = ((unsigned short *)(ide_buf + ATA_IDENT_DEVICETYPE));
        //      ide_devices[count].Capabilities = ((unsigned short *)(ide_buf + ATA_IDENT_CAPABILITIES));
        //      ide_devices[count].CommandSets  = ((unsigned int *)(ide_buf + ATA_IDENT_COMMANDSETS));

        //      (VII) Get Size:
        //      if (ide_devices[count].CommandSets & (1 << 26))
        //         // Device uses 48-Bit Addressing:
        //         ide_devices[count].Size   = ((unsigned int *)(ide_buf + ATA_IDENT_MAX_LBA_EXT));
        //      else
        //         // Device uses CHS or 28-bit Addressing:
        //         ide_devices[count].Size   = ((unsigned int *)(ide_buf + ATA_IDENT_MAX_LBA));

        //      (VIII) String indicates model of device (like Western Digital HDD and SONY DVD-RW...):
        //      for(k = 0; k < 40; k += 2) {
        //         ide_devices[count].Model[k] = ide_buf[ATA_IDENT_MODEL + k + 1];
        //         ide_devices[count].Model[k + 1] = ide_buf[ATA_IDENT_MODEL + k];}
        //      ide_devices[count].Model[40] = 0; // Terminate String.

        xCount++;
      }
    }

  }
}
