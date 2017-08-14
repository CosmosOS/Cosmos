using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Assembler.X86
{
    /// <summary>
    /// This class represents a variant of a intructions with a specific asm code and specific operands
    /// </summary>
    public class InstructionVariant
    {
        //public Action<byte[], Instruction> ModifyBytes;

        //public byte[] OpCode;
        public byte[] NasmData;

        public List<Operand> AllowedOperandData { get; set; }

        /// <summary>
        /// Gets or sets the requirements like instructionsets
        /// </summary>
        /// <value>The requirements.</value>
        public InstructionRequirement Requirements { get; set; } 

        /// <summary>
        /// If ModR/M byte needed, set to true. If true, all other fields on <see cref="InstructionEncodingOption"/>
        /// which refer to <see cref="OpCode"/> bytes, can assume an extra ModRM byte.
        /// </summary>
        public bool NeedsModRMByte;
        public byte InitialModRMByteValue;
        public bool ReverseRegisters = false;

        public BitSize AllowedSizes = BitSize.Default;
        public BitSize DefaultSize = BitSize.Bits32;

        public string ToString( InstructionOutputFormat aFormat )
        {
            string Instruction = "";
            string tmp;
            switch( aFormat )
            {
                case InstructionOutputFormat.ASM:

								throw new Exception("Fix");
                    //tmp = Size.ToString();
                    if( tmp == "" )
                        return Instruction;

                    Instruction += tmp + " ";




                    return Instruction;
            }

            return "Format not supported.";
        }

        /// <summary>
        /// The index in OpCode where the OperandSize bit is encoded
        /// </summary>
        //public byte? OperandSizeByte;
        /// <summary>
        /// The amount of bits the operandsize bit gets shifted to left, if neccessary
        /// </summary>
        //public byte OperandSizeBitShiftLeft;
    }
}
