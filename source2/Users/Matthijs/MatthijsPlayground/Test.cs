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
new Comment(" First line of X#");
new LiteralAssemblerCode(" Literal assembler code");
new Move{DestinationReg = RegistersEnum.EAX, SourceValue = 1};
		}
	}
}
