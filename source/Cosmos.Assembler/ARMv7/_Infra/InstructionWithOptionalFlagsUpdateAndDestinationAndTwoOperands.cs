namespace Cosmos.Assembler.ARMv7
{
    public abstract class InstructionWithOptionalFlagsUpdateAndDestinationAndTwoOperands : Instruction, IInstructionWithOptionalFlagsUpdate, IInstructionWithDestination, IInstructionWithOperand, IInstructionWithSecondOperand
    {
        public InstructionWithOptionalFlagsUpdateAndDestinationAndTwoOperands()
        {
        }

        public InstructionWithOptionalFlagsUpdateAndDestinationAndTwoOperands(string mnemonic) : base(mnemonic)
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

        public RegistersEnum? SecondOperandReg
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
            string secondOperand = this.GetSecondOperandAsString();

            if (!destination.Equals(""))
            {
                aOutput.Write(" ");
                aOutput.Write(destination);
            }

            if (!firstOperand.Equals(""))
            {
                if(!destination.Equals(""))
                {
                    aOutput.Write(", ");
                }

                aOutput.Write(firstOperand);
            }

            if (!secondOperand.Equals(""))
            {
                if (!firstOperand.Equals(""))
                {
                    aOutput.Write(", ");
                }

                aOutput.Write(secondOperand);
            }
        }
    }
}
