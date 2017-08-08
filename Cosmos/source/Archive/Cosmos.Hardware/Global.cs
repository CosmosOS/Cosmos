using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Kernel;

namespace Cosmos.Hardware2 {
    public class Global2 {
        public static void Init() {
            //Console.WriteLine("    Init Serial");
            //Serial.InitSerial(0); // MtW: disabled, screws up debugging
            //PIT.Initialize(Tick);

            //Doku: See ACPIManager class
            //Console.WriteLine("    Init ACPI");
            //ACPIManager.Init();
                                                                                
            //Console.WriteLine("    Init PCIBus");
            //PCIBus.Init();

            //Console.WriteLine("    Init Mouse");
            //Mouse.Initialize();
            // New
            //Console.WriteLine("    Init ATA");
            //Storage.ATA.ATA.Initialize();
            //Device.Add(new PC.Bus.CPU.Keyboard());

            //Network.Devices.RTL8139.RTL8139.InitDriver();
            //Network.Devices.AMDPCNetII.AMDPCNet.InitDriver();
            //Network.Devices.ViaRhine.VT6102.InitDriver();

            //Console.WriteLine("    Init Device Drivers");
            //Device.DriversInit();
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
            //Cosmos.Debug.Debugger.SendNumber("PC", "Sleep", aMSec, 32);
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
            //Cosmos.Debug.Debugger.SendMessage("PC", "Sleeping done");
        }
    }
}
