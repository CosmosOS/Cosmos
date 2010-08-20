using System;

namespace Cosmos.Core {
    // Sealed so higher rings cannot inherit and muck about
    sealed public class IOPort {
        public readonly UInt16 Port;

        // Only Core ring can create it.. but hardware ring can use it.
        internal IOPort(UInt16 aPort) {
            Port = aPort;
        }

        public byte Byte {
            get { return Read8(Port); }
            set { Write8(Port, value); }
        }

        public UInt16 Word {
            get { return Read16(Port); }
            set { Write16(Port, value); }
        }

        public UInt32 DWord {
            get { return Read32(Port); }
            set { Write32(Port, value); }
        }

        protected void Write8(UInt16 aPort, byte aData) { } // Plugged
        protected void Write16(UInt16 aPort, UInt16 aData) { } // Plugged
        protected void Write32(UInt16 aPort, UInt32 aData) { } // Plugged

        protected byte Read8(UInt16 aPort) { return 0; } // Plugged
        protected UInt16 Read16(UInt16 aPort) { return 0; } // Plugged
        protected UInt32 Read32(UInt16 aPort) { return 0; } // Plugged
    }
}
