using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Core {
    static public class Global {
        static public BaseIOGroups BaseIOGroups = new BaseIOGroups();

        static public void Init() {
            // Temp
            Kernel.Global.Init();
            Console.WriteLine("    Init Global Descriptor Table");
            Kernel.CPU.CreateGDT();
            Console.WriteLine("    Init IDT");
            Kernel.CPU.CreateIDT(true);
            Console.WriteLine("    Init Floating point unit");
            Kernel.CPU.InitFloat();
            // End Temp
            IRQs.Dummy();
        }
    }
}
