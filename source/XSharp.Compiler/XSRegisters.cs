using Cosmos.Assembler.x86;

namespace XSharp.Compiler
{
  public static class XSRegisters
  {
    public enum RegisterSize: byte
    {
      Byte8 = 8,
      Short16 = 16,
      Int32 = 32
    }

    public abstract class Register
    {
      public readonly RegisterSize Size;
      public readonly string Name;
      public readonly RegistersEnum RegEnum;

      protected Register(string name, RegistersEnum regEnum, RegisterSize size)
      {
        Size = size;
        Name = name;
        RegEnum = regEnum;
      }
    }

    public class Register8: Register
    {
      public Register8(string name, RegistersEnum regEnum) : base(name, regEnum, RegisterSize.Byte8)
      {
      }
    }

    public class Register16 : Register
    {
      public Register16(string name, RegistersEnum regEnum) : base(name, regEnum, RegisterSize.Short16)
      {
      }
    }

    public class Register32 : Register
    {
      public Register32(string name, RegistersEnum regEnum) : base(name, regEnum, RegisterSize.Int32)
      {
      }
    }

    public static readonly Register AL = new Register8(nameof(AL), RegistersEnum.AL);
    public static readonly Register AH = new Register8(nameof(AH), RegistersEnum.AH);
    public static readonly Register AX = new Register16(nameof(AX), RegistersEnum.AX);
    public static readonly Register EAX = new Register32(nameof(EAX), RegistersEnum.EAX);

    public static readonly Register BL = new Register8(nameof(BL), RegistersEnum.BL);
    public static readonly Register BH = new Register8(nameof(BH), RegistersEnum.BH);
    public static readonly Register BX = new Register16(nameof(BX), RegistersEnum.BX);
    public static readonly Register EBX = new Register32(nameof(EBX), RegistersEnum.EBX);

    public static readonly Register CL = new Register8(nameof(CL), RegistersEnum.CL);
    public static readonly Register CH = new Register8(nameof(CH), RegistersEnum.CH);
    public static readonly Register CX = new Register16(nameof(CX), RegistersEnum.CX);
    public static readonly Register ECX = new Register32(nameof(ECX), RegistersEnum.ECX);

    public static readonly Register DL = new Register8(nameof(DL), RegistersEnum.DL);
    public static readonly Register DH = new Register8(nameof(DH), RegistersEnum.DH);
    public static readonly Register DX = new Register16(nameof(DX), RegistersEnum.DX);
    public static readonly Register EDX = new Register32(nameof(EDX), RegistersEnum.EDX);

    public static readonly Register EBP = new Register32(nameof(EBP), RegistersEnum.EBP);
    public static readonly Register ESP = new Register32(nameof(ESP), RegistersEnum.ESP);
    public static readonly Register ESI = new Register32(nameof(ESI), RegistersEnum.ESI);
    public static readonly Register EDI = new Register32(nameof(EDI), RegistersEnum.EDI);
  }
}
