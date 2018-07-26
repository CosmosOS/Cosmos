using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Core;

namespace Cosmos.HAL.BlockDevice
{
    public class ATAPI : BlockDevice
    {

        public static List<BlockDevice> ATAPIDevices = new List<BlockDevice>();

        class PacketCommands
        {
            public byte[] TableOfContents = { 0x43, 0, 1, 0, 0, 0, 0, 0, 12, 0x40, 0, 0 };
            public byte[] ReadSector = { 0xA8, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0 };
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
        public UInt16[] Buffer = new UInt16[256];

        public ATAPI()
        {
            ATAPIDevices.Add(this);
            Init();
        }

        public void Init()
        {
            INTs.SetIrqHandler(0x0F, HandleIRQ);
        }

        public void Test()
        {
            Init();
            ReadBlock(65, 0, new byte[] { 0, 0 });
            PrintBuffer();
        }

        public static void HandleIRQ(ref INTs.IRQContext aContext)
        {
            IRQReceived = true;
        }

        public void PrintBuffer()
        {
            for (int p = 0; p < 256; p++)
            {
                if (p != 255)
                {
                    Console.Write(Buffer[p].ToString() + ",");
                }
                else
                {
                    Console.WriteLine(Buffer[p].ToString());
                }
            }
        }

        // Single block size = 2048 bytes for CD/DVD
        public override void ReadBlock(ulong aBlockNo, ulong aBlockCount, byte[] aData)
        {
            IRQReceived = false;

            IO.DeviceSelect.Byte = (0xA0 | (1 << 4));
            IO.Error.Byte = 0;

            IO.LBA1.Byte = (byte)(SectorSize & 0xFF);
            IO.LBA2.Byte = (byte)(SectorSize >> 8);
            IO.Command.Byte = 0xA0;

            byte status = 0;
            bool OKtoRead = false;

            for (int i = 0; i < 500; i++)
            {
                status = IO.Status.Byte;
                if ((status & 0xFF) != 0x1)
                {
                    OKtoRead = true;
                    break;
                }
            }


            if (OKtoRead == true)
            {
                Console.WriteLine("OK to transfer!");
            }
            else
            {
                Console.WriteLine("Timeout!");
            }

            IO.Data.Byte = 0xA8;//Read Sector
            IO.Data.Byte = 0;
            IO.Data.Byte = (byte)((aBlockNo >> 0x18) & 0xFF);//MSB
            IO.Data.Byte = (byte)((aBlockNo >> 0x10) & 0xFF);
            IO.Data.Byte = (byte)((aBlockNo >> 0x08) & 0xFF);
            IO.Data.Byte = (byte)((aBlockNo >> 0x00) & 0xFF);//LSB
            IO.Data.Byte = 0;
            IO.Data.Byte = 0;
            IO.Data.Byte = 0;
            IO.Data.Byte = 1;//Sector Count
            IO.Data.Byte = 0;
            IO.Data.Byte = 0;

            System.Threading.Thread.Sleep(100);

            for (int i = 0; i < 256; i++)
            {
                Buffer[i] = IO.Data.Word;
            }
            IRQReceived = false;
        }

        public override void WriteBlock(ulong aBlockNo, ulong aBlockCount, byte[] aData)
        {
            throw new NotImplementedException();
        }

        public void Unload()
        {

        }
    }
}
