using System;
using System.Linq;
using Cosmos.Assembler;
using Cosmos.Assembler.x86;

namespace MatthijsPlayground
{
	public class Test: Cosmos.IL2CPU.Plugs.AssemblerMethod
	{
		public override void AssembleNew(object aAssembler, object aMethodInfo)
		{
new Comment("This is a comment");
new Comment("This next line allows emission of standard yucky Intel mnemonics");
new LiteralAssemblerCode("Mov EAX, 1");
new Comment("This is X# code");
new Move{DestinationReg = RegistersEnum.EAX, SourceValue = 1};
new Move{DestinationReg = RegistersEnum.EDX, SourceValue = 2};
new Move{DestinationReg = RegistersEnum.EDX, SourceValue = 2};
		}
	}
}
