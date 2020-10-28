using System;
using System.Collections.Generic;
using Cosmos.Common.Extensions;
using Cosmos.Core;
using Cosmos.Debug.Kernel;
using Cosmos.HAL.BlockDevice;
using Cosmos.HAL.USB;

namespace Cosmos.HAL
{
    public static class Global
    {
        public static readonly Debugger mDebugger = new Debugger("HAL", "Global");

        public static PIT PIT = new PIT();
        // Must be static init, other static inits rely on it not being null

        public static TextScreenBase TextScreen = new TextScreen();
        public static PCI Pci;

        public static readonly PS2Controller PS2Controller = new PS2Controller();
        public static USBHostUHCIRegisters uhci;
        public static PCIDevice aDevice;
        // TODO: continue adding exceptions to the list, as HAL and Core would be documented.
        /// <summary>
        /// Init <see cref="Global"/> inctanse.
        /// </summary>
        /// <param name="textScreen">Text screen.</param>
        /// <exception cref="System.IO.IOException">Thrown on IO error.</exception>
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
            Console.Clear();
            Console.WriteLine("Finding PCI Devices");
            mDebugger.Send("PCI Devices");
            PCI.Setup();

            Console.WriteLine("Starting ACPI");
            mDebugger.Send("ACPI Init");
            ACPI.Start();

            // http://wiki.osdev.org/%228042%22_PS/2_Controller#Initialising_the_PS.2F2_Controller
            // TODO: USB should be initialized before the PS/2 controller
            // TODO: ACPI should be used to check if a PS/2 controller exists
            try
            {
                Console.WriteLine("Starting USB OHCI");
                USBHost.ScanDevices();
                Console.WriteLine("(hex)BAR4:" + aDevice.BAR4.ToHex());
                Console.WriteLine("(uint)BAR4:" + aDevice.BAR4);
                Console.WriteLine("(string)BAR4:" + aDevice.BAR4.ToString());
                var val = uhci.USBCMD;
                uhci.USBCMD = 1;
                Console.WriteLine("USBCMD Before: " + val);
                Console.WriteLine("USBCMD After: " + uhci.USBCMD);
                Console.WriteLine("FRBASEADD: " + uhci.FRBASEADD);
                Console.WriteLine("FRNUM: " + uhci.FRNUM);
                Console.WriteLine("POSTSC1: " + uhci.PORTSC1);
                Console.WriteLine("POSTSC2: " + uhci.PORTSC2);
                Console.WriteLine("SOFMOD: " + uhci.SOFMOD);
                Console.WriteLine("USBINTR: " + uhci.USBINTR);
                Console.WriteLine("USBSTS: " + uhci.USBSTS);


            }
            catch(Exception e)
            {
                Console.WriteLine("Message: ");
                Console.WriteLine(e.ToString());
            }
            mDebugger.Send("PS/2 Controller Init");
            PS2Controller.Initialize();

            IDE.InitDriver();
            AHCI.InitDriver();
            //EHCI.InitDriver();


            mDebugger.Send("Done initializing Cosmos.HAL.Global");

        }

        /// <summary>
        /// Enable interrupts.
        /// </summary>
        public static void EnableInterrupts()
        {
            CPU.EnableInterrupts();
        }

        /// <summary>
        /// Check if CPU interrupts are enabled.
        /// </summary>
        public static bool InterruptsEnabled => CPU.mInterruptsEnabled;

        /// <summary>
        /// Get keyboard devices.
        /// </summary>
        /// <returns>IEnumerable{KeyboardBase} value.</returns>
        public static IEnumerable<KeyboardBase> GetKeyboardDevices()
        {
            var xKeyboardDevices = new List<KeyboardBase>();

            if (PS2Controller.FirstDevice is KeyboardBase xKeyboard1)
            {
                xKeyboardDevices.Add(xKeyboard1);
            }

            if (PS2Controller.SecondDevice is KeyboardBase xKeyboard2)
            {
                xKeyboardDevices.Add(xKeyboard2);
            }

            return xKeyboardDevices;
        }

        /// <summary>
        /// Get mouse devices.
        /// </summary>
        /// <returns>IEnumerable{MouseBase} value.</returns>
        public static IEnumerable<MouseBase> GetMouseDevices()
        {
            var xMouseDevices = new List<MouseBase>();

            if (PS2Controller.FirstDevice is PS2Mouse xMouse1)
            {
                xMouseDevices.Add(xMouse1);
            }

            if (PS2Controller.SecondDevice is PS2Mouse xMouse2)
            {
                xMouseDevices.Add(xMouse2);
            }

            return xMouseDevices;
        }
    }
}
