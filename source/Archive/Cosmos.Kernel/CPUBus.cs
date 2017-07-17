using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Kernel {
    public class CPUBus {

        // plugged
        public static void Write8(UInt16 aPort, byte aData) { }
        // plugged
        public static void Write16(UInt16 aPort, UInt16 aData) { }
        // plugged
        public static void Write32(UInt16 aPort, UInt32 aData) { }

        // plugged
        public static byte Read8(UInt16 aPort) { return 0; }
        // plugged
        public static UInt16 Read16(UInt16 aPort) { return 0; }
        // plugged
        public static UInt32 Read32(UInt16 aPort) { return 0; }
    }
}
