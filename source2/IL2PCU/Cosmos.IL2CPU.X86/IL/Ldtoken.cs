using System;
using Cosmos.IL2CPU.ILOpCodes;
using CPUx86 = Cosmos.IL2CPU.X86;
using CPU = Cosmos.IL2CPU.X86;
using Indy.IL2CPU;
namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Ldtoken )]
    public class Ldtoken : ILOp
    {
        public Ldtoken( Cosmos.IL2CPU.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            OpToken xToken = ( OpToken )aOpCode;

            throw new Exception("Ldtoken not implemented!");
            

            //if( mType != null )
            //{
            //    mTokenAddress = GetService<IMetaDataInfoService>().GetTypeIdLabel( mType );
            //}
            new CPUx86.Push { DestinationValue = xToken.Value };
            //new CPU.Push { DestinationRef = ElementReference.New( mTokenAddress ) };
            Assembler.Stack.Push( new StackContents.Item( 4, typeof( uint ) ) );
        }


        // using System;
        // using System.Collections.Generic;
        // using System.IO;
        // using Cosmos.IL2CPU.X86;
        // 
        // 
        // using CPU = Cosmos.IL2CPU.X86;
        // using System.Reflection;
        // using Indy.IL2CPU.Compiler;
        // 
        // namespace Indy.IL2CPU.IL.X86 {
        // 	[OpCode(OpCodeEnum.Ldtoken)]
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
        //             new CPU.Push { DestinationRef = ElementReference.New(mTokenAddress) };
        // 			Assembler.Stack.Push(new StackContent(4, typeof(uint)));
        // 		}
        // 	}
        // }

    }
}
