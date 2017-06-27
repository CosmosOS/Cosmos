using System;
using System.IO;
using System.Reflection;

using Cosmos.Build.Common;
using Cosmos.TestRunner.Core;

using NUnit.Framework;

namespace Cosmos.TestRunner.UnitTest
{
    using Assert = NUnit.Framework.Assert;

    [TestFixture]
    public class RunKernels
    {
        [TestCaseSource(typeof(MySource), nameof(MySource.ProvideData))]
        public void Test(Type kernelToRun)
        {
            try
            {
                Directory.SetCurrentDirectory(Path.GetDirectoryName(typeof(RunKernels).GetTypeInfo().Assembly.Location));

                var xEngine = new Engine();

                // Sets the time before an error is registered. For example if set to 60 then if a kernel runs for more than 60 seconds then
                // that kernel will be marked as a failure and terminated
                xEngine.AllowedSecondsInKernel = 1200;

                // If you want to test only specific platforms, add them to the list, like next line. By default, all platforms are run.
                xEngine.RunTargets.Add(RunTargetEnum.Bochs);

                //xEngine.StartBochsDebugGui = false;
                //xEngine.RunWithGDB = true;
                // If you're working on the compiler (or other lower parts), you can choose to run the compiler in process
                // one thing to keep in mind though, is that this only works with 1 kernel at a time!
                xEngine.DebugIL2CPU = false;
                xEngine.TraceAssembliesLevel = TraceAssemblies.User;

                xEngine.EnableStackCorruptionChecks = true;
                xEngine.StackCorruptionChecksLevel = StackCorruptionDetectionLevel.AllInstructions;

                // Select kernels to be tested by adding them to the engine
                xEngine.AddKernel(kernelToRun.GetTypeInfo().Assembly.Location);

                xEngine.OutputHandler = new TestOutputHandler();

                Assert.IsTrue(xEngine.Execute());
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
                TestContext.WriteLine(string.Concat(DateTime.Now.ToString("hh:mm:ss.ffffff "), new string(' ', mLogLevel * 2), message));
            }
        }
    }
}
