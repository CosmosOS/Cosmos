using System;   
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.Tests.Assembler.X86
{
    [TestFixture]
    public class PopTests : BaseTest
    {
        /*
         * situations to cover:
         * Top of stack to segment register
         * top of stack to memory (32 bit)
         * top of stack to general register (32 bit)
         * 16 bit is commented but not covered for now
         * NB: CS cannot be used
         */
        [Test]
        public void SegmentRegisters()
        {
            new Pop { DestinationReg = Registers.DS };
            new Pop { DestinationReg = Registers.ES };
            new Pop { DestinationReg = Registers.SS };
            new Pop { DestinationReg = Registers.FS };
            new Pop { DestinationReg = Registers.GS };
            Verify();
        }

        [Test]
        public void Memory()
        {
            /*changed Pop to Instruction without size
             new Pop { Size = 16, DestinationIsIndirect = true};
            */
            new Pop { DestinationIsIndirect = true , DestinationValue=65 };
            Verify();
        }

        [Test]
        public void Registers32()
        {
            new Pop { DestinationReg = Registers.EAX };
            new Pop { DestinationReg = Registers.EBX };
            new Pop { DestinationReg = Registers.ECX };
            new Pop { DestinationReg = Registers.EDX };
            new Pop { DestinationReg = Registers.EDI };
            new Pop { DestinationReg = Registers.ESI };
            new Pop { DestinationReg = Registers.ESP };
            new Pop { DestinationReg = Registers.EBP };
            Verify();
        }

        /*[Test]
        public void Registers16()
        {
            new Pop { DestinationReg = Registers.AX };
            new Pop { DestinationReg = Registers.BX };
            new Pop { DestinationReg = Registers.CX };
            new Pop { DestinationReg = Registers.DX };
            new Pop { DestinationReg = Registers.DI };
            new Pop { DestinationReg = Registers.SI };
            new Pop { DestinationReg = Registers.BP };
            new Pop { DestinationReg = Registers.SP };
            Verify();
        }*/
    }
}
