using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.Tests.AssemblerTests.X86 {
    partial class InvalidOpcodeTester {
        private static void AddExceptions() {
            opcodesException.Add(typeof(Dec), new ConstraintsContainer {
                DestInfo = new Constraints { TestImmediate16 = false, TestImmediate32 = false, TestImmediate8 = false, TestCR = false, TestSegments = false }
            });
            opcodesException.Add(typeof(Divide), new ConstraintsContainer {
                DestInfo = new Constraints { TestImmediate16 = false, TestImmediate32 = false, TestImmediate8 = false, TestCR = false, TestSegments = false }
            });
            opcodesException.Add(typeof(Inc), new ConstraintsContainer {
                DestInfo = new Constraints { TestImmediate16 = false, TestImmediate32 = false, TestImmediate8 = false, TestCR = false, TestSegments = false }
            });
            opcodesException.Add(typeof(Neg), new ConstraintsContainer {
                DestInfo = new Constraints { TestImmediate16 = false, TestImmediate32 = false, TestImmediate8 = false, TestCR = false, TestSegments = false }
            });
            opcodesException.Add(typeof(Not), new ConstraintsContainer {
                DestInfo = new Constraints { TestImmediate16 = false, TestImmediate32 = false, TestImmediate8 = false, TestCR = false, TestSegments=false }
            });
            opcodesException.Add(typeof(Pop), new ConstraintsContainer {
                DestInfo = new Constraints { TestImmediate16 = false, TestImmediate32 = false, TestImmediate8 = false, TestCR = false, TestSegments = false, 
                    InvalidRegisters=Registers.Get8BitRegisters(), TestMem8=false },
                InvalidSizes = Instruction.InstructionSizes.Byte
            });
            opcodesException.Add(typeof(ShiftLeft), new ConstraintsContainer {
                DestInfo = new Constraints { TestImmediate16 = false, TestImmediate32 = false, TestImmediate8 = false, TestCR = false, TestSegments = false},
                SourceInfo = new Constraints{TestCR=false, TestMem16=false, TestMem32=false, TestMem8=false, InvalidRegisters= from item in Registers.GetRegisters()where item != Registers.CL select item, TestImmediate16=false, TestImmediate32=false}
            });
            opcodesException.Add(typeof(ShiftRight), new ConstraintsContainer {
                DestInfo = new Constraints { TestImmediate16 = false, TestImmediate32 = false, TestImmediate8 = false, TestCR = false, TestSegments = false },
                SourceInfo = new Constraints{TestCR=false, TestMem16=false, TestMem32=false, TestMem8=false, InvalidRegisters= from item in Registers.GetRegisters()where item != Registers.CL select item, TestImmediate16=false, TestImmediate32=false}
            });
        }
    }
}
