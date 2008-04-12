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
                System.Console.Write("Creating StageQueue");
                Cosmos.Kernel.Staging.DefaultStageQueue stages = new Cosmos.Kernel.Staging.DefaultStageQueue();
                System.Console.WriteLine("                                      [ done ]");

                System.Console.WriteLine("Queueing Shell");
                stages.Enqueue(new CommanderShell());
                System.Console.WriteLine("                                      [ done ]");

                stages.Run();
                stages.Teardown();
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