using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.FileSystem;

namespace Cosmos.Kernel {

    /// <summary>
    /// Boot configurations for Cosmos.
    /// One of these configurations should be called from the first line of any Cosmos-based operating system.
    /// For now we just have default, but can add others in the future.
    /// </summary>
    public class Boot {

        /// <summary>
        /// Boot the kernel using default boot-configuration.
        /// Initializes basic hardware like CPU, serialports, PCI, Keyboard and blockdevices.
        /// </summary>
        public static void Default() {
            //Init Heap first - Hardware loads devices and they need heap
			Heap.CheckInit();
            
            // This should be the ONLY reference to the specific assembly
            // Later I would like to make this auto loading, but .NET's
            // init methods are a bit more work than Delphi's. :)
			// MTW: you could use partial methods for this, but then you dont
			// have control of the order in which the individual methods are called..
            Cosmos.Hardware.PC.Global.Init();
			MBT.Init();
            Console.Clear();
        }
    }
}
