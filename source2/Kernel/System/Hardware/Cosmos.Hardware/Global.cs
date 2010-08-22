using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Hardware {
    static public class Global {
        static readonly public Cosmos.Debug.Kernel.Debugger Dbg = new Cosmos.Debug.Kernel.Debugger("Hardware", "");

        static public Keyboard Keyboard = new Keyboard();
        static public PIT PIT = new PIT();
        static public TextScreen TextScreen = new TextScreen();
        static public ATA ATA;

        static public void Init() {
            Global.Dbg.Send("Cosmos.Hardware.Global.Init");
            Core.PciBus.OnPCIDeviceFound = PCIDeviceFound;
            Cosmos.Core.Global.Init();
        }

        static void PCIDeviceFound(Core.PciBus.PciInfo aInfo, Core.IOGroup.PciDevice aIO) {
            // Later we need to dynamically load these, but we need to finish the design first.
            if ((aInfo.VendorID == 0x8086) && (aInfo.DeviceID == 0x7111)) {
                ATA = new ATA(aIO);
            }
        }

    }
}
