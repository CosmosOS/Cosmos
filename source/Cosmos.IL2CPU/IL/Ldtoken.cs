using System;
using Cosmos.IL2CPU.ILOpCodes;
using CPUx86 = Cosmos.Assembler.x86;
using CPU = Cosmos.Assembler.x86;
using Cosmos.Assembler;
using XSharp.Compiler;

namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Ldtoken )]
    public class Ldtoken : ILOp
    {
        public Ldtoken( Cosmos.Assembler.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            OpToken xToken = ( OpToken )aOpCode;
            string xTokenAddress = null;

            if (xToken.ValueIsType)
            {
                xTokenAddress = ILOp.GetTypeIDLabel(xToken.ValueType);
            }
            if (xToken.ValueIsField)
            {
                xTokenAddress= DataMember.GetStaticFieldName(xToken.ValueField);
            }

            if (String.IsNullOrEmpty(xTokenAddress))
            {
                throw new Exception("Ldtoken not implemented!");
            }

            //if( mType != null )
            //{
            //    mTokenAddress = GetService<IMetaDataInfoService>().GetTypeIdLabel( mType );
            //}
            //XS.Push(xToken.Value);
            XS.Push(xTokenAddress);
            XS.Push(0);
        }


        // using System;
        // using System.Collections.Generic;
        // using System.IO;
        // using Cosmos.IL2CPU.X86;
        //
        //
        // using CPU = Cosmos.Assembler.x86;
        // using System.Reflection;
        // using Cosmos.IL2CPU.Compiler;
        //
        // namespace Cosmos.IL2CPU.IL.X86 {
        // 	[Cosmos.Assembler.OpCode(OpCodeEnum.Ldtoken)]
        // 	public class Ldtoken: Op {
        // 		private string mTokenAddress;
        //
        //         //public static void ScanOp(ILReader aReader, MethodInformation aMethodInfo, SortedList<string, object> aMethodData) {
        //         //    FieldInfo xFieldDef = aReader.OperandValueField;
        //         //    if (xFieldDef != null)
        //         //    {
        //         //        if (!xFieldDef.IsStatic)
        //         //        {
        //         //            throw new Exception("Nonstatic field-backed tokens not supported yet!");
        //         //        }
        //         //        Engine.QueueStaticField(xFieldDef);
        //         //        return;
        //         //    }
        //         //    Type xTypeRef = aReader.OperandValueType;
        //         //    if (xTypeRef != null)
        //         //    {
        //         //        return;
        //         //    }
        //         //    throw new Exception("Token type not supported yet!");
        //         //}
        //
        // 		public Ldtoken(ILReader aReader, MethodInformation aMethodInfo)
        // 			: base(aReader, aMethodInfo) {
        // 			// todo: add support for type tokens and method tokens
        // 			FieldInfo xFieldDef = aReader.OperandValueField;
        // 			if (xFieldDef != null) {
        // 				if (!xFieldDef.IsStatic) {
        // 					throw new Exception("Nonstatic field-backed tokens not supported yet!");
        // 				}
        // 				mTokenAddress = DataMember.GetStaticFieldName(xFieldDef);
        // 				return;
        // 			}
        // 			mType= aReader.OperandValueType;
        //             if (mType != null)
        //             {
        //                 return;
        //             }
        // 			throw new Exception("Token type not supported yet!");
        // 		}
        //
        //         private Type mType;
        //
        // 		public override void DoAssemble() {
        //             if (mType != null)
        //             {
        //                 mTokenAddress = GetService<IMetaDataInfoService>().GetTypeIdLabel(mType);
        //             }
        //             XS.Push(Cosmos.Assembler.ElementReference.New(mTokenAddress));
        // 			Assembler.Stack.Push(new StackContent(4, typeof(uint)));
        // 		}
        // 	}
        // }

    }
}
