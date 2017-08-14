using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EcmaCil.IL
{
    public class InstructionMethod : BaseInstruction
    {
        public InstructionMethod(InstructionKindEnum aKind, int aIndex, MethodMeta aValue)
            : base(aKind, aIndex)
        {
            Value = aValue;
        }

        public MethodMeta Value
        {
            get;
            private set;
        }
    }
}