using System;
using CPUx86 = Cosmos.IL2CPU.X86;
namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Ldflda )]
    public class Ldflda : ILOp
    {
        public Ldflda( Cosmos.IL2CPU.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            int aExtraOffset = 0;
            var xType = aMethod.MethodBase.DeclaringType;
            var xOpCode = ( ILOpCodes.OpField )aOpCode;
            System.Reflection.FieldInfo xField = xOpCode.Value;
            bool xNeedsGC = xField.FieldType.IsClass && !xField.FieldType.IsValueType;
            uint xSize = SizeOfType( xField.FieldType );

            if( xNeedsGC )
            {
                aExtraOffset = 12;
            }
            uint xOffset = 0;

            var xFields = xField.DeclaringType.GetFields();

            foreach( System.Reflection.FieldInfo xInfo in xFields )
            {
                if( xInfo == xField )
                    break;

                xOffset += SizeOfType( xInfo.FieldType );
            }

            new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };

            new CPUx86.Add { DestinationReg = CPUx86.Registers.EAX, SourceValue = ( uint )( xOffset + aExtraOffset ) };
            Assembler.Stack.Pop();
            Assembler.Stack.Push( new StackContents.Item( 4, xType ) );
#warning TODO: Implement Plugs
            //if( aDerefExternalAddress && aField.IsExternalField )
            //{
            //    new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX, DestinationIsIndirect = true };
            //}
            //else
            {
                new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
            }
        }


        // using System;
        // using System.Collections.Generic;
        // using System.IO;
        // 
        // 
        // using CPUx86 = Cosmos.IL2CPU.X86;	    
        // using System.Reflection;
        // using Indy.IL2CPU.Compiler;
        // 
        // namespace Indy.IL2CPU.IL.X86 {
        // 	[OpCode(OpCodeEnum.Ldflda)]
        // 	public class Ldflda: Op {
        //         private Type mType;
        // 		private TypeInformation mTypeInfo;
        // 		private TypeInformation.Field mField;
        //         private string mFieldId;
        //         //public static void ScanOp(ILReader aReader, MethodInformation aMethodInfo, SortedList<string, object> aMethodData)
        //         //{
        //         //    FieldInfo xField = aReader.OperandValueField;
        //         //    if (xField == null)
        //         //    {
        //         //        throw new Exception("Field not found!");
        //         //    }
        //         //    Engine.RegisterType(xField.DeclaringType);
        //         //    Engine.RegisterType(xField.FieldType);
        //         //}
        // 
        // 		public Ldflda(ILReader aReader, MethodInformation aMethodInfo)
        // 			: base(aReader, aMethodInfo) {
        // 			FieldInfo xField = aReader.OperandValueField;
        // 			if (xField == null) {
        // 					throw new Exception("Field not found!");
        // 			}
        // 			mFieldId = xField.GetFullName();
        //             mType = xField.DeclaringType;
        // 		}
        // 
        // 		public override void DoAssemble() {
        //             mTypeInfo = GetService<IMetaDataInfoService>().GetTypeInfo(mType);
        //             mField = mTypeInfo.Fields[mFieldId];
        // 			Ldflda(Assembler, mTypeInfo, mField);
        // 		}
        // 	}
        // }

    }
}
