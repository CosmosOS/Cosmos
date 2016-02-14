using System;
using NUnit.Framework;

namespace DebugCompiler
{
    [TestFixture]
    public class RunKernels
    {
        [Test]
        public void Test([ValueSource(typeof(MySource), "ProvideData")] Type kernelToRun)
        {
            Assert.IsNotNull(kernelToRun);
            Assert.Fail("Cannot run kernel '" + kernelToRun.FullName + "'!");
        }
    }
}
