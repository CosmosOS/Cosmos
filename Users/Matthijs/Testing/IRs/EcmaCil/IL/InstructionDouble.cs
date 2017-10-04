using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EcmaCil.IL
{
    public class InstructionDouble: BaseInstruction
    {
        public InstructionDouble(InstructionKindEnum aKind, int aIndex, Double aValue)
            : base(aKind, aIndex)
        {
            Value = aValue;
        }

        public Double Value
        {
            get;
            private set;
        }
    }
}