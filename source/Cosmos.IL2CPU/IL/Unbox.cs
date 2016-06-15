using System;
using CPU = Cosmos.Assembler.x86;
using CPUx86 = Cosmos.Assembler.x86;
using Cosmos.IL2CPU.ILOpCodes;
using Cosmos.Assembler;
using Cosmos.IL2CPU.Plugs.System;
using XSharp.Compiler;
using SysReflection = System.Reflection;

namespace Cosmos.IL2CPU.X86.IL
{
  [Cosmos.IL2CPU.OpCode(ILOpCode.Code.Unbox)]
  public class Unbox : ILOp
  {
    public Unbox(Cosmos.Assembler.Assembler aAsmblr)
        : base(aAsmblr)
    {
    }

    public override void Execute(MethodInfo aMethod, ILOpCode aOpCode)
    {
      DoNullReferenceCheck(Assembler, DebugEnabled, 0);
      OpType xType = (OpType)aOpCode;
      string xTypeID = GetTypeIDLabel(xType.Value);
      string xBaseLabel = GetLabel(aMethod, aOpCode) + ".";
      string mReturnNullLabel = xBaseLabel + "_ReturnNull";
      uint xTypeSize = SizeOfType(xType.Value);

      XS.Compare(XSRegisters.OldToNewRegister(CPU.RegistersEnum.EAX), 0);
      new CPU.ConditionalJump { Condition = CPU.ConditionalTestEnum.Zero, DestinationLabel = mReturnNullLabel };
      new CPU.Mov { DestinationReg = CPU.RegistersEnum.EAX, SourceReg = CPU.RegistersEnum.EAX, SourceIsIndirect = true };
      new CPU.Push { DestinationReg = CPU.RegistersEnum.EAX, DestinationIsIndirect = true };
      new CPU.Push { DestinationRef = ElementReference.New(xTypeID), DestinationIsIndirect = true };
      SysReflection.MethodBase xMethodIsInstance = ReflectionUtilities.GetMethodBase(typeof(VTablesImpl), "IsInstance", "System.UInt32", "System.UInt32");
      Call.DoExecute(Assembler, aMethod, xMethodIsInstance, aOpCode, GetLabel(aMethod, aOpCode), xBaseLabel + "_After_IsInstance_Call", DebugEnabled);
      new Label(xBaseLabel + "_After_IsInstance_Call");
      XS.Pop(XSRegisters.OldToNewRegister(CPU.RegistersEnum.EAX));
      XS.Compare(XSRegisters.OldToNewRegister(CPU.RegistersEnum.EAX), 0);
      new CPU.ConditionalJump { Condition = CPU.ConditionalTestEnum.Equal, DestinationLabel = mReturnNullLabel };
      XS.Pop(XSRegisters.OldToNewRegister(CPU.RegistersEnum.EAX));
      uint xSize = xTypeSize;
      if (xSize % 4 > 0)
      {
        xSize += 4 - (xSize % 4);
      }
      int xItems = (int)xSize / 4;
      for (int i = xItems - 1; i >= 0; i--)
      {
        new CPU.Push { DestinationReg = CPU.RegistersEnum.EAX, DestinationIsIndirect = true, DestinationDisplacement = ((i * 4) + ObjectImpl.FieldDataOffset) };
      }
      new CPU.Jump { DestinationLabel = GetLabel(aMethod, aOpCode.NextPosition) };
      new Label(mReturnNullLabel);
      XS.Add(XSRegisters.OldToNewRegister(CPU.RegistersEnum.ESP), 4);
      new CPUx86.Push { DestinationValue = 0 };
    }
  }
}
