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
            //PIT.Initialize(Tick);

            //HW.Interrupts.IRQ01 += new Interrupts.InterruptDelegate(Cosmos.Hardware.Keyboard.HandleKeyboardInterrupt);
            Interrupts.Init();
            Kernel.CPU.CreateIDT(true);
            
            PCIBus.Init();

            // Old
            Keyboard.Initialize();
            // New
            Storage.ATA.ATA.Initialize();
            //Device.Add(new PC.Bus.CPU.Keyboard());
        }

        public static uint TickCount {
            get;
            private set;
        }

        private static void Tick(object aSender, EventArgs aEventArgs) {
            TickCount++;
        }

        // DO NOT USE, try to keep the kernel tickless
        // note: if you do use it, first enable PIT in code again..
        public static void Sleep_Old(uint aMSec) {
            Cosmos.Hardware.DebugUtil.SendNumber("PC", "Sleep", aMSec, 32);
            CPU.Halt();//At least one hlt even if aMSec is 0
            if (aMSec > 0)
            {
                uint xStart = TickCount;
                uint xEnd = xStart + aMSec;
                while (TickCount < xEnd)
                {
                    CPU.Halt();
                }
            }
            Cosmos.Hardware.DebugUtil.SendMessage("PC", "Sleeping done");
        }
    }
}
