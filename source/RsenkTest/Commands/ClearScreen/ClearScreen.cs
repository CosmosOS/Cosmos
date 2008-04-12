using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RsenkTest.Commands.ClearScreen
{
    class ClearScreen : CommandBase
    {
        public override string Name
        {
            get { return "cls"; }
        }

        public override string Summary
        {
            get { return "Clears the screen and displays the welcome message."; }
        }

        public override void Help()
        {
            throw new NotImplementedException();
        }

        public override void Execute(params ParameterBase[] args)
        {
            if (args.Length > 0)
            {
                Prompter.PrintError("The command '" + Name + "' does not take any parameters.");
            }
            else
            {
                Console.Clear();
                Prompter.PrintWelcome("");
            }
        }
    }
}
