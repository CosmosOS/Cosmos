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
            short num = 5; // TODO: Random Number Generator
            short guess = short.Parse(System.Console.ReadLine());

            while (guess != num)
            {
                if (guess > num)
                    System.Console.WriteLine("Too High.");
                else
                    System.Console.WriteLine("Too Low.");
                guess = short.Parse(System.Console.ReadLine());
            }

            System.Console.WriteLine("You Got It!!!!");
        }

        public override void Help()
        {
            System.Console.WriteLine(Name);
            System.Console.Write("  "); System.Console.WriteLine(Summary);
        }
    }
}
