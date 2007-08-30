using System;
using Mono.Cecil.Cil;

namespace Indy.IL2CPU.IL {
	[OpCode(Code.Ldtoken)]
	public abstract class Ldtoken: Op {
	}
}