using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Assembler;
using Cosmos.Assembler.x86;

namespace Cosmos.Assembler.XSharp {
    public class RegisterAX : Register16 {
        public static readonly RegisterAX Instance = new RegisterAX();

        public static implicit operator RegisterAX(Cosmos.Assembler.ElementReference aReference) {
            Instance.Move(aReference);
            return Instance;
        }

        public static implicit operator RegisterAX(MemoryAction aAction) {
            Instance.Move(aAction);
            return Instance;
        }

        public static implicit operator RegisterAX(UInt16 aValue) {
            Instance.Move(aValue);
            return Instance;
        }

        public static implicit operator PortNumber(RegisterAX aAX)
        {
            return new PortNumber(aAX.GetId());
        }

    }
}
