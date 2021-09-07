using System;

using Sys = Cosmos.System;

namespace Guess
{
    public class GuessOS : Sys.Kernel
    {
        private Random mRandom;
        private int mCount;
        private int mNumber;
        
        protected override void BeforeRun()
        {
            mRandom = new Random();

            mCount = 0;
            mNumber = mRandom.Next(1, 101);

            Console.Clear();

            Console.WriteLine("Guess Demo");
            Console.WriteLine("----------");
            Console.WriteLine();
            Console.WriteLine("Please guess a number from 1 to 100.");
            Console.WriteLine();
        }

        protected override void Run()
        {
            mCount++;
            
            Console.Write($"Guess #{mCount}: ");
            
            var xGuess = Int32.Parse(Console.ReadLine());
            mDebugger.Send($"Guess#{mCount}: {xGuess}");

            if (xGuess < mNumber)
            {
                Console.WriteLine("Too low.");
            }
            else if (xGuess > mNumber)
            {
                Console.WriteLine("Too high.");
            }
            else
            {
                Console.WriteLine("You guessed it!");
                Console.WriteLine("Press s to stop or any other key to play again. Thanks for playing!");

                if (Console.ReadKey(true).Key == ConsoleKey.S)
                {
                    Stop();
                }

                mNumber = mRandom.Next(1, 100);
                mCount = 0;
            }
        }

        protected override void AfterRun()
        {
            Sys.Power.Shutdown();
        }
    }
}
