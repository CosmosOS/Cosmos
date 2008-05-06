using System;
using Cosmos.Build.Windows;

namespace RsenkTest
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
            try
            {
                Cosmos.Kernel.Boot.Default();
                System.Console.WriteLine("                                      [ done ]");

                System.Console.WriteLine("Queueing Shell");
                System.Console.WriteLine("                                      [ done ]");
            }
            catch (PrompterException e)
            {
                Prompter.PrintError(e.Message);
            }

            while (true)
                ;
        }
    }
}