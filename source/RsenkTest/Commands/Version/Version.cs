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
            parameters = new List<ParameterBase>();

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
            throw new NotImplementedException();
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
