using System;
using System.Linq;

namespace Indy.IL2CPU.Assembler {
	public class Comment: Instruction {
		public readonly string Text;

		public Comment(string aText) {
			if(aText.StartsWith(";")) {
				aText = aText.TrimStart(';').TrimStart();
			}
			Text = aText;
		}

		public override string ToString() {
			return "; " + Text;
		}

        public override void UpdateAddress(Assembler aAssembler, ref ulong aAddress) {
            base.UpdateAddress(aAssembler, ref aAddress);
        }

        public override bool IsComplete(Assembler aAssembler) {
            return true;
        }

        public override byte[] GetData(Assembler aAssembler) {
            return new byte[0];
        }
	}
}