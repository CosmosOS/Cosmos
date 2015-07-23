using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos.TestRunner.Core;

namespace Cosmos.TestRunner.UI
{
    public partial class MainWindow
    {
        private void RunTestEngine()
        {
            var xEngine = new Engine();

            DefaultEngineConfiguration.Apply(xEngine);

            var xOutputXml = new OutputHandlerXml();
            xEngine.OutputHandler = new MultiplexingOutputHandler(
                xOutputXml,
                (OutputHandlerBase)this);

            xEngine.Execute();
        }
    }
}
