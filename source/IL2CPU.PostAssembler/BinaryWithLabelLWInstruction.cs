using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IL2CPU.PostAssembler
{
    internal class BinaryWithLabelLWInstruction : LWInstruction, ILabelInstruction
    {

        public override LWInstructionType InstructionType
        {
            get { return LWInstructionType.BinaryInstructionWithLabel; }
        }

        #region ILabelInstruction Members

        public string Label
        {
            get { throw new NotImplementedException(); }
        }

        #endregion
    }
}
