﻿namespace Cosmos.Assembler.ARMv7
{
    public class InstructionWithDestinationAndOperand : Instruction, IInstructionWithDestination, IInstructionWithOperand
    {
        public InstructionWithDestinationAndOperand()
        {
        }

        public InstructionWithDestinationAndOperand(string mnemonic) : base(mnemonic)
        {
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
