using System;

namespace Cosmos.Assembler.x86
{
    [OpCode("setcc")]
    public class SetByteOnCondition : InstructionWithDestinationAndSize, IInstructionWithCondition
    {
        public ConditionalTestEnum Condition
        {
            get;
            set;
        }

        public override void WriteText(Assembler aAssembler, System.IO.TextWriter aOutput)
        {
            if (!DestinationIsIndirect && !Registers.Is8Bit(DestinationReg.Value))
            {
                throw new Exception("Size not supported!");
            }

            Size = 8;

            mMnemonic = "set" + Condition.GetMnemonic();
            base.WriteText(aAssembler, aOutput);
        }
    }
}
