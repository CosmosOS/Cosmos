using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Shell.Console.Commands
{
    public class BreakCommand : CommandBase
    {
        public override string Name
        {
            get { return "break"; }
        }

        public override string Summary
        {
            get { return "If a debugger is attached, causes a break."; }
        }

        public override void Execute(string param)
        {
            System.Diagnostics.Debugger.Break();
        }

        public override void Help()
        {
            System.Console.WriteLine(Name);
            System.Console.Write(" "); System.Console.WriteLine(Summary);
        }
    }
}
