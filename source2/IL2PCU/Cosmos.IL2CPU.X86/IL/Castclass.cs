using System;
using System.Collections.Generic;
using System.IO;
using CPU = Cosmos.IL2CPU.X86;
using CPUx86 = Cosmos.IL2CPU.X86;
using System.Reflection;
using Cosmos.IL2CPU.X86;
using Indy.IL2CPU.Compiler;
using Cosmos.IL2CPU.ILOpCodes;
using Indy.IL2CPU;
namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Castclass )]
    public class Castclass : ILOp
    {
        public Castclass( Cosmos.IL2CPU.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            string xCurrentMethodLabel = CPU.MethodInfoLabelGenerator.GenerateLabelName( aMethod.MethodBase );
            OpType xType = ( OpType )aOpCode;
            string xTypeID = Label.FilterStringForIncorrectChars( xType.Value.AssemblyQualifiedName + "__TYPE_ID" );

            //mTypeId = GetService<IMetaDataInfoService>().GetTypeIdLabel( mCastAsType );
            // todo: throw an exception when the class does not support the cast!
            string mReturnNullLabel = xCurrentMethodLabel + "_ReturnNull";
            new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true };
            new CPUx86.Compare { DestinationReg = CPUx86.Registers.EAX, SourceValue = 0 };
            new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.Zero, DestinationLabel = mReturnNullLabel };
            new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX, DestinationIsIndirect = true };
            new CPUx86.Push { DestinationRef = ElementReference.New( xTypeID ), DestinationIsIndirect = true };
            Assembler.Stack.Push( new StackContents.Item( 4, typeof( object ) ) );
            Assembler.Stack.Push( new StackContents.Item( 4, typeof( object ) ) );
            System.Reflection.MethodBase xMethodIsInstance = Indy.IL2CPU.Compiler.ReflectionUtilities.GetMethodBase( typeof( VTablesImpl ), "IsInstance", "System.Int32", "System.Int32" );
            new Call( Assembler ).Execute( aMethod, new OpMethod( ILOpCode.Code.Call, 0, xMethodIsInstance ) );
            new Label( xCurrentMethodLabel + "_After_IsInstance_Call" );
            Assembler.Stack.Pop();
            new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
            new CPUx86.Compare { DestinationReg = CPUx86.Registers.EAX, SourceValue = 0 };
            new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.Equal, DestinationLabel = mReturnNullLabel };
            Jump_End(aMethod);

            new Label( mReturnNullLabel );
            new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 4 };
            string xAllocInfoLabelName = CPU.MethodInfoLabelGenerator.GenerateLabelName( GCImplementationRefs.AllocNewObjectRef );
