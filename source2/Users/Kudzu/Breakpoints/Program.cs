using System;
using System.Collections.Generic;
using System.Text;

namespace BreakpointsKernel
{
    public class Kernel
    {
        public static void Boot()
        {
            Cosmos.Sys.Boot xBoot = new Cosmos.Sys.Boot();
            xBoot.Execute();

            Console.WriteLine("Test");
            Console.WriteLine("3 Cosmos booted successfully. Type a line of text to get it echoed back.");
            Console.WriteLine("Test");
            while (true)
            {
                Console.Write("Input: ");
                string xResult = Console.ReadLine();
                Console.Write("Text typed: ");
                Console.WriteLine(xResult);
                //Cosmos.Debug.Debugger.Send(xResult);
            }
        }
    }
}
