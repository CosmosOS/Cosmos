using System;
using System.IO;
using Mono.Cecil.Cil;

namespace Indy.IL2CPU.IL {
	[OpCode(Code.Call)]
	public abstract class Call: Op {
	}
}