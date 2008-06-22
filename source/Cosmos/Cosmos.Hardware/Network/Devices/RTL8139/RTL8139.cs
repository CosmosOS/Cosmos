using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Hardware.Network;
using Cosmos.Hardware.PC.Bus;
using Cosmos.Hardware;
using Cosmos.Kernel;

namespace Cosmos.Hardware.Network.Devices.RTL8139
{
    /// <summary>
    /// Driver for networkcards using the RTL8139 chip.
    /// Some documentation can be found at: http://www.osdev.org/wiki/RTL8139
    /// </summary>
    public class RTL8139 : NetworkDevice
    {
        #region Construction

        private PCIDevice pciCard;
        private Kernel.MemoryAddressSpace mem;
        //private Register.MainRegister reg;
        //private Register.CommandRegister cr;
        private Register.ValueTypeRegisters valueReg;
        private Register.InterruptMaskRegister imr;
        private Register.InterruptStatusRegister isr;
        private byte[] TxBuffer0;
        private byte[] TxBuffer1;
        private byte[] TxBuffer2;
        private byte[] TxBuffer3;
        private byte[] RxBuffer;

        public RTL8139(PCIDevice device)
        {
            if (device == null)
                throw new ArgumentException("PCI Device is null. Unable to get RTL8139 card");

            pciCard = device;
            mem = device.GetAddressSpace(1) as Kernel.MemoryAddressSpace;
            valueReg = Register.ValueTypeRegisters.Load(mem);
            imr = Register.InterruptMaskRegister.Load(mem);
            isr = Register.InterruptStatusRegister.Load(mem);
        }

        /// <summary>
        /// Retrieve all Realtek 8139 network cards found on computer.
        /// </summary>
        /// <returns></returns>
        public static List<RTL8139> FindAll()
        {
            List<RTL8139> found = new List<RTL8139>();

            foreach (PCIDevice device in Cosmos.Hardware.PCIBus.Devices)
            {
                //Console.WriteLine("VendorID: " + device.VendorID + " - DeviceID: " + device.DeviceID);
                if (device.VendorID == 0x10EC && device.DeviceID == 0x8139)
                    found.Add(new RTL8139(device));
            }

            return found;
        }

        #endregion

        #region Power and Initilization

        /// <summary>
        /// Performs additional hardware initilization
        /// </summary>
        public void InitializeDriver()
        {
            //Turn on Tx and Rx
            EnableTransmit();
            EnableReceive();

            //Initialize buffers
            InitTransmitBuffer();
            InitReceiveBuffer();

            //Setting Transmit configuration
            var tcr = Register.TransmitConfigurationRegister.Load(mem);
            tcr.Init();
            SetEarlyTxThreshold(1024);
            
            //Setting Receive configuration
            var rcr = Register.ReceiveConfigurationRegister.Load(mem);
            rcr.Init();
            rcr.PromiscuousMode = true;
            
            //Enable IRQ Interrupt
            InitIRQMaskRegister();
            Cosmos.Hardware.Interrupts.IRQ11 = new Cosmos.Hardware.Interrupts.InterruptDelegate(this.HandleNetworkInterrupt);
            //Console.WriteLine("Listening for IRQ" + pciCard.InterruptLine + ".");
        }

        /// <summary>
        /// Initialize the Receive Buffer. The RBSTART register consists of 4 bytes (32-bits at 0x30h to 0x33h) which should contain
        /// the address of a buffer to save incoming data to.
        /// </summary>
        private void InitReceiveBuffer()
        {
            //Prepare a buffer area
            UInt16 bufferSize = (1024 * 16) + (4 * 4); //last 4  bytes used for CRC
            RxBuffer = new byte[bufferSize];

            //Write the address of the buffer area to the RBSTART 
            valueReg.RBSTART = GetMemoryAddress(ref RxBuffer);
        }

