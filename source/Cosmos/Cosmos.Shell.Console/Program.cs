using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Shell.Console {
    class Program {
        static void Main() {
            Kernel.CPU.Init();
            //System.Console.WriteLine("Cosmos creation complete");
        	Kernel.Interrupts.DoTest();
        	object o = new object();
        	System.Console.WriteLine("Object Created");
			if (o == null) {
				System.Console.WriteLine("Object == null");
			} else {
				System.Console.WriteLine("Object != null");
			}
			o = null;
        }
    }
}
