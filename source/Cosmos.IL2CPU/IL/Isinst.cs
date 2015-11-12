using System;
using Cosmos.IL2CPU.ILOpCodes;
using CPUx86 = Cosmos.Assembler.x86;
using CPU = Cosmos.Assembler.x86;
using Cosmos.Assembler;
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

            new CPUx86.Mov { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true };

            new CPUx86.Compare { DestinationReg = CPUx86.Registers.EAX, SourceValue = 0 };
            new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.Zero, DestinationLabel = mReturnNullLabel };

            // EAX contains a memory handle now. Lets convert it to a pointer
            new CPUx86.Mov { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.EAX, SourceIsIndirect = true };

            //new CPUx86.Mov {DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.EAX, SourceIsIndirect = true};
            new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX, DestinationIsIndirect = true };
            new CPUx86.Push { DestinationRef = Cosmos.Assembler.ElementReference.New( xTypeID ), DestinationIsIndirect = true };

            SysReflection.MethodBase xMethodIsInstance = ReflectionUtilities.GetMethodBase( typeof( VTablesImpl ), "IsInstance", "System.UInt32", "System.UInt32" );
//, new OpMethod( ILOpCode.Code.Call, aOpCode.Position, aOpCode.NextPosition, xMethodIsInstance, aOpCode.CurrentExceptionHandler));
            Call.DoExecute(Assembler, aMethod, xMethodIsInstance, aOpCode, GetLabel(aMethod, aOpCode), GetLabel(aMethod, aOpCode) + "_After_IsInstance_Call", DebugEnabled);

            new Label( GetLabel( aMethod, aOpCode ) + "_After_IsInstance_Call" );
            new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
            new CPUx86.Compare { DestinationReg = CPUx86.Registers.EAX, SourceValue = 0 };
            new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.Equal, DestinationLabel = mReturnNullLabel };
            // push nothing now, as we should return the object instance pointer.
            new CPUx86.Jump { DestinationLabel = GetLabel(aMethod, aOpCode.NextPosition) };
            new Label( mReturnNullLabel );
            new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 4 };
            new CPUx86.Push { DestinationValue = 0 };
        }
    }
}
