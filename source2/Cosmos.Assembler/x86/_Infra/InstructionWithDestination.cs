using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Assembler.x86 {
    public abstract class InstructionWithDestination : Instruction, IInstructionWithDestination{
        public InstructionWithDestination()
        {
            
        }

        public InstructionWithDestination(string mnemonic):base(mnemonic)
        {

        }

        public Cosmos.Assembler.ElementReference DestinationRef {
            get;
            set;
        }

        public RegistersEnum? DestinationReg
        {
            get;
            set;
        }

        public uint? DestinationValue
        {
            get;
            set;
        }

        public bool DestinationIsIndirect {
            get;
            set;
        }

        public int DestinationDisplacement {
            get;
            set;
        }

        public bool DestinationEmpty
        {
            get;
            set;
        }

        public override bool IsComplete( Cosmos.Assembler.Assembler aAssembler )
        {
            if (DestinationRef != null) {
                ulong xAddress;
                return base.IsComplete(aAssembler) && DestinationRef.Resolve(aAssembler, out xAddress);
            }
            return base.IsComplete(aAssembler);
        }

        public override void UpdateAddress( Cosmos.Assembler.Assembler aAssembler, ref ulong aAddresss )
        {
            if (DestinationRef != null) {
                DestinationValue = 0xFFFFFFFF;
            }
            base.UpdateAddress(aAssembler, ref aAddresss);
        }


        public override byte[] GetData( Cosmos.Assembler.Assembler aAssembler )
        {
            if (DestinationRef != null) {
                ulong xAddress = 0;
                if (!DestinationRef.Resolve(aAssembler, out xAddress)) {
                    throw new Exception("Cannot resolve DestinationRef!");
                }
                DestinationValue = (uint)xAddress;
            }
            return base.GetData(aAssembler);
        }

        public override void WriteText( Cosmos.Assembler.Assembler aAssembler, System.IO.TextWriter aOutput )
        {
            aOutput.Write(mMnemonic);
            String destination = this.GetDestinationAsString();
            if (!(DestinationEmpty && destination.Equals("")))
            {
                aOutput.Write(" ");
                aOutput.Write(destination);
            }
        }
    }
}