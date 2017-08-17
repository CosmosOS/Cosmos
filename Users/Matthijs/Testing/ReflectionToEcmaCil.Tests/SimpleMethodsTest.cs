using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace ReflectionToEcmaCil.Tests
{
    [TestFixture]
    public class SimpleMethodsTestsTest : BaseTest
    {
        [Test]
        public void DoTest()
        {
            AssertCompilationSame("SimpleMethodsTests", typeof(SimpleMethodsTest.Program));
        }
    }
}
