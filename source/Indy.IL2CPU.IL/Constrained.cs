using System;
using Mono.Cecil.Cil;

namespace Indy.IL2CPU.IL {
	[OpCode(Code.Constrained)]
	public abstract class Constrained: Op {
	}
}