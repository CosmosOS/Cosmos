using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IL2CPU.PostAssembler
{
    internal class LabelLWInstruction : LWInstruction, ILabelInstruction
    {
        private string[] mLabel; 

        internal LabelLWInstruction(string label)
        {
            mLabel = new StringCompactor().LabelToCompact(label); 

        }

        public override LWInstructionType InstructionType
        {
            get { return LWInstructionType.Label; }
        }



        public string Label
        {
            get { return new StringCompactor().CompactToLabel(mLabel); }
        }

        public override string ToString()
        {
            return Label;
        }




        // split on __Version ? 
        //		System_Void__System_Collections_Generic_Dictionary_2__System_Guid__mscorlib__Version_2_0_0_0__Culture_neutral__PublicKeyToken_b77a5c561934e089___System_Object__mscorlib__Version_2_0_0_0__Culture_neutral__PublicKeyToken_b77a5c561934e0890___Clear____DOT__END__OF__METHOD_EXCEPTION:
        // lots of repeats here.


    }
}
