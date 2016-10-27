namespace Cosmos.Assembler.ARMv7
{
    public abstract class InstructionWithOptionalFlagsUpdateAndDestinationAndOperand : Instruction, IInstructionWithOptionalFlagsUpdate, IInstructionWithDestination, IInstructionWithOperand
    {
        public InstructionWithOptionalFlagsUpdateAndDestinationAndOperand()
        {
        }

        public InstructionWithOptionalFlagsUpdateAndDestinationAndOperand(string mnemonic) : base(mnemonic)
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
            string operand = this.GetOperandAsString();

            if (!destination.Equals(""))
            {
                aOutput.Write(" ");
                aOutput.Write(destination);
            }

            if (!operand.Equals(""))
            {
                if (!destination.Equals(""))
                {
                    aOutput.Write(", ");
                }

                aOutput.Write(operand);
            }
        }
    }
}
