using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Core;

namespace Cosmos.HAL.BlockDevice
{
    public class ATAPI : BlockDevice
    {
        class PacketCommands
        {
            public byte[] TableOfContents = { 0x43, 0, 1, 0, 0, 0, 0, 0, 12, 0x40, 0, 0 };
            public byte[] ReadSector = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        }

        /*
         *  Capacity is worked out using the following:
         *  Capacity = (Last LBA + 1) * Block Size (virtually always 2KB)
         *
         */

        protected static Core.IOGroup.ATA IO;
        private const UInt16 SectorSize = 2048;
        private const UInt32 LBA = 0;
        private static bool IRQReceived = false;
        public static UInt16[] Buffer = new UInt16[256];

        public static void Init()
        {
            INTs.SetIrqHandler(0x0F, HandleIRQ);
        }

        public static void HandleIRQ(ref INTs.IRQContext aContext)
        {
            IRQReceived = true;
        }

        // Single block size = 2048 bytes for CD/DVD
        public override void ReadBlock(ulong aBlockNo, ulong aBlockCount, byte[] aData)
        {
            IRQReceived = false;
            IO.DeviceSelect.Byte = (0xA0 | (1 << 4));
            IO.LBA1.Byte = (byte)(SectorSize & 0xFF);
            IO.LBA2.Byte = (byte)(SectorSize >> 8);
            IO.Command.Byte = 0xA0;
            byte status = 0;
            bool OKtoRead = false;
            IO.Wait();
            status = IO.Status.Byte;

            if ((status & 0xFF) != 0x1)
            {
                OKtoRead = true;
            }
            if (OKtoRead == true)
            {

            }
            else
            {

            }
            IO.Data.Byte = 0xA8;
            IO.Data.Byte = 0xA8;//Read Sector
            IO.Data.Byte = 0;
            IO.Data.Byte = (byte)((LBA >> 0x18) & 0xFF);//MSB
            IO.Data.Byte = (byte)((LBA >> 0x10) & 0xFF);
            IO.Data.Byte = (byte)((LBA >> 0x08) & 0xFF);
            IO.Data.Byte = (byte)((LBA >> 0x00) & 0xFF);//LSB
            IO.Data.Byte = 0;
            IO.Data.Byte = 0;
            IO.Data.Byte = 0;
            IO.Data.Byte = 1;//Sector Count
            IO.Data.Byte = 0;
            IO.Data.Byte = 0;

            while (IRQReceived == false)
            {

            }

            for (int i = 0; i < 256; i++)
            {
                Buffer[i] = IO.Data.Word;
            }
            IRQReceived = false;
        }

        /*
        public void ReadSector()
        {
            // 0xA8 is READ SECTORS command byte.
            uint[] read_cmd = new uint[12]  { 0xA8, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            uint status;
            int size;
            IO.Wait();
            IO.
        }
        */

        public override void WriteBlock(ulong aBlockNo, ulong aBlockCount, byte[] aData)
        {
            throw new NotImplementedException();
        }

        public static void Eject()
        {

        }
    }
}
