using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EcmaCil.IL
{
    public abstract class BaseInstruction
    {
        protected BaseInstruction(InstructionKindEnum aInstructionKind, int aInstructionIndex)
        {
            mInstructionKind = aInstructionKind;
            InstructionIndex = aInstructionIndex;
        }
        private readonly InstructionKindEnum mInstructionKind;

        public InstructionKindEnum InstructionKind
        {
            get
            {
                return mInstructionKind;
            }
        }

        public int InstructionIndex
        {
            get;
            set;
        }
    }
}