using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Hardware.Network;
using Cosmos.Hardware.PC.Bus;
using Cosmos.Hardware;

namespace Cosmos.Hardware.Network.Devices.RTL8139
{
    /// <summary>
    /// Driver for networkcards using the RTL8139 chip.
    /// Some documentation can be found at: http://www.osdev.org/wiki/RTL8139
    /// </summary>
    public class RTL8139 : NetworkDevice //, DeviceDriver interface
    {
        /// <summary>
        /// Retrieve all Realtek 8139 network cards found on computer.
        /// </summary>
        /// <returns></returns>
        public static List<RTL8139> FindAll()
        {
            List<RTL8139> found = new List<RTL8139>();

            foreach (PCIDevice device in Cosmos.Hardware.PC.Bus.PCIBus.Devices)
            {
                //Console.WriteLine("VendorID: " + device.VendorID + " - DeviceID: " + device.DeviceID);
                if (device.VendorID == 0x10EC && device.DeviceID == 0x8139)
                    found.Add(new RTL8139(device));
            }

            return found;
        }

        private PCIDevice pciCard;
        private MemoryAddressSpace mem;
        private Register.MainRegister reg;
        private Register.ValueTypeRegisters valueReg;
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
            mem = device.GetAddressSpace(1) as MemoryAddressSpace;
            reg = new Register.MainRegister(mem);
            valueReg = Register.ValueTypeRegisters.Load(mem);
        }

        public PCIDevice PCICard { get { return pciCard; } private set { ;} }

        #region NetworkDevice members
        public override MACAddress MACAddress
        {
            get 
            {
                return valueReg.Mac;
                //return reg.Mac; 
            }
        }

