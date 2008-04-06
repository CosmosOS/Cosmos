using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using Cosmos.Hardware.PC.Bus;
using Cosmos.Hardware.Network.Devices.RTL8139.Register;

namespace Cosmos.Hardware.Network.Devices.RTL8139.Register
{

    /// <summary>
    /// The registers in the RTL 8139 can be divided in two types. The first type is the registers where each bit has
    /// a meaning, and it is common to change specific bits. F.instance the CommandRegister where you have a Reset bit.
    /// 
    /// The other type of registers are the ones who contain values. Like a 32-bit pointer to an address, or a counter
    /// of some sort. Here we never set individual bits, but treat the entire 8, 16 or 32 bits as one logical unit.
    /// 
    /// This class contains accessor to valuetype registers.
    /// </summary>
    public class ValueTypeRegisters
    {
        #region Constructor

        MemoryAddressSpace xMem;
        public static ValueTypeRegisters Load(MemoryAddressSpace aMem)
        {
            return new ValueTypeRegisters(aMem);   
        }

        private ValueTypeRegisters(MemoryAddressSpace aMem)
        {
            this.xMem = aMem;
        }

        #endregion


        #region MAC address


        public MACAddress Mac
        {
            get
            {
                byte[] b = new byte[6];

                for (byte i = 0; i < 6; i++)
                    b[i] = xMem.Read8Unchecked((UInt32)MainRegister.Bit.MAC0 + i);

                return new MACAddress(b);
            }
            set
            {
                //TODO: Fix - only permitted to write with 4 byte access. I.e. use Write32.
                for (byte b = 0; b < 6; b++)
                    xMem.Write8Unchecked((UInt32)MainRegister.Bit.MAC0 + b, value.bytes[b]);
            }
        }


        #endregion

        #region MAR - Multicast

        public byte[] Mar
        {
            get
            {
                byte[] b = new byte[6];

                for (byte i = 0; i < 6; i++)
                    b[i] = xMem.Read8Unchecked((UInt32)MainRegister.Bit.MAR0 + i);

                return b;
            }
            set
            {
                //TODO: Fix - only allowed to do 32 bit write
                for (byte b = 0; b < 6; b++)
                    xMem.Write8Unchecked((UInt32)MainRegister.Bit.MAR0 + b, value[b]);
            }
        }

        #endregion


        //TSAD

        //RBSTART

        //CAPR(?)

        //CBR

        #region CBR/CBP/Current Buffer Address/Buffer Write Pointer/Received byte count
        public UInt16 CurrentBufferPointer 
        {
            get { return xMem.Read16((UInt32)MainRegister.Bit.RxBufAddr); }
            private set { ;}
        }

        #endregion

        //TCTR

        //MPC
    }
}
