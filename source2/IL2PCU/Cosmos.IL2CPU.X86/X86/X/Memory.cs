using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.IL2CPU.X86.X {
    public class Memory {
        public MemoryAction this[Address aAddress] {
            get {
                var xAddrDirect = aAddress as AddressDirect;
                if (xAddrDirect != null) {
                    if (xAddrDirect.Label != null) {
                        return new MemoryAction(ElementReference.New(xAddrDirect.Label));
                    } else {
                        if (xAddrDirect.Register != null) {
                            return new MemoryAction(xAddrDirect.Register.Value);
                        } else {
                            return new MemoryAction(xAddrDirect.Address);
                        }
                    }
                } else {
                    var xAddrIndirect = aAddress as AddressIndirect;
                    if (xAddrIndirect != null) {
                        if (xAddrIndirect.Reference != null) {
                            return new MemoryAction(xAddrIndirect.Reference, xAddrIndirect.Displacement) { IsIndirect = true };
                        } else {
                            if (xAddrIndirect.Register != null) {
                                return new MemoryAction(xAddrIndirect.Register.Value, xAddrIndirect.Displacement) { IsIndirect = true };
                            } else {
                                return new MemoryAction(xAddrIndirect.Address, xAddrIndirect.Displacement) { IsIndirect = true };
                            }
                        }
                    }
                    throw new Exception("Address type not supported!");
                }
            }
            set {
                if (value.Register!=null) {
                    var xAddrDirect = aAddress as AddressDirect;

                    if (xAddrDirect != null) {
                        if (xAddrDirect.Label != null) {
                            new Move { DestinationRef = ElementReference.New(xAddrDirect.Label), DestinationIsIndirect=true, SourceRef = value.Reference, SourceReg = value.Register, SourceIsIndirect = value.IsIndirect };
                        } else {
                            new Move { DestinationValue = xAddrDirect.Address, SourceValue = value.Value.GetValueOrDefault(), SourceRef = value.Reference, SourceReg = value.Register, SourceIsIndirect = value.IsIndirect };
                        }
                    } else {
                        var xAddrIndirect = aAddress as AddressIndirect;
                        if (xAddrIndirect != null) {
                                new Move { DestinationRef = xAddrIndirect.Reference, DestinationDisplacement = xAddrIndirect.Displacement, DestinationValue = xAddrIndirect.Address, DestinationReg = xAddrIndirect.Register, DestinationIsIndirect = true, SourceValue = value.Value.GetValueOrDefault(), SourceRef = value.Reference, SourceReg = value.Register, SourceIsIndirect = value.IsIndirect };
                        } else {
                            throw new Exception("Invalid Address type!");
                        }
                    }
                } else {
                    var xAddrDirect = aAddress as AddressDirect;
                    if (xAddrDirect != null) {
                        if (xAddrDirect.Label != null) {
                            new Move { DestinationRef = ElementReference.New(xAddrDirect.Label), SourceValue = value.Value.GetValueOrDefault(), SourceRef = value.Reference, SourceReg = value.Register, SourceIsIndirect = value.IsIndirect };
                        } else {
                            new Move { DestinationValue = xAddrDirect.Address, SourceValue = value.Value.GetValueOrDefault(), SourceRef = value.Reference, SourceReg = value.Register, SourceIsIndirect = value.IsIndirect };
                        }
                    } else {
                        var xAddrIndirect = aAddress as AddressIndirect;
                        if (xAddrIndirect != null) {
                            new Move { DestinationRef = xAddrIndirect.Reference, DestinationDisplacement = xAddrIndirect.Displacement, DestinationValue = xAddrIndirect.Address, DestinationReg = xAddrIndirect.Register, DestinationIsIndirect = true, SourceValue = value.Value.GetValueOrDefault(), SourceRef = value.Reference, SourceReg = value.Register, SourceIsIndirect = value.IsIndirect };
                        } else {
                            throw new Exception("Invalid Address type!");
                        }
                    }
                }
            }
        }

        public MemoryAction this[Address aAddress, byte aSize] {
            get {
                var xResult = this[aAddress];
                xResult.Size = aSize;
                if (xResult.Reference != null) {
                    xResult.IsIndirect = true;
                }
                return xResult;
            }
            set {
                // ++ operators return ++
                // Maybe later change ++ etc to return actions?
                if (value != null) {
                    if (value.Register != null) {
                        var xAddrDirect = aAddress as AddressDirect;
                        if (xAddrDirect != null) {
                            if (xAddrDirect.Label != null) {
                                new Move {
                                    DestinationRef = ElementReference.New(xAddrDirect.Label),
                                    DestinationIsIndirect = true,
                                    SourceValue = value.Value.GetValueOrDefault(),
                                    SourceRef = value.Reference,
                                    SourceReg = value.Register,
                                    Size = aSize,
                                    SourceIsIndirect = value.IsIndirect
                                };
                            } else {
                                new Move {
                                    DestinationValue = xAddrDirect.Address,
                                    SourceValue = value.Value.GetValueOrDefault(),
                                    SourceRef = value.Reference,
                                    Size = aSize,
                                    SourceReg = value.Register,
                                    SourceIsIndirect = value.IsIndirect
                                };
                            }
                        } else {
                            var xAddrIndirect = aAddress as AddressIndirect;
                            if (xAddrIndirect != null) {
                                new Move {
                                    DestinationRef = xAddrIndirect.Reference,
                                    DestinationDisplacement = xAddrIndirect.Displacement,
                                    DestinationValue = (xAddrIndirect.Address != 0 ? (uint?)xAddrIndirect.Address : (uint?)null),
                                    DestinationReg = xAddrIndirect.Register,
                                    DestinationIsIndirect = true,
                                    SourceValue = value.Value.GetValueOrDefault(),
                                    Size = aSize,
                                    SourceRef = value.Reference,
                                    SourceReg = value.Register,
                                    SourceIsIndirect = value.IsIndirect
                                };
                            } else {
                                throw new Exception("Invalid Address type!");
                            }
                        }
                    } else {
                        var xAddrDirect = aAddress as AddressDirect;
                        if (xAddrDirect != null) {
                            if (xAddrDirect.Label != null) {
                                new Move {
                                    DestinationRef = ElementReference.New(xAddrDirect.Label),
                                    DestinationIsIndirect=true,
                                    SourceValue = value.Value.GetValueOrDefault(),
                                    SourceRef = value.Reference,
                                    SourceReg = value.Register,
                                    Size = aSize,
                                    SourceIsIndirect = value.IsIndirect
                                };
                            } else {
                                new Move {
                                    DestinationValue = xAddrDirect.Address,
                                    SourceValue = value.Value.GetValueOrDefault(),
                                    SourceRef = value.Reference,
                                    SourceReg = value.Register,
                                    Size = aSize,
                                    SourceIsIndirect = value.IsIndirect
                                };
                            }
                        } else {
                            var xAddrIndirect = aAddress as AddressIndirect;
                            if (xAddrIndirect != null) {
                                new Move {
                                    DestinationRef = xAddrIndirect.Reference,
                                    DestinationDisplacement = xAddrIndirect.Displacement,
                                    DestinationValue = xAddrIndirect.Address,
                                    DestinationReg = xAddrIndirect.Register,
                                    DestinationIsIndirect = true,
                                    Size = aSize,
                                    SourceValue = value.Value.GetValueOrDefault(),
                                    SourceRef = value.Reference,
                                    SourceReg = value.Register,
                                    SourceIsIndirect = value.IsIndirect
                                };
                            } else {
                                throw new Exception("Invalid Address type!");
                            }
                        }
                    }
                }
            }
        }
    }
}
