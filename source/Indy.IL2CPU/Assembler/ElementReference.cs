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

        private ulong? mActualAddress;

        public bool Resolve(Assembler aAssembler, out ulong aAddress) {
            if (mActualAddress != null) {
                aAddress = mActualAddress.Value;
                return true;
            }
            BaseAssemblerElement xElement = (from item in aAssembler.Instructions
                                             let xLabel = item as Label
                                             where xLabel != null && xLabel.QualifiedName.Equals(Name, StringComparison.InvariantCultureIgnoreCase)
                                             select item).SingleOrDefault();
            if (xElement == null) {
                xElement = (from item in aAssembler.DataMembers
                            where item.Name.Equals(Name, StringComparison.InvariantCultureIgnoreCase)
                            select item).SingleOrDefault();
            }

            if (xElement != null) {
                if (xElement.ActualAddress.HasValue) {
                    mActualAddress = xElement.ActualAddress.Value + (uint)Offset;
                    aAddress = mActualAddress.Value;
                    return true;
                }
            }

            aAddress = 0;
            return false;
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
