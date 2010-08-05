using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.IL2CPU.X86.X {
    public class RegisterDX : Register16 {
        public static readonly RegisterDX Instance = new RegisterDX();

        public static implicit operator RegisterDX(UInt16 aValue) {
            Instance.Move(aValue);
            return Instance;
        }
    }
}
