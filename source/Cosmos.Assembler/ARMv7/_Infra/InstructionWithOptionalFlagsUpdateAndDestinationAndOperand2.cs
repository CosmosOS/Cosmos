namespace Cosmos.Assembler.ARMv7
{
    public class InstructionWithOptionalFlagsUpdateAndDestinationAndOperand2 : Instruction, IInstructionWithOptionalFlagsUpdate, IInstructionWithDestination, IInstructionWithOperand2
    {
        public InstructionWithOptionalFlagsUpdateAndDestinationAndOperand2()
        {
        }

        public InstructionWithOptionalFlagsUpdateAndDestinationAndOperand2(string mnemonic) : base(mnemonic)
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

            if(UpdateFlags)
            {
                aOutput.Write("S");
            }

            aOutput.Write(this.GetConditionAsString());

            string destination = this.GetDestinationAsString();
            string operand2 = this.GetOperand2AsString();

            if (!destination.Equals(""))
            {
                aOutput.Write(" ");
                aOutput.Write(destination);
            }

            if (!(operand2.Equals("")))
            {
                if (!destination.Equals(""))
                {
                    aOutput.Write(", ");
                }

                aOutput.Write(operand2);
            }
        }
    }
}
