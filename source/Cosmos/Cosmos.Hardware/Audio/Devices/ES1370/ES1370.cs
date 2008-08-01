using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Hardware;
using Cosmos.Hardware.Audio.Devices.ES1370.Registers;
using Cosmos.Hardware.Audio.Managers;
using Cosmos.Hardware.Audio.Components;
namespace Cosmos.Hardware.Audio.Devices.ES1370
{
    /// <summary>
    /// Driver for the soundcard Ensoniq 1370 AudioPCI (testing for QEMU audio emulation)
    /// It should work also in Ensoniq 1371 all revision.
    /// </summary>
    public class ES1370 : GenericSoundCard
    {
        private InterruptStatusRegister isr;
        private ControlRegister cr;
        private SerialInterfaceRegister sir;
        private UARTInterfaceRegister uir;
        public int[] FixedRatesSupported={5512, 11025, 22050, 44100};

        public ES1370(PCIDevice device) : base(device)
        {
            isr=(InterruptStatusRegister.Load(getMemReference()));
            sir = (SerialInterfaceRegister.Load(getMemReference()));
            uir = (UARTInterfaceRegister.Load(getMemReference()));
            cr=(ControlRegister.Load(getMemReference()));
            dacs.Add(new DACManager(new DACEntity((byte)MainRegister.Bit.Dac1FrameAddr, (byte)MainRegister.Bit.Dac1FrameSize)));
            dacs.Add(new DACManager(new DACEntity((byte)MainRegister.Bit.Dac1FrameAddr, (byte)MainRegister.Bit.Dac1FrameSize)));

        }
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

        #region Power Management
        public override bool Enable()
        {
            cr.PowerEnabled=true;
            return cr.PowerEnabled;
        }
        
        public override bool Disable()
        {
            cr.PowerEnabled = false;
            return cr.PowerEnabled;
        }

        public void InitializeDriver()
        {
            Cosmos.Hardware.Interrupts.IRQ05 = new Cosmos.Hardware.Interrupts.InterruptDelegate(this.HandleAudioInterrupt);
        }

        #endregion

        public string Name
        {
            get { return "Generic ES1370 Audio device"; }
        }


        
        #region Interrupt (IRQ)
        /// <summary>
        /// (Should be) Called when the PCI audio card raises an Interrupt.
        /// </summary>
        public void HandleAudioInterrupt(ref Interrupts.InterruptContext aContext)
        {
            Console.Write("IRQ detected: ");
            if (isr.CodecBusyIntEnabled)
                Console.WriteLine("Codec busy Interrupt! ");
            if(isr.CodecStatusIntEnabled)
                Console.WriteLine("Codec Enabled Interrupt! ");
            if (isr.CodecWriteInProgressEnabled)
                Console.WriteLine("Codec WriteInProgress Interrupt!");
            if (isr.DAC1InterruptEnabled)
                Console.WriteLine("DAC1 Interrupt!");
            if (isr.DAC2InterruptEnabled)
                Console.WriteLine("DAC2 Interrupt!");
            if (isr.UARTInterruptEnabled)
                Console.WriteLine("UART Interrupt!");
            if (isr.MCCBIntEnabled)
                Console.WriteLine("MCCB Interrupt!");
            this.ResetAllIRQ();

        }

        private void ResetAllIRQ()
        {
            //Setting a bit to 1 will reset it. So we write 16 one's to reset entire getISR().
            isr.ISR = 0xFFFF;
        }

        /// <summary>
        /// The IRQMaskRegister determines what kind of events which cause IRQ to be raised.
        /// </summary>
        #endregion
        #region Debugging

        public void DumpRegisters()
        {
            Console.WriteLine("Control Register: " + cr.ToString());
            Console.WriteLine("Status Register: " + isr.ToString());
        }
        #endregion

        public void prepareStreamPlayBack(PCMStream pcmStream)
        {

        }

    }
}
