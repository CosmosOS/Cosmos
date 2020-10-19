using System;
using Cosmos.Core;
using Cosmos.Debug.Kernel;

namespace Cosmos.HAL.BlockDevice
{
    // We need this class as a fallback for debugging/boot/debugstub in the future even when
    // we create DMA and other method support
    //
    // AtaPio is also used as the detection class to detect drives. If another ATA mode is used,
    // the instnace of AtaPio should be discarded in favour of another ATA class. AtaPio is used
    // to do this as capabilities can also be detected, but all ATA devices must support PIO
    // and thus it can also be used to read the partition table and perform other tasks before
    // initializing another ATA class in favour of AtaPio
    public class AtaPio : Ata
    {
        protected Core.IOGroup.ATA IO;

        protected ControllerIdEnum ControllerID { get; }

        protected BusPositionEnum BusPosition { get; }

        public DriveTypeEnum DriveType { get; private set; }

        public string SerialNo { get; private set; }

        public string FirmwareRev { get; private set; }

        public string ModelNo { get; private set; }

        public bool LBA48Bit;

        private static Debugger mDebugger = new Debugger("HAL", "AtaPio");

        private static void Debug(string message)
        {
            AtaDebugger.Send("ATAPIO debug: " + message);
        }

        private static void DebugHex(string message, uint number)
        {
            Debug(message);
            AtaDebugger.SendNumber(number);
        }

        private static void DebugHex(string message, ulong number)
        {
            Debug(message);
            AtaDebugger.SendNumber(number);
        }

        public AtaPio(Core.IOGroup.ATA aIO, ControllerIdEnum aControllerId, BusPositionEnum aBusPosition)
        {
            IO = aIO;

            ControllerID = aControllerId;
            BusPosition = aBusPosition;


            INTs.SetIrqHandler(0x0E, HandlePrimaryIRQ);
            INTs.SetIrqHandler(0x0F, HandleSecondaryIRQ);

            // Disable IRQs, we use polling currently
            IO.Control.Byte = 0x02;

            DriveType = DiscoverDrive();
            if (DriveType != DriveTypeEnum.Null)
            {
                InitDrive();
            }
        }

        private void HandlePrimaryIRQ(ref INTs.IRQContext aContext)
        {
            Debug("IRQ 14");
            byte xStatus = IO.Status.Byte;
        }

        private void HandleSecondaryIRQ(ref INTs.IRQContext aContext)
        {
            Debug("IRQ 15");
            byte xStatus = IO.Status.Byte;
        }

        public DriveTypeEnum DiscoverDrive()
        {
            SelectDrive(0);

            // Read status before sending command. If 0xFF, it's a floating
            // bus (nothing connected)
            if (IO.AlternateStatus.Byte == 0xFF)
            {
                return DriveTypeEnum.Null;
            }

            var xIdentifyStatus = SendCmd(Cmd.Identify, false);
            // No drive found, go to next
            if (xIdentifyStatus == StatusFlags.None)
            {
                return DriveTypeEnum.Null;
            }
            else if ((xIdentifyStatus & StatusFlags.Error) != 0)
            {
                // Can look in Error port for more info
                // Device is not ATA
                // This is also triggered by ATAPI devices
                int xTypeId = IO.LBAHigh.Byte << 8 | IO.LBAMid.Byte;
                if (xTypeId == 0xEB14 || xTypeId == 0x9669)
                {
                    return DriveTypeEnum.ATAPI;
                }
                else
                {
                    // Unknown type. Might not be a device.
                    return DriveTypeEnum.Null;
                }
            }
            else if ((xIdentifyStatus & StatusFlags.DRQ) == 0)
            {
                // Error
                return DriveTypeEnum.Null;
            }
            return DriveTypeEnum.ATA;
        }

        // ATA requires a wait of 400 nanoseconds.
        // Read the Status register FIVE TIMES, and only pay attention to the value
        // returned by the last one -- after selecting a new master or slave device. The point being that
        // you can assume an IO port read takes approximately 100ns, so doing the first four creates a 400ns
        // delay -- which allows the drive time to push the correct voltages onto the bus.
        // Since we read status again later, we wait by reading it 4 times.
        private void Wait()
        {
            // Wait 400 ns
            byte xVoid;
            xVoid = IO.AlternateStatus.Byte;
            xVoid = IO.AlternateStatus.Byte;
            xVoid = IO.AlternateStatus.Byte;
            xVoid = IO.AlternateStatus.Byte;
        }

        private StatusFlags WaitOnBusy()
        {
            StatusFlags xStatus;
            do
            {
                Wait();
                xStatus = (StatusFlags)IO.AlternateStatus.Byte;
            } while ((xStatus & StatusFlags.Busy) != 0);

            return xStatus;
        }

        private StatusFlags WaitOnDrq()
        {
            StatusFlags xStatus;
            do
            {
                xStatus = WaitOnBusy();
            } while ((xStatus & StatusFlags.DRQ) != 0 || (xStatus & StatusFlags.Error) != 0);

            return xStatus;
        }