        /// <summary>
        /// Returns a text with the hardware revision model. F.instance RTL8139C+ or RTL8139.
        /// </summary>
        public string HardwareRevision
        {
            get {
                var tcr = Register.TransmitConfigurationRegister.Load(mem);
                return Register.TransmitConfigurationRegister.GetHardwareRevision(tcr.GetHWVERID());
            }
            private set { ;}
        }

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
            SetIRQMaskRegister();
            Console.WriteLine("Listening for IRQ" + pciCard.InterruptLine + " for incoming data...");
            Cosmos.Hardware.PC.Interrupts.IRQ11 = new Cosmos.Hardware.PC.Interrupts.InterruptDelegate(HandleNetworkInterrupt);
        }

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

        #endregion

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

        public override bool IsSendBufferFull()
        {
            throw new NotImplementedException();
        }

        public override bool IsReceiveBufferFull()
        {
            throw new NotImplementedException();
        }
        #endregion

        public bool IsReceiveBufferEmpty()
        {
            var cr = Register.CommandRegister.Load(mem);
            return cr.RxBufferEmpty;
        }

        public override string Name
        {
            get { return "Generic RTL8139 Network device"; }
        }

        #region Power and Initilization

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

        #region Receive and Interrupt


        /// <summary>
        /// (Should be) Called when the PCI network card raises an Interrupt.
        /// </summary>
        public static void HandleNetworkInterrupt()
        {
            Console.WriteLine("Network IRQ raised! Indicates data received...");
        }

        /// <summary>
        /// The IRQMaskRegister
        /// </summary>
        private void SetIRQMaskRegister()
        {
            //Note; The reference driver from Realtek sets mask = 0x7F (all bits high).
            //reg.IntrMask = (Register.CommandRegister.BitValue)
            //    (
            //    Register.InterruptMaskRegister.Bit.ROK & 
            //    Register.InterruptMaskRegister.Bit.TOK & 
            //    Register.InterruptMaskRegister.Bit.RER & 
            //    Register.InterruptMaskRegister.Bit.TER
            //    );

            var imr = Register.InterruptMaskRegister.Load(mem);
            imr.ReceiveOK = true;
            imr.ReceiveError = true;
            imr.TransmitOK = true;
            imr.TransmitError = true;
            imr.CableLengthChange = true;
            imr.SystemError = true;
            imr.TimeOut = true;
        }

        /// <summary>
        /// This register indicates the source of an interrupt when the INTA pin goes active. 
        /// Enabling the corresponding bits in the Interrupt Mask Register (IMR) allows bits in this register to produce an interrupt. 
        /// When an interrupt is active, one of more bits in this register are set to a “1”. 
        /// The interrupt Status Register reflects all current pending interrupts, regardless of the state of the corresponding mask bit in the IMR. 
        /// Reading the ISR clears all interrupts. Writing to the ISR has no effect.
        /// </summary>
        private void GetIRQServiceRegister()
        {
            //Could perhaps be used to raise events?
            throw new NotImplementedException();
        }

        #endregion

        /// <summary>
        /// Enable the NIC to be able to Receive data.
        /// </summary>
        private void EnableReceive()
        {
            var cr = Register.CommandRegister.Load(mem);
            cr.RxEnabled = true;
        }

        /// <summary>
        /// Enable the NIC to be able to Transmit data.
        /// </summary>
        private void EnableTransmit()
        {
            var cr = Register.CommandRegister.Load(mem);
            cr.TxEnabled = true;
        }

        public void DisplayDebugInfo()
        {
            var cr = Register.CommandRegister.Load(mem);
            var msr = Register.MediaStatusRegister.Load(mem);
            var imr = Register.InterruptMaskRegister.Load(mem);

            Console.WriteLine("Tx enabled?: " + cr.TxEnabled.ToString());
            Console.WriteLine("Rx enabled?: " + cr.RxEnabled.ToString());
            //Console.WriteLine("RxBufAddr: " + reg.RxBufAddr.ToString());
            Console.WriteLine("RxBufPtr: " + reg.RxBufPtr.ToString());
            Console.WriteLine("Speed 10Mb?: " + msr.Speed10MB.ToString());
            Console.WriteLine("Link OK?: " + (!msr.LinkStatusInverse).ToString());
            Console.WriteLine("CBR (byte count): " + valueReg.CurrentBufferPointer.ToString());
            Console.WriteLine("IMR: " + imr.ToString());


        }

        public void DumpRegisters()
        {
            Console.WriteLine("Command Register: " + Register.CommandRegister.Load(mem).ToString());
            Console.WriteLine("Config1 Register: " + Register.ConfigurationRegister1.Load(mem).ToString());
            Console.WriteLine("Media S Register: " + Register.MediaStatusRegister.Load(mem).ToString());
        }


        /// <summary>
        /// A general purpose timer. Writing to this will reset timer. NB: Timer does not work in Qemu.
        /// </summary>
        public UInt32 TimerCount 
        {
            get 
            {
                return IOSpace.Read32(pciCard.BaseAddress1 + (byte)Register.MainRegister.Bit.Timer);
            } 
            set
            {
                UInt32 address = pciCard.BaseAddress1 + (byte)Register.MainRegister.Bit.Timer;
                IOSpace.Write32(address, 0x00); //Resets timer
            }
        }

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
            IOSpace.Write8(address, (byte)bytecount);
        }

        /// <summary>
        /// Initialize the Receive Buffer. The RBSTART register consists of 4 bytes (32-bits at 0x30h to 0x33h) which should contain
        /// the address of a buffer to save incoming data to.
        /// </summary>
        private void InitReceiveBuffer()
        {
            //Prepare a buffer area
            RxBuffer = new byte[50];

            UInt32 address = pciCard.BaseAddress1 + (byte)Register.MainRegister.Bit.RxBuf;

            //Write the address of the buffer area to the RBSTART 
            WriteAddressToPCI(ref RxBuffer, address);
        }

        public byte[] ReadReceiveBuffer()
        {
            return RxBuffer;
        }


        private void InitTransmitBuffer()
        {
            //Initialize Tx Buffers

            TxBuffer0 = new byte[2048];
            TxBuffer1 = new byte[2048];
            TxBuffer2 = new byte[2048];
            TxBuffer3 = new byte[2048];

            WriteAddressToPCI(ref TxBuffer0, pciCard.BaseAddress1 + (byte)Register.MainRegister.Bit.TSAD0);
            WriteAddressToPCI(ref TxBuffer1, pciCard.BaseAddress1 + (byte)Register.MainRegister.Bit.TSAD1);
            WriteAddressToPCI(ref TxBuffer2, pciCard.BaseAddress1 + (byte)Register.MainRegister.Bit.TSAD2);
            WriteAddressToPCI(ref TxBuffer3, pciCard.BaseAddress1 + (byte)Register.MainRegister.Bit.TSAD3);
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

            IOSpace.Write32(address, GetMemoryAddress(ref bytearray));
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

        /// <summary>
        /// Transmits the given Packet
        /// </summary>
        /// <param name="packet"></param>
        [Obsolete]
        public bool Transmit(Packet packet)
        {
            if (packet == null)
                return false;
            //Put the packet into the correct TxBuffer
            
            //TODO: Do NOT set all registers! This works, but is not efficient!
            TxBuffer0 = packet.PacketBody;
            TxBuffer1 = packet.PacketBody;
            TxBuffer2 = packet.PacketBody;
            TxBuffer3 = packet.PacketBody;
            WriteAddressToPCI(ref TxBuffer0, pciCard.BaseAddress1 + (byte)Register.MainRegister.Bit.TSAD0);
            WriteAddressToPCI(ref TxBuffer1, pciCard.BaseAddress1 + (byte)Register.MainRegister.Bit.TSAD1);
            WriteAddressToPCI(ref TxBuffer2, pciCard.BaseAddress1 + (byte)Register.MainRegister.Bit.TSAD2);
            WriteAddressToPCI(ref TxBuffer3, pciCard.BaseAddress1 + (byte)Register.MainRegister.Bit.TSAD3);

            var tsd = Register.TransmitStatusDescriptor.Load(pciCard);
            tsd.Size = packet.PacketBody.Length;
            Console.WriteLine("Told NIC to send " + tsd.Size + " bytes.");
            tsd.OWN = false; //Begins sending
            Register.TransmitStatusDescriptor.IncrementTSDescriptor();

            return true;
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

            var tsd = Register.TransmitStatusDescriptor.Load(pciCard);
            tsd.Size = aData.Length;
            Console.WriteLine("Told NIC to send " + tsd.Size + " bytes.");
            tsd.OWN = false; //Begins sending
            Register.TransmitStatusDescriptor.IncrementTSDescriptor();

            return true;
        }
        
        [Obsolete]
        public bool TransmitRaw(byte[] aData) {
            WriteAddressToPCI(ref aData, pciCard.BaseAddress1 + (byte)Register.MainRegister.Bit.TSAD0);

            //Set the transmit status - which enables the transmit.
            var tsd = Register.TransmitStatusDescriptor.Load(pciCard);
            tsd.Size = aData.Length;
            Console.WriteLine("Told NIC to send " + tsd.Size + " bytes.");

            SetEarlyTxThreshold(1024);
            Console.WriteLine("Sending packet...");
            tsd.OWN = false;
            Register.TransmitStatusDescriptor.IncrementTSDescriptor();

            return true;
        }
    }
}
