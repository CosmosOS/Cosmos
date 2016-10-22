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

        public RegistersEnum? FirstOperandReg
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

        public short? MemoryAddressOffset
        {
            get;
            set;
        }

        public MemoryAddressOffsetType MemoryAddressOffsetType
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

            string firstOperand = this.GetFirstOperandAsString();

            if (!(firstOperand.Equals("")))
            {
                aOutput.Write(" ");
                aOutput.Write(firstOperand);
            }

            if ((int)DataSize.Value >= 5)
            {
                string secondOperand = this.GetSecondOperandAsString();
            }

            string memoryAddress = this.GetMemoryAddressAsString();

            if (!memoryAddress.Equals(""))
            {
                aOutput.Write(", ");
                aOutput.Write(memoryAddress);
            }
        }
    }
}