        private void InitTransmitBuffer()
        {
            //Initialize Tx Buffers

            TxBuffer0 = new byte[2048];
            TxBuffer1 = new byte[2048];
            TxBuffer2 = new byte[2048];
            TxBuffer3 = new byte[2048];

            valueReg.TSAD0 = GetMemoryAddress(ref TxBuffer0);
            valueReg.TSAD1 = GetMemoryAddress(ref TxBuffer1);
            valueReg.TSAD2 = GetMemoryAddress(ref TxBuffer2);
            valueReg.TSAD3 = GetMemoryAddress(ref TxBuffer3);
        }

        /// <summary>
        /// Enables RTL network card by setting CONFIG_1 register.
        /// </summary>
        /// <returns></returns>
        public override bool Enable()
        {
            var config1 = Register.ConfigurationRegister1.Load(mem);
            config1.PowerEnabled = true; //Uncertain if this is needed

            return base.Enable(); //enables PCI card as well
        }

        public override bool Disable()
        {
            var config1 = Register.ConfigurationRegister1.Load(mem);
            config1.PowerEnabled = false;

            return base.Disable();
        }

        /// <summary>
        /// Performs an internal system hardware reset of the network card.
        /// </summary>
        public void SoftReset()
        {
            //Tell RTL chip to issue a Reset`
            var cr = Register.CommandRegister.Load(mem);
            cr.Reset = true;

            //Wait while RST bit is active
            while (cr.Reset)
            {
                Console.WriteLine("Reset in progress");
            }
        }

        #endregion

        #region Operational properties

        /// <summary>
        /// Changes the Loopback mode. 
        /// </summary>
        /// <param name="value">True to enable Loopback. False for normal operation.</param>
        public bool LoopbackMode
        {
            get
            {
                var tcr = Register.TransmitConfigurationRegister.Load(mem);
                return tcr.LoopbackMode;
            }
            set
            {
                var tcr = Register.TransmitConfigurationRegister.Load(mem);
                tcr.LoopbackMode = value;
            }
        }

        public bool PromiscuousMode
        {
            get
            {
                var rcr = Register.ReceiveConfigurationRegister.Load(mem);
                return rcr.PromiscuousMode;
            }
            set
            {
                var rcr = Register.ReceiveConfigurationRegister.Load(mem);
                rcr.PromiscuousMode = value;
            }

        }

        public override string Name
        {
            get { return "Generic RTL8139 Network device"; }
        }

        public override MACAddress MACAddress
        {
            get
            {
                return valueReg.Mac;
            }
        }

        /// <summary>
        /// Returns a text with the hardware revision model. F.instance RTL8139C+ or RTL8139.
        /// </summary>
        public string HardwareRevision
        {
            get
            {
                var tcr = Register.TransmitConfigurationRegister.Load(mem);
                return Register.TransmitConfigurationRegister.GetHardwareRevision(tcr.GetHWVERID());
            }
            private set { ;}
        }

        public PCIDevice PCICard { get { return pciCard; } private set { ;} }


        /// <summary>
        /// A general purpose timer. Writing to this will reset timer. NB: Timer does not work in Qemu.
        /// </summary>
        public UInt32 TimerCount
        {
            get
            {
                var xMem = new Kernel.MemoryAddressSpace(pciCard.BaseAddress1 + (byte)Register.MainRegister.Bit.Timer, 1);
                return xMem.Read32(0);
            }
            set
            {
                UInt32 address = pciCard.BaseAddress1 + (byte)Register.MainRegister.Bit.Timer;
                var xMem = new Kernel.MemoryAddressSpace(address, 1);
                xMem.Write32(0, 0); //Resets timer
            }
        }


        #endregion

        #region Receive data

        /// <summary>
        /// Enable the NIC to be able to Receive data.
        /// </summary>
        private void EnableReceive()
        {
            var cr = Register.CommandRegister.Load(mem);
            cr.RxEnabled = true;
        }

