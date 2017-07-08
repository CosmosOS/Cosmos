using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Shell.Console.Commands
{
    public class ExitCommand : CommandBase
    {
        public override string Name
        {
            get { return "exit"; }
        }

        public override string Summary
        {
            get { return "Closes the console."; }
        }

        public delegate void SimpleDelegate();

        private SimpleDelegate _exit;

        public ExitCommand(SimpleDelegate exit)
        {
            _exit = exit;
        }

        public override void Execute(string param)
        {
            _exit();
        }

        public override void Help()
        {
            System.Console.WriteLine("exit");
            System.Console.WriteLine(" Closes the console.");
        }
    }
}
