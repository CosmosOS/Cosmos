using System;
using Cosmos.HAL;
using Sys = Cosmos.System;

namespace Playground
{
    public class Kernel : Sys.Kernel
    {
        protected override void BeforeRun()
        {
            Console.WriteLine("Cosmos booted successfully. Type a line of text to get it echoed back.");
        }



        protected override void Run()
        {
            Console.WriteLine("Started");
            //Console.ForegroundColor = ConsoleColor.Red;
            //Console.BackgroundColor=ConsoleColor.DarkYellow;
            //Console.WriteLine("Rood op donker geel!");
            //Console.ReadLine();
            //Console.WriteLine("Done");

            try
            {
                Console.Write("");
            }
            catch(Exception E)
            {
                Console.WriteLine("Error occurred!");
                Console.Write("Error: ");
                Console.WriteLine(E.Message);
            }
            Console.WriteLine("Done.");
            while (true)
                ;

        }
    }
}
