using System;
using CPU = Cosmos.Assembler.x86;
using CPUx86 = Cosmos.Assembler.x86;
using Cosmos.IL2CPU.ILOpCodes;
using Cosmos.Assembler;
using Cosmos.IL2CPU.Plugs.System;

using SysReflection = System.Reflection;

namespace Cosmos.IL2CPU.X86.IL
{
  [Cosmos.IL2CPU.OpCode(ILOpCode.Code.Unbox_Any)]
  public class Unbox_Any : ILOp
  {
    public Unbox_Any(Cosmos.Assembler.Assembler aAsmblr)
        : base(aAsmblr)
    {
    }

    public override void Execute(MethodInfo aMethod, ILOpCode aOpCode)
    {
      DoNullReferenceCheck(Assembler, DebugEnabled, 0);
      OpType xType = (OpType)aOpCode;
      string xBaseLabel = GetLabel(aMethod, aOpCode) + ".";
      string xTypeID = GetTypeIDLabel(xType.Value);
      uint xTypeSize = SizeOfType(xType.Value);
      string mReturnNullLabel = xBaseLabel + "_ReturnNull";

      new CPUx86.Compare { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true, SourceValue = 0 };
      new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.Zero, DestinationLabel = mReturnNullLabel };
      new CPUx86.Mov { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true };
      new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX, DestinationIsIndirect = true };
      new CPUx86.Push { DestinationRef = Cosmos.Assembler.ElementReference.New(xTypeID), DestinationIsIndirect = true };
      SysReflection.MethodBase xMethodIsInstance = ReflectionUtilities.GetMethodBase(typeof(VTablesImpl), "IsInstance", "System.UInt32", "System.UInt32");
      Call.DoExecute(Assembler, aMethod, xMethodIsInstance, aOpCode, GetLabel(aMethod, aOpCode), xBaseLabel + "_After_IsInstance_Call", DebugEnabled);
      new Label(xBaseLabel + "_After_IsInstance_Call");
      new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
      new CPUx86.Compare { DestinationReg = CPUx86.Registers.EAX, SourceValue = 0 };
      new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.Equal, DestinationLabel = mReturnNullLabel };
      new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
      uint xSize = xTypeSize;
      if (xSize % 4 > 0)
      {
        xSize += 4 - (xSize % 4);
      }
      int xItems = (int)xSize / 4;
      for (int i = xItems - 1; i >= 0; i--)
      {
        new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = ((i * 4) + ObjectImpl.FieldDataOffset) };
      }
      new CPUx86.Jump { DestinationLabel = GetLabel(aMethod, aOpCode.NextPosition) };
      new Label(mReturnNullLabel);
      new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 4 };
      new CPUx86.Push { DestinationValue = 0 };
    }
  }
}
