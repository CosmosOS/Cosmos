using System;
using Mono.Cecil.Cil;

namespace Indy.IL2CPU.IL {
	[OpCode(Code.Ldarg)]
	public abstract class Ldarg: Op {
	}
}