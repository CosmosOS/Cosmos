using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.System {
    static public class Global {
        static readonly public Cosmos.Debug.Kernel.Debugger Dbg = new Cosmos.Debug.Kernel.Debugger("System", "");
        static public Console Console;
        
        static public void Init() {
            // Danger: See note in Hardware.Global.Init about TextScreen.
            Console = new Console(); 
            
            Cosmos.Hardware.Global.Init();
        }
    }
}
