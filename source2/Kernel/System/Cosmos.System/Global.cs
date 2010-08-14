using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.System {
    public class Global {
        static public void Init() {
            Cosmos.Hardware.Global.Init();
            // Temp
            Cosmos.Sys.Global.Init();
            // End temp
        }
    }
}
