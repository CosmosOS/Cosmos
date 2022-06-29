/*
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Hardware2.Audio.Devices.ES1370.Registers
{
    class SerialInterfaceRegister
    {
        Kernel.MemoryAddressSpace xMem;
        public static SerialInterfaceRegister Load(Kernel.MemoryAddressSpace aMem)
        {
            return new SerialInterfaceRegister(aMem);
        }

        private SerialInterfaceRegister(Kernel.MemoryAddressSpace aMem)
        {
            xMem = aMem;
        }
        
        #region Bits

        [Flags]
        public enum BitPosition : byte
        {
            DAC2Mode = 14,
            DAC1Mode = 13,
            DAC2PlayMode=12,
            DAC1PlayMode=11,
            DAC2IntEn=9,
            DAC1IntEn = 8,
            DAC1ForceReloadCnt=7,
            DAC2OnStopMode=6
        }

        [Flags]
        public enum BitValue : uint
        {
            DAC2Mode = BinaryHelper.BitPos.BIT14,
            DAC1Mode = BinaryHelper.BitPos.BIT13,
            DAC2PlayMode = BinaryHelper.BitPos.BIT12,
            DAC1PlayMode = BinaryHelper.BitPos.BIT11,
            DAC2IntEn = BinaryHelper.BitPos.BIT9,
            DAC1IntEn = BinaryHelper.BitPos.BIT8,
            DAC1ForceReloadCnt = BinaryHelper.BitPos.BIT7,
            DAC2OnStopMode = BinaryHelper.BitPos.BIT6
        }
        #endregion

        public byte SERIAL
        {
            get
            {
                return xMem.Read8((UInt32)Registers.MainRegister.Bit.SerialIntContr);
            }
            set
            {
                xMem.Write8((UInt32)Registers.MainRegister.Bit.SerialIntContr, value);
            }
        }
    }
}
*/