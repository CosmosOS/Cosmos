using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Hardware2;

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
        /// Later properties will be added, which will alter this. 
        /// However if no properties are changed from defaults, and Execute is called it should
        /// perform a default boot.
        /// </summary>
        public void Execute(bool aCleanScreen) {
            //Hardware.VGAScreen.SetTextMode(VGAScreen.TextSize.Size80x25);
            Console.Clear();

            Kernel.Global.Init();
            Hardware2.Global2.Init();
            Sys.Global.Init();

            // Clear again in case debug information has been written out
            if (aCleanScreen)
            {
                Console.Clear();
            }
        }

        public void Execute() {
            Execute(true);
        }

    }
}
