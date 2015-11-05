using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Assembler.X86
{
    /// <summary>
    /// This class represents a intructions and has information about all variants for it
    /// </summary>
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

				public abstract bool Initialize();
    }
}
