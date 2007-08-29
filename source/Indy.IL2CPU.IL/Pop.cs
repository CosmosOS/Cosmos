using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil.Cil;

namespace Indy.IL2CPU.IL {
	[OpCode(Code.Pop)]
	public class Pop: Op
	{
		public override void Process(Instruction aInstruction)
		{
			Console.WriteLine("Pop");
		}
	}
}