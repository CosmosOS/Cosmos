using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Compiler.Assembler;
using Cosmos.Compiler.Assembler.X86;

namespace Cosmos.Compiler.XSharp {
  public abstract class CodeBlock {
    public enum Flags {
      Zero,
      Equal, // Zero is synonym for Equal
      NotZero,
      NotEqual, // Synonyms
      GreaterThanOrEqualTo,
      LessThan,
      LessThanOrEqualTo,
      Above, AboveOrEqual,
      Below
    };

    //TODO: Add registers as needed, not all are here yet
    public RegisterEAX EAX = RegisterEAX.Instance;
    public RegisterAX AX = RegisterAX.Instance;
    //public RegisterAH AH = RegisterAH.Instance;
    public RegisterAL AL = RegisterAL.Instance;

    public RegisterEBX EBX = RegisterEBX.Instance;
    //BX
    //BH
    public RegisterBL BL = RegisterBL.Instance;

    public RegisterECX ECX = RegisterECX.Instance;
    //CX
    //CH
    //CL

    public RegisterEDX EDX = RegisterEDX.Instance;
    public RegisterDX DX = RegisterDX.Instance;
    //DH
    //DL

    public RegisterEBP EBP = RegisterEBP.Instance;
    public RegisterESP ESP = RegisterESP.Instance;
    public RegisterESI ESI = RegisterESI.Instance;
    public RegisterEDI EDI = RegisterEDI.Instance;

    public readonly Ports Port = new Ports();
    public readonly Memory Memory = new Memory();

    public abstract void Assemble();

    static public string MakeLabel(Type aType) {
      var xParts = aType.FullName.Split('.');
      string xLabel = xParts[xParts.Length - 1];
      return xLabel.Replace('+', '_');
    }

    public string Label {
      set {
        new Cosmos.Compiler.Assembler.Label(value);
      }
    }

    private uint mLabelCounter = 0;
    public string NewLabel() {
      mLabelCounter++;
      return GetType().Name + mLabelCounter.ToString("X8").ToUpper();
    }

    static public void Call<T>() {
      new Call { DestinationLabel = MakeLabel(typeof(T)) };
    }
    public void Call(string aLabel) {
      new Call { DestinationLabel = aLabel };
    }

    public void Define(string aSymbol) {
      new Define(aSymbol);
    }

    public void IfDefined(string aSymbol) {
      new IfDefined(aSymbol);
    }

    public void EndIfDefined() {
      new EndIfDefined();
    }

    public void CallIf(Flags aFlags, string aLabel) {
      CallIf(aFlags, aLabel, "");
    }

    public void CallIf(Flags aFlags, string aLabel, string aJumpAfter) {
      // TODO: This is inefficient - lots of jumps
      // Maybe make an invert function for Flags
      var xLabelIf = NewLabel();
      var xLabelExit = NewLabel();

      JumpIf(aFlags, xLabelIf);
      Jump(xLabelExit);

      Label = xLabelIf;
      Call(aLabel);
      if (aJumpAfter != "") {
        Jump(aJumpAfter);
      }

      Label = xLabelExit;
    }

    public void Jump(string aLabel) {
      new Jump { DestinationLabel = aLabel };
    }

    public void JumpIf(Flags aFlags, string aLabel) {
      ConditionalTestEnum xCondition;
      switch (aFlags) {
        //Overflow = 0,
        //NoOverflow = 1,
        //NotAboveOrEqual = 2,
        //NotBelow = 3,
        //BelowOrEqual = 6,
        //NotAbove = 6,
        //NotBelowOrEqual = 7,
        //Above = 7,
        //Sign = 8,
        //NotSign = 9,
        //Parity = 10,
        //ParityEven = 10,
        //NotParity = 11,
        //ParityOdd = 11,
        //NotGreaterThanOrEqualTo = 12,
        //NotLessThan = 13,
        //NotGreaterThan = 14,
        //NotLessThanOrEqualTo = 15,
        //GreaterThan = 15
        case Flags.Zero:
        case Flags.Equal:
          xCondition = ConditionalTestEnum.Zero;
          break;

        case Flags.NotZero:
        case Flags.NotEqual:
          xCondition = ConditionalTestEnum.NotZero;
          break;

        case Flags.LessThan:
          xCondition = ConditionalTestEnum.LessThan;
          break;

        case Flags.LessThanOrEqualTo:
          xCondition = ConditionalTestEnum.LessThanOrEqualTo;
          break;

        case Flags.GreaterThanOrEqualTo:
          xCondition = ConditionalTestEnum.GreaterThanOrEqualTo;
          break;

        case Flags.Below:
          xCondition = ConditionalTestEnum.Below;
          break;

        case Flags.Above:
          xCondition = ConditionalTestEnum.Above;
          break;

        case Flags.AboveOrEqual:
          xCondition = ConditionalTestEnum.AboveOrEqual;
          break;

        default:
          throw new NotImplementedException("Flag '" + aFlags.ToString() + "' not implemented yet!");

      }
      new ConditionalJump { Condition = xCondition, DestinationLabel = aLabel };
    }

    public void Push(UInt32 aValue) {
      Push(aValue, 32);
    }
    public void Push(UInt32 aValue, byte aSize) {
      new Push { DestinationValue = aValue, Size = aSize };
    }

    public void PopAll32() {
      new Popad();
    }

    public void PushAll32() {
      new Pushad();
    }

    public void Return() {
      new Return();
    }

    public void Return(UInt16 aBytes) {
      new Return { DestinationValue = aBytes };
    }

    public void EnableInterrupts() {
      new Sti();
    }

    public void DisableInterrupts() {
      new ClrInterruptFlag();
    }

    public ElementReference AddressOf(string aDataName) {
      return ElementReference.New(aDataName);
    }
  }
}
