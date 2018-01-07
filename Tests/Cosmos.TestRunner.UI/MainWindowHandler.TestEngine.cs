using System;
using System.Collections.Generic;
using System.Threading;
using Cosmos.TestRunner.Core;

namespace Cosmos.TestRunner.UI
{
    partial class MainWindowHandler
    {
        private Thread TestEngineThread = null;
        public OutputHandlerXml outputHandler;

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

            var xOutputXml = new OutputHandlerXml();
            xEngine.OutputHandler = new MultiplexingOutputHandler(
                xOutputXml,
                this);

            xEngine.Execute();

            outputHandler = xOutputXml;

            TestFinished();
        }
    }
}
