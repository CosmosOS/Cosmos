using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Shell.Console.Commands
{
    public class KudzuCommand : CommandBase
    {
        public override string Name
        {
            get { return "kudzu"; }
        }

        public override string Summary
        {
            get { return "Runs Chad's tests."; }
        }

        public override void Execute(string param)
        {
            Kernel.Temp.Kudzu.PCI.Test();
        }

        public override void Help()
        {
            System.Console.WriteLine(Name);
            System.Console.Write(" ");
            System.Console.WriteLine(Summary);
        }
    }
}
