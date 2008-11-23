using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.Tests.AssemblerTests.X86 {
    [TestFixture]
    public class MoveTests: BaseTest {
        /*
         * situations to cover:
         * register to memory indirect + dword (8bit, 16bit, 32bit)
         * register to memoryreg  indirect + byte (8bit, 16bit, 32bit)
         * register to memoryreg indirect + dword (8bit, 16bit, 32bit)
         */
        [Test]
        public void TestImmediateToMemoryReg8BitOffset8() {
            new Move { DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 65, SourceValue = 70, Size=8 };
            new Move { DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 65, SourceValue = 70, Size = 8 };
            new Move { DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 65, SourceValue = 70, Size = 8 };
            new Move { DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 65, SourceValue = 70, Size = 8 };
            new Move { DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 65, SourceValue = 70, Size = 8 };
            new Move { DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 65, SourceValue = 70, Size = 8 };
            new Move { DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 65, SourceValue = 70, Size = 8 };
            new Move { DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 65, SourceValue = 70, Size = 8 };
        }

        [Test]
        public void TestImmediateToMemoryReg16BitOffset8() {
            new Move { DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 650, SourceValue = 70, Size = 8 };
            new Move { DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 650, SourceValue = 70, Size = 8 };
            new Move { DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 650, SourceValue = 70, Size = 8 };
            new Move { DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 650, SourceValue = 70, Size = 8 };
            new Move { DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 650, SourceValue = 70, Size = 8 };
            new Move { DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 650, SourceValue = 70, Size = 8 };
            new Move { DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 650, SourceValue = 70, Size = 8 };
            new Move { DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 650, SourceValue = 70, Size = 8 };
        }

        [Test]
        public void TestImmediateToMemoryReg32BitOffset8() {
            new Move { DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 650000, SourceValue = 70, Size = 8 };
            new Move { DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 650000, SourceValue = 70, Size = 8 };
            new Move { DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 650000, SourceValue = 70, Size = 8 };
            new Move { DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 650000, SourceValue = 70, Size = 8 };
            new Move { DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 650000, SourceValue = 70, Size = 8 };
            new Move { DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 650000, SourceValue = 70, Size = 8 };
            new Move { DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 650000, SourceValue = 70, Size = 8 };
            new Move { DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 650000, SourceValue = 70, Size = 8 };
        }

        [Test]
        public void TestImmediateToMemoryReg8BitOffset16() {
            new Move { DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 65, SourceValue = 70, Size = 16 };
            new Move { DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 65, SourceValue = 70, Size = 16 };
            new Move { DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 65, SourceValue = 70, Size = 16 };
            new Move { DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 65, SourceValue = 70, Size = 16 };
            new Move { DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 65, SourceValue = 70, Size = 16 };
            new Move { DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 65, SourceValue = 70, Size = 16 };
            new Move { DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 65, SourceValue = 70, Size = 16 };
            new Move { DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 65, SourceValue = 70, Size = 16 };
        }

        [Test]
        public void TestImmediateToMemoryReg16BitOffset16() {
            new Move { DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 650, SourceValue = 70, Size = 16 };
            new Move { DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 650, SourceValue = 70, Size = 16 };
            new Move { DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 650, SourceValue = 70, Size = 16 };
            new Move { DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 650, SourceValue = 70, Size = 16 };
            new Move { DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 650, SourceValue = 70, Size = 16 };
            new Move { DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 650, SourceValue = 70, Size = 16 };
            new Move { DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 650, SourceValue = 70, Size = 16 };
            new Move { DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 650, SourceValue = 70, Size = 16 };
        }

        [Test]
        public void TestImmediateToMemoryReg32BitOffset16() {
            new Move { DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 650000, SourceValue = 70, Size = 16 };
            new Move { DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 650000, SourceValue = 70, Size = 16 };
            new Move { DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 650000, SourceValue = 70, Size = 16 };
            new Move { DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 650000, SourceValue = 70, Size = 16 };
            new Move { DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 650000, SourceValue = 70, Size = 16 };
            new Move { DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 650000, SourceValue = 70, Size = 16 };
            new Move { DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 650000, SourceValue = 70, Size = 16 };
            new Move { DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 650000, SourceValue = 70, Size = 16 };
        }

        [Test]
        public void TestImmediateToMemoryReg8BitOffset32() {
            new Move { DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 65, SourceValue = 70, Size = 32 };
            new Move { DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 65, SourceValue = 70, Size = 32 };
            new Move { DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 65, SourceValue = 70, Size = 32 };
            new Move { DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 65, SourceValue = 70, Size = 32 };
            new Move { DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 65, SourceValue = 70, Size = 32 };
            new Move { DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 65, SourceValue = 70, Size = 32 };
            new Move { DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 65, SourceValue = 70, Size = 32 };
            new Move { DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 65, SourceValue = 70, Size = 32 };
        }

        [Test]
        public void TestImmediateToMemoryReg16BitOffset32() {
            new Move { DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 650, SourceValue = 70, Size = 32 };
            new Move { DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 650, SourceValue = 70, Size = 32 };
            new Move { DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 650, SourceValue = 70, Size = 32 };
            new Move { DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 650, SourceValue = 70, Size = 32 };
            new Move { DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 650, SourceValue = 70, Size = 32 };
            new Move { DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 650, SourceValue = 70, Size = 32 };
            new Move { DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 650, SourceValue = 70, Size = 32 };
            new Move { DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 650, SourceValue = 70, Size = 32 };
        }

        [Test]
        public void TestImmediateToMemoryReg32BitOffset32() {
            new Move { DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 650000, SourceValue = 70, Size = 32 };
            new Move { DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 650000, SourceValue = 70, Size = 32 };
            new Move { DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 650000, SourceValue = 70, Size = 32 };
            new Move { DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 650000, SourceValue = 70, Size = 32 };
            new Move { DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 650000, SourceValue = 70, Size = 32 };
            new Move { DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 650000, SourceValue = 70, Size = 32 };
            new Move { DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 650000, SourceValue = 70, Size = 32 };
            new Move { DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 650000, SourceValue = 70, Size = 32 };
        }

        [Test]
        public void TestRegisterToMemory32BitOffset32() {
            new Move { Size = 32, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.EAX };
            new Move { Size = 32, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.EBX };
            new Move { Size = 32, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.ECX };
            new Move { Size = 32, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.EDX };
            new Move { Size = 32, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.EDI };
            new Move { Size = 32, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.ESI };
            new Move { Size = 32, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.EBP };
            new Move { Size = 32, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.ESP };
            new Move { Size = 32, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.EAX };
            new Move { Size = 32, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.EBX };
            new Move { Size = 32, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.ECX };
            new Move { Size = 32, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.EDX };
            new Move { Size = 32, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.EDI };
            new Move { Size = 32, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.ESI };
            new Move { Size = 32, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.EBP };
            new Move { Size = 32, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.ESP };
            new Move { Size = 32, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.EAX };
            new Move { Size = 32, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.EBX };
            new Move { Size = 32, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.ECX };
            new Move { Size = 32, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.EDX };
            new Move { Size = 32, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.EDI };
            new Move { Size = 32, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.ESI };
            new Move { Size = 32, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.EBP };
            new Move { Size = 32, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.ESP };
            new Move { Size = 32, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.EAX };
            new Move { Size = 32, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.EBX };
            new Move { Size = 32, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.ECX };
            new Move { Size = 32, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.EDX };
            new Move { Size = 32, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.EDI };
            new Move { Size = 32, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.ESI };
            new Move { Size = 32, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.EBP };
            new Move { Size = 32, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.ESP };
            new Move { Size = 32, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.EAX };
            new Move { Size = 32, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.EBX };
            new Move { Size = 32, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.ECX };
            new Move { Size = 32, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.EDX };
            new Move { Size = 32, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.EDI };
            new Move { Size = 32, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.ESI };
            new Move { Size = 32, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.EBP };
            new Move { Size = 32, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.ESP };
            new Move { Size = 32, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.EAX };
            new Move { Size = 32, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.EBX };
            new Move { Size = 32, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.ECX };
            new Move { Size = 32, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.EDX };
            new Move { Size = 32, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.EDI };
            new Move { Size = 32, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.ESI };
            new Move { Size = 32, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.EBP };
            new Move { Size = 32, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.ESP };
            new Move { Size = 32, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.EAX };
            new Move { Size = 32, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.EBX };
            new Move { Size = 32, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.ECX };
            new Move { Size = 32, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.EDX };
            new Move { Size = 32, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.EDI };
            new Move { Size = 32, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.ESI };
            new Move { Size = 32, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.EBP };
            new Move { Size = 32, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.ESP };
            new Move { Size = 32, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.EAX };
            new Move { Size = 32, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.EBX };
            new Move { Size = 32, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.ECX };
            new Move { Size = 32, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.EDX };
            new Move { Size = 32, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.EDI };
            new Move { Size = 32, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.ESI };
            new Move { Size = 32, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.EBP };
            new Move { Size = 32, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.ESP };
            Verify();
        }

        [Test]
        public void TestRegisterToMemory32BitOffset16() {
            new Move { Size = 16, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.AX };
            new Move { Size = 16, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.BX };
            new Move { Size = 16, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.CX };
            new Move { Size = 16, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.DX };
            new Move { Size = 16, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.DI };
            new Move { Size = 16, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.SI };
            new Move { Size = 16, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.BP };
            new Move { Size = 16, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.SP };
            new Move { Size = 16, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.AX };
            new Move { Size = 16, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.BX };
            new Move { Size = 16, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.CX };
            new Move { Size = 16, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.DX };
            new Move { Size = 16, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.DI };
            new Move { Size = 16, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.SI };
            new Move { Size = 16, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.BP };
            new Move { Size = 16, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.SP };
            new Move { Size = 16, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.AX };
            new Move { Size = 16, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.BX };
            new Move { Size = 16, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.CX };
            new Move { Size = 16, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.DX };
            new Move { Size = 16, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.DI };
            new Move { Size = 16, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.SI };
            new Move { Size = 16, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.BP };
            new Move { Size = 16, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.SP };
            new Move { Size = 16, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.AX };
            new Move { Size = 16, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.BX };
            new Move { Size = 16, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.CX };
            new Move { Size = 16, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.DX };
            new Move { Size = 16, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.DI };
            new Move { Size = 16, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.SI };
            new Move { Size = 16, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.BP };
            new Move { Size = 16, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.SP };
            new Move { Size = 16, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.AX };
            new Move { Size = 16, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.BX };
            new Move { Size = 16, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.CX };
            new Move { Size = 16, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.DX };
            new Move { Size = 16, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.DI };
            new Move { Size = 16, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.SI };
            new Move { Size = 16, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.BP };
            new Move { Size = 16, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.SP };
            new Move { Size = 16, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.AX };
            new Move { Size = 16, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.BX };
            new Move { Size = 16, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.CX };
            new Move { Size = 16, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.DX };
            new Move { Size = 16, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.DI };
            new Move { Size = 16, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.SI };
            new Move { Size = 16, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.BP };
            new Move { Size = 16, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.SP };
            new Move { Size = 16, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.AX };
            new Move { Size = 16, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.BX };
            new Move { Size = 16, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.CX };
            new Move { Size = 16, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.DX };
            new Move { Size = 16, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.DI };
            new Move { Size = 16, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.SI };
            new Move { Size = 16, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.BP };
            new Move { Size = 16, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.SP };
            new Move { Size = 16, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.AX };
            new Move { Size = 16, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.BX };
            new Move { Size = 16, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.CX };
            new Move { Size = 16, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.DX };
            new Move { Size = 16, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.DI };
            new Move { Size = 16, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.SI };
            new Move { Size = 16, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.BP };
            new Move { Size = 16, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.SP };
            Verify();
        }

        [Test]
        public void TestRegisterToMemory32BitOffset8() {
            new Move { Size = 8, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.AL };
            new Move { Size = 8, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.BL };
            new Move { Size = 8, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.CL };
            new Move { Size = 8, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.DL };
            new Move { Size = 8, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.AH };
            new Move { Size = 8, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.BH };
            new Move { Size = 8, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.CH };
            new Move { Size = 8, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.DH };
            new Move { Size = 8, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.AL };
            new Move { Size = 8, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.BL };
            new Move { Size = 8, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.CL };
            new Move { Size = 8, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.DL };
            new Move { Size = 8, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.AH };
            new Move { Size = 8, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.BH };
            new Move { Size = 8, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.CH };
            new Move { Size = 8, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.DH };
            new Move { Size = 8, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.AL };
            new Move { Size = 8, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.BL };
            new Move { Size = 8, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.CL };
            new Move { Size = 8, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.DL };
            new Move { Size = 8, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.AH };
            new Move { Size = 8, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.BH };
            new Move { Size = 8, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.CH };
            new Move { Size = 8, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.DH };
            new Move { Size = 8, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.AL };
            new Move { Size = 8, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.BL };
            new Move { Size = 8, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.CL };
            new Move { Size = 8, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.DL };
            new Move { Size = 8, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.AH };
            new Move { Size = 8, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.BH };
            new Move { Size = 8, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.CH };
            new Move { Size = 8, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.DH };
            new Move { Size = 8, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.AL };
            new Move { Size = 8, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.BL };
            new Move { Size = 8, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.CL };
            new Move { Size = 8, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.DL };
            new Move { Size = 8, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.AH };
            new Move { Size = 8, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.BH };
            new Move { Size = 8, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.CH };
            new Move { Size = 8, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.DH };
            new Move { Size = 8, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.AL };
            new Move { Size = 8, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.BL };
            new Move { Size = 8, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.CL };
            new Move { Size = 8, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.DL };
            new Move { Size = 8, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.AH };
            new Move { Size = 8, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.BH };
            new Move { Size = 8, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.CH };
            new Move { Size = 8, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.DH };
            new Move { Size = 8, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.AL };
            new Move { Size = 8, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.BL };
            new Move { Size = 8, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.CL };
            new Move { Size = 8, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.DL };
            new Move { Size = 8, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.AH };
            new Move { Size = 8, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.BH };
            new Move { Size = 8, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.CH };
            new Move { Size = 8, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.DH };
            new Move { Size = 8, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.AL };
            new Move { Size = 8, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.BL };
            new Move { Size = 8, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.CL };
            new Move { Size = 8, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.DL };
            new Move { Size = 8, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.AH };
            new Move { Size = 8, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.BH };
            new Move { Size = 8, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.CH };
            new Move { Size = 8, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.DH };
            Verify();
        }

        [Test]
        public void TestRegisterToMemory16BitOffset32() {
            new Move { Size = 32, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.EAX };
            new Move { Size = 32, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.EBX };
            new Move { Size = 32, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.ECX };
            new Move { Size = 32, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.EDX };
            new Move { Size = 32, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.EDI };
            new Move { Size = 32, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.ESI };
            new Move { Size = 32, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.EBP };
            new Move { Size = 32, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.ESP };
            new Move { Size = 32, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.EAX };
            new Move { Size = 32, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.EBX };
            new Move { Size = 32, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.ECX };
            new Move { Size = 32, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.EDX };
            new Move { Size = 32, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.EDI };
            new Move { Size = 32, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.ESI };
            new Move { Size = 32, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.EBP };
            new Move { Size = 32, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.ESP };
            new Move { Size = 32, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.EAX };
            new Move { Size = 32, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.EBX };
            new Move { Size = 32, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.ECX };
            new Move { Size = 32, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.EDX };
            new Move { Size = 32, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.EDI };
            new Move { Size = 32, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.ESI };
            new Move { Size = 32, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.EBP };
            new Move { Size = 32, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.ESP };
            new Move { Size = 32, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.EAX };
            new Move { Size = 32, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.EBX };
            new Move { Size = 32, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.ECX };
            new Move { Size = 32, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.EDX };
            new Move { Size = 32, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.EDI };
            new Move { Size = 32, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.ESI };
            new Move { Size = 32, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.EBP };
            new Move { Size = 32, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.ESP };
            new Move { Size = 32, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.EAX };
            new Move { Size = 32, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.EBX };
            new Move { Size = 32, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.ECX };
            new Move { Size = 32, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.EDX };
            new Move { Size = 32, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.EDI };
            new Move { Size = 32, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.ESI };
            new Move { Size = 32, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.EBP };
            new Move { Size = 32, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.ESP };
            new Move { Size = 32, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.EAX };
            new Move { Size = 32, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.EBX };
            new Move { Size = 32, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.ECX };
            new Move { Size = 32, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.EDX };
            new Move { Size = 32, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.EDI };
            new Move { Size = 32, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.ESI };
            new Move { Size = 32, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.EBP };
            new Move { Size = 32, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.ESP };
            new Move { Size = 32, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.EAX };
            new Move { Size = 32, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.EBX };
            new Move { Size = 32, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.ECX };
            new Move { Size = 32, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.EDX };
            new Move { Size = 32, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.EDI };
            new Move { Size = 32, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.ESI };
            new Move { Size = 32, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.EBP };
            new Move { Size = 32, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.ESP };
            new Move { Size = 32, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.EAX };
            new Move { Size = 32, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.EBX };
            new Move { Size = 32, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.ECX };
            new Move { Size = 32, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.EDX };
            new Move { Size = 32, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.EDI };
            new Move { Size = 32, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.ESI };
            new Move { Size = 32, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.EBP };
            new Move { Size = 32, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.ESP };
            Verify();
        }

        [Test]
        public void TestRegisterToMemory16BitOffset16() {
            new Move { Size = 16, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.AX };
            new Move { Size = 16, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.BX };
            new Move { Size = 16, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.CX };
            new Move { Size = 16, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.DX };
            new Move { Size = 16, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.DI };
            new Move { Size = 16, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.SI };
            new Move { Size = 16, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.BP };
            new Move { Size = 16, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.SP };
            new Move { Size = 16, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.AX };
            new Move { Size = 16, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.BX };
            new Move { Size = 16, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.CX };
            new Move { Size = 16, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.DX };
            new Move { Size = 16, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.DI };
            new Move { Size = 16, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.SI };
            new Move { Size = 16, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.BP };
            new Move { Size = 16, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.SP };
            new Move { Size = 16, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.AX };
            new Move { Size = 16, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.BX };
            new Move { Size = 16, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.CX };
            new Move { Size = 16, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.DX };
            new Move { Size = 16, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.DI };
            new Move { Size = 16, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.SI };
            new Move { Size = 16, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.BP };
            new Move { Size = 16, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.SP };
            new Move { Size = 16, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.AX };
            new Move { Size = 16, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.BX };
            new Move { Size = 16, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.CX };
            new Move { Size = 16, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.DX };
            new Move { Size = 16, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.DI };
            new Move { Size = 16, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.SI };
            new Move { Size = 16, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.BP };
            new Move { Size = 16, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.SP };
            new Move { Size = 16, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.AX };
            new Move { Size = 16, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.BX };
            new Move { Size = 16, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.CX };
            new Move { Size = 16, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.DX };
            new Move { Size = 16, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.DI };
            new Move { Size = 16, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.SI };
            new Move { Size = 16, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.BP };
            new Move { Size = 16, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.SP };
            new Move { Size = 16, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.AX };
            new Move { Size = 16, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.BX };
            new Move { Size = 16, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.CX };
            new Move { Size = 16, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.DX };
            new Move { Size = 16, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.DI };
            new Move { Size = 16, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.SI };
            new Move { Size = 16, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.BP };
            new Move { Size = 16, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.SP };
            new Move { Size = 16, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.AX };
            new Move { Size = 16, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.BX };
            new Move { Size = 16, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.CX };
            new Move { Size = 16, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.DX };
            new Move { Size = 16, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.DI };
            new Move { Size = 16, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.SI };
            new Move { Size = 16, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.BP };
            new Move { Size = 16, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.SP };
            new Move { Size = 16, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.AX };
            new Move { Size = 16, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.BX };
            new Move { Size = 16, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.CX };
            new Move { Size = 16, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.DX };
            new Move { Size = 16, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.DI };
            new Move { Size = 16, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.SI };
            new Move { Size = 16, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.BP };
            new Move { Size = 16, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.SP };
            Verify();
        }

        [Test]
        public void TestRegisterToMemory16BitOffset8() {
            new Move { Size = 8, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.AL };
            new Move { Size = 8, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.BL };
            new Move { Size = 8, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.CL };
            new Move { Size = 8, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.DL };
            new Move { Size = 8, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.AH };
            new Move { Size = 8, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.BH };
            new Move { Size = 8, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.CH };
            new Move { Size = 8, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.DH };
            new Move { Size = 8, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.AL };
            new Move { Size = 8, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.BL };
            new Move { Size = 8, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.CL };
            new Move { Size = 8, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.DL };
            new Move { Size = 8, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.AH };
            new Move { Size = 8, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.BH };
            new Move { Size = 8, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.CH };
            new Move { Size = 8, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.DH };
            new Move { Size = 8, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.AL };
            new Move { Size = 8, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.BL };
            new Move { Size = 8, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.CL };
            new Move { Size = 8, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.DL };
            new Move { Size = 8, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.AH };
            new Move { Size = 8, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.BH };
            new Move { Size = 8, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.CH };
            new Move { Size = 8, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.DH };
            new Move { Size = 8, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.AL };
            new Move { Size = 8, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.BL };
            new Move { Size = 8, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.CL };
            new Move { Size = 8, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.DL };
            new Move { Size = 8, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.AH };
            new Move { Size = 8, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.BH };
            new Move { Size = 8, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.CH };
            new Move { Size = 8, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.DH };
            new Move { Size = 8, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.AL };
            new Move { Size = 8, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.BL };
            new Move { Size = 8, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.CL };
            new Move { Size = 8, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.DL };
            new Move { Size = 8, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.AH };
            new Move { Size = 8, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.BH };
            new Move { Size = 8, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.CH };
            new Move { Size = 8, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.DH };
            new Move { Size = 8, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.AL };
            new Move { Size = 8, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.BL };
            new Move { Size = 8, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.CL };
            new Move { Size = 8, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.DL };
            new Move { Size = 8, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.AH };
            new Move { Size = 8, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.BH };
            new Move { Size = 8, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.CH };
            new Move { Size = 8, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.DH };
            new Move { Size = 8, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.AL };
            new Move { Size = 8, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.BL };
            new Move { Size = 8, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.CL };
            new Move { Size = 8, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.DL };
            new Move { Size = 8, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.AH };
            new Move { Size = 8, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.BH };
            new Move { Size = 8, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.CH };
            new Move { Size = 8, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.DH };
            new Move { Size = 8, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.AL };
            new Move { Size = 8, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.BL };
            new Move { Size = 8, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.CL };
            new Move { Size = 8, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.DL };
            new Move { Size = 8, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.AH };
            new Move { Size = 8, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.BH };
            new Move { Size = 8, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.CH };
            new Move { Size = 8, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.DH };
            Verify();
        }

        [Test]
        public void TestRegisterToMemory8BitOffset32() {
            new Move { Size = 32, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.EAX };
            new Move { Size = 32, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.EBX };
            new Move { Size = 32, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.ECX };
            new Move { Size = 32, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.EDX };
            new Move { Size = 32, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.EDI };
            new Move { Size = 32, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.ESI };
            new Move { Size = 32, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.EBP };
            new Move { Size = 32, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.ESP };
            new Move { Size = 32, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.EAX };
            new Move { Size = 32, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.EBX };
            new Move { Size = 32, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.ECX };
            new Move { Size = 32, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.EDX };
            new Move { Size = 32, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.EDI };
            new Move { Size = 32, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.ESI };
            new Move { Size = 32, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.EBP };
            new Move { Size = 32, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.ESP };
            new Move { Size = 32, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.EAX };
            new Move { Size = 32, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.EBX };
            new Move { Size = 32, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.ECX };
            new Move { Size = 32, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.EDX };
            new Move { Size = 32, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.EDI };
            new Move { Size = 32, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.ESI };
            new Move { Size = 32, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.EBP };
            new Move { Size = 32, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.ESP };
            new Move { Size = 32, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.EAX };
            new Move { Size = 32, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.EBX };
            new Move { Size = 32, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.ECX };
            new Move { Size = 32, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.EDX };
            new Move { Size = 32, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.EDI };
            new Move { Size = 32, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.ESI };
            new Move { Size = 32, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.EBP };
            new Move { Size = 32, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.ESP };
            new Move { Size = 32, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.EAX };
            new Move { Size = 32, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.EBX };
            new Move { Size = 32, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.ECX };
            new Move { Size = 32, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.EDX };
            new Move { Size = 32, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.EDI };
            new Move { Size = 32, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.ESI };
            new Move { Size = 32, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.EBP };
            new Move { Size = 32, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.ESP };
            new Move { Size = 32, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.EAX };
            new Move { Size = 32, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.EBX };
            new Move { Size = 32, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.ECX };
            new Move { Size = 32, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.EDX };
            new Move { Size = 32, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.EDI };
            new Move { Size = 32, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.ESI };
            new Move { Size = 32, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.EBP };
            new Move { Size = 32, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.ESP };
            new Move { Size = 32, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.EAX };
            new Move { Size = 32, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.EBX };
            new Move { Size = 32, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.ECX };
            new Move { Size = 32, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.EDX };
            new Move { Size = 32, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.EDI };
            new Move { Size = 32, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.ESI };
            new Move { Size = 32, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.EBP };
            new Move { Size = 32, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.ESP };
            new Move { Size = 32, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.EAX };
            new Move { Size = 32, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.EBX };
            new Move { Size = 32, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.ECX };
            new Move { Size = 32, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.EDX };
            new Move { Size = 32, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.EDI };
            new Move { Size = 32, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.ESI };
            new Move { Size = 32, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.EBP };
            new Move { Size = 32, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.ESP };
            Verify();
        }

        [Test]
        public void TestRegisterToMemory8BitOffset16() {
            new Move { Size = 16, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.AX };
            new Move { Size = 16, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.BX };
            new Move { Size = 16, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.CX };
            new Move { Size = 16, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.DX };
            new Move { Size = 16, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.DI };
            new Move { Size = 16, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.SI };
            new Move { Size = 16, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.BP };
            new Move { Size = 16, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.SP };
            new Move { Size = 16, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.AX };
            new Move { Size = 16, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.BX };
            new Move { Size = 16, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.CX };
            new Move { Size = 16, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.DX };
            new Move { Size = 16, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.DI };
            new Move { Size = 16, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.SI };
            new Move { Size = 16, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.BP };
            new Move { Size = 16, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.SP };
            new Move { Size = 16, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.AX };
            new Move { Size = 16, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.BX };
            new Move { Size = 16, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.CX };
            new Move { Size = 16, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.DX };
            new Move { Size = 16, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.DI };
            new Move { Size = 16, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.SI };
            new Move { Size = 16, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.BP };
            new Move { Size = 16, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.SP };
            new Move { Size = 16, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.AX };
            new Move { Size = 16, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.BX };
            new Move { Size = 16, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.CX };
            new Move { Size = 16, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.DX };
            new Move { Size = 16, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.DI };
            new Move { Size = 16, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.SI };
            new Move { Size = 16, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.BP };
            new Move { Size = 16, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.SP };
            new Move { Size = 16, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.AX };
            new Move { Size = 16, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.BX };
            new Move { Size = 16, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.CX };
            new Move { Size = 16, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.DX };
            new Move { Size = 16, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.DI };
            new Move { Size = 16, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.SI };
            new Move { Size = 16, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.BP };
            new Move { Size = 16, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.SP };
            new Move { Size = 16, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.AX };
            new Move { Size = 16, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.BX };
            new Move { Size = 16, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.CX };
            new Move { Size = 16, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.DX };
            new Move { Size = 16, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.DI };
            new Move { Size = 16, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.SI };
            new Move { Size = 16, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.BP };
            new Move { Size = 16, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.SP };
            new Move { Size = 16, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.AX };
            new Move { Size = 16, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.BX };
            new Move { Size = 16, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.CX };
            new Move { Size = 16, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.DX };
            new Move { Size = 16, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.DI };
            new Move { Size = 16, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.SI };
            new Move { Size = 16, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.BP };
            new Move { Size = 16, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.SP };
            new Move { Size = 16, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.AX };
            new Move { Size = 16, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.BX };
            new Move { Size = 16, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.CX };
            new Move { Size = 16, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.DX };
            new Move { Size = 16, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.DI };
            new Move { Size = 16, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.SI };
            new Move { Size = 16, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.BP };
            new Move { Size = 16, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.SP };
            Verify();
        }

        [Test]
        public void TestRegisterToMemory8BitOffset8() {
            new Move { Size = 8, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.AL };
            new Move { Size = 8, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.BL };
            new Move { Size = 8, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.CL };
            new Move { Size = 8, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.DL };
            new Move { Size = 8, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.AH };
            new Move { Size = 8, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.BH };
            new Move { Size = 8, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.CH };
            new Move { Size = 8, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.DH };
            new Move { Size = 8, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.AL };
            new Move { Size = 8, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.BL };
            new Move { Size = 8, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.CL };
            new Move { Size = 8, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.DL };
            new Move { Size = 8, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.AH };
            new Move { Size = 8, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.BH };
            new Move { Size = 8, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.CH };
            new Move { Size = 8, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.DH };
            new Move { Size = 8, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.AL };
            new Move { Size = 8, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.BL };
            new Move { Size = 8, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.CL };
            new Move { Size = 8, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.DL };
            new Move { Size = 8, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.AH };
            new Move { Size = 8, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.BH };
            new Move { Size = 8, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.CH };
            new Move { Size = 8, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.DH };
            new Move { Size = 8, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.AL };
            new Move { Size = 8, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.BL };
            new Move { Size = 8, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.CL };
            new Move { Size = 8, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.DL };
            new Move { Size = 8, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.AH };
            new Move { Size = 8, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.BH };
            new Move { Size = 8, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.CH };
            new Move { Size = 8, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.DH };
            new Move { Size = 8, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.AL };
            new Move { Size = 8, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.BL };
            new Move { Size = 8, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.CL };
            new Move { Size = 8, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.DL };
            new Move { Size = 8, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.AH };
            new Move { Size = 8, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.BH };
            new Move { Size = 8, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.CH };
            new Move { Size = 8, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.DH };
            new Move { Size = 8, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.AL };
            new Move { Size = 8, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.BL };
            new Move { Size = 8, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.CL };
            new Move { Size = 8, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.DL };
            new Move { Size = 8, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.AH };
            new Move { Size = 8, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.BH };
            new Move { Size = 8, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.CH };
            new Move { Size = 8, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.DH };
            new Move { Size = 8, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.AL };
            new Move { Size = 8, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.BL };
            new Move { Size = 8, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.CL };
            new Move { Size = 8, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.DL };
            new Move { Size = 8, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.AH };
            new Move { Size = 8, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.BH };
            new Move { Size = 8, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.CH };
            new Move { Size = 8, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.DH };
            new Move { Size = 8, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.AL };
            new Move { Size = 8, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.BL };
            new Move { Size = 8, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.CL };
            new Move { Size = 8, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.DL };
            new Move { Size = 8, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.AH };
            new Move { Size = 8, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.BH };
            new Move { Size = 8, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.CH };
            new Move { Size = 8, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.DH };
            Verify();
        }

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

        [Test]
        public void TestImmediateToMemorySimple16() {
            new Move { Size = 16, DestinationReg = Registers.EAX, DestinationIsIndirect = true, SourceValue = 65 };
            new Move { Size = 16, DestinationReg = Registers.EBX, DestinationIsIndirect = true, SourceValue = 66 };
            new Move { Size = 16, DestinationReg = Registers.ECX, DestinationIsIndirect = true, SourceValue = 67 };
            new Move { Size = 16, DestinationReg = Registers.EDX, DestinationIsIndirect = true, SourceValue = 68 };
            new Move { Size = 16, DestinationReg = Registers.EDI, DestinationIsIndirect = true, SourceValue = 69 };
            new Move { Size = 16, DestinationReg = Registers.ESI, DestinationIsIndirect = true, SourceValue = 70 };
            new Move { Size = 16, DestinationReg = Registers.ESP, DestinationIsIndirect = true, SourceValue = 71 };
            new Move { Size = 16, DestinationReg = Registers.EBP, DestinationIsIndirect = true, SourceValue = 72 };
            Verify();
        }

        [Test]
        public void TestImmediateToMemorySimple32() {
            new Move { Size = 32, DestinationReg = Registers.EAX, DestinationIsIndirect = true, SourceValue = 65 };
            new Move { Size = 32, DestinationReg = Registers.EBX, DestinationIsIndirect = true, SourceValue = 66 };
            new Move { Size = 32, DestinationReg = Registers.ECX, DestinationIsIndirect = true, SourceValue = 67 };
            new Move { Size = 32, DestinationReg = Registers.EDX, DestinationIsIndirect = true, SourceValue = 68 };
            new Move { Size = 32, DestinationReg = Registers.EDI, DestinationIsIndirect = true, SourceValue = 69 };
            new Move { Size = 32, DestinationReg = Registers.ESI, DestinationIsIndirect = true, SourceValue = 70 };
            new Move { Size = 32, DestinationReg = Registers.ESP, DestinationIsIndirect = true, SourceValue = 71 };
            new Move { Size = 32, DestinationReg = Registers.EBP, DestinationIsIndirect = true, SourceValue = 72 };
            Verify();
        }

        [Test]
        public void TestRegisterToRegister32() {
            new Move { DestinationReg = Registers.EAX, SourceReg = Registers.EAX };
            new Move { DestinationReg = Registers.EAX, SourceReg = Registers.EBX };
            new Move { DestinationReg = Registers.EAX, SourceReg = Registers.ECX };
            new Move { DestinationReg = Registers.EAX, SourceReg = Registers.EDX };
            new Move { DestinationReg = Registers.EAX, SourceReg = Registers.EDI };
            new Move { DestinationReg = Registers.EAX, SourceReg = Registers.ESI };
            new Move { DestinationReg = Registers.EAX, SourceReg = Registers.EBP };
            new Move { DestinationReg = Registers.EAX, SourceReg = Registers.ESP };
            new Move { DestinationReg = Registers.EBX, SourceReg = Registers.EAX };
            new Move { DestinationReg = Registers.EBX, SourceReg = Registers.EBX };
            new Move { DestinationReg = Registers.EBX, SourceReg = Registers.ECX };
            new Move { DestinationReg = Registers.EBX, SourceReg = Registers.EDX };
            new Move { DestinationReg = Registers.EBX, SourceReg = Registers.EDI };
            new Move { DestinationReg = Registers.EBX, SourceReg = Registers.ESI };
            new Move { DestinationReg = Registers.EBX, SourceReg = Registers.EBP };
            new Move { DestinationReg = Registers.EBX, SourceReg = Registers.ESP };
            new Move { DestinationReg = Registers.ECX, SourceReg = Registers.EAX };
            new Move { DestinationReg = Registers.ECX, SourceReg = Registers.EBX };
            new Move { DestinationReg = Registers.ECX, SourceReg = Registers.ECX };
            new Move { DestinationReg = Registers.ECX, SourceReg = Registers.EDX };
            new Move { DestinationReg = Registers.ECX, SourceReg = Registers.EDI };
            new Move { DestinationReg = Registers.ECX, SourceReg = Registers.ESI };
            new Move { DestinationReg = Registers.ECX, SourceReg = Registers.EBP };
            new Move { DestinationReg = Registers.ECX, SourceReg = Registers.ESP };
            new Move { DestinationReg = Registers.EDX, SourceReg = Registers.EAX };
            new Move { DestinationReg = Registers.EDX, SourceReg = Registers.EBX };
            new Move { DestinationReg = Registers.EDX, SourceReg = Registers.ECX };
            new Move { DestinationReg = Registers.EDX, SourceReg = Registers.EDX };
            new Move { DestinationReg = Registers.EDX, SourceReg = Registers.EDI };
            new Move { DestinationReg = Registers.EDX, SourceReg = Registers.ESI };
            new Move { DestinationReg = Registers.EDX, SourceReg = Registers.EBP };
            new Move { DestinationReg = Registers.EDX, SourceReg = Registers.ESP };
            new Move { DestinationReg = Registers.EDI, SourceReg = Registers.EAX };
            new Move { DestinationReg = Registers.EDI, SourceReg = Registers.EBX };
            new Move { DestinationReg = Registers.EDI, SourceReg = Registers.ECX };
            new Move { DestinationReg = Registers.EDI, SourceReg = Registers.EDX };
            new Move { DestinationReg = Registers.EDI, SourceReg = Registers.EDI };
            new Move { DestinationReg = Registers.EDI, SourceReg = Registers.ESI };
            new Move { DestinationReg = Registers.EDI, SourceReg = Registers.EBP };
            new Move { DestinationReg = Registers.EDI, SourceReg = Registers.ESP };
            new Move { DestinationReg = Registers.ESI, SourceReg = Registers.EAX };
            new Move { DestinationReg = Registers.ESI, SourceReg = Registers.EBX };
            new Move { DestinationReg = Registers.ESI, SourceReg = Registers.ECX };
            new Move { DestinationReg = Registers.ESI, SourceReg = Registers.EDX };
            new Move { DestinationReg = Registers.ESI, SourceReg = Registers.EDI };
            new Move { DestinationReg = Registers.ESI, SourceReg = Registers.ESI };
            new Move { DestinationReg = Registers.ESI, SourceReg = Registers.EBP };
            new Move { DestinationReg = Registers.ESI, SourceReg = Registers.ESP };
            new Move { DestinationReg = Registers.EBP, SourceReg = Registers.EAX };
            new Move { DestinationReg = Registers.EBP, SourceReg = Registers.EBX };
            new Move { DestinationReg = Registers.EBP, SourceReg = Registers.ECX };
            new Move { DestinationReg = Registers.EBP, SourceReg = Registers.EDX };
            new Move { DestinationReg = Registers.EBP, SourceReg = Registers.EDI };
            new Move { DestinationReg = Registers.EBP, SourceReg = Registers.ESI };
            new Move { DestinationReg = Registers.EBP, SourceReg = Registers.EBP };
            new Move { DestinationReg = Registers.EBP, SourceReg = Registers.ESP };
            new Move { DestinationReg = Registers.ESP, SourceReg = Registers.EAX };
            new Move { DestinationReg = Registers.ESP, SourceReg = Registers.EBX };
            new Move { DestinationReg = Registers.ESP, SourceReg = Registers.ECX };
            new Move { DestinationReg = Registers.ESP, SourceReg = Registers.EDX };
            new Move { DestinationReg = Registers.ESP, SourceReg = Registers.EDI };
            new Move { DestinationReg = Registers.ESP, SourceReg = Registers.ESI };
            new Move { DestinationReg = Registers.ESP, SourceReg = Registers.EBP };
            new Move { DestinationReg = Registers.ESP, SourceReg = Registers.ESP };
            Verify();
        }

        [Test]
        public void TestRegisterToRegister16() {
            new Move { DestinationReg = Registers.AX, SourceReg = Registers.AX };
            new Move { DestinationReg = Registers.AX, SourceReg = Registers.BX };
            new Move { DestinationReg = Registers.AX, SourceReg = Registers.CX };
            new Move { DestinationReg = Registers.AX, SourceReg = Registers.DX };
            new Move { DestinationReg = Registers.AX, SourceReg = Registers.DI };
            new Move { DestinationReg = Registers.AX, SourceReg = Registers.SI };
            new Move { DestinationReg = Registers.AX, SourceReg = Registers.BP };
            new Move { DestinationReg = Registers.AX, SourceReg = Registers.SP };
            new Move { DestinationReg = Registers.BX, SourceReg = Registers.AX };
            new Move { DestinationReg = Registers.BX, SourceReg = Registers.BX };
            new Move { DestinationReg = Registers.BX, SourceReg = Registers.CX };
            new Move { DestinationReg = Registers.BX, SourceReg = Registers.DX };
            new Move { DestinationReg = Registers.BX, SourceReg = Registers.DI };
            new Move { DestinationReg = Registers.BX, SourceReg = Registers.SI };
            new Move { DestinationReg = Registers.BX, SourceReg = Registers.BP };
            new Move { DestinationReg = Registers.BX, SourceReg = Registers.SP };
            new Move { DestinationReg = Registers.CX, SourceReg = Registers.AX };
            new Move { DestinationReg = Registers.CX, SourceReg = Registers.BX };
            new Move { DestinationReg = Registers.CX, SourceReg = Registers.CX };
            new Move { DestinationReg = Registers.CX, SourceReg = Registers.DX };
            new Move { DestinationReg = Registers.CX, SourceReg = Registers.DI };
            new Move { DestinationReg = Registers.CX, SourceReg = Registers.SI };
            new Move { DestinationReg = Registers.CX, SourceReg = Registers.BP };
            new Move { DestinationReg = Registers.CX, SourceReg = Registers.SP };
            new Move { DestinationReg = Registers.DX, SourceReg = Registers.AX };
            new Move { DestinationReg = Registers.DX, SourceReg = Registers.BX };
            new Move { DestinationReg = Registers.DX, SourceReg = Registers.CX };
            new Move { DestinationReg = Registers.DX, SourceReg = Registers.DX };
            new Move { DestinationReg = Registers.DX, SourceReg = Registers.DI };
            new Move { DestinationReg = Registers.DX, SourceReg = Registers.SI };
            new Move { DestinationReg = Registers.DX, SourceReg = Registers.BP };
            new Move { DestinationReg = Registers.DX, SourceReg = Registers.SP };
            new Move { DestinationReg = Registers.DI, SourceReg = Registers.AX };
            new Move { DestinationReg = Registers.DI, SourceReg = Registers.BX };
            new Move { DestinationReg = Registers.DI, SourceReg = Registers.CX };
            new Move { DestinationReg = Registers.DI, SourceReg = Registers.DX };
            new Move { DestinationReg = Registers.DI, SourceReg = Registers.DI };
            new Move { DestinationReg = Registers.DI, SourceReg = Registers.SI };
            new Move { DestinationReg = Registers.DI, SourceReg = Registers.BP };
            new Move { DestinationReg = Registers.DI, SourceReg = Registers.SP };
            new Move { DestinationReg = Registers.SI, SourceReg = Registers.AX };
            new Move { DestinationReg = Registers.SI, SourceReg = Registers.BX };
            new Move { DestinationReg = Registers.SI, SourceReg = Registers.CX };
            new Move { DestinationReg = Registers.SI, SourceReg = Registers.DX };
            new Move { DestinationReg = Registers.SI, SourceReg = Registers.DI };
            new Move { DestinationReg = Registers.SI, SourceReg = Registers.SI };
            new Move { DestinationReg = Registers.SI, SourceReg = Registers.BP };
            new Move { DestinationReg = Registers.SI, SourceReg = Registers.SP };
            new Move { DestinationReg = Registers.BP, SourceReg = Registers.AX };
            new Move { DestinationReg = Registers.BP, SourceReg = Registers.BX };
            new Move { DestinationReg = Registers.BP, SourceReg = Registers.CX };
            new Move { DestinationReg = Registers.BP, SourceReg = Registers.DX };
            new Move { DestinationReg = Registers.BP, SourceReg = Registers.DI };
            new Move { DestinationReg = Registers.BP, SourceReg = Registers.SI };
            new Move { DestinationReg = Registers.BP, SourceReg = Registers.BP };
            new Move { DestinationReg = Registers.BP, SourceReg = Registers.SP };
            new Move { DestinationReg = Registers.SP, SourceReg = Registers.AX };
            new Move { DestinationReg = Registers.SP, SourceReg = Registers.BX };
            new Move { DestinationReg = Registers.SP, SourceReg = Registers.CX };
            new Move { DestinationReg = Registers.SP, SourceReg = Registers.DX };
            new Move { DestinationReg = Registers.SP, SourceReg = Registers.DI };
            new Move { DestinationReg = Registers.SP, SourceReg = Registers.SI };
            new Move { DestinationReg = Registers.SP, SourceReg = Registers.BP };
            new Move { DestinationReg = Registers.SP, SourceReg = Registers.SP };
            Verify();
        }

        [Test]
        public void TestRegisterToRegister8() {
            new Move { DestinationReg = Registers.AL, SourceReg = Registers.AL };
            new Move { DestinationReg = Registers.AL, SourceReg = Registers.AH };
            new Move { DestinationReg = Registers.AL, SourceReg = Registers.BL };
            new Move { DestinationReg = Registers.AL, SourceReg = Registers.BH };
            new Move { DestinationReg = Registers.AL, SourceReg = Registers.CL };
            new Move { DestinationReg = Registers.AL, SourceReg = Registers.CH };
            new Move { DestinationReg = Registers.AL, SourceReg = Registers.DL };
            new Move { DestinationReg = Registers.AL, SourceReg = Registers.DH };
            new Move { DestinationReg = Registers.AH, SourceReg = Registers.AL };
            new Move { DestinationReg = Registers.AH, SourceReg = Registers.AH };
            new Move { DestinationReg = Registers.AH, SourceReg = Registers.BL };
            new Move { DestinationReg = Registers.AH, SourceReg = Registers.BH };
            new Move { DestinationReg = Registers.AH, SourceReg = Registers.CL };
            new Move { DestinationReg = Registers.AH, SourceReg = Registers.CH };
            new Move { DestinationReg = Registers.AH, SourceReg = Registers.DL };
            new Move { DestinationReg = Registers.AH, SourceReg = Registers.DH };
            new Move { DestinationReg = Registers.BL, SourceReg = Registers.AL };
            new Move { DestinationReg = Registers.BL, SourceReg = Registers.AH };
            new Move { DestinationReg = Registers.BL, SourceReg = Registers.BL };
            new Move { DestinationReg = Registers.BL, SourceReg = Registers.BH };
            new Move { DestinationReg = Registers.BL, SourceReg = Registers.CL };
            new Move { DestinationReg = Registers.BL, SourceReg = Registers.CH };
            new Move { DestinationReg = Registers.BL, SourceReg = Registers.DL };
            new Move { DestinationReg = Registers.BL, SourceReg = Registers.DH };
            new Move { DestinationReg = Registers.BH, SourceReg = Registers.AL };
            new Move { DestinationReg = Registers.BH, SourceReg = Registers.AH };
            new Move { DestinationReg = Registers.BH, SourceReg = Registers.BL };
            new Move { DestinationReg = Registers.BH, SourceReg = Registers.BH };
            new Move { DestinationReg = Registers.BH, SourceReg = Registers.CL };
            new Move { DestinationReg = Registers.BH, SourceReg = Registers.CH };
            new Move { DestinationReg = Registers.BH, SourceReg = Registers.DL };
            new Move { DestinationReg = Registers.BH, SourceReg = Registers.DH };
            new Move { DestinationReg = Registers.CL, SourceReg = Registers.AL };
            new Move { DestinationReg = Registers.CL, SourceReg = Registers.AH };
            new Move { DestinationReg = Registers.CL, SourceReg = Registers.BL };
            new Move { DestinationReg = Registers.CL, SourceReg = Registers.BH };
            new Move { DestinationReg = Registers.CL, SourceReg = Registers.CL };
            new Move { DestinationReg = Registers.CL, SourceReg = Registers.CH };
            new Move { DestinationReg = Registers.CL, SourceReg = Registers.DL };
            new Move { DestinationReg = Registers.CL, SourceReg = Registers.DH };
            new Move { DestinationReg = Registers.CH, SourceReg = Registers.AL };
            new Move { DestinationReg = Registers.CH, SourceReg = Registers.AH };
            new Move { DestinationReg = Registers.CH, SourceReg = Registers.BL };
            new Move { DestinationReg = Registers.CH, SourceReg = Registers.BH };
            new Move { DestinationReg = Registers.CH, SourceReg = Registers.CL };
            new Move { DestinationReg = Registers.CH, SourceReg = Registers.CH };
            new Move { DestinationReg = Registers.CH, SourceReg = Registers.DL };
            new Move { DestinationReg = Registers.CH, SourceReg = Registers.DH };
            new Move { DestinationReg = Registers.DL, SourceReg = Registers.AL };
            new Move { DestinationReg = Registers.DL, SourceReg = Registers.AH };
            new Move { DestinationReg = Registers.DL, SourceReg = Registers.BL };
            new Move { DestinationReg = Registers.DL, SourceReg = Registers.BH };
            new Move { DestinationReg = Registers.DL, SourceReg = Registers.CL };
            new Move { DestinationReg = Registers.DL, SourceReg = Registers.CH };
            new Move { DestinationReg = Registers.DL, SourceReg = Registers.DL };
            new Move { DestinationReg = Registers.DL, SourceReg = Registers.DH };
            new Move { DestinationReg = Registers.DH, SourceReg = Registers.AL };
            new Move { DestinationReg = Registers.DH, SourceReg = Registers.AH };
            new Move { DestinationReg = Registers.DH, SourceReg = Registers.BL };
            new Move { DestinationReg = Registers.DH, SourceReg = Registers.BH };
            new Move { DestinationReg = Registers.DH, SourceReg = Registers.CL };
            new Move { DestinationReg = Registers.DH, SourceReg = Registers.CH };
            new Move { DestinationReg = Registers.DH, SourceReg = Registers.DL };
            new Move { DestinationReg = Registers.DH, SourceReg = Registers.DH };
            Verify();
        }

        [Test]
        public void TestImmediateToMemory8BitOffset32BitData() {
            new Move { DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 203, SourceValue = 65, Size = 32 };
            new Move { DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 203, SourceValue = 65, Size = 32 };
            new Move { DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 203, SourceValue = 65, Size = 32 };
            new Move { DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 203, SourceValue = 65, Size = 32 };
            new Move { DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 203, SourceValue = 65, Size = 32 };
            new Move { DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 203, SourceValue = 65, Size = 32 };
            new Move { DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 203, SourceValue = 65, Size = 32 };
            new Move { DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 203, SourceValue = 65, Size = 32 };
            Verify();
        }

        [Test]
        public void TestImmediateToMemory8BitOffset16BitData() {
            new Move { DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 203, SourceValue = 65, Size = 16 };
            new Move { DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 203, SourceValue = 65, Size = 16 };
            new Move { DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 203, SourceValue = 65, Size = 16 };
            new Move { DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 203, SourceValue = 65, Size = 16 };
            new Move { DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 203, SourceValue = 65, Size = 16 };
            new Move { DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 203, SourceValue = 65, Size = 16 };
            new Move { DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 203, SourceValue = 65, Size = 16 };
            new Move { DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 203, SourceValue = 65, Size = 16 };
            Verify();
        }

        [Test]
        public void TestImmediateToMemory8BitOffset8BitData() {
            new Move { DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 203, SourceValue = 65, Size = 8 };
            new Move { DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 203, SourceValue = 65, Size = 8 };
            new Move { DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 203, SourceValue = 65, Size = 8 };
            new Move { DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 203, SourceValue = 65, Size = 8 };
            new Move { DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 203, SourceValue = 65, Size = 8 };
            new Move { DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 203, SourceValue = 65, Size = 8 };
            new Move { DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 203, SourceValue = 65, Size = 8 };
            new Move { DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 203, SourceValue = 65, Size = 8 };
            Verify();
        }

        [Test]
        public void TestImmediateToMemory16BitOffset32BitData() {
            new Move { DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 1203, SourceValue = 65, Size = 32 };
            new Move { DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 1203, SourceValue = 65, Size = 32 };
            new Move { DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 1203, SourceValue = 65, Size = 32 };
            new Move { DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 1203, SourceValue = 65, Size = 32 };
            new Move { DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 1203, SourceValue = 65, Size = 32 };
            new Move { DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 1203, SourceValue = 65, Size = 32 };
            new Move { DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 1203, SourceValue = 65, Size = 32 };
            new Move { DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 1203, SourceValue = 65, Size = 32 };
            Verify();
        }

        [Test]
        public void TestImmediateToMemory16BitOffset16BitData() {
            new Move { DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 1203, SourceValue = 65, Size = 16 };
            new Move { DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 1203, SourceValue = 65, Size = 16 };
            new Move { DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 1203, SourceValue = 65, Size = 16 };
            new Move { DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 1203, SourceValue = 65, Size = 16 };
            new Move { DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 1203, SourceValue = 65, Size = 16 };
            new Move { DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 1203, SourceValue = 65, Size = 16 };
            new Move { DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 1203, SourceValue = 65, Size = 16 };
            new Move { DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 1203, SourceValue = 65, Size = 16 };
            Verify();
        }

        [Test]
        public void TestImmediateToMemory16BitOffset8BitData() {
            new Move { DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 1203, SourceValue = 65, Size = 8 };
            new Move { DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 1203, SourceValue = 65, Size = 8 };
            new Move { DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 1203, SourceValue = 65, Size = 8 };
            new Move { DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 1203, SourceValue = 65, Size = 8 };
            new Move { DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 1203, SourceValue = 65, Size = 8 };
            new Move { DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 1203, SourceValue = 65, Size = 8 };
            new Move { DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 1203, SourceValue = 65, Size = 8 };
            new Move { DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 1203, SourceValue = 65, Size = 8 };
            Verify();
        }

        [Test]
        public void TestImmediateToMemory32BitOffset32BitData() {
            new Move { DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x56781203, SourceValue = 65, Size = 32 };
            new Move { DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x56781203, SourceValue = 65, Size = 32 };
            new Move { DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x56781203, SourceValue = 65, Size = 32 };
            new Move { DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x56781203, SourceValue = 65, Size = 32 };
            new Move { DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x56781203, SourceValue = 65, Size = 32 };
            new Move { DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x56781203, SourceValue = 65, Size = 32 };
            new Move { DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x56781203, SourceValue = 65, Size = 32 };
            new Move { DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x56781203, SourceValue = 65, Size = 32 };
            Verify();
        }

        [Test]
        public void TestImmediateToMemory32BitOffset16BitData() {
            new Move { DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x56781203, SourceValue = 65, Size = 16 };
            new Move { DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x56781203, SourceValue = 65, Size = 16 };
            new Move { DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x56781203, SourceValue = 65, Size = 16 };
            new Move { DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x56781203, SourceValue = 65, Size = 16 };
            new Move { DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x56781203, SourceValue = 65, Size = 16 };
            new Move { DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x56781203, SourceValue = 65, Size = 16 };
            new Move { DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x56781203, SourceValue = 65, Size = 16 };
            new Move { DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x56781203, SourceValue = 65, Size = 16 };
            Verify();
        }

        [Test]
        public void TestImmediateToMemory32BitOffset8BitData() {
            new Move { DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x56781203, SourceValue = 65, Size = 8 };
            new Move { DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x56781203, SourceValue = 65, Size = 8 };
            new Move { DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x56781203, SourceValue = 65, Size = 8 };
            new Move { DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x56781203, SourceValue = 65, Size = 8 };
            new Move { DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x56781203, SourceValue = 65, Size = 8 };
            new Move { DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x56781203, SourceValue = 65, Size = 8 };
            new Move { DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x56781203, SourceValue = 65, Size = 8 };
            new Move { DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x56781203, SourceValue = 65, Size = 8 };
            Verify();
        }

        [Test]
        public void TestMemoryReg32BitOffsetToReg8BitData() {
            new Move { DestinationReg = Registers.AL, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.EAX };
            new Move { DestinationReg = Registers.BL, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.EAX };
            new Move { DestinationReg = Registers.CL, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.EAX };
            new Move { DestinationReg = Registers.DL, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.EAX };
            new Move { DestinationReg = Registers.AH, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.EAX };
            new Move { DestinationReg = Registers.BH, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.EAX };
            new Move { DestinationReg = Registers.CH, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.EAX };
            new Move { DestinationReg = Registers.DH, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.EAX };
            new Move { DestinationReg = Registers.AL, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.EBX };
            new Move { DestinationReg = Registers.BL, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.EBX };
            new Move { DestinationReg = Registers.CL, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.EBX };
            new Move { DestinationReg = Registers.DL, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.EBX };
            new Move { DestinationReg = Registers.AH, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.EBX };
            new Move { DestinationReg = Registers.BH, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.EBX };
            new Move { DestinationReg = Registers.CH, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.EBX };
            new Move { DestinationReg = Registers.DH, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.EBX };
            new Move { DestinationReg = Registers.AL, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.ECX };
            new Move { DestinationReg = Registers.BL, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.ECX };
            new Move { DestinationReg = Registers.CL, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.ECX };
            new Move { DestinationReg = Registers.DL, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.ECX };
            new Move { DestinationReg = Registers.AH, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.ECX };
            new Move { DestinationReg = Registers.BH, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.ECX };
            new Move { DestinationReg = Registers.CH, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.ECX };
            new Move { DestinationReg = Registers.DH, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.ECX };
            new Move { DestinationReg = Registers.AL, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.EDX };
            new Move { DestinationReg = Registers.BL, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.EDX };
            new Move { DestinationReg = Registers.CL, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.EDX };
            new Move { DestinationReg = Registers.DL, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.EDX };
            new Move { DestinationReg = Registers.AH, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.EDX };
            new Move { DestinationReg = Registers.BH, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.EDX };
            new Move { DestinationReg = Registers.CH, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.EDX };
            new Move { DestinationReg = Registers.DH, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.EDX };
            new Move { DestinationReg = Registers.AL, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.ESI };
            new Move { DestinationReg = Registers.BL, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.ESI };
            new Move { DestinationReg = Registers.CL, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.ESI };
            new Move { DestinationReg = Registers.DL, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.ESI };
            new Move { DestinationReg = Registers.AH, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.ESI };
            new Move { DestinationReg = Registers.BH, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.ESI };
            new Move { DestinationReg = Registers.CH, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.ESI };
            new Move { DestinationReg = Registers.DH, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.ESI };
            new Move { DestinationReg = Registers.AL, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.EDI };
            new Move { DestinationReg = Registers.BL, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.EDI };
            new Move { DestinationReg = Registers.CL, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.EDI };
            new Move { DestinationReg = Registers.DL, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.EDI };
            new Move { DestinationReg = Registers.AH, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.EDI };
            new Move { DestinationReg = Registers.BH, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.EDI };
            new Move { DestinationReg = Registers.CH, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.EDI };
            new Move { DestinationReg = Registers.DH, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.EDI };
            new Move { DestinationReg = Registers.AL, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.EBP };
            new Move { DestinationReg = Registers.BL, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.EBP };
            new Move { DestinationReg = Registers.CL, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.EBP };
            new Move { DestinationReg = Registers.DL, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.EBP };
            new Move { DestinationReg = Registers.AH, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.EBP };
            new Move { DestinationReg = Registers.BH, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.EBP };
            new Move { DestinationReg = Registers.CH, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.EBP };
            new Move { DestinationReg = Registers.DH, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.EBP };
            new Move { DestinationReg = Registers.AL, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.ESP };
            new Move { DestinationReg = Registers.BL, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.ESP };
            new Move { DestinationReg = Registers.CL, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.ESP };
            new Move { DestinationReg = Registers.DL, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.ESP };
            new Move { DestinationReg = Registers.AH, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.ESP };
            new Move { DestinationReg = Registers.BH, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.ESP };
            new Move { DestinationReg = Registers.CH, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.ESP };
            new Move { DestinationReg = Registers.DH, SourceIsIndirect = true, SourceDisplacement = 0x56781203, SourceReg = Registers.ESP };
            Verify();
        }

        [Test]
        public void TestMemoryReg16BitOffsetToReg8BitData() {
            new Move { DestinationReg = Registers.AL, SourceIsIndirect = true, SourceDisplacement = 54123, SourceReg = Registers.EAX };
            new Move { DestinationReg = Registers.BL, SourceIsIndirect = true, SourceDisplacement = 54123, SourceReg = Registers.EAX };
            new Move { DestinationReg = Registers.CL, SourceIsIndirect = true, SourceDisplacement = 54123, SourceReg = Registers.EAX };
            new Move { DestinationReg = Registers.DL, SourceIsIndirect = true, SourceDisplacement = 54123, SourceReg = Registers.EAX };
            new Move { DestinationReg = Registers.AH, SourceIsIndirect = true, SourceDisplacement = 54123, SourceReg = Registers.EAX };
            new Move { DestinationReg = Registers.BH, SourceIsIndirect = true, SourceDisplacement = 54123, SourceReg = Registers.EAX };
            new Move { DestinationReg = Registers.CH, SourceIsIndirect = true, SourceDisplacement = 54123, SourceReg = Registers.EAX };
            new Move { DestinationReg = Registers.DH, SourceIsIndirect = true, SourceDisplacement = 54123, SourceReg = Registers.EAX };
            new Move { DestinationReg = Registers.AL, SourceIsIndirect = true, SourceDisplacement = 54123, SourceReg = Registers.EBX };
            new Move { DestinationReg = Registers.BL, SourceIsIndirect = true, SourceDisplacement = 54123, SourceReg = Registers.EBX };
            new Move { DestinationReg = Registers.CL, SourceIsIndirect = true, SourceDisplacement = 54123, SourceReg = Registers.EBX };
            new Move { DestinationReg = Registers.DL, SourceIsIndirect = true, SourceDisplacement = 54123, SourceReg = Registers.EBX };
            new Move { DestinationReg = Registers.AH, SourceIsIndirect = true, SourceDisplacement = 54123, SourceReg = Registers.EBX };
            new Move { DestinationReg = Registers.BH, SourceIsIndirect = true, SourceDisplacement = 54123, SourceReg = Registers.EBX };
            new Move { DestinationReg = Registers.CH, SourceIsIndirect = true, SourceDisplacement = 54123, SourceReg = Registers.EBX };
            new Move { DestinationReg = Registers.DH, SourceIsIndirect = true, SourceDisplacement = 54123, SourceReg = Registers.EBX };
            new Move { DestinationReg = Registers.AL, SourceIsIndirect = true, SourceDisplacement = 54123, SourceReg = Registers.ECX };
            new Move { DestinationReg = Registers.BL, SourceIsIndirect = true, SourceDisplacement = 54123, SourceReg = Registers.ECX };
            new Move { DestinationReg = Registers.CL, SourceIsIndirect = true, SourceDisplacement = 54123, SourceReg = Registers.ECX };
            new Move { DestinationReg = Registers.DL, SourceIsIndirect = true, SourceDisplacement = 54123, SourceReg = Registers.ECX };
            new Move { DestinationReg = Registers.AH, SourceIsIndirect = true, SourceDisplacement = 54123, SourceReg = Registers.ECX };
            new Move { DestinationReg = Registers.BH, SourceIsIndirect = true, SourceDisplacement = 54123, SourceReg = Registers.ECX };
            new Move { DestinationReg = Registers.CH, SourceIsIndirect = true, SourceDisplacement = 54123, SourceReg = Registers.ECX };
            new Move { DestinationReg = Registers.DH, SourceIsIndirect = true, SourceDisplacement = 54123, SourceReg = Registers.ECX };
            new Move { DestinationReg = Registers.AL, SourceIsIndirect = true, SourceDisplacement = 54123, SourceReg = Registers.EDX };
            new Move { DestinationReg = Registers.BL, SourceIsIndirect = true, SourceDisplacement = 54123, SourceReg = Registers.EDX };
            new Move { DestinationReg = Registers.CL, SourceIsIndirect = true, SourceDisplacement = 54123, SourceReg = Registers.EDX };
            new Move { DestinationReg = Registers.DL, SourceIsIndirect = true, SourceDisplacement = 54123, SourceReg = Registers.EDX };
            new Move { DestinationReg = Registers.AH, SourceIsIndirect = true, SourceDisplacement = 54123, SourceReg = Registers.EDX };
            new Move { DestinationReg = Registers.BH, SourceIsIndirect = true, SourceDisplacement = 54123, SourceReg = Registers.EDX };
            new Move { DestinationReg = Registers.CH, SourceIsIndirect = true, SourceDisplacement = 54123, SourceReg = Registers.EDX };
            new Move { DestinationReg = Registers.DH, SourceIsIndirect = true, SourceDisplacement = 54123, SourceReg = Registers.EDX };
            new Move { DestinationReg = Registers.AL, SourceIsIndirect = true, SourceDisplacement = 54123, SourceReg = Registers.ESI };
            new Move { DestinationReg = Registers.BL, SourceIsIndirect = true, SourceDisplacement = 54123, SourceReg = Registers.ESI };
            new Move { DestinationReg = Registers.CL, SourceIsIndirect = true, SourceDisplacement = 54123, SourceReg = Registers.ESI };
            new Move { DestinationReg = Registers.DL, SourceIsIndirect = true, SourceDisplacement = 54123, SourceReg = Registers.ESI };
            new Move { DestinationReg = Registers.AH, SourceIsIndirect = true, SourceDisplacement = 54123, SourceReg = Registers.ESI };
            new Move { DestinationReg = Registers.BH, SourceIsIndirect = true, SourceDisplacement = 54123, SourceReg = Registers.ESI };
            new Move { DestinationReg = Registers.CH, SourceIsIndirect = true, SourceDisplacement = 54123, SourceReg = Registers.ESI };
            new Move { DestinationReg = Registers.DH, SourceIsIndirect = true, SourceDisplacement = 54123, SourceReg = Registers.ESI };
            new Move { DestinationReg = Registers.AL, SourceIsIndirect = true, SourceDisplacement = 54123, SourceReg = Registers.EDI };
            new Move { DestinationReg = Registers.BL, SourceIsIndirect = true, SourceDisplacement = 54123, SourceReg = Registers.EDI };
            new Move { DestinationReg = Registers.CL, SourceIsIndirect = true, SourceDisplacement = 54123, SourceReg = Registers.EDI };
            new Move { DestinationReg = Registers.DL, SourceIsIndirect = true, SourceDisplacement = 54123, SourceReg = Registers.EDI };
            new Move { DestinationReg = Registers.AH, SourceIsIndirect = true, SourceDisplacement = 54123, SourceReg = Registers.EDI };
            new Move { DestinationReg = Registers.BH, SourceIsIndirect = true, SourceDisplacement = 54123, SourceReg = Registers.EDI };
            new Move { DestinationReg = Registers.CH, SourceIsIndirect = true, SourceDisplacement = 54123, SourceReg = Registers.EDI };
            new Move { DestinationReg = Registers.DH, SourceIsIndirect = true, SourceDisplacement = 54123, SourceReg = Registers.EDI };
            new Move { DestinationReg = Registers.AL, SourceIsIndirect = true, SourceDisplacement = 54123, SourceReg = Registers.EBP };
            new Move { DestinationReg = Registers.BL, SourceIsIndirect = true, SourceDisplacement = 54123, SourceReg = Registers.EBP };
            new Move { DestinationReg = Registers.CL, SourceIsIndirect = true, SourceDisplacement = 54123, SourceReg = Registers.EBP };
            new Move { DestinationReg = Registers.DL, SourceIsIndirect = true, SourceDisplacement = 54123, SourceReg = Registers.EBP };
            new Move { DestinationReg = Registers.AH, SourceIsIndirect = true, SourceDisplacement = 54123, SourceReg = Registers.EBP };
            new Move { DestinationReg = Registers.BH, SourceIsIndirect = true, SourceDisplacement = 54123, SourceReg = Registers.EBP };
            new Move { DestinationReg = Registers.CH, SourceIsIndirect = true, SourceDisplacement = 54123, SourceReg = Registers.EBP };
            new Move { DestinationReg = Registers.DH, SourceIsIndirect = true, SourceDisplacement = 54123, SourceReg = Registers.EBP };
            new Move { DestinationReg = Registers.AL, SourceIsIndirect = true, SourceDisplacement = 54123, SourceReg = Registers.ESP };
            new Move { DestinationReg = Registers.BL, SourceIsIndirect = true, SourceDisplacement = 54123, SourceReg = Registers.ESP };
            new Move { DestinationReg = Registers.CL, SourceIsIndirect = true, SourceDisplacement = 54123, SourceReg = Registers.ESP };
            new Move { DestinationReg = Registers.DL, SourceIsIndirect = true, SourceDisplacement = 54123, SourceReg = Registers.ESP };
            new Move { DestinationReg = Registers.AH, SourceIsIndirect = true, SourceDisplacement = 54123, SourceReg = Registers.ESP };
            new Move { DestinationReg = Registers.BH, SourceIsIndirect = true, SourceDisplacement = 54123, SourceReg = Registers.ESP };
            new Move { DestinationReg = Registers.CH, SourceIsIndirect = true, SourceDisplacement = 54123, SourceReg = Registers.ESP };
            new Move { DestinationReg = Registers.DH, SourceIsIndirect = true, SourceDisplacement = 54123, SourceReg = Registers.ESP };
            Verify();
        }

        [Test]
        public void TestMemoryReg8BitOffsetToReg8BitData() {
            new Move { DestinationReg = Registers.AL, SourceIsIndirect = true, SourceDisplacement = 123, SourceReg = Registers.EAX };
            new Move { DestinationReg = Registers.BL, SourceIsIndirect = true, SourceDisplacement = 123, SourceReg = Registers.EAX };
            new Move { DestinationReg = Registers.CL, SourceIsIndirect = true, SourceDisplacement = 123, SourceReg = Registers.EAX };
            new Move { DestinationReg = Registers.DL, SourceIsIndirect = true, SourceDisplacement = 123, SourceReg = Registers.EAX };
            new Move { DestinationReg = Registers.AH, SourceIsIndirect = true, SourceDisplacement = 123, SourceReg = Registers.EAX };
            new Move { DestinationReg = Registers.BH, SourceIsIndirect = true, SourceDisplacement = 123, SourceReg = Registers.EAX };
            new Move { DestinationReg = Registers.CH, SourceIsIndirect = true, SourceDisplacement = 123, SourceReg = Registers.EAX };
            new Move { DestinationReg = Registers.DH, SourceIsIndirect = true, SourceDisplacement = 123, SourceReg = Registers.EAX };
            new Move { DestinationReg = Registers.AL, SourceIsIndirect = true, SourceDisplacement = 123, SourceReg = Registers.EBX };
            new Move { DestinationReg = Registers.BL, SourceIsIndirect = true, SourceDisplacement = 123, SourceReg = Registers.EBX };
            new Move { DestinationReg = Registers.CL, SourceIsIndirect = true, SourceDisplacement = 123, SourceReg = Registers.EBX };
            new Move { DestinationReg = Registers.DL, SourceIsIndirect = true, SourceDisplacement = 123, SourceReg = Registers.EBX };
            new Move { DestinationReg = Registers.AH, SourceIsIndirect = true, SourceDisplacement = 123, SourceReg = Registers.EBX };
            new Move { DestinationReg = Registers.BH, SourceIsIndirect = true, SourceDisplacement = 123, SourceReg = Registers.EBX };
            new Move { DestinationReg = Registers.CH, SourceIsIndirect = true, SourceDisplacement = 123, SourceReg = Registers.EBX };
            new Move { DestinationReg = Registers.DH, SourceIsIndirect = true, SourceDisplacement = 123, SourceReg = Registers.EBX };
            new Move { DestinationReg = Registers.AL, SourceIsIndirect = true, SourceDisplacement = 123, SourceReg = Registers.ECX };
            new Move { DestinationReg = Registers.BL, SourceIsIndirect = true, SourceDisplacement = 123, SourceReg = Registers.ECX };
            new Move { DestinationReg = Registers.CL, SourceIsIndirect = true, SourceDisplacement = 123, SourceReg = Registers.ECX };
            new Move { DestinationReg = Registers.DL, SourceIsIndirect = true, SourceDisplacement = 123, SourceReg = Registers.ECX };
            new Move { DestinationReg = Registers.AH, SourceIsIndirect = true, SourceDisplacement = 123, SourceReg = Registers.ECX };
            new Move { DestinationReg = Registers.BH, SourceIsIndirect = true, SourceDisplacement = 123, SourceReg = Registers.ECX };
            new Move { DestinationReg = Registers.CH, SourceIsIndirect = true, SourceDisplacement = 123, SourceReg = Registers.ECX };
            new Move { DestinationReg = Registers.DH, SourceIsIndirect = true, SourceDisplacement = 123, SourceReg = Registers.ECX };
            new Move { DestinationReg = Registers.AL, SourceIsIndirect = true, SourceDisplacement = 123, SourceReg = Registers.EDX };
            new Move { DestinationReg = Registers.BL, SourceIsIndirect = true, SourceDisplacement = 123, SourceReg = Registers.EDX };
            new Move { DestinationReg = Registers.CL, SourceIsIndirect = true, SourceDisplacement = 123, SourceReg = Registers.EDX };
            new Move { DestinationReg = Registers.DL, SourceIsIndirect = true, SourceDisplacement = 123, SourceReg = Registers.EDX };
            new Move { DestinationReg = Registers.AH, SourceIsIndirect = true, SourceDisplacement = 123, SourceReg = Registers.EDX };
            new Move { DestinationReg = Registers.BH, SourceIsIndirect = true, SourceDisplacement = 123, SourceReg = Registers.EDX };
            new Move { DestinationReg = Registers.CH, SourceIsIndirect = true, SourceDisplacement = 123, SourceReg = Registers.EDX };
            new Move { DestinationReg = Registers.DH, SourceIsIndirect = true, SourceDisplacement = 123, SourceReg = Registers.EDX };
            new Move { DestinationReg = Registers.AL, SourceIsIndirect = true, SourceDisplacement = 123, SourceReg = Registers.ESI };
            new Move { DestinationReg = Registers.BL, SourceIsIndirect = true, SourceDisplacement = 123, SourceReg = Registers.ESI };
            new Move { DestinationReg = Registers.CL, SourceIsIndirect = true, SourceDisplacement = 123, SourceReg = Registers.ESI };
            new Move { DestinationReg = Registers.DL, SourceIsIndirect = true, SourceDisplacement = 123, SourceReg = Registers.ESI };
            new Move { DestinationReg = Registers.AH, SourceIsIndirect = true, SourceDisplacement = 123, SourceReg = Registers.ESI };
            new Move { DestinationReg = Registers.BH, SourceIsIndirect = true, SourceDisplacement = 123, SourceReg = Registers.ESI };
            new Move { DestinationReg = Registers.CH, SourceIsIndirect = true, SourceDisplacement = 123, SourceReg = Registers.ESI };
            new Move { DestinationReg = Registers.DH, SourceIsIndirect = true, SourceDisplacement = 123, SourceReg = Registers.ESI };
            new Move { DestinationReg = Registers.AL, SourceIsIndirect = true, SourceDisplacement = 123, SourceReg = Registers.EDI };
            new Move { DestinationReg = Registers.BL, SourceIsIndirect = true, SourceDisplacement = 123, SourceReg = Registers.EDI };
            new Move { DestinationReg = Registers.CL, SourceIsIndirect = true, SourceDisplacement = 123, SourceReg = Registers.EDI };
            new Move { DestinationReg = Registers.DL, SourceIsIndirect = true, SourceDisplacement = 123, SourceReg = Registers.EDI };
            new Move { DestinationReg = Registers.AH, SourceIsIndirect = true, SourceDisplacement = 123, SourceReg = Registers.EDI };
            new Move { DestinationReg = Registers.BH, SourceIsIndirect = true, SourceDisplacement = 123, SourceReg = Registers.EDI };
            new Move { DestinationReg = Registers.CH, SourceIsIndirect = true, SourceDisplacement = 123, SourceReg = Registers.EDI };
            new Move { DestinationReg = Registers.DH, SourceIsIndirect = true, SourceDisplacement = 123, SourceReg = Registers.EDI };
            new Move { DestinationReg = Registers.AL, SourceIsIndirect = true, SourceDisplacement = 123, SourceReg = Registers.EBP };
            new Move { DestinationReg = Registers.BL, SourceIsIndirect = true, SourceDisplacement = 123, SourceReg = Registers.EBP };
            new Move { DestinationReg = Registers.CL, SourceIsIndirect = true, SourceDisplacement = 123, SourceReg = Registers.EBP };
            new Move { DestinationReg = Registers.DL, SourceIsIndirect = true, SourceDisplacement = 123, SourceReg = Registers.EBP };
            new Move { DestinationReg = Registers.AH, SourceIsIndirect = true, SourceDisplacement = 123, SourceReg = Registers.EBP };
            new Move { DestinationReg = Registers.BH, SourceIsIndirect = true, SourceDisplacement = 123, SourceReg = Registers.EBP };
            new Move { DestinationReg = Registers.CH, SourceIsIndirect = true, SourceDisplacement = 123, SourceReg = Registers.EBP };
            new Move { DestinationReg = Registers.DH, SourceIsIndirect = true, SourceDisplacement = 123, SourceReg = Registers.EBP };
            new Move { DestinationReg = Registers.AL, SourceIsIndirect = true, SourceDisplacement = 123, SourceReg = Registers.ESP };
            new Move { DestinationReg = Registers.BL, SourceIsIndirect = true, SourceDisplacement = 123, SourceReg = Registers.ESP };
            new Move { DestinationReg = Registers.CL, SourceIsIndirect = true, SourceDisplacement = 123, SourceReg = Registers.ESP };
            new Move { DestinationReg = Registers.DL, SourceIsIndirect = true, SourceDisplacement = 123, SourceReg = Registers.ESP };
            new Move { DestinationReg = Registers.AH, SourceIsIndirect = true, SourceDisplacement = 123, SourceReg = Registers.ESP };
            new Move { DestinationReg = Registers.BH, SourceIsIndirect = true, SourceDisplacement = 123, SourceReg = Registers.ESP };
            new Move { DestinationReg = Registers.CH, SourceIsIndirect = true, SourceDisplacement = 123, SourceReg = Registers.ESP };
            new Move { DestinationReg = Registers.DH, SourceIsIndirect = true, SourceDisplacement = 123, SourceReg = Registers.ESP };
            Verify();
        }

    }
}