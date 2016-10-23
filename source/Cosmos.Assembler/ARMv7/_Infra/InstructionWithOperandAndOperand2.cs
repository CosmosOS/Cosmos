namespace Cosmos.Assembler.ARMv7
{
    public abstract class InstructionWithOperandAndOperand2 : Instruction, IInstructionWithOperand, IInstructionWithOperand2
    {
        public InstructionWithOperandAndOperand2()
        {
        }

        public InstructionWithOperandAndOperand2(string mnemonic) : base(mnemonic)
        {
        }

        public RegistersEnum? FirstOperandReg
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

        public Operand2Shift Operand2Shift
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

            aOutput.Write(this.GetConditionAsString());

            string firstOperand = this.GetFirstOperandAsString();

            if (!(firstOperand.Equals("")))
            {
                aOutput.Write(" ");
                aOutput.Write(firstOperand);
            }

            string operand2 = this.GetOperand2AsString();

            if (!operand2.Equals(""))
            {
                aOutput.Write(", ");
                aOutput.Write(operand2);
            }
        }
    }
}
