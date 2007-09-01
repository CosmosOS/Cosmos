using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
    public class Assembler : Indy.IL2CPU.Assembler.Assembler {
        public Assembler(StreamWriter aOutputWriter) : base(aOutputWriter) {
        }
    }
}
