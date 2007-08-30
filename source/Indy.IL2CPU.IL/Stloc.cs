using System;
using Mono.Cecil.Cil;

namespace Indy.IL2CPU.IL {
	[OpCode(Code.Stloc)]
	public abstract class Stloc: Op {
	}
}