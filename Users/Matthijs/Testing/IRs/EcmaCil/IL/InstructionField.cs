using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EcmaCil.IL
{
    public class InstructionField: BaseInstruction
    {
        public InstructionField(InstructionKindEnum aKind, int aIndex, FieldMeta aField)
            : base(aKind, aIndex)
        {
            Field = aField;
        }

        public FieldMeta Field
        {
            get;
            private set;
        }
    }
}