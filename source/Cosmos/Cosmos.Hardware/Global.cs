using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Kernel;

namespace Cosmos.Hardware {
    public class Global {
        public static void Init() {
            Console.WriteLine("    Init Global Descriptor Table");
            Kernel.CPU.CreateGDT();
            Console.WriteLine("    Init PIC");
            PIC.Init();

            Console.WriteLine("    Init Serial");
            //Serial.InitSerial(0); // MtW: disabled, screws up debugging
            //PIT.Initialize(Tick);

            //HW.Interrupts.IRQ01 += new Interrupts.InterruptDelegate(Cosmos.Hardware.Keyboard.HandleKeyboardInterrupt);
            Console.WriteLine("    Init IRQ");
            Interrupts.Init();
            Kernel.CPU.CreateIDT(false);

            //Doku: See ACPIManager class
            //Console.WriteLine("    Init ACPI");
            //ACPIManager.Init();

            Console.WriteLine("    Init PCIBus");
            PCIBus.Init();

            Console.WriteLine("    Init PIT");
            PIT.Init();

            // Old
            Console.WriteLine("    Init Keyboard");
            Keyboard.Initialize();

            Console.WriteLine("    Init Mouse");
            Mouse.Initialize();
            // New
            Console.WriteLine("    Init ATA");
            //Storage.ATA.ATA.Initialize();
            //Device.Add(new PC.Bus.CPU.Keyboard());

            Network.Devices.RTL8139.RTL8139.InitDriver();
            //Network.Devices.AMDPCNetII.AMDPCNet.InitDriver();
            //Network.Devices.ViaRhine.VT6102.InitDriver();

            Console.WriteLine("    Init Device Drivers");
            Device.DriversInit();
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
