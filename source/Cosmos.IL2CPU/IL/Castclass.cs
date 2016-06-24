using System;
using System.Collections.Generic;
using System.IO;
using CPU = Cosmos.Assembler.x86;
using CPUx86 = Cosmos.Assembler.x86;
using System.Reflection;
using Cosmos.IL2CPU.X86;
using Cosmos.IL2CPU.ILOpCodes;
using Cosmos.Assembler;
using XSharp.Compiler;
using SysReflection = System.Reflection;

namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Castclass )]
    public class Castclass : ILOp
    {
        public Castclass( Cosmos.Assembler.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            string xCurrentMethodLabel = GetLabel( aMethod, aOpCode );
            OpType xType = ( OpType )aOpCode;
            string xTypeID = GetTypeIDLabel(xType.Value);

            //mTypeId = GetService<IMetaDataInfoService>().GetTypeIdLabel( mCastAsType );
            // todo: throw an exception when the class does not support the cast!
            string mReturnNullLabel = xCurrentMethodLabel + "_ReturnNull";
            XS.Set(XSRegisters.EAX, XSRegisters.ESP, sourceIsIndirect: true);
            XS.Compare(XSRegisters.EAX, 0);
            XS.Jump(CPU.ConditionalTestEnum.Zero, mReturnNullLabel);

            XS.Set(XSRegisters.EAX, XSRegisters.EAX, sourceIsIndirect: true);
            XS.Push(XSRegisters.EAX, isIndirect: true);
            XS.Push(xTypeID, isIndirect: true);
            MethodBase xMethodIsInstance = VTablesImplRefs.IsInstanceRef;
            // new OpMethod( ILOpCode.Code.Call, 0, 0, xMethodIsInstance, aOpCode.CurrentExceptionHandler ) );
            Call.DoExecute(Assembler, aMethod, xMethodIsInstance, aOpCode, xCurrentMethodLabel, xCurrentMethodLabel + "_After_IsInstance_Call", DebugEnabled);
            XS.Label(xCurrentMethodLabel + "_After_IsInstance_Call" );
            XS.Pop(XSRegisters.EAX);
            XS.Compare(XSRegisters.EAX, 0);
            XS.Jump(CPU.ConditionalTestEnum.Equal, mReturnNullLabel);
            new CPU.Jump { DestinationLabel = GetLabel(aMethod, aOpCode.NextPosition) };

            XS.Label(mReturnNullLabel );
            XS.Add(XSRegisters.ESP, 4);
            //string xAllocInfoLabelName = LabelName.Get( GCImplementationRefs.AllocNewObjectRef );
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
            XS.Label(xCurrentMethodLabel + "_After_NewException" );
            //Call.EmitExceptionLogic( Assembler, ( uint )mCurrentILOffset, mMethodInfo, mNextOpLabel, false, null );
        }


        // using System;
        // using System.Collections.Generic;
        // using System.IO;
        //
        //
        // using CPU = Cosmos.Assembler.x86;
        // using CPUx86 = Cosmos.Assembler.x86;
        // using System.Reflection;
        // using Cosmos.IL2CPU.X86;
        // using Cosmos.IL2CPU.Compiler;
        //
        // namespace Cosmos.IL2CPU.IL.X86 {
        // 	[Cosmos.Assembler.OpCode(OpCodeEnum.Castclass)]
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
        //             XS.Mov(XSRegisters.EAX, XSRegisters.ESP, sourceIsIndirect: true);
        //             XS.Compare(XSRegisters.EAX, 0);
        //             XS.Jump(ConditionalTestEnum.Zero, mReturnNullLabel);
        //             XS.Push(XSRegisters.EAX, isIndirect: true);
        // 			XS.Push(Cosmos.Assembler.ElementReference.New(mTypeId), isIndirect: true);
        // 			Assembler.Stack.Push(new StackContent(4, typeof(object)));
        // 			Assembler.Stack.Push(new StackContent(4, typeof(object)));
        // 			MethodBase xMethodIsInstance = ReflectionUtilities.GetMethodBase(typeof(VTablesImpl), "IsInstance", "System.Int32", "System.Int32");
        // 			Op xOp = new Call(xMethodIsInstance, (uint)mCurrentILOffset, mMethodInfo.DebugMode, mThisLabel + "_After_IsInstance_Call");
        // 			xOp.Assembler = Assembler;
        //             xOp.SetServiceProvider(GetServiceProvider());
        // 			xOp.Assemble();
        // 		    XS.Label(mThisLabel + "_After_IsInstance_Call");
        // 			Assembler.Stack.Pop();
        //             XS.Pop(XSRegisters.EAX);
        //             XS.Compare(XSRegisters.EAX, 0);
        //             XS.Jump(ConditionalTestEnum.NotEqual, mNextOpLabel);
        // 			XS.Label(mReturnNullLabel);
        //             XS.Add(XSRegisters.ESP, 4);
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
        //             XS.Label(mThisLabel + "_After_NewException");
        // 			Call.EmitExceptionLogic(Assembler, (uint)mCurrentILOffset, mMethodInfo, mNextOpLabel, false, null);
        // 		}
        // 	}
        // }

    }
}
