//using System;
//using System.IO;


//using CPU = Indy.IL2CPU.Assembler.X86;

//namespace Indy.IL2CPU.IL.X86 {
//    [OpCode(OpCodeEnum.Sub_Ovf_Un)]
//    public class Sub_Ovf_Un: Op {
//        private string mCurrentLabel;
//        private string mNextLabel;
//        private uint mCurrentOffset;
//        private MethodInformation mCurrentMethodInfo;
//        public Sub_Ovf_Un(ILReader aReader, MethodInformation aMethodInfo)
//            : base(aReader, aMethodInfo)
//        {
//            mCurrentLabel = GetInstructionLabel(aReader);
//            mCurrentOffset = aReader.Position;
//            mNextLabel = GetInstructionLabel(aReader.NextPosition);
//            mCurrentMethodInfo = aMethodInfo;
//        }

//        public override void DoAssemble() {
//            EmitNotImplementedException(Assembler, GetServiceProvider(), "Sub_Ovf_Un instruction not yet implemented",
//                mCurrentLabel, mCurrentMethodInfo, mCurrentOffset, mNextLabel);
//        }
//    }
//}