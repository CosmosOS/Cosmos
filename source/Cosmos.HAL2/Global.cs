using System;
using Cosmos.Core;
using Cosmos.Debug.Kernel;
using Cosmos.HAL.BlockDevice;
using Cosmos.HAL.Drivers.PCI.Controllers;

namespace Cosmos.HAL
{
    public static class Global
    {
        public static readonly Debugger mDebugger = new Debugger("HAL", "Global");

        //static public PIT PIT = new PIT();
        // Must be static init, other static inits rely on it not being null

        public static TextScreenBase TextScreen = new TextScreen();
        public static PCI Pci;

        static public void Init(TextScreenBase textScreen)
        {
            if (textScreen != null)
            {
                TextScreen = textScreen;
            }

            mDebugger.Send("Before Core.Global.Init");
            Core.Global.Init();

            //TODO Redo this - Global init should be other.
            // Move PCI detection to hardware? Or leave it in core? Is Core PC specific, or deeper?
            // If we let hardware do it, we need to protect it from being used by System.
            // Probably belongs in hardware, and core is more specific stuff like CPU, memory, etc.
            //Core.PCI.OnPCIDeviceFound = PCIDeviceFound;

            //TODO: Since this is FCL, its "common". Otherwise it should be
            // system level and not accessible from Core. Need to think about this
            // for the future.

            Console.WriteLine("Finding PCI Devices");
            mDebugger.Send("PCI Devices");
            PCI.Setup();

            Console.WriteLine("Starting ACPI");
            mDebugger.Send("ACPI Init");
            ACPI.Start();

            mDebugger.Send("Done initializing Cosmos.HAL.Global");
  
            // Currently ATA won't be initialized until we find a solution for the
            // Two Controllers initialize bug
            if (PCI.GetDeviceClass(0x01, 0x01) != null)
            {
                mDebugger.Send("ATA Primary Master");
                IDE ATA1 = new IDE(Ata.ControllerIdEnum.Primary, Ata.BusPositionEnum.Master);
                mDebugger.Send("ATA Secondary Master");
                IDE ATA2 = new IDE(Ata.ControllerIdEnum.Secondary, Ata.BusPositionEnum.Master);
                //InitAta(BlockDevice.Ata.ControllerIdEnum.Secondary, BlockDevice.Ata.BusPositionEnum.Slave);
            }
            if (PCI.GetDeviceClass(0x01, 0x06) != null)
            {
                Console.WriteLine("Initializing AHCI Controller");
                AHCI xAHCI = new AHCI(PCI.GetDeviceClass(0x01, 0x06).BaseAddressBar[5].BaseAddress);
            }
            else if (PCI.GetDeviceClass(0x01, 0x01) == null)
            {
                Console.Write("Booting without ATA Initialization");
            }

        }

        public static void EnableInterrupts()
        {
            CPU.EnableInterrupts();
        }

        public static bool InterruptsEnabled => CPU.mInterruptsEnabled;
    }
}
