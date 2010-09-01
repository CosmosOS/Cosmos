using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.System {
    static public class Global {
        static readonly public Cosmos.Debug.Kernel.Debugger Dbg = new Cosmos.Debug.Kernel.Debugger("System", "");
        static public Console Console;
        
        static public void Init() {
            // We must init Console before calling Inits. This is part of the 
            // "minimal" boot to allow output
            Console = new Console(); 
            
            Cosmos.Hardware.Global.Init();
        }
    }
}
