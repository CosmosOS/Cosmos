using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86.X {
    public class RegisterDX : Register16 {
        public static readonly RegisterDX Instance = new RegisterDX();

        protected override string GetName() {
            return "DX";
        }

        public static implicit operator RegisterDX(UInt16 aValue) {
            Instance.Move(aValue.ToString());
            return Instance;
        }
    }
}
