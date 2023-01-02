using System;

using Sys = Cosmos.System;

namespace Guess
{
    public class GuessOS : Sys.Kernel
    {
        private Random _Random;
        private int _Count;
        private int _Number;
        
        protected override void BeforeRun()
        {
            _Random = new Random(); // random works

            _Count = 0;
            _Number = _Random.Next(1, 101);

            Console.Clear();

            Console.WriteLine("Guess Demo");
            Console.WriteLine("----------");
            Console.WriteLine();
            Console.WriteLine("Please guess a number from 1 to 100.");
            Console.WriteLine();
        }

        protected override void Run()
        {
            _Count++;
            
            Console.Write($"Guess #{_Count}: ");
            
            var xGuess = Int32.Parse(Console.ReadLine());
            mDebugger.Send($"Guess#{_Count}: {xGuess}");

            if (xGuess < _Number)
            {
                Console.WriteLine("Too low.");
            }
            else if (xGuess > _Number)
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

                _Number = _Random.Next(1, 100);
                _Count = 0;
            }
        }

        protected override void AfterRun()
        {
            Sys.Power.Shutdown();
        }
    }
}
