using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EcmaCil.IL
{
    public class InstructionType: BaseInstruction
    {
        public InstructionType(InstructionKindEnum aKind, int aIndex, TypeMeta aType)
            : base(aKind, aIndex)
        {
            Type = aType;
        }

        public TypeMeta Type
        {
            get;
            private set;
        }
    }
}