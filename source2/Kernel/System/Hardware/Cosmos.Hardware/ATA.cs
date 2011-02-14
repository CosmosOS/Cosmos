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

    protected void Write(byte channel, byte reg, byte data) {
      //if (reg > 0x07 && reg < 0x0C)
      //   ide_write(channel, ATA_REG_CONTROL, 0x80 | channels[channel].nIEN);
      //if (reg < 0x08)
      //   outb(data, channels[channel].base  + reg - 0x00);
      //else if (reg < 0x0C)
      //   outb(data, channels[channel].base  + reg - 0x06);
      //else if (reg < 0x0E)
      //   outb(data, channels[channel].ctrl  + reg - 0x0A);
      //else if (reg < 0x16)
      //   outb(data, channels[channel].bmide + reg - 0x0E);
      //if (reg > 0x07 && reg < 0x0C)
      //   ide_write(channel, ATA_REG_CONTROL, channels[channel].nIEN);
    }

    [Flags]
    enum Status : byte {
      None = 0x00,
      Busy = 0x80,
      ATA_SR_DRD = 0x40,
      ATA_SR_DF = 0x20,
      ATA_SR_DSC = 0x10,
      DRQ = 0x08,
      ATA_SR_COR = 0x04,
      ATA_SR_IDX = 0x02,
      Error = 0x01
    };

    [Flags]
    enum Error : byte {
      ATA_ER_BBK = 0x80,
      ATA_ER_UNC = 0x40,
      ATA_ER_MC = 0x20,
      ATA_ER_IDNF = 0x10,
      ATA_ER_MCR = 0x08,
      ATA_ER_ABRT = 0x04,
      ATA_ER_TK0NF = 0x02,
      ATA_ER_AMNF = 0x01
    };

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

    enum Cmd : byte {
      ReadPio = 0x20,
      ReadPioExt = 0x24,
      ReadDma = 0xC8,
      ReadDmaExt = 0x25,
      WritePio = 0x30,
      WritePioExt = 0x34,
      WriteDma = 0xCA,
      WriteDmaExt = 0x35,
      CacheFlush = 0xE7,
      CacheFlushExt = 0xEA,
      Packet = 0xA0,
      IdentifyPacket = 0xA1,
      Identify = 0xEC,
      Read = 0xA8,
      Eject = 0x1B
    }

    enum Ident : byte {
      ATA_IDENT_DEVICETYPE = 0,
      ATA_IDENT_CYLINDERS = 2,
      ATA_IDENT_HEADS = 6,
      ATA_IDENT_SECTORS = 12,
      ATA_IDENT_SERIAL = 20,
      ATA_IDENT_MODEL = 54,
      ATA_IDENT_CAPABILITIES = 98,
      ATA_IDENT_FIELDVALID = 106,
      ATA_IDENT_MAX_LBA = 120,
      ATA_IDENT_COMMANDSETS = 164,
      ATA_IDENT_MAX_LBA_EXT = 200
    }

    public enum SpecLevel {
      ATA = 0,
      ATAPI = 1
    }

    //#define ATA_MASTER     0x00
    //#define ATA_SLAVE      0x01

    // Directions:
    //#define      ATA_READ      0x00
    //#define      ATA_WRITE     0x01

    public void Test() {
      // Disable IRQs:
      IO.Control.Byte = 0x02;

      int xCount = 0;
      for (int xDrive = 0; xDrive <= 1; xDrive++) {
        byte xErr = 0;

        //      ide_devices[count].Reserved = 0; // Assuming that no drive here.

        // Select Drive
        IO.DeviceSelect.Byte = (byte)(DvcSelVal.Default | DvcSelVal.LBA | (xDrive == 1 ? DvcSelVal.Slave : 0));
        // Wait 1ms for drive select to work.
        //Thread.Sleep(1);

        // Send ATA Identify Command
        IO.Command.Byte = (byte)Cmd.Identify;
        //Thread.Sleep(1);

        // Polling
        // No drive found
        if (IO.Status.Byte == 0) {
          continue;
        }
        while (true) {
          Status xStatus;
          xStatus = (Status)IO.Status.Byte;
          if ((xStatus & Status.Error) == Status.None) {
            // Device is not ATA
            xErr = 1;
            break;
          } else if ((xStatus & Status.Busy) == Status.None && (xStatus & Status.DRQ) != Status.None) {
            // Found drive and its ok
            break;
          }
        }

        // Look for ATAPI devices
        var xType = SpecLevel.ATA;
        if (xErr == 0) {
          byte xCL = IO.LBA1.Byte;
          byte xCH = IO.LBA2.Byte;
          if (xCL == 0x14 && xCH == 0xEB) {
            xType = SpecLevel.ATAPI;
          } else if (xCL == 0x69 && xCH == 0x96) {
            xType = SpecLevel.ATAPI;
          } else {
            // Unknown type. Might not be a device.
            continue;
          }
          IO.Command.Byte = (byte)Cmd.IdentifyPacket;
          //Thread.Sleep(1);
        }

        // Read Identification Space of the Device
        var xBuff = new uint[128];
        IO.Data.Read(xBuff);

        // Read Device Parameters:
        Global.Dbg.Send("--------------------------");
        Global.Dbg.Send("Drive Found");
        Global.Dbg.Send("Type: " + xType);
        if (xType == SpecLevel.ATA) {
          Global.Dbg.Send("Type: ATA");
        } else {
          Global.Dbg.Send("Type: ATAPI");
        }
        Global.Dbg.Send("Drive #: " + xDrive);
        //      ide_devices[count].Signature    = ((unsigned short *)(ide_buf + ATA_IDENT_DEVICETYPE));
        //      ide_devices[count].Capabilities = ((unsigned short *)(ide_buf + ATA_IDENT_CAPABILITIES));
        //      ide_devices[count].CommandSets  = ((unsigned int *)(ide_buf + ATA_IDENT_COMMANDSETS));

        //      // (VII) Get Size:
        //      if (ide_devices[count].CommandSets & (1 << 26))
        //         // Device uses 48-Bit Addressing:
        //         ide_devices[count].Size   = ((unsigned int *)(ide_buf + ATA_IDENT_MAX_LBA_EXT));
        //      else
        //         // Device uses CHS or 28-bit Addressing:
        //         ide_devices[count].Size   = ((unsigned int *)(ide_buf + ATA_IDENT_MAX_LBA));

        //      // (VIII) String indicates model of device (like Western Digital HDD and SONY DVD-RW...):
        //      for(k = 0; k < 40; k += 2) {
        //         ide_devices[count].Model[k] = ide_buf[ATA_IDENT_MODEL + k + 1];
        //         ide_devices[count].Model[k + 1] = ide_buf[ATA_IDENT_MODEL + k];}
        //      ide_devices[count].Model[40] = 0; // Terminate String.
        Global.Dbg.Send("--------------------------");

        xCount++;
      }
    }

  }
}
