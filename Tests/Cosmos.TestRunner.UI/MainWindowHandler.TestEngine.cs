using System.Threading;

using Cosmos.TestRunner.Core;

namespace Cosmos.TestRunner.UI
{
    partial class MainWindowHandler
    {
        public ITestResult TestResult { get; private set; }

        private Thread TestEngineThread = null;

        public delegate void TestFinishedEventHandler();
        public TestFinishedEventHandler TestFinished = delegate { };

        public void RunTestEngine()
        {
            TestEngineThread = new Thread(TestEngineThreadMain);
            TestEngineThread.Start();
        }

        private void TestEngineThreadMain()
        {
            var xEngine = new Engine(new DefaultEngineConfiguration());
            xEngine.SetOutputHandler(this);

            TestResult = xEngine.Execute();

            TestFinished();
        }
    }
}
