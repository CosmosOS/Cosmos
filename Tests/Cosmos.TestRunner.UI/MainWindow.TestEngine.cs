using System;
using System.Collections.Generic;
using System.Threading;
using Cosmos.TestRunner.Core;

namespace Cosmos.TestRunner.UI
{
    public partial class MainWindow
    {
        private ParameterizedThreadStart tEngineThreadStart = null;
        private Thread TestEngineThread = null;
        private void RunTestEngine()
        {
            tEngineThreadStart = new ParameterizedThreadStart(TestEngineThreadMain);
            TestEngineThread = new Thread(tEngineThreadStart);
            TestEngineThread.Start((OutputHandlerBase)this);
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
