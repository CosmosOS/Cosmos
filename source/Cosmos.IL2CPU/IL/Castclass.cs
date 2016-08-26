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
    [Cosmos.IL2CPU.OpCode(ILOpCode.Code.Castclass)]
    public class Castclass : ILOp
    {
        public Castclass(Cosmos.Assembler.Assembler aAsmblr)
            : base(aAsmblr)
        {
        }

        public override void Execute(MethodInfo aMethod, ILOpCode aOpCode)
        {
            OpType xType = (OpType)aOpCode;
            string xTypeID = GetTypeIDLabel(xType.Value);
            string xCurrentMethodLabel = GetLabel(aMethod, aOpCode);
            string xReturnNullLabel = xCurrentMethodLabel + "_ReturnNull";
            string xNextPositionLabel = GetLabel(aMethod, aOpCode.NextPosition);

            XS.Set(XSRegisters.EAX, XSRegisters.ESP, sourceDisplacement: 4);
            XS.Compare(XSRegisters.EAX, 0);
            XS.Jump(CPU.ConditionalTestEnum.Zero, xReturnNullLabel);
            XS.Push(XSRegisters.EAX, isIndirect: true);
            XS.Push(xTypeID, isIndirect: true);
            MethodBase xMethodIsInstance = VTablesImplRefs.IsInstanceRef;
            Call.DoExecute(Assembler, aMethod, xMethodIsInstance, aOpCode, xCurrentMethodLabel, xCurrentMethodLabel + "_After_IsInstance_Call", DebugEnabled);
            XS.Label(xCurrentMethodLabel + "_After_IsInstance_Call");
            XS.Pop(XSRegisters.EAX);
            XS.Compare(XSRegisters.EAX, 0);
            XS.Jump(CPU.ConditionalTestEnum.Equal, xReturnNullLabel);
            XS.Jump(xNextPositionLabel);
            XS.Label(xReturnNullLabel);
            XS.Add(XSRegisters.ESP, 8);

            //string xAllocInfoLabelName = LabelName.Get( GCImplementationRefs.AllocNewObjectRef );
            // TODO: Emit new exceptions
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
            XS.Label(xCurrentMethodLabel + "_After_NewException");
            //Call.EmitExceptionLogic( Assembler, ( uint )mCurrentILOffset, mMethodInfo, mNextOpLabel, false, null );
        }
    }
}
