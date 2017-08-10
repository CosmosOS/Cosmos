using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EcmaCil.IL
{
    public class InstructionArgument: BaseInstruction
    {
        public InstructionArgument(InstructionKindEnum aKind, int aIndex, MethodParameterMeta aArgument):base(aKind, aIndex)
        {
            Argument = aArgument;
        }

        public MethodParameterMeta Argument
        {
            get;
            private set;
        }
    }
}