using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CPU = Cosmos.IL2CPU.X86;
using Cosmos.IL2CPU.ILOpCodes;
using System.Reflection;
using Cosmos.IL2CPU.X86.IL;
using Indy.IL2CPU.IL;
using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Cosmos.IL2CPU.X86
{
    public abstract class ILOp : Cosmos.IL2CPU.ILOp
    {
        protected new readonly Assembler Assembler;

        protected ILOp( Cosmos.IL2CPU.Assembler aAsmblr )
            : base( aAsmblr )
        {
            Assembler = ( Assembler )aAsmblr;
        }

        protected void Jump_Exception(MethodInfo aMethod) {
          // todo: port to numeric labels
          new CPU.Jump { DestinationLabel = MethodInfoLabelGenerator.GenerateLabelName(aMethod.MethodBase) + "___EXCEPTION___EXIT" };
        }

        protected void Jump_End(MethodInfo aMethod)
        {
#warning todo: Jump_End jumps to ___EXCEPTION___EXIT
            new CPU.Jump { DestinationLabel = MethodInfoLabelGenerator.GenerateLabelName( aMethod.MethodBase ) + "___EXCEPTION___NORMAL" };
        }

        protected uint GetStackCountForLocal(MethodInfo aMethod, LocalVariableInfo aField)
		{
			var xSize = SizeOfType(aField.LocalType);
			var xResult = xSize / 4;
			if (xSize % 4 == 0)
			{
				xResult++;
			}
			return xResult;
		}

		protected uint GetEBPOffsetForLocal(MethodInfo aMethod, OpVar aOp)
		{
			var xBody = aMethod.MethodBase.GetMethodBody();
			uint xOffset = 0;
			for(int i = 0; i < xBody.LocalVariables.Count;i++){
				if (i == aOp.Value)
				{
					break;
				}
				var xField = xBody.LocalVariables[i];
				xOffset += GetStackCountForLocal(aMethod, xField);
			}
			return xOffset;
		}

		protected string GetLabel(MethodInfo aMethod, ILOpCode aOpCode)
		{
            return MethodInfoLabelGenerator.GenerateLabelName( aMethod.MethodBase ) + "__DOT__" + aOpCode.Position.ToString( "X8" ).ToUpper();
		}

        protected uint Align( uint aSize, uint aAlign )
        {
            return aSize % 4 == 0 ? aSize : ( ( aSize / aAlign ) * aAlign ) + 1;
        }
        protected void ThrowNotImplementedException(string aMessage) {
          new CPU.Push {
            DestinationRef = ElementReference.New(LdStr.GetContentsArrayName("Conv_Ovf_I4 instruction not implemented"))
          };
          new CPU.Call {
            DestinationLabel = MethodInfoLabelGenerator.GenerateLabelName(typeof(ExceptionHelper).GetMethod("ThrowNotImplemented", BindingFlags.Static | BindingFlags.Public))
          };
        }
    }
}
