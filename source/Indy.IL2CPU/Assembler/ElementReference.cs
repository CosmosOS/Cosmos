using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler {
    public class ElementReference {

        public ElementReference(string aName, int aOffset)
            : this(aName) {
            Offset = aOffset;
        }

        public ElementReference(string aName) {
            if (aName.StartsWith(".")) {
                Name = Label.LastFullLabel + aName;
            } else {
                Name = aName;
            }
        }

        public int Offset {
            get;
            set;
        }

        public string Name {
            get;
            set;
        }

        public override string ToString() {
            if (Offset != 0) {
                return Label.FilterStringForIncorrectChars(Name) + " + " + Offset;
            }else {
                return Label.FilterStringForIncorrectChars(Name);
            }
        }
    }
}
