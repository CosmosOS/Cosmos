using System;
using CPUx86 = Cosmos.Assembler.x86;
namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Initobj )]
    public class Initobj : ILOp
    {
        public Initobj( Cosmos.Assembler.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            
            uint mObjSize = 0;

            Type mType = (( Cosmos.IL2CPU.ILOpCodes.OpType )aOpCode).Value;
            mObjSize = SizeOfType( mType );

            new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
            for( int i = 0; i < ( mObjSize / 4 ); i++ )
            {
                new CPUx86.Mov { DestinationReg = CPUx86.Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = i * 4, SourceValue = 0, Size = 32 };
            }
            switch( mObjSize % 4 )
            {
                case 1:
                    {
                        new CPUx86.Mov { DestinationReg = CPUx86.Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = ( int )( ( mObjSize / 4 ) * 4 ), SourceValue = 0, Size = 8 };
                        break;
                    }
                case 2:
                    {
                        new CPUx86.Mov { DestinationReg = CPUx86.Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = ( int )( ( mObjSize / 4 ) * 4 ), SourceValue = 0, Size = 16 };
                        break;
                    }
                case 3:
                    {
                        new CPUx86.Mov { DestinationReg = CPUx86.Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = ( int )( ( mObjSize / 4 ) * 4 ), SourceValue = 0, Size = 8 };
                        new CPUx86.Mov { DestinationReg = CPUx86.Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = ( int )( ( ( mObjSize / 4 ) * 4 ) + 1 ), SourceValue = 0, Size = 16 };
                        break;
                    }
                case 0:
                    break;
                default:
                    {
                        throw new NotImplementedException( "Remainder size " + mObjSize % 4 + " not supported yet! (Type = '" + mType.FullName + "')" );
                    }
            }
        }


        // using System;
        // using System.Collections.Generic;
        // using CPUx86 = Cosmos.Assembler.x86;
        // using Cosmos.IL2CPU.Compiler;
        // 
        // namespace Cosmos.IL2CPU.IL.X86 {
        // 	[Cosmos.Assembler.OpCode(OpCodeEnum.Initobj)]
        // 	public class Initobj: Op {
        // 		private uint mObjSize;
        // 
        //         //public static void ScanOp(ILReader aReader, MethodInformation aMethodInfo, SortedList<string, object> aMethodData) {
        //         //    Type xTypeRef = aReader.OperandValueType;
        //         //    if (xTypeRef == null)
        //         //    {
        //         //        throw new Exception("Type not found!");
        //         //    }
        //         //    Engine.RegisterType(xTypeRef);
        //         //}
        //         private Type mType;
        // 
        // 		public Initobj(ILReader aReader, MethodInformation aMethodInfo)
        // 			: base(aReader, aMethodInfo) {
        // 			mType = aReader.OperandValueType;
        //             if (mType == null)
        //             {
        // 				throw new Exception("Type not found!");
        // 			}
        // 			mObjSize = 0;
        // 		}
        // 
        // 		public override void DoAssemble() {
        //             if (mType.IsValueType)
        //             {
        //                 GetService<IMetaDataInfoService>().GetTypeFieldInfo(mType, out mObjSize);
        //             }
        // 			Assembler.Stack.Pop();
        //             new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
        // 			for (int i = 0; i < (mObjSize / 4); i++) {
        //                 new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = i * 4, SourceValue = 0, Size=32 };
        // 			}
        // 			switch (mObjSize % 4) {
        // 				case 1: {
        //                         new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = (int)((mObjSize / 4) * 4), SourceValue = 0, Size = 8 };
        //                         break;
        // 					}
        // 				case 2: {
        //                         new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = (int)((mObjSize / 4) * 4), SourceValue = 0, Size = 16 };
        //                         break;
        // 					}
        //                 case 3:
        //                     {
        //                         new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = (int)((mObjSize / 4) * 4), SourceValue = 0, Size = 8 };
        //                         new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = (int)(((mObjSize / 4) * 4)+1), SourceValue = 0, Size = 16 };
        //                         break;
        //                     }
        // 				case 0:
        // 					break;
        // 				default: {
        // 						throw new Exception("Remainder size " + mObjSize % 4 + " not supported yet! (Type = '" + mType.FullName + "')");
        // 					}
        // 			}
        // 		}
        // 	}
        // }

    }
}
