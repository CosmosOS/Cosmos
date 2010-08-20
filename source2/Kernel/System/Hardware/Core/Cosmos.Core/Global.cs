using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Core {
    static public class Global {
        static readonly public BaseIOGroups BaseIOGroups = new BaseIOGroups();
        static readonly public Cosmos.Debug.Kernel.Debugger Dbg = new Cosmos.Debug.Kernel.Debugger("Core", "");
        static public PIC PIC;
        static public Heap Heap;

        static public void Init() {
            // Temp
            //Init Heap first - Hardware loads devices and they need heap
            Console.WriteLine("    Init Heap");
            Heap = new Heap(Kernel.CPU.EndOfKernel, (Kernel.CPU.AmountOfMemory * 1024 * 1024) - 1024);

            Kernel.CPU.CreateGDT();
            PIC = new PIC();
            Kernel.CPU.CreateIDT(true);
            Kernel.CPU.InitFloat();
            // End Temp
            IRQs.Dummy();
        }
    }
}
