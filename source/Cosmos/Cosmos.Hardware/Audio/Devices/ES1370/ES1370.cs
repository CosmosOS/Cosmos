using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Hardware;
using Cosmos.Hardware.Audio.Devices.ES1370.Register;
namespace Cosmos.Hardware.Audio.Devices.ES1370
{
    /// <summary>
    /// Driver for the soundcard Ensoniq 1370 AudioPCI (testing for QEMU audio emulation)
    /// It should work also in Ensoniq 1371 all revision.
    /// </summary>
    public class ES1370
    {
        #region Construction

        private PCIDevice pciCard;
        private Cosmos.Kernel.MemoryAddressSpace mem;

        public ES1370(PCIDevice device)
        {
            if (device == null)
                throw new ArgumentException("PCI Device is null. Unable to get ES1370 card");

            pciCard = device;
            mem = device.GetAddressSpace(1) as Cosmos.Kernel.MemoryAddressSpace;
        }
        #endregion

        /// <summary>
        /// Retrieve all Ensoniq AudioPCI 1370 cards found on computer.
        /// </summary>
        /// <returns></returns>
        public static List<ES1370> FindAll()
        {
            List<ES1370> found = new List<ES1370>();

            foreach (PCIDevice device in Cosmos.Hardware.PCIBus.Devices)
            {
                Console.WriteLine("VendorID: " + device.VendorID + " - DeviceID: " + device.DeviceID);
                if (device.VendorID == 0x10EC && device.DeviceID == 0x8139)
                    found.Add(new ES1370(device));
            }

            return found;
        }
        #region Power and Initilization
        public void InitializeDriver()
        {
            //Enable IRQ Interrupt
            InitIRQMaskRegister();
            Cosmos.Hardware.Interrupts.IRQ05 = new Cosmos.Hardware.Interrupts.InterruptDelegate(this.HandleNetworkInterrupt);
        }
        
        public bool Enable()
        {
            var control = ControlRegister.Load(mem);
            control.PowerEnabled=true;
            return control.PowerEnabled;
        }

        public bool Disable()
        {
            var control = ControlRegister.Load(mem);
            control.PowerEnabled = false;
            return control.PowerEnabled;
        }
        #endregion

        public string Name
        {
            get { return "Generic ES1370 Audio device"; }
        }


        public PCIDevice PCICard { get { return pciCard; } private set { ;} }
        #region Interrupt (IRQ)
        /// <summary>
        /// (Should be) Called when the PCI audio card raises an Interrupt.
        /// </summary>
        public void HandleNetworkInterrupt()
        {
            Console.Write("IRQ detected: ");
            /*
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
            */
            this.ResetAllIRQ();

        }

        private void ResetAllIRQ()
        {
            //Setting a bit to 1 will reset it. So we write 16 one's to reset entire ISR.
            //isr.ISR = isr.ISR;
        }

        /// <summary>
        /// The IRQMaskRegister determines what kind of events which cause IRQ to be raised.
        /// </summary>
        private void InitIRQMaskRegister()
        {
            //Note; The reference driver from Realtek sets mask = 0x7F (all bits high).
            //var imr = Register.InterruptMaskRegister.Load(mem);
            //imr.IMR = 0x7F;
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
            //var cr = Register.CommandRegister.Load(mem);
            //var msr = Register.MediaStatusRegister.Load(mem);

            

            //Console.WriteLine("IMR: " + imr.ToString());
            //Console.WriteLine("ISR: " + isr.ToString());
        }

        public void DumpRegisters()
        {
            Console.WriteLine("Control Register: " + Register.ControlRegister.Load(mem).ToString());
            Console.WriteLine("Status Register: " + Register.InterruptStatusRegister.Load(mem).ToString());
        }
        #endregion

    }
}
