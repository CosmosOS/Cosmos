using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HW = Cosmos.Hardware;

namespace Cosmos.Hardware.PC {
    public class Global : Cosmos.Hardware.Global {
        public static void Init(bool noATA, bool noATAOld, bool noATA2) {
            mProcessor = new Processor();
            Bus.CPU.PIC.Init(); 

            //All old.. need to port ----------------
            HW.Serial.InitSerial(0);
            HW.DebugUtil.Initialize();
            HW.DebugUtil.SendMessage("Logging", "Initialized!");
            HW.PIT.Initialize(Tick);

            // Partially new
            //HW.Interrupts.IRQ01 += new Interrupts.InterruptDelegate(Cosmos.Hardware.Keyboard.HandleKeyboardInterrupt);
            Interrupts.Init();

			HW.PC.Bus.PCIBus.Init();

            // end partially new
			if (!noATA) HW.Storage.ATA.Initialize(Sleep);
			if (!noATAOld) HW.Storage.ATAOld.Initialize(Sleep);
			
            HW.CPU.CreateIDT();
            // end old -----------------

			// MTW new
			if (!noATA2) HW.Storage.ATA2.ATA.Initialize(Sleep);
			// MTW new end

            HW.Device.Add(new Bus.CPU.Keyboard());
        }
		[Obsolete("Use Init(noATA, noATAOld, noATA2)")]
		public static void Init()
		{
			Init(false, false, false);
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
			Cosmos.Hardware.DebugUtil.SendNumber("PC", "Sleep", aMSec, 32);
            while (TickCount < xEnd) {
                ;
            }
			Cosmos.Hardware.DebugUtil.SendMessage("PC", "Sleeping done");
        }
    }
}
