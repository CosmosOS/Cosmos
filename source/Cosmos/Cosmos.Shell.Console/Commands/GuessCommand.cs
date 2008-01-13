using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Shell.Console.Commands
{
    public class GuessCommand : CommandBase
    {
        public override string Name
        {
            get { return "guess"; }
        }

        public override string Summary
        {
            get { return "Plays a game of guess."; }
        }

        public override void Execute(string param)
        {
            MT mt = new MT();
            int n = mt.Next();
            if (n <= 0)
                n = -n;

            n /= (Int32.MaxValue / Int16.MaxValue);
            n /= (Int16.MaxValue / 20);

            System.Console.WriteLine("I am thinking of a number between 0 and 20. What is it?");
            short num = (short)n;
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
        }

        public override void Help()
        {
            System.Console.WriteLine(Name);
            System.Console.Write("  "); System.Console.WriteLine(Summary);
        }
    }
}
