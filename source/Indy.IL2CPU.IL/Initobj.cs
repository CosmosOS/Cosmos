using System;
using Mono.Cecil.Cil;

namespace Indy.IL2CPU.IL {
	[OpCode(Code.Initobj)]
	public abstract class Initobj: Op {
	}
}