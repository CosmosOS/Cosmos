using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.IL2CPU.Assembler.X86;
using NUnit.Framework;

namespace Indy.IL2CPU.Tests.AssemblerTests.X86 {
    [TestFixture]
    public class HaltTests: BaseTest {
        [Test]
        public void TestIt() {
            new Halt();
            Verify();
        }
    }
}