#warning TODO: Emit new exceptions
            //new Newobj( Assembler ).Execute( aMethod, aOpCode );
   
            //Newobj.Assemble( Assembler,
            //                typeof( InvalidCastException ).GetConstructor( new Type[ 0 ] ),
            //                GetService<IMetaDataInfoService>().GetTypeIdLabel( typeof( InvalidCastException ) ),
            //                mThisLabel,
            //                mMethodInfo,
            //                mCurrentILOffset,
            //                mThisLabel + "_After_NewException",
            //                GetService<IMetaDataInfoService>().GetTypeInfo( typeof( InvalidCastException ) ),
            //                GetService<IMetaDataInfoService>().GetMethodInfo( typeof( InvalidCastException ).GetConstructor( new Type[ 0 ] ), false ),
            //                GetServiceProvider(),
            //                xAllocInfo.LabelName );
            new Label( xCurrentMethodLabel + "_After_NewException" );
            //Call.EmitExceptionLogic( Assembler, ( uint )mCurrentILOffset, mMethodInfo, mNextOpLabel, false, null );
        }


        // using System;
        // using System.Collections.Generic;
        // using System.IO;
        // 
        // 
        // using CPU = Cosmos.IL2CPU.X86;
        // using CPUx86 = Cosmos.IL2CPU.X86;
        // using System.Reflection;
        // using Cosmos.IL2CPU.X86;
        // using Indy.IL2CPU.Compiler;
        // 
        // namespace Indy.IL2CPU.IL.X86 {
        // 	[OpCode(OpCodeEnum.Castclass)]
        // 	public class Castclass: Op {
        // 		private string mTypeId;
        // 		private string mThisLabel;
        // 		private string mNextOpLabel;
        // 		private Type mCastAsType;
        // 		private int mCurrentILOffset;
        // 		private MethodInformation mMethodInfo;
        // 
        //         public static void ScanOp(ILReader aReader, MethodInformation aMethodInfo, SortedList<string, object> aMethodData, IServiceProvider aProvider)
        //         {
        //             Type xType = aReader.OperandValueType;
        //             if (xType == null)
        //             {
        //                 throw new Exception("Unable to determine Type!");
        //             }
        //             aProvider.GetService<IMetaDataInfoService>().GetTypeInfo(xType);
        //             Call.ScanOp(Engine.GetMethodBase(typeof(VTablesImpl),
        //                                              "IsInstance",
        //                                              "System.Int32",
        //                                              "System.Int32"), aProvider);
        //             Newobj.ScanOp(typeof(InvalidCastException).GetConstructor(new Type[0]), aProvider);
        //         }
        // 
        // 		public Castclass(ILReader aReader, MethodInformation aMethodInfo)
        // 			: base(aReader, aMethodInfo) {
        // 			Type xType = aReader.OperandValueType;
        // 			if (xType == null) {
        // 				throw new Exception("Unable to determine Type!");
        // 			}
        // 			mCastAsType = xType;
        // 			mThisLabel = GetInstructionLabel(aReader);
        // 			mNextOpLabel = GetInstructionLabel(aReader.NextPosition);
        // 			mCurrentILOffset = (int)aReader.Position;
        // 			mMethodInfo = aMethodInfo;
        // 		}
        // 
        // 		public override void DoAssemble() {
        //             mTypeId = GetService<IMetaDataInfoService>().GetTypeIdLabel(mCastAsType);
        // 			// todo: throw an exception when the class does not support the cast!
        // 			string mReturnNullLabel = mThisLabel + "_ReturnNull";
        //             new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true };
        //             new CPUx86.Compare { DestinationReg = CPUx86.Registers.EAX, SourceValue = 0 };
        //             new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.Zero, DestinationLabel = mReturnNullLabel };
        //             new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX, DestinationIsIndirect = true };
        // 			new CPUx86.Push{DestinationRef=ElementReference.New(mTypeId), DestinationIsIndirect=true};
        // 			Assembler.Stack.Push(new StackContent(4, typeof(object)));
        // 			Assembler.Stack.Push(new StackContent(4, typeof(object)));
        // 			MethodBase xMethodIsInstance = ReflectionUtilities.GetMethodBase(typeof(VTablesImpl), "IsInstance", "System.Int32", "System.Int32");
        // 			Op xOp = new Call(xMethodIsInstance, (uint)mCurrentILOffset, mMethodInfo.DebugMode, mThisLabel + "_After_IsInstance_Call");
        // 			xOp.Assembler = Assembler;
        //             xOp.SetServiceProvider(GetServiceProvider());
        // 			xOp.Assemble();
        // 		    new Label(mThisLabel + "_After_IsInstance_Call");
        // 			Assembler.Stack.Pop();
        //             new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
        //             new CPUx86.Compare { DestinationReg = CPUx86.Registers.EAX, SourceValue = 0 };
        //             new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.NotEqual, DestinationLabel = mNextOpLabel };
        // 			new Label(mReturnNullLabel);
        //             new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 4 };
        //             var xAllocInfo = GetService<IMetaDataInfoService>().GetMethodInfo(GCImplementationRefs.AllocNewObjectRef,
        //                                                                               false);
        //             Newobj.Assemble(Assembler, 
        //                             typeof(InvalidCastException).GetConstructor(new Type[0]), 
        //                             GetService<IMetaDataInfoService>().GetTypeIdLabel(typeof(InvalidCastException)), 
        //                             mThisLabel, 
        //                             mMethodInfo, 
        //                             mCurrentILOffset, 
        //                             mThisLabel + "_After_NewException",
        //                             GetService<IMetaDataInfoService>().GetTypeInfo(typeof(InvalidCastException)),
        //                             GetService<IMetaDataInfoService>().GetMethodInfo(typeof(InvalidCastException).GetConstructor(new Type[0]), false),
        //                             GetServiceProvider(),
        //                             xAllocInfo.LabelName);
        //             new Label(mThisLabel + "_After_NewException");
        // 			Call.EmitExceptionLogic(Assembler, (uint)mCurrentILOffset, mMethodInfo, mNextOpLabel, false, null);
        // 		}
        // 	}
        // }

    }
}
