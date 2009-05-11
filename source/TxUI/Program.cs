using System;
using Cosmos.Compiler.Builder;
using Cosmos.Hardware;

namespace Cosmos.Playground.Xenni
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            BuildUI.Run();
        }

        // Main entry point of the kernel
        public static void Init()
        {
            new Cosmos.Sys.Boot().Execute();

            PIT.T2Frequency = 100;
            PIT.RegisterTimer(new PIT.PITTimer(WriteA, 2000000000, true));
            PIT.RegisterTimer(new PIT.PITTimer(WriteB, 2000000000, 1000000000));

            PIT.T0RateGen = true;
            PIT.T0DelyNS = 1000000;

            while (true)
            {
                Kernel.CPU.Halt();
            }
        }
    }
}