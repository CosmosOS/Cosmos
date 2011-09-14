using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Core {
    static public class Global {
        static public BaseIOGroups BaseIOGroups = new BaseIOGroups();
        static readonly public Cosmos.Debug.Kernel.Debugger Dbg = new Cosmos.Debug.Kernel.Debugger("Core", "");

        // These are used by Bootstrap.. but also called to signal end of interrupt etc...
        // Need to chagne this.. I dont like how this is.. maybe isolate or split into to classes... one for boostrap one for 
        // later user
        static public PIC PIC;
        static public CPU CPU;
        static public PCI PCI;

        static public void Init() {
            // See note in Bootstrap about these
            CPU = Bootstrap.CPU;
            PIC = Bootstrap.PIC;

            //TODO: Since this is FCL, its "common". Otherwise it should be
            // system level and not accessible from Core. Need to think about this
            // for the future.
            Console.WriteLine("Finding PCI Devices");
            PCI.Setup();
        }
    }
}
