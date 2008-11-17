using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.Tests.Assembler.X86 {
    [TestFixture]
    public class SubTests : BaseTest {
        /*
         * situations to cover:
         * register to memory indirect + dword (8bit, 16bit, 32bit)
         * register to memoryreg  indirect + byte (8bit, 16bit, 32bit)
         * register to memoryreg indirect + dword (8bit, 16bit, 32bit)
         */
        [Test]
        public void TestImmediateToMemoryReg8BitOffset8() {
            new Sub { DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 65, SourceValue = 70, Size = 8 };
            new Sub { DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 65, SourceValue = 70, Size = 8 };
            new Sub { DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 65, SourceValue = 70, Size = 8 };
            new Sub { DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 65, SourceValue = 70, Size = 8 };
            new Sub { DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 65, SourceValue = 70, Size = 8 };
            new Sub { DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 65, SourceValue = 70, Size = 8 };
            new Sub { DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 65, SourceValue = 70, Size = 8 };
            new Sub { DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 65, SourceValue = 70, Size = 8 };
        }

        [Test]
        public void TestImmediateToMemoryReg16BitOffset8() {
            new Sub { DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 650, SourceValue = 70, Size = 8 };
            new Sub { DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 650, SourceValue = 70, Size = 8 };
            new Sub { DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 650, SourceValue = 70, Size = 8 };
            new Sub { DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 650, SourceValue = 70, Size = 8 };
            new Sub { DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 650, SourceValue = 70, Size = 8 };
            new Sub { DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 650, SourceValue = 70, Size = 8 };
            new Sub { DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 650, SourceValue = 70, Size = 8 };
            new Sub { DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 650, SourceValue = 70, Size = 8 };
        }

        [Test]
        public void TestImmediateToMemoryReg32BitOffset8() {
            new Sub { DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 650000, SourceValue = 70, Size = 8 };
            new Sub { DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 650000, SourceValue = 70, Size = 8 };
            new Sub { DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 650000, SourceValue = 70, Size = 8 };
            new Sub { DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 650000, SourceValue = 70, Size = 8 };
            new Sub { DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 650000, SourceValue = 70, Size = 8 };
            new Sub { DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 650000, SourceValue = 70, Size = 8 };
            new Sub { DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 650000, SourceValue = 70, Size = 8 };
            new Sub { DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 650000, SourceValue = 70, Size = 8 };
        }

        [Test]
        public void TestImmediateToMemoryReg8BitOffset16() {
            new Sub { DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 65, SourceValue = 70, Size = 16 };
            new Sub { DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 65, SourceValue = 70, Size = 16 };
            new Sub { DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 65, SourceValue = 70, Size = 16 };
            new Sub { DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 65, SourceValue = 70, Size = 16 };
            new Sub { DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 65, SourceValue = 70, Size = 16 };
            new Sub { DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 65, SourceValue = 70, Size = 16 };
            new Sub { DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 65, SourceValue = 70, Size = 16 };
            new Sub { DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 65, SourceValue = 70, Size = 16 };
        }

        [Test]
        public void TestImmediateToMemoryReg16BitOffset16() {
            new Sub { DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 650, SourceValue = 70, Size = 16 };
            new Sub { DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 650, SourceValue = 70, Size = 16 };
            new Sub { DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 650, SourceValue = 70, Size = 16 };
            new Sub { DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 650, SourceValue = 70, Size = 16 };
            new Sub { DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 650, SourceValue = 70, Size = 16 };
            new Sub { DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 650, SourceValue = 70, Size = 16 };
            new Sub { DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 650, SourceValue = 70, Size = 16 };
            new Sub { DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 650, SourceValue = 70, Size = 16 };
        }

        [Test]
        public void TestImmediateToMemoryReg32BitOffset16() {
            new Sub { DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 650000, SourceValue = 70, Size = 16 };
            new Sub { DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 650000, SourceValue = 70, Size = 16 };
            new Sub { DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 650000, SourceValue = 70, Size = 16 };
            new Sub { DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 650000, SourceValue = 70, Size = 16 };
            new Sub { DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 650000, SourceValue = 70, Size = 16 };
            new Sub { DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 650000, SourceValue = 70, Size = 16 };
            new Sub { DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 650000, SourceValue = 70, Size = 16 };
            new Sub { DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 650000, SourceValue = 70, Size = 16 };
        }

        [Test]
        public void TestImmediateToMemoryReg8BitOffset32() {
            new Sub { DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 65, SourceValue = 70, Size = 32 };
            new Sub { DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 65, SourceValue = 70, Size = 32 };
            new Sub { DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 65, SourceValue = 70, Size = 32 };
            new Sub { DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 65, SourceValue = 70, Size = 32 };
            new Sub { DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 65, SourceValue = 70, Size = 32 };
            new Sub { DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 65, SourceValue = 70, Size = 32 };
            new Sub { DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 65, SourceValue = 70, Size = 32 };
            new Sub { DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 65, SourceValue = 70, Size = 32 };
        }

        [Test]
        public void TestImmediateToMemoryReg16BitOffset32() {
            new Sub { DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 650, SourceValue = 70, Size = 32 };
            new Sub { DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 650, SourceValue = 70, Size = 32 };
            new Sub { DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 650, SourceValue = 70, Size = 32 };
            new Sub { DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 650, SourceValue = 70, Size = 32 };
            new Sub { DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 650, SourceValue = 70, Size = 32 };
            new Sub { DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 650, SourceValue = 70, Size = 32 };
            new Sub { DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 650, SourceValue = 70, Size = 32 };
            new Sub { DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 650, SourceValue = 70, Size = 32 };
        }

        [Test]
        public void TestImmediateToMemoryReg32BitOffset32() {
            new Sub { DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 650000, SourceValue = 70, Size = 32 };
            new Sub { DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 650000, SourceValue = 70, Size = 32 };
            new Sub { DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 650000, SourceValue = 70, Size = 32 };
            new Sub { DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 650000, SourceValue = 70, Size = 32 };
            new Sub { DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 650000, SourceValue = 70, Size = 32 };
            new Sub { DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 650000, SourceValue = 70, Size = 32 };
            new Sub { DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 650000, SourceValue = 70, Size = 32 };
            new Sub { DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 650000, SourceValue = 70, Size = 32 };
        }

        [Test]
        public void TestRegisterToMemory32BitOffset32() {
            new Sub { Size = 32, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.EAX };
            new Sub { Size = 32, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.EBX };
            new Sub { Size = 32, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.ECX };
            new Sub { Size = 32, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.EDX };
            new Sub { Size = 32, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.EDI };
            new Sub { Size = 32, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.ESI };
            new Sub { Size = 32, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.EBP };
            new Sub { Size = 32, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.ESP };
            new Sub { Size = 32, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.EAX };
            new Sub { Size = 32, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.EBX };
            new Sub { Size = 32, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.ECX };
            new Sub { Size = 32, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.EDX };
            new Sub { Size = 32, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.EDI };
            new Sub { Size = 32, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.ESI };
            new Sub { Size = 32, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.EBP };
            new Sub { Size = 32, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.ESP };
            new Sub { Size = 32, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.EAX };
            new Sub { Size = 32, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.EBX };
            new Sub { Size = 32, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.ECX };
            new Sub { Size = 32, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.EDX };
            new Sub { Size = 32, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.EDI };
            new Sub { Size = 32, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.ESI };
            new Sub { Size = 32, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.EBP };
            new Sub { Size = 32, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.ESP };
            new Sub { Size = 32, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.EAX };
            new Sub { Size = 32, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.EBX };
            new Sub { Size = 32, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.ECX };
            new Sub { Size = 32, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.EDX };
            new Sub { Size = 32, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.EDI };
            new Sub { Size = 32, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.ESI };
            new Sub { Size = 32, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.EBP };
            new Sub { Size = 32, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.ESP };
            new Sub { Size = 32, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.EAX };
            new Sub { Size = 32, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.EBX };
            new Sub { Size = 32, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.ECX };
            new Sub { Size = 32, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.EDX };
            new Sub { Size = 32, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.EDI };
            new Sub { Size = 32, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.ESI };
            new Sub { Size = 32, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.EBP };
            new Sub { Size = 32, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.ESP };
            new Sub { Size = 32, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.EAX };
            new Sub { Size = 32, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.EBX };
            new Sub { Size = 32, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.ECX };
            new Sub { Size = 32, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.EDX };
            new Sub { Size = 32, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.EDI };
            new Sub { Size = 32, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.ESI };
            new Sub { Size = 32, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.EBP };
            new Sub { Size = 32, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.ESP };
            new Sub { Size = 32, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.EAX };
            new Sub { Size = 32, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.EBX };
            new Sub { Size = 32, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.ECX };
            new Sub { Size = 32, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.EDX };
            new Sub { Size = 32, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.EDI };
            new Sub { Size = 32, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.ESI };
            new Sub { Size = 32, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.EBP };
            new Sub { Size = 32, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.ESP };
            new Sub { Size = 32, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.EAX };
            new Sub { Size = 32, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.EBX };
            new Sub { Size = 32, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.ECX };
            new Sub { Size = 32, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.EDX };
            new Sub { Size = 32, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.EDI };
            new Sub { Size = 32, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.ESI };
            new Sub { Size = 32, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.EBP };
            new Sub { Size = 32, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.ESP };
            Verify();
        }

        [Test]
        public void TestRegisterToMemory32BitOffset16() {
            new Sub { Size = 16, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.AX };
            new Sub { Size = 16, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.BX };
            new Sub { Size = 16, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.CX };
            new Sub { Size = 16, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.DX };
            new Sub { Size = 16, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.DI };
            new Sub { Size = 16, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.SI };
            new Sub { Size = 16, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.BP };
            new Sub { Size = 16, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.SP };
            new Sub { Size = 16, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.AX };
            new Sub { Size = 16, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.BX };
            new Sub { Size = 16, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.CX };
            new Sub { Size = 16, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.DX };
            new Sub { Size = 16, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.DI };
            new Sub { Size = 16, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.SI };
            new Sub { Size = 16, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.BP };
            new Sub { Size = 16, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.SP };
            new Sub { Size = 16, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.AX };
            new Sub { Size = 16, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.BX };
            new Sub { Size = 16, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.CX };
            new Sub { Size = 16, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.DX };
            new Sub { Size = 16, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.DI };
            new Sub { Size = 16, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.SI };
            new Sub { Size = 16, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.BP };
            new Sub { Size = 16, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.SP };
            new Sub { Size = 16, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.AX };
            new Sub { Size = 16, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.BX };
            new Sub { Size = 16, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.CX };
            new Sub { Size = 16, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.DX };
            new Sub { Size = 16, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.DI };
            new Sub { Size = 16, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.SI };
            new Sub { Size = 16, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.BP };
            new Sub { Size = 16, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.SP };
            new Sub { Size = 16, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.AX };
            new Sub { Size = 16, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.BX };
            new Sub { Size = 16, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.CX };
            new Sub { Size = 16, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.DX };
            new Sub { Size = 16, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.DI };
            new Sub { Size = 16, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.SI };
            new Sub { Size = 16, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.BP };
            new Sub { Size = 16, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.SP };
            new Sub { Size = 16, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.AX };
            new Sub { Size = 16, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.BX };
            new Sub { Size = 16, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.CX };
            new Sub { Size = 16, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.DX };
            new Sub { Size = 16, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.DI };
            new Sub { Size = 16, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.SI };
            new Sub { Size = 16, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.BP };
            new Sub { Size = 16, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.SP };
            new Sub { Size = 16, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.AX };
            new Sub { Size = 16, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.BX };
            new Sub { Size = 16, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.CX };
            new Sub { Size = 16, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.DX };
            new Sub { Size = 16, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.DI };
            new Sub { Size = 16, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.SI };
            new Sub { Size = 16, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.BP };
            new Sub { Size = 16, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.SP };
            new Sub { Size = 16, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.AX };
            new Sub { Size = 16, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.BX };
            new Sub { Size = 16, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.CX };
            new Sub { Size = 16, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.DX };
            new Sub { Size = 16, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.DI };
            new Sub { Size = 16, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.SI };
            new Sub { Size = 16, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.BP };
            new Sub { Size = 16, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.SP };
            Verify();
        }

        [Test]
        public void TestRegisterToMemory32BitOffset8() {
            new Sub { Size = 8, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.AL };
            new Sub { Size = 8, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.BL };
            new Sub { Size = 8, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.CL };
            new Sub { Size = 8, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.DL };
            new Sub { Size = 8, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.AH };
            new Sub { Size = 8, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.BH };
            new Sub { Size = 8, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.CH };
            new Sub { Size = 8, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.DH };
            new Sub { Size = 8, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.AL };
            new Sub { Size = 8, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.BL };
            new Sub { Size = 8, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.CL };
            new Sub { Size = 8, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.DL };
            new Sub { Size = 8, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.AH };
            new Sub { Size = 8, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.BH };
            new Sub { Size = 8, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.CH };
            new Sub { Size = 8, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.DH };
            new Sub { Size = 8, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.AL };
            new Sub { Size = 8, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.BL };
            new Sub { Size = 8, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.CL };
            new Sub { Size = 8, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.DL };
            new Sub { Size = 8, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.AH };
            new Sub { Size = 8, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.BH };
            new Sub { Size = 8, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.CH };
            new Sub { Size = 8, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.DH };
            new Sub { Size = 8, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.AL };
            new Sub { Size = 8, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.BL };
            new Sub { Size = 8, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.CL };
            new Sub { Size = 8, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.DL };
            new Sub { Size = 8, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.AH };
            new Sub { Size = 8, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.BH };
            new Sub { Size = 8, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.CH };
            new Sub { Size = 8, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.DH };
            new Sub { Size = 8, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.AL };
            new Sub { Size = 8, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.BL };
            new Sub { Size = 8, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.CL };
            new Sub { Size = 8, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.DL };
            new Sub { Size = 8, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.AH };
            new Sub { Size = 8, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.BH };
            new Sub { Size = 8, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.CH };
            new Sub { Size = 8, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.DH };
            new Sub { Size = 8, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.AL };
            new Sub { Size = 8, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.BL };
            new Sub { Size = 8, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.CL };
            new Sub { Size = 8, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.DL };
            new Sub { Size = 8, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.AH };
            new Sub { Size = 8, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.BH };
            new Sub { Size = 8, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.CH };
            new Sub { Size = 8, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.DH };
            new Sub { Size = 8, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.AL };
            new Sub { Size = 8, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.BL };
            new Sub { Size = 8, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.CL };
            new Sub { Size = 8, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.DL };
            new Sub { Size = 8, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.AH };
            new Sub { Size = 8, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.BH };
            new Sub { Size = 8, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.CH };
            new Sub { Size = 8, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.DH };
            new Sub { Size = 8, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.AL };
            new Sub { Size = 8, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.BL };
            new Sub { Size = 8, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.CL };
            new Sub { Size = 8, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.DL };
            new Sub { Size = 8, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.AH };
            new Sub { Size = 8, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.BH };
            new Sub { Size = 8, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.CH };
            new Sub { Size = 8, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x65000, SourceReg = Registers.DH };
            Verify();
        }

        [Test]
        public void TestRegisterToMemory16BitOffset32() {
            new Sub { Size = 32, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.EAX };
            new Sub { Size = 32, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.EBX };
            new Sub { Size = 32, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.ECX };
            new Sub { Size = 32, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.EDX };
            new Sub { Size = 32, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.EDI };
            new Sub { Size = 32, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.ESI };
            new Sub { Size = 32, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.EBP };
            new Sub { Size = 32, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.ESP };
            new Sub { Size = 32, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.EAX };
            new Sub { Size = 32, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.EBX };
            new Sub { Size = 32, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.ECX };
            new Sub { Size = 32, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.EDX };
            new Sub { Size = 32, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.EDI };
            new Sub { Size = 32, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.ESI };
            new Sub { Size = 32, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.EBP };
            new Sub { Size = 32, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.ESP };
            new Sub { Size = 32, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.EAX };
            new Sub { Size = 32, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.EBX };
            new Sub { Size = 32, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.ECX };
            new Sub { Size = 32, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.EDX };
            new Sub { Size = 32, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.EDI };
            new Sub { Size = 32, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.ESI };
            new Sub { Size = 32, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.EBP };
            new Sub { Size = 32, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.ESP };
            new Sub { Size = 32, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.EAX };
            new Sub { Size = 32, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.EBX };
            new Sub { Size = 32, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.ECX };
            new Sub { Size = 32, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.EDX };
            new Sub { Size = 32, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.EDI };
            new Sub { Size = 32, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.ESI };
            new Sub { Size = 32, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.EBP };
            new Sub { Size = 32, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.ESP };
            new Sub { Size = 32, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.EAX };
            new Sub { Size = 32, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.EBX };
            new Sub { Size = 32, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.ECX };
            new Sub { Size = 32, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.EDX };
            new Sub { Size = 32, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.EDI };
            new Sub { Size = 32, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.ESI };
            new Sub { Size = 32, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.EBP };
            new Sub { Size = 32, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.ESP };
            new Sub { Size = 32, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.EAX };
            new Sub { Size = 32, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.EBX };
            new Sub { Size = 32, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.ECX };
            new Sub { Size = 32, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.EDX };
            new Sub { Size = 32, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.EDI };
            new Sub { Size = 32, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.ESI };
            new Sub { Size = 32, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.EBP };
            new Sub { Size = 32, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.ESP };
            new Sub { Size = 32, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.EAX };
            new Sub { Size = 32, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.EBX };
            new Sub { Size = 32, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.ECX };
            new Sub { Size = 32, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.EDX };
            new Sub { Size = 32, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.EDI };
            new Sub { Size = 32, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.ESI };
            new Sub { Size = 32, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.EBP };
            new Sub { Size = 32, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.ESP };
            new Sub { Size = 32, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.EAX };
            new Sub { Size = 32, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.EBX };
            new Sub { Size = 32, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.ECX };
            new Sub { Size = 32, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.EDX };
            new Sub { Size = 32, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.EDI };
            new Sub { Size = 32, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.ESI };
            new Sub { Size = 32, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.EBP };
            new Sub { Size = 32, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.ESP };
            Verify();
        }

        [Test]
        public void TestRegisterToMemory16BitOffset16() {
            new Sub { Size = 16, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.AX };
            new Sub { Size = 16, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.BX };
            new Sub { Size = 16, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.CX };
            new Sub { Size = 16, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.DX };
            new Sub { Size = 16, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.DI };
            new Sub { Size = 16, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.SI };
            new Sub { Size = 16, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.BP };
            new Sub { Size = 16, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.SP };
            new Sub { Size = 16, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.AX };
            new Sub { Size = 16, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.BX };
            new Sub { Size = 16, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.CX };
            new Sub { Size = 16, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.DX };
            new Sub { Size = 16, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.DI };
            new Sub { Size = 16, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.SI };
            new Sub { Size = 16, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.BP };
            new Sub { Size = 16, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.SP };
            new Sub { Size = 16, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.AX };
            new Sub { Size = 16, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.BX };
            new Sub { Size = 16, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.CX };
            new Sub { Size = 16, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.DX };
            new Sub { Size = 16, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.DI };
            new Sub { Size = 16, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.SI };
            new Sub { Size = 16, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.BP };
            new Sub { Size = 16, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.SP };
            new Sub { Size = 16, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.AX };
            new Sub { Size = 16, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.BX };
            new Sub { Size = 16, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.CX };
            new Sub { Size = 16, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.DX };
            new Sub { Size = 16, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.DI };
            new Sub { Size = 16, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.SI };
            new Sub { Size = 16, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.BP };
            new Sub { Size = 16, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.SP };
            new Sub { Size = 16, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.AX };
            new Sub { Size = 16, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.BX };
            new Sub { Size = 16, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.CX };
            new Sub { Size = 16, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.DX };
            new Sub { Size = 16, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.DI };
            new Sub { Size = 16, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.SI };
            new Sub { Size = 16, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.BP };
            new Sub { Size = 16, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.SP };
            new Sub { Size = 16, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.AX };
            new Sub { Size = 16, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.BX };
            new Sub { Size = 16, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.CX };
            new Sub { Size = 16, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.DX };
            new Sub { Size = 16, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.DI };
            new Sub { Size = 16, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.SI };
            new Sub { Size = 16, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.BP };
            new Sub { Size = 16, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.SP };
            new Sub { Size = 16, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.AX };
            new Sub { Size = 16, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.BX };
            new Sub { Size = 16, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.CX };
            new Sub { Size = 16, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.DX };
            new Sub { Size = 16, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.DI };
            new Sub { Size = 16, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.SI };
            new Sub { Size = 16, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.BP };
            new Sub { Size = 16, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.SP };
            new Sub { Size = 16, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.AX };
            new Sub { Size = 16, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.BX };
            new Sub { Size = 16, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.CX };
            new Sub { Size = 16, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.DX };
            new Sub { Size = 16, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.DI };
            new Sub { Size = 16, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.SI };
            new Sub { Size = 16, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.BP };
            new Sub { Size = 16, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.SP };
            Verify();
        }

        [Test]
        public void TestRegisterToMemory16BitOffset8() {
            new Sub { Size = 8, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.AL };
            new Sub { Size = 8, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.BL };
            new Sub { Size = 8, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.CL };
            new Sub { Size = 8, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.DL };
            new Sub { Size = 8, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.AH };
            new Sub { Size = 8, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.BH };
            new Sub { Size = 8, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.CH };
            new Sub { Size = 8, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.DH };
            new Sub { Size = 8, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.AL };
            new Sub { Size = 8, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.BL };
            new Sub { Size = 8, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.CL };
            new Sub { Size = 8, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.DL };
            new Sub { Size = 8, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.AH };
            new Sub { Size = 8, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.BH };
            new Sub { Size = 8, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.CH };
            new Sub { Size = 8, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.DH };
            new Sub { Size = 8, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.AL };
            new Sub { Size = 8, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.BL };
            new Sub { Size = 8, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.CL };
            new Sub { Size = 8, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.DL };
            new Sub { Size = 8, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.AH };
            new Sub { Size = 8, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.BH };
            new Sub { Size = 8, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.CH };
            new Sub { Size = 8, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.DH };
            new Sub { Size = 8, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.AL };
            new Sub { Size = 8, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.BL };
            new Sub { Size = 8, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.CL };
            new Sub { Size = 8, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.DL };
            new Sub { Size = 8, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.AH };
            new Sub { Size = 8, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.BH };
            new Sub { Size = 8, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.CH };
            new Sub { Size = 8, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.DH };
            new Sub { Size = 8, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.AL };
            new Sub { Size = 8, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.BL };
            new Sub { Size = 8, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.CL };
            new Sub { Size = 8, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.DL };
            new Sub { Size = 8, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.AH };
            new Sub { Size = 8, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.BH };
            new Sub { Size = 8, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.CH };
            new Sub { Size = 8, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.DH };
            new Sub { Size = 8, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.AL };
            new Sub { Size = 8, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.BL };
            new Sub { Size = 8, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.CL };
            new Sub { Size = 8, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.DL };
            new Sub { Size = 8, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.AH };
            new Sub { Size = 8, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.BH };
            new Sub { Size = 8, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.CH };
            new Sub { Size = 8, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.DH };
            new Sub { Size = 8, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.AL };
            new Sub { Size = 8, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.BL };
            new Sub { Size = 8, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.CL };
            new Sub { Size = 8, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.DL };
            new Sub { Size = 8, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.AH };
            new Sub { Size = 8, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.BH };
            new Sub { Size = 8, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.CH };
            new Sub { Size = 8, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.DH };
            new Sub { Size = 8, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.AL };
            new Sub { Size = 8, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.BL };
            new Sub { Size = 8, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.CL };
            new Sub { Size = 8, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.DL };
            new Sub { Size = 8, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.AH };
            new Sub { Size = 8, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.BH };
            new Sub { Size = 8, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.CH };
            new Sub { Size = 8, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x650, SourceReg = Registers.DH };
            Verify();
        }

        [Test]
        public void TestRegisterToMemory8BitOffset32() {
            new Sub { Size = 32, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.EAX };
            new Sub { Size = 32, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.EBX };
            new Sub { Size = 32, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.ECX };
            new Sub { Size = 32, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.EDX };
            new Sub { Size = 32, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.EDI };
            new Sub { Size = 32, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.ESI };
            new Sub { Size = 32, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.EBP };
            new Sub { Size = 32, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.ESP };
            new Sub { Size = 32, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.EAX };
            new Sub { Size = 32, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.EBX };
            new Sub { Size = 32, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.ECX };
            new Sub { Size = 32, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.EDX };
            new Sub { Size = 32, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.EDI };
            new Sub { Size = 32, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.ESI };
            new Sub { Size = 32, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.EBP };
            new Sub { Size = 32, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.ESP };
            new Sub { Size = 32, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.EAX };
            new Sub { Size = 32, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.EBX };
            new Sub { Size = 32, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.ECX };
            new Sub { Size = 32, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.EDX };
            new Sub { Size = 32, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.EDI };
            new Sub { Size = 32, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.ESI };
            new Sub { Size = 32, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.EBP };
            new Sub { Size = 32, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.ESP };
            new Sub { Size = 32, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.EAX };
            new Sub { Size = 32, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.EBX };
            new Sub { Size = 32, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.ECX };
            new Sub { Size = 32, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.EDX };
            new Sub { Size = 32, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.EDI };
            new Sub { Size = 32, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.ESI };
            new Sub { Size = 32, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.EBP };
            new Sub { Size = 32, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.ESP };
            new Sub { Size = 32, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.EAX };
            new Sub { Size = 32, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.EBX };
            new Sub { Size = 32, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.ECX };
            new Sub { Size = 32, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.EDX };
            new Sub { Size = 32, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.EDI };
            new Sub { Size = 32, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.ESI };
            new Sub { Size = 32, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.EBP };
            new Sub { Size = 32, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.ESP };
            new Sub { Size = 32, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.EAX };
            new Sub { Size = 32, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.EBX };
            new Sub { Size = 32, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.ECX };
            new Sub { Size = 32, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.EDX };
            new Sub { Size = 32, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.EDI };
            new Sub { Size = 32, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.ESI };
            new Sub { Size = 32, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.EBP };
            new Sub { Size = 32, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.ESP };
            new Sub { Size = 32, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.EAX };
            new Sub { Size = 32, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.EBX };
            new Sub { Size = 32, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.ECX };
            new Sub { Size = 32, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.EDX };
            new Sub { Size = 32, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.EDI };
            new Sub { Size = 32, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.ESI };
            new Sub { Size = 32, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.EBP };
            new Sub { Size = 32, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.ESP };
            new Sub { Size = 32, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.EAX };
            new Sub { Size = 32, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.EBX };
            new Sub { Size = 32, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.ECX };
            new Sub { Size = 32, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.EDX };
            new Sub { Size = 32, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.EDI };
            new Sub { Size = 32, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.ESI };
            new Sub { Size = 32, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.EBP };
            new Sub { Size = 32, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.ESP };
            Verify();
        }

        [Test]
        public void TestRegisterToMemory8BitOffset16() {
            new Sub { Size = 16, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.AX };
            new Sub { Size = 16, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.BX };
            new Sub { Size = 16, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.CX };
            new Sub { Size = 16, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.DX };
            new Sub { Size = 16, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.DI };
            new Sub { Size = 16, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.SI };
            new Sub { Size = 16, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.BP };
            new Sub { Size = 16, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.SP };
            new Sub { Size = 16, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.AX };
            new Sub { Size = 16, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.BX };
            new Sub { Size = 16, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.CX };
            new Sub { Size = 16, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.DX };
            new Sub { Size = 16, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.DI };
            new Sub { Size = 16, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.SI };
            new Sub { Size = 16, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.BP };
            new Sub { Size = 16, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.SP };
            new Sub { Size = 16, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.AX };
            new Sub { Size = 16, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.BX };
            new Sub { Size = 16, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.CX };
            new Sub { Size = 16, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.DX };
            new Sub { Size = 16, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.DI };
            new Sub { Size = 16, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.SI };
            new Sub { Size = 16, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.BP };
            new Sub { Size = 16, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.SP };
            new Sub { Size = 16, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.AX };
            new Sub { Size = 16, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.BX };
            new Sub { Size = 16, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.CX };
            new Sub { Size = 16, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.DX };
            new Sub { Size = 16, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.DI };
            new Sub { Size = 16, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.SI };
            new Sub { Size = 16, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.BP };
            new Sub { Size = 16, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.SP };
            new Sub { Size = 16, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.AX };
            new Sub { Size = 16, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.BX };
            new Sub { Size = 16, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.CX };
            new Sub { Size = 16, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.DX };
            new Sub { Size = 16, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.DI };
            new Sub { Size = 16, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.SI };
            new Sub { Size = 16, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.BP };
            new Sub { Size = 16, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.SP };
            new Sub { Size = 16, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.AX };
            new Sub { Size = 16, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.BX };
            new Sub { Size = 16, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.CX };
            new Sub { Size = 16, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.DX };
            new Sub { Size = 16, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.DI };
            new Sub { Size = 16, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.SI };
            new Sub { Size = 16, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.BP };
            new Sub { Size = 16, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.SP };
            new Sub { Size = 16, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.AX };
            new Sub { Size = 16, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.BX };
            new Sub { Size = 16, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.CX };
            new Sub { Size = 16, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.DX };
            new Sub { Size = 16, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.DI };
            new Sub { Size = 16, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.SI };
            new Sub { Size = 16, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.BP };
            new Sub { Size = 16, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.SP };
            new Sub { Size = 16, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.AX };
            new Sub { Size = 16, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.BX };
            new Sub { Size = 16, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.CX };
            new Sub { Size = 16, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.DX };
            new Sub { Size = 16, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.DI };
            new Sub { Size = 16, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.SI };
            new Sub { Size = 16, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.BP };
            new Sub { Size = 16, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.SP };
            Verify();
        }

        [Test]
        public void TestRegisterToMemory8BitOffset8() {
            new Sub { Size = 8, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.AL };
            new Sub { Size = 8, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.BL };
            new Sub { Size = 8, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.CL };
            new Sub { Size = 8, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.DL };
            new Sub { Size = 8, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.AH };
            new Sub { Size = 8, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.BH };
            new Sub { Size = 8, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.CH };
            new Sub { Size = 8, DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.DH };
            new Sub { Size = 8, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.AL };
            new Sub { Size = 8, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.BL };
            new Sub { Size = 8, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.CL };
            new Sub { Size = 8, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.DL };
            new Sub { Size = 8, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.AH };
            new Sub { Size = 8, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.BH };
            new Sub { Size = 8, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.CH };
            new Sub { Size = 8, DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.DH };
            new Sub { Size = 8, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.AL };
            new Sub { Size = 8, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.BL };
            new Sub { Size = 8, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.CL };
            new Sub { Size = 8, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.DL };
            new Sub { Size = 8, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.AH };
            new Sub { Size = 8, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.BH };
            new Sub { Size = 8, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.CH };
            new Sub { Size = 8, DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.DH };
            new Sub { Size = 8, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.AL };
            new Sub { Size = 8, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.BL };
            new Sub { Size = 8, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.CL };
            new Sub { Size = 8, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.DL };
            new Sub { Size = 8, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.AH };
            new Sub { Size = 8, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.BH };
            new Sub { Size = 8, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.CH };
            new Sub { Size = 8, DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.DH };
            new Sub { Size = 8, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.AL };
            new Sub { Size = 8, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.BL };
            new Sub { Size = 8, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.CL };
            new Sub { Size = 8, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.DL };
            new Sub { Size = 8, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.AH };
            new Sub { Size = 8, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.BH };
            new Sub { Size = 8, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.CH };
            new Sub { Size = 8, DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.DH };
            new Sub { Size = 8, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.AL };
            new Sub { Size = 8, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.BL };
            new Sub { Size = 8, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.CL };
            new Sub { Size = 8, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.DL };
            new Sub { Size = 8, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.AH };
            new Sub { Size = 8, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.BH };
            new Sub { Size = 8, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.CH };
            new Sub { Size = 8, DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.DH };
            new Sub { Size = 8, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.AL };
            new Sub { Size = 8, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.BL };
            new Sub { Size = 8, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.CL };
            new Sub { Size = 8, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.DL };
            new Sub { Size = 8, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.AH };
            new Sub { Size = 8, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.BH };
            new Sub { Size = 8, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.CH };
            new Sub { Size = 8, DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.DH };
            new Sub { Size = 8, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.AL };
            new Sub { Size = 8, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.BL };
            new Sub { Size = 8, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.CL };
            new Sub { Size = 8, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.DL };
            new Sub { Size = 8, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.AH };
            new Sub { Size = 8, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.BH };
            new Sub { Size = 8, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.CH };
            new Sub { Size = 8, DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x65, SourceReg = Registers.DH };
            Verify();
        }

        [Test]
        public void TestImmediateToRegister32() {
            new Sub { DestinationReg = Registers.EAX, SourceValue = 1 };
            new Sub { DestinationReg = Registers.EBX, SourceValue = 2 };
            new Sub { DestinationReg = Registers.ECX, SourceValue = 3 };
            new Sub { DestinationReg = Registers.EDX, SourceValue = 4 };
            new Sub { DestinationReg = Registers.ESI, SourceValue = 5 };
            new Sub { DestinationReg = Registers.EDI, SourceValue = 6 };
            new Sub { DestinationReg = Registers.ESP, SourceValue = 6 };
            new Sub { DestinationReg = Registers.EBP, SourceValue = 6 };
            Verify();
        }

        [Test]
        public void TestImmediateToRegister16() {
            new Sub { DestinationReg = Registers.AX, SourceValue = 1 };
            new Sub { DestinationReg = Registers.BX, SourceValue = 2 };
            new Sub { DestinationReg = Registers.CX, SourceValue = 3 };
            new Sub { DestinationReg = Registers.DX, SourceValue = 4 };
            new Sub { DestinationReg = Registers.SI, SourceValue = 5 };
            new Sub { DestinationReg = Registers.DI, SourceValue = 6 };
            new Sub { DestinationReg = Registers.BP, SourceValue = 5 };
            new Sub { DestinationReg = Registers.SP, SourceValue = 6 };
            Verify();
        }

        [Test]
        public void TestImmediateToRegister8() {
            new Sub { DestinationReg = Registers.AL, SourceValue = 1 };
            new Sub { DestinationReg = Registers.BL, SourceValue = 2 };
            new Sub { DestinationReg = Registers.CL, SourceValue = 3 };
            new Sub { DestinationReg = Registers.DL, SourceValue = 4 };
            new Sub { DestinationReg = Registers.AH, SourceValue = 1 };
            new Sub { DestinationReg = Registers.BH, SourceValue = 2 };
            new Sub { DestinationReg = Registers.CH, SourceValue = 3 };
            new Sub { DestinationReg = Registers.DH, SourceValue = 4 };
            Verify();
        }

        [Test]
        public void TestImmediateToMemorySimple8() {
            new Sub { Size = 8, DestinationReg = Registers.EAX, DestinationIsIndirect = true, SourceValue = 65 };
            new Sub { Size = 8, DestinationReg = Registers.EBX, DestinationIsIndirect = true, SourceValue = 66 };
            new Sub { Size = 8, DestinationReg = Registers.ECX, DestinationIsIndirect = true, SourceValue = 67 };
            new Sub { Size = 8, DestinationReg = Registers.EDX, DestinationIsIndirect = true, SourceValue = 68 };
            new Sub { Size = 8, DestinationReg = Registers.EDI, DestinationIsIndirect = true, SourceValue = 69 };
            new Sub { Size = 8, DestinationReg = Registers.ESI, DestinationIsIndirect = true, SourceValue = 70 };
            new Sub { Size = 8, DestinationReg = Registers.ESP, DestinationIsIndirect = true, SourceValue = 71 };
            new Sub { Size = 8, DestinationReg = Registers.EBP, DestinationIsIndirect = true, SourceValue = 72 };
            Verify();
        }

        [Test]
        public void TestImmediateToMemorySimple16() {
            new Sub { Size = 16, DestinationReg = Registers.EAX, DestinationIsIndirect = true, SourceValue = 65 };
            new Sub { Size = 16, DestinationReg = Registers.EBX, DestinationIsIndirect = true, SourceValue = 66 };
            new Sub { Size = 16, DestinationReg = Registers.ECX, DestinationIsIndirect = true, SourceValue = 67 };
            new Sub { Size = 16, DestinationReg = Registers.EDX, DestinationIsIndirect = true, SourceValue = 68 };
            new Sub { Size = 16, DestinationReg = Registers.EDI, DestinationIsIndirect = true, SourceValue = 69 };
            new Sub { Size = 16, DestinationReg = Registers.ESI, DestinationIsIndirect = true, SourceValue = 70 };
            new Sub { Size = 16, DestinationReg = Registers.ESP, DestinationIsIndirect = true, SourceValue = 71 };
            new Sub { Size = 16, DestinationReg = Registers.EBP, DestinationIsIndirect = true, SourceValue = 72 };
            Verify();
        }

        [Test]
        public void TestImmediateToMemorySimple32() {
            new Sub { Size = 32, DestinationReg = Registers.EAX, DestinationIsIndirect = true, SourceValue = 65 };
            new Sub { Size = 32, DestinationReg = Registers.EBX, DestinationIsIndirect = true, SourceValue = 66 };
            new Sub { Size = 32, DestinationReg = Registers.ECX, DestinationIsIndirect = true, SourceValue = 67 };
            new Sub { Size = 32, DestinationReg = Registers.EDX, DestinationIsIndirect = true, SourceValue = 68 };
            new Sub { Size = 32, DestinationReg = Registers.EDI, DestinationIsIndirect = true, SourceValue = 69 };
            new Sub { Size = 32, DestinationReg = Registers.ESI, DestinationIsIndirect = true, SourceValue = 70 };
            new Sub { Size = 32, DestinationReg = Registers.ESP, DestinationIsIndirect = true, SourceValue = 71 };
            new Sub { Size = 32, DestinationReg = Registers.EBP, DestinationIsIndirect = true, SourceValue = 72 };
            Verify();
        }

        [Test]
        public void TestRegisterToRegister32() {
            new Sub { DestinationReg = Registers.EAX, SourceReg = Registers.EAX };
            new Sub { DestinationReg = Registers.EAX, SourceReg = Registers.EBX };
            new Sub { DestinationReg = Registers.EAX, SourceReg = Registers.ECX };
            new Sub { DestinationReg = Registers.EAX, SourceReg = Registers.EDX };
            new Sub { DestinationReg = Registers.EAX, SourceReg = Registers.EDI };
            new Sub { DestinationReg = Registers.EAX, SourceReg = Registers.ESI };
            new Sub { DestinationReg = Registers.EAX, SourceReg = Registers.EBP };
            new Sub { DestinationReg = Registers.EAX, SourceReg = Registers.ESP };
            new Sub { DestinationReg = Registers.EBX, SourceReg = Registers.EAX };
            new Sub { DestinationReg = Registers.EBX, SourceReg = Registers.EBX };
            new Sub { DestinationReg = Registers.EBX, SourceReg = Registers.ECX };
            new Sub { DestinationReg = Registers.EBX, SourceReg = Registers.EDX };
            new Sub { DestinationReg = Registers.EBX, SourceReg = Registers.EDI };
            new Sub { DestinationReg = Registers.EBX, SourceReg = Registers.ESI };
            new Sub { DestinationReg = Registers.EBX, SourceReg = Registers.EBP };
            new Sub { DestinationReg = Registers.EBX, SourceReg = Registers.ESP };
            new Sub { DestinationReg = Registers.ECX, SourceReg = Registers.EAX };
            new Sub { DestinationReg = Registers.ECX, SourceReg = Registers.EBX };
            new Sub { DestinationReg = Registers.ECX, SourceReg = Registers.ECX };
            new Sub { DestinationReg = Registers.ECX, SourceReg = Registers.EDX };
            new Sub { DestinationReg = Registers.ECX, SourceReg = Registers.EDI };
            new Sub { DestinationReg = Registers.ECX, SourceReg = Registers.ESI };
            new Sub { DestinationReg = Registers.ECX, SourceReg = Registers.EBP };
            new Sub { DestinationReg = Registers.ECX, SourceReg = Registers.ESP };
            new Sub { DestinationReg = Registers.EDX, SourceReg = Registers.EAX };
            new Sub { DestinationReg = Registers.EDX, SourceReg = Registers.EBX };
            new Sub { DestinationReg = Registers.EDX, SourceReg = Registers.ECX };
            new Sub { DestinationReg = Registers.EDX, SourceReg = Registers.EDX };
            new Sub { DestinationReg = Registers.EDX, SourceReg = Registers.EDI };
            new Sub { DestinationReg = Registers.EDX, SourceReg = Registers.ESI };
            new Sub { DestinationReg = Registers.EDX, SourceReg = Registers.EBP };
            new Sub { DestinationReg = Registers.EDX, SourceReg = Registers.ESP };
            new Sub { DestinationReg = Registers.EDI, SourceReg = Registers.EAX };
            new Sub { DestinationReg = Registers.EDI, SourceReg = Registers.EBX };
            new Sub { DestinationReg = Registers.EDI, SourceReg = Registers.ECX };
            new Sub { DestinationReg = Registers.EDI, SourceReg = Registers.EDX };
            new Sub { DestinationReg = Registers.EDI, SourceReg = Registers.EDI };
            new Sub { DestinationReg = Registers.EDI, SourceReg = Registers.ESI };
            new Sub { DestinationReg = Registers.EDI, SourceReg = Registers.EBP };
            new Sub { DestinationReg = Registers.EDI, SourceReg = Registers.ESP };
            new Sub { DestinationReg = Registers.ESI, SourceReg = Registers.EAX };
            new Sub { DestinationReg = Registers.ESI, SourceReg = Registers.EBX };
            new Sub { DestinationReg = Registers.ESI, SourceReg = Registers.ECX };
            new Sub { DestinationReg = Registers.ESI, SourceReg = Registers.EDX };
            new Sub { DestinationReg = Registers.ESI, SourceReg = Registers.EDI };
            new Sub { DestinationReg = Registers.ESI, SourceReg = Registers.ESI };
            new Sub { DestinationReg = Registers.ESI, SourceReg = Registers.EBP };
            new Sub { DestinationReg = Registers.ESI, SourceReg = Registers.ESP };
            new Sub { DestinationReg = Registers.EBP, SourceReg = Registers.EAX };
            new Sub { DestinationReg = Registers.EBP, SourceReg = Registers.EBX };
            new Sub { DestinationReg = Registers.EBP, SourceReg = Registers.ECX };
            new Sub { DestinationReg = Registers.EBP, SourceReg = Registers.EDX };
            new Sub { DestinationReg = Registers.EBP, SourceReg = Registers.EDI };
            new Sub { DestinationReg = Registers.EBP, SourceReg = Registers.ESI };
            new Sub { DestinationReg = Registers.EBP, SourceReg = Registers.EBP };
            new Sub { DestinationReg = Registers.EBP, SourceReg = Registers.ESP };
            new Sub { DestinationReg = Registers.ESP, SourceReg = Registers.EAX };
            new Sub { DestinationReg = Registers.ESP, SourceReg = Registers.EBX };
            new Sub { DestinationReg = Registers.ESP, SourceReg = Registers.ECX };
            new Sub { DestinationReg = Registers.ESP, SourceReg = Registers.EDX };
            new Sub { DestinationReg = Registers.ESP, SourceReg = Registers.EDI };
            new Sub { DestinationReg = Registers.ESP, SourceReg = Registers.ESI };
            new Sub { DestinationReg = Registers.ESP, SourceReg = Registers.EBP };
            new Sub { DestinationReg = Registers.ESP, SourceReg = Registers.ESP };
            Verify();
        }

        [Test]
        public void TestRegisterToRegister16() {
            new Sub { DestinationReg = Registers.AX, SourceReg = Registers.AX };
            new Sub { DestinationReg = Registers.AX, SourceReg = Registers.BX };
            new Sub { DestinationReg = Registers.AX, SourceReg = Registers.CX };
            new Sub { DestinationReg = Registers.AX, SourceReg = Registers.DX };
            new Sub { DestinationReg = Registers.AX, SourceReg = Registers.DI };
            new Sub { DestinationReg = Registers.AX, SourceReg = Registers.SI };
            new Sub { DestinationReg = Registers.AX, SourceReg = Registers.BP };
            new Sub { DestinationReg = Registers.AX, SourceReg = Registers.SP };
            new Sub { DestinationReg = Registers.BX, SourceReg = Registers.AX };
            new Sub { DestinationReg = Registers.BX, SourceReg = Registers.BX };
            new Sub { DestinationReg = Registers.BX, SourceReg = Registers.CX };
            new Sub { DestinationReg = Registers.BX, SourceReg = Registers.DX };
            new Sub { DestinationReg = Registers.BX, SourceReg = Registers.DI };
            new Sub { DestinationReg = Registers.BX, SourceReg = Registers.SI };
            new Sub { DestinationReg = Registers.BX, SourceReg = Registers.BP };
            new Sub { DestinationReg = Registers.BX, SourceReg = Registers.SP };
            new Sub { DestinationReg = Registers.CX, SourceReg = Registers.AX };
            new Sub { DestinationReg = Registers.CX, SourceReg = Registers.BX };
            new Sub { DestinationReg = Registers.CX, SourceReg = Registers.CX };
            new Sub { DestinationReg = Registers.CX, SourceReg = Registers.DX };
            new Sub { DestinationReg = Registers.CX, SourceReg = Registers.DI };
            new Sub { DestinationReg = Registers.CX, SourceReg = Registers.SI };
            new Sub { DestinationReg = Registers.CX, SourceReg = Registers.BP };
            new Sub { DestinationReg = Registers.CX, SourceReg = Registers.SP };
            new Sub { DestinationReg = Registers.DX, SourceReg = Registers.AX };
            new Sub { DestinationReg = Registers.DX, SourceReg = Registers.BX };
            new Sub { DestinationReg = Registers.DX, SourceReg = Registers.CX };
            new Sub { DestinationReg = Registers.DX, SourceReg = Registers.DX };
            new Sub { DestinationReg = Registers.DX, SourceReg = Registers.DI };
            new Sub { DestinationReg = Registers.DX, SourceReg = Registers.SI };
            new Sub { DestinationReg = Registers.DX, SourceReg = Registers.BP };
            new Sub { DestinationReg = Registers.DX, SourceReg = Registers.SP };
            new Sub { DestinationReg = Registers.DI, SourceReg = Registers.AX };
            new Sub { DestinationReg = Registers.DI, SourceReg = Registers.BX };
            new Sub { DestinationReg = Registers.DI, SourceReg = Registers.CX };
            new Sub { DestinationReg = Registers.DI, SourceReg = Registers.DX };
            new Sub { DestinationReg = Registers.DI, SourceReg = Registers.DI };
            new Sub { DestinationReg = Registers.DI, SourceReg = Registers.SI };
            new Sub { DestinationReg = Registers.DI, SourceReg = Registers.BP };
            new Sub { DestinationReg = Registers.DI, SourceReg = Registers.SP };
            new Sub { DestinationReg = Registers.SI, SourceReg = Registers.AX };
            new Sub { DestinationReg = Registers.SI, SourceReg = Registers.BX };
            new Sub { DestinationReg = Registers.SI, SourceReg = Registers.CX };
            new Sub { DestinationReg = Registers.SI, SourceReg = Registers.DX };
            new Sub { DestinationReg = Registers.SI, SourceReg = Registers.DI };
            new Sub { DestinationReg = Registers.SI, SourceReg = Registers.SI };
            new Sub { DestinationReg = Registers.SI, SourceReg = Registers.BP };
            new Sub { DestinationReg = Registers.SI, SourceReg = Registers.SP };
            new Sub { DestinationReg = Registers.BP, SourceReg = Registers.AX };
            new Sub { DestinationReg = Registers.BP, SourceReg = Registers.BX };
            new Sub { DestinationReg = Registers.BP, SourceReg = Registers.CX };
            new Sub { DestinationReg = Registers.BP, SourceReg = Registers.DX };
            new Sub { DestinationReg = Registers.BP, SourceReg = Registers.DI };
            new Sub { DestinationReg = Registers.BP, SourceReg = Registers.SI };
            new Sub { DestinationReg = Registers.BP, SourceReg = Registers.BP };
            new Sub { DestinationReg = Registers.BP, SourceReg = Registers.SP };
            new Sub { DestinationReg = Registers.SP, SourceReg = Registers.AX };
            new Sub { DestinationReg = Registers.SP, SourceReg = Registers.BX };
            new Sub { DestinationReg = Registers.SP, SourceReg = Registers.CX };
            new Sub { DestinationReg = Registers.SP, SourceReg = Registers.DX };
            new Sub { DestinationReg = Registers.SP, SourceReg = Registers.DI };
            new Sub { DestinationReg = Registers.SP, SourceReg = Registers.SI };
            new Sub { DestinationReg = Registers.SP, SourceReg = Registers.BP };
            new Sub { DestinationReg = Registers.SP, SourceReg = Registers.SP };
            Verify();
        }

        [Test]
        public void TestRegisterToRegister8() {
            new Sub { DestinationReg = Registers.AL, SourceReg = Registers.AL };
            new Sub { DestinationReg = Registers.AL, SourceReg = Registers.AH };
            new Sub { DestinationReg = Registers.AL, SourceReg = Registers.BL };
            new Sub { DestinationReg = Registers.AL, SourceReg = Registers.BH };
            new Sub { DestinationReg = Registers.AL, SourceReg = Registers.CL };
            new Sub { DestinationReg = Registers.AL, SourceReg = Registers.CH };
            new Sub { DestinationReg = Registers.AL, SourceReg = Registers.DL };
            new Sub { DestinationReg = Registers.AL, SourceReg = Registers.DH };
            new Sub { DestinationReg = Registers.AH, SourceReg = Registers.AL };
            new Sub { DestinationReg = Registers.AH, SourceReg = Registers.AH };
            new Sub { DestinationReg = Registers.AH, SourceReg = Registers.BL };
            new Sub { DestinationReg = Registers.AH, SourceReg = Registers.BH };
            new Sub { DestinationReg = Registers.AH, SourceReg = Registers.CL };
            new Sub { DestinationReg = Registers.AH, SourceReg = Registers.CH };
            new Sub { DestinationReg = Registers.AH, SourceReg = Registers.DL };
            new Sub { DestinationReg = Registers.AH, SourceReg = Registers.DH };
            new Sub { DestinationReg = Registers.BL, SourceReg = Registers.AL };
            new Sub { DestinationReg = Registers.BL, SourceReg = Registers.AH };
            new Sub { DestinationReg = Registers.BL, SourceReg = Registers.BL };
            new Sub { DestinationReg = Registers.BL, SourceReg = Registers.BH };
            new Sub { DestinationReg = Registers.BL, SourceReg = Registers.CL };
            new Sub { DestinationReg = Registers.BL, SourceReg = Registers.CH };
            new Sub { DestinationReg = Registers.BL, SourceReg = Registers.DL };
            new Sub { DestinationReg = Registers.BL, SourceReg = Registers.DH };
            new Sub { DestinationReg = Registers.BH, SourceReg = Registers.AL };
            new Sub { DestinationReg = Registers.BH, SourceReg = Registers.AH };
            new Sub { DestinationReg = Registers.BH, SourceReg = Registers.BL };
            new Sub { DestinationReg = Registers.BH, SourceReg = Registers.BH };
            new Sub { DestinationReg = Registers.BH, SourceReg = Registers.CL };
            new Sub { DestinationReg = Registers.BH, SourceReg = Registers.CH };
            new Sub { DestinationReg = Registers.BH, SourceReg = Registers.DL };
            new Sub { DestinationReg = Registers.BH, SourceReg = Registers.DH };
            new Sub { DestinationReg = Registers.CL, SourceReg = Registers.AL };
            new Sub { DestinationReg = Registers.CL, SourceReg = Registers.AH };
            new Sub { DestinationReg = Registers.CL, SourceReg = Registers.BL };
            new Sub { DestinationReg = Registers.CL, SourceReg = Registers.BH };
            new Sub { DestinationReg = Registers.CL, SourceReg = Registers.CL };
            new Sub { DestinationReg = Registers.CL, SourceReg = Registers.CH };
            new Sub { DestinationReg = Registers.CL, SourceReg = Registers.DL };
            new Sub { DestinationReg = Registers.CL, SourceReg = Registers.DH };
            new Sub { DestinationReg = Registers.CH, SourceReg = Registers.AL };
            new Sub { DestinationReg = Registers.CH, SourceReg = Registers.AH };
            new Sub { DestinationReg = Registers.CH, SourceReg = Registers.BL };
            new Sub { DestinationReg = Registers.CH, SourceReg = Registers.BH };
            new Sub { DestinationReg = Registers.CH, SourceReg = Registers.CL };
            new Sub { DestinationReg = Registers.CH, SourceReg = Registers.CH };
            new Sub { DestinationReg = Registers.CH, SourceReg = Registers.DL };
            new Sub { DestinationReg = Registers.CH, SourceReg = Registers.DH };
            new Sub { DestinationReg = Registers.DL, SourceReg = Registers.AL };
            new Sub { DestinationReg = Registers.DL, SourceReg = Registers.AH };
            new Sub { DestinationReg = Registers.DL, SourceReg = Registers.BL };
            new Sub { DestinationReg = Registers.DL, SourceReg = Registers.BH };
            new Sub { DestinationReg = Registers.DL, SourceReg = Registers.CL };
            new Sub { DestinationReg = Registers.DL, SourceReg = Registers.CH };
            new Sub { DestinationReg = Registers.DL, SourceReg = Registers.DL };
            new Sub { DestinationReg = Registers.DL, SourceReg = Registers.DH };
            new Sub { DestinationReg = Registers.DH, SourceReg = Registers.AL };
            new Sub { DestinationReg = Registers.DH, SourceReg = Registers.AH };
            new Sub { DestinationReg = Registers.DH, SourceReg = Registers.BL };
            new Sub { DestinationReg = Registers.DH, SourceReg = Registers.BH };
            new Sub { DestinationReg = Registers.DH, SourceReg = Registers.CL };
            new Sub { DestinationReg = Registers.DH, SourceReg = Registers.CH };
            new Sub { DestinationReg = Registers.DH, SourceReg = Registers.DL };
            new Sub { DestinationReg = Registers.DH, SourceReg = Registers.DH };
            Verify();
        }

        [Test]
        public void TestImmediateToMemory8BitOffset32BitData() {
            new Sub { DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 203, SourceValue = 65, Size = 32 };
            new Sub { DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 203, SourceValue = 65, Size = 32 };
            new Sub { DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 203, SourceValue = 65, Size = 32 };
            new Sub { DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 203, SourceValue = 65, Size = 32 };
            new Sub { DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 203, SourceValue = 65, Size = 32 };
            new Sub { DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 203, SourceValue = 65, Size = 32 };
            new Sub { DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 203, SourceValue = 65, Size = 32 };
            new Sub { DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 203, SourceValue = 65, Size = 32 };
            Verify();
        }

        [Test]
        public void TestImmediateToMemory8BitOffset16BitData() {
            new Sub { DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 203, SourceValue = 65, Size = 16 };
            new Sub { DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 203, SourceValue = 65, Size = 16 };
            new Sub { DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 203, SourceValue = 65, Size = 16 };
            new Sub { DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 203, SourceValue = 65, Size = 16 };
            new Sub { DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 203, SourceValue = 65, Size = 16 };
            new Sub { DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 203, SourceValue = 65, Size = 16 };
            new Sub { DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 203, SourceValue = 65, Size = 16 };
            new Sub { DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 203, SourceValue = 65, Size = 16 };
            Verify();
        }

        [Test]
        public void TestImmediateToMemory8BitOffset8BitData() {
            new Sub { DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 203, SourceValue = 65, Size = 8 };
            new Sub { DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 203, SourceValue = 65, Size = 8 };
            new Sub { DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 203, SourceValue = 65, Size = 8 };
            new Sub { DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 203, SourceValue = 65, Size = 8 };
            new Sub { DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 203, SourceValue = 65, Size = 8 };
            new Sub { DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 203, SourceValue = 65, Size = 8 };
            new Sub { DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 203, SourceValue = 65, Size = 8 };
            new Sub { DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 203, SourceValue = 65, Size = 8 };
            Verify();
        }

        [Test]
        public void TestImmediateToMemory16BitOffset32BitData() {
            new Sub { DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 1203, SourceValue = 65, Size = 32 };
            new Sub { DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 1203, SourceValue = 65, Size = 32 };
            new Sub { DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 1203, SourceValue = 65, Size = 32 };
            new Sub { DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 1203, SourceValue = 65, Size = 32 };
            new Sub { DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 1203, SourceValue = 65, Size = 32 };
            new Sub { DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 1203, SourceValue = 65, Size = 32 };
            new Sub { DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 1203, SourceValue = 65, Size = 32 };
            new Sub { DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 1203, SourceValue = 65, Size = 32 };
            Verify();
        }

        [Test]
        public void TestImmediateToMemory16BitOffset16BitData() {
            new Sub { DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 1203, SourceValue = 65, Size = 16 };
            new Sub { DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 1203, SourceValue = 65, Size = 16 };
            new Sub { DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 1203, SourceValue = 65, Size = 16 };
            new Sub { DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 1203, SourceValue = 65, Size = 16 };
            new Sub { DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 1203, SourceValue = 65, Size = 16 };
            new Sub { DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 1203, SourceValue = 65, Size = 16 };
            new Sub { DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 1203, SourceValue = 65, Size = 16 };
            new Sub { DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 1203, SourceValue = 65, Size = 16 };
            Verify();
        }

        [Test]
        public void TestImmediateToMemory16BitOffset8BitData() {
            new Sub { DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 1203, SourceValue = 65, Size = 8 };
            new Sub { DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 1203, SourceValue = 65, Size = 8 };
            new Sub { DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 1203, SourceValue = 65, Size = 8 };
            new Sub { DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 1203, SourceValue = 65, Size = 8 };
            new Sub { DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 1203, SourceValue = 65, Size = 8 };
            new Sub { DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 1203, SourceValue = 65, Size = 8 };
            new Sub { DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 1203, SourceValue = 65, Size = 8 };
            new Sub { DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 1203, SourceValue = 65, Size = 8 };
            Verify();
        }

        [Test]
        public void TestImmediateToMemory32BitOffset32BitData() {
            new Sub { DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x56781203, SourceValue = 65, Size = 32 };
            new Sub { DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x56781203, SourceValue = 65, Size = 32 };
            new Sub { DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x56781203, SourceValue = 65, Size = 32 };
            new Sub { DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x56781203, SourceValue = 65, Size = 32 };
            new Sub { DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x56781203, SourceValue = 65, Size = 32 };
            new Sub { DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x56781203, SourceValue = 65, Size = 32 };
            new Sub { DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x56781203, SourceValue = 65, Size = 32 };
            new Sub { DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x56781203, SourceValue = 65, Size = 32 };
            Verify();
        }

        [Test]
        public void TestImmediateToMemory32BitOffset16BitData() {
            new Sub { DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x56781203, SourceValue = 65, Size = 16 };
            new Sub { DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x56781203, SourceValue = 65, Size = 16 };
            new Sub { DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x56781203, SourceValue = 65, Size = 16 };
            new Sub { DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x56781203, SourceValue = 65, Size = 16 };
            new Sub { DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x56781203, SourceValue = 65, Size = 16 };
            new Sub { DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x56781203, SourceValue = 65, Size = 16 };
            new Sub { DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x56781203, SourceValue = 65, Size = 16 };
            new Sub { DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x56781203, SourceValue = 65, Size = 16 };
            Verify();
        }

        [Test]
        public void TestImmediateToMemory32BitOffset8BitData() {
            new Sub { DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 0x56781203, SourceValue = 65, Size = 8 };
            new Sub { DestinationReg = Registers.EBX, DestinationIsIndirect = true, DestinationDisplacement = 0x56781203, SourceValue = 65, Size = 8 };
            new Sub { DestinationReg = Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = 0x56781203, SourceValue = 65, Size = 8 };
            new Sub { DestinationReg = Registers.EDX, DestinationIsIndirect = true, DestinationDisplacement = 0x56781203, SourceValue = 65, Size = 8 };
            new Sub { DestinationReg = Registers.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0x56781203, SourceValue = 65, Size = 8 };
            new Sub { DestinationReg = Registers.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0x56781203, SourceValue = 65, Size = 8 };
            new Sub { DestinationReg = Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = 0x56781203, SourceValue = 65, Size = 8 };
            new Sub { DestinationReg = Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 0x56781203, SourceValue = 65, Size = 8 };
            Verify();
        }
    }
}