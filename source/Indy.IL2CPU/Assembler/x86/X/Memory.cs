using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86.X {
    public class Memory {
        public MemoryAction this[Address aAddress] {
            get {
                var xAddrDirect = aAddress as AddressDirect;
                if (xAddrDirect != null) {
                    if (xAddrDirect.Label != null) {
                        return new MemoryAction(new ElementReference(xAddrDirect.Label));
                    } else {
                        if (xAddrDirect.Register != Guid.Empty) {
                            return new MemoryAction(xAddrDirect.Register);
                        } else {
                            return new MemoryAction(xAddrDirect.Address);
                        }
                    }
                } else {
                    var xAddrIndirect = aAddress as AddressIndirect;
                    if (xAddrIndirect != null) {
                        if (xAddrIndirect.Reference != null) {
                            return new MemoryAction(xAddrIndirect.Reference, xAddrIndirect.Displacement);
                        } else {
                            if (xAddrDirect.Register != Guid.Empty) {
                                return new MemoryAction(xAddrDirect.Register, xAddrIndirect.Displacement);
                            } else {
                                return new MemoryAction(xAddrDirect.Address, xAddrIndirect.Displacement);
                            }
                        }
                    }
                    throw new Exception("Address type not supported!");
                }
            }
            set {
                if (value.Register!=Guid.Empty) {
                    var xAddrDirect = aAddress as AddressDirect;
                    if (xAddrDirect != null) {
                        if (xAddrDirect.Label != null) {
                            new X86.Move { DestinationRef = new ElementReference(xAddrDirect.Label), SourceValue = value.Value.GetValueOrDefault(), SourceRef = value.Reference, SourceReg = value.Register, SourceIsIndirect = value.IsIndirect };
                        } else {
                            new X86.Move { DestinationValue = xAddrDirect.Address, SourceValue = value.Value.GetValueOrDefault(), SourceRef = value.Reference, SourceReg = value.Register, SourceIsIndirect = value.IsIndirect };
                        }
                    } else {
                        var xAddrIndirect = aAddress as AddressIndirect;
                        if (xAddrIndirect != null) {
                                new X86.Move { DestinationRef = xAddrIndirect.Reference, DestinationDisplacement = xAddrIndirect.Displacement, DestinationValue = xAddrIndirect.Address, DestinationReg = xAddrIndirect.Register, DestinationIsIndirect = true, SourceValue = value.Value.GetValueOrDefault(), SourceRef = value.Reference, SourceReg = value.Register, SourceIsIndirect = value.IsIndirect };
                        } else {
                            throw new Exception("Invalid Address type!");
                        }
                    }
                } else {
                    var xAddrDirect = aAddress as AddressDirect;
                    if (xAddrDirect != null) {
                        if (xAddrDirect.Label != null) {
                            new X86.Move { DestinationRef = new ElementReference(xAddrDirect.Label), SourceValue = value.Value.GetValueOrDefault(), SourceRef = value.Reference, SourceReg = value.Register, SourceIsIndirect = value.IsIndirect };
                        } else {
                            new X86.Move { DestinationValue = xAddrDirect.Address, SourceValue = value.Value.GetValueOrDefault(), SourceRef = value.Reference, SourceReg = value.Register, SourceIsIndirect = value.IsIndirect };
                        }
                    } else {
                        var xAddrIndirect = aAddress as AddressIndirect;
                        if (xAddrIndirect != null) {
                            new X86.Move { DestinationRef = xAddrIndirect.Reference, DestinationDisplacement = xAddrIndirect.Displacement, DestinationValue = xAddrIndirect.Address, DestinationReg = xAddrIndirect.Register, DestinationIsIndirect = true, SourceValue = value.Value.GetValueOrDefault(), SourceRef = value.Reference, SourceReg = value.Register, SourceIsIndirect = value.IsIndirect };
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
                return xResult;
            }
            set {
                // ++ operators return ++
                // Maybe later change ++ etc to return actions?
                if (value != null) {
                    if (value.Register != Guid.Empty) {
                        var xAddrDirect = aAddress as AddressDirect;
                        if (xAddrDirect != null) {
                            if (xAddrDirect.Label != null) {
                                new X86.Move { DestinationRef = new ElementReference(xAddrDirect.Label), SourceValue = value.Value.GetValueOrDefault(), SourceRef = value.Reference, SourceReg = value.Register, SourceIsIndirect = value.IsIndirect};
                            } else {
                                new X86.Move { DestinationValue = xAddrDirect.Address, SourceValue = value.Value.GetValueOrDefault(), SourceRef = value.Reference, SourceReg = value.Register, SourceIsIndirect = value.IsIndirect };
                            }
                        } else {
                            var xAddrIndirect = aAddress as AddressIndirect;
                            if (xAddrIndirect != null) {
                                new X86.Move { DestinationRef = xAddrIndirect.Reference, DestinationDisplacement = xAddrIndirect.Displacement, DestinationValue = xAddrIndirect.Address, DestinationReg = xAddrIndirect.Register, DestinationIsIndirect = true, SourceValue = value.Value.GetValueOrDefault(), SourceRef = value.Reference, SourceReg = value.Register, SourceIsIndirect = value.IsIndirect };
                            } else {
                                throw new Exception("Invalid Address type!");
                            }
                        }
                    } else {
                        var xAddrDirect = aAddress as AddressDirect;
                        if (xAddrDirect != null) {
                            if (xAddrDirect.Label != null) {
                                new X86.Move { DestinationRef = new ElementReference(xAddrDirect.Label), SourceValue = value.Value.GetValueOrDefault(), SourceRef = value.Reference, SourceReg = value.Register, SourceIsIndirect = value.IsIndirect };
                            } else {
                                new X86.Move { DestinationValue = xAddrDirect.Address, SourceValue = value.Value.GetValueOrDefault(), SourceRef = value.Reference, SourceReg = value.Register, SourceIsIndirect = value.IsIndirect };
                            }
                        } else {
                            var xAddrIndirect = aAddress as AddressIndirect;
                            if (xAddrIndirect != null) {
                                new X86.Move { DestinationRef = xAddrIndirect.Reference, DestinationDisplacement = xAddrIndirect.Displacement, DestinationValue = xAddrIndirect.Address, DestinationReg = xAddrIndirect.Register, DestinationIsIndirect = true, SourceValue = value.Value.GetValueOrDefault(), SourceRef = value.Reference, SourceReg = value.Register, SourceIsIndirect = value.IsIndirect };
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
