using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos.Compiler.Tests.Interfaces.Kernel;
using Cosmos.TestRunner.Core;

namespace Cosmos.TestRunner.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var xEngine = new Engine();
            xEngine.AddKernel(typeof(Kernel).Assembly.Location);
            xEngine.Execute();
        }
    }
}
