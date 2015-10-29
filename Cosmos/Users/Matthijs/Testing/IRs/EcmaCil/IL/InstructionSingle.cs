using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EcmaCil.IL
{
    public class InstructionSingle: BaseInstruction
    {
        public InstructionSingle(InstructionKindEnum aKind, int aIndex, Single aValue)
            : base(aKind, aIndex)
        {
            Value = aValue;
        }

        public Single Value
        {
            get;
            private set;
        }
    }
}