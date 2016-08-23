using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Cosmos.Debug.Kernel;
using Sys = Cosmos.System;

namespace GuessKernel
{
    public class GuessOS : Sys.Kernel
    {
        protected int mCount = 0;

        protected int mMagicNo = 22;

        public GuessOS()
        {
            // Didnt check if tickcount is working yet.. can change this later
            //var xRandom = new Random(234243534);
            //mMagicNo = xRandom.Next(1, 100);
        }

        protected override void BeforeRun()
        {
            //Cosmos.Core.HMI.Init();
            Console.Clear();

            Console.WriteLine("Guess Demo");
            Console.WriteLine("Please guess a number from 1 to 100.");
        }

        protected override void Run()
        {
            mCount++;

            mDebugger.Send("");
            mDebugger.SendMessage("Kernel", "New iteration");
            Console.WriteLine();
            Console.WriteLine("Guess #" + mCount);
            Console.Write("Please enter a guess: ");
            string xInputStr = Console.ReadLine();
            Console.Write("Input length: ");
            Console.WriteLine(xInputStr.Length.ToString());
            int xGuess = int.Parse(xInputStr);
            Console.WriteLine("Your guess was " + xGuess);
            if (xGuess < mMagicNo)
            {
                Console.WriteLine("Too low.");
            }
            else if (xGuess > mMagicNo)
            {
                Console.WriteLine("Too high.");
            }
            else
            {
                Console.WriteLine("You guessed it!");
                Stop();
            }
        }
    }
}
