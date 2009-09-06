using System;
using Cosmos.IL2CPU.ILOpCodes;
using CPUx86 = Cosmos.IL2CPU.X86;
using CPU = Cosmos.IL2CPU.X86;
using Indy.IL2CPU;

namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Box )]
    public class Box : ILOp
    {
        public Box( Cosmos.IL2CPU.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            OpType xType = ( OpType )aOpCode;

            uint xSize = SizeOfType( xType.Value );
            string xTypeID = Label.FilterStringForIncorrectChars( typeof( Array ).AssemblyQualifiedName + "__TYPE_ID" );
            new CPUx86.Push { DestinationValue = ( ObjectImpl.FieldDataOffset + xSize ) };
            new CPUx86.Call { DestinationLabel = CPU.MethodInfoLabelGenerator.GenerateLabelName( GCImplementationRefs.AllocNewObjectRef ) };
            new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
            new CPUx86.Move { DestinationReg = CPUx86.Registers.EBX, SourceRef = ElementReference.New( xTypeID ), SourceIsIndirect = true };
            new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, DestinationIsIndirect = true, SourceReg = CPUx86.Registers.EBX };
            new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 4, SourceValue = ( uint )InstanceTypeEnum.BoxedValueType, Size = 32 };
            new Comment(Assembler, "xSize is " + xSize );
            for( int i = 0; i < ( xSize / 4 ); i++ )
            {
                new CPUx86.Pop { DestinationReg = CPUx86.Registers.EDX };
                new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = ( ObjectImpl.FieldDataOffset + ( i * 4 ) ), SourceReg = CPUx86.Registers.EDX, Size = 32 };
            }
            new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
            Assembler.Stack.Pop();
            Assembler.Stack.Push( new StackContents.Item( 4, false, false, false ) );
        }


        // using System;
        // using System.Collections.Generic;
        // using System.Linq;
        // using Cosmos.IL2CPU.X86;
        // using CPU = Cosmos.IL2CPU.X86;
        // using CPUx86 = Cosmos.IL2CPU.X86;
        // using Indy.IL2CPU.Compiler;
        // 
        // namespace Indy.IL2CPU.IL.X86 {
        // 	[OpCode(OpCodeEnum.Box)]
        // 	public class Box: Op {
        //         private Type mType;
        // 		private string mTypeId;
        // 
        //         public static void ScanOp(ILReader aReader, MethodInformation aMethodInfo, SortedList<string, object> aMethodData,
        //             IServiceProvider aServiceProvider)
        //         {
        //             Type xTypeRef = aReader.OperandValueType as Type;
        //             if (xTypeRef == null)
        //             {
        //                 throw new Exception("Couldn't determine Type!");
        //             }
        //             aServiceProvider.GetService<IMetaDataInfoService>().GetTypeInfo(xTypeRef);
        //             aServiceProvider.GetService<IMetaDataInfoService>().GetMethodInfo(GCImplementationRefs.AllocNewObjectRef, false);
        //         }
        // 
        // 	    public Box(ILReader aReader, MethodInformation aMethodInfo)
        // 			: base(aReader, aMethodInfo) {
        // 			mType = aReader.OperandValueType;
        // 			if (mType == null) {
        // 				throw new Exception("Couldn't determine Type!");
        // 			}
        // 				
        // 		}
        // 
        // 		public override void DoAssemble() {
        //             var xTheSize = GetService<IMetaDataInfoService>().SizeOfType(mType);
        //             mTypeId = GetService<IMetaDataInfoService>().GetTypeIdLabel(mType);
        //             uint xSize = xTheSize;
        //             if (xTheSize % 4 != 0)
        //             {
        //                 xSize += 4 - (xTheSize % 4);
        // 			}
        //             new CPUx86.Push { DestinationValue = (ObjectImpl.FieldDataOffset + xSize) };
        //             new CPUx86.Call { DestinationLabel = CPU.MethodInfoLabelGenerator.GenerateLabelName(GCImplementationRefs.AllocNewObjectRef) };
        //             new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
        //             new CPUx86.Move { DestinationReg = CPUx86.Registers.EBX, SourceRef = ElementReference.New(mTypeId), SourceIsIndirect = true };
        //             new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, DestinationIsIndirect = true, SourceReg = CPUx86.Registers.EBX };
        //             new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 4, SourceValue = (uint)InstanceTypeEnum.BoxedValueType, Size=32 };
        //             new CPU.Comment("xSize is " + xSize);
        //             for (int i = 0; i < (xSize / 4); i++)
        //             {
        //                 new CPUx86.Pop { DestinationReg = CPUx86.Registers.EDX };
        //                 new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = (ObjectImpl.FieldDataOffset + (i * 4)), SourceReg = CPUx86.Registers.EDX, Size=32 };
        //             }
        //             new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
        //             Assembler.Stack.Pop();
        //             Assembler.Stack.Push(new StackContent(4, false, false, false));
        // 		} 
        // 	}
        // }

    }
}
