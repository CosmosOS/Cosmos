using System;
using Mono.Cecil.Cil;

namespace Indy.IL2CPU.IL {
	[OpCode(Code.Readonly)]
	public abstract class Readonly: Op {
	}
}