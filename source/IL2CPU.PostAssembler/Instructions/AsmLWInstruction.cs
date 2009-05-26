using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IL2CPU.PostAssembler
{
    internal class AsmLWInstruction : LWInstruction, IOpCodeInstruction
    {
        private string[] mAsm;

        internal AsmLWInstruction(string asm)
        {
            if (string.IsNullOrEmpty(asm))
                throw new ArgumentNullException("empty instruction"); 

            mAsm = new StringCompactor().StringToCompact(asm); 

        }

        public override LWInstructionType InstructionType
        {
            get { return LWInstructionType.AsmInstruction; }
        }


        public override string ToString()
        {

            return new StringCompactor().CompactToString(mAsm);
        }


        public string OpCode
        {
            get
            {
                return mAsm[0];
            }
        }
    }
}
