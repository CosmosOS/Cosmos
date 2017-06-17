using System.Reflection;

using CPU = Cosmos.Assembler.x86;
using Cosmos.IL2CPU.ILOpCodes;
using Cosmos.IL2CPU.Plugs;
using XSharp.Common;

namespace Cosmos.IL2CPU.X86.IL
{
  [Cosmos.IL2CPU.OpCode(ILOpCode.Code.Unbox_Any)]
  public class Unbox_Any : ILOp
  {
    public Unbox_Any(Cosmos.Assembler.Assembler aAsmblr)
        : base(aAsmblr)
    {
    }

    public override void Execute(_MethodInfo aMethod, ILOpCode aOpCode)
    {
      DoNullReferenceCheck(Assembler, DebugEnabled, 0);
      OpType xType = (OpType)aOpCode;
      string xBaseLabel = GetLabel(aMethod, aOpCode) + ".";
      string xTypeID = GetTypeIDLabel(xType.Value);
      uint xTypeSize = SizeOfType(xType.Value);
      string mReturnNullLabel = xBaseLabel + "_ReturnNull";

      XS.Compare(XSRegisters.ESP, 0, destinationIsIndirect: true);
      XS.Jump(CPU.ConditionalTestEnum.Zero, mReturnNullLabel);
      XS.Set(XSRegisters.EAX, XSRegisters.ESP, sourceIsIndirect: true);
      XS.Push(XSRegisters.EAX, isIndirect: true);
      XS.Push(xTypeID, isIndirect: true);
      MethodBase xMethodIsInstance = ReflectionUtilities.GetMethodBase(typeof(VTablesImpl), "IsInstance", "System.UInt32", "System.UInt32");
      Call.DoExecute(Assembler, aMethod, xMethodIsInstance, aOpCode, GetLabel(aMethod, aOpCode), xBaseLabel + "_After_IsInstance_Call", DebugEnabled);
      XS.Label(xBaseLabel + "_After_IsInstance_Call");
      XS.Pop(XSRegisters.EAX);
      XS.Compare(XSRegisters.EAX, 0);
      XS.Jump(CPU.ConditionalTestEnum.Equal, mReturnNullLabel);
      XS.Pop(XSRegisters.EAX);
      uint xSize = xTypeSize;
      if (xSize % 4 > 0)
      {
        xSize += 4 - (xSize % 4);
      }
      int xItems = (int)xSize / 4;
      for (int i = xItems - 1; i >= 0; i--)
      {
        new CPU.Push { DestinationReg = CPU.RegistersEnum.EAX, DestinationIsIndirect = true, DestinationDisplacement = ((i * 4) + ObjectUtilities.FieldDataOffset) };
      }
      new CPU.Jump { DestinationLabel = GetLabel(aMethod, aOpCode.NextPosition) };
      XS.Label(mReturnNullLabel);
      XS.Add(XSRegisters.ESP, 4);
      XS.Push(0);
    }
  }
}
