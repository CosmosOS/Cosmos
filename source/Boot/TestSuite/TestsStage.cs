using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Kernel;
using Cosmos.Kernel.Staging;

namespace TestSuite
{
    public class TestsStage : StageBase
    {
        private List<TestBase> _tests = new List<TestBase>();

        public override string Name
        {
            get
            {
                return "Tests";
            }
        }

        public override void Initialize()
        {
            _tests.Add(new Tests.MathTest());
            _tests.Add(new Tests.StringTest());
            _tests.Add(new Tests.ParseTest());
            Execute();
        }

        public override void Teardown()
        {
            //_tests.Clear();
        }

        public void Execute()
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
    }
}
