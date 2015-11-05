using System;
using Cosmos.Assembler;
using Cosmos.IL2CPU.ILOpCodes;
using CPUx86 = Cosmos.Assembler.x86;
using System.Reflection;
using System.Linq;
using SysReflection = System.Reflection;


namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Ldsfld )]
    public class Ldsfld : ILOp
    {
        public Ldsfld( Cosmos.Assembler.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {

            var xType = aMethod.MethodBase.DeclaringType;
            var xOpCode = ( ILOpCodes.OpField )aOpCode;
            SysReflection.FieldInfo xField = xOpCode.Value;

            // call cctor:
			var xCctor = (xField.DeclaringType.GetConstructors(BindingFlags.Static | BindingFlags.NonPublic) ?? new ConstructorInfo[0]).SingleOrDefault();
            if (xCctor != null)
            {
                new CPUx86.Call { DestinationLabel = LabelName.Get(xCctor) };
                ILOp.EmitExceptionLogic(Assembler, aMethod, aOpCode, true, null, ".AfterCCTorExceptionCheck");
                new Label(".AfterCCTorExceptionCheck");
            }

            //Assembler.Stack.Pop();
            //int aExtraOffset;// = 0;
            //bool xNeedsGC = xField.FieldType.IsClass && !xField.FieldType.IsValueType;
            var xSize = SizeOfType( xField.FieldType );
            //if( xNeedsGC )
            //{
            //    aExtraOffset = 12;
            //}

            string xDataName = DataMember.GetStaticFieldName(xField);
            if( xSize >= 4 )
            {
                for( int i = 1; i <= ( xSize / 4 ); i++ )
                {
                    //	Pop("eax");
                    //	Move(Assembler, "dword [" + mDataName + " + 0x" + (i * 4).ToString("X") + "]", "eax");
                    new CPUx86.Push { DestinationRef = Cosmos.Assembler.ElementReference.New( xDataName ), DestinationIsIndirect = true, DestinationDisplacement = ( int )( xSize - ( i * 4 ) ) };
                }
                switch( xSize % 4 )
                {
                    case 1:
                        {
                            new CPUx86.Mov { DestinationReg = CPUx86.Registers.EAX, SourceValue = 0 };
                            new CPUx86.Mov { DestinationReg = CPUx86.Registers.AL, SourceRef = Cosmos.Assembler.ElementReference.New( xDataName ), SourceIsIndirect = true };
                            new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
                            break;
                        }
                    case 2:
                        {
                            new CPUx86.Mov { DestinationReg = CPUx86.Registers.EAX, SourceValue = 0 };
                            new CPUx86.Mov { DestinationReg = CPUx86.Registers.AX, SourceRef = Cosmos.Assembler.ElementReference.New( xDataName ), SourceIsIndirect = true };
                            new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
                            break;
                        }
                    case 0:
                        {
                            break;
                        }
                    default:
                        //EmitNotImplementedException( Assembler, GetServiceProvider(), "Ldsfld: Remainder size " + ( xSize % 4 ) + " not supported!", mCurLabel, mMethodInformation, mCurOffset, mNextLabel );
                        throw new NotImplementedException();
                        //break;
                }
            }
            else
            {
                switch( xSize )
                {
                    case 1:
                        {
                            new CPUx86.Mov { DestinationReg = CPUx86.Registers.EAX, SourceValue = 0 };
                            new CPUx86.Mov { DestinationReg = CPUx86.Registers.AL, SourceRef = Cosmos.Assembler.ElementReference.New( xDataName ), SourceIsIndirect = true };
                            new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
                            break;
                        }
                    case 2:
                        {
                            new CPUx86.Mov { DestinationReg = CPUx86.Registers.EAX, SourceValue = 0 };
                            new CPUx86.Mov { DestinationReg = CPUx86.Registers.AX, SourceRef = Cosmos.Assembler.ElementReference.New( xDataName ), SourceIsIndirect = true };
                            new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
                            break;
                        }
                    case 0:
                        {
                            break;
                        }
                    default:
                        //EmitNotImplementedException( Assembler, GetServiceProvider(), "Ldsfld: Remainder size " + ( xSize % 4 ) + " not supported!", mCurLabel, mMethodInformation, mCurOffset, mNextLabel );
                        throw new NotImplementedException();
                        //break;
                }
            }
        }
    }
}
