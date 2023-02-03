using Cosmos.Core;
using Cosmos.HAL.BlockDevice;
using System;
using System.Collections.Generic;
using System.IO;
using static Cosmos.HAL.BlockDevice.Ata;
using static Cosmos.HAL.BlockDevice.ATA_PIO;

namespace Cosmos.HAL.BlockDevice
{
    public class ATAPI : BlockDevice
    {
        /// <summary>
        /// CPU IOGroup for all ATA devices, ATAPI uses this group too
        /// </summary>
        protected static Core.IOGroup.ATA IO;

        /// <summary>
        /// Default sector size of all ATAPI drives is 2048
        /// </summary>
        public const ushort SectorSize = 2048;

        /// <summary>
        /// Is the ATAPI drive on the Primary or Secondary channel of the IDE controller.
        /// </summary>
        public bool Primary { get; private set; }

        /// <summary>
        /// Get The max lba
        /// </summary>
        public ulong MaxLBA { get; private set; }

        /// <summary>
        /// Each IDE channel also has a Master or a Slave. This just gets or sets which position it is at.
        /// </summary>
        private BusPositionEnum BusPosition { get; set; }
        public override BlockDeviceType Type => BlockDeviceType.RemovableCD;

        public ATA_PIO device;

        /// <summary>
        /// Collection of predefined command packets to be sent to the ATAPI device
        /// </summary>
        public class PacketCommands
        {
            public static byte[] ReadSector = { (byte)ATA_PIO.Cmd.Read, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0 };
            public static byte[] Unload = { (byte)ATA_PIO.Cmd.Eject, 0, 0, 0, 0x02, 0, 0, 0, 0, 0, 0, 0 };
            public static byte[] GetMaxLBA = { 0x25, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        }

        /// <summary>
        /// Constructor for ATAPI speclevel device.
        /// </summary>
        /// <param name="parentDevice"></param>
        public ATAPI(ATA_PIO parentDevice)
        {
            device = parentDevice;
            this.BusPosition = parentDevice.BusPosition;
            this.Primary = parentDevice.ControllerID == ControllerIdEnum.Primary;

            mBlockSize = SectorSize;
            IO = new Core.IOGroup.ATA(!Primary);
            var p = BusPosition == BusPositionEnum.Master;
            Ata.AtaDebugger.Send("ATAPI: Primary controller: " + this.Primary + " Bus postion: IsMaster: " + p);

            MaxLBA = GetMaxLBA();
            Init();
        }

        /// <summary>
        /// Check if ATAPI-speclevel device is on Primary or Secondary channel,
        /// and register the appropriate interrupt
        /// </summary>
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

            IOPort.Write8(IO.Control, 0); //Enable IRQs
        }

        /// <summary>
        /// Handles the ATAPI IRQ as specified in Init()
        /// </summary>
        /// <param name="aContext"></param>
        private void HandleIRQ(ref INTs.IRQContext aContext)
        {
            Ata.AtaDebugger.Send("ATAPI IRQ");
        }

        /// <summary>
        /// Override method for BlockDevice.ReadBlock, returns the specified buffered sector of ATAPI drive
        /// </summary>
        /// <param name="SectorNum"></param>
        /// <param name="SectorCount"></param>
        /// <param name="aData"></param>
        public override void ReadBlock(ulong SectorNum, ulong SectorCount, ref byte[] aData)
        {
            if (SectorCount != 1)
            {
                throw new NotImplementedException("Reading more than one sectors is not supported. SectorCount: " + SectorCount);
            }

            Ata.AtaDebugger.Send("ATAPI: Reading block. Sector: " + SectorNum + " SectorCount: " + SectorCount);


            byte[] packet = new byte[12];
            packet[0] = (byte)ATA_PIO.Cmd.Read;//Read Sector
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
            for (int i = 0; i < SectorSize / 2; i++)
            {
                var item = buffer[i];
                var bytes = BitConverter.GetBytes(item);

                array[counter++] = bytes[0];
                array[counter++] = bytes[1];
            }

            //Return
            aData = array;
        }

