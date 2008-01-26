using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Build.Windows;

namespace Cosmos.Shell.Guess {
    public class Program {

        #region Build Console
        // This contains code to launch the build console. Most users should not chagne this.
        [STAThread]
        public static void Main()
        {
            var xBuilder = new Builder();
            xBuilder.Build();
        }
        #endregion

        public static void Init() {
            Kernel.Boot.Default();

            Kernel.Staging.DefaultStageQueue stages = new Cosmos.Kernel.Staging.DefaultStageQueue();
            
            System.Console.Clear();
            System.Console.BackgroundColor = ConsoleColor.Black;
            System.Console.ForegroundColor = ConsoleColor.Red;
            System.Console.WriteLine("Cosmos Kernel. Copyright 2007-2008 The Cosmos Project.");
            System.Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
            System.Console.ForegroundColor = ConsoleColor.Green;
            System.Console.WriteLine("Now Booting...");

            System.Console.ForegroundColor = ConsoleColor.White;
            System.Console.WriteLine("Success.");
            Kernel.CPU.PrintTime();


            stages.Run();

            Random mt = new Random();
            short num = mt.Next();

            System.Console.WriteLine("I am thinking of a number between 0 and 255. What is it?");
            System.Console.ForegroundColor = ConsoleColor.Blue;
            System.Console.Write("Take a guess: ");
            System.Console.ForegroundColor = ConsoleColor.White;
            short guess = short.Parse(System.Console.ReadLine());

            while (guess != num)
            {
                System.Console.ForegroundColor = ConsoleColor.Red;
                if (guess > num)
                    System.Console.WriteLine("Too high.");
                else
                    System.Console.WriteLine("Too low.");

                System.Console.ForegroundColor = ConsoleColor.Blue;
                System.Console.Write("Take another guess: ");
                System.Console.ForegroundColor = ConsoleColor.White;
                guess = short.Parse(System.Console.ReadLine());
            }

            System.Console.WriteLine("You got it!!!!");


            System.Console.WriteLine("Done");

            stages.Teardown();
        }
    }
}
