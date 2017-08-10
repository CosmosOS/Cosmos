using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EcmaCil.IL
{
    public class InstructionInt32: BaseInstruction
    {
        public InstructionInt32(InstructionKindEnum aKind, int aIndex, int aValue)
            : base(aKind, aIndex)
        {
            Value = aValue;
        }

        public int Value
        {
            get;
            private set;
        }
    }
}