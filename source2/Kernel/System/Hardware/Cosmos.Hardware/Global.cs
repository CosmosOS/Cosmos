using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Hardware {
    static public class Global {
        static public TextScreen TextScreen = new TextScreen();

        static public void Init() {
            Cosmos.Core.Global.Init();
            // Temp
            Cosmos.Hardware2.Global2.Init();
            // End Temp
        }

    }
}
