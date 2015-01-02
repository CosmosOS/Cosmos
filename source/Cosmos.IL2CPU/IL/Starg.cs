using System;
using CPU = Cosmos.Assembler.x86;
using CPUx86 = Cosmos.Assembler.x86;
using Cosmos.IL2CPU.ILOpCodes;
using SysReflection = System.Reflection;

namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Starg )]
    public class Starg : ILOp
    {
        public Starg( Cosmos.Assembler.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            //throw new NotImplementedException();

            OpVar xOpVar = ( OpVar )aOpCode;
            //mAddresses = aMethodInfo.Arguments[ xOpVar.Value ].VirtualAddresses;

            var xMethodInfo = aMethod.MethodBase as SysReflection.MethodInfo;
            uint xReturnSize = 0;
            if( xMethodInfo != null )
            {
                xReturnSize = Align( SizeOfType( xMethodInfo.ReturnType ), 4 );
            }
            uint xOffset = 8;
            var xCorrectedOpValValue = xOpVar.Value;
            if( !aMethod.MethodBase.IsStatic && xOpVar.Value > 0 )
            {
                // if the method has a $this, the OpCode value includes the this at index 0, but GetParameters() doesnt include the this
                xCorrectedOpValValue -= 1;
            }
            var xParams = aMethod.MethodBase.GetParameters();

            for( int i = xParams.Length - 1; i > xCorrectedOpValValue; i-- )
            {
                var xSize = Align( SizeOfType( xParams[ i ].ParameterType ), 4 );
                xOffset += xSize;
            }
            var xCurArgSize = Align( SizeOfType( xParams[ xCorrectedOpValValue ].ParameterType ), 4 );
            uint xArgSize = 0;
            foreach( var xParam in xParams )
            {
                xArgSize += Align( SizeOfType( xParam.ParameterType ), 4 );
            }
            //for (int i = xParams.Length - 1; i >= xOpVar.Value; i--) {
            //  var xSize = Align(SizeOfType(xParams[i].ParameterType), 4);
            //  xOffset += xSize;
            //}
            for (int i = 0; i < (xCurArgSize / 4); i++) {
              new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
              new CPUx86.Mov { DestinationReg = CPUx86.Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = (int)(xOffset + /*xCurArgSize -*/ ((i/* + 1*/) * 4)), SourceReg = CPUx86.Registers.EAX };
            }
        }


        // using System;
        //
        //
        // using CPUx86 = Cosmos.Assembler.x86;
        //
        // namespace Cosmos.IL2CPU.IL.X86 {
        // 	[Cosmos.Assembler.OpCode(OpCodeEnum.Starg)]
        // 	public class Starg: Op {
        // 		private int[] mAddresses;
        // 		protected void SetArgIndex(int aIndex, MethodInformation aMethodInfo) {
        // 			mAddresses = aMethodInfo.Arguments[aIndex].VirtualAddresses;
        //
        // 		}
        // 		public Starg(ILReader aReader, MethodInformation aMethodInfo)
        // 			: base(aReader, aMethodInfo) {
        // 			SetArgIndex(aReader.OperandValueInt32, aMethodInfo);
        // 		}
        // 		public override void DoAssemble() {
        // 			if (mAddresses == null || mAddresses.Length == 0) {
        // 				throw new Exception("No Address Specified!");
        // 			}
        // 			for (int i = (mAddresses.Length - 1); i >= 0; i -= 1) {
        // 				new CPUx86.Pop{DestinationReg=CPUx86.Registers.EAX};
        //                 new CPUx86.Move { DestinationReg = CPUx86.Registers.EBP, DestinationIsIndirect = true, DestinationDisplacement = mAddresses[i], SourceReg = CPUx86.Registers.EAX };
        // 			}
        // 			Assembler.Stack.Pop();
        // 		}
        // 	}
        // }

    }
}
