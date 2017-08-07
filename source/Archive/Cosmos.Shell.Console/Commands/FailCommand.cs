using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Shell.Console.Commands
{
    public class FailCommand : CommandBase
    {
        public override string Name
        {
            get { return "fail"; }
        }

        public override string Summary
        {
            get { return "Causes a kernel-level exception (RSOD)."; }
        }

        public override void Execute(string param)
        {

            throw new Exception("This is a kernel-level Exception!");
        }

        public override void Help()
        {
            System.Console.WriteLine("fail");
            System.Console.WriteLine(" Causes a kernel-level exception (RSOD).");
        }
    }
}
