using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IL2CPU.PostAssembler
{
    internal class BinaryLWInstruction :LWInstruction
    {

        public override LWInstructionType InstructionType
        {
            get { return LWInstructionType.BinaryInstruction; }
        }
    }
}
