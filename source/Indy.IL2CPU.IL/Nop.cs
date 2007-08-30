using System;
using Mono.Cecil.Cil;

namespace Indy.IL2CPU.IL {
	[OpCode(Code.Nop)]
	public abstract class Nop: Op {
	}
}