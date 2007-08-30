using System;
using Mono.Cecil.Cil;

namespace Indy.IL2CPU.IL {
	[OpCode(Code.Jmp)]
	public abstract class Jmp: Op {
	}
}