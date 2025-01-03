using System;
using System.Collections.Generic;
using System.Threading;
using Cosmos.Core;
using Cosmos.Debug.Kernel;
using Cosmos.HAL.BlockDevice;
using Cosmos.HAL.Network;
using static Cosmos.HAL.PIT;

namespace Cosmos.HAL
{
    public static class Global
    {
        public static readonly Debugger debugger = new("Global");

        public static PIT PIT;
        // Must be static init, other static inits rely on it not being null

        public static TextScreenBase TextScreen = new TextScreen();
        public static PCI Pci;

        public static PS2Controller PS2Controller;

        // TODO: continue adding exceptions to the list, as HAL and Core would be documented.
        /// <summary>
        /// Init <see cref="Global"/> instance.
        /// </summary>
        /// <param name="textScreen">Text screen.</param>
        /// <exception cref="System.IO.IOException">Thrown on IO error.</exception>
        static public void Init(TextScreenBase textScreen, bool InitScrollWheel, bool InitPS2, bool InitNetwork, bool IDEInit)
        {
            if (textScreen != null)
            {
                TextScreen = textScreen;
            }

            debugger.Send("Before Core.Global.Init");
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

            Console.WriteLine("Starting ACPI");
            debugger.Send("ACPI Init");
            ACPI.Start();

            Console.WriteLine("Finding PCI Devices");
            debugger.Send("PCI Devices");
            PCI.Setup();

            Console.WriteLine("Starting APIC");
            debugger.Send("Local APIC Init");
            LocalAPIC.Initialize();

            Console.WriteLine("Starting PIT");
            debugger.Send("PIT init");
            PIT = new();

            Console.WriteLine("Starting Processor Scheduler");
            debugger.Send("Processor Scheduler");
            Core.Processing.ProcessorScheduler.Initialize();

            debugger.Send("Local APIC Timer Init");
            ApicTimer.Initialize();
            ApicTimer.Start();

            Console.WriteLine("Starting PS/2");
            debugger.Send("PS/2 init");
            PS2Controller = new();

            // http://wiki.osdev.org/%228042%22_PS/2_Controller#Initialising_the_PS.2F2_Controller
            // TODO: USB should be initialized before the PS/2 controller
            // TODO: ACPI should be used to check if a PS/2 controller exists
            debugger.Send("PS/2 Controller Init");
            if (InitPS2)
            {
                PS2Controller.Initialize(InitScrollWheel);
            }
            else
            {
                debugger.Send("PS/2 Controller disabled in User Kernel");
            }
            if (IDEInit)
            {
                IDE.InitDriver();
            }
            else
            {
                debugger.Send("IDE Driver disabled in User Kernel");
            }
            AHCI.InitDriver();
            //EHCI.InitDriver();

            if (InitNetwork)
            {
                debugger.Send("Network Devices Init");
                NetworkInit.Init();
            }
            else
            {
                debugger.Send("Network Driver disabled in User Kernel");
            }
            Console.WriteLine("Enabling Serial Output on COM1");
            SerialPort.Enable(COMPort.COM1, BaudRate.BaudRate38400);
            debugger.Send("Done initializing Cosmos.HAL.Global");

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

        public static uint SpawnThread(ThreadStart aStart)
        {
            return Core.Processing.ProcessContext.StartContext("", aStart, Core.Processing.ProcessContext.Context_Type.THREAD);
        }

        public static uint SpawnThread(ParameterizedThreadStart aStart, object param)
        {
            return Core.Processing.ProcessContext.StartContext("", aStart, Core.Processing.ProcessContext.Context_Type.THREAD, param);
        }

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
