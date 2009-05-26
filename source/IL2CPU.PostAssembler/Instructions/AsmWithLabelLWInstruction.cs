using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IL2CPU.PostAssembler
{
    internal class AsmWithLabelLWInstruction : LWInstruction, ILabelInstruction
    {
        private string[] mAsm;
        private string[] mLabel; 

        internal AsmWithLabelLWInstruction(string asm)
        {
            if (string.IsNullOrEmpty(asm))
                throw new ArgumentNullException("empty instruction"); 

            var xAsm = new List<string>(new StringCompactor().StringToCompact(asm)); 

            if ( xAsm.Count <2 )
                throw new ArgumentNullException("Does not contain label or instruction"); 

            string label =  xAsm[xAsm.Count-1];
            xAsm.Remove(label);

            mAsm = xAsm.ToArray(); 
            mLabel =  new StringCompactor().LabelToCompact(label); 
        }

        public override LWInstructionType InstructionType
        {
            get { return LWInstructionType.AsmInstruction; }
        }


        public string Label
        {
            get { return new StringCompactor().CompactToLabel(mLabel); }
        }

        public override string ToString()
        {
            var list = new List<string>(mAsm);
            list.Add(Label);
            return  new StringCompactor().CompactToString(list.ToArray()) ;
        }



        public string OpCode
        {
            get
            {
                return mAsm[0];
            }
        }
    }
}
