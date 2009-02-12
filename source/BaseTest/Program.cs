using System;
using Cosmos.Compiler.Builder;
using Cosmos.Sys;

namespace BaseTest
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
            Boot bt = new Boot();
            bt.Execute();
            Console.WriteLine("Welcome! This will be my playground to test out the various features i'll implement. :)");
            Deboot.ShutDown();
        }
    }
}