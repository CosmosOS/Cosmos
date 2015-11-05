using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EcmaCil.IL
{
    public class InstructionNone: BaseInstruction
    {
        public InstructionNone(InstructionKindEnum aKind, int aIndex)
            : base(aKind, aIndex)
        {
        }
    }
}