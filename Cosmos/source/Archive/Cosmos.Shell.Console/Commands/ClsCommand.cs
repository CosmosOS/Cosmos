using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Shell.Console.Commands
{
    public class ClsCommand : CommandBase
    {
        public override string Name
        {
            get { return "cls"; }
        }

        public override string Summary
        {
            get { return "Clears the screen."; }
        }

        public override void Execute(string param)
        {
            System.Console.Clear();
        }

        public override void Help()
        {
            System.Console.WriteLine("cls");
            System.Console.WriteLine(" Clears the screen.");
        }
    }
}
