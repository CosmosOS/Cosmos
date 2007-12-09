using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Hardware {
    public class Hardware {
        //Dont want non hardware members to be able to call this - but need to expose it for future hardware..... 
        protected static void IOWriteByte(ushort aPort, byte aData) { }
        protected static byte IOReadByte(ushort aPort) { return 0; }
    }
}
