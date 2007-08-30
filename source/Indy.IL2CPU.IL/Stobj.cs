using System;
using Mono.Cecil.Cil;

namespace Indy.IL2CPU.IL {
	[OpCode(Code.Stobj)]
	public abstract class Stobj: Op {
	}
}