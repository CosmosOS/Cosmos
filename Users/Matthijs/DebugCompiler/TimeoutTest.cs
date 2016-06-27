using System.Threading;
using NUnit.Framework;

namespace DebugCompiler
{
    [TestFixture]
    public class TimeoutTest
    {
        [Test]
        public void Do()
        {
            Assert.IsTrue(true);
            Thread.Sleep(10 * 60 * 1000);
            Assert.IsTrue(true);

        }
    }
}
