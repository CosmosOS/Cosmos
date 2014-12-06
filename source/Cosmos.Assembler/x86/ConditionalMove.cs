namespace Cosmos.Assembler.x86
{
    [Cosmos.Assembler.OpCode("cmovcc")]
    public class ConditionalMove : InstructionWithDestinationAndSourceAndSize, IInstructionWithCondition
    {
        public ConditionalTestEnum Condition
        {
            get;
            set;
        }

        public override void WriteText(Cosmos.Assembler.Assembler aAssembler, System.IO.TextWriter aOutput)
        {
            mMnemonic = "cmov" + Condition.GetMnemonic();
            base.WriteText(aAssembler, aOutput);
        }
    }
}