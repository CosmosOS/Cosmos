using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
    [OpCode("push")]
    public class Push : InstructionWithDestination{
        public Push()
        {
            //Changed without size
            //Size = 32;
        }
        public override string ToString()
        {
            return this.mMnemonic + " dword " + this.GetDestinationAsString();
        }


   }
}
