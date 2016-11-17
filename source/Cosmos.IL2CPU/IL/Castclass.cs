using System.Reflection;

using Cosmos.IL2CPU.ILOpCodes;
using CPUx86 = Cosmos.Assembler.x86;
using XSharp.Compiler;
using static XSharp.Compiler.XSRegisters;

namespace Cosmos.IL2CPU.X86.IL
{
    [OpCode(ILOpCode.Code.Castclass)]
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
            string xAfterIsInstanceCallLabel = xCurrentMethodLabel + "_After_IsInstance_Call";
            string xReturnNullLabel = xCurrentMethodLabel + "_ReturnNull";
            string xNextPositionLabel = GetLabel(aMethod, aOpCode.NextPosition);

            XS.Set(EAX, ESP, sourceDisplacement: 4);

            XS.Compare(EAX, 0);
            XS.Jump(CPUx86.ConditionalTestEnum.Zero, xReturnNullLabel);
            XS.Push(EAX, isIndirect: true);
            XS.Push(xTypeID, isIndirect: true);
            
            MethodBase xMethodIsInstance = VTablesImplRefs.IsInstanceRef;
            
            Call.DoExecute(Assembler, aMethod, xMethodIsInstance, aOpCode, xCurrentMethodLabel, xAfterIsInstanceCallLabel, DebugEnabled);
            
            XS.Label(xAfterIsInstanceCallLabel);
            
            XS.Pop(EAX);

            XS.Compare(EAX, 0);
            XS.Jump(CPUx86.ConditionalTestEnum.Equal, xReturnNullLabel);
            
            XS.Jump(xNextPositionLabel);
            XS.Label(xReturnNullLabel);
            XS.Add(ESP, 8);

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
