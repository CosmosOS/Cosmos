using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Hardware2.Network;
using Cosmos.Hardware2.Network.Devices.RTL8139.Register;
using Cosmos.Hardware2.PC.Bus;
using Cosmos.Hardware2;
using Cosmos.Kernel;
using Cosmos.Hardware2.Network.TCPIPModel.PhysicalLayer.Ethernet2;

namespace Cosmos.Hardware2.Network.Devices.RTL8139
{
    /// <summary>
    /// Driver for networkcards using the RTL8139 chip.
    /// Some documentation can be found at: http://www.osdev.org/wiki/RTL8139
    /// </summary>
    public class RTL8139_Old : NetworkDevice {
        protected PCIDevice pciCard;
        protected Kernel.MemoryAddressSpace mem;
        protected Register.ValueTypeRegisters valueReg;
        protected Register.InterruptMaskRegister imr;
        protected Register.InterruptStatusRegister isr;
        protected byte[] TxBuffer0;
        protected byte[] TxBuffer1;
        protected byte[] TxBuffer2;
        protected byte[] TxBuffer3;
        protected byte[] RxBuffer;
        protected ushort RxBufferIdx = 0;
        private const ushort RxBufferSize = 34832;
        //TODO: Remove this later, should also be a prop to be proper but since it 
        // will be removed later...
        public static bool DebugOutput = true;

        // Writing out to the console is slow and often other interrupts come in
        // before its done. So this allows us to turn it on and off
        protected static void DebugWriteLine(string aText) {
            //if (DebugOutput) {
//                Console.WriteLine(aText);
            //}
        }
        protected static void DebugWrite(string aText) {
  //          if (DebugOutput) {
            DebugWriteLine(aText);
//            }
        }

