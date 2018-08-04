using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Core;

namespace Cosmos.HAL.BlockDevice
{
    public class ATAPI : BlockDevice
    {
        public static List<BlockDevice> ATAPIDevices = new List<BlockDevice>();

        public class PacketCommands
        {
            public static byte[] TableOfContents = { (byte) ATA_PIO.Cmd.ReadTOC, 0, 1, 0, 0, 0, 0, 0, 12, 0x40, 0, 0 };
            public static byte[] ReadSector = { (byte) ATA_PIO.Cmd.Read, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0 };
            public static byte[] Unload = { (byte)ATA_PIO.Cmd.Eject, 0, 0, 0, 0x02, 0, 0, 0, 0, 0, 0, 0 };
        }

        /*
         *  Capacity is worked out using the following:
         *  Capacity = (Last LBA + 1) * Block Size (virtually always 2KB)
         *
         */

        protected static Core.IOGroup.ATA IO;
        protected byte[] ATAPI_Packet;
        private const UInt16 SectorSize = 2048;
        private const UInt32 LBA = 0;
        private static bool IRQReceived = false;
        public UInt16[] Buffer = new UInt16[256];

        public ATAPI()
        {
            BlockDevice.Devices.Add(this);
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
            //IRQReceived = false;

            //IO.DeviceSelect.Byte = (0xA0 | (1 << 4));
            //IO.Error.Byte = 0;

            //IO.LBA1.Byte = (byte)(SectorSize & 0xFF);
            //IO.LBA2.Byte = (byte)(SectorSize >> 8);
            //IO.Command.Byte = 0xA0;

            //byte status = 0;
            //bool OKtoRead = false;

            //for (int i = 0; i < 500; i++)
            //{
            //    status = IO.Status.Byte;
            //    if ((status & 0xFF) != 0x1)
            //    {
            //        OKtoRead = true;
            //        break;
            //    }
            //}


            //if (OKtoRead == true)
            //{
            //    Console.WriteLine("OK to transfer!");
            //}
            //else
            //{
            //    Console.WriteLine("Timeout!");
            //}

            ATAPI_Packet[0] = (byte) ATA_PIO.Cmd.Read;//Read Sector
            ATAPI_Packet[1] = 0;
            ATAPI_Packet[2] = (byte)((aBlockNo >> 0x18) & 0xFF);//MSB
            ATAPI_Packet[3] = (byte)((aBlockNo >> 0x10) & 0xFF);
            ATAPI_Packet[4] = (byte)((aBlockNo >> 0x08) & 0xFF);
            ATAPI_Packet[5] = (byte)((aBlockNo >> 0x00) & 0xFF);//LSB
            ATAPI_Packet[6] = 0;
            ATAPI_Packet[7] = 0;
            ATAPI_Packet[8] = 0;
            ATAPI_Packet[9] = (byte) (aBlockCount & 0xFF);//Sector Count
            ATAPI_Packet[10] = 0;
            ATAPI_Packet[11] = 0;

            IRQReceived = false;
            IO.Control.Byte = 0x00;
            IO.Features.Byte = 0x00;
            IO.LBA1.Byte = (byte)((SectorSize) & 0xFF);
            IO.LBA2.Byte = (byte)((SectorSize >> 8) & 0xFF);
            SendCmd();
            System.Threading.Thread.Sleep(100);

            for (int i = 0; i < SectorSize; i++)
            {
                Buffer[i] = IO.Data.Word;
            }
            IRQReceived = false;
        }

        public override void WriteBlock(ulong aBlockNo, ulong aBlockCount, byte[] aData)
        {
            throw new NotImplementedException();
        }

        private void SendCmd()
        {
            IO.Command.Byte = (byte)ATA_PIO.Cmd.Packet;
            AdvPoll();
            foreach (byte command in ATAPI_Packet)
            {
                IO.Data.Byte = command;
            }
            IRQClear();
            Poll();
        }

        public void Eject()
        {
            ATAPI_Packet = ATAPI.PacketCommands.Unload;
            IRQReceived = false;
            SendCmd();
        }

        private void Poll()
        {
            // 400ns until BSR is set
            IO.Wait();

            while ((IO.Status.Byte & (byte) ATA_PIO.Status.Busy) != 0)
            {
                // Wait for the ATAPI Device to no longer be busy...
            }

        }

        private void AdvPoll()
        {
            Poll();
            var aState = (ATA_PIO.Status)IO.Status.Byte;
            if ((aState & ATA_PIO.Status.Error) != 0)
            {
                ATA.ATADebugger.Send("ATA Error");
                throw new Exception("ATA Error occured!");
            }

            if ((aState & ATA_PIO.Status.ATA_SR_DF) != 0)
            {
                ATA.ATADebugger.Send("ATA Device Fault");
                throw new Exception("ATA device fault encountered!");
            }

            if ((aState & ATA_PIO.Status.DRQ) != 0)
            {
                ATA.ATADebugger.Send("DRQ not set");
                throw new Exception("ATA DRQ not set!");
            }
        }

        private void IRQClear()
        {
            while (!IRQReceived) ;
            IRQReceived = false;
        }
    }
}
