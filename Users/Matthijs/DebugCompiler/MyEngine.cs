using System;
using Cosmos.Build.Common;
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
            // Sets the time before an error is registered. For example if set to 60 then if a kernel runs for more than 60 seconds then
            // that kernel will be marked as a failiure and terminated
            xEngine.AllowedSecondsInKernel = 1800;

            // If you want to test only specific platforms, add them to the list, like next line. By default, all platforms are run.
            xEngine.RunTargets.Add(RunTargetEnum.Bochs);

            // If you're working on the compiler (or other lower parts), you can choose to run the compiler in process
            // one thing to keep in mind though, is that this only works with 1 kernel at a time!
            xEngine.RunIL2CPUInProcess = false;
            xEngine.TraceAssembliesLevel = TraceAssemblies.User;
            xEngine.EnableStackCorruptionChecks = true;
            xEngine.StackCorruptionChecksLevel = StackCorruptionDetectionLevel.AllInstructions;

            // Select kernels to be tested by adding them to the engine
            xEngine.AddKernel(kernelToRun.Assembly.Location);

            xEngine.OutputHandler = new TestOutputHandler();

            Assert.IsTrue(xEngine.Execute());

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
