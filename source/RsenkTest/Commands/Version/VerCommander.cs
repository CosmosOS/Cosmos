using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RsenkTest.Commands.Version
{
    class VerCommander : ParameterBase
    {
        public override string Name
        {
            get { return "commander"; }
        }

        public override string Summary
        {
            get { return "Gets the current version number of the Cosmos Commander Shell."; }
        }

        public override void Help()
        {
            Prompter.PrintMessage(""); //TODO: Finish this method
        }

        public override void Execute()
        {
            Prompter.PrintMessage("Commander 0.0.0.1 alpha\n");
        }

        public override void Execute(params ParameterBase[] args)
        {
            throw new NotImplementedException();
        }
    }
}
