using System;
using CPUx86 = Cosmos.Assembler.x86;
using CPU = Cosmos.Assembler.x86;
using Cosmos.Assembler;
using System.Reflection;
using Cosmos.IL2CPU.IL.CustomImplementations.System;

namespace Cosmos.IL2CPU.X86.IL
{
    /// <summary>
    /// Pushes an object reference to a new zero-based, one-dimensional array whose elements are of a specific type onto the evaluation stack.
    /// </summary>
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Newarr )]
    public class Newarr : ILOp
    {
        public Newarr( Cosmos.Assembler.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            Cosmos.IL2CPU.ILOpCodes.OpType xType = ( Cosmos.IL2CPU.ILOpCodes.OpType )aOpCode;

            uint xSize = SizeOfType( xType.Value );

			//TODO cache it to reduce calculation
            string xTypeID = GetTypeIDLabel(typeof(Array));
            MethodBase xCtor = typeof( Array ).GetConstructors( BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance )[ 0 ];
            string xCtorName = LabelName.Get( xCtor );

            new Comment( Assembler, "Element Size = " + xSize );
            // element count is on the stack
            new CPUx86.Pop { DestinationReg = CPUx86.Registers.ESI };
            new CPUx86.Push { DestinationReg = CPUx86.Registers.ESI };
            //Assembler.StackSizes.Push(xElementCountSize);
            new CPUx86.Push { DestinationValue = xSize };
            new Mul( Assembler ).Execute( aMethod, aOpCode );
            // the total items size is now on the stack
            new CPUx86.Push { DestinationValue = ( ObjectImpl.FieldDataOffset + 4 ) };
            new Add( Assembler ).Execute( aMethod, aOpCode );
            // the total array size is now on the stack.
            new CPUx86.Call { DestinationLabel = LabelName.Get( GCImplementationRefs.AllocNewObjectRef ) };
            new CPUx86.Push { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true };
            new CPUx86.Push { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true };

            new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
            new CPUx86.Mov { DestinationReg = CPUx86.Registers.EBX, SourceRef = Cosmos.Assembler.ElementReference.New( xTypeID ), SourceIsIndirect = true };
            new CPUx86.Mov { DestinationReg = CPUx86.Registers.EAX, DestinationIsIndirect = true, SourceReg = CPUx86.Registers.EBX };
            new CPUx86.Add { DestinationReg = CPUx86.Registers.EAX, SourceValue = 4 };
            new CPUx86.Mov { DestinationReg = CPUx86.Registers.EAX, DestinationIsIndirect = true, SourceValue = ( uint )InstanceTypeEnum.Array, Size = 32 };
            new CPUx86.Add { DestinationReg = CPUx86.Registers.EAX, SourceValue = 4 };
            new CPUx86.Mov { DestinationReg = CPUx86.Registers.EAX, DestinationIsIndirect = true, SourceReg = CPUx86.Registers.ESI, Size = 32 };
            new CPUx86.Add { DestinationReg = CPUx86.Registers.EAX, SourceValue = 4 };
            new CPUx86.Mov { DestinationReg = CPUx86.Registers.EAX, DestinationIsIndirect = true, SourceValue = ( uint )xSize, Size = 32 };
            new CPUx86.Call { DestinationLabel = xCtorName };
        }
    }
}