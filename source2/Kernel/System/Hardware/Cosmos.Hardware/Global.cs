using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Hardware {
    static public class Global {
        static readonly public Cosmos.Debug.Kernel.Debugger Dbg = new Cosmos.Debug.Kernel.Debugger("Hardware", "");

        static public Keyboard Keyboard;
        //static public PIT PIT = new PIT();
        static public TextScreen TextScreen;
        static public ATA ATA1;

        static public void Init() {
            // DANGER! This is before heap? Yet somehow its working currently...
            // Leaving it for now because Core.Init outputs to Console, but we need
            // to change this...
            // Heap seems to self init on demand? But even before IDT/GDT etc?
            TextScreen = new TextScreen();
            TextScreen.Clear();
            
            Global.Dbg.Send("Cosmos.Hardware.Global.Init");
            Core.PciBus.OnPCIDeviceFound = PCIDeviceFound;
            Cosmos.Core.Global.Init();

            Keyboard = new Keyboard();
            //ATA1.Test();
        }

        static void PCIDeviceFound(Core.PciBus.PciInfo aInfo, Core.IOGroup.PciDevice aIO) {
            // Later we need to dynamically load these, but we need to finish the design first.
            if ((aInfo.VendorID == 0x8086) && (aInfo.DeviceID == 0x7111)) {
                //ATA1 = new ATA(Core.Global.BaseIOGroups.ATA1);
            }
        }

    }
}
