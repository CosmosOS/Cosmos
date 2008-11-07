using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86.X {
    public class RegisterDX : Register16 {
        public const string Name = "DX";
        public static readonly RegisterDX Instance = new RegisterDX();

        public override string ToString() {
            return Name;
        }

        public static implicit operator RegisterDX(UInt16 aValue) {
            Instance.Move(aValue);
            return Instance;
        }
    }
}