        /// <summary>
        /// ATAPI writing is not currently supported, that's a future milestone,
        /// probably very distant future as we do all development on an existing OS :)
        /// </summary>
        /// <param name="aBlockNo"></param>
        /// <param name="aBlockCount"></param>
        /// <param name="aData"></param>
        public override void WriteBlock(ulong aBlockNo, ulong aBlockCount, ref byte[] aData)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Sends a SCSI packet to the device
        /// </summary>
        /// <param name="AtapiPacket"></param>
        /// <param name="size"></param>
        /// <param name="outputData"></param>
        private void SendCmd(byte[] AtapiPacket, int size, ref ushort[] outputData)
        {
            //Select the ATAPI device
            IOPort.Write8(IO.DeviceSelect, (byte)((byte)(DvcSelVal.Default | DvcSelVal.LBA | (BusPosition == BusPositionEnum.Slave ? DvcSelVal.Slave : 0)) | 0));

            //Wait for the select complete
            IO.Wait();

            IOPort.Write8(IO.Features, 0x00); //PIO Mode

            //Set commmand size
            IOPort.Write8(IO.LBA1, SectorSize & 0xFF);
            IOPort.Write8(IO.LBA2, (SectorSize >> 8) & 0xFF);

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
                IOPort.Write16(IO.Data, toSend);

                i++;
            }
            Poll(true);
            CheckForErrors();
            var a = IOPort.Read8(IO.LBA2) << 8;

            var size2 = a | IOPort.Read8(IO.LBA1);
            if (size != size2)
            {
                throw new Exception("[ATAPI] Packet size mismatch. Expected: " + size + " but got " + size2);
            }

            outputData = new ushort[size2 / 2];
            if (size2 != 0)
            {
                IOPort.Read16(IO.Data, outputData);
            }
        }

        /// <summary>
        /// Ejects the ATAPI drive (optical media, iomega Zip etc.)
        /// </summary>
        public void Eject()
        {
            ushort[] output = new ushort[12];
            SendCmd(ATAPI.PacketCommands.Unload, SectorSize, ref output);
        }

        /// <summary>
        /// Is the ATAPI device still in the middle of an operation? If so, wait!
        /// </summary>
        /// <param name="checkDRQ"></param>
        private void Poll(bool checkDRQ = true)
        {
            // 400ns until BSR is set
            IO.Wait();

            while (true)
            {
                // Wait for the ATAPI Device to no longer be busy...
                if (checkDRQ)
                {
                    if ((IOPort.Read8(IO.Status) & (byte)ATA_PIO.Status.Busy) == 0 && (IOPort.Read8(IO.Status) & (byte)ATA_PIO.Status.DRQ) != 0)
                    {
                        break;
                    }
                }
                else
                {
                    if ((IOPort.Read8(IO.Status) & (byte)ATA_PIO.Status.Busy) == 0)
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Checks the status flag of the ATAPI device.
        /// </summary>
        private void CheckForErrors()
        {
            if ((IOPort.Read8(IO.Status) & (byte)ATA_PIO.Status.Error) != 0)
            {
                throw new Exception("ATA Error occured!");
            }

            if ((IOPort.Read8(IO.Status) & (byte)ATA_PIO.Status.ATA_SR_DF) != 0)
            {
                throw new Exception("ATA device fault encountered!");
            }
            if ((IOPort.Read8(IO.Status) & (byte)ATA_PIO.Status.DRQ) == 0)
            {
                //throw new Exception("ATAPI DRQ not set");
            }
        }
        /// <summary>
        ///  The function returns the max LBA value of the ATAPI device. Code is based on <a href="https://forum.osdev.org/viewtopic.php?f=1&amp;t=14604">this</a>
        /// </summary>
        /// <returns>The maximum LBA</returns>
        private ulong GetMaxLBA()
        {
            //Select the ATAPI device
            IOPort.Write8(IO.DeviceSelect, (byte)(((byte)BusPosition << 4) + (1 << 6)));

            //Wait for the select complete
            IO.Wait();

            // get max lba
            ulong Max_LBA;
            if (device.LBA48Bit)
            {
                device.SendCmd(Cmd.ReadNativeMaxAdressExt,false); // says not check errors

                Max_LBA = IOPort.Read8(IO.LBA0);
                Max_LBA += (ulong)(IOPort.Read8(IO.LBA1) << 8);
                Max_LBA += (ulong)(IOPort.Read8(IO.LBA2) << 16);

                IOPort.Write8(IO.Control, 0x80); // Set HOB to 1

                Max_LBA += (ulong)(IOPort.Read8(IO.LBA0) << 24);
                Max_LBA += (ulong)(IOPort.Read8(IO.LBA1) << 32);
                Max_LBA += (ulong)(IOPort.Read8(IO.LBA2) << 40);
            }
            else
            {
                device.SendCmd(Cmd.ReadNativeMaxAdress,false); // says not check errors

                Max_LBA = IOPort.Read8(IO.LBA0);
                Max_LBA += (ulong)(IOPort.Read8(IO.LBA1) << 8);
                Max_LBA += (ulong)(IOPort.Read8(IO.LBA2) << 16);
                Max_LBA += (ulong)(IOPort.Read8(IO.DeviceSelect) & 0xF) << 24;
            }
            return Max_LBA;
        }
    }
}
