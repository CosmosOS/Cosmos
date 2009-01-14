using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler {
    public abstract class BaseAssemblerElement {
        /// <summary>
        /// Gets/sets the address at which the element could start emitting data. Note that if
        /// the element needs any alignment, start address is unaligned, and the element should 
        /// do the alignment itself. ActualAddress is used for referencing the actual address.
        /// </summary>
        public virtual ulong? StartAddress {
            get;
            set;
        }

        public abstract ulong? ActualAddress {
            get;
        }

        public virtual void UpdateAddress(Assembler aAssembler, ref ulong aAddress) {
            StartAddress = aAddress;
        }

        public abstract bool IsComplete(Assembler aAssembler);
        public abstract byte[] GetData(Assembler aAssembler);
    }
}