using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.Tests.Assembler.X86 {
    [TestFixture]
    public class MoveTests: BaseTest {
        [Test]
        public void TestImmediateToRegister32() {
            new Move { DestinationReg = Registers.EAX, SourceValue = 1 };
            new Move { DestinationReg = Registers.EBX, SourceValue = 2 };
            new Move { DestinationReg = Registers.ECX, SourceValue = 3 };
            new Move { DestinationReg = Registers.EDX, SourceValue = 4 };
            new Move { DestinationReg = Registers.ESI, SourceValue = 5 };
            new Move { DestinationReg = Registers.EDI, SourceValue = 6 };
            Verify();
        }
    }
}