        private void SelectDrive(byte aLbaHigh4)
        {
            IO.DriveSelect.Byte = (byte)((byte)(DeviceSelectFlags.Default | DeviceSelectFlags.LBA | (BusPosition == BusPositionEnum.Slave ? DeviceSelectFlags.Slave : 0)) | aLbaHigh4);
            Wait();
        }

        private StatusFlags SendCmd(Cmd aCmd, bool aThrowOnError = true)
        {
            IO.Command.Byte = (byte)aCmd;
            StatusFlags xStatus;
            do
            {
                Wait();
                xStatus = (StatusFlags)IO.Status.Byte;
            } while ((xStatus & StatusFlags.Busy) != 0);

            // Error occurred
            if (aThrowOnError && (xStatus & StatusFlags.Error) != 0)
            {
                // TODO: Read error port
                Debug("ATA Error in SendCmd.");
                DebugHex("Cmd", (byte)aCmd);
                DebugHex("Status", (byte)xStatus);
                throw new Exception("ATA Error");
            }
            return xStatus;
        }

        private string GetString(UInt16[] aBuffer, int aIndexStart, int aStringLength)
        {
            // Would be nice to convert to byte[] and use
            // new string(ASCIIEncoding.ASCII.GetChars(xBytes));
            // But it requires some code Cosmos doesnt support yet
            var xChars = new char[aStringLength];
            for (int i = 0; i < aStringLength / 2; i++)
            {
                UInt16 xChar = aBuffer[aIndexStart + i];
                xChars[i * 2] = (char)(xChar >> 8);
                xChars[i * 2 + 1] = (char)xChar;
            }
            return new string(xChars);
        }

        protected void InitDrive()
        {
            if (DriveType == DriveTypeEnum.ATA)
            {
                SendCmd(Cmd.Identify);
            }
            else
            {
                SendCmd(Cmd.IdentifyPacket);
            }
            //IDENTIFY command
            // Not sure if all this is needed, its different than documented elsewhere but might not be bad
            // to add code to do all listed here:
            //
            //To use the IDENTIFY command, select a target drive by sending 0xA0 for the master drive, or 0xB0 for the slave, to the "drive select" IO port. On the Primary bus, this would be port 0x1F6.
            // Then set the Sectorcount, LBAlo, LBAmid, and LBAhi IO ports to 0 (port 0x1F2 to 0x1F5).
            // Then send the IDENTIFY command (0xEC) to the Command IO port (0x1F7).
            // Then read the Status port (0x1F7) again. If the value read is 0, the drive does not exist. For any other value: poll the Status port (0x1F7) until bit 7 (BSY, value = 0x80) clears.
            // Because of some ATAPI drives that do not follow spec, at this point you need to check the LBAmid and LBAhi ports (0x1F4 and 0x1F5) to see if they are non-zero.
            // If so, the drive is not ATA, and you should stop polling. Otherwise, continue polling one of the Status ports until bit 3 (DRQ, value = 8) sets, or until bit 0 (ERR, value = 1) sets.
            // At that point, if ERR is clear, the data is ready to read from the Data port (0x1F0). Read 256 words, and store them.

            // Read Identification Space of the Device
            var xBuff = new UInt16[256];
            IO.Data.Read16(xBuff);
            SerialNo = GetString(xBuff, 10, 20);
            FirmwareRev = GetString(xBuff, 23, 8);
            ModelNo = GetString(xBuff, 27, 40);

            //Words (61:60) shall contain the value one greater than the total number of user-addressable
            //sectors in 28-bit addressing and shall not exceed 0FFFFFFFh.  The content of words (61:60) shall
            //be greater than or equal to one and less than or equal to 268,435,455.
            // We need 28 bit addressing - small drives on VMWare and possibly other cases are 28 bit
            mBlockCount = ((UInt32)xBuff[61] << 16 | xBuff[60]) - 1;

            //Words (103:100) shall contain the value one greater than the total number of user-addressable
            //sectors in 48-bit addressing and shall not exceed 0000FFFFFFFFFFFFh.
            //The contents of words (61:60) and (103:100) shall not be used to determine if 48-bit addressing is
            //supported. IDENTIFY DEVICE bit 10 word 83 indicates support for 48-bit addressing.
            LBA48Bit = (xBuff[83] & 0x40) != 0;
            // LBA48Bit = mBlockCount > 0x0FFFFFFF;
            if (LBA48Bit)
            {
                mBlockCount = ((UInt64)xBuff[102] << 32 | (UInt64)xBuff[101] << 16 | (UInt64)xBuff[100]) - 1;
            }
        }

