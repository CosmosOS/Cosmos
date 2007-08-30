using System;
using Mono.Cecil.Cil;

namespace Indy.IL2CPU.IL {
	[OpCode(Code.Unaligned)]
	public abstract class Unaligned: Op {
	}
}