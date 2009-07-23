using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.IL2CPU {
  // Attribute
  // ILOpcode represents the opcode and provides all parsing and scanning support.
  // No assembly support is included except a refernece to ILOp
  public class ILOpCode {
    // private Init function called by descendant ctors
    //ILOp: AssemblerOp;
    //OpCode: OpCode - reference to instance from System.Emit.OpCodes

    //TODO: Change this to abstract
    public virtual void Scan(ILReader aReader, ILScanner aScanner) {
    }
  }
}
