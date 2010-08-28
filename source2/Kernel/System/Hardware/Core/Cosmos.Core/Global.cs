using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Core {
    static public class Global {
        static public CPU CPU;
        static public BaseIOGroups BaseIOGroups;
        static readonly public Cosmos.Debug.Kernel.Debugger Dbg = new Cosmos.Debug.Kernel.Debugger("Core", "");
        static public PIC PIC;
        static internal PciBus PciBus;

        static public void Init() {
            CPU = new CPU();

            CPU.CreateGDT();
            PIC = new PIC();
            CPU.CreateIDT(true);
            CPU.InitFloat();

            // Drag this stuff in to the compiler manually until we add the always include attrib
            INTs.Dummy();

            //Init Heap first - Hardware loads devices and they need heap
            // drag in the heap:
            Heap.Initialize();
            Console.WriteLine("    Heap OK");

            // After heap init etc
            BaseIOGroups = new BaseIOGroups();

            Console.WriteLine("    Finding PCI Devices");
            // Enumerate PCI Bus
            PciBus = new PciBus();
        }
    }
}
