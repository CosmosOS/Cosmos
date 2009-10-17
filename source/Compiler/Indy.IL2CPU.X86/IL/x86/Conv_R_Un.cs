//using System;
//using System.IO;


//using CPU = Indy.IL2CPU.Assembler.X86;

//namespace Indy.IL2CPU.IL.X86 {
//    [OpCode(OpCodeEnum.Conv_R_Un)]
//    public class Conv_R_Un: Op {
//        private string mNextLabel;
//        private string mCurLabel;
//        private uint mCurOffset;
//        private MethodInformation mMethodInformation;
//        public Conv_R_Un(ILReader aReader, MethodInformation aMethodInfo)
//            : base(aReader, aMethodInfo) {
//             mMethodInformation = aMethodInfo;
//            mCurOffset = aReader.Position;
//            mCurLabel = IL.Op.GetInstructionLabel(aReader);
//            mNextLabel = IL.Op.GetInstructionLabel(aReader.NextPosition);
//        }
//        public override void DoAssemble() {
//            EmitNotImplementedException(Assembler, GetServiceProvider(), "Conv_R_Un: Not implemented at all yet!", mCurLabel, mMethodInformation, mCurOffset, mNextLabel);
//        }
//    }
//}