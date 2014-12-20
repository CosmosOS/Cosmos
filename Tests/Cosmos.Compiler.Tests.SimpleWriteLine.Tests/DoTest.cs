using System;
using System.IO;
using ApprovalTests;
using ApprovalTests.Reporters;
using Cosmos.Compiler.TestsBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmos.Compiler.Tests.SimpleWriteLine.Tests
{
    [TestClass]
    //[UseReporter(typeof(VisualStudioReporter))]
    public class DoTest: BaseTest
    {
        [TestMethod]
        public void Test()
        {
            // these files contain the output to be verified. don't clean them.
            var xOutputFile = Path.GetTempFileName();
            var xLogFile = Path.GetTempFileName();
            
            var xRunner = new CompilerRunner();
            
            xRunner.References.Add(typeof(Kernel.Kernel).Assembly.Location);
            xRunner.References.Add(typeof(Cosmos.System.Plugs.System.TypeImpl).Assembly.Location);
            xRunner.References.Add(typeof(Cosmos.Core.Plugs.CPUImpl).Assembly.Location);
            xRunner.References.Add(typeof(Cosmos.Debug.Kernel.Plugs.DebugBreak).Assembly.Location);

            xRunner.AssemblerLogFile = xLogFile;
            xRunner.OutputFile = xOutputFile;
            xRunner.Execute();
            
            Verify("Output", File.ReadAllText(xOutputFile));
            Verify("Logfile", File.ReadAllText(xLogFile));
        }
    }
}