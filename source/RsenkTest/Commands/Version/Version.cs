using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RsenkTest.Commands.Version
{
    class Version : CommandBase
    {
        public Version()
        {
            parameters = new List<CommandBase>();

            parameters.Add(new VerAll());
            parameters.Add(new VerCommander());
        }

        public override string Name
        {
            get { return "ver"; }
        }

        public override string Summary
        {
            get { return "Gets the version number."; }
        }

        public override void Help()
        {
            Prompter.PrintMessage("Usage: ver [arg].");
            Prompter.PrintMessage("Valid args: ");

            for (int x = 0; x < parameters.Count; x++)
            {
                Prompter.PrintMessage("  " + parameters[x].Name);
            }

            Prompter.PrintMessage("");
        }

        public override void Execute(params ParameterBase[] args)
        {
            foreach (ParameterBase param in args)
            {
                param.Execute();
            }
        }
    }
}
