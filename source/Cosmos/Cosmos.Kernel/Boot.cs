using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Kernel {
    // This class provides boot configurations to be called
    // as the first line from Application code.
    // For now we just have default, but can add others in the future
    public class Boot {
        public static void Default() {
			System.Diagnostics.Debugger.Break();
            //Init Heap first - Hardware loads devices and they need heap
			Heap.CheckInit();
            
            // This should be the ONLY reference to the specific assembly
            // Later I would like to make this auto loading, but .NET's
            // init methods are a bit more work than Delphi's. :)
			// MTW: you could use partial methods for this, but then you dont
			// have control of the order in which the individual methods are called..
            Cosmos.Hardware.PC.Global.Init();
			New.Partitioning.MBT.Initialize();

            // Now init kernel devices and rest of kernel
			Console.WriteLine("Init Keyboard");
			System.Diagnostics.Debugger.Break();
            Keyboard.Initialize();
        }
    }
}
