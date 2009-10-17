using System;
using Cosmos.IL2CPU;
using Cosmos.IL2CPU.ILOpCodes;
using CPUx86 = Cosmos.IL2CPU.X86;
using System.Collections.Generic;

namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Ldc_I8 )]
    public class Ldc_I8 : ILOp
    {
        public Ldc_I8( Cosmos.IL2CPU.Assembler aAsmblr ) : base( aAsmblr ) { }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode ) {
          //TODO: Fix for 64 bit
            //new CPUx86.Push { DestinationValue = ( ( OpInt64 )aOpCode ).Value };
            Assembler.Stack.Push(8, typeof( long ));
        }

    }

    
		// using System;
		// using System.IO;
		// using CPU = Cosmos.IL2CPU.X86;
		// using Cosmos.IL2CPU.X86;
		// using System.Diagnostics;
		// 
		// namespace Cosmos.IL2CPU.IL.X86 {
		// 	[OpCode(OpCodeEnum.Ldc_I8)]
		// 	public class Ldc_I8: Op {
		// 		private readonly long mValue;
		// 		public Ldc_I8(ILReader aReader, MethodInformation aMethodInfo)
		// 			: base(aReader, aMethodInfo)
		// 		{
		// 			Debug.Assert(aReader.Operand.Length == 8);
		// 
		// 			ulong value = 0;
		// 			for (int i = 7; i >=0; i--)
		// 			{
		// 				value <<= 8;
		// 				value |= aReader.Operand[i];
		// 			}
		// 			mValue = (long)value;
		// 		}
		// 		public override void DoAssemble() {
		// 			string theValue = mValue.ToString("X16");
		//             new CPU.Push { DestinationValue = BitConverter.ToUInt32(BitConverter.GetBytes(mValue), 0) };
		//             new CPU.Push { DestinationValue = BitConverter.ToUInt32(BitConverter.GetBytes(mValue), 4) };
		// 			Assembler.Stack.Push(new StackContent(8, typeof(long)));
		// 		}
		// 	}
		// }
}
