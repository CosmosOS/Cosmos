using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EcmaCil.IL
{
    public class InstructionToken: BaseInstruction
    {
        public InstructionToken(InstructionKindEnum aKind, int aIndex, BaseMeta aToken)
            : base(aKind, aIndex)
        {
            Token = aToken;
        }

        public BaseMeta Token
        {
            get;
            private set;
        }
    }
}