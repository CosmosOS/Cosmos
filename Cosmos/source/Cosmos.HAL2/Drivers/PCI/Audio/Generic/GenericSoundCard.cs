//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Cosmos.Hardware2.Audio.Devices.Generic.Managers;
//namespace Cosmos.Hardware2.Audio.Devices
//{
//    public abstract class GenericSoundCard
//    {
//        #region Construction
//        private PCIDevice pciCard;
//        private Cosmos.Kernel.MemoryAddressSpace mem;
//        protected List<DACManager> dacs;
//        protected List<ADCManager> adcs;
//        protected List<UARTManager> uarts;

//        #endregion

//        public GenericSoundCard(PCIDevice device){
//            if (device == null)
//                throw new ArgumentException("PCI Device is null. Unable to get "+this.GetType()+" card");
//            pciCard = device;
//            mem = device.GetAddressSpace(1) as Cosmos.Kernel.MemoryAddressSpace;
            
//            dacs = new List<DACManager>();
//            adcs = new List<ADCManager>();
//            uarts = new List<UARTManager>();
//        }
//        public PCIDevice PCICard { get { return pciCard; } private set { ;} }
//        #region Power and Initilization
//        public abstract bool Disable();
//        public abstract bool Enable();
//        #endregion
//        public abstract void playStream(PCMStream pcmStream);
//        #region Helpers
//        protected Cosmos.Kernel.MemoryAddressSpace getMemReference()
//        {
//            return mem;
//        }
//        #endregion

//    }
//}

