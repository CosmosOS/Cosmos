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
            throw new NotImplementedException();
        }

        public override void Execute()
        {
            Prompter.PrintMessage("Commander 0.0.0.1 alpha\n");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        [Obsolete("This method was inherited from CommandBase and should not be used. User Execute() instead.")]
        public override void Execute(params ParameterBase[] args)
        {
            throw new NotImplementedException();
        }
    }
}
