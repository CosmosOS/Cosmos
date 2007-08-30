using System;
using Mono.Cecil.Cil;

namespace Indy.IL2CPU.IL {
	[OpCode(Code.Throw)]
	public abstract class Throw: Op {
	}
}