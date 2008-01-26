using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HW = Cosmos.Hardware;

namespace Cosmos.Hardware.PC {
    public class Global : Cosmos.Hardware.Global {
        public static void Init() {
            mProcessor = new Processor();
            HW.Device.Add(new Bus.CPU.Keyboard());
            //All old.. need to port
            Bus.CPU.PIC.Init();
            HW.Serial.InitSerial(0);
            HW.DebugUtil.Initialize();
            HW.DebugUtil.SendMessage("Logging", "Initialized!");
            HW.PIT.Initialize(Tick);

            //HW.Interrupts.IRQ01 += new Interrupts.InterruptDelegate(Cosmos.Hardware.Keyboard.HandleKeyboardInterrupt);
            Interrupts.IncludeAllHandlers();
            
            HW.Storage.ATA.Initialize(Sleep);
            HW.CPU.CreateIDT();
            // end old
        }

        public static uint TickCount {
            get;
            private set;
        }

        private static void Tick(object aSender, EventArgs aEventArgs) {
            TickCount += 1;
        }   

        //TODO: Change this to use an x86 Op or something so it doesnt
        // just thrash
        public static void Sleep(uint aMSec) {
            uint xStart = TickCount;
            uint xEnd = xStart + aMSec;
            while (TickCount < xEnd) {
                ;
            }
        }
    }
}
