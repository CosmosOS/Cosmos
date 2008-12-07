using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.Tests.AssemblerTests.X86 {
    partial class InvalidOpcodeTester {
        private static void AddExceptions() {
            opcodesException.Add(typeof(ConditionalJump), new ConstraintsContainer {
                DestInfo = new Constraints { TestImmediate32 = true, TestImmediate16 = false, TestImmediate8 = false, TestMem8 = false, TestMem16 = false, TestMem32 = false, TestRegisters = false }
            });
            opcodesException.Add(typeof(Dec), new ConstraintsContainer {
                DestInfo = new Constraints { TestImmediate16 = false, TestImmediate32 = false, TestImmediate8 = false, TestCR = false, TestSegments = false }
            });
            opcodesException.Add(typeof(Divide), new ConstraintsContainer {
                DestInfo = new Constraints { TestImmediate16 = false, TestImmediate32 = false, TestImmediate8 = false, TestCR = false, TestSegments = false }
            });
            opcodesException.Add(typeof(FXSave), new ConstraintsContainer {
                DestInfo = new Constraints { TestImmediate16 = false, TestImmediate32 = false, TestImmediate8 = false, TestCR = false, TestSegments = false, TestRegisters=false, TestMem16 = false, TestMem8=false}
            });
            opcodesException.Add(typeof(FXStore), new ConstraintsContainer {
                DestInfo = new Constraints { TestImmediate16 = false, TestImmediate32 = false, TestImmediate8 = false, TestCR = false, TestSegments = false, TestRegisters = false, TestMem16 = false, TestMem8 = false }
            });
            opcodesException.Add(typeof(In), new ConstraintsContainer {
                DestInfo = new Constraints { TestImmediate16 = false, TestImmediate32 = false, InvalidRegisters = (from item in Registers.GetRegisters() where item != Registers.DX select item), TestCR = false, TestMem16 = false, TestMem32 = false, TestMem8 = false, TestSegments = false },
                SourceInfo = new Constraints { TestImmediate16 = false, TestImmediate32 = false, TestImmediate8 = false, InvalidRegisters = (from item in Registers.GetRegisters() where item != Registers.EAX || item == Registers.AX || item == Registers.AL select item), TestCR = false, TestMem16 = false, TestMem32 = false, TestMem8 = false, TestSegments = false }
            });
            opcodesException.Add(typeof(Inc), new ConstraintsContainer {
                DestInfo = new Constraints { TestImmediate16 = false, TestImmediate32 = false, TestImmediate8 = false, TestCR = false, TestSegments = false }
            });
            opcodesException.Add(typeof(Interrupt), new ConstraintsContainer {
                DestInfo = new Constraints { TestImmediate16 = false, TestImmediate32 = true, TestImmediate8 = false, TestCR = false, TestSegments = false, TestMem8 = false, TestMem16 = false, TestMem32 = false, TestRegisters = false }
            });
            opcodesException.Add(typeof(Lgdt), new ConstraintsContainer {
                DestInfo = new Constraints { TestImmediate16 = false, TestImmediate32 = false, TestImmediate8 = false, TestCR = false, TestSegments = false, TestMem8 = false, TestMem16 = false, TestMem32 = true, TestRegisters = false }
            });
            opcodesException.Add(typeof(Lidt), new ConstraintsContainer {
                DestInfo = new Constraints { TestImmediate16 = false, TestImmediate32 = false, TestImmediate8 = false, TestCR = false, TestSegments = false, TestMem8 = false, TestMem16 = false, TestMem32 = true, TestRegisters = false }
            });
            opcodesException.Add(typeof(Move), new ConstraintsContainer {
                DestInfo = new Constraints { TestImmediate16 = false, TestImmediate32 = false, TestImmediate8 = false, TestCR = false, TestSegments = false },
                SourceInfo = new Constraints { TestImmediate16 = false, TestImmediate32 = false, TestImmediate8 = false, TestCR = false, TestSegments = false },
                MemToMem=false
            });
            opcodesException.Add(typeof(Neg), new ConstraintsContainer {
                DestInfo = new Constraints { TestImmediate16 = false, TestImmediate32 = false, TestImmediate8 = false, TestCR = false, TestSegments = false }
            });
            opcodesException.Add(typeof(Not), new ConstraintsContainer {
                DestInfo = new Constraints { TestImmediate16 = false, TestImmediate32 = false, TestImmediate8 = false, TestCR = false, TestSegments=false }
            });
            opcodesException.Add(typeof(Out), new ConstraintsContainer {
                DestInfo = new Constraints{TestImmediate16=false, TestImmediate32=false, InvalidRegisters = (from item in Registers.GetRegisters() where item != Registers.DX select item), TestCR=false, TestMem16=false, TestMem32=false, TestMem8=false, TestSegments=false},
                SourceInfo = new Constraints { TestImmediate16 = false, TestImmediate32 = false, TestImmediate8 = false, InvalidRegisters = (from item in Registers.GetRegisters() where item != Registers.EAX || item == Registers.AX || item == Registers.AL select item), TestCR=false, TestMem16=false, TestMem32=false, TestMem8=false, TestSegments=false}
            });
            opcodesException.Add(typeof(Pop), new ConstraintsContainer {
                DestInfo = new Constraints { TestImmediate16 = false, TestImmediate32 = false, TestImmediate8 = false, TestCR = false, TestSegments = false, 
                    InvalidRegisters=Registers.Get8BitRegisters(), TestMem8=false },
                InvalidSizes = Instruction.InstructionSizes.Byte
            });
            opcodesException.Add(typeof(Push), new ConstraintsContainer {
                DestInfo = new Constraints { TestImmediate16 = false, TestImmediate32 = false, TestImmediate8 = false, TestCR = false, TestSegments = false, InvalidRegisters = Registers.Get8BitRegisters(), TestMem8 = false},
                InvalidSizes = Instruction.InstructionSizes.Byte
            });
            opcodesException.Add(typeof(Return), new ConstraintsContainer {
                DestInfo = new Constraints { TestImmediate16 = true, TestImmediate32 = false, TestImmediate8 = false, TestCR = false, TestSegments = false, TestMem8 = false, TestMem16=false, TestMem32=false, TestRegisters=false }
            });
            opcodesException.Add(typeof(ShiftLeft), new ConstraintsContainer {
                DestInfo = new Constraints { TestImmediate16 = false, TestImmediate32 = false, TestImmediate8 = false, TestCR = false, TestSegments = false},
                SourceInfo = new Constraints{TestCR=false, TestMem16=false, TestMem32=false, TestMem8=false, InvalidRegisters= from item in Registers.GetRegisters()where item != Registers.CL select item, TestImmediate16=false, TestImmediate32=false}
            });
            opcodesException.Add(typeof(ShiftRight), new ConstraintsContainer {
                DestInfo = new Constraints { TestImmediate16 = false, TestImmediate32 = false, TestImmediate8 = false, TestCR = false, TestSegments = false },
                SourceInfo = new Constraints{TestCR=false, TestMem16=false, TestMem32=false, TestMem8=false, InvalidRegisters= from item in Registers.GetRegisters()where item != Registers.CL select item, TestImmediate16=false, TestImmediate32=false}
            });
            opcodesException.Add(typeof(Xchg), new ConstraintsContainer {
                DestInfo=new Constraints{TestImmediate16=false, TestImmediate32=false, TestImmediate8=false},
                SourceInfo = new Constraints { TestMem16 = false, TestMem32 = false, TestMem8 = false, TestImmediate16 = false, TestImmediate32 = false, TestImmediate8 = false }
            });
        }
    }
}
