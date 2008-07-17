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
            Kernel.Global.Init();
            Hardware.Global.Init();
            Console.Clear();
        }

        public static void MtWDefault() {
            Console.Clear();
            Kernel.Global.Init();
            Hardware.Global.Init();
            DebugUtil.SendMessage("Test", "Start Test");
            var xDevice = Hardware.Device.FindFirst(Device.DeviceType.Storage);
            if (xDevice == null) { Console.WriteLine("ERROR: No StorageDevice Found!");
                return; }
            var xStorDevice = xDevice as BlockDevice;
            if (xStorDevice == null)
            {
                Console.WriteLine("ERROR: No StorageDevice Found! (2)");
                return;
            }
            var xTempBuff = xStorDevice.ReadBlock(1);
            if (xTempBuff.Length != 512) {
                Console.WriteLine("ERROR: Wrong block size!");
                return;
            }
            for (int i = 0; i < 512; i++) {
                if (xTempBuff[i] != (byte)(i % 256)) {
                    Console.WriteLine("ERROR: Wrong Data read!");
                    return;
                }
            }
            Console.WriteLine("Done");
        }

    }
}
