using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos.TestRunner.Core;

namespace Cosmos.TestRunner.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var xEngine = new Engine();

            DefaultEngineConfiguration.Apply(xEngine);

            //xEngine.OutputHandler = new OutputHandlerXml(@"c:\data\CosmosTests.xml");
            xEngine.OutputHandler = new OutputHandlerConsole();
            xEngine.Execute();
        }
    }
}
