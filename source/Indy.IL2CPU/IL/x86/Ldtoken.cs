using System;
using System.Collections.Generic;
using System.IO;
using Indy.IL2CPU.Assembler;


using CPU = Indy.IL2CPU.Assembler.X86;
using System.Reflection;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(OpCodeEnum.Ldtoken)]
	public class Ldtoken: Op {
		private string mTokenAddress;

        public static void ScanOp(ILReader aReader, MethodInformation aMethodInfo, SortedList<string, object> aMethodData) {
            FieldInfo xFieldDef = aReader.OperandValueField;
            if (xFieldDef != null)
            {
                if (!xFieldDef.IsStatic)
                {
                    throw new Exception("Nonstatic field-backed tokens not supported yet!");
                }
                Engine.QueueStaticField(xFieldDef);
                return;
            }
            Type xTypeRef = aReader.OperandValueType;
            if (xTypeRef != null)
            {
                return;
            }
            throw new Exception("Token type not supported yet!");
        }
		public Ldtoken(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
			// todo: add support for type tokens and method tokens
			FieldInfo xFieldDef = aReader.OperandValueField;
			if (xFieldDef != null) {
				if (!xFieldDef.IsStatic) {
					throw new Exception("Nonstatic field-backed tokens not supported yet!");
				}
				Engine.QueueStaticField(xFieldDef);
				mTokenAddress = DataMember.GetStaticFieldName(xFieldDef);
				return;
			}
			Type xTypeRef = aReader.OperandValueType;
			if(xTypeRef!=null) {
                throw new Exception("Type Tokens not supported atm!");
//				mTokenAddress = "0" + Engine.RegisterType(xTypeRef).ToString("X") + "h";
				//return;
			}
			throw new Exception("Token type not supported yet!");
		}

		public override void DoAssemble() {
            new CPU.Push { DestinationRef = new ElementReference(mTokenAddress) };
			Assembler.StackContents.Push(new StackContent(4, typeof(uint)));
		}
	}
}