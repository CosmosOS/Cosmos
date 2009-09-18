using System;
using Cosmos.IL2CPU.ILOpCodes;
using CPUx86 = Cosmos.IL2CPU.X86;
using CPU = Cosmos.IL2CPU.X86;
using Indy.IL2CPU;
namespace Cosmos.IL2CPU.X86.IL
{
    /// <summary>
    /// Tests whether an object reference (type O) is an instance of a particular class.
    /// </summary>
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Isinst )]
    public class Isinst : ILOp
    {
        public Isinst( Cosmos.IL2CPU.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            OpType xType = ( OpType )aOpCode;
            string xTypeID = GetTypeIDLabel(xType.Value);
            string mReturnNullLabel = GetLabel( aMethod, aOpCode ) + "_ReturnNull";

            new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true };
            new CPUx86.Compare { DestinationReg = CPUx86.Registers.EAX, SourceValue = 0 };
            new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.Zero, DestinationLabel = mReturnNullLabel };
            new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX, DestinationIsIndirect = true };
            new CPUx86.Push { DestinationRef = ElementReference.New( xTypeID ), DestinationIsIndirect = true };

            Assembler.Stack.Push( new StackContents.Item( 4, typeof( object ) ) );
            Assembler.Stack.Push( new StackContents.Item( 4, typeof( object ) ) );

            System.Reflection.MethodBase xMethodIsInstance = Indy.IL2CPU.Compiler.ReflectionUtilities.GetMethodBase( typeof( VTablesImpl ), "IsInstance", "System.Int32", "System.Int32" );

            new Call( Assembler ).Execute( aMethod, new OpMethod( ILOpCode.Code.Call, 0, 0, xMethodIsInstance) );

            new Label( GetLabel( aMethod, aOpCode ) + "_After_IsInstance_Call" );
            Assembler.Stack.Pop();
            new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
            new CPUx86.Compare { DestinationReg = CPUx86.Registers.EAX, SourceValue = 0 };
            new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.Equal, DestinationLabel = mReturnNullLabel };
            Jump_End( aMethod );
            new Label( mReturnNullLabel );
            new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 4 };
            new CPUx86.Push { DestinationValue = 0 };
        }


        // using System;
        // using System.Collections.Generic;
        // using System.IO;
        // using CPU = Cosmos.IL2CPU.X86;
        // using CPUx86 = Cosmos.IL2CPU.X86;
        // using System.Reflection;
        // using Cosmos.IL2CPU.X86;
        // using Indy.IL2CPU.Compiler;
        // 
        // namespace Indy.IL2CPU.IL.X86 {
        //     [OpCode(OpCodeEnum.Isinst)]
        //     public class Isinst : Op {
        //         private string mTypeId;
        //         private string mThisLabel;
        //         private string mNextOpLabel;
        //         private int mCurrentILOffset;
        //         private bool mDebugMode;
        // 
        //         //public static void ScanOp(ILReader aReader,
        //         //                          MethodInformation aMethodInfo,
        //         //                          SortedList<string, object> aMethodData) {
        //         //    Type xType = aReader.OperandValueType;
        //         //    if (xType == null) {
        //         //        throw new Exception("Unable to determine Type!");
        //         //    }
        //         //    Engine.RegisterType(xType);
        //         //    Call.ScanOp(Engine.GetMethodBase(typeof(VTablesImpl),
        //         //                                     "IsInstance",
        //         //                                     "System.Int32",
        //         //                                     "System.Int32"));
        //         //    Newobj.ScanOp(typeof(InvalidCastException).GetConstructor(new Type[0]));
        //         //}
        // 
        //         public Isinst(ILReader aReader,
        //                       MethodInformation aMethodInfo)
        //             : base(aReader,
        //                    aMethodInfo) {
        //             Type xType = aReader.OperandValueType;
        //             if (xType == null) {
        //                 throw new Exception("Unable to determine Type!");
        //             }
        //             mTypeDef = xType;
        //             mThisLabel = GetInstructionLabel(aReader.Position);
        //             mNextOpLabel = GetInstructionLabel(aReader.NextPosition);
        //             mCurrentILOffset = (int)aReader.Position;
        //             mDebugMode = aMethodInfo.DebugMode;
        //         }
        //         private Type mTypeDef;
        //         public override void DoAssemble() {
        //             mTypeId = GetService<IMetaDataInfoService>().GetTypeIdLabel(mTypeDef);
        //             string mReturnNullLabel = mThisLabel + "_ReturnNull";
        //             new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true };
        //             new CPUx86.Compare { DestinationReg = CPUx86.Registers.EAX, SourceValue = 0 };
        //             new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.Zero, DestinationLabel = mReturnNullLabel };
        //             new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX, DestinationIsIndirect = true };
        //             new CPUx86.Push { DestinationRef = ElementReference.New(mTypeId), DestinationIsIndirect=true };
        //             Assembler.Stack.Push(new StackContent(4,
        //                                                           typeof(object)));
        //             Assembler.Stack.Push(new StackContent(4,
        //                                                           typeof(object)));
        //             MethodBase xMethodIsInstance = ReflectionUtilities.GetMethodBase(typeof(VTablesImpl),
        //                                                                 "IsInstance",
        //                                                                 "System.Int32",
        //                                                                 "System.Int32");
        //             Op xOp = new Call(xMethodIsInstance,
        //                               (uint)mCurrentILOffset,
        //                               mDebugMode,
        //                               mThisLabel+ "_After_IsInstance_Call");
        //             xOp.Assembler = Assembler;
        //             xOp.SetServiceProvider(GetServiceProvider());
        //             xOp.Assemble();
        //             new Label(mThisLabel + "_After_IsInstance_Call");
        //             Assembler.Stack.Pop();
        //             new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
        //             new CPUx86.Compare { DestinationReg = CPUx86.Registers.EAX, SourceValue = 0 };
        //             new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.NotEqual, DestinationLabel = mNextOpLabel };
        //             new Label(mReturnNullLabel);
        //             new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 4 };
        //             new CPUx86.Push { DestinationValue = 0 };
        //         }
        //     }
        // }

    }
}
