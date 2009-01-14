using System;
using System.IO;
using System.Linq;

namespace Indy.IL2CPU.Assembler {
	public abstract class Instruction: BaseAssemblerElement {
		protected string mMnemonic;

		public string Mnemonic {
			get { return mMnemonic; }
		}

		public override string ToString() {
			return Mnemonic;
		}

		public Instruction():this(true) {
		}

		public Instruction(bool aAddToAssembler) {
			var xAttribs = GetType().GetCustomAttributes(typeof (OpCodeAttribute), false);
			if (xAttribs != null && xAttribs.Length > 0) {
				var xAttrib = (OpCodeAttribute)xAttribs[0];
				mMnemonic = xAttrib.Mnemonic;
			}
			if(aAddToAssembler) {
				Assembler.CurrentInstance.Peek().Add(this);
			}
		}

        public override ulong? ActualAddress {
            get { 
                // TODO: for now, we dont have any data alignment
                return StartAddress;
            }
        }

        public override void UpdateAddress(Assembler aAssembler, ref ulong aAddress) {
            base.UpdateAddress(aAssembler, ref aAddress);
        }

        public override bool IsComplete(Assembler aAssembler) {
            throw new NotImplementedException("Method not implemented for instruction " + this.GetType().FullName.Substring(typeof(Instruction).Namespace.Length + 1));
        }

        public override byte[] GetData(Assembler aAssembler) {
            throw new NotImplementedException("Method not implemented for instruction " + this.GetType().FullName.Substring(typeof(Instruction).Namespace.Length + 1));
        }
    }
}