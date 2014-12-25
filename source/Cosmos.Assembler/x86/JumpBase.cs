using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Cosmos.Assembler.x86 {
  public abstract class JumpBase : InstructionWithDestination
  {
      public JumpBase(string mnemonic =null) : base(mnemonic)
      {
      }

      public string DestinationLabel {
      get {
        if (DestinationRef != null) {
          return DestinationRef.Name;
        }
        return String.Empty;
      }
      set {
        DestinationRef = Cosmos.Assembler.ElementReference.New(value);
      }
    }
    protected bool mNear = true;
    protected bool mCorrectAddress = true;
    protected virtual bool IsRelativeJump {
      get {
        return true;
      }
    }

    public override void WriteData(Cosmos.Assembler.Assembler aAssembler, Stream aOutput) {
      if (mCorrectAddress) {
        if (IsRelativeJump) {
          if (DestinationValue.HasValue && !DestinationIsIndirect) {
            var xCurAddress = ActualAddress;
            var xOrigValue = DestinationValue.Value;
            DestinationValue = (uint)(xOrigValue - xCurAddress.Value);
            try {
              base.WriteData(aAssembler, aOutput);
              return;
            } finally {
              DestinationValue = xOrigValue;
            }
          }
        }
      }
      base.WriteData(aAssembler, aOutput);
    }
    //public override string ToString() {
    //    var xResult = base.ToString();
    //    if (mNear) {
    //        if (!xResult.StartsWith(Mnemonic + " near", StringComparison.InvariantCultureIgnoreCase)) {
    //            if (xResult.StartsWith(Mnemonic)) {
    //                return Mnemonic + " near " + xResult.Substring(Mnemonic.Length + 1);
    //            }
    //        }
    //    }
    //    return xResult;
    //}
  }
}