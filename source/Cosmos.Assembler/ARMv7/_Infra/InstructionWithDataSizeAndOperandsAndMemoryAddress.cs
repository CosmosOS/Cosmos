using System;

namespace Cosmos.Assembler.ARMv7
{
    public abstract class InstructionWithDataSizeAndOperandsAndMemoryAddress : Instruction, IInstructionWithDataSize, IInstructionWithOperand, IInstructionWithSecondOperand, IInstructionWithMemoryAddress
    {
        public InstructionWithDataSizeAndOperandsAndMemoryAddress()
        {
        }

        public InstructionWithDataSizeAndOperandsAndMemoryAddress(string mnemonic) : base(mnemonic)
        {
        }

        public DataSize? DataSize
        {
            get;
            set;
        }

        public RegistersEnum? OperandReg
        {
            get;
            set;
        }

        public RegistersEnum? SecondOperandReg
        {
            get;
            set;
        }

        public RegistersEnum? BaseMemoryAddressReg
        {
            get;
            set;
        }

        public RegistersEnum? MemoryAddressOffsetReg
        {
            get;
            set;
        }

        public short? MemoryAddressOffsetValue
        {
            get;
            set;
        }

        public MemoryAddressOffsetType MemoryAddressOffsetType
        {
            get;
            set;
        }

        public OptionalShift MemoryAddressOptionalShift
        {
            get;
            set;
        }

        public override bool IsComplete(Assembler aAssembler)
        {
            return base.IsComplete(aAssembler);
        }

        public override void UpdateAddress(Assembler aAssembler, ref ulong aAddress)
        {
            base.UpdateAddress(aAssembler, ref aAddress);
        }

        public override void WriteText(Assembler aAssembler, System.IO.TextWriter aOutput)
        {
            aOutput.Write(mMnemonic);

            aOutput.Write(this.GetDataSizeAsString());

            aOutput.Write(this.GetConditionAsString());

            string firstOperand = this.GetOperandAsString();

            if (!firstOperand.Equals(""))
            {
                aOutput.Write(" ");
                aOutput.Write(firstOperand);
            }

            if ((int)DataSize.Value >= 5)
            {
                string secondOperand = this.GetSecondOperandAsString();

                if (!secondOperand.Equals(""))
                {
                    if (!firstOperand.Equals(""))
                    {
                        aOutput.Write(", ");
                    }

                    aOutput.Write(secondOperand);
                }
            }

            string memoryAddress = this.GetMemoryAddressAsString();

            if (!memoryAddress.Equals(""))
            {
                if (!firstOperand.Equals(""))
                {
                    aOutput.Write(", ");
                }

                aOutput.Write(memoryAddress);
            }
        }
    }
}
