using System;
using Cosmos.Kernel;

namespace Cosmos.Core {
    // Sealed so higher rings cannot inherit and muck about
    sealed public class IOPort {
        public readonly UInt16 Port;

        // Only Core ring can create it.. but hardware ring can use it.
        internal IOPort(UInt16 aPort) {
            Port = aPort;
        }

        public void Write8(byte aData) {
            CPUBus.Write8(Port, aData);
        }
        public void Write16(UInt16 aData) {
            CPUBus.Write16(Port, aData);
        }
        public void Write32(UInt32 aData) {
            CPUBus.Write32(Port, aData);
        }

        public byte Read8() {
            return CPUBus.Read8(Port); 
        }
        public UInt16 Read16() {
            return CPUBus.Read16(Port);
        }
        public UInt32 Read32() {
            return CPUBus.Read32(Port);
        }

    }
}
