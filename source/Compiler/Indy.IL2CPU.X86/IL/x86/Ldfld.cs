using System;
using System.Collections.Generic;
using System.IO;


using CPU = Indy.IL2CPU.Assembler;
using System.Reflection;
using Indy.IL2CPU.Compiler;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(OpCodeEnum.Ldfld)]
	public class Ldfld: Op {
        private Type mDeclaringType;
		private TypeInformation.Field mFieldInfo;
		private readonly TypeInformation mTypeInfo;
        private string mFieldId;

        //public static void ScanOp(ILReader aReader, MethodInformation aMethodInfo, SortedList<string, object> aMethodData) {
        //    FieldInfo xField = aReader.OperandValueField;
        //    if (xField == null)
        //    {
        //        throw new Exception("Field not found!");
        //    }
        //    Engine.RegisterType(xField.DeclaringType);
        //    Engine.RegisterType(xField.FieldType);
        //}

		public Ldfld(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
			FieldInfo xField = aReader.OperandValueField;
			if (xField == null) {
					throw new Exception("Field not found!");
			}
			mFieldId = xField.GetFullName();
            mDeclaringType = xField.DeclaringType;
		}

		public override void DoAssemble() {
            var xType = GetService<IMetaDataInfoService>().GetTypeInfo(mDeclaringType);
            if (!xType.Fields.ContainsKey(mFieldId))
            {
                Console.Write("");
                xType = GetService<IMetaDataInfoService>().GetTypeInfo(mDeclaringType);
                xType = GetService<IMetaDataInfoService>().GetTypeInfo(mDeclaringType);
                xType = GetService<IMetaDataInfoService>().GetTypeInfo(mDeclaringType);
                xType = GetService<IMetaDataInfoService>().GetTypeInfo(mDeclaringType);
            }
            mFieldInfo = xType.Fields[mFieldId];
			Ldfld(Assembler, xType, mFieldId);
		}
	}
}