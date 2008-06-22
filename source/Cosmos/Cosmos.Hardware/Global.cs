using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Hardware {
    public class Global {
        public static void InitMatthijs(bool noATA, bool noATAOld, bool noATA2) {
        }

        public static void Init() {
            Kernel.CPU.CreateGDT();
            PC.Bus.CPU.PIC.Init();

            Serial.InitSerial(0);
            PIT.Initialize(Tick);

            //HW.Interrupts.IRQ01 += new Interrupts.InterruptDelegate(Cosmos.Hardware.Keyboard.HandleKeyboardInterrupt);
            PC.Interrupts.Init();
            Kernel.CPU.CreateIDT();

            PC.Bus.PCIBus.Init();

            // Old
            Keyboard.Initialize();
            // New
            //Device.Add(new PC.Bus.CPU.Keyboard());

            Cosmos.Hardware.PC.Bus.PCIBus.Init();
        }

        public static uint TickCount {
            get;
            private set;
        }

        private static void Tick(object aSender, EventArgs aEventArgs) {
            TickCount++;
        }

        //TODO: Change this to use an x86 Op or something so it doesnt
        // just thrash
        public static void Sleep(uint aMSec) {
            uint xStart = TickCount;
            uint xEnd = xStart + aMSec;
            Cosmos.Hardware.DebugUtil.SendNumber("PC", "Sleep", aMSec, 32);
            while (TickCount < xEnd) {
                ;
            }
            Cosmos.Hardware.DebugUtil.SendMessage("PC", "Sleeping done");
        }
    }
}
