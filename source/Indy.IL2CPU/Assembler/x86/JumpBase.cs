using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
	public abstract class JumpBase: InstructionWithDestination {
        public string DestinationLabel {
            get {
                if (DestinationRef != null) {
                    return DestinationRef.Name;
                }
                return String.Empty;
            }
            set {
                DestinationRef = new ElementReference(value);
            }
        }
        protected bool mNear = true;
        protected bool mCorrectAddress = true;
	    protected virtual bool IsRelativeJump {
	        get {
	            return true;
	        }
	    }
        public override byte[] GetData(Indy.IL2CPU.Assembler.Assembler aAssembler) {
            if (mCorrectAddress) {
                if(IsRelativeJump) {
                    if (DestinationValue.HasValue && !DestinationIsIndirect) {
                        var xCurAddress = ActualAddress;
                        var xOrigValue = DestinationValue.Value;
                        DestinationValue = (uint)(xOrigValue - xCurAddress.Value);
                        try {
                            return base.GetData(aAssembler);
                        } finally {
                            DestinationValue = xOrigValue;
                        }
                    }
                }
            }
            return base.GetData(aAssembler);
        }
        public override string ToString() {
            var xResult = base.ToString();
            if (mNear) {
                if (!xResult.StartsWith(Mnemonic + " near", StringComparison.InvariantCultureIgnoreCase)) {
                    if (xResult.StartsWith(Mnemonic)) {
                        return Mnemonic + " near " + xResult.Substring(Mnemonic.Length + 1);
                    }
                }
            }
            return xResult;
        }
	}
}