using System;
using Cosmos.Compiler.Assembler;
using Cosmos.IL2CPU.ILOpCodes;
using CPUx86 = Cosmos.Compiler.Assembler.X86;
using System.Reflection;
using System.Linq;

namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Ldsfld )]
    public class Ldsfld : ILOp
    {
        public Ldsfld( Cosmos.Compiler.Assembler.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {

            var xType = aMethod.MethodBase.DeclaringType;
            var xOpCode = ( ILOpCodes.OpField )aOpCode;
            System.Reflection.FieldInfo xField = xOpCode.Value;

            // call cctor:
            var xCctor = (xField.DeclaringType.GetConstructors(BindingFlags.Static | BindingFlags.NonPublic) ?? new ConstructorInfo[0]).SingleOrDefault();
            if (xCctor != null)
            {
                new CPUx86.Call { DestinationLabel = MethodInfoLabelGenerator.GenerateLabelName(xCctor) };
                ILOp.EmitExceptionLogic(Assembler, aMethod, aOpCode, true, null);
            }

            //Assembler.Stack.Pop();
            int aExtraOffset;// = 0;
            bool xNeedsGC = xField.FieldType.IsClass && !xField.FieldType.IsValueType;
            uint xSize = SizeOfType( xField.FieldType );
            if( xNeedsGC )
            {
                aExtraOffset = 12;
            }


            string xDataName = DataMember.GetStaticFieldName(xField);
            if( xSize >= 4 )
            {
                for( int i = 1; i <= ( xSize / 4 ); i++ )
                {
                    //	Pop("eax");
                    //	Move(Assembler, "dword [" + mDataName + " + 0x" + (i * 4).ToString("X") + "]", "eax");
                    new CPUx86.Push { DestinationRef = ElementReference.New( xDataName ), DestinationIsIndirect = true, DestinationDisplacement = ( int )( xSize - ( i * 4 ) ) };
                }
                switch( xSize % 4 )
                {
                    case 1:
                        {
                            new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceValue = 0 };
                            new CPUx86.Move { DestinationReg = CPUx86.Registers.AL, SourceRef = ElementReference.New( xDataName ), SourceIsIndirect = true };
                            new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
                            break;
                        }
                    case 2:
                        {
                            new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceValue = 0 };
                            new CPUx86.Move { DestinationReg = CPUx86.Registers.AX, SourceRef = ElementReference.New( xDataName ), SourceIsIndirect = true };
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
                            new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceValue = 0 };
                            new CPUx86.Move { DestinationReg = CPUx86.Registers.AL, SourceRef = ElementReference.New( xDataName ), SourceIsIndirect = true };
                            new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
                            break;
                        }
                    case 2:
                        {
                            new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceValue = 0 };
                            new CPUx86.Move { DestinationReg = CPUx86.Registers.AX, SourceRef = ElementReference.New( xDataName ), SourceIsIndirect = true };
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

            Assembler.Stack.Push( new StackContents.Item( ( int )xSize, null ) );

            if( xNeedsGC )
            {
                new Dup( Assembler ).Execute( aMethod, aOpCode );

                new CPUx86.Call { DestinationLabel = MethodInfoLabelGenerator.GenerateLabelName( GCImplementationRefs.IncRefCountRef ) };
                Assembler.Stack.Pop();
            }
        }


        // using System;
        // using System.Collections.Generic;
        // using Cosmos.IL2CPU.X86;
        // 
        // 
        // using CPUx86 = Cosmos.Compiler.Assembler.X86;
        // using System.Reflection;
        // using Cosmos.IL2CPU.Compiler;
        // 
        // namespace Cosmos.IL2CPU.IL.X86 {
        // 	[OpCode(OpCodeEnum.Ldsfld)]
        // 	public class Ldsfld: Op {
        // 		private string mDataName;
        // 		private bool mNeedsGC;
        //         private string mNextLabel;
        // 	    private string mCurLabel;
        // 	    private uint mCurOffset;
        // 	    private MethodInformation mMethodInformation;
        // 
        //         //public static void ScanOp(ILReader aReader, MethodInformation aMethodInfo, SortedList<string, object> aMethodData) {
        //         //    FieldInfo xField = aReader.OperandValueField;
        //         //    Engine.QueueStaticField(xField);
        //         //}
        // 
        // 		public Ldsfld(ILReader aReader, MethodInformation aMethodInfo)
        // 			: base(aReader, aMethodInfo) {
        // 			mField = aReader.OperandValueField;
        //             // todo: improve, strings need gc?
        // 			mNeedsGC = !mField.FieldType.IsValueType;
        //              mMethodInformation = aMethodInfo;
        // 		    mCurOffset = aReader.Position;
        // 		    mCurLabel = IL.Op.GetInstructionLabel(aReader);
        //             mNextLabel = IL.Op.GetInstructionLabel(aReader.NextPosition);
        // 		}
        //         private FieldInfo mField;
        // 		public override void DoAssemble() {
        //             var xSize = GetService<IMetaDataInfoService>().SizeOfType(mField.FieldType);
        //             mDataName = GetService<IMetaDataInfoService>().GetStaticFieldLabel(mField);
        // 		    if (xSize >= 4) {
        // 				for (int i = 1; i <= (xSize / 4); i++) {
        // 					//	Pop("eax");
        // 					//	Move(Assembler, "dword [" + mDataName + " + 0x" + (i * 4).ToString("X") + "]", "eax");
        //                     new CPUx86.Push { DestinationRef = ElementReference.New(mDataName), DestinationIsIndirect = true, DestinationDisplacement = (int)(xSize - (i * 4)) };
        // 				}
        // 				switch (xSize % 4) {
        // 					case 1: {
        //                             new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceValue = 0 };
        //                             new CPUx86.Move { DestinationReg = CPUx86.Registers.AL, SourceRef = ElementReference.New(mDataName), SourceIsIndirect = true };
        //                             new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
        // 							break;
        // 						}
        // 					case 2: {
        //                             new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceValue = 0 };
        //                             new CPUx86.Move { DestinationReg = CPUx86.Registers.AX, SourceRef = ElementReference.New(mDataName), SourceIsIndirect = true };
        // 							new CPUx86.Push{DestinationReg=CPUx86.Registers.EAX};
        // 							break;
        // 						}
        // 					case 0: {
        // 							break;
        // 						}
        // 					default:
        //                         EmitNotImplementedException(Assembler, GetServiceProvider(), "Ldsfld: Remainder size " + (xSize % 4) + " not supported!", mCurLabel, mMethodInformation, mCurOffset, mNextLabel);
        //                         break;
        // 				}
        // 			} else {
        // 				switch (xSize) {
        // 					case 1: {
        //                             new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceValue = 0 };
        //                             new CPUx86.Move { DestinationReg = CPUx86.Registers.AL, SourceRef = ElementReference.New(mDataName), SourceIsIndirect = true };
        //                             new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
        // 							break;
        // 						}
        // 					case 2: {
        //                             new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceValue = 0 };
        //                             new CPUx86.Move { DestinationReg = CPUx86.Registers.AX, SourceRef = ElementReference.New(mDataName), SourceIsIndirect = true };
        // 							new CPUx86.Push{DestinationReg=CPUx86.Registers.EAX};
        // 							break;
        // 						}
        // 					case 0: {
        // 							break;
        // 						}
        // 					default:
        //                         EmitNotImplementedException(Assembler, GetServiceProvider(), "Ldsfld: Remainder size " + (xSize % 4) + " not supported!", mCurLabel, mMethodInformation, mCurOffset, mNextLabel);
        //                         break;
        // 				}
        // 			}
        // 			Assembler.Stack.Push(new StackContent((int)xSize, null));
        // 			if (mNeedsGC) {
        // 				new Dup(null, null) {
        // 					Assembler = this.Assembler
        // 				}.Assemble();
        // 				new CPUx86.Call { DestinationLabel = MethodInfoLabelGenerator.GenerateLabelName(GCImplementationRefs.IncRefCountRef) };
        // 				Assembler.Stack.Pop();
        // 			}
        // 		}
        // 	}
        // }

    }
}
