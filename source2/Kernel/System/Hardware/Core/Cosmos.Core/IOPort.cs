using System;

namespace Cosmos.Core {
    public abstract class IOPortBase {
        public readonly UInt16 Port;

        // all ctors are internal - Only Core ring can create it.. but hardware ring can use it.
        internal IOPortBase(UInt16 aPort) {
            Port = aPort;
        }
        internal IOPortBase(UInt16 aBase, UInt16 aOffset) {
            // C# math promotes things to integers, so we have this constructor
            // to relieve the use from having to do so many casts
            Port = (UInt16)(aBase + aOffset);
        }

        protected void Write8(UInt16 aPort, byte aData) { } // Plugged
        protected void Write16(UInt16 aPort, UInt16 aData) { } // Plugged
        protected void Write32(UInt16 aPort, UInt32 aData) { } // Plugged

        protected byte Read8(UInt16 aPort) { return 0; } // Plugged
        protected UInt16 Read16(UInt16 aPort) { return 0; } // Plugged
        protected UInt32 Read32(UInt16 aPort) { return 0; } // Plugged
    }

    public class IOPort : IOPortBase {
        internal IOPort(UInt16 aPort) : base(aPort) { }
        internal IOPort(UInt16 aBase, UInt16 aOffset) : base(aBase, aBase) { }

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
    }

    public class IOPortRead : IOPortBase {
        internal IOPortRead(UInt16 aPort) : base(aPort) { }
        internal IOPortRead(UInt16 aBase, UInt16 aOffset) : base(aBase, aOffset) { }

        public byte Byte {
            get { return Read8(Port); }
        }

        public UInt16 Word {
            get { return Read16(Port); }
        }

        public UInt32 DWord {
            get { return Read32(Port); }
        }
    }

    public class IOPortWrite : IOPortBase {
        internal IOPortWrite(UInt16 aPort) : base(aPort) { }
        internal IOPortWrite(UInt16 aBase, UInt16 aOffset) : base(aBase, aOffset) { }

        public byte Byte {
            set { Write8(Port, value); }
        }

        public UInt16 Word {
            set { Write16(Port, value); }
        }

        public UInt32 DWord {
            set { Write32(Port, value); }
        }
    }

}
