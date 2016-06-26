using System;
using System.Collections.Generic;
using System.Reflection;
using Cosmos.Assembler.x86;

namespace XSharp.Compiler
{
  public static class XSRegisters
  {
    public enum RegisterSize: byte
    {
      Byte8 = 8,
      Short16 = 16,
      Int32 = 32,
      Long64 = 64,
      FPU = 128,
      XMM = 128
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

      public static implicit operator RegistersEnum(Register register)
      {
        return register.RegEnum;
      }
    }

    private static readonly Dictionary<string, Register> mRegisters;

    static XSRegisters()
    {
      mRegisters = new Dictionary<string, Register>();
      foreach (var xField in typeof(XSRegisters).GetFields(BindingFlags.Static | BindingFlags.Public))
      {
        mRegisters.Add(xField.Name, (Register)xField.GetValue(null));
      }
    }

    public static Register OldToNewRegister(RegistersEnum register)
    {
      Register xResult;
      if (!mRegisters.TryGetValue(register.ToString(), out xResult))
      {
        throw new NotImplementedException($"Register {register} not yet implemented!");
      }
      return xResult;
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

    public class RegisterFPU: Register
    {
      public RegisterFPU(string name, RegistersEnum regEnum): base(name, regEnum, RegisterSize.FPU)
      {
      }
    }

    public class RegisterXMM : Register
    {
      public RegisterXMM(string name, RegistersEnum regEnum) : base(name, regEnum, RegisterSize.XMM)
      {
      }
    }

    public class RegisterSegment: Register
    {
      public RegisterSegment(string name, RegistersEnum regEnum): base(name, regEnum, RegisterSize.Short16)
      {
      }
    }

    public static readonly Register8 AL = new Register8(nameof(AL), RegistersEnum.AL);
    public static readonly Register8 AH = new Register8(nameof(AH), RegistersEnum.AH);
    public static readonly Register16 AX = new Register16(nameof(AX), RegistersEnum.AX);
    public static readonly Register32 EAX = new Register32(nameof(EAX), RegistersEnum.EAX);

    public static readonly Register8 BL = new Register8(nameof(BL), RegistersEnum.BL);
    public static readonly Register8 BH = new Register8(nameof(BH), RegistersEnum.BH);
    public static readonly Register16 BX = new Register16(nameof(BX), RegistersEnum.BX);
    public static readonly Register32 EBX = new Register32(nameof(EBX), RegistersEnum.EBX);

    public static readonly Register8  CL = new Register8(nameof(CL), RegistersEnum.CL);
    public static readonly Register8  CH = new Register8(nameof(CH), RegistersEnum.CH);
    public static readonly Register16 CX = new Register16(nameof(CX), RegistersEnum.CX);
    public static readonly Register32 ECX = new Register32(nameof(ECX), RegistersEnum.ECX);

    public static readonly Register8  DL = new Register8(nameof(DL), RegistersEnum.DL);
    public static readonly Register8  DH = new Register8(nameof(DH), RegistersEnum.DH);
    public static readonly Register16 DX = new Register16(nameof(DX), RegistersEnum.DX);
    public static readonly Register32 EDX = new Register32(nameof(EDX), RegistersEnum.EDX);

    public static readonly Register32 EBP = new Register32(nameof(EBP), RegistersEnum.EBP);
    public static readonly Register32 ESP = new Register32(nameof(ESP), RegistersEnum.ESP);
    public static readonly Register32 ESI = new Register32(nameof(ESI), RegistersEnum.ESI);
    public static readonly Register32 EDI = new Register32(nameof(EDI), RegistersEnum.EDI);

    // Segment registers
    public static readonly RegisterSegment CS = new RegisterSegment(nameof(CS), RegistersEnum.CS);
    public static readonly RegisterSegment DS = new RegisterSegment(nameof(DS), RegistersEnum.DS);
    public static readonly RegisterSegment ES = new RegisterSegment(nameof(ES), RegistersEnum.ES);
    public static readonly RegisterSegment FS = new RegisterSegment(nameof(FS), RegistersEnum.FS);
    public static readonly RegisterSegment GS = new RegisterSegment(nameof(GS), RegistersEnum.GS);
    public static readonly RegisterSegment SS = new RegisterSegment(nameof(SS), RegistersEnum.SS);

    public static readonly RegisterFPU ST0 = new RegisterFPU(nameof(ST0), RegistersEnum.ST0);
    public static readonly RegisterFPU ST1 = new RegisterFPU(nameof(ST1), RegistersEnum.ST1);
    public static readonly RegisterFPU ST2 = new RegisterFPU(nameof(ST2), RegistersEnum.ST2);
    public static readonly RegisterFPU ST3 = new RegisterFPU(nameof(ST3), RegistersEnum.ST3);
    public static readonly RegisterFPU ST4 = new RegisterFPU(nameof(ST4), RegistersEnum.ST4);
    public static readonly RegisterFPU ST5 = new RegisterFPU(nameof(ST5), RegistersEnum.ST5);
    public static readonly RegisterFPU ST6 = new RegisterFPU(nameof(ST6), RegistersEnum.ST6);
    public static readonly RegisterFPU ST7 = new RegisterFPU(nameof(ST7), RegistersEnum.ST7);

    public static readonly RegisterXMM XMM0 = new RegisterXMM(nameof(XMM0), RegistersEnum.XMM0);
    public static readonly RegisterXMM XMM1 = new RegisterXMM(nameof(XMM1), RegistersEnum.XMM1);
    public static readonly RegisterXMM XMM2 = new RegisterXMM(nameof(XMM2), RegistersEnum.XMM2);
    public static readonly RegisterXMM XMM3 = new RegisterXMM(nameof(XMM3), RegistersEnum.XMM3);
    public static readonly RegisterXMM XMM4 = new RegisterXMM(nameof(XMM4), RegistersEnum.XMM4);
    public static readonly RegisterXMM XMM5 = new RegisterXMM(nameof(XMM5), RegistersEnum.XMM5);
    public static readonly RegisterXMM XMM6 = new RegisterXMM(nameof(XMM6), RegistersEnum.XMM6);
    public static readonly RegisterXMM XMM7 = new RegisterXMM(nameof(XMM7), RegistersEnum.XMM7);

    // Control Registers
    public static readonly Register32 CR0 = new Register32(nameof(CR0), RegistersEnum.CR0);
    public static readonly Register32 CR1 = new Register32(nameof(CR1), RegistersEnum.CR1);
    public static readonly Register32 CR2 = new Register32(nameof(CR2), RegistersEnum.CR2);
    public static readonly Register32 CR3 = new Register32(nameof(CR3), RegistersEnum.CR3);
    public static readonly Register32 CR4 = new Register32(nameof(CR4), RegistersEnum.CR4);
    }
}
