using System;
using Cosmos.TestRunner.Core;
using NUnit.Framework;

namespace DebugCompiler
{
    [TestFixture]
    public class RunKernels
    {
        [Test]
        public void Test([ValueSource(typeof(MySource), nameof(MySource.ProvideData))] Type kernelToRun)
        {
            var xEngine = new Engine();
            DefaultEngineConfiguration.Apply(xEngine);
            xEngine.KernelsToRun.Clear();

            xEngine.KernelsToRun.Add(kernelToRun.Assembly.Location);
            xEngine.OutputHandler = new TestOutputHandler();


            Assert.IsNotNull(kernelToRun);
            Assert.Fail("Cannot run kernel '" + kernelToRun.FullName + "'!");
        }

        private class TestOutputHandler: OutputHandlerFullTextBase
        {
            protected override void Log(string message)
            {
                TestContext.WriteLine(String.Concat(DateTime.Now.ToString("hh:mm:ss.ffffff "), new String(' ', mLogLevel * 2), message));
            }
        }
    }
}
