using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Core;

namespace Cosmos.HAL.BlockDevice
{
    public class ATAPI : BlockDevice
    {
        /*
         *  Capacity is worked out using the following:
         *  Capacity = (Last LBA + 1) * Block Size (virtually always 2KB)
         *
         */
        protected static Core.IOGroup.ATA IO;
        private const UInt16 SectorSize = 2048;
        private const UInt32 LBA = 0;
        public static UInt16[] DataBuffer = new UInt16[256];
        public static bool IRQReceived = false;
        private byte[] ReadToc = { 0x43, 0, 1, 0, 0, 0, 0, 0, 12, 0x40, 0, 0 };

        public static void Init()
        {
            Core.INTs.SetIrqHandler(0x0F, HandleIRQ);
            IO.DeviceSelect.Byte = 0xA0;
            IO.Control.Byte = 0x00;
        }

        public static void Test()
        {
            ReadBlock(1);
            PrintBuffer();
        }

        public static void HandleIRQ(ref Cosmos.Core.INTs.IRQContext aContext)
        {
            Console.WriteLine("IRQ15");
            IRQReceived = true;
        }

        public static void PrintBuffer()
        {
            for (int p = 0; p < 256; p++)
            {
                if (p != 255)
                {
                    Console.Write(DataBuffer[p].ToString() + ",");
                }
                else
                {
                    Console.WriteLine(DataBuffer[p].ToString());
                }
            }
        }

        public static void ReadBlock(uint lba)
        {
            IRQReceived = false;
            IO.DeviceSelect.Byte = (0xA0 | (1 << 4));
            //Write8(0x1F6, (0xA0 | (1 << 4)));
            /*  The error IOPort is readonly
                as to why it's being set here, I've no idea.
                Leave commented out
            */
            // IO.Error.Byte = 0;

            //Write8(0x1F1, 0);//Not DMA

            IO.LBA1.Byte = (byte)(SectorSize & 0xFF);
            //Write8(0x1F4, (byte)(SectorSize & 0xff));
            IO.LBA2.Byte = (byte)(SectorSize >> 8);
            //Write8(0x1F5, (byte)(SectorSize >> 8));
            IO.Command.Byte = 0xA0;
            //Write8(0x1F7, 0xA0);

            byte status = 0;
            bool ReadIsOk = false;
            IO.Wait();
            status = IO.Status.Byte;
            //status = Read8(0x1F7);
            if ((status & 0xFF) != 0x1)
            {
                ReadIsOk = true;
            }
            if (ReadIsOk == true)
            {
                Console.WriteLine("Ok to Transfer!");
            }
            else
            {
                Console.WriteLine("TimeOut!");
            }

            IO.Data.Byte = 0xA8; // Read sector
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


            while (IRQReceived == false) { }

            for (int i = 0; i < 256; i++)
            {
                DataBuffer[i] = IO.Data.Word;
                //DataBuffer[i] = Read16(0x1F0);
            }
            IRQReceived = false;
        }

        public override void ReadBlock(ulong aBlockNo, ulong aBlockCount, byte[] aData)
        {
            IRQReceived = false;
            IO.DeviceSelect.Byte = (0xA0 | (1 << 4));
            
            /*  The error IOPort is readonly
                as to why it's being set here, I've no idea.
                Leave commented out
            */
            // IO.Error.Byte = 0;
            //Write8(0x1F1, 0);//Not DMA

            IO.LBA1.Byte = (byte)(SectorSize & 0xFF);
            //Write8(0x1F4, (byte)(SectorSize & 0xff));
            IO.LBA2.Byte = (byte)(SectorSize >> 8);
            //Write8(0x1F5, (byte)(SectorSize >> 8));
            IO.Command.Byte = 0xA0;
            //Write8(0x1F7, 0xA0);

            byte status = 0;
            bool ReadIsOk = false;
            IO.Wait();
            status = IO.Status.Byte;
            //status = Read8(0x1F7);
            if ((status & 0xFF) != 0x1)
            {
                ReadIsOk = true;
            }
            if (ReadIsOk == true)
            {
                Console.WriteLine("Ok to Transfer!");
            }
            else
            {
                Console.WriteLine("TimeOut!");
            }

            IO.Data.Byte = 0xA8; // Read sector
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


            while (IRQReceived == false) { }

            for (int i = 0; i < 256; i++)
            {
                DataBuffer[i] = IO.Data.Word;
                //DataBuffer[i] = Read16(0x1F0);
            }
            IRQReceived = false;

        }

        public override void WriteBlock(ulong aBlockNo, ulong aBlockCount, byte[] aData)
        {
            throw new NotImplementedException();
        }
    }
}
