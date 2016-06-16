using System;
using Cosmos.IL2CPU.ILOpCodes;
using CPUx86 = Cosmos.Assembler.x86;
using CPU = Cosmos.Assembler.x86;
using Cosmos.Assembler;
using XSharp.Compiler;
using SysReflection = System.Reflection;

namespace Cosmos.IL2CPU.X86.IL
{
    /// <summary>
    /// Tests whether an object reference (type O) is an instance of a particular class.
    /// </summary>
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Isinst )]
    public class Isinst : ILOp
    {
        public Isinst( Cosmos.Assembler.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            OpType xType = ( OpType )aOpCode;
            string xTypeID = GetTypeIDLabel(xType.Value);
            string mReturnNullLabel = GetLabel( aMethod, aOpCode ) + "_ReturnNull";

            XS.Set(XSRegisters.EAX, XSRegisters.ESP, sourceIsIndirect: true);

            XS.Compare(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX), 0);
            new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.Zero, DestinationLabel = mReturnNullLabel };

            // EAX contains a memory handle now. Lets convert it to a pointer
            XS.Set(XSRegisters.EAX, XSRegisters.EAX, sourceIsIndirect: true);

            //XS.Mov(XSRegisters.EAX, XSRegisters.EAX, sourceIsIndirect: true);
            new CPUx86.Push { DestinationReg = CPUx86.RegistersEnum.EAX, DestinationIsIndirect = true };
            new CPUx86.Push { DestinationRef = ElementReference.New( xTypeID ), DestinationIsIndirect = true };

            SysReflection.MethodBase xMethodIsInstance = ReflectionUtilities.GetMethodBase( typeof( VTablesImpl ), "IsInstance", "System.UInt32", "System.UInt32" );
//, new OpMethod( ILOpCode.Code.Call, aOpCode.Position, aOpCode.NextPosition, xMethodIsInstance, aOpCode.CurrentExceptionHandler));
            Call.DoExecute(Assembler, aMethod, xMethodIsInstance, aOpCode, GetLabel(aMethod, aOpCode), GetLabel(aMethod, aOpCode) + "_After_IsInstance_Call", DebugEnabled);

            new Label( GetLabel( aMethod, aOpCode ) + "_After_IsInstance_Call" );
            XS.Pop(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX));
            XS.Compare(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX), 0);
            new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.Equal, DestinationLabel = mReturnNullLabel };
            // push nothing now, as we should return the object instance pointer.
            new CPUx86.Jump { DestinationLabel = GetLabel(aMethod, aOpCode.NextPosition) };
            XS.Label(mReturnNullLabel );
            XS.Add(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ESP), 4);
            new CPUx86.Push { DestinationValue = 0 };
        }
    }
}