        public byte[] ReadReceiveBuffer()
        {
            List<byte> receivedBytes = new List<byte>();

            Console.WriteLine("RxBuffer is at address " + GetMemoryAddress(ref RxBuffer));
            Console.WriteLine("Received data from address " + valueReg.CurrentAddressOfPacketRead + " to address " + valueReg.CurrentBufferAddress);

            //The data to be read is in the RxBuffer, but offset by the CBR.

            UInt16 readPointer = valueReg.CurrentAddressOfPacketRead;
            UInt16 writtenPointer = valueReg.CurrentBufferAddress;
            while (readPointer != writtenPointer)
            {
                receivedBytes.Add(RxBuffer[readPointer]);

                if (readPointer == 0xFFF0)
                    readPointer = 0;
                else
                    readPointer++;
            }

            //Update the CAPR so that the RTL8139 knows that we've read the data.
            //Console.WriteLine("Setting CAPR to " + readPointer + ". CBR is " + writtenPointer);
            valueReg.CurrentAddressOfPacketRead = (UInt16)(readPointer - 16); //TODO: Figure out if 16 is the correct value. For now it works. RxBufferOverflow is no longer thrown.

            return receivedBytes.ToArray();
        }

        public override bool QueueBytes(byte[] buffer, int offset, int length)
        {
            throw new NotImplementedException();
        }

        public override bool ReceiveBytes(byte[] buffer, int offset, int max)
        {
            throw new NotImplementedException();
        }

        public override int BytesAvailable()
        {
            throw new NotImplementedException();
        }

        public override bool IsReceiveBufferFull()
        {
            throw new NotImplementedException();
        }

        public bool IsReceiveBufferEmpty()
        {
            var cr = Register.CommandRegister.Load(mem);
            return cr.RxBufferEmpty;
        }

        #endregion

        #region Transmit data

        /// <summary>
        /// Enable the NIC to be able to Transmit data.
        /// </summary>
        private void EnableTransmit()
        {
            var cr = Register.CommandRegister.Load(mem);
            cr.TxEnabled = true;
        }

        public override bool IsSendBufferFull()
        {
            throw new NotImplementedException();
        }


        public bool TransmitBytes(byte[] aData)
        {
            //TODO: Do NOT set all registers! This works, but is not efficient!
            TxBuffer0 = aData;
            TxBuffer1 = aData;
            TxBuffer2 = aData;
            TxBuffer3 = aData;
            WriteAddressToPCI(ref TxBuffer0, pciCard.BaseAddress1 + (byte)Register.MainRegister.Bit.TSAD0);
            WriteAddressToPCI(ref TxBuffer1, pciCard.BaseAddress1 + (byte)Register.MainRegister.Bit.TSAD1);
            WriteAddressToPCI(ref TxBuffer2, pciCard.BaseAddress1 + (byte)Register.MainRegister.Bit.TSAD2);
            WriteAddressToPCI(ref TxBuffer3, pciCard.BaseAddress1 + (byte)Register.MainRegister.Bit.TSAD3);

            var tsd = Register.TransmitStatusDescriptor.Load(mem);
            Console.WriteLine("Telling NIC to send " + aData.Length + " bytes.");
            tsd.Size = aData.Length;
            //Console.WriteLine("TransmitStatusDescriptor contains size of" + tsd.Size + " bytes.");
            //Console.WriteLine("TDS : " + tsd.ToString());
            tsd.OWN = false; //Begins sending
            //Console.WriteLine("TDS : " + tsd.ToString());
            Register.TransmitStatusDescriptor.IncrementTSDescriptor();

            return true;
        }

