using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.IL2CPU.X86.ELF {
    public static class Extensions {
        public static void AppendLine(this StringBuilder aThis, string aFormat, params object[] aArgs) {
            aThis.AppendLine(String.Format(aFormat, aArgs));
        }
    }
}