using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Hardware2.Audio.Devices.ES1370.Registers
{
    class UARTInterfaceRegister
    {
        Kernel.MemoryAddressSpace xMem;
        public static UARTInterfaceRegister Load(Kernel.MemoryAddressSpace aMem)
        {
            return new UARTInterfaceRegister(aMem);
        }

        private UARTInterfaceRegister(Kernel.MemoryAddressSpace aMem)
        {
            xMem = aMem;
        }
    }
}
