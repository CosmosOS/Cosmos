using System;
using Mono.Cecil.Cil;

namespace Indy.IL2CPU.IL {
	[OpCode(Code.Box)]
	public abstract class Box: Op {
	}
}