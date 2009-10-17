//using System;
//using System.Collections.Generic;
//using Indy.IL2CPU.Compiler;
//using CPU = Indy.IL2CPU.Assembler.X86;
//using System.Reflection;
//using Indy.IL2CPU.Assembler;

//namespace Indy.IL2CPU.IL.X86 {
//    [OpCode(OpCodeEnum.Ldsflda)]
//    public class Ldsflda: Op {
//        private string mDataName;
//        private FieldInfo mField;

//            public static void ScanOp(ILReader aReader, MethodInformation aMethodInfo, SortedList<string, object> aMethodData, IServiceProvider aProvider)
//            {
//                FieldInfo xField = aReader.OperandValueField;
//                Engine.QueueStaticField(xField);
//            }

//        public Ldsflda(ILReader aReader, MethodInformation aMethodInfo)
//            : base(aReader, aMethodInfo) {
//            mField = aReader.OperandValueField;
            
//        }

//        public override void DoAssemble() {
//            mDataName = GetService<IMetaDataInfoService>().GetStaticFieldLabel(mField);
//            new CPU.Push { DestinationRef = ElementReference.New(mDataName) };
//            Assembler.StackContents.Push(new StackContent(4, true, false, false));
//        }
//    }
//}