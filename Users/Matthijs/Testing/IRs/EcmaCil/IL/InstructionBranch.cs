using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EcmaCil.IL
{
    public class InstructionBranch: BaseInstruction
    {
        public InstructionBranch(InstructionKindEnum aKind, int aIndex)
            : base(aKind, aIndex)
        {
        }

        private BaseInstruction mTarget;
        public BaseInstruction Target
        {
            get
            {
                return mTarget;
            }
            set
            {
                if (mTarget != null)
                {
                    throw new Exception("Cannot change Target once set!");
                }
                mTarget = value;
            }
        }
    }
}