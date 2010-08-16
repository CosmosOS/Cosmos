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

        public byte Byte {
            get { return CPUBus.Read8(Port); }
            set { CPUBus.Write8(Port, value); }
        }

        public UInt16 Word {
            get { return CPUBus.Read16(Port); }
            set { CPUBus.Write16(Port, value); }
        }

        public UInt32 DWord {
            get { return CPUBus.Read32(Port); }
            set { CPUBus.Write32(Port, value); }
        }

        //[AsmBody(Assembler = typeof(IOWrite8))]
        //public static void Write8(UInt16 aPort, byte aData) { }
        //public class IOWrite8 : CodeBlock {
        //    public override void Assemble() {
        //        EDX = EBP + 0x0C;
        //        EAX = EBP + 0x08;
        //        Port[DX] = AL;
        //    }
        //} 

    }
}
