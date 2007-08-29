using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.IL2CPU.IL;

namespace IL2CPU {
    class OpCodeMap {
        protected List<int, Op> mMap = new List<int, Op>();

        void OpCodeMap() {
            //Use reflection and scan Indy.ILCPU.IL for all descendants of Op and populate mMap
        }
    }
}
