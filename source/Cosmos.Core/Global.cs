using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Core.IOGroup;

namespace Cosmos.Core {
    static public class Global {
        static public BaseIOGroups BaseIOGroups = new BaseIOGroups();
        static readonly public Cosmos.Debug.Kernel.Debugger Dbg = new Cosmos.Debug.Kernel.Debugger("Core", "");

        // These are used by Bootstrap.. but also called to signal end of interrupt etc...
        // Need to chagne this.. I dont like how this is.. maybe isolate or split into to classes... one for boostrap one for 
        // later user
        static public PIC PIC
        {
            get
            {
                return Bootstrap.PIC;
            }
        }

        static public CPU CPU
        {
            get
            {
                return Bootstrap.CPU;
            }
        }
        
        static public void Init() {
            // See note in Bootstrap about these
            
            // DONT transform the properties in fields, as then they remain null somehow.
        }
    }
}
