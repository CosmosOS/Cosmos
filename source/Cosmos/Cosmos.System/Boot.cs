using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Hardware;

namespace Cosmos.Sys {

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
            Console.Clear();
            Console.WriteLine("Initializing KERNEL");
            Kernel.Global.Init();

            Console.WriteLine("Initializing HARDWARE");
            Hardware.Global.Init();

            Console.WriteLine("Initializing SYSTEM");
            Sys.Global.Init();
            
            Console.Clear();
        }
    }
}
