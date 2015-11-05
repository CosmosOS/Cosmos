using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EcmaCil.IL
{
    public class InstructionString: BaseInstruction
    {
        public InstructionString(InstructionKindEnum aKind, int aIndex, string aString)
            : base(aKind, aIndex)
        {
            LiteralString = aString;
        }

        public string LiteralString
        {
            get;
            private set;
        }
    }
}