        [Obsolete]
        public bool TransmitRaw(byte[] aData)
        {
            WriteAddressToPCI(ref aData, pciCard.BaseAddress1 + (byte)Register.MainRegister.Bit.TSAD0);

            //Set the transmit status - which enables the transmit.
            var tsd = Register.TransmitStatusDescriptor.Load(mem);
            tsd.Size = aData.Length;
            Console.WriteLine("Told NIC to send " + tsd.Size + " bytes.");

            SetEarlyTxThreshold(1024);
            Console.WriteLine("Sending packet...");
            tsd.OWN = false;
            Register.TransmitStatusDescriptor.IncrementTSDescriptor();

            return true;
        }

        #endregion
        
        #region Interrupt (IRQ)
        
        /// <summary>
        /// (Should be) Called when the PCI network card raises an Interrupt.
        /// </summary>
        public void HandleNetworkInterrupt()
        {
            Console.Write("IRQ detected: ");

            if (imr.ReceiveOK & isr.ReceiveOK)
            {
                Console.WriteLine("Receive OK");
                this.DisplayReadBuffer();
            }

            if (imr.ReceiveError & isr.ReceiveError)
                Console.WriteLine("Receive ERROR");

            if (imr.TransmitOK & isr.TransmitOK)
                Console.WriteLine("Transmit OK");

            if (imr.TransmitError & isr.TransmitError)
                Console.WriteLine("Transmit Error");

            if (imr.RxBufferOverflow & isr.RxBufferOverflow)
                Console.WriteLine("RxBufferOverflow");

            if (imr.RxFifoOverflow & isr.RxFifoOverflow)
                Console.WriteLine("RxFIFOOverflow");

            if (imr.CableLengthChange & isr.CableLengthChange)
                Console.WriteLine("Cable Length Change");

            if (imr.PacketUnderrun & isr.PacketUnderrun)
                Console.WriteLine("Packet Underrun");

            if (imr.SoftwareInterrupt & isr.SoftwareInterrupt)
                Console.WriteLine("Software Interrupt");

            if (imr.TxDescriptorUnavailable & isr.TxDescriptorUnavailable)
                Console.WriteLine("TxDescriptorUnavailable");

            if (imr.SystemError & isr.SystemError)
                Console.WriteLine("System Error!");

            this.ResetAllIRQ();

        }

        private void ResetAllIRQ()
        {
            //Setting a bit to 1 will reset it. So we write 16 one's to reset entire ISR.
            isr.ISR = isr.ISR;
        }

        /// <summary>
        /// The IRQMaskRegister determines what kind of events which cause IRQ to be raised.
        /// </summary>
        private void InitIRQMaskRegister()
        {
            //Note; The reference driver from Realtek sets mask = 0x7F (all bits high).
            var imr = Register.InterruptMaskRegister.Load(mem);
            //imr.IMR = 0x7F;
            imr.IMR = 0xFFFF; //Listen for all IRQ events
/*            imr.ReceiveOK = true;
            imr.ReceiveError = true;
            imr.TransmitOK = true;
            imr.TransmitError = true;
            imr.CableLengthChange = true;
            imr.SystemError = true;
            imr.TimeOut = true;
  */      }

        #endregion

        #region Debugging

        public void DisplayDebugInfo()
        {
            var cr = Register.CommandRegister.Load(mem);
            var msr = Register.MediaStatusRegister.Load(mem);

            Console.WriteLine("Tx enabled?: " + cr.TxEnabled.ToString());
            Console.WriteLine("Rx enabled?: " + cr.RxEnabled.ToString());
            Console.WriteLine("Speed 10Mb?: " + msr.Speed10MB.ToString());
            Console.WriteLine("Link OK?: " + (!msr.LinkStatusInverse).ToString());
            Console.WriteLine("CBR (byte count): " + valueReg.CurrentBufferAddress.ToString());
            Console.WriteLine("IMR: " + imr.ToString());
            Console.WriteLine("ISR: " + isr.ToString());
        }

