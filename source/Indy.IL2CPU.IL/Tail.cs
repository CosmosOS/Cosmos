using System;
using Mono.Cecil.Cil;

namespace Indy.IL2CPU.IL {
	[OpCode(Code.Tail)]
	public abstract class Tail: Op {
	}
}