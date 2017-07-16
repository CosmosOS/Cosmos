using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Hardware2;
using Cosmos.Hardware2.Audio.Devices.ES1370.Registers;
using Cosmos.Hardware2.Audio.Devices.ES1370.Components;
using Cosmos.Hardware2.Audio.Devices.ES1370.Managers;
namespace Cosmos.Hardware2.Audio.Devices.ES1370
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
        public const int SRClock = 1411200;
        public const int  minClockDen=29;
        public const int maxClockDen=353;
        public const int clockStep=1;
        public const byte DACPage = 0x0c;
        public const byte UARTPage = 0x0e;
        public const byte UART1Page = 0x0f;
        public const byte DAC1SMPREG =0x70;
        public const byte DAC2SMPREG=0x74;
        public const byte DAC1VolSMPREG=0x7c;
        public const byte DAC2VolSMPREG=0x7e;
        public const byte TruncNumSMPREG=0x00;
        public const byte INTRegsSMPREG=0x01;
        public const byte AccumFrac=0x02;
        public const byte VFreqFrac = 0x03;

        public ES1370(PCIDevice device) : base(device)
        {
            isr=(InterruptStatusRegister.Load(getMemReference()));
            sir = (SerialInterfaceRegister.Load(getMemReference()));
            uir = (UARTInterfaceRegister.Load(getMemReference()));
            cr=(ControlRegister.Load(getMemReference()));
            //dacs.Add(new AK(new DACak4531(), cr.DAC1Enabled,(byte)MainRegister.Bit.SerialIntContr,MainRegister.Bit.Dac1FrameAddr, (byte)MainRegister.Bit.Dac1FrameSize));
            //dacs.Add(new DACManager(new DACak4531(), cr.DAC2Enabled, (byte)MainRegister.Bit.SerialIntContr, (byte)MainRegister.Bit.Dac2FrameAddr, (byte)MainRegister.Bit.Dac2FrameSize));
        }
        /// <summary>
        /// Retrieve all Ensoniq AudioPCI 1370 cards found on computer.
        /// </summary>
        /// <returns></returns>
        public static List<ES1370> FindAll()
        {
            List<ES1370> found = new List<ES1370>();

            foreach (PCIDevice device in Cosmos.Hardware2.PCIBus.Devices)
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
            //Cosmos.Hardware2.Interrupts.IRQ05 = new Cosmos.Hardware2.Interrupts.InterruptDelegate(this.HandleAudioInterrupt);
            //Cosmos.Hardware2.Interrupts.AddIRQHandler(5, this.HandleAudioInterrupt);
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
        //public void HandleAudioInterrupt(ref IRQContext aContext)
        //{
        //    Console.Write("IRQ detected: ");
        //    if (isr.IsCodecBusyIntEnabled)
        //        Console.WriteLine("Codec busy Interrupt! ");
        //    if(isr.IsCodecStatusIntEnabled)
        //        Console.WriteLine("Codec Enabled Interrupt! ");
        //    if (isr.IsCodecWriteInProgressEnabled)
        //        Console.WriteLine("Codec WriteInProgress Interrupt!");
        //    if (isr.IsDAC1InterruptEnabled)
        //        Console.WriteLine("DAC1 Interrupt!");
        //    if (isr.IsDAC2InterruptEnabled)
        //        Console.WriteLine("DAC2 Interrupt!");
        //    if (isr.IsUARTInterruptEnabled)
        //        Console.WriteLine("UART Interrupt!");
        //    if (isr.IsMCCBIntEnabled)
        //        Console.WriteLine("MCCB Interrupt!");
        //    this.ResetAllIRQ();

        //}

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

#region I/O Helper routine
        private void setDataOnDACCodec(AK4531Manager dacManager){ }
        private byte getMemPageValueByNum(byte num)
        {
           return getMemReference().Read8((UInt32)(Registers.MainRegister.Bit.MemPageAddr+(byte)((num) & 0x0f)));
        }

        private void setMemPageValueByNum(byte num, byte value)
        {
            getMemReference().Write8((UInt32)(Registers.MainRegister.Bit.MemPageAddr + (byte)((num) & 0x0f)), value);
        }
        private int getDAC2DividendRatio(int num)
        {
            int offset = (((num) & 0x1fff) << 16);
            return getMemReference().Read8((UInt32)(Registers.MainRegister.Bit.Control + (byte)offset));
        }
        private void setDAC2ClockInDividendRatio(int num,byte value)
        {
            int offset = (((num) >> 16) & 0x1fff);
            getMemReference().Write8((UInt32)(Registers.MainRegister.Bit.Control + (byte)offset),value);
        }
        private int getDAC1ClockDividendRatio(byte num)
        {
            int offset = (((num) & 0x03) << 12);
            return getMemReference().Read8((UInt32)(((Registers.MainRegister.Bit.Control) + (byte)offset)));
        }

        private void setVoiceCodeFromCCBNum(byte num, byte value)
        {
            int offset = (((num) >> 5) & 0x03);
            getMemReference().Write8((UInt32)(Registers.MainRegister.Bit.Status + (byte)offset), value);
        }

        private void setTxUARTInterruptInFromNum(int num, byte value){
            byte offset = (byte)(((num) >> 5) & 0x03);
            getMemReference().Write8((UInt32)(MainRegister.Bit.UartInfo + (byte)offset), value);
        }
        private byte getTxUARTInterruptOutFromNum(byte num)
        {
            byte offset = (byte)(((num) & 0x03 ) << 5);
            return getMemReference().Read8((UInt32)(MainRegister.Bit.UartInfo + (byte)offset));
        }
        private byte controlOutFromNum(byte num)
        {
            return getMemReference().Read8((UInt32)(MainRegister.Bit.UartInfo + (byte)((num) & 0x03)));
        }

        private int SRClockDivideByNum(int n)
        {
            return (SRClock / n - 2);
        }

#endregion
        private void preparePlayBackOnDac(AK4531Manager dacManager)
        {
            //dacManager.setDACStateEnabled(true);
            
        }
        public override void playStream(PCMStream pcmStream)
        {
            for (int count = 0; count < pcmStream.getData().Length; count++)
            {

            }
        }


    }
}
