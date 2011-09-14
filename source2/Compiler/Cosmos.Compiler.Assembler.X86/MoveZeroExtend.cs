namespace Cosmos.Compiler.Assembler.X86 {
    [OpCode("movzx")]
	public class MoveZeroExtend : InstructionWithDestinationAndSourceAndSize
	{

		public override void WriteText(Assembler aAssembler, System.IO.TextWriter aOutput)
		{
			if (Size == 0)
			{
				Size = 32;
			}
			aOutput.Write(mMnemonic);
			if (!DestinationEmpty)
			{
				aOutput.Write(" ");
				aOutput.Write(this.GetDestinationAsString());
				aOutput.Write(", ");
				aOutput.Write(SizeToString(Size));
				aOutput.Write(" ");
				aOutput.Write(this.GetSourceAsString());
			}
		}
	}
}