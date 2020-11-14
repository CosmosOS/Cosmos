using System;
using System.Collections.Generic;
using System.IO;

using NUnit.Framework;

using Cosmos.TestRunner.Core;
using Cosmos.TestRunner.Full;

namespace Cosmos.TestRunner.UnitTest
{
    using Assert = NUnit.Framework.Assert;

    [TestFixture]
    public class KernelTests
    {
        private static IEnumerable<Type> KernelsToRun => TestKernelSets.GetStableKernelTypes();

        [TestCaseSource(nameof(KernelsToRun))]
        public void TestKernel(Type aKernelType)
        {
            try
            {
                Directory.SetCurrentDirectory(Path.GetDirectoryName(typeof(KernelTests).Assembly.Location));

                var xEngine = new FullEngine(new EngineConfiguration(aKernelType));
                xEngine.SetOutputHandler(new TestOutputHandler());

                Assert.IsTrue(xEngine.Execute().KernelTestResults[0].Result);
            }
            catch (AssertionException)
            {
                throw;
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception occurred: " + e.ToString());
                Assert.Fail();
            }
        }

        private class TestOutputHandler : OutputHandlerFullTextBase
        {
            protected override void Log(string message)
            {
                TestContext.WriteLine(String.Concat(DateTime.Now.ToString("hh:mm:ss.ffffff "), new string(' ', mLogLevel * 2), message));
            }
        }
    }
}
