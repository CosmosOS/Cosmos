using System;
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
            //Console.WriteLine("Started");
            ////Console.ForegroundColor = ConsoleColor.Red;
            ////Console.BackgroundColor=ConsoleColor.DarkYellow;
            ////Console.WriteLine("Rood op donker geel!");
            ////Console.ReadLine();
            ////Console.WriteLine("Done");

            //try
            //{
            //    Console.Write("");
            //}
            //catch(Exception E)
            //{
            //    Console.WriteLine("Error occurred!");
            //    Console.Write("Error: ");
            //    Console.WriteLine(E.Message);
            //}
            //Console.WriteLine("Done.");
            //while (true)
            //    ;
            Array1();
            Array2();
            Array4();
            Array8();
        }

        private void Array1()
        {
            var xArray = new byte[4];
            xArray[1] = 4;
            var xTest = xArray[2];
        }

        private void Array2()
        {
            var xArray = new short[4];
            xArray[1] = 4;
            var xTest = xArray[2];
        }

        private void Array4()
        {
            var xArray = new int[4];
            xArray[1] = 4;
            var xTest = xArray[2];
        }

        private void Array8()
        {
            var xArray = new ulong[4];
            xArray[1] = 0x0102030405060708;
            var xTest = xArray[2];
        }
    }
}
