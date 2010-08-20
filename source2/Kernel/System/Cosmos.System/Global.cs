using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.System {
    static public class Global {
        static readonly public Cosmos.Debug.Kernel.Debugger Dbg = new Cosmos.Debug.Kernel.Debugger("System", "");
        static readonly public Console Console = new Console();
        
        static public void Init() {
            Cosmos.Hardware.Global.Init();
        }
    }
}