        public void DumpRegisters()
        {
            Console.WriteLine("Command Register: " + Register.CommandRegister.Load(mem).ToString());
            Console.WriteLine("Config1 Register: " + Register.ConfigurationRegister1.Load(mem).ToString());
            Console.WriteLine("Media S Register: " + Register.MediaStatusRegister.Load(mem).ToString());
            Console.WriteLine("Interrupt Mask R: " + Register.InterruptMaskRegister.Load(mem).ToString());
            Console.WriteLine("Interrupt Status: " + Register.InterruptStatusRegister.Load(mem).ToString());
            Console.WriteLine("Rx Configuration: " + Register.ReceiveConfigurationRegister.Load(mem).ToString());
            Console.WriteLine("Tx Configuration: " + Register.TransmitConfigurationRegister.Load(mem).ToString());
            Console.WriteLine("Tx Status Descr.: " + Register.TransmitStatusDescriptor.Load(mem).ToString());
            Console.WriteLine("Tx Start Address: " + valueReg.TransmitStartAddress.ToString());
            Console.WriteLine("Current Descrip.: " + Register.TransmitStatusDescriptor.GetCurrentTSDescriptor().ToString());
        }


        //Just for testing
        public void DisplayReadBuffer()
        {
            byte[] readData = this.ReadReceiveBuffer();

            Console.WriteLine("Read buffer contains " + readData.Length + " bytes.");
            Console.WriteLine("---------------------------------");
            
            foreach (byte b in readData)
                Console.Write(b.ToHex() + ":");
            Console.WriteLine();
            
            Console.WriteLine("---------------------------------");
        }

        #endregion

        #region Misc
        /// <summary>
        /// The Early TX Threshold specifies the threshold level in Tx FIFO register before transmission begins.
        /// The bytecount should not exceed 2048(2k bytes).
        /// The bytecount also needs to be dividable by 32.
        /// If bytecount 0 is set then NIC will use 8 bytes as threshold
        /// </summary>
        /// <param name="bytecount">Number zero or a number dividable by 32.</param>
        private void SetEarlyTxThreshold(uint bytecount)
        {
            //TODO: This method should be in TransmitStatusDescriptors.cs
            if (bytecount != 0 & (bytecount % 32 > 0))
                throw new ArgumentException("Early TX Threshold must be 0 or dividable by 32");

            //Each of the four Transmit Status Descriptors (TSD) has its own EarlyTxThreshold.

            UInt32 address = pciCard.BaseAddress1 + (byte)Register.MainRegister.Bit.RxEarlyCnt;
            var xMem = new Kernel.MemoryAddressSpace(address, 1);
            xMem.Write8(0, (byte)bytecount);

            //TODO: The code above (with the warning, could probably be replaced with the following line). Need to test.
            //mem.Write8((byte)Register.MainRegister.Bit.RxEarlyCnt, 0);
        }

        /// <summary>
        /// Takes a byte array, and a memory address. 
        /// The memoryaddress of the begining of the bytearray is written to the memory address.
        /// </summary>
        /// <param name="bytearray"></param>
        /// <param name="address"></param>
        private void WriteAddressToPCI(ref byte[] bytearray, uint address)
        {

            /* The data in the bytearray contains the actual bytes we want to transfer to the network.
             * This bytearray must be in a continous memoryarea on the computer.
             * We then write the address of this memoryarea to the network card.
             * The address is stored in the Transmit Start Address which corresponds to the Transmit Status Descriptor we are currently using (0-3).
             */

            var xMem = new Kernel.MemoryAddressSpace(address, 1);
            xMem.Write32(0, GetMemoryAddress(ref bytearray));
        }

        /// <summary>
        /// Get the 32-bit address where the bytearray is stored.
        /// </summary>
        private unsafe UInt32 GetMemoryAddress(ref byte[] bytearray)
        {
            fixed (byte* bodystart = bytearray)
            {
                IntPtr bodyAddress = (IntPtr)bodystart;
                return (UInt32)bodystart;
            }
        }

        #endregion

    }
}
