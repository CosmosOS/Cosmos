using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Hardware.Audio.Devices.ES1370.Registers
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
    }
}
