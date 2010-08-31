using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Core {
    static public class Global {
        static public CPU CPU;
        static public BaseIOGroups BaseIOGroups = new BaseIOGroups();
        static readonly public Cosmos.Debug.Kernel.Debugger Dbg = new Cosmos.Debug.Kernel.Debugger("Core", "");
        static public PIC PIC;
        static internal PciBus PciBus;

        static public void Init() {
            //TODO: Move this stuff to a pre init so its guaranteed to be first, even before static inits and other calls
            // Drag this stuff in to the compiler manually until we add the always include attrib
            INTs.Dummy();

            CPU = new CPU();
            CPU.CreateGDT();
            PIC = new PIC();
            CPU.CreateIDT(true);
            CPU.InitFloat();
            /////////////////////////////////

            //Init Heap first - Hardware loads devices and they need heap
            // drag in the heap:
            Heap.Initialize();
            //TODO: Since this is FCL, its "common". Otherwise it should be
            // system level and not accessible from Core. Need to think about this
            // for the future.
            Console.WriteLine("    Heap OK");

            Console.WriteLine("    Finding PCI Devices");
            // Enumerate PCI Bus
            PciBus = new PciBus();
        }
    }
}
