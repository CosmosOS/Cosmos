using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EcmaCil.IL
{
    public class InstructionLocal: BaseInstruction
    {
        public InstructionLocal(InstructionKindEnum aKind, int aIndex, LocalVariableMeta aLocal)
            : base(aKind, aIndex)
        {
            LocalVariable = aLocal;
        }

        public LocalVariableMeta LocalVariable
        {
            get;
            private set;
        }
    }
}