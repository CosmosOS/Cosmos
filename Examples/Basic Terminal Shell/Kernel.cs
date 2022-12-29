using System;
using Cosmos.Core;
using Sys = Cosmos.System;

namespace BasicTerminalShell
{
    public class Kernel : Sys.Kernel
    {
        // prompt variable
        private string _Prompt;

        protected override void BeforeRun()
        {
            Console.WriteLine("Cosmos booted successfully. Type a command to get it executed!");
            _Prompt = "";
        }

        protected override void Run()
        {
            Console.Write($"{_Prompt}> ");
            var input = Console.ReadLine();
            string[] words = input.Split(' ');
            switch (words[0])
            {
                case "cpu":
                    Console.WriteLine($"Vendor: {CPU.GetCPUVendorName()}, Name: {CPU.GetCPUBrandString()}, Frequency: {CPU.GetCPUCycleSpeed()}");
                    break;
                case "shutdown":
                    Sys.Power.Shutdown(); // shutdown is supported
                    break;
                case "restart":
                    Sys.Power.Reboot(); // restart too
                    break;
                case "help":
                    // console methods are plugged
                    Console.WriteLine("cpu      - prints info about current cpu");
                    Console.WriteLine("shutdown - shuts down current computer");
                    Console.WriteLine("restart  - restarts current computer");
                    Console.WriteLine("help     - shows this help menu");
                    break;
                default:
                    // switch operator wokrs great
                    Console.WriteLine($"\"{words[0]}\" is not a command");
                    break;
            }
            // overloading works too
            Console.WriteLine();
        }
    }
}
