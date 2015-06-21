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

            xEngine.AddKernel(typeof(Cosmos.Compiler.Tests.SimpleWriteLine.Kernel.Kernel).Assembly.Location);
            xEngine.AddKernel(typeof(Cosmos.Compiler.Tests.Interfaces.Kernel.Kernel).Assembly.Location);

            xEngine.OutputHandler = new OutputHandlerXml(@"c:\data\CosmosTests.xml");
            xEngine.Execute();
        }
    }
}
