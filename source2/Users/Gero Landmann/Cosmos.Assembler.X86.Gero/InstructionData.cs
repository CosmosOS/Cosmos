using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Assembler.X86
{
    /// <summary>
    /// 
    /// </summary>
    public class InstructionData : Cosmos.Assembler.X86.IInstructionData
    {
        /// <summary>
        /// Gets or sets the size.
        /// </summary>
        /// <value>The size.</value>
        public BitSize Size { get; set; }

        /// <summary>
        /// Gets or sets the target instruction set.
        /// </summary>
        /// <value>The target instruction set.</value>
        public InstructionSet InstructionSet { get; set; }

        public Operand Op1 { get; set; }
        public Operand Op2 { get; set; }
        public Operand Op3 { get; set; }
        public Operand Op4 { get; set; }

        public string ToString( InstructionOutputFormat aFormat )
        {
            string Instruction = "";
            string tmp;
            switch( aFormat )
            {
                case InstructionOutputFormat.ASM:
                    tmp = Size.ToString();
                    if( tmp == "" )
                        return Instruction;
                    Instruction += tmp + " ";

                    
                    //tmp = this.GetDestinationAsString();
                    if( tmp == "" )
                        return Instruction;
                    Instruction += tmp + " ";

                    //tmp = this.GetSourceAsString();
                    if( tmp == "" )
                        return Instruction;
                    Instruction += ", " + tmp;

                    return Instruction;
            }

            return "Format not supported.";
        }
    }
}
