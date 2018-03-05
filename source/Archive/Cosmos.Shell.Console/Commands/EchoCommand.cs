using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Shell.Console.Commands
{
    public class EchoCommand : CommandBase
    {
        public override string Name
        {
            get { return "echo"; }
        }

        public override string Summary
        {
            get { return "Duplicates text you enter to the console."; }
        }

        public override void Execute(string param)
        {
            System.Console.WriteLine(param);
            System.Console.WriteLine();
        }

        public override void Help()
        {
            System.Console.WriteLine("echo [text]");
            System.Console.WriteLine("  Duplicates text you enter to the console.");
            System.Console.WriteLine("  [text]: The text to duplicate.");
        }
    }
}
