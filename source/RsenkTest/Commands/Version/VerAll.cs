using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RsenkTest.Commands.Version
{
    class VerAll : ParameterBase
    {
        public override string Name
        {
            get { return "all"; }
        }

        public override string Summary
        {
            get { return "Gets the version number of Cosmos and Commander"; }
        }

        public override void Help()
        {
            Prompter.PrintMessage(""); //TODO: Finish this method
        }

        public override void Execute()
        {
            Prompter.PrintMessage("About Commander:");
        }

        public override void Execute(params ParameterBase[] args)
        {
            throw new NotImplementedException();
        }
    }
}
