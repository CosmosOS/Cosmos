using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Hardware.PC.Bus {
    public class CPUBus : Cosmos.Hardware.Bus.CPUBus {
        // These are public. Would prefer internal, but will cause issues
        // in future as we add devices from other assemblies.
        // What else can we do to restrict access to them?
        public static void WriteByte(ushort aPort, byte aData) { }
        public static void WriteWord(ushort aPort, ushort aData) { }
        public static byte ReadByte(ushort aPort) { return 0; }
        public static ushort ReadWord(ushort aPort) { return 0; }
    }
}
