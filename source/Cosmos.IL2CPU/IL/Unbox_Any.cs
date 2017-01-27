using Cosmos.Assembler.x86;
using Cosmos.IL2CPU.ILOpCodes;
using XSharp.Compiler;
using static XSharp.Compiler.XSRegisters;

using ObjectInfo = Cosmos.IL2CPU.Plugs.System.ObjectImpl;
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

      XS.Compare(ESP, 0, destinationIsIndirect: true);
      XS.Jump(ConditionalTestEnum.Zero, mReturnNullLabel);
      XS.Set(EAX, ESP, sourceIsIndirect: true);
      XS.Push(EAX, isIndirect: true);
      XS.Push(xTypeID, isIndirect: true);
      SysReflection.MethodBase xMethodIsInstance = ReflectionUtilities.GetMethodBase(typeof(VTablesImpl), "IsInstance", "System.UInt32", "System.UInt32");
      Call.DoExecute(Assembler, aMethod, xMethodIsInstance, aOpCode, GetLabel(aMethod, aOpCode), xBaseLabel + "_After_IsInstance_Call", DebugEnabled);
      XS.Label(xBaseLabel + "_After_IsInstance_Call");
      XS.Pop(EAX);
      XS.Compare(EAX, 0);
      XS.Jump(ConditionalTestEnum.Equal, mReturnNullLabel);
      XS.Pop(EAX);
      uint xSize = xTypeSize;
      if (xSize % 4 > 0)
      {
        xSize += 4 - (xSize % 4);
      }
      int xItems = (int)xSize / 4;
      for (int i = xItems - 1; i >= 0; i--)
      {
        XS.Push(EAX, displacement: (i * 4) + ObjectInfo.FieldDataOffset);
        //new CPU.Push { DestinationReg = CPU.RegistersEnum.EAX, DestinationIsIndirect = true, DestinationDisplacement = ((i * 4) + ObjectInfo.FieldDataOffset) };
      }
      XS.Jump(GetLabel(aMethod, aOpCode.NextPosition));
      XS.Label(mReturnNullLabel);
      XS.Add(ESP, 4);
      XS.Push(0);
    }
  }
}
