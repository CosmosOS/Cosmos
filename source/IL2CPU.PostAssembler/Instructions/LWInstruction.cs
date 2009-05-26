using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IL2CPU.PostAssembler
{

    /// <summary>
    /// Purpose to hold instruction in as light a format as possible to enable optomizations
    /// 
    /// Note Instructions purpose on the other hand is to generate the x86 from MISL. Instruction is good for local optomization 
    /// 
    /// LWInstruction is good for global optomization. 
    /// 
    /// We start with storing the string and byte[] in the long term hopefully we dont need the string.
    /// 
    /// In the long term we may be able to skip the text  gerneation entirley or disassble it from the byte codes. 
    /// 
    /// 
    /// Use access inheritance and inheritance is bad but in this case we use it to ensure the minimum information is stored.
    /// 
    /// </summary>
    public abstract class  LWInstruction :ILWInstruction
    {
        //Root 
        //    String
        //        Label 
        //        Comment
        //    Instruction
        //        Binary
        //        String 
        //        Both


             
        // added in constructor 
        // Option 1 variable arguments 
        //Option2 inheritance.. 


        //size and by inheritance

     
        //public virtual string ToString()
        //{
        //    return string.Empty; 
        //}


        public virtual byte[] ToBinary()
        {
            return new byte[0];
        }


        public abstract LWInstructionType InstructionType { get; }

      
    }
}
