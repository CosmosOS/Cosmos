using System;
using CPUx86 = Cosmos.IL2CPU.X86;
using Indy.IL2CPU;
namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Stfld )]
    public class Stfld : ILOp
    {
        public Stfld( Cosmos.IL2CPU.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            var xType = aMethod.MethodBase.DeclaringType;
            var xOpCode = ( ILOpCodes.OpField )aOpCode;
            System.Reflection.FieldInfo xField = xOpCode.Value;


            int aExtraOffset = 0;
            bool xNeedsGC = xField.FieldType.IsClass && !xField.FieldType.IsValueType;
            uint xSize = SizeOfType( xField.FieldType );
            if( xNeedsGC )
            {
                aExtraOffset = 12;
            }
            new Comment( Assembler, "Type = '" + xField.FieldType.FullName + "', NeedsGC = " + xNeedsGC );

            uint xOffset = 0;

            var xFields = xField.DeclaringType.GetFields();

            foreach( System.Reflection.FieldInfo xInfo in xFields )
            {
                if( xInfo == xField )
                    break;

                xOffset += SizeOfType( xInfo.FieldType );
            }

            Assembler.Stack.Pop();
            
            uint xRoundedSize = Align( xSize, 4);

            if( xNeedsGC )
            {
                new CPUx86.Push { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 4 };
                //Ldfld(aAssembler, aType, aField, false);
                new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
                new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = (int)( xOffset + aExtraOffset ) };
                new CPUx86.Call { DestinationLabel = MethodInfoLabelGenerator.GenerateLabelName( GCImplementationRefs.DecRefCountRef ) };
            }
            new CPUx86.Move { DestinationReg = CPUx86.Registers.ECX, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true, SourceDisplacement = (int)xRoundedSize };
            new CPUx86.Add
            {
                DestinationReg = CPUx86.Registers.ECX,
                SourceValue = ( uint )( xOffset + aExtraOffset )
            };
            for( int i = 0; i < ( xSize / 4 ); i++ )
            {
                new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
                new CPUx86.Move { DestinationReg = CPUx86.Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = i * 4, SourceReg = CPUx86.Registers.EAX };
            }
            switch( xSize % 4 )
            {
                case 1:
                    {
                        new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
                        new CPUx86.Move { DestinationReg = CPUx86.Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = (int)( ( xSize / 4 ) * 4 ), SourceReg = CPUx86.Registers.AL };
                        break;
                    }
                case 2:
                    {
                        new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
                        new CPUx86.Move { DestinationReg = CPUx86.Registers.ECX, DestinationIsIndirect = true, DestinationDisplacement = ( int )( ( xSize / 4 ) * 4 ), SourceReg = CPUx86.Registers.AX };
                        break;
                    }

                case 3: //TODO 
                    break;
                case 0:
                    {
                        break;
                    }
                default:
                    throw new Exception( "Remainder size " + ( xSize % 4 ) + " not supported!" );
            }
            if( xNeedsGC )
            {
                new CPUx86.Push { DestinationReg = CPUx86.Registers.ECX };
                new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
                new CPUx86.Call { DestinationLabel = MethodInfoLabelGenerator.GenerateLabelName( GCImplementationRefs.DecRefCountRef ) };
                new CPUx86.Call { DestinationLabel = MethodInfoLabelGenerator.GenerateLabelName( GCImplementationRefs.DecRefCountRef ) };
            }
            new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 4 };
            Assembler.Stack.Pop();
        }


        // using System;
        // using System.Collections;
        // using System.Collections.Generic;		 
        // using System.Linq;
        // 
        // 
        // using CPU = Cosmos.IL2CPU.X86;
        // using System.Reflection;
        // using Indy.IL2CPU.Compiler;
        // 
        // namespace Indy.IL2CPU.IL.X86 {
        // 	[OpCode(OpCodeEnum.Stfld)]
        // 	public class Stfld: Op {
        // 		private TypeInformation.Field mField;
        // 		private TypeInformation mType;
        //         private Type mDeclaringType;
        // 
        //         //public static void ScanOp(ILReader aReader, MethodInformation aMethodInfo, SortedList<string, object> aMethodData) {
        //         //    FieldInfo xField = aReader.OperandValueField;
        //         //    if (xField == null)
        //         //    {
        //         //        throw new Exception("Field not found!");
        //         //    }
        //         //    Engine.RegisterType(xField.FieldType);
        //         //}
        // 
        // 		public Stfld(ILReader aReader, MethodInformation aMethodInfo)
        // 			: base(aReader, aMethodInfo) {
        // 			if (aReader == null) {
        // 				throw new ArgumentNullException("aReader");
        // 			}
        // 			if (aMethodInfo == null) {
        // 				throw new ArgumentNullException("aMethodInfo");
        // 			}
        // 			FieldInfo xField = aReader.OperandValueField;
        // 			if (xField == null) {
        // 				throw new Exception("Field not found!");
        // 			}
        // 			mFieldId = xField.GetFullName();
        //             mDeclaringType = xField.DeclaringType;
        // 			
        // 		}
        // 
        //         private string mFieldId;
        // 
        // 		public override void DoAssemble() {
        //             mType = GetService<IMetaDataInfoService>().GetTypeInfo(mDeclaringType);
        //             if (!mType.Fields.ContainsKey(mFieldId))
        //             {
        //                 Console.Write("");
        //                 mType = GetService<IMetaDataInfoService>().GetTypeInfo(mDeclaringType);
        //                 mType = GetService<IMetaDataInfoService>().GetTypeInfo(mDeclaringType);
        //                 mType = GetService<IMetaDataInfoService>().GetTypeInfo(mDeclaringType);
        //                 mType = GetService<IMetaDataInfoService>().GetTypeInfo(mDeclaringType);
        //                 throw new Exception("Field not found!");
        //             }
        //             mField = mType.Fields[mFieldId];
        // 			Stfld(Assembler, mType, mField);
        // 		}
        // 	}
        // }

    }
}
