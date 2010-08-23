using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        [FlagsAttribute]
        enum Status : byte {
            ATA_SR_BSY = 0x80,
            ATA_SR_DRD = 0x40,
            ATA_SR_DF = 0x20,
            ATA_SR_DSC = 0x10,
            ATA_SR_DRQ = 0x08,
            ATA_SR_COR = 0x04,
            ATA_SR_IDX = 0x02,
            ATA_SR_ERR = 0x01
        };

        [FlagsAttribute]
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

        enum Cmd : byte {
            ATA_CMD_READ_PIO = 0x20,
            ATA_CMD_READ_PIO_EXT = 0x24,
            ATA_CMD_READ_DMA = 0xC8,
            ATA_CMD_READ_DMA_EXT = 0x25,
            ATA_CMD_WRITE_PIO = 0x30,
            ATA_CMD_WRITE_PIO_EXT = 0x34,
            ATA_CMD_WRITE_DMA = 0xCA,
            ATA_CMD_WRITE_DMA_EXT = 0x35,
            ATA_CMD_CACHE_FLUSH = 0xE7,
            ATA_CMD_CACHE_FLUSH_EXT = 0xEA,
            ATA_CMD_PACKET = 0xA0,
            ATA_CMD_IDENTIFY_PACKET = 0xA1,
            ATA_CMD_IDENTIFY = 0xEC,
            ATAPI_CMD_READ = 0xA8,
            ATAPI_CMD_EJECT = 0x1B
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

        //#define IDE_ATA        0x00
        //#define IDE_ATAPI      0x01

        //#define ATA_MASTER     0x00
        //#define ATA_SLAVE      0x01

        // Channels:
        //#define      ATA_PRIMARY      0x00
        //#define      ATA_SECONDARY    0x01

        // Directions:
        //#define      ATA_READ      0x00
        //#define      ATA_WRITE     0x01

        enum Register : byte {
            ATA_REG_DATA = 0x00,
            ATA_REG_ERROR = 0x01,
            ATA_REG_FEATURES = 0x01,
            ATA_REG_SECCOUNT0 = 0x02,
            ATA_REG_LBA0 = 0x03,
            ATA_REG_LBA1 = 0x04,
            ATA_REG_LBA2 = 0x05,
            ATA_REG_HDDEVSEL = 0x06,
            ATA_REG_COMMAND = 0x07,
            ATA_REG_STATUS = 0x07,
            ATA_REG_SECCOUNT1 = 0x08,
            ATA_REG_LBA3 = 0x09,
            ATA_REG_LBA4 = 0x0A,
            ATA_REG_LBA5 = 0x0B,
            ATA_REG_CONTROL = 0x0C,
            ATA_REG_ALTSTATUS = 0x0C,
            ATA_REG_DEVADDRESS = 0x0D
            // 13 regs

            //The map above is the same with the secondary channel, but it uses BAR2 and BAR3 instead of BAR0 and BAR1. 
        }
    }
}
