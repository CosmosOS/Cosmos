using System;
using System.Linq;
using System.Text;
using Indy.IL2CPU.Assembler;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Asm = Indy.IL2CPU.Assembler.Assembler;
using Instruction=Mono.Cecil.Cil.Instruction;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(Code.Ldstr)]
	public class LdStr: Op {
		public override void Assemble(Instruction aInstruction, MethodInformation aMethodInfo) {
            // Make sure the crawler finds string constructors
            DoQueueMethod(typeof(String).Assembly.FullName, typeof(String).FullName, ".ctor");
            
            // todo: see if we need to output trailing bytes 00 00 or 00 01 depending on whether there are bytes >7F
			string xDataName = Assembler.GetIdentifier("StringLiteral");
			var xDataByteArray = new StringBuilder();
			foreach (byte x in Encoding.Unicode.GetBytes((string)aInstruction.Operand)) {
				xDataByteArray.Append(x.ToString());
				xDataByteArray.Append(",");
			}
            Assembler.DataMembers.Add(new DataMember(xDataName, "db", xDataByteArray.ToString().TrimEnd(',')));

			new Newobj().Assemble((new Label(typeof(String).FullName, typeof(Char).FullName + "*")).Name);
		}
	}
}