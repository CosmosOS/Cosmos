//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Cosmos.Hardware2.PC.Bus;
//using Cosmos.Kernel;

//namespace Cosmos.Hardware2.PC.Bus.USB
//{
//    public class USBHostOHCIRegisters
//    {
//        private MemoryAddressSpace regs;
//        public USBHostOHCIRegisters(MemoryAddressSpace regs)
//        {
//            this.regs = regs;
//        }

//        public UInt32 HcRevision { get { return regs.Read32Unchecked(0x0); } }
//        public UInt32 HcControl { get { return regs.Read32Unchecked(0x4); } set { regs.Write32Unchecked(0x4, value); } }
//        public UInt32 HcCommandStatus { get { return regs.Read32Unchecked(0x8); } set { regs.Write32Unchecked(0x8, value); } }
//        public UInt32 HcInterruptStatus { get { return regs.Read32Unchecked(0xC); } set { regs.Write32Unchecked(0xC, value); } }
//        public UInt32 HcInterruptEnable { get { return regs.Read32Unchecked(0x10); } set { regs.Write32Unchecked(0x10, value); } }
//        public UInt32 HcInterruptDisable { get { return regs.Read32Unchecked(0x14); } set { regs.Write32Unchecked(0x14, value); } }
//        public UInt32 HcHCCA { get { return regs.Read32Unchecked(0x18); } set { regs.Write32Unchecked(0x18, value); } }
//        public UInt32 HcPeriodCurrentED { get { return regs.Read32Unchecked(0x1C); } set { regs.Write32Unchecked(0x1C, value); } }
//        public UInt32 HcControlHeadED { get { return regs.Read32Unchecked(0x20); } set { regs.Write32Unchecked(0x20, value); } }
//        public UInt32 HcControlCurrentED { get { return regs.Read32Unchecked(0x24); } set { regs.Write32Unchecked(0x24, value); } }
//        public UInt32 HcBulkHeadED { get { return regs.Read32Unchecked(0x28); } set { regs.Write32Unchecked(0x28, value); } }
//        public UInt32 HcBulkCurrentED { get { return regs.Read32Unchecked(0x2C); } set { regs.Write32Unchecked(0x2C, value); } }
//        public UInt32 HcDoneHead { get { return regs.Read32Unchecked(0x30); } set { regs.Write32Unchecked(0x30, value); } }
//        public UInt32 HcFmInterval { get { return regs.Read32Unchecked(0x34); } set { regs.Write32Unchecked(0x34, value); } }
//        public UInt32 HcFmRemaining { get { return regs.Read32Unchecked(0x38); } set { regs.Write32Unchecked(0x38, value); } }
//        public UInt32 HcFmNumber { get { return regs.Read32Unchecked(0x3C); } set { regs.Write32Unchecked(0x3C, value); } }
//        public UInt32 HcPeriodicStart { get { return regs.Read32Unchecked(0x40); } set { regs.Write32Unchecked(0x40, value); } }
//        public UInt32 HcLSThreshold { get { return regs.Read32Unchecked(0x44); } set { regs.Write32Unchecked(0x44, value); } }
//        public UInt32 HcRhDescriptorA { get { return regs.Read32Unchecked(0x48); } set { regs.Write32Unchecked(0x48, value); } }
//        public UInt32 HcRhDescriptorB { get { return regs.Read32Unchecked(0x4C); } set { regs.Write32Unchecked(0x4C, value); } }
//        public UInt32 HcRhStatus { get { return regs.Read32Unchecked(0x50); } set { regs.Write32Unchecked(0x50, value); } }
                
//    }

//    public struct HcRevisionReg
//    {
//        private UInt32 data;
//        private HcRevisionReg(UInt32 a)
//        {
//            this.data = a;
//        }

//        #region Implicit Conversions
//        public static implicit operator UInt32(HcRevisionReg a)
//        {
//            return a.data;
//        }

//        public static implicit operator HcRevisionReg(UInt32 u)
//        {
//            return new HcRevisionReg(u);
//        }
//        #endregion

//        #region FieldAccessors
//        public byte Major
//        {
//            get { return (byte)((data & 0xf0) >> 4); }
//            set { data = (data & 0xffffff0f) | (UInt32)((value & 0xf) << 4); }
//        }
//        public byte Minor
//        {
//            get { return (byte)(data & 0xf); }
//            set { data = (data & 0xfffffff0) | (UInt32)(value & 0xf); }
//        }
//        #endregion

//        public override string ToString()
//        {
//            return data.ToString();
//        }
//    }

//    public struct HcControlReg
//    {
//        private UInt32 data;
//        private HcControlReg(UInt32 a)
//        {
//            this.data = a;
//        }

//        #region Implicit Conversions
//        public static implicit operator UInt32(HcControlReg a)
//        {
//            return a.data;
//        }

//        public static implicit operator HcControlReg(UInt32 u)
//        {
//            return new HcControlReg(u);
//        }
//        #endregion

//        #region FieldAccessors

//        public byte ControlBulkServiceRatio
//        {
//            get { return (byte)((data & 0x3)); }
//        }
//        public bool PeriodicListEnable
//        {
//            get { return (data & 0x4) == 0x4; }
//        }
//        public bool IsochronousEnable
//        {
//            get { return (data & 0x8) == 0x8; }
//        }

//        public bool ControlListEnable
//        {
//            get { return (data & 0x10) == 0x10; }
//        }
//        public bool BulkListEnable
//        {
//            get { return (data & 0x20) == 0x20; }
//        }

//        public USBStates HostControllerFunctionalState
//        {
//            get { return (USBStates)((data & 0xc0) >> 6); }
//            set { data = data & 0xffffff3f | ((UInt32)value << 6); }
//        }

//        #endregion

//        public override string ToString()
//        {
//            return data.ToString();
//        }
//    }

//    public enum USBStates : byte
//    {
//        Reset = 0, Resume = 1, Operational = 2, Suspend = 3
//    }
//}