        protected void SelectSector(UInt64 aSectorNo, UInt64 aSectorCount)
        {
            CheckBlockNo(aSectorNo, aSectorCount);
            //TODO: Check for 48 bit sectorno mode and select 48 bits
            SelectDrive((byte)(aSectorNo >> 24));
            if (LBA48Bit)
            {
                IO.SectorCount.Word = (ushort)aSectorCount;
                IO.LBALow.Byte = (byte)(aSectorNo >> 24);
                IO.LBALow.Byte = (byte)(aSectorNo);
                IO.LBAMid.Byte = (byte)(aSectorNo >> 32);
                IO.LBAMid.Byte = (byte)(aSectorNo >> 8);
                IO.LBAHigh.Byte = (byte)(aSectorNo >> 40);
                IO.LBAHigh.Byte = (byte)(aSectorNo >> 16);
            }
            else
            {
                // Number of sectors to read
                IO.SectorCount.Byte = (byte)aSectorCount;
                IO.LBALow.Byte = (byte)(aSectorNo);
                IO.LBAMid.Byte = (byte)(aSectorNo >> 8);
                IO.LBAHigh.Byte = (byte)(aSectorNo >> 16);
                //IO.LBA0.Byte = (byte)(aSectorNo & 0xFF);
                //IO.LBA1.Byte = (byte)((aSectorNo & 0xFF00) >> 8);
                //IO.LBA2.Byte = (byte)((aSectorNo & 0xFF0000) >> 16);
            }
            //TODO LBA3  ...
        }

        public override void ReadBlock(UInt64 aBlockNo, UInt64 aBlockCount, ref byte[] aData)
        {
            if (DriveType == DriveTypeEnum.ATA)
            {
                CheckDataSize(aData, aBlockCount);
                SelectSector(aBlockNo, aBlockCount);
                SendCmd(LBA48Bit ? Cmd.ReadPioExt : Cmd.ReadPio);

                IO.Data.Read8(aData);
            }
            else if (DriveType == DriveTypeEnum.ATAPI)
            {
                // Selecting the Drive[Master / Slave].
                // Waiting 400ns for select to complete.
                // Setting FEATURES Register to 0[PIO Mode].
                // Setting LBA1 and LBA2 Registers to 0x0008[Number of Bytes will be returned].
                // Sending Packet Command, then Polling.
                // Sending the ATAPI Packet, then polling.ATAPI packet must be 6 words long(12 bytes).
                // If there isn't an error, reading 4 Words [8 bytes] from the DATA Register. 

           Debug("Set max bytes to 2048");
           IO.LBAMid.Byte = 0x08 & 0xFF; // 2048 & 0xFF;
           IO.LBAHigh.Byte = 0x08 >> 0x08; //2048 >> 0x08;
                Debug("Send packet command");
                SendCmd(Cmd.Atapi_Packet);
                byte[] read_cmd = { (byte)Cmd.Atapi_ReadCapacity, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                //read_cmd[9] = 1;              /* 1 sector */
                //read_cmd[2] = (byte)((aBlockNo >> 0x18) & 0xFF);   /* most sig. byte of LBA */
                //read_cmd[3] = (byte)((aBlockNo >> 0x10) & 0xFF);
                //read_cmd[4] = (byte)((aBlockNo >> 0x08) & 0xFF);
                //read_cmd[5] = (byte)((aBlockNo >> 0x00) & 0xFF);   /* least sig. byte of LBA */
                Debug("Write packet data");
                IO.Data.Word = BitConverter.ToUInt16(read_cmd, 0);
                IO.Data.Word = BitConverter.ToUInt16(read_cmd, 2);
                IO.Data.Word = BitConverter.ToUInt16(read_cmd, 4);
                IO.Data.Word = BitConverter.ToUInt16(read_cmd, 6);
                IO.Data.Word = BitConverter.ToUInt16(read_cmd, 8);
                IO.Data.Word = BitConverter.ToUInt16(read_cmd, 10);
                Debug("Wait on DRQ");
                WaitOnDrq();
                Debug("Read data");
                IO.Data.Read8(aData);
                DebugHex("data[0]", aData[0]);
                DebugHex("data[1]", aData[1]);
                DebugHex("data[2]", aData[2]);
                DebugHex("data[3]", aData[3]);
                DebugHex("data[4]", aData[4]);
                DebugHex("data[5]", aData[5]);
                DebugHex("data[6]", aData[6]);
                DebugHex("data[7]", aData[7]);
            }
        }

        public override void WriteBlock(UInt64 aBlockNo, UInt64 aBlockCount, ref byte[] aData)
        {
            CheckDataSize(aData, aBlockCount);
            SelectSector(aBlockNo, aBlockCount);
            SendCmd(LBA48Bit ? Cmd.WritePioExt : Cmd.WritePio);

            UInt16 xValue;

            for (long i = 0; i < aData.Length / 2; i++)
            {
                xValue = (UInt16)((aData[i * 2 + 1] << 8) | aData[i * 2]);
                IO.Data.Word = xValue;
                WaitOnBusy();
                // There must be a tiny delay between each OUTSW output word. A jmp $+2 size of delay.
                // But that delay is cpu specific? so how long of a delay?
            }

            SendCmd(Cmd.CacheFlush);
        }

        public override string ToString()
        {
            return "AtaPio";
        }
    }
}
