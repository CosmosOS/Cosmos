using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Assembler.X86
{
    public abstract class Instruction
    {
        public InstructionEnum AsmInstruction { get; protected set; }
        public ConditionalTestEnum Condition { get; set; }

        public List<InstructionVariant> Variants { get; protected set; }

        public IInstructionData InstructionData { get; set; }
        public Instruction()
        { 
        
        }

        public InstructionVariant Match( InstructionData aData )
        {
            if( Variants == null && Initialize() == false )
                throw new NotSupportedException(); 


            return null;
        }

        public abstract bool Initialize() { return false;  }
    }
}
