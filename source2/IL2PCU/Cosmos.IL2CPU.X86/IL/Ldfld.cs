using System;
// using System.Collections.Generic;
// using System.IO;
// 
// using CPU = Indy.IL2CPU.Assembler;
// using System.Reflection;
// using Indy.IL2CPU.Compiler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;
namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Ldfld )]
    public class Ldfld : ILOp
    {
        public Ldfld( Cosmos.IL2CPU.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            var xType = aMethod.MethodBase.DeclaringType;
            var xOpCode = ( ILOpCodes.OpField )aOpCode;

            //             if (!xType.Fields.ContainsKey(mFieldId))
            //             {
            //                 Console.Write("");
            //                 xType = GetService<IMetaDataInfoService>().GetTypeInfo(mDeclaringType);
            //                 xType = GetService<IMetaDataInfoService>().GetTypeInfo(mDeclaringType);
            //                 xType = GetService<IMetaDataInfoService>().GetTypeInfo(mDeclaringType);
            //                 xType = GetService<IMetaDataInfoService>().GetTypeInfo(mDeclaringType);
            //             }
            //             mFieldInfo = xType.Fields[mFieldId];
            // 			Ldfld(Assembler, xType, mFieldId);
            Assembler.Stack.Pop();
            int aExtraOffset = 0;
            if( aType.NeedsGC )
            {
                aExtraOffset = 12;
            }
            new Comment( "Type = '" + aType.TypeDef.FullName + "', NeedsGC = " + aType.NeedsGC );
            new CPUx86.Pop { DestinationReg = CPUx86.Registers.ECX };
            new CPUx86.Add { DestinationReg = CPUx86.Registers.ECX, SourceValue = ( uint )( aField.Offset + aExtraOffset ) };
            if( aField.IsExternalField && aDerefExternalField )
            {
                new CPUx86.Move { DestinationReg = CPUx86.Registers.ECX, SourceReg = CPUx86.Registers.ECX, SourceIsIndirect = true };
            }
            for( int i = 1; i <= ( aField.Size / 4 ); i++ )
            {
                new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.ECX, SourceIsIndirect = true, SourceDisplacement = ( aField.Size - ( i * 4 ) ) };
                new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
            }
            switch( aField.Size % 4 )
            {
                case 1:
                    {
                        new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceValue = 0 };
                        new CPUx86.Move { DestinationReg = CPUx86.Registers.AL, SourceReg = CPUx86.Registers.ECX, SourceIsIndirect = true };
                        new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
                        break;
                    }
                case 2:
                    {
                        new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceValue = 0 };
                        new CPUx86.Move { DestinationReg = CPUx86.Registers.AX, SourceReg = CPUx86.Registers.ECX, SourceIsIndirect = true };
                        new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
                        break;
                    }

                case 3: //For Release
                    {
                        new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceValue = 0 };
                        new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.ECX, SourceIsIndirect = true };
                        new CPUx86.ShiftRight { DestinationReg = CPUx86.Registers.EAX, SourceValue = 8 };
                        new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
                        break;
                    }
                case 0:
                    {
                        break;
                    }
                default:
                    throw new Exception( "Remainder size " + aField.FieldType.ToString() + ( aField.Size ) + " not supported!" );
            }
            if( aAddGCCode && aField.NeedsGC )
            {
                new CPUx86.Push { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true };
                new CPUx86.Call { DestinationLabel = MethodInfoLabelGenerator.GenerateLabelName( GCImplementationRefs.IncRefCountRef ) };
            }
            Assembler.Stack.Push( new StackContents.Item( aField.Size, aField.FieldType ) );
        }

        // 	public class Ldfld: Op {
        //         private Type mDeclaringType;
        // 		private TypeInformation.Field mFieldInfo;
        // 		private readonly TypeInformation mTypeInfo;
        //         private string mFieldId;
        // 
        // 		public Ldfld(ILReader aReader, MethodInformation aMethodInfo)
        // 			: base(aReader, aMethodInfo) {
        // 			FieldInfo xField = aReader.OperandValueField;
        // 			if (xField == null) {
        // 					throw new Exception("Field not found!");
        // 			}
        // 			mFieldId = xField.GetFullName();
        //             mDeclaringType = xField.DeclaringType;
        // 		}

    }
}
