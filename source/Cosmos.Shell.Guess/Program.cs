using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Compiler.Builder;
using Cosmos.Debug;

namespace Cosmos.Demo.Guess {
	public class Program {

		#region Build Console
		// This contains code to launch the build console. Most users should not chagne this.
		[STAThread]
		public static void Main() {
			BuildUI.Run();
		}
		#endregion

        public static void Init() {
            var xBoot = new Cosmos.Sys.Boot();
            xBoot.Execute();
            Random xRandom = new Random((int)(Cosmos.Hardware.Global.TickCount
                + Cosmos.Hardware.RTC.GetSeconds()));
            // Divide by 100, get remainder
            Console.WriteLine("I am thinking of a number between 0 and 100. What is it?");
            int xMagicNo = xRandom.Next() % 100;
            Debugger.Send("Magic Number is " + xMagicNo);
            while (true) {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.Write("Take a guess: ");
                Console.ForegroundColor = ConsoleColor.White;
                short xGuess = short.Parse(Console.ReadLine());
                if (xGuess == xMagicNo) {
                    break;
                }
                //Debugger.Send("Guessed number is " + xGuess);
                //Debugger.Break();
                Console.ForegroundColor = ConsoleColor.Red;
                if (xGuess > xMagicNo) {
                    Console.WriteLine("Too high.");
                } else {
                    Console.WriteLine("Too low.");
                }
            }
            Console.WriteLine("You got it!!!!");
        }
    }
}
