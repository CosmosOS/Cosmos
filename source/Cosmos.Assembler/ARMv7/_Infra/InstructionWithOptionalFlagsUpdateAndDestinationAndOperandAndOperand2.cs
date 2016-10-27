namespace Cosmos.Assembler.ARMv7
{
    public abstract class InstructionWithOptionalFlagsUpdateAndDestinationAndOperandAndOperand2 : Instruction, IInstructionWithOptionalFlagsUpdate, IInstructionWithDestination, IInstructionWithOperand, IInstructionWithOperand2
    {
        public InstructionWithOptionalFlagsUpdateAndDestinationAndOperandAndOperand2()
        {
        }

        public InstructionWithOptionalFlagsUpdateAndDestinationAndOperandAndOperand2(string mnemonic) : base(mnemonic)
        {
        }

        public bool UpdateFlags
        {
            get;
            set;
        }

        public RegistersEnum? DestinationReg
        {
            get;
            set;
        }

        public RegistersEnum? OperandReg
        {
            get;
            set;
        }

        public RegistersEnum? Operand2Reg
        {
            get;
            set;
        }

        public uint? Operand2Value
        {
            get;
            set;
        }

        public OptionalShift Operand2Shift
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

            if (UpdateFlags)
            {
                aOutput.Write("S");
            }

            aOutput.Write(this.GetConditionAsString());

            string destination = this.GetDestinationAsString();
            string firstOperand = this.GetOperandAsString();
            string operand2 = this.GetOperand2AsString();

            if (!destination.Equals(""))
            {
                aOutput.Write(" ");
                aOutput.Write(destination);
            }

            if (!destination.Equals("") && !firstOperand.Equals(""))
            {
                aOutput.Write(", ");
            }

            if (!firstOperand.Equals(""))
            {
                aOutput.Write(firstOperand);
            }

            if (!operand2.Equals(""))
            {
                if (!firstOperand.Equals(""))
                {
                    aOutput.Write(", ");
                }

                aOutput.Write(operand2);
            }
        }
    }
}
