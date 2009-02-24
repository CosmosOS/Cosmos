using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
    public static class InfraExtensions {
        public static string GetDestinationAsString(this IInstructionWithDestination aThis) {
            string xDest = "";
            if((aThis.DestinationValue.HasValue ||aThis.DestinationRef != null) && 
                aThis.DestinationIsIndirect && 
                aThis.DestinationReg!=Guid.Empty) {
                throw new Exception("[Scale*index+base] style addressing not supported at the moment");
            }
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

        public static string GetMnemonic(this ConditionalTestEnum aThis) {
            switch (aThis) {
                case ConditionalTestEnum.Overflow:
                    return "o";
                case ConditionalTestEnum.NoOverflow:
                    return "no";
                case ConditionalTestEnum.Below:
                    return "b";
                case ConditionalTestEnum.NotBelow:
                    return "nb";
                case ConditionalTestEnum.Equal:
                    return "e";
                case ConditionalTestEnum.NotEqual:
                    return "ne";
                case ConditionalTestEnum.BelowOrEqual:
                    return "be";
                case ConditionalTestEnum.NotBelowOrEqual:
                    return "nbe";
                case ConditionalTestEnum.Sign:
                    return "s";
                case ConditionalTestEnum.NotSign:
                    return "ns";
                case ConditionalTestEnum.Parity:
                    return "p";
                case ConditionalTestEnum.NotParity:
                    return "np";
                case ConditionalTestEnum.LessThan:
                    return "l";
                case ConditionalTestEnum.NotLessThan:
                    return "nl";
                case ConditionalTestEnum.LessThanOrEqualTo:
                    return "le";
                case ConditionalTestEnum.NotLessThanOrEqualTo:
                    return "nle";
                default: throw new NotImplementedException();
            }
        }
    }
}