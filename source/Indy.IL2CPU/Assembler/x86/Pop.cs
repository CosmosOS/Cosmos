using System;
using System.Linq;

namespace Indy.IL2CPU.Assembler.X86 {
    [OpCode("pop")]
	public class Pop: InstructionWithDestination{
        public override string ToString()
        {
            return base.mMnemonic + " dword " + this.GetDestinationAsString();
        }
	}

}