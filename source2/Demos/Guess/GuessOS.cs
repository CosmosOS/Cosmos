using System;
using System.Collections.Generic;
using System.Text;
using Sys = Cosmos.System;

namespace GuessKernel {
    public class GuessOS : Sys.Kernel {
        protected override void BeforeRun() {
            Console.WriteLine("Guess Demo");
        }

        protected int xCount = 0;
        protected int xMagicNo = 22;

        protected override void Run() {
            xCount++;
            Console.WriteLine();
            Console.WriteLine("Guess #" + xCount);
            Console.Write("Please enter a guess: ");
            string xInputStr = Console.ReadLine();
            int xGuess = int.Parse(xInputStr);
            Console.WriteLine("Your guess was " + xGuess);
            if (xGuess < xMagicNo) {
                Console.WriteLine("Too low.");
            } else if (xGuess > xMagicNo) {
                Console.WriteLine("Too high.");
            } else {
                Console.WriteLine("You guessed it!");
                Stop();
            }
        }
    }
}