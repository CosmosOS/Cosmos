using System;
using System.Collections.Generic;
using System.Threading;
using Cosmos.TestRunner.Core;

namespace Cosmos.TestRunner.UI
{
    public partial class MainWindowHandler
    {
        private ParameterizedThreadStart tEngineThreadStart = null;
        private Thread TestEngineThread = null;
        public void RunTestEngine()
        {
            tEngineThreadStart = new ParameterizedThreadStart(TestEngineThreadMain);
            TestEngineThread = new Thread(tEngineThreadStart);
            TestEngineThread.Start(this);
        }

        private void TestEngineThreadMain(object refrence)
        {
            var xEngine = new Engine();

            DefaultEngineConfiguration.Apply(xEngine);

            var xOutputXml = new OutputHandlerXml();
            xEngine.OutputHandler = new MultiplexingOutputHandler(
                xOutputXml,
                (OutputHandlerBase)refrence);

            xEngine.Execute();
        }
    }
}
