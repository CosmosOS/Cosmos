using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Hardware.PC.Bus {
    public class CPUBus : Cosmos.Hardware.Bus.CPUBus {

        // all plugs
        public static void Write8(UInt16 aPort, byte aData) { }
        public static void Write16(UInt16 aPort, UInt16 aData) { }
        public static void Write32(UInt16 aPort, UInt32 aData) { }

        public static byte Read8(UInt16 aPort) { return 0; }
        public static UInt16 Read16(UInt16 aPort) { return 0; }
        public static UInt32 Read32(UInt16 aPort) { return 0; }
    }
}
