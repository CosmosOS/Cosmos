using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler {
    public abstract class Instruction {
        public abstract void EmitParams(BinaryWriter aWriter); 
    }
}
