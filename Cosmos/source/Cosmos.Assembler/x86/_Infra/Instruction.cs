using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Cosmos.Assembler.x86 {
  // todo: cache the EncodingOption and InstructionData instances..
  public abstract class Instruction : Cosmos.Assembler.Instruction {
    
    [Flags]
    public enum InstructionSizes {
      None,
      Byte = 8,
      Word = 16,
      DWord = 32,
      QWord = 64,
      All = Byte | Word | DWord
    }

    public enum InstructionSize {
      None = 0,
      Byte = 8,
      Word = 16,
      DWord = 32,
      QWord = 64
    }

    [Flags]
    public enum OperandMemoryKinds {
      Default = Address | IndirectReg | IndirectRegOffset,
      Address = 1,
      IndirectReg = 2,
      IndirectRegOffset = 4
    }

    protected Instruction(string mnemonic = null) {  }
    protected Instruction(bool aAddToAssembler, string mnemonic = null):base(aAddToAssembler, mnemonic) {  }

    protected static string SizeToString(byte aSize) {
      switch (aSize) {
        case 8:
          return "byte";
        case 16:
          return "word";
        case 32:
          return "dword";
        case 64:
          return "qword";
        case 80:
          return string.Empty;
        default:
          return "dword";
      }
    }
  }
}
