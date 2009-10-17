//using System;
//using System.Collections.Generic;
//using Indy.IL2CPU.Assembler;


//using CPUx86 = Indy.IL2CPU.Assembler.X86;
//using System.Reflection;
//using Indy.IL2CPU.Compiler;

//namespace Indy.IL2CPU.IL.X86 {
//    [OpCode(OpCodeEnum.Stsfld)]
//    public class Stsfld: Op {
//        private string mDataName;
//        private Type mDataType;
//        private bool mNeedsGC;
//        private string mBaseLabel;
//        private string mNextLabel;
//        private string mCurLabel;
//        private uint mCurOffset;
//        private MethodInformation mMethodInformation;

//            public static void ScanOp(ILReader aReader, MethodInformation aMethodInfo, SortedList<string, object> aMethodData, IServiceProvider aProvider)
//            {
//                FieldInfo xField = aReader.OperandValueField;
//                Engine.QueueStaticField(xField);
//            }
//        private FieldInfo mField;
//        public Stsfld(ILReader aReader, MethodInformation aMethodInfo)
//            : base(aReader, aMethodInfo) {
//            mField = aReader.OperandValueField;
//            mDataName = DataMember.GetStaticFieldName(mField);
//            mNeedsGC = !mField.FieldType.IsValueType;
//            mDataType = mField.FieldType;
//            mBaseLabel = GetInstructionLabel(aReader);
//             mMethodInformation = aMethodInfo;
//            mCurOffset = aReader.Position;
//            mCurLabel = IL.Op.GetInstructionLabel(aReader);
//            mNextLabel = IL.Op.GetInstructionLabel(aReader.NextPosition);
//        }

//        public override void DoAssemble() {
//            var xSize = GetService<IMetaDataInfoService>().SizeOfType(mField.FieldType);
//            var xDecRefMethodInfo = GetService<IMetaDataInfoService>().GetMethodInfo(GCImplementationRefs.DecRefCountRef,
//                                                                                     false);


//            if (mNeedsGC) {
//                new CPUx86.Push { DestinationRef = ElementReference.New(mDataName), DestinationIsIndirect = true };
//                new CPUx86.Call { DestinationLabel = xDecRefMethodInfo.LabelName};
//            }
//            for (int i = 0; i < (xSize / 4); i++)
//            {
//                new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
//                new CPUx86.Move { DestinationRef = ElementReference.New(mDataName, i * 4), DestinationIsIndirect = true, SourceReg = CPUx86.Registers.EAX };
//            }
//            switch (xSize % 4)
//            {
//                case 1: {
//                        new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
//                        new CPUx86.Move { DestinationRef = ElementReference.New(mDataName, (int)((xSize / 4) * 4)), DestinationIsIndirect = true, SourceReg = CPUx86.Registers.AL };
//                        break;
//                    }
//                case 2: {
//                        new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
//                        new CPUx86.Move { DestinationRef = ElementReference.New(mDataName, (int)((xSize / 4) * 4)), DestinationIsIndirect = true, SourceReg = CPUx86.Registers.AX };
//                        break;
//                    }
//                case 0: {
//                        break;
//                    }
//                default:
//                    EmitNotImplementedException(Assembler, GetServiceProvider(), "Ldsfld: Remainder size " + (xSize % 4) + " not supported!", mCurLabel, mMethodInformation, mCurOffset, mNextLabel);
//                    break;

//            }
//            Assembler.StackContents.Pop();
//        }
//    }
//}
