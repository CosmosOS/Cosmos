using System;
using Mono.Cecil.Cil;

namespace Indy.IL2CPU.IL {
	[OpCode(Code.Ldobj)]
	public abstract class Ldobj: Op {
	}
}