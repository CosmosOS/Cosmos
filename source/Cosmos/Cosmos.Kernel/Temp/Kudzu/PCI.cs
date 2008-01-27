using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Kernel.Temp.Kudzu {
    public class PCI {
        static public void Test() {
            Cosmos.Hardware.PC.Bus.PCIBus.Init();
        }
    }
}
