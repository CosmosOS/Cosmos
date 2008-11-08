using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
    public abstract class Instruction : Indy.IL2CPU.Assembler.Instruction {
        protected Instruction() { }
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
                default:
                    throw new Exception("Invalid size: " + aSize);
            }
        }
    }
}
