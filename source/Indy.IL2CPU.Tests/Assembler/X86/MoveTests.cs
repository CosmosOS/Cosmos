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
            new Move { DestinationReg = Registers.ESP, SourceValue = 6 };
            new Move { DestinationReg = Registers.EBP, SourceValue = 6 };
            Verify();
        }

        [Test]
        public void TestImmediateToRegister16() {
            new Move { DestinationReg = Registers.AX, SourceValue = 1 };
            new Move { DestinationReg = Registers.BX, SourceValue = 2 };
            new Move { DestinationReg = Registers.CX, SourceValue = 3 };
            new Move { DestinationReg = Registers.DX, SourceValue = 4 };
            new Move { DestinationReg = Registers.SI, SourceValue = 5 };
            new Move { DestinationReg = Registers.DI, SourceValue = 6 };
            new Move { DestinationReg = Registers.BP, SourceValue = 5 };
            new Move { DestinationReg = Registers.SP, SourceValue = 6 };
            Verify();
        }

        [Test]
        public void TestImmediateToRegister8() {
            new Move { DestinationReg = Registers.AL, SourceValue = 1 };
            new Move { DestinationReg = Registers.BL, SourceValue = 2 };
            new Move { DestinationReg = Registers.CL, SourceValue = 3 };
            new Move { DestinationReg = Registers.DL, SourceValue = 4 };
            new Move { DestinationReg = Registers.AH, SourceValue = 1 };
            new Move { DestinationReg = Registers.BH, SourceValue = 2 };
            new Move { DestinationReg = Registers.CH, SourceValue = 3 };
            new Move { DestinationReg = Registers.DH, SourceValue = 4 };
            Verify();
        }

        [Test]
        public void TestImmediateToMemorySimple8() {
            new Move { Size = 8, DestinationReg = Registers.EAX, DestinationIsIndirect = true, SourceValue = 65 };
            new Move { Size = 8, DestinationReg = Registers.EBX, DestinationIsIndirect = true, SourceValue = 66 };
            new Move { Size = 8, DestinationReg = Registers.ECX, DestinationIsIndirect = true, SourceValue = 67 };
            new Move { Size = 8, DestinationReg = Registers.EDX, DestinationIsIndirect = true, SourceValue = 68 };
            new Move { Size = 8, DestinationReg = Registers.EDI, DestinationIsIndirect = true, SourceValue = 69 };
            new Move { Size = 8, DestinationReg = Registers.ESI, DestinationIsIndirect = true, SourceValue = 70 };
            new Move { Size = 8, DestinationReg = Registers.ESP, DestinationIsIndirect = true, SourceValue = 71 };
            new Move { Size = 8, DestinationReg = Registers.EBP, DestinationIsIndirect = true, SourceValue = 72 };
            Verify();
        }
    }
}