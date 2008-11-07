using System;
using System.Collections.Generic;
using System.IO;


using CPU = Indy.IL2CPU.Assembler.X86;
using Indy.IL2CPU.Assembler;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(OpCodeEnum.Sizeof)]
	public class Sizeof: Op {
		private uint mTheSize;
        public static void ScanOp(ILReader aReader, MethodInformation aMethodInfo, SortedList<string, object> aMethodData) {
            Type xTypeRef = aReader.OperandValueType;
            if (xTypeRef == null)
            {
                throw new Exception("Type not found!");
            }
            Engine.RegisterType(xTypeRef);
        }
		public Sizeof(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
			Type xTypeRef = aReader.OperandValueType;
			if (xTypeRef == null) {
				throw new Exception("Type not found!");}
			Engine.GetTypeFieldInfo(xTypeRef, out mTheSize);
		}
		public override void DoAssemble() {
            new CPU.Push { DestinationValue =mTheSize};
			Assembler.StackContents.Push(new StackContent(4, typeof(int)));
		}
	}
}