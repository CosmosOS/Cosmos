using System;
using Cosmos.Core;
using Cosmos.Debug.Kernel;
using Cosmos.HAL.BlockDevice;

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
          
            IDE.InitDriver();
            AHCI.InitDriver();
            //EHCI.InitDriver();

            mDebugger.Send("Done initializing Cosmos.HAL.Global");

        }

        public static void EnableInterrupts()
        {
            CPU.EnableInterrupts();
        }

        public static bool InterruptsEnabled => CPU.mInterruptsEnabled;
    }
}
