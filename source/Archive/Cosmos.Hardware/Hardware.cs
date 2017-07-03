using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Hardware2 {
    public class Hardware {
        //Dont want non hardware members to be able to call this - but need to expose it for future hardware..... 
        //TODO: Remove these when all old hardware is ported
        protected static void IOWriteByte(ushort aPort, byte aData) { }
        protected static byte IOReadByte(ushort aPort) { return 0; }
		protected static void IOWriteWord(ushort aPort, ushort aData) { }
		protected static ushort IOReadWord(ushort aPort) { return 0; }
    }
}
