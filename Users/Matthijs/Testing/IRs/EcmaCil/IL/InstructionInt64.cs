using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EcmaCil.IL
{
    public class InstructionInt64: BaseInstruction
    {
        public InstructionInt64(InstructionKindEnum aKind, int aIndex, long aValue)
            : base(aKind, aIndex)
        {
            Value = aValue;
        }

        public long Value
        {
            get;
            private set;
        }
    }
}