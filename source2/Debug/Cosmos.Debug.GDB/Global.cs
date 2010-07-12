using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Cosmos.Debug.GDB {
    public class Global {
        static public UInt32 FromHex(string aValue) {
            return UInt32.Parse(aValue.Substring(2), NumberStyles.HexNumber);
        }
    }
}
