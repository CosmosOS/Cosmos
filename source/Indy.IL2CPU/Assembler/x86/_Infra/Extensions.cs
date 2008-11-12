using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
    public static class InfraExtensions {
        public static string GetDestinationAsString(this IInstructionWithDestination aThis) {
            string xDest = "";
            if (aThis.DestinationRef != null) {
                xDest = aThis.DestinationRef.ToString();
            } else {
                if (aThis.DestinationReg != Guid.Empty) {
                    xDest = Registers.GetRegisterName(aThis.DestinationReg);
                } else {
                    xDest = "0x" + aThis.DestinationValue.GetValueOrDefault().ToString("X").ToUpperInvariant();
                }
            }
            if (aThis.DestinationDisplacement != 0) {
                if (aThis.DestinationDisplacement > 255) {
                    xDest += " + 0x" + aThis.DestinationDisplacement.ToString("X");
                }else {
                    xDest += " + " + aThis.DestinationDisplacement;
                }
            }
            if (aThis.DestinationIsIndirect) {
                return "[" + xDest + "]";
            } else {
                return xDest;
            }
        }

        public static void DetermineSize(this IInstructionWithDestination aThis, IInstructionWithSize aThis2, byte aSize) {
            if (aSize == 0) {
                if (aThis.DestinationReg != Guid.Empty && !aThis.DestinationIsIndirect) {
                    if (Registers.Is16Bit(aThis.DestinationReg)) {
                        aThis2.Size = 16;
                    } else {
                        if (Registers.Is32Bit(aThis.DestinationReg)) {
                            aThis2.Size = 32;
                        } else {
                            aThis2.Size = 8;
                        }
                    }
                    return;
                }
                if (aThis.DestinationRef != null && !aThis.DestinationIsIndirect) {
                    aThis2.Size = 32;
                    return;
                }
            }
        }
    }
}