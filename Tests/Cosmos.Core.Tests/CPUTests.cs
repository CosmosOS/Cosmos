using NUnit.Framework;
using Cosmos.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Core.Tests
{
    [TestFixture()]
    public class CPUTests
    {
        [Test()]
        public void EstimateCPUSpeedFromNameTest()
        {
            Assert.AreEqual((long)2.8e9, CPU.EstimateCPUSpeedFromName("                Intel(R) Celeron(R) CPU 2.80GHz"));
            Assert.AreEqual((long)2.8e9, CPU.EstimateCPUSpeedFromName("Intel(R) Celeron(R) CPU 2.80GHz"));
        }
    }
}
