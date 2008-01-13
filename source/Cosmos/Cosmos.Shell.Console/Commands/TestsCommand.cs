using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Kernel;

namespace Cosmos.Shell.Console.Commands
{
    public class TestsCommand : CommandBase
    {
        private List<Tests.TestBase> _tests = new List<Cosmos.Shell.Console.Tests.TestBase>();

        public TestsCommand()
        {
            _tests.Add(new Tests.MathTest());
            _tests.Add(new Tests.StringTest());
            _tests.Add(new Tests.ParseTest());
        }

        public override string Name
        {
            get { return "tests"; }
        }

        public override string Summary
        {
            get { return "Runs the kernel test cases."; }
        }

        public override void Execute(string param)
        {
            for (int i = 0; i < _tests.Count; i++)
            {
                DebugUtil.SendTestCase(_tests[i].Name);
                System.Console.ForegroundColor = ConsoleColor.Yellow;
                System.Console.Write("TEST CASE: ");
                System.Console.ForegroundColor = ConsoleColor.Blue;
                System.Console.WriteLine(_tests[i].Name);
                System.Console.ForegroundColor = ConsoleColor.Yellow;
                System.Console.WriteLine("-------------------------");

                System.Console.ForegroundColor = ConsoleColor.White;
                // Run the test.
                _tests[i].Initialize();
                _tests[i].Test();
                _tests[i].Teardown();
            }
        }

        public override void Help()
        {
            System.Console.WriteLine(Name);
            System.Console.Write("  "); System.Console.WriteLine(Summary);
        }
    }
}
