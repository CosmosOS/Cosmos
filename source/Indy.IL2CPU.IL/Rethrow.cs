using System;
using Mono.Cecil.Cil;

namespace Indy.IL2CPU.IL {
	[OpCode(Code.Rethrow)]
	public abstract class Rethrow: Op {
	}
}