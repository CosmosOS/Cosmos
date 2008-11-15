using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.Tests.Assembler.X86
{
    [TestFixture]
    public class PushTests : BaseTest
    {

        /*
         * situations to Push to:
         * registers (32 bit)
         * memory (32 bit)
         * memory with 16 bit offset (32 bit)
         * memory with 32 bit offset (32 bit)
         * immediate (8bit, 16bit, 32bit)
         * segment registers are not covered
         * The 16 bit for registers and memory are not covered
         */
        [Test]
        public void Immediate8()
        {
            new Push { DestinationValue = 30};
            Verify();
        }
        
        [Test]
        public void Immediate16()
        {
            new Push { DestinationValue = 300};
            Verify();
        }
        
        [Test]
        public void Immediate32()
        {
            new Push { DestinationValue = 300000};
            Verify();
        }

        /*Segment registers are 16 bits
         * [Test]
        public void SegmentRegisters()
        {
            new Push { DestinationReg = Registers.DS };
            new Push { DestinationReg = Registers.ES };
            new Push { DestinationReg = Registers.SS };
            new Push { DestinationReg = Registers.FS };
            new Push { DestinationReg = Registers.GS };
            Verify();
        }*/

        [Test]
        public void MemorySimple32()
        {
            /*changed Push to Instruction without size
             new Push { Size = 16, DestinationIsIndirect = true};
            */
            new Push { DestinationIsIndirect = true , DestinationValue=65 };
            Verify();
        }

        [Test]
        public void Memory32BitOffset32()
        {
            /*changed Push to Instruction without size
             new Push { Size = 16, DestinationIsIndirect = true};
            */
            new Push { DestinationIsIndirect = true, DestinationDisplacement=70000, DestinationValue = 65 };
            Verify();
        }

        [Test]
        public void Memory16BitOffset32()
        {
            /*changed Push to Instruction without size
             new Push { Size = 16, DestinationIsIndirect = true};
            */
            new Push { DestinationIsIndirect = true, DestinationDisplacement=203, DestinationValue = 65 };
            Verify();
        }

        [Test]
        public void Registers32()
        {
            new Push { DestinationReg = Registers.EAX };
            new Push { DestinationReg = Registers.EBX };
            new Push { DestinationReg = Registers.ECX };
            new Push { DestinationReg = Registers.EDX };
            new Push { DestinationReg = Registers.EDI };
            new Push { DestinationReg = Registers.ESI };
            new Push { DestinationReg = Registers.ESP };
            new Push { DestinationReg = Registers.EBP };
            Verify();
        }

        /*[Test]
        public void Registers16()
        {
            new Push { DestinationReg = Registers.AX };
            new Push { DestinationReg = Registers.BX };
            new Push { DestinationReg = Registers.CX };
            new Push { DestinationReg = Registers.DX };
            new Push { DestinationReg = Registers.DI };
            new Push { DestinationReg = Registers.SI };
            new Push { DestinationReg = Registers.BP };
            new Push { DestinationReg = Registers.SP };
            Verify();
        }*/
    }
}