        public RTL8139_Old(PCIDevice device)
        {
            if (device == null) {
                throw new ArgumentException("PCI Device is null. Unable to get RTL8139 card");
            }
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
        public static List<RTL8139_Old> FindAll()
        {
            List<RTL8139_Old> found = new List<RTL8139_Old>();
            foreach (PCIDevice device in Cosmos.Hardware2.PCIBus.Devices) {
                //DebugWriteLine("VendorID: " + device.VendorID + " - DeviceID: " + device.DeviceID);
                if (device.VendorID == 0x10EC && device.DeviceID == 0x8139)
                    found.Add(new RTL8139_Old(device));
            }
            return found;
        }

        #region Power and Initilization

        /// <summary>
        /// Performs additional hardware initilization
        /// </summary>
        public void InitializeDriver()
        {
            mBuffer=new Queue<byte[]>(16);
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
            //Cosmos.Hardware2.Interrupts.IRQ11 += HandleNetworkInterrupt;
            //Interrupts.AddIRQHandler(pciCard.InterruptLine, HandleNetworkInterrupt);

            InitIRQMaskRegister();
            mInstance = this;
            //DebugWriteLine("Listening for IRQ" + pciCard.InterruptLine + ".");

        }

        private static RTL8139_Old mInstance;
        /// <summary>
        /// Initialize the Receive Buffer. The RBSTART register consists of 4 bytes (32-bits at 0x30h to 0x33h) which should contain
        /// the address of a buffer to save incoming data to.
        /// </summary>
        private void InitReceiveBuffer()
        {
            //Prepare a buffer area
            //UInt16 bufferSize = (1024 * 16) + (4 * 4); //last 4  bytes used for CRC
            RxBuffer = new byte[RxBufferSize];
            RxBufferIdx = 3 & (~3);

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

            bool pci_enable = base.Enable(); //enables PCI card as well
            if (pci_enable == true)
            {
                InitializeDriver();
            }

            return pci_enable;
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
                DebugWriteLine("Reset in progress");
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

        private ReceiveConfigurationRegister mRCR;
        public ReceiveConfigurationRegister RCR
        {
            get
            {
                return mRCR;
            }
        }

        public bool PromiscuousMode
        {
            get
            {
                return RCR.PromiscuousMode;
            }
            set
            {
                RCR.PromiscuousMode = value;
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
        public override bool Ready
        {
            get { return this.IsEnabled; }
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
                return mem.Read32((byte)Register.MainRegister.Bit.Timer);
            }
            set
            {
                //Reset timer
                mem.Write32((byte)Register.MainRegister.Bit.Timer, 0);
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
            //outportl(0x44, 0xf | (1 << 7)); // (1 << 7) is the WRAP bit, 0xf is AB+AM+APM+AAP
            mRCR = ReceiveConfigurationRegister.Load(mem);
            cr.RxEnabled = true;
        }

        public Queue<byte[]> mBuffer;

        public byte[] ReadReceiveBuffer()
        {
            List<byte> receivedBytes = new List<byte>();
//DebugUtil.WriteBinary("RTL8139", "RxBuffer", RxBuffer);
            DebugWriteLine("RxBuffer is at address " + GetMemoryAddress(ref RxBuffer));
            DebugWriteLine("Received data from address " + valueReg.CurrentAddressOfPacketRead + " to address " + valueReg.CurrentBufferAddress);

            //The data to be read is in the RxBuffer, but offset by the CBR.

            UInt16 readPointer = valueReg.CurrentAddressOfPacketRead;
            UInt16 writtenPointer = valueReg.CurrentBufferAddress;
            readPointer += 20;
            for (int i = 0; i < writtenPointer; i++) {
                //while (readPointer != writtenPointer)
                //{
                receivedBytes.Add(RxBuffer[readPointer]);

                //if (readPointer == 0xFFF0)
                //    readPointer = 0;
                //else
                readPointer++;
            }
            var xResult = receivedBytes.ToArray();
            if (DataReceived != null)
            {
                DataReceived(xResult);
            }
            //Update the CAPR so that the RTL8139 knows that we've read the data.
            //DebugWriteLine("Setting CAPR to " + readPointer + ". CBR is " + writtenPointer);
            valueReg.CurrentAddressOfPacketRead = (UInt16)(readPointer - 16); //TODO: Figure out if 16 is the correct value. For now it works. RxBufferOverflow is no longer thrown.

            return xResult;
        }

        public override bool QueueBytes(byte[] buffer, int offset, int length)
        {
            TransmitBytes(buffer);

            return true;
        }

        public override bool ReceiveBytes(byte[] buffer, int offset, int max)
        {
            throw new NotImplementedException();
        }

        public override byte[] ReceivePacket()
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

        /// <summary>
        /// Transmits the byte array out onto the network wire.
        /// </summary>
        /// <param name="aData"></param>
        /// <returns></returns>
        public bool TransmitBytes(byte[] aData)
        {
            //TODO: Do NOT set all registers! This works, but is not efficient!
            TxBuffer0 = aData;
            TxBuffer1 = aData;
            TxBuffer2 = aData;
            TxBuffer3 = aData;
            WriteAddressToPCI(ref TxBuffer0, (byte)Register.MainRegister.Bit.TSAD0);
            WriteAddressToPCI(ref TxBuffer1, (byte)Register.MainRegister.Bit.TSAD1);
            WriteAddressToPCI(ref TxBuffer2, (byte)Register.MainRegister.Bit.TSAD2);
            WriteAddressToPCI(ref TxBuffer3, (byte)Register.MainRegister.Bit.TSAD3);

            var tsd = Register.TransmitStatusDescriptor.Load(mem);
            //DebugWriteLine("Telling NIC to send " + aData.Length + " bytes.");
            tsd.Size = aData.Length;
            tsd.OWN = false; //Begins sending - causes QEMU to DIE (Frode, 29.july)!
            
            Register.TransmitStatusDescriptor.IncrementTSDescriptor();
            return true;
        }

        /// <summary>
        /// Converts the frame into a byte array, and sends it using TransmitBytes.
        /// </summary>
        /// <param name="frame"></param>
        /// <returns></returns>
        public bool TransmitFrame(Ethernet2Frame frame)
        {
            return this.TransmitBytes(frame.RawBytes());
        }

        #endregion
        
        #region Interrupt (IRQ)

        private void HandleReceiveInterrupt() {
            //var xStatus = isr.ISR;
            mem.Write32((uint)MainRegister.Bit.Cfg9346,
                        0xc0);
            Console.WriteLine("RTL8139 Interrupt Recvd");
            while ((mem.Read8((uint)MainRegister.Bit.ChipCmd) & 0x01) == 0) {
                //DebugUtil.WriteBinary("RTL8139",
                //                      "Full RxBuffer",
                //                      RxBuffer);
                // iterate while buffer is not empty
                uint xStatus = BitConverter.ToUInt32(RxBuffer,
                                                     RxBufferIdx);
                uint xLen = xStatus >> 16;
                if (xLen == 0xFFF0) {
                    break;
                }
                if (xLen == 0) {
                    break;
                }
                /* check for:
                 * * Invalid Symbol error (100b-tx) = 0x20
                 * * runt packet = 0x10
                 * * long packet (>4k) = 0x8
                 * * CRC error = 0x4
                 * * frame alignment error = 0x2
                 */
                if ((xStatus & 0x3E) != 0) {
                    // handle error    
                    mem.Write8((uint)MainRegister.Bit.ChipCmd, 0x4); // only TX enabled
                    // set up rx mode/configuration
                    RCR.RCR = (UInt32)(ReceiveConfigurationRegister.BitValue.RBLEN0 | ReceiveConfigurationRegister.BitValue.MXDMA0 | ReceiveConfigurationRegister.BitValue.MXDMA1 | ReceiveConfigurationRegister.BitValue.AB | ReceiveConfigurationRegister.BitValue.AM | ReceiveConfigurationRegister.BitValue.APM);

                    RxBufferIdx = valueReg.CurrentBufferAddress;
                    valueReg.CurrentAddressOfPacketRead = (ushort)RxBufferIdx;

                    mem.Write8((uint)MainRegister.Bit.ChipCmd, 0xC); // both TX and RX enabled

                    RCR.RCR = (UInt32)(ReceiveConfigurationRegister.BitValue.RBLEN0 | ReceiveConfigurationRegister.BitValue.MXDMA0 | ReceiveConfigurationRegister.BitValue.MXDMA1 | ReceiveConfigurationRegister.BitValue.AB | ReceiveConfigurationRegister.BitValue.AM | ReceiveConfigurationRegister.BitValue.APM);

                    // Enable interrupts
                    imr.IMR = 0x7F;
                    break;
                } else {
                    xLen -= 4;
                    RxBufferIdx += 4;
                    var xBuff = new byte[xLen];
                    for (uint i = 0; i < xLen; i++) {
                        xBuff[i] = RxBuffer[i + RxBufferIdx];
                    }
                    mBuffer.Enqueue(xBuff);
                    if (DataReceived != null)
                    {
                        DataReceived(xBuff);
                    }

                    RxBufferIdx += (ushort)((xLen + 4 + 3) & 0xFFFFFFFC);
                    if (RxBufferIdx > RxBufferSize) {
                        RxBufferIdx -= RxBufferSize;
                    }
                }
                valueReg.CurrentAddressOfPacketRead = (ushort)(RxBufferIdx - 16);
//                break;
            }
            mem.Write32((uint)MainRegister.Bit.Cfg9346,
                        0x0);
        }

        /// <summary>
        /// (Should be) Called when the PCI network card raises an Interrupt.
        /// </summary>
        //public static void HandleNetworkInterrupt(ref IRQContext aContext)
        //{
        //    if (mInstance.isr.ReceiveOK)
        //    {
        //        DebugWriteLine("IRQ detected: Receive OK");
        //        mInstance.HandleReceiveInterrupt();
        //    }

        //    if (mInstance.isr.ReceiveError)
        //        DebugWriteLine("IRQ detected: Receive ERROR");

        //    if (mInstance.isr.TransmitOK)
        //        DebugWriteLine("IRQ detected: Transmit OK");
            
        //    if (mInstance.imr.TransmitError & mInstance.isr.TransmitError)
        //        DebugWriteLine("IRQ detected: Transmit Error");

        //    if (mInstance.imr.RxBufferOverflow & mInstance.isr.RxBufferOverflow)
        //        DebugWriteLine("IRQ detected: RxBufferOverflow");

        //    if (mInstance.imr.RxFifoOverflow & mInstance.isr.RxFifoOverflow)
        //        DebugWriteLine("IRQ detected: RxFIFOOverflow");

        //    if (mInstance.imr.CableLengthChange & mInstance.isr.CableLengthChange)
        //        DebugWriteLine("IRQ detected: Cable Length Change");

        //    if (mInstance.imr.PacketUnderrun & mInstance.isr.PacketUnderrun)
        //        DebugWriteLine("IRQ detected: Packet Underrun");

        //    if (mInstance.imr.SoftwareInterrupt & mInstance.isr.SoftwareInterrupt)
        //        DebugWriteLine("IRQ detected: Software Interrupt");

        //    if (mInstance.imr.TxDescriptorUnavailable & mInstance.isr.TxDescriptorUnavailable)
        //        DebugWriteLine("IRQ detected: TxDescriptorUnavailable");

        //    if (mInstance.imr.SystemError & mInstance.isr.SystemError)
        //        DebugWriteLine("IRQ detected: System Error!");


        //    mInstance.ResetAllIRQ();
        //    //Console.ReadLine();

        //}

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
            imr.IMR = 0x7F;
            //imr.IMR = 0xFFFF; //Listen for all IRQ events
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

            DebugWriteLine("Tx enabled?: " + cr.TxEnabled.ToString());
            DebugWriteLine("Rx enabled?: " + cr.RxEnabled.ToString());
            DebugWriteLine("Speed 10Mb?: " + msr.Speed10MB.ToString());
            DebugWriteLine("Link OK?: " + (!msr.LinkStatusInverse).ToString());
            DebugWriteLine("CBR (byte count): " + valueReg.CurrentBufferAddress.ToString());
            DebugWriteLine("IMR: " + imr.ToString());
            DebugWriteLine("ISR: " + isr.ToString());
        }

        public void DumpRegisters()
        {
            DebugWriteLine("Command Register: " + Register.CommandRegister.Load(mem).ToString());
            DebugWriteLine("Config1 Register: " + Register.ConfigurationRegister1.Load(mem).ToString());
            DebugWriteLine("Media S Register: " + Register.MediaStatusRegister.Load(mem).ToString());
            DebugWriteLine("Interrupt Mask R: " + Register.InterruptMaskRegister.Load(mem).ToString());
            DebugWriteLine("Interrupt Status: " + Register.InterruptStatusRegister.Load(mem).ToString());
            DebugWriteLine("Rx Configuration: " + Register.ReceiveConfigurationRegister.Load(mem).ToString());
            DebugWriteLine("Tx Configuration: " + Register.TransmitConfigurationRegister.Load(mem).ToString());
            DebugWriteLine("Tx Status Descr.: " + Register.TransmitStatusDescriptor.Load(mem).ToString());
            DebugWriteLine("Tx Start Address: " + valueReg.TransmitStartAddress.ToString());
            DebugWriteLine("Current Descrip.: " + Register.TransmitStatusDescriptor.GetCurrentTSDescriptor().ToString());
        }


        //Just for testing
        public void DisplayReadBuffer()
        {
            //byte[] readData = this.ReadReceiveBuffer();

            //DebugWriteLine("Read buffer contains " + readData.Length + " bytes.");
            //DebugWriteLine("---------------------------------");
            
            //foreach (byte b in readData)
            //    DebugWrite(b.ToHex() + ":");
            //DebugWriteLine("");
            
            //DebugWriteLine("---------------------------------");
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

            //UInt32 address = pciCard.BaseAddress1 + (byte)Register.MainRegister.Bit.RxEarlyCnt;
            //var xMem = new Kernel.MemoryAddressSpace(address, 1);
            //xMem.Write8(0, (byte)bytecount);

            mem.Write8((byte)Register.MainRegister.Bit.RxEarlyCnt, (byte)bytecount);
        }

        /// <summary>
        /// Takes a byte array, and a memory address. 
        /// The memoryaddress of the begining of the bytearray is written to the memory address.
        /// </summary>
        /// <param name="bytearray"></param>
        /// <param name="address"></param>
        private void WriteAddressToPCI(ref byte[] bytearray, byte addressOffset)
        {

            /* The data in the bytearray contains the actual bytes we want to transfer to the network.
             * This bytearray must be in a continous memoryarea on the computer.
             * We then write the address of this memoryarea to the network card.
             * The address is stored in the Transmit Start Address which corresponds to the Transmit Status Descriptor we are currently using (0-3).
             */

            //var xMem = new Kernel.MemoryAddressSpace(address, 1);
            //xMem.Write32(0, GetMemoryAddress(ref bytearray));

            mem.Write32(addressOffset, GetMemoryAddress(ref bytearray));
        }

        /// <summary>
        /// Get the 32-bit address where the bytearray is stored.
        /// </summary>
        private unsafe UInt32 GetMemoryAddress(ref byte[] bytearray)
        {
            fixed (byte* bodystart = bytearray)
            {
                return (UInt32)bodystart;
            }
        }

        #endregion

    }
}
