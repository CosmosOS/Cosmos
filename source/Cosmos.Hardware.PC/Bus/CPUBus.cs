using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Hardware.PC.Bus {
    public class CPUBus : Cosmos.Hardware.Bus.CPUBus {
        protected static void WriteByte(ushort aPort, byte aData) { }
        protected static void WriteWord(ushort aPort, ushort aData) { }
        protected static byte ReadByte(ushort aPort) { return 0; }
        protected static ushort ReadWord(ushort aPort) { return 0; }
    }
}
