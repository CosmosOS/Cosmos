using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Sys {
    public class Deboot {
        public static void Halt() {
            Console.WriteLine("Cosmos: Halting system.");
            while (true)
            {
                Cosmos.Kernel.CPU.Halt();
            }
        }

        public static void ShutDown() {
            //TODO: For now we just call Halt
            Halt();
        }

        //Plugged
        public static void Reboot() {

        }
    }
}
