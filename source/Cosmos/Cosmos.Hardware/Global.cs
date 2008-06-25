using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Kernel;

namespace Cosmos.Hardware {
    public class Global {
        public static void InitMatthijs(bool noATA, bool noATAOld, bool noATA2) {
        }

        public static void Init() {
            Kernel.CPU.CreateGDT();
            PIC.Init();

            Serial.InitSerial(0);
            PIT.Initialize(Tick);

            //HW.Interrupts.IRQ01 += new Interrupts.InterruptDelegate(Cosmos.Hardware.Keyboard.HandleKeyboardInterrupt);
            Interrupts.Init();
            Kernel.CPU.CreateIDT();

            PCIBus.Init();

            // Old
            Keyboard.Initialize();
            // New
            //Device.Add(new PC.Bus.CPU.Keyboard());
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
                CPU.Halt();
            }
            Cosmos.Hardware.DebugUtil.SendMessage("PC", "Sleeping done");
        }
    }
}
