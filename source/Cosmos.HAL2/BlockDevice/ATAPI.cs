using Cosmos.Core;
using Cosmos.HAL.BlockDevice;
using System;
using System.Collections.Generic;
using static Cosmos.HAL.BlockDevice.Ata;
using static Cosmos.HAL.BlockDevice.AtaPio;

namespace Cosmos.HALs
{
    public class ATAPI : BlockDevice
    {
        public static List<BlockDevice> ATAPIDevices = new List<BlockDevice>();
        protected static Core.IOGroup.ATA IO;
        public const ushort SectorSize = 2048;
        private bool Primary { get; set; }
        private BusPositionEnum BusPosition { get; set; }
        private AtaPio device;

        public class PacketCommands
        {
            public static byte[] ReadSector = { (byte)AtaPio.Cmd.Read, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0 };
            public static byte[] Unload = { (byte)AtaPio.Cmd.Eject, 0, 0, 0, 0x02, 0, 0, 0, 0, 0, 0, 0 };
        }

        public ATAPI(AtaPio parentDevice)
        {
            device = parentDevice;
            this.BusPosition = parentDevice.BusPosition;
            this.Primary = parentDevice.ControllerID == ControllerIdEnum.Primary;

            mBlockSize = SectorSize;
            IO = new Core.IOGroup.ATA(!Primary);
            var p = BusPosition == BusPositionEnum.Master;
            Ata.AtaDebugger.Send("ATAPI: Primary controller: " + this.Primary + " Bus postion: IsMaster: " + p);

            Devices.Add(this);
            ATAPIDevices.Add(this);

            Init();
        }


        public void Init()
        {
            if (Primary)
            {
                INTs.SetIrqHandler(0x0E, HandleIRQ);
            }
            else
            {
                INTs.SetIrqHandler(0x0F, HandleIRQ);
            }

            IO.Control.Byte = 0; //Enable IRQs
        }
        private void HandleIRQ(ref INTs.IRQContext aContext)
        {
            Ata.AtaDebugger.Send("ATAPI IRQ");
        }

        public override void ReadBlock(ulong SectorNum, ulong SectorCount, ref byte[] aData)
        {
            if (SectorCount != 1)
            {
                throw new NotImplementedException("Reading more than one sectors is not supported. SectorCount: " + SectorCount);
            }

            Ata.AtaDebugger.Send("ATAPI: Reading block. Sector: " + SectorNum + " SectorCount: " + SectorCount);


            byte[] packet = new byte[12];
            packet[0] = (byte)AtaPio.Cmd.Read;//Read Sector
            packet[1] = 0;
            packet[2] = (byte)((SectorNum >> 24) & 0xFF);//MSB
            packet[3] = (byte)((SectorNum >> 16) & 0xFF);
            packet[4] = (byte)((SectorNum >> 8) & 0xFF);
            packet[5] = (byte)((SectorNum >> 0) & 0xFF);//LSB
            packet[6] = 0;
            packet[7] = 0;
            packet[8] = 0;
            packet[9] = (byte)SectorCount;//Sector Count
            packet[10] = 0;
            packet[11] = 0;

            var buffer = new ushort[SectorSize / 2]; //Divide by 2 because we are reading words
            SendCmd(packet, SectorSize, ref buffer);

            CheckForErrors();

            //Poll
            Poll(false);

            //Convert the buffer into bytes
            byte[] array = new byte[SectorSize];
            int counter = 0;
            for (int i = 0; i < (SectorSize / 2); i++)
            {
                var item = buffer[i];
                var bytes = BitConverter.GetBytes(item);

                array[counter++] = bytes[0];
                array[counter++] = bytes[1];
            }

            //Return
            aData = array;
        }

        public override void WriteBlock(ulong aBlockNo, ulong aBlockCount, ref byte[] aData)
        {
            throw new NotImplementedException();
        }

        private void SendCmd(byte[] AtapiPacket, int size, ref ushort[] outputData)
        {
            //Select the ATAPI device
            IO.DeviceSelect.Byte = (byte)((byte)(DvcSelVal.Default | DvcSelVal.LBA | (BusPosition == BusPositionEnum.Slave ? DvcSelVal.Slave : 0)) | 0);

            //Wait for the select complete
            IO.Wait();

            IO.Features.Byte = 0x00; //PIO Mode

            //Set commmand size
            IO.LBA1.Byte = (byte)((SectorSize) & 0xFF);
            IO.LBA2.Byte = (byte)((SectorSize >> 8) & 0xFF);

            //Send ATAPI packet command
            device.SendCmd(Cmd.Packet);
            Ata.AtaDebugger.Send("ATAPI: Polling");

            Poll(true);
            Ata.AtaDebugger.Send("ATAPI: Polling complete");

            //Send the command as words
            for (int i = 0; i < AtapiPacket.Length; i++)
            {
                var b1 = AtapiPacket[i];
                var b2 = AtapiPacket[i + 1];

                var toSend = BitConverter.ToUInt16(new byte[] { b1, b2 }, 0);
                IO.Data.Word = toSend;

                i++;
            }
            Poll(true);
            CheckForErrors();
            var a = (IO.LBA2.Byte << 8);

            var size2 = a | IO.LBA1.Byte;
            if (size != size2)
            {
                throw new Exception("[ATAPI] Packet size mismatch. Expected: " + size + " but got " + size2);
            }

            outputData = new ushort[size2 / 2];
            if (size2 != 0)
            {
                IO.Data.Read16(outputData);
            }
        }

        public void Eject()
        {
            ushort[] output = new ushort[12];
            SendCmd(ATAPI.PacketCommands.Unload, SectorSize, ref output);
        }

        private void Poll(bool checkDRQ = true)
        {
            // 400ns until BSR is set
            IO.Wait();

            while (true)
            {
                // Wait for the ATAPI Device to no longer be busy...
                if (checkDRQ)
                {
                    if ((IO.Status.Byte & (byte)AtaPio.Status.Busy) == 0 && (IO.Status.Byte & (byte)AtaPio.Status.DRQ) != 0)
                    {
                        break;
                    }
                }
                else
                {
                    if ((IO.Status.Byte & (byte)AtaPio.Status.Busy) == 0)
                    {
                        break;
                    }
                }
            }
        }

        private void CheckForErrors()
        {
            if ((IO.Status.Byte & (byte)AtaPio.Status.Error) != 0)
            {
                throw new Exception("ATA Error occured!");
            }

            if ((IO.Status.Byte & (byte)AtaPio.Status.ATA_SR_DF) != 0)
            {
                throw new Exception("ATA device fault encountered!");
            }
            if ((IO.Status.Byte & (byte)AtaPio.Status.DRQ) == 0)
            {
                //throw new Exception("ATAPI DRQ not set");
            }
        }
    }
}
