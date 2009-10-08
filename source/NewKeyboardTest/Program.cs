using System;
using Cosmos.Compiler.Builder;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using Cosmos.Hardware;
using Cosmos.Kernel;

namespace Cosmos.Playground.Xenni.KeyboardTest
{
    class Program
    {
        #region Cosmos Builder logic
        // Most users wont touch this. This will call the Cosmos Build tool
        [STAThread]
        static void Main(string[] args)
        {
            BuildUI.Run();
        }
        #endregion

        // Main entry point of the kernel
        public static void Init()
        {
            new Cosmos.Sys.Boot().Execute();

            uint scancode = 0;

            while (true)
            {
                CPU.Halt();

                while (Keyboard.GetScancode(out scancode))
                {
                    if (scancode == 0)
                    {
                        continue;
                    }
                    Keyboard.KeyMapping mapping = null;

                    if (Keyboard.GetKeyMapping(scancode, out mapping))
                    {
                        Console.WriteLine(scancode.ToHex() + mapping.Value + mapping.Key.ToString());
                    }
                    else
                    {
                        Console.WriteLine(scancode.ToHex());
                    }
                }
            }
        }
    }
}