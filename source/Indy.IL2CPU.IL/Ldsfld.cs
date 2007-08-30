using System;
using Mono.Cecil.Cil;

namespace Indy.IL2CPU.IL {
	[OpCode(Code.Ldsfld)]
	public abstract class Ldsfld: Op {
	}
}