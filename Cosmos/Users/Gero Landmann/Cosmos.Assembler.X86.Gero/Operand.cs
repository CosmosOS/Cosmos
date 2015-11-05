using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Assembler.X86
{
    public class Operand
    {
        /// <summary>
        /// is this EncodingOption valid for situations where the Operand is a register?
        /// if so, DestinationReg == Guid.Empty. if it is specific to a given register, the 32bit id is put in DestinationReg
        /// </summary>
        public RegistersEnum BaseRegister;
       
        public BitSize RegisterSize;
        public RegisterGroup RegisterGroup;

        /// <summary>
        /// the index in OpCode where the Register bit is encoded
        /// </summary>
        public sbyte? RegisterByte;
        /// <summary>
        /// the amount of bits the Register bits gets shifted to left, if neccessary
        /// </summary>
        public byte RegisterBitShiftLeft;

        

        /// <summary>
        /// is this EncodingOption valid for situations where the Operand is memory?
        /// </summary>
        public OperandType Type;
        /// <summary>
        /// is this EncodingOption valid for situations where the Operand is an immediate value
        /// </summary>
        public BitSize ImmediateSize = BitSize.None;

        public override string ToString()
        {
            return base.ToString();
        }
    }
}
